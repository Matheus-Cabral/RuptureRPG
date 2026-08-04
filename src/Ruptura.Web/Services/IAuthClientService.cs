using Ruptura.Shared.Auth;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public interface IAuthClientService
{
    Task<ApiResponse<AuthResponse>?> LoginAsync(LoginRequest request);
    Task<ApiResponse<AuthResponse>?> RegisterGameMasterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>?> RegisterPlayerAsync(RegisterPlayerRequest request);
    Task LogoutAsync();
}
