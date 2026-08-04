using System.Net.Http.Json;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.Web.Services;

public class InviteClientService(IHttpClientFactory factory) : IInviteClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<InviteCodeResponse>?> GenerateAsync()
    {
        var response = await Http.PostAsync("api/invites", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>();
    }

    public async Task<ApiResponse<IEnumerable<InviteCodeResponse>>?> GetAllAsync()
    {
        var response = await Http.GetAsync("api/invites");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<InviteCodeResponse>>>();
    }
}
