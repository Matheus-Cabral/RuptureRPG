using Microsoft.AspNetCore.Identity;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Identity;
using Ruptura.Shared.Campaigns;

namespace Ruptura.Infrastructure.Services;

public class CampaignService(
    ICampaignRepository campaignRepo,
    ICampaignMembershipRepository membershipRepo,
    UserManager<ApplicationUser> userManager) : ICampaignService
{
    public async Task<Result<CampaignResponse>> CreateAsync(
        Guid gameMasterId,
        CreateCampaignRequest request,
        CancellationToken ct = default)
    {
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            GameMasterId = gameMasterId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await campaignRepo.AddAsync(campaign, ct);
        await campaignRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(campaign));
    }

    public async Task<Result<IEnumerable<CampaignResponse>>> GetByGameMasterAsync(
        Guid gameMasterId,
        CancellationToken ct = default)
    {
        var campaigns = await campaignRepo.GetByGameMasterAsync(gameMasterId, ct);
        return Result.Success(campaigns.Select(MapToResponse));
    }

    public Task<Result<IEnumerable<PlayerRosterResponse>>> GetRosterAsync(
        Guid gameMasterId,
        CancellationToken ct = default)
    {
        // NOTE: userManager.Users is IQueryable; only synchronous LINQ is used here
        // (not ToListAsync) so this stays testable against a plain in-memory queryable
        // in unit tests, matching the existing convention in AuthService.RefreshTokenAsync.
        var players = userManager.Users
            .Where(u => u.RecruitedByGameMasterId == gameMasterId)
            .ToList();

        var response = players.Select(p => new PlayerRosterResponse
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            Email = p.Email!,
            RecruitedAt = p.CreatedAt
        });

        return Task.FromResult(Result.Success(response));
    }

    public async Task<Result<CampaignMemberResponse>> AssignMemberAsync(
        Guid gameMasterId,
        Guid campaignId,
        AssignMemberRequest request,
        CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CampaignMemberResponse>(ErrorCodes.Campaign.NotFound);

        var player = await userManager.FindByIdAsync(request.PlayerId.ToString());
        if (player is null || player.RecruitedByGameMasterId != gameMasterId)
            return Result.Failure<CampaignMemberResponse>(ErrorCodes.Campaign.PlayerNotInRoster);

        if (await membershipRepo.ExistsAsync(campaignId, request.PlayerId, ct))
            return Result.Failure<CampaignMemberResponse>(ErrorCodes.Campaign.AlreadyMember);

        var membership = new CampaignMembership
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            PlayerId = request.PlayerId,
            AssignedAt = DateTime.UtcNow
        };

        await membershipRepo.AddAsync(membership, ct);
        await membershipRepo.SaveChangesAsync(ct);

        return Result.Success(new CampaignMemberResponse
        {
            PlayerId = player.Id,
            DisplayName = player.DisplayName,
            Email = player.Email!,
            AssignedAt = membership.AssignedAt
        });
    }

    public async Task<Result<IEnumerable<CampaignMemberResponse>>> GetMembersAsync(
        Guid gameMasterId,
        Guid campaignId,
        CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<IEnumerable<CampaignMemberResponse>>(ErrorCodes.Campaign.NotFound);

        var memberships = await membershipRepo.GetByCampaignAsync(campaignId, ct);

        var responses = new List<CampaignMemberResponse>();
        foreach (var membership in memberships)
        {
            var player = await userManager.FindByIdAsync(membership.PlayerId.ToString());
            if (player is null) continue;

            responses.Add(new CampaignMemberResponse
            {
                PlayerId = player.Id,
                DisplayName = player.DisplayName,
                Email = player.Email!,
                AssignedAt = membership.AssignedAt
            });
        }

        return Result.Success(responses.AsEnumerable());
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static CampaignResponse MapToResponse(Campaign c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        CreatedAt = c.CreatedAt
    };
}
