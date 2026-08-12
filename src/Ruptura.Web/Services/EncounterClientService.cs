using System.Net.Http.Json;
using Ruptura.Shared.Common;
using Ruptura.Shared.Encounters;

namespace Ruptura.Web.Services;

public class EncounterClientService(IHttpClientFactory factory) : IEncounterClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<EncounterResponse>>?> GetAllAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/encounters");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<EncounterResponse>>>();
    }

    public async Task<ApiResponse<EncounterResponse>?> GetByIdAsync(Guid campaignId, Guid id)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/encounters/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>();
    }

    public async Task<ApiResponse<EncounterResponse>?> CreateAsync(Guid campaignId, CreateEncounterRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/encounters", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>();
    }

    public async Task<ApiResponse<EncounterResponse>?> UpdateAsync(Guid campaignId, Guid id, UpdateEncounterRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/encounters/{id}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>();
    }

    public async Task<ApiResponse?> DeleteAsync(Guid campaignId, Guid id)
    {
        var response = await Http.DeleteAsync($"api/campaigns/{campaignId}/encounters/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
