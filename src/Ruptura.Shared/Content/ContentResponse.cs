namespace Ruptura.Shared.Content;

// A resolved soft link: an entity id paired with its display name, resolved at read time.
public class LinkRef
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Server-authoritative arc read model: echoes the arc's name/order and typed prep data.
public class ArcResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public ArcData Data { get; set; } = new();
}

// Server-authoritative floor read model: echoes the floor's arc/number/name and typed
// prep data, plus the linked encounters/rewards resolved to names at read time.
public class FloorResponse
{
    public Guid Id { get; set; }
    public Guid ArcId { get; set; }
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public FloorData Data { get; set; } = new();

    // Resolved from FloorData.LinkedEncounterIds / .LinkedRewardIds at read time.
    public List<LinkRef> Encounters { get; set; } = [];
    public List<LinkRef> Rewards { get; set; } = [];
}

// Server-authoritative session log read model.
public class SessionLogResponse
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public SessionLogData Data { get; set; } = new();
}
