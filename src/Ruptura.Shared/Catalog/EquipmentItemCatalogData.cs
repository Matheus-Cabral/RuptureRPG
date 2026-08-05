namespace Ruptura.Shared.Catalog;

// EquipmentItem is deliberately unseeded (homebrew-only, see sub-plan #2 notes) — this
// shape matches the design spec §4.2.1 field list exactly, for when a GM creates one via
// the existing raw-DataJson Catalog admin page.
public class EquipmentItemCatalogData
{
    public string Category { get; set; } = string.Empty; // "arma" | "armadura" | "escudo" | "item"
    public string Rarity { get; set; } = string.Empty;    // Comum/Incomum/Raro/Épico/Lendário/Divino
    public int AttackBonus { get; set; }
    public int DamageBonus { get; set; }
    public int DefenseBonus { get; set; }
    public string? WeaponDiceCategory { get; set; }  // Leve/Média/Pesada/DuasMãos — set only if Category == "arma"
    public int? ArmorDamageReduction { get; set; }    // set only if Category == "armadura"
    public decimal Weight { get; set; }
}
