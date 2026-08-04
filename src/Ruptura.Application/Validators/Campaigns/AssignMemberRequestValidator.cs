using FluentValidation;
using Ruptura.Shared.Campaigns;

namespace Ruptura.Application.Validators.Campaigns;

public class AssignMemberRequestValidator : AbstractValidator<AssignMemberRequest>
{
    public AssignMemberRequestValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}
