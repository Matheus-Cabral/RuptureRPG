using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class EncounterRepository(AppDbContext db)
    : BaseRepository<Encounter>(db), IEncounterRepository
{
    public async Task<IEnumerable<Encounter>> GetByCampaignAsync(
        Guid campaignId, CancellationToken ct = default) =>
        await Set
            .Where(e => e.CampaignId == campaignId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
}
