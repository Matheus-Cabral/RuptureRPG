using System.Text.Json;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Catalog;

namespace Ruptura.Application.Services;

public class CharacterStatsCalculator : ICharacterStatsCalculator
{
    private static readonly Dictionary<string, int> RankingHpBonus = new()
    {
        ["Bronze"] = 0, ["Ferro"] = 5, ["Aço"] = 10, ["Prata"] = 15,
        ["Ouro"] = 20, ["Mithril"] = 25, ["Adamante"] = 30, ["Lendário"] = 35
    };

    private static readonly Dictionary<string, string> WeaponDiceByCategory = new()
    {
        ["Leve"] = "1d6", ["Média"] = "1d8", ["Pesada"] = "1d10", ["DuasMãos"] = "2d6"
    };

    private static readonly Dictionary<string, int> TalentNpWeight = new()
    {
        ["menor"] = 1, ["médio"] = 3, ["maior"] = 5
    };

    private static readonly Dictionary<string, int> EquipmentNpWeight = new()
    {
        ["Comum"] = 1, ["Incomum"] = 3, ["Raro"] = 7, ["Épico"] = 15, ["Lendário"] = 30, ["Divino"] = 50
    };

    public CharacterDerivedStats Calculate(
        CharacterSheetData data,
        IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries)
    {
        var attributeScores = GetAttributeScores(data.Attributes);
        var attributeModifiers = attributeScores.ToDictionary(kv => kv.Key, kv => kv.Value - 2);
        var attributeGradeBonuses = attributeScores.ToDictionary(kv => kv.Key, kv => kv.Value - 1);

        var skillGradeBonuses = data.Skills.ToDictionary(s => s.CatalogEntryId, s => SkillGradeBonus(s.Points));

        var rankingBonus = RankingHpBonus.GetValueOrDefault(data.GuildRegistry.Ranking, 0);
        var maxHp = 10 + attributeScores["Vigor"] * 2 + rankingBonus;
        var movement = 4 + attributeModifiers["Vigor"];
        var initiative = attributeModifiers["Controle"];

        var equipped = data.Equipment
            .Where(e => e.IsEquipped)
            .Select(e => (Entry: e, Data: DeserializeEquipment(e.CatalogEntryId, catalogEntries)))
            .Where(x => x.Data is not null)
            .ToList();

        var armorAndShieldDefense = equipped
            .Where(x => x.Data!.Category is "armadura" or "escudo")
            .Sum(x => x.Data!.DefenseBonus);
        var passiveDefense = 10 + attributeModifiers["Controle"] + armorAndShieldDefense;

        var damageReduction = equipped
            .Where(x => x.Data!.Category == "armadura")
            .Sum(x => x.Data!.ArmorDamageReduction ?? 0);

        var carryCapacity = attributeScores["Corpo"] * 5;
        var currentWeight = data.Equipment.Sum(e =>
            (DeserializeEquipment(e.CatalogEntryId, catalogEntries)?.Weight ?? 0) * e.Quantity);

        var weapons = equipped
            .Where(x => x.Data!.Category == "arma")
            .Select(x =>
            {
                var name = catalogEntries.TryGetValue(x.Entry.CatalogEntryId, out var itemEntry)
                    ? itemEntry.Name : string.Empty;
                return BuildWeaponRow(
                    x.Entry, x.Data!, name, attributeModifiers, attributeGradeBonuses,
                    skillGradeBonuses, catalogEntries);
            })
            .ToList();

        var np = attributeGradeBonuses.Values.Sum()
            + skillGradeBonuses.Values.Sum()
            + data.Talents.Sum(t => TalentNpWeightFor(t.CatalogEntryId, catalogEntries))
            + data.Equipment.Sum(e => EquipmentNpWeightFor(e.CatalogEntryId, catalogEntries));

        return new CharacterDerivedStats
        {
            AttributeModifiers = attributeModifiers,
            AttributeGradeBonuses = attributeGradeBonuses,
            MaxHp = maxHp,
            Movement = movement,
            Initiative = initiative,
            PassiveDefense = passiveDefense,
            DamageReduction = damageReduction,
            CarryCapacity = carryCapacity,
            CurrentWeight = currentWeight,
            Np = np,
            SkillGradeBonuses = skillGradeBonuses,
            Weapons = weapons
        };
    }

