namespace Ruptura.Domain.Entities;

// A persisted encounter scoped to one Campaign. The GDD §9.8/§9.9 threat inputs
// (creatures×quantity, terrain/objective/intelligence, difficulty/duration, pressure
// toggle, party overrides) live in the typed DataJson blob — see
// Ruptura.Shared.Encounters.EncounterData. All threat math is computed server-side.
public class Encounter
{
    public Guid Id { get; set; }

    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    // Typed EncounterData blob — see Ruptura.Shared.Encounters.EncounterData.
    public string DataJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
