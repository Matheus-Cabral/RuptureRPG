namespace Ruptura.Shared.CharacterSheets;

// GDD §6.6.7 — requisitos formais por categoria de Técnica (FECHADO). Suprema additionally
// requires Academia Militar built in the campaign's Guild (§10.3.1) — that check lives in
// CharacterSheetService.ValidateTechniqueProjectStartAsync, not here (this table has no
// notion of Guild state).
public static class TechniqueReference
{
    public static readonly IReadOnlyList<string> Categories = ["Postura", "Tecnica", "Reacao", "Suprema"];

    public static readonly IReadOnlyDictionary<string, int> RequiredDaysByCategory = new Dictionary<string, int>
    {
        ["Postura"] = 5, ["Tecnica"] = 10, ["Reacao"] = 10, ["Suprema"] = 25
    };

    public static readonly IReadOnlyDictionary<string, int> MinSkillPointsByCategory = new Dictionary<string, int>
    {
        ["Postura"] = 25, ["Tecnica"] = 50, ["Reacao"] = 50, ["Suprema"] = 75
    };

    // Only Suprema has a minimum Ranking; null means "no requirement."
    public static readonly IReadOnlyDictionary<string, string?> MinRankingByCategory = new Dictionary<string, string?>
    {
        ["Postura"] = null, ["Tecnica"] = null, ["Reacao"] = null, ["Suprema"] = "Prata"
    };
}
