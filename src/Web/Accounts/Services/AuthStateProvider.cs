using Beseler.Web.Common;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Beseler.Web.Accounts.Services;

internal sealed class AuthStateProvider(ILocalStorageService localStorage, HttpClient http) : AuthenticationStateProvider
{
    private static readonly JsonWebTokenHandler _handler = new();
    private AuthenticationState _state = new(new());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await localStorage.GetItemAsStringAsync(StorageKeys.AccessToken);
            if (string.IsNullOrWhiteSpace(token))
            {
                http.DefaultRequestHeaders.Authorization = null;
                _state = new(new());
                return _state;
            }

            var jwt = _handler.ReadJsonWebToken(token);
            if (jwt is null)
            {
                Log.Warning("Token was not a valid json web token");
                http.DefaultRequestHeaders.Authorization = null;
                await localStorage.RemoveItemAsync(StorageKeys.AccessToken);
                return _state;
            }

            http.DefaultRequestHeaders.Authorization = new("Bearer", token);
            var identity = new ClaimsIdentity(jwt.Claims, "JWT", JwtRegisteredClaimNames.Sub, null);
            var principal = new ClaimsPrincipal(identity);
            _state = new AuthenticationState(principal);
            return _state;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Caught exception while getting authentication state");
            await localStorage.RemoveItemAsync(StorageKeys.AccessToken);
            return _state;
        }
        finally
        {
            NotifyAuthenticationStateChanged(Task.FromResult(_state));
        }
    }
}
