namespace Ruptura.Shared.Guilds;

// Selector ONLY — no numeric deltas. The server recomputes the effect for {Kind, TargetId, Days}.
public class ApplyInterludeRequest
{
    public string Kind { get; set; } = string.Empty; // Maintenance|Income|ResearchProgress|CraftingProgress
    public Guid? TargetId { get; set; }              // required for ResearchProgress/CraftingProgress
    public int Days { get; set; }

    // The guild blob version the client's projection was based on. A Maintenance/Income apply is a
    // read-modify-write on the blob (Resources.Silver); this is checked against the row's xmin so a
    // concurrent blob save between the client's load and the apply → Guild.Conflict, never a silent
    // lost update. Ignored by the ResearchProgress/CraftingProgress branches (they touch child rows).
    public uint Version { get; set; }
}
