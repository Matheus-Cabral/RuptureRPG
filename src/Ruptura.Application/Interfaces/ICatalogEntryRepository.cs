using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICatalogEntryRepository : IRepository<CatalogEntry>
{
    Task<IEnumerable<CatalogEntry>> GetByTypeAsync(
        CatalogEntryType type, Guid campaignId, bool includeArchived, CancellationToken ct = default);

    Task<bool> ExistsAsync(CatalogEntryType type, Guid? campaignId, string name, CancellationToken ct = default);

    Task<IEnumerable<CatalogEntry>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
