namespace Ruptura.Shared.Campaigns;

public class PlayerRosterResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RecruitedAt { get; set; }
}
