using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICampaignMembershipRepository : IRepository<CampaignMembership>
{
    Task<IEnumerable<CampaignMembership>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid campaignId, Guid playerId, CancellationToken ct = default);
}
