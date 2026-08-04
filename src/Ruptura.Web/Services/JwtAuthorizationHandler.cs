using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace Ruptura.Web.Services;

public class JwtAuthorizationHandler(ILocalStorageService localStorage) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct)
    {
        var token = await localStorage.GetItemAsync<string>("access_token", ct);
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, ct);
    }
}
