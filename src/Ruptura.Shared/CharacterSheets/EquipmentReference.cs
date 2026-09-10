namespace Ruptura.Shared.CharacterSheets;

// GDD §6.7.6 — Golpes de Desgaste até precisar manutenção, by Raridade (FECHADO). The
// repair-days table is NOT from the GDD (which only says "tempo curto") — it's derived from
// §6.7.4's Criação-days table: max(1, ceil(CreationDays/4)). Divino has no fixed Criação days
// (requires a prior Pesquisa project) so it falls back to its own Golpes de Desgaste ceiling.
// See design spec §3 for the full derivation.
public static class EquipmentReference
{
    public static readonly IReadOnlyDictionary<string, int> MaxDurabilityByRarity =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Comum"] = 3, ["Incomum"] = 4, ["Raro"] = 5, ["Épico"] = 6, ["Lendário"] = 8, ["Divino"] = 10
        };

    public static readonly IReadOnlyDictionary<string, int> RepairDaysByRarity =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Comum"] = 1, ["Incomum"] = 1, ["Raro"] = 2, ["Épico"] = 4, ["Lendário"] = 8, ["Divino"] = 10
        };

    // Raridade is free text (GM-typed via the catalog form) — trims and looks up
    // case-insensitively, matching CharacterStatsCalculator.CategoryIs' established reasoning
    // for free-text catalog fields. An unrecognized value resolves to 0 (never "damaged" —
    // there's no known ceiling to compare DurabilityRemaining against).
    public static int MaxDurabilityFor(string rarity) =>
        MaxDurabilityByRarity.GetValueOrDefault(rarity.Trim(), 0);

    public static int RepairDaysFor(string rarity) =>
        RepairDaysByRarity.GetValueOrDefault(rarity.Trim(), 0);
}
