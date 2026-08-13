using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IRewardRepository : IRepository<Reward>
{
    // All rewards for one campaign, newest first. Campaign-ownership auth is the
    // service's responsibility — the repository only scopes by CampaignId.
    Task<IEnumerable<Reward>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
}
