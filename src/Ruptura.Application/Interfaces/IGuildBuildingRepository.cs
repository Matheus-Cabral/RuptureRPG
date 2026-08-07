using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IGuildBuildingRepository : IRepository<GuildBuilding>
{
    Task<IEnumerable<GuildBuilding>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default);
}
