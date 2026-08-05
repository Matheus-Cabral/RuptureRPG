using Ruptura.Shared.Catalog;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public interface ICatalogClientService
{
    Task<ApiResponse<IEnumerable<CatalogEntryResponse>>?> GetByTypeAsync(string type, Guid campaignId);
    Task<ApiResponse<CatalogEntryResponse>?> CreateAsync(CreateCatalogEntryRequest request);
    Task<ApiResponse<CatalogEntryResponse>?> UpdateAsync(Guid id, UpdateCatalogEntryRequest request);
    Task<ApiResponse?> DeleteAsync(Guid id);
}
