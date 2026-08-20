namespace Ruptura.Shared.Catalog;

public enum CatalogFieldKind { Text, TextArea, Number, Bool }

public record CatalogField(string Key, string LabelKey, CatalogFieldKind Kind);

// Per-type field schema — Keys are the exact JSON property names from CatalogSeedData.*
// (the consumer contract). LabelKey resolves to a Web resx string.
public static class CatalogSchema
{
    private static CatalogField F(string key, CatalogFieldKind kind) =>
        new(key, $"Gm.Catalog.Field.{key}", kind);

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<CatalogField>> ByType =
        new Dictionary<string, IReadOnlyList<CatalogField>>
        {
            ["Origin"] = [F("MainBenefit", CatalogFieldKind.TextArea), F("PrimarySkill", CatalogFieldKind.Text), F("SecondarySkill", CatalogFieldKind.Text), F("StartingEquipment", CatalogFieldKind.Text), F("NarrativeHook", CatalogFieldKind.TextArea)],
            ["Background"] = [F("TriggeringEvent", CatalogFieldKind.TextArea), F("Benefit", CatalogFieldKind.TextArea), F("Complication", CatalogFieldKind.TextArea)],
            ["Lineage"] = [F("RacialAdjustment", CatalogFieldKind.Text), F("RacialTrait", CatalogFieldKind.TextArea)],
            ["Aptitude"] = [F("CoveredAreas", CatalogFieldKind.TextArea)],
            ["Talent"] = [F("Category", CatalogFieldKind.Text), F("Effect", CatalogFieldKind.TextArea), F("PowerTier", CatalogFieldKind.Text)],
            ["Skill"] = [F("Area", CatalogFieldKind.Text), F("RelatedAttribute", CatalogFieldKind.Text)],
            ["Spell"] = [F("School", CatalogFieldKind.Text), F("ComplexityPaCost", CatalogFieldKind.Text), F("Range", CatalogFieldKind.Text), F("Area", CatalogFieldKind.Text), F("Duration", CatalogFieldKind.Text), F("Test", CatalogFieldKind.Text), F("Damage", CatalogFieldKind.Text), F("Effect", CatalogFieldKind.TextArea)],
            ["Technique"] = [F("Style", CatalogFieldKind.Text), F("Category", CatalogFieldKind.Text), F("PaCost", CatalogFieldKind.Text), F("Damage", CatalogFieldKind.Text), F("Effect", CatalogFieldKind.TextArea)],
            ["Installation"] = [F("Category", CatalogFieldKind.Text), F("Weight", CatalogFieldKind.Number), F("LevelCap", CatalogFieldKind.Number), F("Prerequisites", CatalogFieldKind.TextArea), F("Unlocks", CatalogFieldKind.TextArea), F("NonConstructible", CatalogFieldKind.Bool)],
            ["Doctrine"] = [F("Bonus", CatalogFieldKind.TextArea)],
            ["EquipmentItem"] = [F("Category", CatalogFieldKind.Text), F("Damage", CatalogFieldKind.Text), F("Defense", CatalogFieldKind.Text), F("Weight", CatalogFieldKind.Number), F("Properties", CatalogFieldKind.TextArea), F("Description", CatalogFieldKind.TextArea)],
        };

    public static IReadOnlyList<CatalogField> FieldsFor(string type) =>
        ByType.TryGetValue(type ?? string.Empty, out var fields) ? fields : [];
}
