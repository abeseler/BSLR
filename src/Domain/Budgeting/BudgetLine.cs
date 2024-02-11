namespace Beseler.Domain.Budgeting;

public sealed record BudgetLine
{
    public Guid BudgetLineId { get; init; }
    public BudgetLineType LineType { get; init; }
    public required string Description { get; init; }
    public DateOnly TransactionDate { get; init; }
    public decimal Amount { get; init; }
    public int AccountId { get; init; }
    public DateTime CreatedOn { get; init; }
    public DateTime? LastModifiedOn { get; init; }
}
