using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class ExpeditionRepository(AppDbContext db)
    : BaseRepository<Expedition>(db), IExpeditionRepository
{
    public async Task<IEnumerable<Expedition>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default) =>
        await Set.Where(e => e.GuildSheetId == guildSheetId)
                 .OrderByDescending(e => e.Date)
                 .ToListAsync(ct);
}
