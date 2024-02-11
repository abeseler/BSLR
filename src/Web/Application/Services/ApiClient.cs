using Beseler.Shared;
using Beseler.Shared.Accounts.Responses;
using Beseler.Web.Accounts.Services;
using System.Net.Http.Json;

namespace Beseler.Web.Application.Services;

internal sealed class ApiClient(HttpClient client, AuthStateProvider authState)
{
    public async Task<TResponse?> GetSecureAsync<TResponse>(string endpoint) where TResponse : class
    {
        if (authState.Token is not null && authState.IsExpired)
            await RefreshAccessAsync();

        var response = await client.GetAsync(endpoint);
        if (response.IsSuccessStatusCode is false)
            return null;

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<TResponse?> PostSecureAsync<TResponse, TRequest>(string endpoint, TRequest body)
        where TResponse : class
        where TRequest : class
    {
        if (authState.Token is not null && authState.IsExpired)
            await RefreshAccessAsync();

        var response = await client.PostAsJsonAsync(endpoint, body);
        if (response.IsSuccessStatusCode is false)
            return null;

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    public async Task<bool> RefreshAccessAsync()
    {
        var response = await client.PostAsync(Endpoints.Accounts.Refresh, null);
        if (response.IsSuccessStatusCode is false)
            return false;

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        if (token is null)
            return false;

        authState.NotifyUserAuthentication(token.AccessToken, token.ExpiresOn);

        return true;
    }
}
