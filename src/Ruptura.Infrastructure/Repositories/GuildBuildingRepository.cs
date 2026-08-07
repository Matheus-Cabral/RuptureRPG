using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class GuildBuildingRepository(AppDbContext db)
    : BaseRepository<GuildBuilding>(db), IGuildBuildingRepository
{
    public async Task<IEnumerable<GuildBuilding>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default) =>
        await Set.Where(b => b.GuildSheetId == guildSheetId).ToListAsync(ct);
}
