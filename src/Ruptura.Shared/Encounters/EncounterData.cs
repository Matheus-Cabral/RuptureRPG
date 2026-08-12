namespace Ruptura.Shared.Encounters;

// The GM's encounter inputs, persisted as the Encounter.DataJson blob. All enum-like
// fields are strings keyed against EncounterReference (kept string-only so
// Ruptura.Shared carries no project references).
public class EncounterData
{
    public List<EncounterCreature> Creatures { get; set; } = [];

    // Keys from EncounterReference.Intelligences / .Terrains / .Objectives.
    public string Intelligence { get; set; } = string.Empty;
    public string Terrain { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;

    // Apply the campaign's current Pressão PE multiplier (see DungeonPressure).
    public bool ApplyPressure { get; set; }

    public int? Floor { get; set; }

    // Manual PG overrides — bypass the campaign's alive party when set.
    public int? PartyNpOverride { get; set; }
    public int? PartySizeOverride { get; set; }

    // Keys from EncounterReference.Difficulties / .Durations.
    public string DesiredDifficulty { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
}

// One creature line: a bestiary creature reference and how many appear.
public class EncounterCreature
{
    public Guid CreatureId { get; set; }
    public int Quantity { get; set; }
}
