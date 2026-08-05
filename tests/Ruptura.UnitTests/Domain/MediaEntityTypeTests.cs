using FluentAssertions;
using Ruptura.Domain.Enums;

namespace Ruptura.UnitTests.Domain;

public class MediaEntityTypeTests
{
    [Theory]
    [InlineData("CharacterSheetPortrait", true)]
    [InlineData("JournalEntryImage", true)]
    [InlineData("SomethingElse", false)]
    [InlineData("99", false)]  // TryParse alone would accept this; IsDefined must reject it
    public void TryParseAndIsDefined_TogetherRejectUndefinedValues(string input, bool expectedValid)
    {
        var parsed = Enum.TryParse<MediaEntityType>(input, out var value) && Enum.IsDefined(value);
        parsed.Should().Be(expectedValid);
    }
}
