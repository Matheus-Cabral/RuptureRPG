namespace Ruptura.Shared.Rewards;

// Server-authoritative reward read model: echoes the GM's reward package and resolves
// the linked encounter's name (when RewardData.EncounterId is set) at read time.
public class RewardResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public RewardData Data { get; set; } = new();

    // Resolved from RewardData.EncounterId at read time; null when unset or unresolved.
    public string? EncounterName { get; set; }
}
