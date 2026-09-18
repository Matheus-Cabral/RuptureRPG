using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Web.Services;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class CampaignMemberClientServiceTests
{
    private static CampaignClientService CreateSut(
        HttpResponseMessage response, Action<HttpRequestMessage, string>? onRequest = null)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (request, _) =>
            {
                onRequest?.Invoke(request, request.Content is null ? "" : await request.Content.ReadAsStringAsync());
                return response;
            });
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("RupturaApi"))
            .Returns(new HttpClient(handler.Object) { BaseAddress = new Uri("https://api.local/") });
        return new CampaignClientService(factory.Object);
    }

    [Fact]
    public async Task RemoveMemberAsync_OnSuccess_PostsThePasswordToTheRemoveEndpoint()
    {
        var campaignId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        HttpRequestMessage? sent = null;
        string? sentBody = null;
        var sut = CreateSut(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(ApiResponse.Ok("Player removed from the campaign."))
            },
            (request, body) => { sent = request; sentBody = body; });

        var result = await sut.RemoveMemberAsync(campaignId, playerId, new RemoveMemberRequest { Password = "GmPass123" });

        result!.Success.Should().BeTrue();
        sent!.Method.Should().Be(HttpMethod.Post);
        sent.RequestUri!.AbsolutePath.Should().Be($"/api/campaigns/{campaignId}/members/{playerId}/remove");
        sentBody.Should().Contain("GmPass123");
    }

    [Fact]
    public async Task RemoveMemberAsync_OnWrongPassword_SurfacesTheApiMessage()
    {
        var sut = CreateSut(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(ApiResponse.Fail("Incorrect password."))
        });

        var result = await sut.RemoveMemberAsync(Guid.NewGuid(), Guid.NewGuid(), new RemoveMemberRequest { Password = "x" });

        result!.Success.Should().BeFalse();
        result.Message.Should().Be("Incorrect password.");
    }

    [Fact]
    public async Task RemoveMemberAsync_WhenBodyIsNotJson_ReturnsNullInsteadOfThrowing()
    {
        var sut = CreateSut(new HttpResponseMessage(HttpStatusCode.BadGateway)
        {
            Content = new StringContent("<html>bad gateway</html>", System.Text.Encoding.UTF8, "text/html")
        });

        var result = await sut.RemoveMemberAsync(Guid.NewGuid(), Guid.NewGuid(), new RemoveMemberRequest { Password = "x" });

        result.Should().BeNull();
    }
}
