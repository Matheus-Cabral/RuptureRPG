using System.Net.Http.Json;
using Ruptura.Shared.Common;
using Ruptura.Shared.Content;

namespace Ruptura.Web.Services;

public class CampaignContentClientService(IHttpClientFactory factory) : ICampaignContentClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    // ── Arcs ──

    public async Task<ApiResponse<IEnumerable<ArcResponse>>?> GetArcsAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/arcs");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ArcResponse>>>();
    }

    public async Task<ApiResponse<ArcResponse>?> GetArcAsync(Guid campaignId, Guid arcId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/arcs/{arcId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<ArcResponse>>();
    }

    public async Task<ApiResponse<ArcResponse>?> CreateArcAsync(Guid campaignId, CreateArcRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/arcs", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<ArcResponse>>();
    }

    public async Task<ApiResponse<ArcResponse>?> UpdateArcAsync(Guid campaignId, Guid arcId, UpdateArcRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/arcs/{arcId}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<ArcResponse>>();
    }

    public async Task<ApiResponse?> DeleteArcAsync(Guid campaignId, Guid arcId)
    {
        var response = await Http.DeleteAsync($"api/campaigns/{campaignId}/arcs/{arcId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }

    // ── Floors ──

    public async Task<ApiResponse<IEnumerable<FloorResponse>>?> GetFloorsAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/floors");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<FloorResponse>>>();
    }

    public async Task<ApiResponse<IEnumerable<FloorResponse>>?> GetFloorsForArcAsync(Guid campaignId, Guid arcId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/arcs/{arcId}/floors");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<FloorResponse>>>();
    }

    public async Task<ApiResponse<FloorResponse>?> GetFloorAsync(Guid campaignId, Guid floorId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/floors/{floorId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>();
    }

    public async Task<ApiResponse<FloorResponse>?> CreateFloorAsync(Guid campaignId, CreateFloorRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/floors", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>();
    }

    public async Task<ApiResponse<FloorResponse>?> UpdateFloorAsync(Guid campaignId, Guid floorId, UpdateFloorRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/floors/{floorId}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>();
    }

    public async Task<ApiResponse?> DeleteFloorAsync(Guid campaignId, Guid floorId)
    {
        var response = await Http.DeleteAsync($"api/campaigns/{campaignId}/floors/{floorId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
