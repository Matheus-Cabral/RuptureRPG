using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Identity;
using Ruptura.Shared.Invites;

namespace Ruptura.Infrastructure.Services;

public class InviteCodeService(
    IInviteCodeRepository repo,
    UserManager<ApplicationUser> userManager) : IInviteCodeService
{
    private const int CodeLength = 10;
    private const int ExpirationHours = 48;

    public async Task<Result<InviteCodeResponse>> GenerateAsync(
        Guid gameMasterId,
        CancellationToken ct = default)
    {
        var code = new InviteCode
        {
            Id = Guid.NewGuid(),
            Code = GenerateCode(),
            CreatedByGameMasterId = gameMasterId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(ExpirationHours)
        };

        await repo.AddAsync(code, ct);
        await repo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(code));
    }

    public async Task<Result<InviteCodeResponse>> GetByCodeAsync(
        string code,
        CancellationToken ct = default)
    {
        var invite = await repo.GetByCodeAsync(code, ct);
        if (invite is null)
            return Result.Failure<InviteCodeResponse>(ErrorCodes.Invite.NotFound);

        return Result.Success(MapToResponse(invite));
    }

    public async Task<Result<IEnumerable<InviteCodeResponse>>> GetByGameMasterAsync(
        Guid gameMasterId,
        CancellationToken ct = default)
    {
        var codes = await repo.GetByGameMasterAsync(gameMasterId, ct);

        var responses = new List<InviteCodeResponse>();
        foreach (var code in codes)
        {
            var response = MapToResponse(code);

            if (code.UsedByPlayerId is { } playerId)
            {
                var player = await userManager.FindByIdAsync(playerId.ToString());
                if (player is not null)
                {
                    response.RedeemedByDisplayName = player.DisplayName;
                    response.RedeemedByEmail = player.Email;
                }
            }

            responses.Add(response);
        }

        return Result.Success(responses.AsEnumerable());
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        var bytes = RandomNumberGenerator.GetBytes(CodeLength);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }

    private static InviteCodeResponse MapToResponse(InviteCode c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        IsUsed = c.IsUsed,
        ExpiresAt = c.ExpiresAt,
        CreatedAt = c.CreatedAt,
        UsedAt = c.UsedAt
    };
}
