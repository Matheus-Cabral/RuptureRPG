using Ruptura.Application.Common;
using Ruptura.Shared.Notifications;

namespace Ruptura.Application.Interfaces;

public interface INotificationService
{
    Task<Result> CheckAndCreateRankPromotionNotificationAsync(
        Guid campaignId, Guid characterSheetId, string currentRanking, int currentNp, CancellationToken ct = default);

    Task<Result<IEnumerable<NotificationGroupResponse>>> GetForGameMasterAsync(
        Guid gameMasterId, CancellationToken ct = default);

    Task<Result> PromoteAsync(Guid gameMasterId, Guid notificationId, CancellationToken ct = default);

    Task<Result> DismissAsync(Guid gameMasterId, Guid notificationId, CancellationToken ct = default);
}
