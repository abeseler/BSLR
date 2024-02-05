using Beseler.API.Application.Services;
using Beseler.Domain.Accounts;
using Beseler.Infrastructure.Services.Jwt;
using Beseler.Shared.Accounts.Requests;
using Beseler.Shared.Accounts.Responses;
using Microsoft.AspNetCore.Authorization;

namespace Beseler.API.Accounts.Handlers;

internal static class RefreshTokenHandler
{
    [AllowAnonymous]
    public static async Task<IResult> HandleAsync(
        RefreshTokenRequest? request,
        TokenService tokenService,
        CookieService cookieService,
        IAccountRepository repository,
        CancellationToken stoppingToken)
    {
        var token = cookieService.TryGetValue(CookieKeys.RefreshToken, out var value) ? value : request?.RefreshToken; 
        if (token is null)
            return TypedResults.Unauthorized();

        var principal = await tokenService.ValidateAsync(token!);
        if (principal?.Identity is not { IsAuthenticated: true, Name: not null } || int.TryParse(principal?.Identity?.Name, out var accountId) is false)
        {
            cookieService.Remove(CookieKeys.RefreshToken);
            return TypedResults.Unauthorized();
        }

        var account = await repository.GetByIdAsync(accountId, stoppingToken);
        if (account is not { IsLocked: false })
        {
            cookieService.Remove(CookieKeys.RefreshToken);
            return TypedResults.Forbid();
        }

        var (_, expiresOn, accessToken) = tokenService.GenerateAccessToken(account);
        var (_, refreshExpiresOn, refreshToken) = tokenService.GenerateRefreshToken(account);

        cookieService.Set(CookieKeys.RefreshToken, refreshToken, refreshExpiresOn);

        return TypedResults.Ok(new AccessTokenResponse("Bearer", accessToken, expiresOn, refreshToken));
    }
}
