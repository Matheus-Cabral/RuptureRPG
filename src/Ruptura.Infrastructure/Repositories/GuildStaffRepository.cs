using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class GuildStaffRepository(AppDbContext db)
    : BaseRepository<GuildStaff>(db), IGuildStaffRepository
{
    public async Task<IEnumerable<GuildStaff>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default) =>
        await Set.Where(s => s.GuildSheetId == guildSheetId).ToListAsync(ct);
}
