using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICampaignRepository : IRepository<Campaign>
{
    Task<IEnumerable<Campaign>> GetByGameMasterAsync(Guid gameMasterId, CancellationToken ct = default);
}
