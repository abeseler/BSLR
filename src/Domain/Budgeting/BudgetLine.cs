namespace Beseler.Domain.Budgeting;

public sealed record BudgetLine
{
    public Guid BudgetLineId { get; init; }
    public BudgetLineType Type { get; init; }
    public required string Description { get; init; }
    public DateOnly Date { get; init; }
    public decimal Amount { get; init; }
    public int AccountId { get; init; }
    public DateTime CreatedOn { get; init; }
    public DateTime? LastModifiedOn { get; init; }
}
