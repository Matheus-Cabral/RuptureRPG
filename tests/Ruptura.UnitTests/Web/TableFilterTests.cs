using FluentAssertions;
using Ruptura.Web.Shared;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class TableFilterTests
{
    [Fact]
    public void Matches_ReturnsTrue_WhenTermIsNullOrWhitespace()
    {
        TableFilter.Matches(null, "Anything").Should().BeTrue();
        TableFilter.Matches("   ", "Anything").Should().BeTrue();
    }

    [Fact]
    public void Matches_ReturnsTrue_WhenAnyFieldContainsTermCaseInsensitive()
    {
        TableFilter.Matches("gob", "Goblin", "A short humanoid").Should().BeTrue();
        TableFilter.Matches("HUMANOID", "Goblin", "A short humanoid").Should().BeTrue();
    }

    [Fact]
    public void Matches_ReturnsFalse_WhenNoFieldContainsTerm()
    {
        TableFilter.Matches("dragon", "Goblin", "A short humanoid").Should().BeFalse();
    }

    [Fact]
    public void Matches_IgnoresNullFields()
    {
        TableFilter.Matches("gob", null, "Goblin").Should().BeTrue();
    }

    [Fact]
    public void Matches_TrimsLeadingAndTrailingWhitespace_FromTerm()
    {
        TableFilter.Matches(" gob ", "Goblin").Should().BeTrue();
    }
}
