using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class FloorRepository(AppDbContext db)
    : BaseRepository<Floor>(db), IFloorRepository
{
    public async Task<IEnumerable<Floor>> GetByCampaignAsync(
        Guid campaignId, CancellationToken ct = default) =>
        await Set
            .Where(f => f.CampaignId == campaignId)
            .OrderBy(f => f.Number)
            .ThenBy(f => f.CreatedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<Floor>> GetByArcAsync(
        Guid arcId, CancellationToken ct = default) =>
        await Set
            .Where(f => f.ArcId == arcId)
            .OrderBy(f => f.Number)
            .ThenBy(f => f.CreatedAt)
            .ToListAsync(ct);
}
