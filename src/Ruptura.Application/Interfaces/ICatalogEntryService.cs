using Ruptura.Application.Common;
using Ruptura.Shared.Catalog;

namespace Ruptura.Application.Interfaces;

public interface ICatalogEntryService
{
    Task<Result<IEnumerable<CatalogEntryResponse>>> GetByTypeAsync(
        Guid callerId, string type, Guid campaignId, bool includeArchived, CancellationToken ct = default);

    Task<Result<CatalogEntryResponse>> CreateAsync(
        Guid gameMasterId, CreateCatalogEntryRequest request, CancellationToken ct = default);

    Task<Result<CatalogEntryResponse>> UpdateAsync(
        Guid gameMasterId, Guid entryId, UpdateCatalogEntryRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid gameMasterId, Guid entryId, CancellationToken ct = default);
}
