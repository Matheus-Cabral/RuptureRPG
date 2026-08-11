namespace Ruptura.Shared.Bestiary;

// Single source of truth for the bestiary's fixed picker sets and the NP-weight maps
// (§9.5.2 Function, §9.5.3 Behavior, §9.5.4 Characteristic weights, §9.5.5 NP terms,
// §9.5.6 Categories). Consumed by the NP calculator, the write-side validators, and the
// UI pickers. String-only to keep Ruptura.Shared free of project references.
public static class BestiaryReference
{
    // §9.5.2 — the 5 official functions; custom/homebrew values are allowed on write.
    public static readonly IReadOnlyList<string> Functions =
    [
        "Predador", "Guardião", "Soldado", "Parasita", "Evento Vivo"
    ];

    // §9.5.3 — fixed set; drives the GM-2 encounter intelligence multiplier.
    public static readonly IReadOnlyList<string> Behaviors =
    [
        "Instintiva", "Inteligente", "Estrategica"
    ];

    // §9.5.6 — the 8 categories, low → high.
    public static readonly IReadOnlyList<string> Categories =
    [
        "Fraca", "Comum", "Veterana", "Elite",
        "Campea", "ChefeMenor", "ChefeDeArco", "EntidadeSuperior"
    ];

    // §9.5.4 — characteristic weights.
    public static readonly IReadOnlyList<string> Weights =
    [
        "Menor", "Media", "Maior", "Suprema"
    ];

    // §9.5.5 — ability tiers (Habilidade comum/avançada/suprema).
    public static readonly IReadOnlyList<string> Tiers =
    [
        "Comum", "Avancada", "Suprema"
    ];

    // Equipment rarities.
    public static readonly IReadOnlyList<string> Rarities =
    [
        "Comum", "Incomum", "Raro", "Epico", "Lendario", "Divino"
    ];

    // NPC roles — the official set; custom values are allowed on write.
    public static readonly IReadOnlyList<string> Roles =
    [
        "Patrono", "Contato", "LiderDeFaccao", "Comerciante", "QuestGiver"
    ];

    // NPC dispositions — fixed set.
    public static readonly IReadOnlyList<string> Dispositions =
    [
        "Aliado", "Neutro", "Hostil"
    ];

    // Characteristic weight → NP contribution (§9.5.4: Menor 1 / Media 3 / Maior 5 / Suprema 10).
    public static readonly IReadOnlyDictionary<string, int> WeightValues = new Dictionary<string, int>
    {
        ["Menor"] = 1,
        ["Media"] = 3,
        ["Maior"] = 5,
        ["Suprema"] = 10,
    };

    // Ability tier → NP contribution (§9.5.5: Comum 5 / Avancada 10 / Suprema 20).
    public static readonly IReadOnlyDictionary<string, int> TierValues = new Dictionary<string, int>
    {
        ["Comum"] = 5,
        ["Avancada"] = 10,
        ["Suprema"] = 20,
    };

    // Equipment rarity → NP contribution (Comum 1 / Incomum 3 / Raro 7 / Epico 15 / Lendario 30 / Divino 50).
    public static readonly IReadOnlyDictionary<string, int> RarityValues = new Dictionary<string, int>
    {
        ["Comum"] = 1,
        ["Incomum"] = 3,
        ["Raro"] = 7,
        ["Epico"] = 15,
        ["Lendario"] = 30,
        ["Divino"] = 50,
    };

    // §9.5.6 — Category → advisory NP range [NpMin, NpMax]. The two open-ended top tiers
    // (Chefe de Arco 430–550+, Entidade Superior 550+) use int.MaxValue as the upper bound.
    public static readonly IReadOnlyDictionary<string, (int NpMin, int NpMax)> CategoryRanges =
        new Dictionary<string, (int NpMin, int NpMax)>
        {
            ["Fraca"] = (20, 40),
            ["Comum"] = (40, 70),
            ["Veterana"] = (70, 105),
            ["Elite"] = (105, 195),
            ["Campea"] = (195, 340),
            ["ChefeMenor"] = (340, 430),
            ["ChefeDeArco"] = (430, int.MaxValue),
            ["EntidadeSuperior"] = (550, int.MaxValue),
        };
}
