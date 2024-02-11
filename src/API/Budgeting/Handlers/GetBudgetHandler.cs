using Beseler.API.Budgeting.Mappers;
using Beseler.Domain.Budgeting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Beseler.API.Budgeting.Handlers;

internal static class GetBudgetHandler
{
    [Authorize(Policy = Policies.EmailVerified)]
    public static async Task<IResult> HandleAsync(ClaimsPrincipal principal, int year, int month, IBudgetRepository budgetRepository, CancellationToken stoppingToken)
    {
        if (int.TryParse(principal.Identity?.Name, out var accountId) is false)
            return TypedResults.Unauthorized();

        var budget = await budgetRepository.GetAsync(accountId, year, month, stoppingToken)
            ?? Budget.Create(accountId, year, month);

        return TypedResults.Ok(budget.ToResponse());
    }
}
