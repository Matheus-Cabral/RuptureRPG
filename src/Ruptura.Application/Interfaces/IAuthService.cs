using Ruptura.Application.Common;
using Ruptura.Shared.Auth;

namespace Ruptura.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> RegisterGameMasterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> RegisterPlayerAsync(RegisterPlayerRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
    Task<Result> RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
}
