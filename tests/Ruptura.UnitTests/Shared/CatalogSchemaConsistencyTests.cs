using FluentAssertions;
using Ruptura.Shared.Catalog;

namespace Ruptura.UnitTests.Shared;

// Regression guard for the EquipmentItem incident (see CLAUDE.md "Catalog schema field-name
// contract"): CatalogSchema.FieldsFor(type) drives the GM Catalog admin form generically and
// its Keys are serialized straight into DataJson, but the actual mechanical consumer for a
// "typed" catalog type is its own Catalog*CatalogData class — an entirely separate file with
// no compiler link to the schema. If the two drift, the form silently produces JSON the
// consumer class can't read (defaults win, no error, no failing request) — this test would
// have caught the EquipmentItem mismatch (schema had Damage/Defense/Properties/Description;
// EquipmentItemCatalogData has Rarity/AttackBonus/DamageBonus/DefenseBonus/
// WeaponDiceCategory/ArmorDamageReduction) before it shipped.
public class CatalogSchemaConsistencyTests
{
    [Theory]
    [InlineData("Skill", typeof(SkillCatalogData))]
    [InlineData("Talent", typeof(TalentCatalogData))]
    [InlineData("EquipmentItem", typeof(EquipmentItemCatalogData))]
    public void FieldsFor_KeysExactlyMatchTheTypedConsumerClassProperties(string catalogType, Type dataType)
    {
        var schemaKeys = CatalogSchema.FieldsFor(catalogType).Select(f => f.Key).ToHashSet();
        var dataProperties = dataType.GetProperties().Select(p => p.Name).ToHashSet();

        schemaKeys.Should().BeEquivalentTo(dataProperties,
            because: $"every CatalogSchema field for '{catalogType}' must be serialized under the exact " +
                     $"property name {dataType.Name} deserializes into, or the GM form silently produces " +
                     "data the mechanical consumer can never read");
    }
}
