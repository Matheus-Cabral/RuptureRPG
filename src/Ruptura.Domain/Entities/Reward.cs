namespace Ruptura.Domain.Entities;

// A persisted reward package scoped to one Campaign. The GM's payout inputs
// (currencies, materials, strategic assets, knowledge, items, and an optional link
// to the encounter/floor that earned it) live in the typed DataJson blob — see
// Ruptura.Shared.Rewards.RewardData.
public class Reward
{
    public Guid Id { get; set; }

    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    // Typed RewardData blob — see Ruptura.Shared.Rewards.RewardData.
    public string DataJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
