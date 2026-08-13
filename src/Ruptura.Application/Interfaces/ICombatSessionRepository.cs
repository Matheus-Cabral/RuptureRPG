using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICombatSessionRepository : IRepository<CombatSession>
{
    // All combat sessions for one campaign, newest first. Campaign-ownership auth is the
    // service's responsibility — the repository only scopes by CampaignId.
    Task<IEnumerable<CombatSession>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
}
