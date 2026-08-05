namespace Ruptura.Shared.CharacterSheets;

// Everything CharacterStatsCalculator computes — never persisted, always recomputed on read.
public class CharacterDerivedStats
{
    public Dictionary<string, int> AttributeModifiers { get; set; } = [];    // key: CharacterAttributes property name
    public Dictionary<string, int> AttributeGradeBonuses { get; set; } = []; // key: CharacterAttributes property name
    public int MaxHp { get; set; }
    public int Movement { get; set; }
    public int Initiative { get; set; }
    public int PassiveDefense { get; set; }
    public int DamageReduction { get; set; }
    public int CarryCapacity { get; set; }
    public decimal CurrentWeight { get; set; }
    public int Np { get; set; }
    public Dictionary<Guid, int> SkillGradeBonuses { get; set; } = []; // key: Skills[].CatalogEntryId
    public List<WeaponCombatRow> Weapons { get; set; } = [];
}

public class WeaponCombatRow
{
    public Guid CatalogEntryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AttackBonus { get; set; }
    public string DamageFormula { get; set; } = string.Empty; // e.g. "1d8 +3" — the dice itself is rolled at the table
}
