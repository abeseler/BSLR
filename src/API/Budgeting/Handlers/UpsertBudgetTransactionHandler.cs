using Beseler.Domain.Budgeting;
using System.Diagnostics;
using System.Security.Claims;
using System.Transactions;

namespace Beseler.API.Budgeting.Handlers;

internal static class UpsertBudgetTransactionHandler
{
    public static async Task<IResult> HandleAsync(ClaimsPrincipal principal, Guid id, BudgetLineDto request, IBudgetRepository budgetRepository, CancellationToken stoppingToken)
    {
        if (int.TryParse(principal.Identity?.Name, out var accountId) is false)
            return TypedResults.Unauthorized();

        var line = await budgetRepository.GetLineByIdAsync(id, stoppingToken);
        if (line is not null && (
                line.AccountId != accountId ||
                line.TransactionDate.Year != request.TransactionDate.Year ||
                line.TransactionDate.Month != request.TransactionDate.Month)
            )
            return TypedResults.NotFound();

        if (Enum.TryParse<BudgetLineType>(request.LineType, out var lineType) is false)
            return TypedResults.BadRequest($"{request.LineType} is not a valid line type.");

        var budget = await budgetRepository.GetAsync(accountId, request.TransactionDate.Year, request.TransactionDate.Month, stoppingToken) ??
            Budget.Create(accountId, request.TransactionDate.Year, request.TransactionDate.Month);

        var result = line is null ?
            budget.AddLine(request.Id, lineType, request.Description, request.TransactionDate, request.Amount) :
            budget.UpdateLine(request.Id, request.TransactionDate, request.Description, request.Amount);

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
