using Ruptura.Application.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.Application.Interfaces;

public interface IInviteCodeService
{
    Task<Result<InviteCodeResponse>> GenerateAsync(Guid gameMasterId, CancellationToken ct = default);
    Task<Result<InviteCodeResponse>> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<IEnumerable<InviteCodeResponse>>> GetByGameMasterAsync(Guid gameMasterId, CancellationToken ct = default);
}
