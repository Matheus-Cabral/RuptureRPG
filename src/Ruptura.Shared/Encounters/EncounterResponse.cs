namespace Ruptura.Shared.Encounters;

// Server-authoritative encounter read model: echoes the GM's inputs and carries the
// computed GDD §9.8/§9.9 threat values. The client's PE/R are never trusted — these
// are always recomputed by the service.
public class EncounterResponse
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Echoed inputs.
    public string Intelligence { get; set; } = string.Empty;
    public string Terrain { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public bool ApplyPressure { get; set; }
    public int? Floor { get; set; }
    public int? PartyNpOverride { get; set; }
    public int? PartySizeOverride { get; set; }
    public string DesiredDifficulty { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;

    // Resolved creature lines (NP + name pulled from the bestiary at read time).
    public List<EncounterCreatureResolved> Creatures { get; set; } = [];

    // Computed threat values.
    public int Pg { get; set; }
    public int Pe { get; set; }
    public decimal R { get; set; }
    public string RLabel { get; set; } = string.Empty;
    public int Oa { get; set; }
    public decimal Fce { get; set; }
    public decimal RealStatMultiplier { get; set; }

    public bool PressureApplied { get; set; }
    public int PressureValue { get; set; }

    // False when there is no alive party and no PG override — R/Pg cannot be resolved.
    public bool PartyResolved { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// A creature line resolved against the bestiary. A referenced creature that no longer
// exists contributes Np = 0 and is surfaced with an empty/placeholder name.
public class EncounterCreatureResolved
{
    public Guid CreatureId { get; set; }
    public string CreatureName { get; set; } = string.Empty;
    public int Np { get; set; }
    public int Quantity { get; set; }
}
