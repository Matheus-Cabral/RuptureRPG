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

    public async Task<ApiResponse<GuildBuildingResponse>?> AddBuildingAsync(Guid campaignId, CreateBuildingRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/guild/buildings", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>();
    }

    public async Task<ApiResponse<GuildBuildingResponse>?> UpdateBuildingAsync(Guid campaignId, Guid buildingId, UpdateBuildingRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/guild/buildings/{buildingId}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>();
    }

    public async Task<ApiResponse?> DeleteBuildingAsync(Guid campaignId, Guid buildingId)
    {
        var response = await Http.DeleteAsync($"api/campaigns/{campaignId}/guild/buildings/{buildingId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse<GuildStaffResponse>?> AddStaffAsync(Guid campaignId, CreateStaffRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/guild/staff", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<GuildStaffResponse>>();
    }

    public async Task<ApiResponse<GuildStaffResponse>?> UpdateStaffAsync(Guid campaignId, Guid staffId, UpdateStaffRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/guild/staff/{staffId}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<GuildStaffResponse>>();
    }

    public async Task<ApiResponse?> DeleteStaffAsync(Guid campaignId, Guid staffId)
    {
        var response = await Http.DeleteAsync($"api/campaigns/{campaignId}/guild/staff/{staffId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
