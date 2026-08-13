using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class ArcRepository(AppDbContext db)
    : BaseRepository<Arc>(db), IArcRepository
{
    public async Task<IEnumerable<Arc>> GetByCampaignAsync(
        Guid campaignId, CancellationToken ct = default) =>
        await Set
            .Where(a => a.CampaignId == campaignId)
            .OrderBy(a => a.Order)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(ct);
}
