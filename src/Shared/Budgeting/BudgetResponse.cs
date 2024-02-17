namespace Beseler.Shared.Budgeting;

public sealed record BudgetResponse(string Title, int Year, int Month, decimal StartingBalance, Dictionary<string, BudgetLineDto[]> Transactions);
public sealed record BudgetLineDto(Guid Id, string LineType, string Description, DateOnly TransactionDate, decimal Amount);
