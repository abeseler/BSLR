namespace Beseler.Domain.Budgeting;

public sealed class Budget : Aggregate
{
    private readonly List<BudgetLine> _lines = [];
    public int AccountId { get; private set; }
    public DateOnly Start { get; private set; }
    public decimal StartingBalance { get; private set; }
    public decimal EndingBalance { get; private set; }
    public DateTime CreatedOn { get; private set; }
    public DateTime? LastModifiedOn { get; private set; }

    public string Title => $"{Start:MMMM} Budget {Start.Year}";

    public IReadOnlyCollection<BudgetLine> Lines => _lines;
    public IEnumerable<BudgetLine> Income => _lines.Where(l => l.LineType is BudgetLineType.Income);
    public IEnumerable<BudgetLine> Expenses => _lines.Where(l => l.LineType is BudgetLineType.Expense);
    public IEnumerable<BudgetLine> Savings => _lines.Where(l => l.LineType is BudgetLineType.Savings);

    public static Budget Create(int accountId, int year, int month)
    {
        return new()
        {
            AccountId = accountId,
            Start = new(year, month, 1),
            CreatedOn = DateTime.UtcNow
        };
    }

    public Result<Success, Error> AddLine(Guid id, BudgetLineType type, string description, DateOnly date, decimal amount)
    {
        if (_lines.Any(l => l.BudgetLineId == id))
            return new Error("Line already exists.");

        if (Start.Year != date.Year || Start.Month != date.Month)
            return new Error("Line date does not match budget month.");

        if (IsInvalidAmount(type, amount, out var amountError))
            return amountError!;

        EndingBalance += amount;
        _lines.Add(new()
        {
            AccountId = AccountId,
            BudgetLineId = id,
            LineType = type,
            TransactionDate = date,
            Description = description,
            Amount = amount,
            CreatedOn = DateTime.UtcNow
        });

        HasUnsavedChange();
        return Success.Default;
    }

    public Result<Success, Error> UpdateLine(Guid id, DateOnly date, string description, decimal amount)
    {
        var line = _lines.FirstOrDefault(l => l.BudgetLineId == id);
        if (line is null)
            return new Error("Line does not exist.");

        if (Start.Year != date.Year || Start.Month != date.Month)
            return new Error("Line date does not match budget month.");

        if (IsInvalidAmount(line.LineType, amount, out var amountError))
            return amountError!;

        EndingBalance += amount - line.Amount;
        var newLine = line with
        {
            TransactionDate = date,
            Description = description,
            Amount = amount,
            LastModifiedOn = DateTime.UtcNow
        };

        _lines.Remove(line);
        _lines.Add(newLine);

        HasUnsavedChange();
        return Success.Default;
    }

    public Result<Success, Error> RemoveLine(Guid id)
    {
        var line = _lines.FirstOrDefault(l => l.BudgetLineId == id);
        if (line is null)
            return new Error("Line does not exist.");

        EndingBalance -= line.Amount;
        _lines.Remove(line);

        HasUnsavedChange();
        return Success.Default;
    }

    private static bool IsInvalidAmount(BudgetLineType type, decimal amount, out Error? error)
    {
        error = (type, amount) switch
        {
            (_, 0) => new Error("Amount cannot be zero."),
            (BudgetLineType.Income, < 0) => new Error("Income amount must be greater than zero."),
            (BudgetLineType.Expense, > 0) => new Error("Expense amount must be less than zero."),
            _ => null
        };

        return error != null;
    }
}
