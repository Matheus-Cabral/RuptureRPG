using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Auth;
using Ruptura.Shared.Common;

namespace Ruptura.IntegrationTests.Controllers;

public class AuthControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly Faker Faker = new();

    // ── Register Game Master ─────────────────────────────────────────────────

    [Fact]
    public async Task RegisterGameMaster_WithValidData_Returns201AndTokens()
    {
        var response = await _client.PostAsJsonAsync("api/auth/register/gamemaster", new RegisterRequest
        {
            DisplayName = "Dungeon Master",
            Email = Faker.Internet.Email(),
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body!.Success.Should().BeTrue();
        body.Data!.AccessToken.Should().NotBeNullOrEmpty();
        body.Data.RefreshToken.Should().NotBeNullOrEmpty();
        body.Data.User.Role.Should().Be("GameMaster");
    }

    [Fact]
    public async Task RegisterGameMaster_WithDuplicateEmail_Returns400()
    {
        var email = Faker.Internet.Email();
        await _client.PostAsJsonAsync("api/auth/register/gamemaster", new RegisterRequest
        {
            DisplayName = "GM One",
            Email = email,
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1"
        });

        var response = await _client.PostAsJsonAsync("api/auth/register/gamemaster", new RegisterRequest
        {
            DisplayName = "GM Two",
            Email = email,
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterGameMaster_WithInvalidPassword_Returns400WithErrors()
    {
        var response = await _client.PostAsJsonAsync("api/auth/register/gamemaster", new RegisterRequest
        {
            DisplayName = "GM",
            Email = Faker.Internet.Email(),
            Password = "weak",
            ConfirmPassword = "weak"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Errors.Should().NotBeNullOrEmpty();
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndTokens()
    {
        var email = Faker.Internet.Email();
        await AuthHelper.RegisterGameMasterAsync(_client, email);

        var response = await _client.PostAsJsonAsync("api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "TestPass1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body!.Data!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var email = Faker.Internet.Email();
        await AuthHelper.RegisterGameMasterAsync(_client, email);

        var response = await _client.PostAsJsonAsync("api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "WrongPassword9"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns401()
    {
        var response = await _client.PostAsJsonAsync("api/auth/login", new LoginRequest
        {
            Email = "ghost@nowhere.com",
            Password = "AnyPass1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Me ───────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Me_WithValidToken_Returns200WithUserInfo()
    {
        var email = Faker.Internet.Email();
        var auth = await AuthHelper.RegisterGameMasterAsync(_client, email);
        AuthHelper.SetBearerToken(_client, auth.AccessToken);

        var response = await _client.GetAsync("api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserInfo>>();
        body!.Data!.Email.Should().Be(email);
        body.Data.Role.Should().Be("GameMaster");
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var client = factory.CreateClient(); // fresh client, no token
        var response = await client.GetAsync("api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Refresh ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsNewTokens()
    {
        var auth = await AuthHelper.RegisterGameMasterAsync(_client, Faker.Internet.Email());

        var response = await _client.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = auth.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body!.Data!.AccessToken.Should().NotBe(auth.AccessToken);
        body.Data.RefreshToken.Should().NotBe(auth.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequest
        {
            RefreshToken = "fake-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Register Player ──────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterPlayer_WithInvalidInviteCode_Returns400()
    {
        var response = await _client.PostAsJsonAsync("api/auth/register/player", new RegisterPlayerRequest
        {
            DisplayName = "Hero",
            Email = Faker.Internet.Email(),
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1",
            InviteCode = "FAKECODE"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
