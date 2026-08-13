using System.Net.Http.Json;
using Ruptura.Shared.Common;
using Ruptura.Shared.Content;

namespace Ruptura.Web.Services;

public class SessionLogClientService(IHttpClientFactory factory) : ISessionLogClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<SessionLogResponse>>?> GetAllAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/sessions");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<SessionLogResponse>>>();
    }

    public async Task<ApiResponse<SessionLogResponse>?> GetByIdAsync(Guid campaignId, Guid id)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/sessions/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<SessionLogResponse>>();
    }

    public async Task<ApiResponse<SessionLogResponse>?> CreateAsync(Guid campaignId, CreateSessionLogRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/sessions", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<SessionLogResponse>>();
    }

    public async Task<ApiResponse<SessionLogResponse>?> UpdateAsync(Guid campaignId, Guid id, UpdateSessionLogRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/sessions/{id}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<SessionLogResponse>>();
    }

    public async Task<ApiResponse?> DeleteAsync(Guid campaignId, Guid id)
    {
        var response = await Http.DeleteAsync($"api/campaigns/{campaignId}/sessions/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
