using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Notifications;

namespace Ruptura.Infrastructure.Services;

public class CampaignDashboardService(
    ICampaignRepository campaignRepo,
    ICharacterSheetService characterSheetService,
    IGuildSheetRepository guildRepo,
    IGuildSheetService guildService,
    INotificationService notificationService) : ICampaignDashboardService
{
    public async Task<Result<CampaignDashboardResponse>> GetAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CampaignDashboardResponse>(ErrorCodes.Campaign.NotFound);

        return Result.Success(await BuildAsync(campaign, gameMasterId, ct));
    }

    public async Task<Result<CampaignDashboardResponse>> UpdateDungeonAsync(
        Guid gameMasterId, Guid campaignId, UpdateDungeonStateRequest request, CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CampaignDashboardResponse>(ErrorCodes.Campaign.NotFound);

        if (!DungeonFloorStates.All.Contains(request.FloorState))
            return Result.Failure<CampaignDashboardResponse>(ErrorCodes.Campaign.FloorStateInvalid);

        campaign.CurrentFloor = Math.Max(1, request.CurrentFloor);
        campaign.FloorName = request.FloorName ?? string.Empty;
        campaign.FloorState = request.FloorState;
        campaign.Pressure = Math.Clamp(request.Pressure, 0, 100);
        campaign.UpdatedAt = DateTime.UtcNow;
        campaignRepo.Update(campaign);
        await campaignRepo.SaveChangesAsync(ct);

        return Result.Success(await BuildAsync(campaign, gameMasterId, ct));
    }

    private async Task<CampaignDashboardResponse> BuildAsync(
        Campaign campaign, Guid gameMasterId, CancellationToken ct)
    {
        var (stateKey, mult) = DungeonPressure.StateFor(campaign.Pressure);

        // Party — alive, non-retired.
        var sheetsResult = await characterSheetService.GetByCampaignAsync(gameMasterId, campaign.Id, ct);
        var sheets = sheetsResult.Value ?? Enumerable.Empty<CharacterSheetResponse>();
        var party = sheets
            .Where(s => !s.IsDead && !s.IsRetired)
            .Select(s => new PartyMemberDto
            {
                Id = s.Id,
                CharacterName = s.CharacterName,
                Ranking = s.Data.GuildRegistry.Ranking,
                Np = s.DerivedStats.Np,
                CurrentHp = s.Data.Combat.CurrentHp,
                MaxHp = s.DerivedStats.MaxHp
            })
            .ToList();

        // Guild — GET-only; map via service only if it already exists.
        GuildSnapshotDto? guild = null;
        if (await guildRepo.GetByCampaignAsync(campaign.Id, ct) is not null)
        {
            var g = (await guildService.GetByCampaignAsync(gameMasterId, campaign.Id, ct)).Value;
            if (g is not null)
            {
                guild = new GuildSnapshotDto
                {
                    Stage = g.DerivedStats.Stage.ToString(),
                    Cg = g.DerivedStats.Cg,
                    FloorsConquered = g.Data.FloorsConquered,
                    Silver = g.Data.Resources.Silver,
                    PactCoins = g.Data.Resources.PactCoins
                };
            }
        }

        // Notifications — reuse the GM notification service, take this campaign's group.
        var groups = (await notificationService.GetForGameMasterAsync(gameMasterId, ct)).Value
                     ?? Enumerable.Empty<NotificationGroupResponse>();
        var pending = groups.FirstOrDefault(gp => gp.CampaignId == campaign.Id)?.Notifications
                      ?? new List<NotificationResponse>();

        return new CampaignDashboardResponse
        {
            CampaignId = campaign.Id,
            CampaignName = campaign.Name,
            Dungeon = new DungeonStateDto
            {
                CurrentFloor = campaign.CurrentFloor,
                FloorName = campaign.FloorName,
                FloorState = campaign.FloorState,
                Pressure = campaign.Pressure,
                PressureStateKey = stateKey,
                PeMultiplier = mult
            },
            Party = party,
            Guild = guild,
            PendingNotifications = pending.ToList()
        };
    }
}
