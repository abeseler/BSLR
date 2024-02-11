using Beseler.Domain.Common;
using Dapper;

namespace Beseler.Infrastructure.Data.Repositories;

internal sealed class BudgetRepository(IDatabaseConnector connector) : IBudgetRepository
{
    public async Task<Budget?> GetAsync(int accountId, int year, int month, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("AccountId", accountId);
        parameters.Add("Year", year);
        parameters.Add("Month", month);

        using var connection = await connector.ConnectAsync(stoppingToken);
        return null;
    }

    public async Task<Budget?> GetLineByIdAsync(Guid id, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        using var connection = await connector.ConnectAsync(stoppingToken);
        return null;
    }

    public async Task<Result<Budget, Error>> SaveChangesAsync(Budget account, CancellationToken stoppingToken = default)
    {
        using var connection = await connector.ConnectAsync(stoppingToken);
        return new Error("Budget saving is not implemented.");
    }
}
