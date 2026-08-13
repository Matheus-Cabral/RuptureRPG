using Ruptura.Application.Common;
using Ruptura.Shared.Content;

namespace Ruptura.Application.Interfaces;

// Session-prep content CRUD (GM-5): arcs and floors, each persisted as a typed blob scoped to one
// campaign. Every method is campaign-ownership authoritative — a non-owned/missing campaign, arc or
// floor yields Content.NotFound (existence hidden). Floors validate their ArcId, ObjectiveType, and
// soft links (encounters/rewards) against the same campaign; linked names are resolved at read time.
// Deleting an arc cascades its floors at the DB level (DECISION D1).
public interface ICampaignContentService
{
    // ── Arcs ──
    Task<Result<IEnumerable<ArcResponse>>> GetArcsForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);

    Task<Result<ArcResponse>> GetArcByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid arcId, CancellationToken ct = default);

    Task<Result<ArcResponse>> CreateArcAsync(
        Guid gameMasterId, Guid campaignId, CreateArcRequest request, CancellationToken ct = default);

    Task<Result<ArcResponse>> UpdateArcAsync(
        Guid gameMasterId, Guid campaignId, Guid arcId, UpdateArcRequest request, CancellationToken ct = default);

    Task<Result> DeleteArcAsync(
        Guid gameMasterId, Guid campaignId, Guid arcId, CancellationToken ct = default);

    // ── Floors ──
    Task<Result<IEnumerable<FloorResponse>>> GetFloorsForArcAsync(
        Guid gameMasterId, Guid campaignId, Guid arcId, CancellationToken ct = default);

    Task<Result<IEnumerable<FloorResponse>>> GetFloorsForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);

    Task<Result<FloorResponse>> GetFloorByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid floorId, CancellationToken ct = default);

    Task<Result<FloorResponse>> CreateFloorAsync(
        Guid gameMasterId, Guid campaignId, CreateFloorRequest request, CancellationToken ct = default);

    Task<Result<FloorResponse>> UpdateFloorAsync(
        Guid gameMasterId, Guid campaignId, Guid floorId, UpdateFloorRequest request, CancellationToken ct = default);

    Task<Result> DeleteFloorAsync(
        Guid gameMasterId, Guid campaignId, Guid floorId, CancellationToken ct = default);
}
