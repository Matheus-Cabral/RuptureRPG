namespace Ruptura.Shared.Notifications;

public class NotificationGroupResponse
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public List<NotificationResponse> Notifications { get; set; } = [];
}
