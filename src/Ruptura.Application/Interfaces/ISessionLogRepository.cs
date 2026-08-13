using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ISessionLogRepository : IRepository<SessionLog>
{
    // All session logs for one campaign, ordered by Date DESCENDING (most recent first). Campaign-
    // ownership auth is the service's responsibility — the repository only scopes by CampaignId.
    Task<IEnumerable<SessionLog>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
}
