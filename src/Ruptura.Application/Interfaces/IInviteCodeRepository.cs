using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IInviteCodeRepository : IRepository<InviteCode>
{
    Task<InviteCode?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IEnumerable<InviteCode>> GetByGameMasterAsync(Guid gameMasterId, CancellationToken ct = default);
}
