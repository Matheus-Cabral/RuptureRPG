using System.Text.Json;
using FluentValidation;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Application.Validators.CharacterSheets;

public class UpdateCharacterSheetRequestValidator : AbstractValidator<UpdateCharacterSheetRequest>
{
    public UpdateCharacterSheetRequestValidator()
    {
        RuleFor(x => x.CharacterName).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.DataJson).NotEmpty().Must(BeValidJson).WithMessage("DataJson must be valid JSON.");
        RuleFor(x => x.DataJson)
            .Must(DeserializesToCharacterSheetData)
            .WithMessage("DataJson must deserialize to a valid CharacterSheetData object.")
            .When(x => BeValidJson(x.DataJson));
    }

    private static bool BeValidJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    // Valid-JSON alone isn't enough — "[]", "123", or `{"Skills":null}` all parse fine but
    // either fail to deserialize into CharacterSheetData or deserialize with a null module
    // that later throws deep in CharacterStatsCalculator/CharacterSheetService on every
    // subsequent read. Reject those at save time instead.
    private static bool DeserializesToCharacterSheetData(string json)
    {
        try
        {
            var data = JsonSerializer.Deserialize<CharacterSheetData>(json);
            return data is not null
                && data.Skills is not null
                && data.Talents is not null
                && data.Spells is not null
                && data.Techniques is not null
                && data.Equipment is not null
                && data.Identity is not null
                && data.Attributes is not null
                && data.Combat is not null
                && data.Currency is not null
                && data.GuildRegistry is not null
                && data.GuildRegistry.Ranking is not null
                && data.GuildRegistry.State is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
