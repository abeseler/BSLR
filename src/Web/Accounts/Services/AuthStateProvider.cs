using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Beseler.Web.Accounts.Services;

internal sealed class AuthStateProvider : AuthenticationStateProvider
{
    private static readonly JsonWebTokenHandler _handler = new();
    private AuthenticationState _state = new(new());
    private DateTimeOffset? _expiresOn;

    public string? Token { get; private set; }
    public bool IsExpired => _expiresOn is not null && _expiresOn.Value < DateTimeOffset.UtcNow;

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(_state);

    public void NotifyUserAuthentication(string? token = null, DateTimeOffset? expiresOn = null)
    {
        try
        {
            if (token is null)
            {
                Log.Information("Authentication state has been reset");

                Token = null;
                _expiresOn = null;
                _state = new(new());
                return;
            }

            var jwt = _handler.ReadJsonWebToken(token);
            if (jwt is null)
            {
                Log.Warning("Token was not a valid json web token: {Token}", token);

                Token = null;
                _expiresOn = null;
                _state = new(new());
                return;
            }

            var identity = new ClaimsIdentity(jwt.Claims, "JWT", JwtRegisteredClaimNames.Sub, null);
            var principal = new ClaimsPrincipal(identity);

            Token = token;
            _expiresOn = expiresOn;
            _state = new AuthenticationState(principal);
        }
        finally
        {
            NotifyAuthenticationStateChanged(Task.FromResult(_state));
        }
    }
}
