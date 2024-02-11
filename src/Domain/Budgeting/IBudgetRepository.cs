namespace Beseler.Domain.Budgeting;

public interface IBudgetRepository
{
    Task<Budget?> GetAsync(int accountId, int year, int month, CancellationToken stoppingToken = default);
    Task<Budget?> GetLineByIdAsync(Guid id, CancellationToken stoppingToken = default);
    Task<Result<Budget, Error>> SaveChangesAsync(Budget account, CancellationToken stoppingToken = default);
}
