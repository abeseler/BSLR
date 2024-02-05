using Beseler.API.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace Beseler.API.Accounts.Handlers;

internal static class LogoutAccountHandler
{
    [Authorize]
    public static async Task<IResult> HandleAsync(CookieService cookieService)
    {
        await Task.CompletedTask;
        cookieService.Remove(CookieKeys.RefreshToken);
        return TypedResults.NoContent();
    }
}
