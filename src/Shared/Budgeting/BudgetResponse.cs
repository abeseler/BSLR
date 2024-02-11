namespace Beseler.Shared.Budgeting;

public sealed record BudgetResponse(int Year, int Month, decimal StartingBalance, Dictionary<string, IEnumerable<BudgetLineDto>> Transactions);
public sealed record BudgetLineDto(Guid Id, string Type, string Description, DateOnly Date, decimal Amount);
