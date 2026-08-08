using Ruptura.Application.Common;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Interfaces;

public interface IGuildSheetService
{
    Task<Result<GuildSheetResponse>> GetByCampaignAsync(Guid callerId, Guid campaignId, CancellationToken ct = default);

    // Look up the guild BY ID → its campaign → GM-or-member. Non-member/not-found → Guild.NotFound.
    // Used by MediaController for the path-encoded emblem upload/download authorization.
    Task<Result<GuildSheet>> AuthorizeGuildAccessByIdAsync(Guid callerId, Guid guildSheetId, CancellationToken ct = default);

    // Sets Identity.EmblemImagePath inside the blob, preserving all other blob data. Version-
    // checkpointed against expectedVersion: a stale token → Guild.Conflict (never clobbers a
    // concurrent write nor 500s). On success returns the new xmin so the caller can refresh
    // its Version without re-GETting the whole guild.
    Task<Result<uint>> SetEmblemPathAsync(
        Guid guildSheetId, string path, uint expectedVersion, CancellationToken ct = default);

    // Reads the current Identity.EmblemImagePath via the guarded Deserialize so MediaController
    // never has to deserialize the blob inline.
    Task<Result<string?>> GetEmblemPathAsync(Guid guildSheetId, CancellationToken ct = default);

    Task<Result<GuildSheetResponse>> UpdateAsync(
        Guid callerId, Guid campaignId, UpdateGuildSheetRequest request, CancellationToken ct = default);

    Task<Result<ExpeditionResponse>> AddExpeditionAsync(
        Guid callerId, Guid campaignId, CreateExpeditionRequest request, CancellationToken ct = default);

    Task<Result<ExpeditionResponse>> UpdateExpeditionAsync(
        Guid callerId, Guid campaignId, Guid expeditionId, UpdateExpeditionRequest request, CancellationToken ct = default);

    Task<Result> DeleteExpeditionAsync(
        Guid callerId, Guid campaignId, Guid expeditionId, CancellationToken ct = default);

    Task<Result<GuildBuildingResponse>> AddBuildingAsync(
        Guid callerId, Guid campaignId, CreateBuildingRequest request, CancellationToken ct = default);

    Task<Result<GuildBuildingResponse>> UpdateBuildingAsync(
        Guid callerId, Guid campaignId, Guid buildingId, UpdateBuildingRequest request, CancellationToken ct = default);

    Task<Result> DeleteBuildingAsync(
        Guid callerId, Guid campaignId, Guid buildingId, CancellationToken ct = default);

    Task<Result<GuildStaffResponse>> AddStaffAsync(
        Guid callerId, Guid campaignId, CreateStaffRequest request, CancellationToken ct = default);

    Task<Result<GuildStaffResponse>> UpdateStaffAsync(
        Guid callerId, Guid campaignId, Guid staffId, UpdateStaffRequest request, CancellationToken ct = default);

    Task<Result> DeleteStaffAsync(
        Guid callerId, Guid campaignId, Guid staffId, CancellationToken ct = default);

    Task<Result<ResearchProjectResponse>> AddResearchAsync(
        Guid callerId, Guid campaignId, CreateResearchProjectRequest request, CancellationToken ct = default);

    Task<Result<ResearchProjectResponse>> UpdateResearchAsync(
        Guid callerId, Guid campaignId, Guid researchId, UpdateResearchProjectRequest request, CancellationToken ct = default);

    Task<Result> DeleteResearchAsync(
        Guid callerId, Guid campaignId, Guid researchId, CancellationToken ct = default);

    Task<Result<CraftingOrderResponse>> AddCraftingAsync(
        Guid callerId, Guid campaignId, CreateCraftingOrderRequest request, CancellationToken ct = default);

    Task<Result<CraftingOrderResponse>> UpdateCraftingAsync(
        Guid callerId, Guid campaignId, Guid craftingId, UpdateCraftingOrderRequest request, CancellationToken ct = default);

    Task<Result> DeleteCraftingAsync(
        Guid callerId, Guid campaignId, Guid craftingId, CancellationToken ct = default);

    // Interlude preview: server-computed projection of advancing `days`. Display-only deltas.
    Task<Result<InterludeProjection>> PreviewInterludeAsync(
        Guid callerId, Guid campaignId, int days, CancellationToken ct = default);

    // Interlude apply: re-runs the projection from FRESH state and applies ONLY the server-computed
    // delta for the selected {Kind, TargetId} — never any number from the request body.
    Task<Result<GuildSheetResponse>> ApplyInterludeAsync(
        Guid callerId, Guid campaignId, ApplyInterludeRequest request, CancellationToken ct = default);
}
