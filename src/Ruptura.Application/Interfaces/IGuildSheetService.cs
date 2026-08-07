using Ruptura.Application.Common;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Interfaces;

public interface IGuildSheetService
{
    Task<Result<GuildSheetResponse>> GetByCampaignAsync(Guid callerId, Guid campaignId, CancellationToken ct = default);

    Task<Result<GuildSheetResponse>> UpdateAsync(
        Guid callerId, Guid campaignId, UpdateGuildSheetRequest request, CancellationToken ct = default);

    Task<Result<ExpeditionResponse>> AddExpeditionAsync(
        Guid callerId, Guid campaignId, CreateExpeditionRequest request, CancellationToken ct = default);

    Task<Result<ExpeditionResponse>> UpdateExpeditionAsync(
        Guid callerId, Guid campaignId, Guid expeditionId, UpdateExpeditionRequest request, CancellationToken ct = default);

    Task<Result> DeleteExpeditionAsync(
        Guid callerId, Guid campaignId, Guid expeditionId, CancellationToken ct = default);
}
