using FluentValidation;
using Ruptura.Shared.Campaigns;

namespace Ruptura.Application.Validators.Campaigns;

public class CreateCampaignRequestValidator : AbstractValidator<CreateCampaignRequest>
{
    public CreateCampaignRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
    }
}
