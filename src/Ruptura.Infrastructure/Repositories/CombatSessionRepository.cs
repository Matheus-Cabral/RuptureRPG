using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CombatSessionRepository(AppDbContext db)
    : BaseRepository<CombatSession>(db), ICombatSessionRepository
{
    public async Task<IEnumerable<CombatSession>> GetByCampaignAsync(
        Guid campaignId, CancellationToken ct = default) =>
        await Set
            .Where(s => s.CampaignId == campaignId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
}
