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
        CancellationToken ct = default) =>
        await Set
            .Where(c => c.Type == type && (c.CampaignId == null || c.CampaignId == campaignId))
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        CatalogEntryType type,
        Guid? campaignId,
        string name,
        CancellationToken ct = default) =>
        await Set.AnyAsync(c => c.Type == type && c.CampaignId == campaignId && c.Name == name, ct);
}
