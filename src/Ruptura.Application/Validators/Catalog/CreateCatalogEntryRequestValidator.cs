using System.Text.Json;
using FluentValidation;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Catalog;

namespace Ruptura.Application.Validators.Catalog;

public class CreateCatalogEntryRequestValidator : AbstractValidator<CreateCatalogEntryRequest>
{
    public CreateCatalogEntryRequestValidator()
    {
        RuleFor(x => x.CampaignId).NotEmpty();

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => Enum.TryParse<CatalogEntryType>(t, out _))
            .WithMessage("Invalid catalog entry type.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.DataJson)
            .NotEmpty()
            .Must(BeValidJson)
            .WithMessage("DataJson must be valid JSON.");
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