    private static WeaponCombatRow BuildWeaponRow(
        CharacterEquipmentEntry entry,
        EquipmentItemCatalogData eqData,
        string itemName,
        IReadOnlyDictionary<string, int> attributeModifiers,
        IReadOnlyDictionary<string, int> attributeGradeBonuses,
        IReadOnlyDictionary<Guid, int> skillGradeBonuses,
        IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries)
    {
        var skillGrade = 0;
        var attributeGrade = 0;
        var attributeModifier = 0;

        if (entry.LinkedSkillEntryId is { } skillId && catalogEntries.TryGetValue(skillId, out var skillEntry))
        {
            var skillData = JsonSerializer.Deserialize<SkillCatalogData>(skillEntry.DataJson);
            if (skillData is not null)
            {
                var attributeName = NormalizeAttributeName(skillData.RelatedAttribute);
                skillGrade = skillGradeBonuses.GetValueOrDefault(skillId);
                attributeGrade = attributeGradeBonuses.GetValueOrDefault(attributeName);
                attributeModifier = attributeModifiers.GetValueOrDefault(attributeName);
            }
        }

        var dice = eqData.WeaponDiceCategory is not null
            && WeaponDiceByCategory.TryGetValue(eqData.WeaponDiceCategory, out var d)
            ? d
            : "1d6";

        var damage = attributeModifier + skillGrade + eqData.DamageBonus;

        return new WeaponCombatRow
        {
            CatalogEntryId = entry.CatalogEntryId,
            Name = itemName,
            AttackBonus = attributeGrade + skillGrade,
            DamageFormula = $"{dice}{FormatModifier(damage)}"
        };
    }

    private static string FormatModifier(int value) => value switch
    {
        > 0 => $" +{value}",
        < 0 => $" {value}",
        _ => string.Empty
    };

    // Skill.RelatedAttribute values in the catalog are accented GDD names
    // ("Presença", "Percepção"); CharacterAttributes property names drop the accent
    // (C# identifiers can't contain "ç"/"ã") — this bridges the two.
    private static string NormalizeAttributeName(string raw) => raw switch
    {
        "Presença" => "Presenca",
        "Percepção" => "Percepcao",
        _ => raw
    };

    private static Dictionary<string, int> GetAttributeScores(CharacterAttributes attrs) => new()
    {
        ["Corpo"] = attrs.Corpo,
        ["Controle"] = attrs.Controle,
        ["Vigor"] = attrs.Vigor,
        ["Presenca"] = attrs.Presenca,
        ["Intelecto"] = attrs.Intelecto,
        ["Percepcao"] = attrs.Percepcao,
        ["Vontade"] = attrs.Vontade,
        ["Afinidade"] = attrs.Afinidade
    };

    private static int SkillGradeBonus(int points) => points switch
    {
        >= 100 => 4,
        >= 75 => 3,
        >= 50 => 2,
        >= 25 => 1,
        >= 10 => 0,
        _ => -2
    };

    private static EquipmentItemCatalogData? DeserializeEquipment(
        Guid id, IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries) =>
        catalogEntries.TryGetValue(id, out var entry)
            ? JsonSerializer.Deserialize<EquipmentItemCatalogData>(entry.DataJson)
            : null;

    private static int TalentNpWeightFor(Guid id, IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries)
    {
        if (!catalogEntries.TryGetValue(id, out var entry)) return 0;
        var data = JsonSerializer.Deserialize<TalentCatalogData>(entry.DataJson);
        return data is null ? 0 : TalentNpWeight.GetValueOrDefault(data.PowerTier, 0);
    }

    private static int EquipmentNpWeightFor(Guid id, IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries)
    {
        var data = DeserializeEquipment(id, catalogEntries);
        return data is null ? 0 : EquipmentNpWeight.GetValueOrDefault(data.Rarity, 0);
    }
}
