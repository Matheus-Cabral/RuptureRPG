using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class SessionLogRepository(AppDbContext db)
    : BaseRepository<SessionLog>(db), ISessionLogRepository
{
    public async Task<IEnumerable<SessionLog>> GetByCampaignAsync(
        Guid campaignId, CancellationToken ct = default) =>
        await Set
            .Where(s => s.CampaignId == campaignId)
            .OrderByDescending(s => s.Date)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync(ct);
}
