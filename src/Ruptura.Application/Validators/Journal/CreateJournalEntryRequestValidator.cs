using FluentValidation;
using Ruptura.Shared.Journal;

namespace Ruptura.Application.Validators.Journal;

public class CreateJournalEntryRequestValidator : AbstractValidator<CreateJournalEntryRequest>
{
    public CreateJournalEntryRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(10000);
    }
}
