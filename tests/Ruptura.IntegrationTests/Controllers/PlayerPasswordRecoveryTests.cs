using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Auth;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class PlayerPasswordRecoveryTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private record Setup(HttpClient GmClient, Guid PlayerId, string PlayerEmail, AuthResponse PlayerAuth);

    private async Task<Setup> SetupGmWithPlayerAsync()
    {
        var gmClient = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(gmClient, Faker.Internet.Email());
        AuthHelper.SetBearerToken(gmClient, gm.AccessToken);

        var invite = (await (await gmClient.PostAsync("api/invites", null)).Content
            .ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!;

        var playerEmail = Faker.Internet.Email();
        var playerAuth = await AuthHelper.RegisterPlayerAsync(gmClient, invite.Code, playerEmail, "OriginalPass1");

        var roster = (await (await gmClient.GetAsync("api/gamemaster/players")).Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>())!.Data!;

        return new Setup(gmClient, roster.Single(p => p.Email == playerEmail).Id, playerEmail, playerAuth);
    }

    private static async Task<string> ResetAsync(HttpClient gmClient, Guid playerId)
    {
        var response = await gmClient.PostAsync($"api/gamemaster/players/{playerId}/reset-password", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ApiResponse<ResetPlayerPasswordResponse>>())!
            .Data!.TemporaryPassword;
    }

    private async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var response = await factory.CreateClient().PostAsJsonAsync("api/auth/login",
            new LoginRequest { Email = email, Password = password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!;
    }

    [Fact]
    public async Task FullFlow_GmResetsPassword_PlayerIsBlockedUntilTheyChooseANewOne()
    {
        var s = await SetupGmWithPlayerAsync();

        // 1. GM generates the temporary password
        var temp = await ResetAsync(s.GmClient, s.PlayerId);

        // 2. Player logs in with it and is told a change is required
        var tempAuth = await LoginAsync(s.PlayerEmail, temp);
        tempAuth.User.MustChangePassword.Should().BeTrue();

        var player = factory.CreateClient();
        AuthHelper.SetBearerToken(player, tempAuth.AccessToken);

        // 3. The server blocks normal endpoints while the flag is set…
        var blocked = await player.GetAsync("api/campaigns/mine");
        blocked.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        // …but the account can still identify itself
        (await player.GetAsync("api/auth/me")).StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Player picks a new password
        var change = await player.PostAsJsonAsync("api/auth/change-password", new ChangePasswordRequest
        {
            NewPassword = "BrandNewPass9",
            ConfirmNewPassword = "BrandNewPass9"
        });
        change.StatusCode.Should().Be(HttpStatusCode.OK);
        var newAuth = (await change.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!;
        newAuth.User.MustChangePassword.Should().BeFalse();

        // 5. The fresh tokens work everywhere again
        AuthHelper.SetBearerToken(player, newAuth.AccessToken);
        (await player.GetAsync("api/campaigns/mine")).StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. The new password logs in; the temporary one no longer does
        (await LoginAsync(s.PlayerEmail, "BrandNewPass9")).User.MustChangePassword.Should().BeFalse();
        var oldTemp = await factory.CreateClient().PostAsJsonAsync("api/auth/login",
            new LoginRequest { Email = s.PlayerEmail, Password = temp });
        oldTemp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Reset_RevokesThePlayersExistingRefreshToken()
    {
        var s = await SetupGmWithPlayerAsync();

        await ResetAsync(s.GmClient, s.PlayerId);

        var refresh = await factory.CreateClient().PostAsJsonAsync("api/auth/refresh",
            new RefreshTokenRequest { RefreshToken = s.PlayerAuth.RefreshToken });
        refresh.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Reset_ForPlayerOfAnotherGameMaster_Returns404AndKeepsOriginalPassword()
    {
        var s = await SetupGmWithPlayerAsync();

        var otherGm = factory.CreateClient();
        var other = await AuthHelper.RegisterGameMasterAsync(otherGm, Faker.Internet.Email());
        AuthHelper.SetBearerToken(otherGm, other.AccessToken);

        var response = await otherGm.PostAsync($"api/gamemaster/players/{s.PlayerId}/reset-password", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await LoginAsync(s.PlayerEmail, "OriginalPass1")).User.MustChangePassword.Should().BeFalse();
    }

    [Fact]
    public async Task Reset_AsPlayer_Returns403()
    {
        var s = await SetupGmWithPlayerAsync();
        var player = factory.CreateClient();
        AuthHelper.SetBearerToken(player, s.PlayerAuth.AccessToken);

        var response = await player.PostAsync($"api/gamemaster/players/{s.PlayerId}/reset-password", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangePassword_WithoutTemporaryPassword_Returns400()
    {
        var s = await SetupGmWithPlayerAsync();
        var player = factory.CreateClient();
        AuthHelper.SetBearerToken(player, s.PlayerAuth.AccessToken);

        var response = await player.PostAsJsonAsync("api/auth/change-password", new ChangePasswordRequest
        {
            NewPassword = "BrandNewPass9",
            ConfirmNewPassword = "BrandNewPass9"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await LoginAsync(s.PlayerEmail, "OriginalPass1")).Should().NotBeNull();
    }

    [Fact]
    public async Task ChangePassword_WithMismatchedConfirmation_Returns400AndKeepsTheTemporaryPassword()
    {
        var s = await SetupGmWithPlayerAsync();
        var temp = await ResetAsync(s.GmClient, s.PlayerId);
        var tempAuth = await LoginAsync(s.PlayerEmail, temp);
        var player = factory.CreateClient();
        AuthHelper.SetBearerToken(player, tempAuth.AccessToken);

        var response = await player.PostAsJsonAsync("api/auth/change-password", new ChangePasswordRequest
        {
            NewPassword = "BrandNewPass9",
            ConfirmNewPassword = "SomethingElse9"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await LoginAsync(s.PlayerEmail, temp)).User.MustChangePassword.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_WithoutAuthentication_Returns401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("api/auth/change-password",
            new ChangePasswordRequest { NewPassword = "BrandNewPass9", ConfirmNewPassword = "BrandNewPass9" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
