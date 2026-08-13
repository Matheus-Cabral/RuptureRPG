namespace Ruptura.Domain.Entities;

// A persisted in-session combat tracker scoped to one Campaign. The live tracker state
// (round, current turn index, and the combatant roster with initiative/PV/conditions)
// lives in the typed DataJson blob — see Ruptura.Shared.Combat.CombatState. Pressure is
// derived server-side from the roster, never trusted from the client.
public class CombatSession
{
    public Guid Id { get; set; }

    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    // Typed CombatState blob — see Ruptura.Shared.Combat.CombatState.
    public string DataJson { get; set; } = "{}";

    // Only one session is the active tracker at a time (enforced by the service).
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
