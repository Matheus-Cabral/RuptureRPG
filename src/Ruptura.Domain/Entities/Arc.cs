namespace Ruptura.Domain.Entities;

// A narrative arc scoped to one Campaign — the top level of the session-prep content tree
// (Arc → Floor). Ordering within a campaign is explicit via Order. The typed prep fields
// (theme, history, conflict, objective, ecosystem, resources, mechanic, notes) live in the
// DataJson blob — see Ruptura.Shared.Content.ArcData.
public class Arc
{
    public Guid Id { get; set; }

    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    // Explicit display order within the campaign.
    public int Order { get; set; }

    // Typed ArcData blob — see Ruptura.Shared.Content.ArcData.
    public string DataJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
