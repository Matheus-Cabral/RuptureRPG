using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IExpeditionRepository : IRepository<Expedition>
{
    Task<IEnumerable<Expedition>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default);
}
