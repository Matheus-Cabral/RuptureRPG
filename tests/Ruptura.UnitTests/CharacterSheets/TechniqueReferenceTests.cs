using FluentAssertions;
using Ruptura.Shared.CharacterSheets;
using Xunit;

namespace Ruptura.UnitTests.CharacterSheets;

// Mirrors Ruptura.UnitTests.Guilds.ResearchReferenceTests — TechniqueReference is a static
// lookup-table class (like ResearchReference), consumed by
// CharacterSheetService.ValidateTechniqueProjectStartAsync via direct dictionary indexing
// (RequiredDaysByCategory[category], MinSkillPointsByCategory[category]). A future category
// added to Categories without a matching entry in either dictionary would 500 with a
// KeyNotFoundException instead of being caught here.
public class TechniqueReferenceTests
{
    [Fact]
    public void Every_Category_Has_A_RequiredDays_Entry()
    {
        foreach (var category in TechniqueReference.Categories)
            TechniqueReference.RequiredDaysByCategory.Should().ContainKey(category);
    }

    [Fact]
    public void Every_Category_Has_A_MinSkillPoints_Entry()
    {
        foreach (var category in TechniqueReference.Categories)
            TechniqueReference.MinSkillPointsByCategory.Should().ContainKey(category);
    }

    [Fact]
    public void Every_Category_Has_A_MinRanking_Entry_EvenIfNull()
    {
        foreach (var category in TechniqueReference.Categories)
            TechniqueReference.MinRankingByCategory.Should().ContainKey(category);
    }
}
