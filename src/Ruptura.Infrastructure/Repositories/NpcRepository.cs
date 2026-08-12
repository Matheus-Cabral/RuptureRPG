using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class NpcRepository(AppDbContext db)
    : BaseRepository<Npc>(db), INpcRepository
{
    public async Task<IEnumerable<Npc>> GetForGameMasterAsync(
        Guid gameMasterId, CancellationToken ct = default) =>
        await Set
            .Where(n => n.GameMasterId == gameMasterId || n.GameMasterId == null)
            .OrderBy(n => n.Name)
            .ToListAsync(ct);
}
