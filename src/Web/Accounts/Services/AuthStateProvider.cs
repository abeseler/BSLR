using Beseler.Shared;
using Beseler.Shared.Accounts.Responses;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Beseler.Web.Accounts.Services;

internal sealed class AuthStateProvider(HttpClient http) : AuthenticationStateProvider
{
    private static readonly JsonWebTokenHandler _handler = new();
    private AuthenticationState _state = new(new());
    private DateTimeOffset? _expiresOn;
    private bool _isFirstRun = true;

    public string? Token { get; private set; }
    public bool IsExpired => _expiresOn is not null && _expiresOn.Value < DateTimeOffset.UtcNow;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(_state);
    }

    public void NotifyUserAuthentication(string? token = null, DateTimeOffset? expiresOn = null)
    {
        var principal = ParseToken(token);

        Token = principal is not null ? token : null;
        _expiresOn = principal is not null ? expiresOn : null;
        _state = principal is not null ? new AuthenticationState(principal) : new(new());

        if (Token is not null)
            http.DefaultRequestHeaders.Authorization = new("Bearer", Token);
        else
            http.DefaultRequestHeaders.Authorization = null;

        Log.Information("Authentication state changed: {User}", _state.User.Identity?.Name);
        NotifyAuthenticationStateChanged(Task.FromResult(_state));
    }

    private static ClaimsPrincipal? ParseToken(string? token)
    {
        if (token is null)
            return null;

        var jwt = _handler.ReadJsonWebToken(token);
        if (jwt is null)
        {
            Log.Warning("Token was not a valid json web token: {Token}", token);
            return null;
        }

        var identity = new ClaimsIdentity(jwt.Claims, "JWT", JwtRegisteredClaimNames.Sub, null);
        return new ClaimsPrincipal(identity);
    }
}
