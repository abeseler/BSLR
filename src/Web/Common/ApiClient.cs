using Beseler.Shared;
using Beseler.Shared.Accounts.Requests;
using Beseler.Shared.Accounts.Responses;
using Beseler.Web.Accounts.Services;
using Blazored.LocalStorage;
using System.Net.Http.Json;

namespace Beseler.Web.Common;

internal sealed class ApiClient(HttpClient client, AuthStateProvider authStateProvider, ILocalStorageService localStorage)
{
    public async Task<bool> LoginAsync(LoginAccountRequest request)
    {
        var response = await client.PostAsJsonAsync(Endpoints.Accounts.Login, request);
        if (response is not { IsSuccessStatusCode: true })
            return false;

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        if (token is null)
            return false;

        client.DefaultRequestHeaders.Authorization = new("Bearer", token.AccessToken);
        await localStorage.SetItemAsStringAsync(StorageKeys.AccessToken, token.AccessToken);
        await authStateProvider.GetAuthenticationStateAsync();
            
        return true;
    }

    private async Task<bool> RefreshTokenAsync()
    {
        var response = await client.PostAsync(Endpoints.Accounts.Refresh, null);
        if (response is not { IsSuccessStatusCode: true })
            return false;

        var token = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
        if (token is null)
        {
            return false;
        }

        client.DefaultRequestHeaders.Authorization = new("Bearer", token.AccessToken);
        await localStorage.SetItemAsStringAsync(StorageKeys.AccessToken, token.AccessToken);
        await authStateProvider.GetAuthenticationStateAsync();

        return true;
    }
}
