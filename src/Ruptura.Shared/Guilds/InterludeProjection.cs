namespace Ruptura.Shared.Guilds;

public class InterludeProjection
{
    public int Days { get; set; }
    public List<InterludeIndicator> Indicators { get; set; } = [];
}

// One projected effect of advancing `Days`. The client shows Label + Description and an
// Apply button carrying {Kind, TargetId, Days}; it never sends the numeric deltas below
// (they are display-only — the server recomputes on Apply).
public class InterludeIndicator
{
    public string Kind { get; set; } = string.Empty; // Maintenance|Income|ResearchProgress|CraftingProgress
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; // human-readable, e.g. "Manutenção de 30 dias: -450 Prata"
    public Guid? TargetId { get; set; }                // research/crafting row id; null for Maintenance/Income

    // Display-only projected deltas (nullable per kind):
    public int? SilverDelta { get; set; }             // Maintenance (negative) / Income (positive)
    public int? DaysAdded { get; set; }               // ResearchProgress / CraftingProgress
    public bool? WillComplete { get; set; }           // ResearchProgress / CraftingProgress
    public int? PointsAwarded { get; set; }           // ResearchProgress (only if WillComplete)
}
