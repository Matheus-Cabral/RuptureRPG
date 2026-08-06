using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class NotificationRepository(AppDbContext db)
    : BaseRepository<Notification>(db), INotificationRepository
{
    public async Task<bool> ExistsUnreadForSheetAsync(
        Guid characterSheetId, NotificationType type, CancellationToken ct = default) =>
        await Set.AnyAsync(n =>
            n.RelatedCharacterSheetId == characterSheetId && n.Type == type && !n.IsRead, ct);

    public async Task<IEnumerable<Notification>> GetUnreadByRecipientAsync(
        Guid recipientUserId, CancellationToken ct = default) =>
        await Set
            .Where(n => n.RecipientUserId == recipientUserId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);

    public async Task MarkReadForSheetAsync(
        Guid characterSheetId, NotificationType type, CancellationToken ct = default)
    {
        var notifications = await Set
            .Where(n => n.RelatedCharacterSheetId == characterSheetId && n.Type == type && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in notifications)
            notification.IsRead = true;

        if (notifications.Count > 0)
            await Db.SaveChangesAsync(ct);
    }
}
