using Beseler.Domain.Common;

namespace Beseler.Domain.Accounts;

public interface IAccountRepository
{
    Task<Account?> GetByEmailAsync(string email, CancellationToken stoppingToken = default);
    Task<Result<Account, Error>> SaveAsync(Account account, CancellationToken stoppingToken = default);
}
