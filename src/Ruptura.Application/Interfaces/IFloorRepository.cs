using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IFloorRepository : IRepository<Floor>
{
    // All floors for one campaign, ordered by Number then creation. Campaign-ownership auth is the
    // service's responsibility — the repository only scopes by CampaignId.
    Task<IEnumerable<Floor>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);

    // All floors belonging to one arc, ordered by Number then creation.
    Task<IEnumerable<Floor>> GetByArcAsync(Guid arcId, CancellationToken ct = default);
}
