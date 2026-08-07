using System.Net.Http.Json;
using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;

namespace Ruptura.Web.Services;

public class GuildClientService(IHttpClientFactory factory) : IGuildClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<GuildSheetResponse>?> GetGuildAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/guild");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>();
    }
}
