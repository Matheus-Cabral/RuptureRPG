namespace Ruptura.Shared.Notifications;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? RelatedCharacterSheetId { get; set; }
    public string? CharacterName { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
