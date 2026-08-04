using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CampaignRepository(AppDbContext db)
    : BaseRepository<Campaign>(db), ICampaignRepository
{
    public async Task<IEnumerable<Campaign>> GetByGameMasterAsync(
        Guid gameMasterId,
        CancellationToken ct = default) =>
        await Set
            .Where(c => c.GameMasterId == gameMasterId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
}
