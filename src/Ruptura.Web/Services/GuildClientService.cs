using System.Net;
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

    public async Task<GuildSaveResult> UpdateGuildAsync(Guid campaignId, UpdateGuildSheetRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/guild", request);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>();
        return new GuildSaveResult(body, response.StatusCode == HttpStatusCode.Conflict);
    }

    public async Task<ApiResponse<ExpeditionResponse>?> AddExpeditionAsync(Guid campaignId, CreateExpeditionRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/guild/expeditions", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ExpeditionResponse>>();
    }

    public async Task<ApiResponse<ExpeditionResponse>?> UpdateExpeditionAsync(Guid campaignId, Guid expeditionId, UpdateExpeditionRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/guild/expeditions/{expeditionId}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ExpeditionResponse>>();
    }

    public async Task<ApiResponse?> DeleteExpeditionAsync(Guid campaignId, Guid expeditionId)
    {
        var response = await Http.DeleteAsync($"api/campaigns/{campaignId}/guild/expeditions/{expeditionId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
