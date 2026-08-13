namespace Ruptura.Shared.Rewards;

// The GM's reward-package inputs, persisted as the Reward.DataJson blob. All enum-like
// fields are strings keyed against RewardReference (kept string-only so
// Ruptura.Shared carries no project references).
public class RewardData
{
    // Currencies.
    public int Silver { get; set; }
    public int PactCoins { get; set; }
    public int Fragments { get; set; }
    public int Cristais { get; set; }

    public List<RewardMaterial> Materials { get; set; } = [];

    public List<RewardAsset> StrategicAssets { get; set; } = [];

    public List<string> Knowledge { get; set; } = [];

    public List<string> Items { get; set; } = [];

    public string Notes { get; set; } = string.Empty;

    // Optional link to the encounter/floor that earned this package.
    public Guid? EncounterId { get; set; }
    public int? Floor { get; set; }

    public bool IsGranted { get; set; }
}

// One material line: a named material and how many were awarded.
public class RewardMaterial
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

// A strategic asset award. Category is keyed against RewardReference.Categories.
public class RewardAsset
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Ve { get; set; }
    public string Notes { get; set; } = string.Empty;
}
