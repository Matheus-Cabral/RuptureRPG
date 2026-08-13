namespace Ruptura.Shared.Rewards;

// Single source of truth for the reward planner's fixed picker sets. Consumed by the
// write-side validators and the UI pickers. String-only to keep Ruptura.Shared free of
// project references.
public static class RewardReference
{
    // Strategic-asset categories — the fixed set validated on write and offered in the
    // picker.
    public static readonly IReadOnlyList<string> Categories =
    [
        "Infraestrutura", "Conhecimento", "Diplomacia", "Artefatos", "ControleTerritorial"
    ];
}
