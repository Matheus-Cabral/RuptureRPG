using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared;
using Ruptura.Shared.Notifications;

namespace Ruptura.Infrastructure.Services;

public class NotificationService(
    INotificationRepository notificationRepo,
    ICampaignRepository campaignRepo,
    ICharacterSheetRepository sheetRepo,
    ICharacterSheetService characterSheetService) : INotificationService
{
    // Sequence and ceilings live here, not in CharacterStatsCalculator — this is a
    // save-time business rule about when to notify, not a rendered derived stat.
    // See design spec §4.5/§6.8 and this plan's Global Constraints.
    private static readonly string[] RankOrder = RankProgression.Ordered.ToArray();

    private static readonly Dictionary<string, int> RankCeiling = new()
    {
        ["Bronze"] = 70, ["Ferro"] = 105, ["Aço"] = 145, ["Prata"] = 195,
        ["Ouro"] = 260, ["Mithril"] = 340, ["Adamante"] = 430
        // Lendário deliberately absent: open-ended range, never triggers a promotion.
    };

    public async Task<Result> CheckAndCreateRankPromotionNotificationAsync(
        Guid campaignId, Guid characterSheetId, string currentRanking, int currentNp,
        CancellationToken ct = default)
    {
        if (!ExceedsCurrentRankCeiling(currentRanking, currentNp))
        {
            // A GM may resolve an outstanding notification by manually editing Ranking
            // instead of clicking Promote (design spec §4.5's two resolution paths). If that
            // manual edit already brought NP back under the new rank's ceiling, any old
            // unread notification for this sheet is stale — clear it, or the GM could later
            // click a leftover "Promote" and advance a rank the current NP no longer justifies.
            await notificationRepo.MarkReadForSheetAsync(characterSheetId, NotificationType.RankPromotionAvailable, ct);
            return Result.Success();
        }

        if (await notificationRepo.ExistsUnreadForSheetAsync(characterSheetId, NotificationType.RankPromotionAvailable, ct))
            return Result.Success();

        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null)
            return Result.Success(); // nothing to notify — defensive, campaigns are never deleted in this slice

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = campaign.GameMasterId,
            CampaignId = campaignId,
            Type = NotificationType.RankPromotionAvailable,
            RelatedCharacterSheetId = characterSheetId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await notificationRepo.AddAsync(notification, ct);
        await notificationRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<NotificationGroupResponse>>> GetForGameMasterAsync(
        Guid gameMasterId, CancellationToken ct = default)
    {
        var notifications = (await notificationRepo.GetUnreadByRecipientAsync(gameMasterId, ct)).ToList();

        var campaignNames = new Dictionary<Guid, string>();
        var characterNames = new Dictionary<Guid, string>();
        var groups = new List<NotificationGroupResponse>();

        foreach (var campaignGroup in notifications.GroupBy(n => n.CampaignId))
        {
            if (!campaignNames.TryGetValue(campaignGroup.Key, out var campaignName))
            {
                var campaign = await campaignRepo.GetByIdAsync(campaignGroup.Key, ct);
                campaignName = campaign?.Name ?? string.Empty;
                campaignNames[campaignGroup.Key] = campaignName;
            }

            var items = new List<NotificationResponse>();
            foreach (var notification in campaignGroup.OrderByDescending(n => n.CreatedAt))
            {
                string? characterName = null;
                if (notification.RelatedCharacterSheetId is { } sheetId)
                {
                    if (!characterNames.TryGetValue(sheetId, out var name))
                    {
                        var sheet = await sheetRepo.GetByIdAsync(sheetId, ct);
                        name = sheet?.CharacterName ?? string.Empty;
                        characterNames[sheetId] = name;
                    }
                    characterName = name;
                }

                items.Add(new NotificationResponse
                {
                    Id = notification.Id,
                    Type = notification.Type.ToString(),
                    RelatedCharacterSheetId = notification.RelatedCharacterSheetId,
                    CharacterName = characterName,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                });
            }

            groups.Add(new NotificationGroupResponse
            {
                CampaignId = campaignGroup.Key,
                CampaignName = campaignName,
                Notifications = items
            });
        }

        return Result.Success(groups.AsEnumerable());
    }

    public async Task<Result> PromoteAsync(Guid gameMasterId, Guid notificationId, CancellationToken ct = default)
    {
        var notification = await notificationRepo.GetByIdAsync(notificationId, ct);
        // IsRead here means "already resolved" — reject a replay the same way a nonexistent
        // or not-yours notification is rejected, so a double-submit or retried request can't
        // silently advance the rank a second time.
        if (notification is null || notification.RecipientUserId != gameMasterId || notification.IsRead)
            return Result.Failure(ErrorCodes.Notification.NotFound);

        if (notification.RelatedCharacterSheetId is not { } sheetId)
            return Result.Failure(ErrorCodes.Notification.NotPromotable);

        var rankingResult = await characterSheetService.GetRankingAsync(sheetId, ct);
        if (rankingResult.IsFailure)
            return Result.Failure(rankingResult.Error!);

        var nextRank = NextRank(rankingResult.Value!);
        if (nextRank is null)
            return Result.Failure(ErrorCodes.Notification.NotPromotable);

        var setResult = await characterSheetService.SetRankingAsync(sheetId, nextRank, ct);
        if (setResult.IsFailure)
            return Result.Failure(setResult.Error!);

        notification.IsRead = true;
        notificationRepo.Update(notification);
        await notificationRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> DismissAsync(Guid gameMasterId, Guid notificationId, CancellationToken ct = default)
    {
        var notification = await notificationRepo.GetByIdAsync(notificationId, ct);
        if (notification is null || notification.RecipientUserId != gameMasterId || notification.IsRead)
            return Result.Failure(ErrorCodes.Notification.NotFound);

        notification.IsRead = true;
        notificationRepo.Update(notification);
        await notificationRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static bool ExceedsCurrentRankCeiling(string ranking, int np) =>
        RankCeiling.TryGetValue(ranking, out var ceiling) && np > ceiling;

    private static string? NextRank(string ranking)
    {
        var index = Array.IndexOf(RankOrder, ranking);
        return index >= 0 && index + 1 < RankOrder.Length ? RankOrder[index + 1] : null;
    }
}
