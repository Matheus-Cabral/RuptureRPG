using FluentValidation;
using Ruptura.Shared.Journal;

namespace Ruptura.Application.Validators.Journal;

public class UpdateJournalEntryRequestValidator : AbstractValidator<UpdateJournalEntryRequest>
{
    public UpdateJournalEntryRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(10000);
        RuleFor(x => x.ImagePaths).NotNull();
    }
}
