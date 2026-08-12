using Ruptura.Application.Common;
using Ruptura.Shared.Bestiary;

namespace Ruptura.Application.Interfaces;

public interface ICreatureService
{
    // A GM's library: own homebrew + official examples.
    Task<Result<IEnumerable<CreatureResponse>>> GetForGameMasterAsync(
        Guid gameMasterId, CancellationToken ct = default);

    // Own or official → the creature; another GM's homebrew → NotFound (existence hidden).
    Task<Result<CreatureResponse>> GetByIdAsync(
        Guid gameMasterId, Guid id, CancellationToken ct = default);

    Task<Result<CreatureResponse>> CreateAsync(
        Guid gameMasterId, CreateCreatureRequest request, CancellationToken ct = default);

    // Own only. Official → Forbidden; another GM's homebrew → NotFound.
    Task<Result<CreatureResponse>> UpdateAsync(
        Guid gameMasterId, Guid id, UpdateCreatureRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid gameMasterId, Guid id, CancellationToken ct = default);
}
