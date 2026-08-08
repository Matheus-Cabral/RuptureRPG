using FluentAssertions;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;
using Xunit;

namespace Ruptura.UnitTests.Guilds;

public class ResearchReferenceTests
{
    // GuildSheetService derives RequiredDays via ResearchReference.RequiredDays[complexity.ToString()]
    // and the UI pre-fills Points via DefaultPoints. A new ResearchComplexity without a matching dict
    // entry would throw KeyNotFoundException at runtime — this pins the enum↔dictionary invariant so it
    // fails the build's tests instead.
    [Fact]
    public void Every_ResearchComplexity_Has_A_RequiredDays_Entry()
    {
        foreach (var name in Enum.GetNames<ResearchComplexity>())
            ResearchReference.RequiredDays.Should().ContainKey(name);
    }

    [Fact]
    public void Every_ResearchComplexity_Has_A_DefaultPoints_Entry()
    {
        foreach (var name in Enum.GetNames<ResearchComplexity>())
            ResearchReference.DefaultPoints.Should().ContainKey(name);
    }
}
