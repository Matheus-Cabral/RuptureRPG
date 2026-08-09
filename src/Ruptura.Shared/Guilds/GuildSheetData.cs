namespace Ruptura.Shared.Guilds;

// Stable, low-churn modules stored in GuildSheet.DataJson. High-churn lists
// (buildings, staff, research, crafting, expeditions) are child entities, not here.
public class GuildSheetData
{
    public GuildIdentity Identity { get; set; } = new();
    public GuildPrestige Prestige { get; set; } = new();
    public List<InfluenceRelation> Influence { get; set; } = [];
    public GuildResources Resources { get; set; } = new();
    public List<Guid> ActiveDoctrineIds { get; set; } = [];   // Doctrine catalog refs, <= derived limit
    public GuildKnowledge Knowledge { get; set; } = new();
    public List<LegacyFeat> Legado { get; set; } = [];
    public int FloorsConquered { get; set; }                  // drives Guild Stage (§10.8)
}

public class GuildIdentity
{
    public string EmblemImagePath { get; set; } = string.Empty;
    public string PatronDeity { get; set; } = string.Empty;
    public Guid? MainDoctrineId { get; set; }                 // Doctrine catalog ref
    public DateTime? FoundingDate { get; set; }
    // 8 GDD ranks: "Bronze"|"Ferro"|"Aço"|"Prata"|"Ouro"|"Mithril"|"Adamante"|"Lendário".
    public string GuildRanking { get; set; } = "Bronze";
}

public class GuildPrestige
{
    public int Value { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class InfluenceRelation
{
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;         // Cidade|Facção|Guilda|Divindade
    public int Reputation { get; set; }                      // -100..100
    public string Notes { get; set; } = string.Empty;
}

public class GuildResources
{
    public int Silver { get; set; }
    public int PactCoins { get; set; }
    public List<MaterialStock> Materials { get; set; } = [];
    public int DimensionalFragments { get; set; }
    public List<string> Artifacts { get; set; } = [];
    public string StrategicReserveNotes { get; set; } = string.Empty;
}

public class MaterialStock
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }             // inventory only — no longer feeds CG
    public int StrategicValue { get; set; }       // VE 0..5 — the CG Recursos contribution
}

public class GuildKnowledge
{
    public List<string> Maps { get; set; } = [];
    public List<string> Recipes { get; set; } = [];
    public List<string> CataloguedEnemies { get; set; } = [];
    public List<string> DefeatedBosses { get; set; } = [];
    public List<string> HistoricalRecords { get; set; } = [];
}

public class LegacyFeat
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PermanentBenefit { get; set; } = string.Empty;
}
