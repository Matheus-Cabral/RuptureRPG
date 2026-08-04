using Ruptura.Shared.Auth;

namespace Ruptura.Web.Services;

public interface IAuthClientService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RegisterGameMasterAsync(RegisterRequest request);
    Task<AuthResponse?> RegisterPlayerAsync(RegisterPlayerRequest request);
    Task LogoutAsync();
}
