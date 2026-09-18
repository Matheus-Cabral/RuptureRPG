using FluentAssertions;
using Ruptura.Infrastructure.Services;

namespace Ruptura.UnitTests.Application;

public class TemporaryPasswordGeneratorTests
{
    [Fact]
    public void Generate_AlwaysSatisfiesThePasswordPolicy()
    {
        for (var i = 0; i < 500; i++)
        {
            var password = TemporaryPasswordGenerator.Generate();

            password.Length.Should().Be(12);
            password.Should().MatchRegex("[A-Z]").And.MatchRegex("[a-z]").And.MatchRegex("[0-9]");
        }
    }

    [Fact]
    public void Generate_NeverUsesAmbiguousCharacters()
    {
        // The GM reads/copies this password to the player, so look-alikes are excluded.
        for (var i = 0; i < 500; i++)
            TemporaryPasswordGenerator.Generate().Should().NotMatchRegex("[0O1lI]");
    }

    [Fact]
    public void Generate_ReturnsDifferentPasswordsEachTime()
    {
        var passwords = Enumerable.Range(0, 100).Select(_ => TemporaryPasswordGenerator.Generate());

        passwords.Distinct().Should().HaveCount(100);
    }
}
