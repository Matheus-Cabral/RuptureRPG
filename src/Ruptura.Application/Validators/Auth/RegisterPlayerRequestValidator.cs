using FluentValidation;
using Ruptura.Shared.Auth;

namespace Ruptura.Application.Validators.Auth;

public class RegisterPlayerRequestValidator : AbstractValidator<RegisterPlayerRequest>
{
    public RegisterPlayerRequestValidator()
    {
        Include(new RegisterRequestValidator());

        RuleFor(x => x.InviteCode)
            .NotEmpty()
            .MaximumLength(20);
    }
}
