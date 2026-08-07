using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IGuildSheetRepository : IRepository<GuildSheet>
{
    Task<GuildSheet?> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
}
