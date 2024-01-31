using Beseler.Domain.Accounts;
using Beseler.Infrastructure.Services.Jwt;
using Beseler.Shared.Accounts.Requests;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Transactions;

namespace Beseler.API.Accounts.Handlers;

internal static class ConfirmEmailHandler
{
    [AllowAnonymous]
    public static async Task<IResult> HandleAsync(
        ConfirmEmailRequest request,
        TokenService tokenService,
        IAccountRepository repository,
        CancellationToken stoppingToken)
    {
        var principal = tokenService.Validate(request.ConfirmationCode);
        if (principal is not { Identity.IsAuthenticated: true })
            return TypedResults.Unauthorized();

        if (int.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var accountId) is false)
            return TypedResults.Unauthorized();

        if (await repository.GetByIdAsync(accountId, stoppingToken) is not {} account)
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
