using FluentAssertions;
using Ruptura.Shared.Catalog;
using Xunit;

namespace Ruptura.UnitTests.Catalog;

public class CatalogEntryDataTests
{
    [Fact]
    public void Parse_NullOrBlankOrInvalid_YieldsEmptyObject()
    {
        CatalogEntryData.Parse(null).ToJson().Should().Be("{}");
        CatalogEntryData.Parse("   ").ToJson().Should().Be("{}");
        CatalogEntryData.Parse("not json").ToJson().Should().Be("{}");
        CatalogEntryData.Parse("[1,2,3]").ToJson().Should().Be("{}"); // non-object
    }

    [Fact]
    public void SetString_TrimsAndStores_RemovesWhenBlank()
    {
        var d = CatalogEntryData.Parse("{}");
        d.SetString("PrimarySkill", "  Espadas  ");
        d.GetString("PrimarySkill").Should().Be("Espadas");
        d.SetString("PrimarySkill", "   ");
        d.GetString("PrimarySkill").Should().Be("");        // removed
        d.ToJson().Should().Be("{}");
    }

    [Fact]
    public void SetNumber_And_SetBool_SerializeAsJsonPrimitives()
    {
        var d = CatalogEntryData.Parse("{}");
        d.SetNumber("Weight", 3);
        d.SetBool("NonConstructible", true);
        d.GetNumber("Weight").Should().Be(3);
        d.GetBool("NonConstructible").Should().BeTrue();
        d.ToJson().Should().Contain("\"Weight\":3").And.Contain("\"NonConstructible\":true"); // not "3"/"true" strings
    }

    [Fact]
    public void SchemaEdits_PreserveUnknownHomebrewKeys()
    {
        var d = CatalogEntryData.Parse("{\"MainBenefit\":\"old\",\"HomebrewExtra\":\"keep me\"}");
        d.SetString("MainBenefit", "new");
        var json = d.ToJson();
        json.Should().Contain("\"MainBenefit\":\"new\"");
        json.Should().Contain("\"HomebrewExtra\":\"keep me\"");   // untouched
    }

    [Fact]
    public void TryParse_RejectsInvalidOrNonObject()
    {
        CatalogEntryData.TryParse("{ bad", out _).Should().BeFalse();
        CatalogEntryData.TryParse("42", out _).Should().BeFalse();
        CatalogEntryData.TryParse("{\"a\":1}", out var ok).Should().BeTrue();
        ok.GetNumber("a").Should().Be(1);
    }

    [Fact]
    public void FieldsFor_KnownType_ReturnsExactKeys_UnknownType_Empty()
    {
        CatalogSchema.FieldsFor("Origin").Select(f => f.Key)
            .Should().Equal("MainBenefit", "PrimarySkill", "SecondarySkill", "StartingEquipment", "NarrativeHook");
        CatalogSchema.FieldsFor("Nonexistent").Should().BeEmpty();
    }
}
