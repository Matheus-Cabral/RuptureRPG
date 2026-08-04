using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Identity;
using Ruptura.Infrastructure.Settings;
using Ruptura.Shared.Auth;

namespace Ruptura.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    JwtService jwtService,
    IInviteCodeRepository inviteRepo,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<Result<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return Result.Failure<AuthResponse>(ErrorCodes.Auth.InvalidCredentials);

        return Result.Success(await IssueTokensAsync(user));
    }

    public async Task<Result<AuthResponse>> RegisterGameMasterAsync(
        RegisterRequest request,
        CancellationToken ct = default)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return Result.Failure<AuthResponse>(ErrorCodes.Auth.EmailAlreadyExists);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = UserRole.GameMaster
        };

        var created = await userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return Result.Failure<AuthResponse>(
                string.Join("; ", created.Errors.Select(e => e.Description)));

        return Result.Success(await IssueTokensAsync(user));
    }

    public async Task<Result<AuthResponse>> RegisterPlayerAsync(
        RegisterPlayerRequest request,
        CancellationToken ct = default)
    {
        var invite = await inviteRepo.GetByCodeAsync(request.InviteCode, ct);
        if (invite is null || !invite.IsValid())
            return Result.Failure<AuthResponse>(ErrorCodes.Auth.InvalidInviteCode);

        if (await userManager.FindByEmailAsync(request.Email) is not null)
            return Result.Failure<AuthResponse>(ErrorCodes.Auth.EmailAlreadyExists);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = UserRole.Player
        };

        var created = await userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
            return Result.Failure<AuthResponse>(
                string.Join("; ", created.Errors.Select(e => e.Description)));

        invite.UsedByPlayerId = user.Id;
        inviteRepo.Update(invite);
        await inviteRepo.SaveChangesAsync(ct);

        return Result.Success(await IssueTokensAsync(user));
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken ct = default)
    {
        var user = userManager.Users
            .FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

        if (user is null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            return Result.Failure<AuthResponse>(ErrorCodes.Auth.InvalidRefreshToken);

        return Result.Success(await IssueTokensAsync(user));
    }

    public async Task<Result> RevokeTokenAsync(
        string refreshToken,
        CancellationToken ct = default)
    {
        var user = userManager.Users
            .FirstOrDefault(u => u.RefreshToken == refreshToken);

        if (user is null)
            return Result.Failure(ErrorCodes.Auth.InvalidRefreshToken);

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await userManager.UpdateAsync(user);

        return Result.Success();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user)
    {
        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays);
        await userManager.UpdateAsync(user);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserInfo
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email!,
                Role = user.Role.ToString()
            }
        };
    }
}
