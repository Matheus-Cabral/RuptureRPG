using System.Text.Json;
using FluentValidation;
using Ruptura.Shared.Catalog;

namespace Ruptura.Application.Validators.Catalog;

public class UpdateCatalogEntryRequestValidator : AbstractValidator<UpdateCatalogEntryRequest>
{
    public UpdateCatalogEntryRequestValidator()
    {
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
