using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IEncounterRepository : IRepository<Encounter>
{
    // All encounters for one campaign, newest first. Campaign-ownership auth is the
    // service's responsibility — the repository only scopes by CampaignId.
    Task<IEnumerable<Encounter>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
}
