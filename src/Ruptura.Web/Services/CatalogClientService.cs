using System.Net.Http.Json;
using System.Web;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public class CatalogClientService(IHttpClientFactory factory) : ICatalogClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<CatalogEntryResponse>>?> GetByTypeAsync(string type, Guid campaignId)
    {
        var query = $"api/catalog?type={HttpUtility.UrlEncode(type)}&campaignId={campaignId}";
        var response = await Http.GetAsync(query);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>();
    }

    public async Task<ApiResponse<CatalogEntryResponse>?> CreateAsync(CreateCatalogEntryRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/catalog", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>();
    }

    public async Task<ApiResponse<CatalogEntryResponse>?> UpdateAsync(Guid id, UpdateCatalogEntryRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/catalog/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>();
    }

    public async Task<ApiResponse?> DeleteAsync(Guid id)
    {
        var response = await Http.DeleteAsync($"api/catalog/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
