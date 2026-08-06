using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<bool> ExistsUnreadForSheetAsync(
        Guid characterSheetId, NotificationType type, CancellationToken ct = default);

    Task<IEnumerable<Notification>> GetUnreadByRecipientAsync(
        Guid recipientUserId, CancellationToken ct = default);

    Task MarkReadForSheetAsync(
        Guid characterSheetId, NotificationType type, CancellationToken ct = default);
}
