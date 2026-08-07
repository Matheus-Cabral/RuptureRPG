using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IGuildStaffRepository : IRepository<GuildStaff>
{
    Task<IEnumerable<GuildStaff>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default);
}
