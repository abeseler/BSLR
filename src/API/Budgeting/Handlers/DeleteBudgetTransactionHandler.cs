using System.Security.Claims;

namespace Beseler.API.Budgeting.Handlers;

internal static class DeleteBudgetTransactionHandler
{
    public static async Task<IResult> HandleAsync(ClaimsPrincipal principal, Guid id, CancellationToken stoppingToken)
    {
        await Task.CompletedTask;
        return TypedResults.Problem("Not implemented.");
    }
}
