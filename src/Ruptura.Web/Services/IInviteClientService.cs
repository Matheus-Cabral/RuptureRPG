using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.Web.Services;

public interface IInviteClientService
{
    Task<ApiResponse<InviteCodeResponse>?> GenerateAsync();
    Task<ApiResponse<IEnumerable<InviteCodeResponse>>?> GetAllAsync();
}
