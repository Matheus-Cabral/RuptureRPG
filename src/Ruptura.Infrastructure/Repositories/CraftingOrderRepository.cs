using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CraftingOrderRepository(AppDbContext db)
    : BaseRepository<CraftingOrder>(db), ICraftingOrderRepository
{
    public async Task<IEnumerable<CraftingOrder>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default) =>
        await Set.Where(c => c.GuildSheetId == guildSheetId).ToListAsync(ct);
}
