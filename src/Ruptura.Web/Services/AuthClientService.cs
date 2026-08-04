using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Ruptura.Shared.Auth;
using Ruptura.Web.Auth;

namespace Ruptura.Web.Services;

public class AuthClientService(
    HttpClient http,
    ILocalStorageService localStorage,
    AuthenticationStateProvider authStateProvider) : IAuthClientService
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return null;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is null) return null;

        await PersistTokensAsync(auth);
        return auth;
    }

    public async Task<AuthResponse?> RegisterGameMasterAsync(RegisterRequest request)
    {
        var response = await http.PostAsJsonAsync("api/auth/register/gamemaster", request);
        if (!response.IsSuccessStatusCode) return null;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is null) return null;

        await PersistTokensAsync(auth);
        return auth;
    }

    public async Task<AuthResponse?> RegisterPlayerAsync(RegisterPlayerRequest request)
    {
        var response = await http.PostAsJsonAsync("api/auth/register/player", request);
        if (!response.IsSuccessStatusCode) return null;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        if (auth is null) return null;

        await PersistTokensAsync(auth);
        return auth;
    }

    public async Task LogoutAsync()
    {
        await localStorage.RemoveItemAsync(AccessTokenKey);
        await localStorage.RemoveItemAsync(RefreshTokenKey);
        ((JwtAuthStateProvider)authStateProvider).NotifyAuthStateChanged();
    }

    private async Task PersistTokensAsync(AuthResponse auth)
    {
        await localStorage.SetItemAsync(AccessTokenKey, auth.AccessToken);
        await localStorage.SetItemAsync(RefreshTokenKey, auth.RefreshToken);
        ((JwtAuthStateProvider)authStateProvider).NotifyAuthStateChanged();
    }
}
