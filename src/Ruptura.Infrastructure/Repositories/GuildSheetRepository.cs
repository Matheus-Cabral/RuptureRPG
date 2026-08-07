using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class GuildSheetRepository(AppDbContext db)
    : BaseRepository<GuildSheet>(db), IGuildSheetRepository
{
    public async Task<GuildSheet?> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(g => g.CampaignId == campaignId, ct);
}
