using Ruptura.Application.Common;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Interfaces;

public interface IGuildSheetService
{
    Task<Result<GuildSheetResponse>> GetByCampaignAsync(Guid callerId, Guid campaignId, CancellationToken ct = default);
}
