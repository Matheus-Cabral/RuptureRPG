using System.Globalization;

namespace Ruptura.Web.Services;

public class ManualClientService(IHttpClientFactory factory) : IManualClientService
{
    private HttpClient Http => factory.CreateClient("RupturaSelf");

    public async Task<string?> GetManualAsync(ManualType type, CancellationToken ct = default)
    {
        var fileName = ManualReference.FileNameFor(type, CultureInfo.CurrentUICulture.Name);
        try
        {
            var response = await Http.GetAsync($"content/manuals/{fileName}", ct);
            if (!response.IsSuccessStatusCode) return null;

            // nginx (and the WASM dev server) SPA-fallback a missing static file to index.html with a
            // 200 status rather than a 404 — reject an HTML response so a missing manual surfaces the
            // designed error state instead of rendering the app shell as if it were Markdown.
            if (response.Content.Headers.ContentType?.MediaType == "text/html") return null;

            return await response.Content.ReadAsStringAsync(ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
