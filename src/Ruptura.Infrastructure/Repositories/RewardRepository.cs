using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class RewardRepository(AppDbContext db)
    : BaseRepository<Reward>(db), IRewardRepository
{
    public async Task<IEnumerable<Reward>> GetByCampaignAsync(
        Guid campaignId, CancellationToken ct = default) =>
        await Set
            .Where(r => r.CampaignId == campaignId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
}
