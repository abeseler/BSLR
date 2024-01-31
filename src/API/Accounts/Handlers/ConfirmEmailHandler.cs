using Beseler.Domain.Accounts;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Transactions;

namespace Beseler.API.Accounts.Handlers;

internal static class ConfirmEmailHandler
{
    [Authorize]
    public static async Task<IResult> HandleAsync(
        ClaimsPrincipal principal,
        IAccountRepository repository,
        CancellationToken stoppingToken)
    {
        if (int.TryParse(principal.Identity?.Name, out var accountId) is false)
            return TypedResults.Unauthorized();

        if (await repository.GetByIdAsync(accountId, stoppingToken) is not { } account)
            return TypedResults.Unauthorized();

        if (account.IsVerified)
            return TypedResults.BadRequest($"{account.Email} has already been verified.");

        account.Verify();

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var saveResult = await repository.SaveAsync(account, stoppingToken);
        scope.Complete();

        return saveResult.Match<IResult>(
            onSuccess: _ => TypedResults.NoContent(),
            onFailure: error => TypedResults.UnprocessableEntity(error.Message));
    }
}
