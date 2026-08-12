using System.Net.Http.Json;
using Ruptura.Shared.Bestiary;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public class BestiaryClientService(IHttpClientFactory factory) : IBestiaryClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<CreatureResponse>>?> GetCreaturesAsync()
    {
        var response = await Http.GetAsync("api/bestiary/creatures");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CreatureResponse>>>();
    }

    public async Task<ApiResponse<CreatureResponse>?> GetCreatureAsync(Guid id)
    {
        var response = await Http.GetAsync($"api/bestiary/creatures/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>();
    }

    public async Task<ApiResponse<CreatureResponse>?> CreateCreatureAsync(CreateCreatureRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/bestiary/creatures", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>();
    }

    public async Task<ApiResponse<CreatureResponse>?> UpdateCreatureAsync(Guid id, UpdateCreatureRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/bestiary/creatures/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>();
    }

    public async Task<ApiResponse?> DeleteCreatureAsync(Guid id)
    {
        var response = await Http.DeleteAsync($"api/bestiary/creatures/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse<IEnumerable<NpcResponse>>?> GetNpcsAsync()
    {
        var response = await Http.GetAsync("api/bestiary/npcs");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<NpcResponse>>>();
    }

    public async Task<ApiResponse<NpcResponse>?> GetNpcAsync(Guid id)
    {
        var response = await Http.GetAsync($"api/bestiary/npcs/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>();
    }

    public async Task<ApiResponse<NpcResponse>?> CreateNpcAsync(CreateNpcRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/bestiary/npcs", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>();
    }

    public async Task<ApiResponse<NpcResponse>?> UpdateNpcAsync(Guid id, UpdateNpcRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/bestiary/npcs/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>();
    }

    public async Task<ApiResponse?> DeleteNpcAsync(Guid id)
    {
        var response = await Http.DeleteAsync($"api/bestiary/npcs/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
