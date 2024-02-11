using Beseler.Domain.Budgeting;
using System.Diagnostics;
using System.Security.Claims;
using System.Transactions;

namespace Beseler.API.Budgeting.Handlers;

internal static class DeleteBudgetTransactionHandler
{
    public static async Task<IResult> HandleAsync(ClaimsPrincipal principal, Guid id, IBudgetRepository budgetRepository, CancellationToken stoppingToken)
    {
        if (int.TryParse(principal.Identity?.Name, out var accountId) is false)
            return TypedResults.Unauthorized();

        var line = await budgetRepository.GetLineByIdAsync(id, stoppingToken);
        if (line is null || line.AccountId != accountId)
            return TypedResults.NotFound();

        var budget = await budgetRepository.GetAsync(accountId, line.TransactionDate.Year, line.TransactionDate.Month, stoppingToken);
        if (budget is null)
            return TypedResults.NotFound();

        var result = budget.RemoveLine(line.BudgetLineId);
        if (result.IsFailure)
            return result.Match(
                onSuccess: _ => throw new UnreachableException(),
                onFailure: error => TypedResults.Problem(error.Message));

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var saveResult = await budgetRepository.SaveChangesAsync(budget, stoppingToken);
        scope.Complete();

        return saveResult.Match<IResult>(
            onSuccess: _ => TypedResults.NoContent(),
            onFailure: error => TypedResults.Problem(error.Message));
    }
}
