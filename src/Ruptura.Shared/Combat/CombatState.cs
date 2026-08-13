namespace Ruptura.Shared.Combat;

// The live in-session combat tracker state, persisted as the CombatSession.DataJson blob.
// All enum-like fields are strings keyed against CombatReference (kept string-only so
// Ruptura.Shared carries no project references).
public class CombatState
{
    // Combat rounds are 1-based.
    public int Round { get; set; } = 1;

    // Index into Combatants (in initiative order) of whose turn it is.
    public int CurrentIndex { get; set; }

    public List<Combatant> Combatants { get; set; } = [];
}

// One participant in the combat tracker. Kind is a string keyed against
// CombatReference.Kinds ("Character" | "Creature" | "Adhoc"). SourceId links back to the
// originating CharacterSheet/Creature when applicable; null for ad-hoc combatants.
public class Combatant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public Guid? SourceId { get; set; }
    public int Initiative { get; set; }
    public int Percepcao { get; set; }
    public int MaxPv { get; set; }
    public int CurrentPv { get; set; }
    public List<string> Conditions { get; set; } = [];
    public string Notes { get; set; } = string.Empty;
    public bool IsDefeated { get; set; }
}
