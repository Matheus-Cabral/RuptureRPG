using Ruptura.Application.Common;
using Ruptura.Shared.Bestiary;

namespace Ruptura.Application.Interfaces;

public interface INpcService
{
    // A GM's library: own homebrew + official examples.
    Task<Result<IEnumerable<NpcResponse>>> GetForGameMasterAsync(
        Guid gameMasterId, CancellationToken ct = default);

    // Own or official → the NPC; another GM's homebrew → NotFound (existence hidden).
    Task<Result<NpcResponse>> GetByIdAsync(
        Guid gameMasterId, Guid id, CancellationToken ct = default);

    Task<Result<NpcResponse>> CreateAsync(
        Guid gameMasterId, CreateNpcRequest request, CancellationToken ct = default);

    // Own only. Official → Forbidden; another GM's homebrew → NotFound.
    Task<Result<NpcResponse>> UpdateAsync(
        Guid gameMasterId, Guid id, UpdateNpcRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid gameMasterId, Guid id, CancellationToken ct = default);
}
