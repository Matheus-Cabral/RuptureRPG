namespace Ruptura.Domain.Entities;

public class Npc
{
    public Guid Id { get; set; }

    // null = official example (system-owned, read-only to every GM); otherwise the owning GM.
    public Guid? GameMasterId { get; set; }

    public string Name { get; set; } = string.Empty;

    // Typed NpcData blob — see Ruptura.Shared.Bestiary.NpcData.
    public string DataJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
