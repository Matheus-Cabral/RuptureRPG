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
}
