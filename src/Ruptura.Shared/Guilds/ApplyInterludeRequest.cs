namespace Ruptura.Shared.Guilds;

// Selector ONLY — no numeric deltas. The server recomputes the effect for {Kind, TargetId, Days}.
public class ApplyInterludeRequest
{
    public string Kind { get; set; } = string.Empty; // Maintenance|Income|ResearchProgress|CraftingProgress
    public Guid? TargetId { get; set; }              // required for ResearchProgress/CraftingProgress
    public int Days { get; set; }
}
