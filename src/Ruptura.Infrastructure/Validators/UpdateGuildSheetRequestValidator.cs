using FluentValidation;
using Ruptura.Shared.Guilds;

namespace Ruptura.Infrastructure.Validators;

public class UpdateGuildSheetRequestValidator : AbstractValidator<UpdateGuildSheetRequest>
{
    public UpdateGuildSheetRequestValidator()
    {
        RuleFor(x => x.GuildName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DataJson).NotEmpty();
    }
}
