using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IResearchProjectRepository : IRepository<ResearchProject>
{
    Task<IEnumerable<ResearchProject>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default);
}
