using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICharacterSheetRepository : IRepository<CharacterSheet>
{
    Task<IEnumerable<CharacterSheet>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);

    Task<CharacterSheet?> GetAliveByOwnerAndCampaignAsync(
        Guid ownerId, Guid campaignId, CancellationToken ct = default);
}
