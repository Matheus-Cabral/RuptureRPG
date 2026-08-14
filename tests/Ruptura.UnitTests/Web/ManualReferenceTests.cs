using FluentAssertions;
using Ruptura.Web.Services;
using Xunit;

namespace Ruptura.UnitTests.Web;

public class ManualReferenceTests
{
    [Theory]
    [InlineData(ManualType.Player, "pt-BR", "Manual_do_Jogador.md")]
    [InlineData(ManualType.Player, "en", "Manual_do_Jogador.en.md")]
    [InlineData(ManualType.GameMaster, "pt-BR", "Manual_do_Mestre.md")]
    [InlineData(ManualType.GameMaster, "en", "Manual_do_Mestre.en.md")]
    public void FileNameFor_MapsTypeAndCultureToFileName(ManualType type, string culture, string expected)
    {
        ManualReference.FileNameFor(type, culture).Should().Be(expected);
    }
}
