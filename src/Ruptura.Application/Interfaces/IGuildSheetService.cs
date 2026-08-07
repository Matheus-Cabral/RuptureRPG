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

    // Sets Identity.EmblemImagePath inside the blob, preserving all other blob data. No Version
    // enforcement — targeted server-side mutation like CharacterSheetService.SetPortraitPathAsync.
    Task<Result> SetEmblemPathAsync(Guid guildSheetId, string path, CancellationToken ct = default);

    Task<Result<GuildSheetResponse>> UpdateAsync(
        Guid callerId, Guid campaignId, UpdateGuildSheetRequest request, CancellationToken ct = default);

    Task<Result<ExpeditionResponse>> AddExpeditionAsync(
        Guid callerId, Guid campaignId, CreateExpeditionRequest request, CancellationToken ct = default);

    Task<Result<ExpeditionResponse>> UpdateExpeditionAsync(
        Guid callerId, Guid campaignId, Guid expeditionId, UpdateExpeditionRequest request, CancellationToken ct = default);

    Task<Result> DeleteExpeditionAsync(
        Guid callerId, Guid campaignId, Guid expeditionId, CancellationToken ct = default);
}
