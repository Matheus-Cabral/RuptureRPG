namespace Ruptura.Shared.Encounters;

// Single source of truth for the encounter generator's fixed picker sets and the GDD
// §9.8/§9.9 threat multiplier tables. Consumed by EncounterCalculator, the write-side
// validators, and the UI pickers. String-only to keep Ruptura.Shared free of project
// references. See design spec §3 (The Math).
public static class EncounterReference
{
    // ---- Picker string lists (the fixed sets validated on write) ----

    // §9.5.3 ambiguity resolved into a GM picker (decision #4).
    public static readonly IReadOnlyList<string> Intelligences =
    [
        "Instinto", "Tatico", "Militar", "Genial"
    ];

    public static readonly IReadOnlyList<string> Terrains =
    [
        "Neutro", "LevementeFavoravel", "Favoravel", "Extremo"
    ];

    public static readonly IReadOnlyList<string> Objectives =
    [
        "Eliminar", "Sobreviver", "Defender", "Resgatar", "MissaoCritica"
    ];

    public static readonly IReadOnlyList<string> Difficulties =
    [
        "Seguro", "Normal", "Perigoso", "Mortal", "Infernal", "Apocaliptico"
    ];

    public static readonly IReadOnlyList<string> Durations =
    [
        "Curto", "Normal", "Longo", "Extenso"
    ];

    // ---- PG side ----

    // Party synergy by alive party size (§9.8): 6+ caps at 1.5.
    public static decimal Synergy(int partySize) => partySize switch
    {
        <= 1 => 1.0m,
        2 => 1.1m,
        3 => 1.2m,
        4 => 1.3m,
        5 => 1.4m,
        _ => 1.5m,
    };

    // ---- PE side ----

    // Quantity multiplier by total creature count (§9.8 banded).
    public static decimal QuantityMult(int totalCount) => totalCount switch
    {
        <= 1 => 1.0m,
        <= 3 => 1.25m,
        <= 8 => 1.5m,
        <= 20 => 2.0m,
        _ => 3.0m,
    };

    // §9.5.3 Behavior → intelligence multiplier.
    public static readonly IReadOnlyDictionary<string, decimal> IntelligenceMult =
        new Dictionary<string, decimal>
        {
            ["Instinto"] = 1.0m,
            ["Tatico"] = 1.2m,
            ["Militar"] = 1.5m,
            ["Genial"] = 2.0m,
        };

    public static readonly IReadOnlyDictionary<string, decimal> TerrainMult =
        new Dictionary<string, decimal>
        {
            ["Neutro"] = 1.0m,
            ["LevementeFavoravel"] = 1.1m,
            ["Favoravel"] = 1.25m,
            ["Extremo"] = 1.5m,
        };

    public static readonly IReadOnlyDictionary<string, decimal> ObjectiveMult =
        new Dictionary<string, decimal>
        {
            ["Eliminar"] = 1.0m,
            ["Sobreviver"] = 1.25m,
            ["Defender"] = 1.5m,
            ["Resgatar"] = 1.5m,
            ["MissaoCritica"] = 2.0m,
        };

    // ---- OA side (§9.9) ----

    public static readonly IReadOnlyDictionary<string, decimal> DifficultyFactor =
        new Dictionary<string, decimal>
        {
            ["Seguro"] = 0.75m,
            ["Normal"] = 1.0m,
            ["Perigoso"] = 1.25m,
            ["Mortal"] = 1.5m,
            ["Infernal"] = 2.0m,
            ["Apocaliptico"] = 3.0m,
        };

    public static readonly IReadOnlyDictionary<string, int> DurationFactor =
        new Dictionary<string, int>
        {
            ["Curto"] = 1,
            ["Normal"] = 2,
            ["Longo"] = 3,
            ["Extenso"] = 4,
        };

    // ---- FCE advisory (§9.9 / §6.8) ----

    // FCE by the group's Ranking band. Keys are the two-tier band names used by the
    // party-ranking resolver; RealStatMultiplier = 1 + (R − 1) × FCE.
    public static readonly IReadOnlyDictionary<string, decimal> FceByRankingBand =
        new Dictionary<string, decimal>
        {
            ["BronzeFerro"] = 0.40m,
            ["AcoPrata"] = 0.25m,
            ["OuroMithril"] = 0.15m,
            ["AdamanteLendario"] = 0.10m,
        };

    // ---- R → difficulty label bands ----

    // Upper-bound thresholds for the difficulty label, ascending. Every band is
    // inclusive of its upper bound EXCEPT "Extremo", which is strictly R < 3 so that R
    // exactly 3.0 falls through to "PossivelMorte" (R ≥ 3). Exposed for the UI scale;
    // RLabelFor is the authoritative resolver. See design spec §3.
    public static readonly IReadOnlyList<(decimal UpperBound, string Label)> RLabelBands =
    [
        (0.5m, "MuitoFacil"),
        (0.85m, "Facil"),
        (1.15m, "Equilibrado"),
        (1.4m, "Dificil"),
        (1.75m, "MuitoDificil"),
        (3.0m, "Extremo"),          // 1.75 < R < 3 → Extremo
        (decimal.MaxValue, "PossivelMorte"),
    ];

    // Resolve the difficulty label for a computed R value. Authoritative source.
    public static string RLabelFor(decimal r) => r switch
    {
        <= 0.5m => "MuitoFacil",
        <= 0.85m => "Facil",
        <= 1.15m => "Equilibrado",
        <= 1.4m => "Dificil",
        <= 1.75m => "MuitoDificil",
        < 3.0m => "Extremo",
        _ => "PossivelMorte",
    };
}
