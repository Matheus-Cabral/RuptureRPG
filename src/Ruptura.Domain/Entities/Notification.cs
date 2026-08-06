using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid RecipientUserId { get; set; }          // the Game Master
    public Guid CampaignId { get; set; }                // denormalized, for grouping in the UI
    public NotificationType Type { get; set; }
    public Guid? RelatedCharacterSheetId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
