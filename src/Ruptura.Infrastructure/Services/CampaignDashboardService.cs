using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Content;
using Ruptura.Shared.Notifications;

namespace Ruptura.Infrastructure.Services;

public class CampaignDashboardService(
    ICampaignRepository campaignRepo,
    ICharacterSheetService characterSheetService,
    IGuildSheetRepository guildRepo,
    IGuildSheetService guildService,
    INotificationService notificationService,
    IFloorRepository floorRepo) : ICampaignDashboardService
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

        // Optional current-floor pointer. A null request clears it; a non-null id must reference a
        // floor in THIS campaign — a foreign/missing floor is rejected (DECISION D2).
        if (request.CurrentFloorId is { } floorId)
        {
            var floor = await floorRepo.GetByIdAsync(floorId, ct);
            if (floor is null || floor.CampaignId != campaignId)
                return Result.Failure<CampaignDashboardResponse>(ErrorCodes.Campaign.CurrentFloorInvalid);
            campaign.CurrentFloorId = floorId;
        }
        else
        {
            campaign.CurrentFloorId = null;
        }

        campaign.CurrentFloor = Math.Max(1, request.CurrentFloor);
        var floorName = request.FloorName ?? string.Empty;
        campaign.FloorName = floorName.Length > 120 ? floorName[..120] : floorName;
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

        // Current-floor pointer — resolve-if-present. A since-deleted or foreign floor degrades to
        // null (all three fields) rather than throwing. One lookup only.
        Guid? currentFloorId = null;
        string? currentFloorName = null;
        string? currentFloorObjective = null;
        if (campaign.CurrentFloorId is { } floorId)
        {
            var floor = await floorRepo.GetByIdAsync(floorId, ct);
            if (floor is not null && floor.CampaignId == campaign.Id)
            {
                currentFloorId = floor.Id;
                currentFloorName = floor.Name;
                currentFloorObjective = FloorDataSerializer.DeserializeFloor(floor.DataJson).MainObjective;
            }
        }

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
                PeMultiplier = mult,
                CurrentFloorId = currentFloorId,
                CurrentFloorName = currentFloorName,
                CurrentFloorObjective = currentFloorObjective
            },
            Party = party,
            Guild = guild,
            PendingNotifications = pending.ToList()
        };
    }
}
