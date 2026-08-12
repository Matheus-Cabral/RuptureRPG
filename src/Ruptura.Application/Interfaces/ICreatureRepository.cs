using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICreatureRepository : IRepository<Creature>
{
    // Library read: a GM sees their own homebrew plus the official (owner-null) examples.
    // Another GM's homebrew is never returned (existence hidden).
    Task<IEnumerable<Creature>> GetForGameMasterAsync(Guid gameMasterId, CancellationToken ct = default);
}
