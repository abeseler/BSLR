using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Beseler.Web.Common.Services;

public sealed class AuthStateProvider(ILocalStorageService localStorage, HttpClient http) : AuthenticationStateProvider
{
    private static readonly AuthenticationState _anonymous = new(new());
    private static readonly JwtSecurityTokenHandler _handler = new();

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        Log.Information("Getting authentication state");

        var token = await localStorage.GetItemAsStringAsync(StorageKeys.AccessToken);
        http.DefaultRequestHeaders.Authorization = null;

        if (string.IsNullOrEmpty(token))
            return _anonymous;

        try
        {
            var jwt = _handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwt.Claims);
            var principal = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(principal);
            http.DefaultRequestHeaders.Authorization = new("Bearer", token);
            NotifyAuthenticationStateChanged(Task.FromResult(state));
            return state;
        }
        catch(Exception ex)
        {
            Log.Error(ex, "Token validation threw an exception");
            await localStorage.RemoveItemAsync(StorageKeys.AccessToken);
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
            return _anonymous;
        }
    }
}
