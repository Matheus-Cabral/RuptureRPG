using System.Globalization;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Ruptura.Web.Services;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class ManualClientServiceTests
{
    private static ManualClientService CreateSut(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://web.local/") };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("RupturaSelf")).Returns(httpClient);

        return new ManualClientService(factory.Object);
    }

    private static Mock<HttpMessageHandler> CreateHandlerMock(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        return handler;
    }

    public ManualClientServiceTests()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
    }

    [Fact]
    public async Task GetManualAsync_SuccessWithMarkdownContentType_ReturnsBody()
    {
        const string markdown = "# Manual do Jogador\n\nConteudo.";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(markdown, System.Text.Encoding.UTF8, "text/markdown")
        };
        var handler = CreateHandlerMock(response);
        var sut = CreateSut(handler.Object);

        var result = await sut.GetManualAsync(ManualType.Player);

        result.Should().Be(markdown);
    }

    [Fact]
    public async Task GetManualAsync_SuccessWithUnspecifiedContentType_ReturnsBody()
    {
        const string markdown = "# Manual do Jogador\n\nConteudo.";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes(markdown))
        };
        response.Content.Headers.ContentType = null;
        var handler = CreateHandlerMock(response);
        var sut = CreateSut(handler.Object);

        var result = await sut.GetManualAsync(ManualType.Player);

        result.Should().Be(markdown);
    }

    [Fact]
    public async Task GetManualAsync_SuccessWithHtmlContentType_ReturnsNull()
    {
        const string html = "<!doctype html><html><body>SPA fallback shell</body></html>";
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(html, System.Text.Encoding.UTF8, "text/html")
        };
        var handler = CreateHandlerMock(response);
        var sut = CreateSut(handler.Object);

        var result = await sut.GetManualAsync(ManualType.Player);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetManualAsync_NonSuccessStatusCode_ReturnsNull()
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var handler = CreateHandlerMock(response);
        var sut = CreateSut(handler.Object);

        var result = await sut.GetManualAsync(ManualType.GameMaster);

        result.Should().BeNull();
    }
}
