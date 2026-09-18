using FluentValidation;
using Ruptura.Shared.Campaigns;

namespace Ruptura.Application.Validators.Campaigns;

public class RemoveMemberRequestValidator : AbstractValidator<RemoveMemberRequest>
{
    public RemoveMemberRequestValidator()
    {
        RuleFor(x => x.Password).NotEmpty();
    }
}
