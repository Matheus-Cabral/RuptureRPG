using System.Net.Http.Json;
using Ruptura.Shared.Common;
using Ruptura.Shared.Journal;

namespace Ruptura.Web.Services;

public class JournalEntryClientService(IHttpClientFactory factory) : IJournalEntryClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<JournalEntryResponse>>?> GetByCharacterSheetAsync(Guid characterSheetId)
    {
        var response = await Http.GetAsync($"api/character-sheets/{characterSheetId}/journal-entries");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>();
    }

    public async Task<ApiResponse<JournalEntryResponse>?> CreateAsync(Guid characterSheetId, CreateJournalEntryRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/character-sheets/{characterSheetId}/journal-entries", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>();
    }

    public async Task<ApiResponse<JournalEntryResponse>?> UpdateAsync(
        Guid characterSheetId, Guid entryId, UpdateJournalEntryRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/character-sheets/{characterSheetId}/journal-entries/{entryId}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>();
    }

    public async Task<ApiResponse?> DeleteAsync(Guid characterSheetId, Guid entryId)
    {
        var response = await Http.DeleteAsync($"api/character-sheets/{characterSheetId}/journal-entries/{entryId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
