using Beseler.Domain.Accounts;
using Beseler.Shared.Accounts.Requests;
using Microsoft.AspNetCore.Authorization;

namespace Beseler.API.Accounts.Handlers;

internal static class RefreshTokenHandler
{
    [AllowAnonymous]
    public static Task<IResult> HandleAsync(
        RefreshTokenRequest request,
        IAccountRepository repository,
        CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
