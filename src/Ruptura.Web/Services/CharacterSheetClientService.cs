using System.Net.Http.Json;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public class CharacterSheetClientService(IHttpClientFactory factory) : ICharacterSheetClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<CharacterSheetResponse>?> GrantAsync(Guid campaignId, GrantCharacterSheetRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/character-sheets", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>();
    }

    public async Task<ApiResponse<IEnumerable<CharacterSheetResponse>>?> GetByCampaignAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/character-sheets");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CharacterSheetResponse>>>();
    }

    public async Task<ApiResponse<CharacterSheetResponse>?> GetMineAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/character-sheets/mine");
        return await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>();
    }

    public async Task<ApiResponse<CharacterSheetResponse>?> GetAsync(Guid id)
    {
        var response = await Http.GetAsync($"api/character-sheets/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>();
    }

    public async Task<ApiResponse<CharacterSheetResponse>?> UpdateAsync(Guid id, UpdateCharacterSheetRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/character-sheets/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>();
    }
}
