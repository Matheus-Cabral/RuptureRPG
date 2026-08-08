using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICraftingOrderRepository : IRepository<CraftingOrder>
{
    Task<IEnumerable<CraftingOrder>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default);
}
