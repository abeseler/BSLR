using Beseler.Domain.Accounts;
using Beseler.Domain.Common;
using Beseler.Infrastructure.Data.Mappers;
using Dapper;

namespace Beseler.Infrastructure.Data.Repositories;

internal sealed class AccountRepository(IDatabaseConnector connector, OutboxRepository outboxRepository) : AggregateRepository(outboxRepository), IAccountRepository
{
    public async Task<Account?> GetByIdAsync(int id, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("id", id);

        using var connection = await connector.ConnectAsync(stoppingToken);
        return await connection.QuerySingleOrDefaultAsync<Account>(
            """
            SELECT *
            FROM Account
            WHERE AccountId = @id;
            """, parameters);
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("email", email);

        using var connection = await connector.ConnectAsync(stoppingToken);
        return await connection.QuerySingleOrDefaultAsync<Account>(
            """
            SELECT *
            FROM Account
            WHERE Email = @email;
            """, parameters);
    }

    public async Task<Result<Account, Error>> SaveChangesAsync(Account account, CancellationToken stoppingToken = default)
    {
        var result = account switch
        {
            { AccountId: 0 } => await InsertAsync(account, stoppingToken),
            { HasUnsavedChanges: true } => await UpdateAsync(account, stoppingToken),
            _ => account
        };

        if (result.IsSuccess)
            await base.SaveChangesAsync(account, stoppingToken);

        return result;
    }

    private async Task<Result<Account, Error>> InsertAsync(Account account, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters(account);

        using var connection = await connector.ConnectAsync(stoppingToken);
        var accountId = await connection.QuerySingleAsync<int>(
            """
            INSERT INTO Account (
                Email,
                GivenName,
                FamilyName,
                SecretHash,
                CreatedOn)
            VALUES (
                @Email,
                @GivenName,
                @FamilyName,
                @SecretHash,
                @CreatedOn);

            SELECT SCOPE_IDENTITY();
            """, parameters);

        account.SetIdentity(accountId);

        return account;
    }

    private async Task<Result<Account, Error>> UpdateAsync(Account account, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters(account);

        using var connection = await connector.ConnectAsync(stoppingToken);
        var result = await connection.ExecuteAsync(
            """
            UPDATE Account
            SET
                Email = @Email,
                GivenName = @GivenName,
                FamilyName = @FamilyName,
                SecretHash = @SecretHash,
                IsLocked = @IsLocked,
                IsVerified = @IsVerified,
                LastLoginOn = @LastLoginOn,
                FailedLoginAttempts = @FailedLoginAttempts
            WHERE AccountId = @AccountId;
            """, parameters);

        return result == 1 ? account : new Error("Failed to update account.");
    }
}
