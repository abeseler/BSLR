namespace Beseler.Domain.Accounts;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int idl, CancellationToken stoppingToken = default);
    Task<Account?> GetByEmailAsync(string email, CancellationToken stoppingToken = default);
    Task<Result<Account, Exception>> SaveChangesAsync(Account account, CancellationToken stoppingToken = default);
}
