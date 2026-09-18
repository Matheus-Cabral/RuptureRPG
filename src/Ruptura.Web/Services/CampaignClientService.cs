using System.Net.Http.Json;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public class CampaignClientService(IHttpClientFactory factory) : ICampaignClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<PlayerRosterResponse>>?> GetRosterAsync()
    {
        var response = await Http.GetAsync("api/gamemaster/players");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>();
    }

    public async Task<ApiResponse<ResetPlayerPasswordResponse>?> ResetPlayerPasswordAsync(Guid playerId)
    {
        var response = await Http.PostAsync($"api/gamemaster/players/{playerId}/reset-password", null);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<ResetPlayerPasswordResponse>>();
    }

    public async Task<ApiResponse?> RemoveMemberAsync(Guid campaignId, Guid playerId, RemoveMemberRequest request)
    {
        var response = await Http.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/members/{playerId}/remove", request);

        // Read the body on failures too: the API's message ("Incorrect password.") is what the
        // confirmation dialog shows. A non-JSON body (e.g. a gateway error page) is "no answer".
        try
        {
            return await response.Content.ReadFromJsonAsync<ApiResponse>();
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    public async Task<ApiResponse<CampaignResponse>?> CreateAsync(CreateCampaignRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/campaigns", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>();
    }

    public async Task<ApiResponse<IEnumerable<CampaignResponse>>?> GetAllAsync()
    {
        var response = await Http.GetAsync("api/campaigns");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignResponse>>>();
    }

    public async Task<ApiResponse<IEnumerable<CampaignResponse>>?> GetMineAsync()
    {
        var response = await Http.GetAsync("api/campaigns/mine");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignResponse>>>();
    }

    public async Task<ApiResponse<IEnumerable<CampaignMemberResponse>>?> GetMembersAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/members");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignMemberResponse>>>();
    }

    public async Task<ApiResponse<CampaignMemberResponse>?> AssignMemberAsync(
        Guid campaignId, AssignMemberRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/members", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CampaignMemberResponse>>();
    }

    public async Task<ApiResponse<CampaignDashboardResponse>?> GetDashboardAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/dashboard");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<CampaignDashboardResponse>>();
    }

    public async Task<ApiResponse<CampaignDashboardResponse>?> UpdateDungeonAsync(
        Guid campaignId, UpdateDungeonStateRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/campaigns/{campaignId}/dashboard/dungeon", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<CampaignDashboardResponse>>();
    }
}
