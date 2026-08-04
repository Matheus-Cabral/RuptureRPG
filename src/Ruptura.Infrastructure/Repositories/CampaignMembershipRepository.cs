using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CampaignMembershipRepository(AppDbContext db)
    : BaseRepository<CampaignMembership>(db), ICampaignMembershipRepository
{
    public async Task<IEnumerable<CampaignMembership>> GetByCampaignAsync(
        Guid campaignId,
        CancellationToken ct = default) =>
        await Set
            .Where(m => m.CampaignId == campaignId)
            .OrderBy(m => m.AssignedAt)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        Guid campaignId,
        Guid playerId,
        CancellationToken ct = default) =>
        await Set.AnyAsync(m => m.CampaignId == campaignId && m.PlayerId == playerId, ct);
}
