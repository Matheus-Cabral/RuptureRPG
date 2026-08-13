using System.Net.Http.Json;
using Ruptura.Shared.Common;
using Ruptura.Shared.Rewards;

namespace Ruptura.Web.Services;

public class RewardClientService(IHttpClientFactory factory) : IRewardClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<RewardResponse>>?> GetAllAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/rewards");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<RewardResponse>>>();
    }

    public async Task<ApiResponse<RewardResponse>?> GetByIdAsync(Guid campaignId, Guid id)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/rewards/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>();
    }

    public async Task<ApiResponse<RewardResponse>?> CreateAsync(Guid campaignId, CreateRewardRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/rewards", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>();
    }

    public async Task<ApiResponse<RewardResponse>?> UpdateAsync(Guid campaignId, Guid id, UpdateRewardRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/rewards/{id}", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>();
    }

    public async Task<ApiResponse?> DeleteAsync(Guid campaignId, Guid id)
    {
        var response = await Http.DeleteAsync($"api/campaigns/{campaignId}/rewards/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
