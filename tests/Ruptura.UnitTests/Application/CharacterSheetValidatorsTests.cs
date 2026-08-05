using FluentAssertions;
using Ruptura.Application.Validators.CharacterSheets;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.UnitTests.Application;

public class CharacterSheetValidatorsTests
{
    private readonly GrantCharacterSheetRequestValidator _grantValidator = new();
    private readonly UpdateCharacterSheetRequestValidator _updateValidator = new();

    [Fact]
    public void GrantValidator_WithEmptyPlayerId_Fails()
    {
        var result = _grantValidator.Validate(new GrantCharacterSheetRequest { PlayerId = Guid.Empty, CharacterName = "Aldric" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GrantValidator_WithTooShortName_Fails()
    {
        var result = _grantValidator.Validate(new GrantCharacterSheetRequest { PlayerId = Guid.NewGuid(), CharacterName = "A" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GrantValidator_WithValidData_Succeeds()
    {
        var result = _grantValidator.Validate(new GrantCharacterSheetRequest { PlayerId = Guid.NewGuid(), CharacterName = "Aldric" });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateValidator_WithInvalidJson_Fails()
    {
        var result = _updateValidator.Validate(new UpdateCharacterSheetRequest { CharacterName = "Aldric", DataJson = "not json" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateValidator_WithValidData_Succeeds()
    {
        var result = _updateValidator.Validate(new UpdateCharacterSheetRequest { CharacterName = "Aldric", DataJson = "{}" });
        result.IsValid.Should().BeTrue();
    }

    // ── Finding 1b: valid-JSON-but-wrong-shape DataJson must be rejected at save time ──

    [Theory]
    [InlineData("[]")]
    [InlineData("123")]
    [InlineData("""{"Skills":null}""")]
    [InlineData("""{"GuildRegistry":{"Ranking":null}}""")]
    public void UpdateValidator_WithJsonThatDoesNotDeserializeToValidCharacterSheetData_Fails(string dataJson)
    {
        var result = _updateValidator.Validate(new UpdateCharacterSheetRequest { CharacterName = "Aldric", DataJson = dataJson });

        result.IsValid.Should().BeFalse();
    }
}
