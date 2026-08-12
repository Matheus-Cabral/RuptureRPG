using Ruptura.Application.Common;
using Ruptura.Shared.Encounters;

namespace Ruptura.Application.Interfaces;

// Encounter CRUD + server-authoritative threat resolution (GDD §9.8/§9.9). Every method is
// scoped to a campaign the caller GM owns; a non-owned/missing campaign or encounter yields
// Encounter.NotFound (existence hidden). All threat values are recomputed on every read/write
// from the persisted blob — a client-supplied Pe/R is never trusted (the DTO carries none).
public interface IEncounterService
{
    Task<Result<IEnumerable<EncounterResponse>>> GetForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);

    Task<Result<EncounterResponse>> GetByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default);

    Task<Result<EncounterResponse>> CreateAsync(
        Guid gameMasterId, Guid campaignId, CreateEncounterRequest request, CancellationToken ct = default);

    Task<Result<EncounterResponse>> UpdateAsync(
        Guid gameMasterId, Guid campaignId, Guid id, UpdateEncounterRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default);
}
