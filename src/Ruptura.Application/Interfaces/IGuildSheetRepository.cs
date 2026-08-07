using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IGuildSheetRepository : IRepository<GuildSheet>
{
    Task<GuildSheet?> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);

    // Detach a tracked entity so a failed INSERT is not re-attempted by a later SaveChanges in the same scope.
    void Detach(GuildSheet entity);
}
