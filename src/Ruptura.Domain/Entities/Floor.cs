namespace Ruptura.Domain.Entities;

// A floor scoped to one Campaign and belonging to one Arc — the second level of the
// session-prep content tree (Arc → Floor). Deleting the parent Arc cascades its Floors.
// The typed prep fields (objective type, identity, objectives, failure condition, notes,
// linked encounter/reward ids) live in the DataJson blob — see
// Ruptura.Shared.Content.FloorData.
public class Floor
{
    public Guid Id { get; set; }

    public Guid CampaignId { get; set; }

    public Guid ArcId { get; set; }

    // Floor number within the campaign/dungeon.
    public int Number { get; set; }

    public string Name { get; set; } = string.Empty;

    // Typed FloorData blob — see Ruptura.Shared.Content.FloorData.
    public string DataJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
