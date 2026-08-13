using System.Net.Http.Json;
using Ruptura.Shared.Combat;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public class CombatClientService(IHttpClientFactory factory) : ICombatClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<CombatSessionResponse>>?> GetAllAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/combat");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CombatSessionResponse>>>();
    }

    public async Task<ApiResponse<CombatSessionResponse>?> GetByIdAsync(Guid campaignId, Guid id)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/combat/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>();
    }

    public async Task<ApiResponse<CombatSessionResponse>?> CreateAsync(Guid campaignId, CreateCombatSessionRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/combat", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>();
    }

    public async Task<ApiResponse<CombatSessionResponse>?> StartFromEncounterAsync(Guid campaignId, StartFromEncounterRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/combat/start-from-encounter", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>();
    }

    public async Task<ApiResponse<CombatSessionResponse>?> UpdateStateAsync(Guid campaignId, Guid id, UpdateCombatStateRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/combat/{id}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>();
    }

    public async Task<ApiResponse?> DeleteAsync(Guid campaignId, Guid id)
    {
        var response = await Http.DeleteAsync($"api/campaigns/{campaignId}/combat/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
