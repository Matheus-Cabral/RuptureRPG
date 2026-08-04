namespace Ruptura.Domain.Entities;

public class CampaignMembership
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
