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
            return await Http.GetStringAsync($"content/manuals/{fileName}", ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
