using FluentValidation;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Application.Validators.CharacterSheets;

public class GrantCharacterSheetRequestValidator : AbstractValidator<GrantCharacterSheetRequest>
{
    public GrantCharacterSheetRequestValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.CharacterName).NotEmpty().MinimumLength(2).MaximumLength(100);
    }
}
