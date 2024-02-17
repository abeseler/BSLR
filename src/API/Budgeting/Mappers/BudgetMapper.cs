using Beseler.Domain.Budgeting;

namespace Beseler.API.Budgeting.Mappers;

internal static class BudgetMapper
{
    public static BudgetResponse ToResponse(this Budget model)
    {
        return new(
            model.Title,
            model.Start.Year,
            model.Start.Month,
            model.StartingBalance,
            new()
            {
                { "Income", model.Income.ToDtos() },
                { "Expenses", model.Expenses.ToDtos() },
                { "Savings", model.Savings.ToDtos() }
            });
    }
}
