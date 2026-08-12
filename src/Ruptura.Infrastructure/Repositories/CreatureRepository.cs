using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CreatureRepository(AppDbContext db)
    : BaseRepository<Creature>(db), ICreatureRepository
{
    public async Task<IEnumerable<Creature>> GetForGameMasterAsync(
        Guid gameMasterId, CancellationToken ct = default) =>
        await Set
            .Where(c => c.GameMasterId == gameMasterId || c.GameMasterId == null)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
}
