namespace Ruptura.Shared.Bestiary;

// Typed blob stored in Npc.DataJson. Simple non-combat record.
public class NpcData
{
    public string Role { get; set; } = string.Empty;         // BestiaryReference.Roles (custom allowed)
    public string Faction { get; set; } = string.Empty;
    public string Disposition { get; set; } = string.Empty;  // BestiaryReference.Dispositions (Aliado|Neutro|Hostil)
    public string Location { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
