using Beseler.Domain.Common;
using Beseler.Infrastructure.Data.Mappers;
using Dapper;
using System.Data;

namespace Beseler.Infrastructure.Data.Repositories;

internal sealed class BudgetRepository(IDatabaseConnector connector, OutboxRepository outboxRepository) : AggregateRepository(outboxRepository), IBudgetRepository
{
    private const string SaveChangesProcedure = "dbo.BudgetSaveChanges";

    public async Task<Budget?> GetAsync(int accountId, int year, int month, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("AccountId", accountId);
        parameters.Add("Year", year);
        parameters.Add("Month", month);

        using var connection = await connector.ConnectAsync(stoppingToken);
        var results = await connection.QueryMultipleAsync("""
            SELECT *
            FROM Budget
            WHERE AccountId = @AccountId
                AND YEAR(Start) = @Year
                AND MONTH(Start) = @Month;

            SELECT *
            FROM BudgetLine
            WHERE AccountId = @AccountId
                AND YEAR(TransactionDate) = @Year
                AND MONTH(TransactionDate) = @Month;
            """, parameters);

        var budget = await results.ReadSingleOrDefaultAsync<Budget>();
        budget?.SetLines(await results.ReadAsync<BudgetLine>());

        return budget;
    }

    public async Task<BudgetLine?> GetLineByIdAsync(Guid id, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        using var connection = await connector.ConnectAsync(stoppingToken);
        return connection.QuerySingleOrDefault<BudgetLine>("""
            SELECT *
            FROM BudgetLine
            WHERE BudgetLineId = @Id;
            """, parameters);
    }

    public async Task<Result<Budget, Exception>> SaveChangesAsync(Budget model, CancellationToken stoppingToken = default)
    {
        var dt = new DataTable();
        dt.Columns.Add("BudgetLineId");
        dt.Columns.Add("LineType");
        dt.Columns.Add("Description");
        dt.Columns.Add("TransactionDate");
        dt.Columns.Add("Amount");
        dt.Columns.Add("AccountId");
        dt.Columns.Add("CreatedOn");
        dt.Columns.Add("LastModifiedOn");

        foreach (var line in model.Lines)
        {
            dt.Rows.Add(line.BudgetLineId, line.LineType, line.Description, line.TransactionDate, line.Amount, line.AccountId, line.CreatedOn, line.LastModifiedOn);
        }

        var parameters = new DynamicParameters();
        parameters.Add("AccountId", model.AccountId);
        parameters.Add("Start", model.Start.ToDateTime(TimeOnly.MinValue));
        parameters.Add("StartingBalance", model.StartingBalance);
        parameters.Add("EndingBalance", model.EndingBalance);
        parameters.Add("CreatedOn", model.CreatedOn);
        parameters.Add("LastModifiedOn", model.LastModifiedOn);
        parameters.Add("Lines", dt.AsTableValuedParameter("dbo.TVP_BudgetLine"));

        using var connection = await connector.ConnectAsync(stoppingToken);
        var result = await connection.QuerySingleAsync<int>(SaveChangesProcedure, parameters, commandType: CommandType.StoredProcedure);

        if (result <= 0)
            return new Exception("Budget saving failed.");

        await base.SaveChangesAsync(model, stoppingToken);
        return model;
    }
}
