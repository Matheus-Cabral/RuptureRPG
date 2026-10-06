using FluentAssertions;
using Ruptura.Web.Shared;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class SearchableSelectFilterTests
{
    private static readonly SearchableOption<int>[] Options =
    [
        new(1, "Magia Arcana", "Known"),
        new(2, "Furtividade", "Known"),
        new(3, "Mágica de Sangue", "Other"),
        new(4, "Percepção", "Other")
    ];

    [Fact]
    public void Filter_ReturnsEveryOptionInOrder_WhenQueryIsNullOrWhitespace()
    {
        SearchableSelectFilter.Filter(Options, null).Should().Equal(Options);
        SearchableSelectFilter.Filter(Options, "   ").Should().Equal(Options);
    }

    [Fact]
    public void Filter_MatchesAnywhereInTheLabel_CaseInsensitive()
    {
        SearchableSelectFilter.Filter(Options, "ARCANA").Select(o => o.Value).Should().Equal(1);
        SearchableSelectFilter.Filter(Options, "tivi").Select(o => o.Value).Should().Equal(2);
    }

    [Fact]
    public void Filter_IgnoresAccents_InBothQueryAndLabel()
    {
        SearchableSelectFilter.Filter(Options, "magi").Select(o => o.Value).Should().Equal(1, 3);
        SearchableSelectFilter.Filter(Options, "percepcao").Select(o => o.Value).Should().Equal(4);
        SearchableSelectFilter.Filter(Options, "MÁGIA").Select(o => o.Value).Should().Equal(1);
    }

    [Fact]
    public void Filter_TrimsTheQuery()
    {
        SearchableSelectFilter.Filter(Options, "  furt ").Select(o => o.Value).Should().Equal(2);
    }

    [Fact]
    public void Filter_ReturnsEmpty_WhenNothingMatches()
    {
        SearchableSelectFilter.Filter(Options, "dragão").Should().BeEmpty();
    }

    [Fact]
    public void Filter_KeepsEachMatchsGroup()
    {
        SearchableSelectFilter.Filter(Options, "ma").Select(o => o.Group).Should().Equal("Known", "Other");
    }
}
