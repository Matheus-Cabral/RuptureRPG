using Ruptura.Shared.Guilds;

namespace Ruptura.Shared.CharacterSheets;

// GDD §6.7.4 — Dias de Criação e Custo em Materiais by Raridade (FECHADO for those two
// columns). Divino is deliberately absent — the GDD itself states Divino "Requer projeto de
// Pesquisa prévio," a system this app doesn't have (and won't — the would-be sub-project for
// it was dropped as redundant). Installation mapping is NOT a verbatim GDD table — §6.7.4
// names "Oficina Básica/Ferraria/Ferraria Avançada/Forja Rúnica/Forja Divina," none of which
// are separate seeded installations; this is reconciled against the ALREADY-SEEDED
// installations' own §10.3.1 descriptions (see design spec §4): Ferraria's own description
// says "Comum até Raro em Nível I-II; Épico em III+", and Oficina de Runas's says
// "Crafting Épico+" — the only rune-themed installation in the 20-item list.
public static class CraftingReference
{
    public static readonly IReadOnlyList<string> CraftableRarities =
        ["Comum", "Incomum", "Raro", "Épico", "Lendário"];

    public static readonly IReadOnlyDictionary<string, int> RequiredDaysByRarity =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Comum"] = 1, ["Incomum"] = 3, ["Raro"] = 7, ["Épico"] = 14, ["Lendário"] = 30
        };

    public static readonly IReadOnlyDictionary<string, int> MaterialsCostByRarity =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Comum"] = 5, ["Incomum"] = 15, ["Raro"] = 35, ["Épico"] = 75, ["Lendário"] = 150
        };

    public static readonly IReadOnlyDictionary<string, (Guid InstallationId, int MinLevel)> InstallationByRarity =
        new Dictionary<string, (Guid, int)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Comum"] = (GuildCatalogIds.Oficina, 1),
            ["Incomum"] = (GuildCatalogIds.Oficina, 1),
            ["Raro"] = (GuildCatalogIds.Ferraria, 1),
            ["Épico"] = (GuildCatalogIds.Ferraria, 3),
            ["Lendário"] = (GuildCatalogIds.OficinaDeRunas, 1)
        };

    public static bool IsCraftable(string rarity) =>
        CraftableRarities.Any(r => string.Equals(r, rarity.Trim(), StringComparison.OrdinalIgnoreCase));
}
