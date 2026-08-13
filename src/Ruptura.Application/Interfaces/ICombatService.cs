using Ruptura.Application.Common;
using Ruptura.Shared.Combat;

namespace Ruptura.Application.Interfaces;

// In-session combat tracker CRUD (GM-4). Every method is scoped to a campaign the caller GM owns;
// a non-owned/missing campaign or session yields Combat.NotFound (existence hidden). The live
// tracker state is persisted as a typed CombatState blob. Pressure is server-derived from the
// campaign (Pressure + PressureStateKey), never trusted from the client. StartFromEncounter is
// server-authoritative: creature PV comes from the bestiary and party PV from each sheet.
public interface ICombatService
{
    Task<Result<IEnumerable<CombatSessionResponse>>> GetForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);

    Task<Result<CombatSessionResponse>> GetByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default);

    Task<Result<CombatSessionResponse>> CreateAsync(
        Guid gameMasterId, Guid campaignId, CreateCombatSessionRequest request, CancellationToken ct = default);

    Task<Result<CombatSessionResponse>> StartFromEncounterAsync(
        Guid gameMasterId, Guid campaignId, StartFromEncounterRequest request, CancellationToken ct = default);

    Task<Result<CombatSessionResponse>> UpdateStateAsync(
        Guid gameMasterId, Guid campaignId, Guid sessionId, UpdateCombatStateRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default);
}
