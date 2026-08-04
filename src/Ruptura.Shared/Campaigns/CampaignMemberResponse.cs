namespace Ruptura.Shared.Campaigns;

public class CampaignMemberResponse
{
    public Guid PlayerId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
