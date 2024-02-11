using Beseler.Domain.Budgeting;

namespace Beseler.API.Budgeting.Mappers;

internal static class BudgetLineMapper
{
    public static BudgetLineDto ToDto(this BudgetLine model)
    {
        return new(model.BudgetLineId, model.Type.ToString(), model.Description, model.Date, model.Amount);
    }

    public static IEnumerable<BudgetLineDto> ToDtos(this IEnumerable<BudgetLine> models)
    {
        return models.Select(model => model.ToDto());
    }
}
