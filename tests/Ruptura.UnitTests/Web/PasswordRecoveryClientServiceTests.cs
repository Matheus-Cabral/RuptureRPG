using System.Net;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Ruptura.Shared.Auth;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Web.Auth;
using Ruptura.Web.Services;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class PasswordRecoveryClientServiceTests
{
    private static (HttpClient Client, Mock<HttpMessageHandler> Handler) CreateClient(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        return (new HttpClient(handler.Object) { BaseAddress = new Uri("https://api.local/") }, handler);
    }

    private static IHttpClientFactory FactoryFor(HttpClient client)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("RupturaApi")).Returns(client);
        return factory.Object;
    }

    private static void VerifyRequest(Mock<HttpMessageHandler> handler, HttpMethod method, string path) =>
        handler.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Method == method && r.RequestUri!.AbsolutePath == path),
            ItExpr.IsAny<CancellationToken>());

    [Fact]
    public async Task ResetPlayerPasswordAsync_OnSuccess_PostsToPlayerResetEndpointAndReturnsTemporaryPassword()
    {
        var playerId = Guid.NewGuid();
        var (client, handler) = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(ApiResponse<ResetPlayerPasswordResponse>.Ok(
                new ResetPlayerPasswordResponse { TemporaryPassword = "Temp-Pass-12" }))
        });
        var sut = new CampaignClientService(FactoryFor(client));

        var result = await sut.ResetPlayerPasswordAsync(playerId);

        result!.Data!.TemporaryPassword.Should().Be("Temp-Pass-12");
        VerifyRequest(handler, HttpMethod.Post, $"/api/gamemaster/players/{playerId}/reset-password");
    }

    [Fact]
    public async Task ResetPlayerPasswordAsync_OnErrorStatus_ReturnsNull()
    {
        var (client, _) = CreateClient(new HttpResponseMessage(HttpStatusCode.NotFound));
        var sut = new CampaignClientService(FactoryFor(client));

        var result = await sut.ResetPlayerPasswordAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task ChangePasswordAsync_OnSuccess_PersistsTheFreshTokens()
    {
        var auth = new AuthResponse
        {
            AccessToken = "new-access",
            RefreshToken = "new-refresh",
            User = new UserInfo { Email = "p@example.com" }
        };
        var (client, handler) = CreateClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(ApiResponse<AuthResponse>.Ok(auth))
        });
        var storage = new Mock<ILocalStorageService>();
        var sut = new AuthClientService(FactoryFor(client), storage.Object, new JwtAuthStateProvider(storage.Object));

        var result = await sut.ChangePasswordAsync(new ChangePasswordRequest
        {
            NewPassword = "BrandNewPass9",
            ConfirmNewPassword = "BrandNewPass9"
        });

        result!.Data!.AccessToken.Should().Be("new-access");
        VerifyRequest(handler, HttpMethod.Post, "/api/auth/change-password");
        storage.Verify(s => s.SetItemAsync("access_token", "new-access", It.IsAny<CancellationToken>()), Times.Once);
        storage.Verify(s => s.SetItemAsync("refresh_token", "new-refresh", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_OnRejection_DoesNotTouchStoredTokensAndSurfacesTheMessage()
    {
        var (client, _) = CreateClient(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(ApiResponse.Fail("Passwords do not match."))
        });
        var storage = new Mock<ILocalStorageService>();
        var sut = new AuthClientService(FactoryFor(client), storage.Object, new JwtAuthStateProvider(storage.Object));

        var result = await sut.ChangePasswordAsync(new ChangePasswordRequest());

        result!.Data.Should().BeNull();
        result.Message.Should().Be("Passwords do not match.");
        storage.Verify(s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
