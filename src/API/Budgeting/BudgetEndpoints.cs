using Asp.Versioning.Builder;
using Beseler.API.Budgeting.Handlers;

namespace Beseler.API.Budgeting;

internal static class BudgetEndpoints
{
    public static WebApplication MapBudgetingEndpoints(this WebApplication app, ApiVersionSet versions)
    {
        var group = app.MapGroup("")
            .WithTags("Budgeting")
            .WithApiVersionSet(versions)
            .MapToApiVersion(1);

        group.MapGet(Endpoints.Budgeting.Budget, GetBudgetHandler.HandleAsync)
            .Produces<BudgetResponse>(200);

        group.MapPut($"{Endpoints.Budgeting.Transactions}/{{id:Guid}}", UpsertBudgetTransactionHandler.HandleAsync)
            .Produces(204)
            .Produces(400);

        group.MapDelete($"{Endpoints.Budgeting.Transactions}/{{id:Guid}}", DeleteBudgetTransactionHandler.HandleAsync)
            .Produces(204)
            .Produces(400);

        return app;
    }
}
