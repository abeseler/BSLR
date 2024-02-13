using Beseler.Domain.Budgeting;

namespace Beseler.API.Budgeting.Mappers;

internal static class BudgetLineMapper
{
    public static BudgetLineDto ToDto(this BudgetLine model)
    {
        return new(model.BudgetLineId, model.LineType.ToString(), model.Description, model.TransactionDate, model.Amount);
    }

    public static BudgetLineDto[] ToDtos(this IEnumerable<BudgetLine> models)
    {
        return models.Select(model => model.ToDto()).ToArray();
    }
}
