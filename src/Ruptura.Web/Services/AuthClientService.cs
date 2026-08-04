using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Ruptura.Shared.Auth;
using Ruptura.Web.Auth;

namespace Ruptura.Web.Services;

public class AuthClientService(
    IHttpClientFactory factory,
    ILocalStorageService localStorage,
    AuthenticationStateProvider authStateProvider) : IAuthClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");
    private const string AccessKey = "access_token";
    private const string RefreshKey = "refresh_token";

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<Ruptura.Shared.Common.ApiResponse<AuthResponse>>();
        if (result?.Data is null) return null;

        await PersistAsync(result.Data);
        return result.Data;
    }

    public async Task<AuthResponse?> RegisterGameMasterAsync(RegisterRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/auth/register/gamemaster", request);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<Ruptura.Shared.Common.ApiResponse<AuthResponse>>();
        if (result?.Data is null) return null;

        await PersistAsync(result.Data);
        return result.Data;
    }

    public async Task<AuthResponse?> RegisterPlayerAsync(RegisterPlayerRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/auth/register/player", request);
        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<Ruptura.Shared.Common.ApiResponse<AuthResponse>>();
        if (result?.Data is null) return null;

        await PersistAsync(result.Data);
        return result.Data;
    }

    public async Task LogoutAsync()
    {
        var refreshToken = await localStorage.GetItemAsync<string>(RefreshKey);
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            Http.DefaultRequestHeaders.Add("X-Refresh-Token", refreshToken);
            await Http.PostAsync("api/auth/revoke", null);
            Http.DefaultRequestHeaders.Remove("X-Refresh-Token");
        }

        await localStorage.RemoveItemAsync(AccessKey);
        await localStorage.RemoveItemAsync(RefreshKey);
        ((JwtAuthStateProvider)authStateProvider).NotifyAuthStateChanged();
    }

    private async Task PersistAsync(AuthResponse auth)
    {
        await localStorage.SetItemAsync(AccessKey, auth.AccessToken);
        await localStorage.SetItemAsync(RefreshKey, auth.RefreshToken);
        ((JwtAuthStateProvider)authStateProvider).NotifyAuthStateChanged();
    }
}
