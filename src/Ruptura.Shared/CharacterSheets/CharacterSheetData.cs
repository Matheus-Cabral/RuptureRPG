namespace Ruptura.Shared.CharacterSheets;

public class CharacterSheetData
{
    public CharacterIdentity Identity { get; set; } = new();
    public CharacterAttributes Attributes { get; set; } = new();
    public CharacterCombat Combat { get; set; } = new();
    public List<CharacterSkillEntry> Skills { get; set; } = [];
    public List<CharacterCatalogRefEntry> Talents { get; set; } = [];
    public List<CharacterCatalogRefEntry> Spells { get; set; } = [];
    public List<CharacterCatalogRefEntry> Techniques { get; set; } = [];
    public List<CharacterEquipmentEntry> Equipment { get; set; } = [];
    public CharacterCurrency Currency { get; set; } = new();
    public CharacterAttributeTrial? AttributeTrial { get; set; }
    public CharacterGuildRegistry GuildRegistry { get; set; } = new();
}

// Module 1: Identidade. Origin/Background/Lineage/Aptitude/InitialTalent are CatalogEntry
// references (Origin, Background, Lineage, Aptitude, Talent types respectively).
// PatronDisplayName is flavor text for the printed sheet's "Jogador/Patrono" field —
// CharacterSheet.OwnerId is always the real owner for authorization purposes.
public class CharacterIdentity
{
    public Guid? OriginId { get; set; }
    public Guid? BackgroundId { get; set; }
    public Guid? LineageId { get; set; }
    public List<Guid> AptitudeIds { get; set; } = []; // GDD: exactly 2, not enforced server-side in this slice
    public Guid? InitialTalentId { get; set; }
    public string PatronDisplayName { get; set; } = string.Empty;
}

// Module 2: Atributos. Base score 1-6 (GDD); grade/modifier are always calculated
// (CharacterStatsCalculator), never stored.
public class CharacterAttributes
{
    public int Corpo { get; set; } = 1;
    public int Controle { get; set; } = 1;
    public int Vigor { get; set; } = 1;
    public int Presenca { get; set; } = 1;
    public int Intelecto { get; set; } = 1;
    public int Percepcao { get; set; } = 1;
    public int Vontade { get; set; } = 1;
    public int Afinidade { get; set; } = 1;
}

// Module 3: Combate. Only what's NOT derivable (current HP, active conditions) — PV
// max, Defesa Passiva, Deslocamento, Iniciativa, and the weapon table are all calculated.
public class CharacterCombat
{
    public int CurrentHp { get; set; }
    public List<string> ActiveConditions { get; set; } = [];
}

// Module 4: Perícias. Points invested → grade calculated by CharacterStatsCalculator.
public class CharacterSkillEntry
{
    public Guid CatalogEntryId { get; set; }
    public int Points { get; set; }
}

// Modules 5-7: Talentos, Magias Conhecidas, Técnicas/Posturas — just a reference to the
// CatalogEntry (Talent/Spell/Technique type respectively); everything else about them
// (Effect, School, PaCost, ...) is looked up from the CatalogEntry's DataJson on read.
public class CharacterCatalogRefEntry
{
    public Guid CatalogEntryId { get; set; }
}

// Module 8: Equipamentos e Inventário.
// IsEquipped: only equipped items feed Combat derived stats (weapon table row; armor/shield
// DefenseBonus + ArmorDamageReduction into Defesa Passiva).
// LinkedSkillEntryId: which invested Skill (a CatalogEntryId also present in Skills[])
// governs this weapon's attack/damage — the catalog doesn't tie an item to a Skill, the
// player picks per equipped item. Null for non-weapons or unassigned weapons.
public class CharacterEquipmentEntry
{
    public Guid CatalogEntryId { get; set; }
    public int Quantity { get; set; } = 1;
    public int DurabilityRemaining { get; set; }
    public bool IsEquipped { get; set; }
    public Guid? LinkedSkillEntryId { get; set; }
}

public class CharacterCurrency
{
    public int Silver { get; set; }
    public int PactCoins { get; set; }
}

// Module 9: Provação de Atributo — manual entry, no campaign calendar in this slice.
public class CharacterAttributeTrial
{
    public string AttributeName { get; set; } = string.Empty;
    public string TargetGrade { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
}

// Module 10: Registro da Guilda. Ranking is one of the 8 GDD rank names below — stored as
// a plain string (not an enum) because two of them contain accented characters that aren't
// valid C# enum-member identifiers ("Aço", "Lendário"). Valid values:
// "Bronze" | "Ferro" | "Aço" | "Prata" | "Ouro" | "Mithril" | "Adamante" | "Lendário".
// State is free descriptive text (ativo/ferido/ausente/desaparecido, ...) with no mechanical
// effect — distinct from CharacterSheet.IsDead/IsRetired, which are the real columns that
// matter for the uniqueness rule.
public class CharacterGuildRegistry
{
    public string Ranking { get; set; } = "Bronze";
    public DateTime? JoinedDate { get; set; }
    public string State { get; set; } = string.Empty;
    public int Expeditions { get; set; }
    public int FloorsCleared { get; set; }
}
