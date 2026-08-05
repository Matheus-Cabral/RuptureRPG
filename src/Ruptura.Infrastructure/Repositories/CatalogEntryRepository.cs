using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CatalogEntryRepository(AppDbContext db)
    : BaseRepository<CatalogEntry>(db), ICatalogEntryRepository
{
    public async Task<IEnumerable<CatalogEntry>> GetByTypeAsync(
        CatalogEntryType type,
        Guid campaignId,
        bool includeArchived,
        CancellationToken ct = default) =>
        await Set
            .Where(c => c.Type == type && (c.CampaignId == null || c.CampaignId == campaignId))
            .Where(c => includeArchived || !c.IsArchived)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        CatalogEntryType type,
        Guid? campaignId,
        string name,
        CancellationToken ct = default) =>
        await Set.AnyAsync(c => c.Type == type && c.CampaignId == campaignId && c.Name == name, ct);

    public async Task<IEnumerable<CatalogEntry>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return [];
        return await Set.Where(c => idList.Contains(c.Id)).ToListAsync(ct);
    }
}
