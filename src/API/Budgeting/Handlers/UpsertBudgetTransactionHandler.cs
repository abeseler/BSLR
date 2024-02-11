using Beseler.Domain.Budgeting;
using System.Security.Claims;

namespace Beseler.API.Budgeting.Handlers;

internal static class UpsertBudgetTransactionHandler
{
    public static async Task<IResult> HandleAsync(ClaimsPrincipal principal, Guid id, BudgetLineDto request, IBudgetRepository budgetRepository, CancellationToken stoppingToken)
    {
        if (int.TryParse(principal.Identity?.Name, out var accountId) is false)
            return TypedResults.Unauthorized();

        var budget = await budgetRepository.GetAsync(accountId, request.Date.Year, request.Date.Month, stoppingToken)
            ?? Budget.Create(accountId, request.Date.Year, request.Date.Month);

        var line = await budgetRepository.GetLineByIdAsync(id, stoppingToken);

        await Task.CompletedTask;
        return TypedResults.Problem("Not implemented.");
    }
}
