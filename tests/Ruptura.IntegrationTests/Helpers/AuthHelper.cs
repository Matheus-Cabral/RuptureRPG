using System.Net.Http.Json;
using Ruptura.Shared.Auth;
using Ruptura.Shared.Common;

namespace Ruptura.IntegrationTests.Helpers;

public static class AuthHelper
{
    public static async Task<AuthResponse> RegisterGameMasterAsync(
        HttpClient client,
        string email = "gm@test.com",
        string password = "TestPass1",
        string displayName = "Test GM")
    {
        var response = await client.PostAsJsonAsync("api/auth/register/gamemaster", new RegisterRequest
        {
            DisplayName = displayName,
            Email = email,
            Password = password,
            ConfirmPassword = password
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return result!.Data!;
    }

    public static void SetBearerToken(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
}
