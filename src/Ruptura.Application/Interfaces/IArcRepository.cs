using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IArcRepository : IRepository<Arc>
{
    // All arcs for one campaign, ordered by Order then creation. Campaign-ownership auth is the
    // service's responsibility — the repository only scopes by CampaignId.
    Task<IEnumerable<Arc>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
}
