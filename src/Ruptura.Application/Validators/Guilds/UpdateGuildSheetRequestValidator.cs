using System.Text.Json;
using FluentValidation;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Validators.Guilds;

public class UpdateGuildSheetRequestValidator : AbstractValidator<UpdateGuildSheetRequest>
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public UpdateGuildSheetRequestValidator()
    {
        RuleFor(x => x.GuildName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DataJson)
            .NotEmpty()
            .Must(BeValidGuildSheetData)
            .WithMessage("DataJson must deserialize to a valid GuildSheetData object.");
    }

    // Valid-JSON alone isn't enough — "garbage", "[]", "123", and "null" either fail to
    // deserialize into GuildSheetData or produce null modules / null list elements that later
    // throw deep in GuildStatsCalculator/GuildSheetService on every subsequent read. Reject
    // those at save time instead of persisting a corrupt blob (the service-side Deserialize
    // guard stays as defence-in-depth for non-update paths like GetOrCreate / emblem writes).
    private static bool BeValidGuildSheetData(string json)
    {
        GuildSheetData? data;
        try { data = JsonSerializer.Deserialize<GuildSheetData>(json, JsonOpts); }
        catch (JsonException) { return false; }

        return data is not null
            && data.Identity is not null
            && data.Prestige is not null
            && data.Resources is not null
            && data.Knowledge is not null
            && data.Influence is not null
            && data.ActiveDoctrineIds is not null
            && data.Legado is not null
            && data.Resources.Materials is not null
            && data.Resources.Artifacts is not null
            && data.Knowledge.Maps is not null
            && data.Knowledge.Recipes is not null
            && data.Knowledge.CataloguedEnemies is not null
            && data.Knowledge.DefeatedBosses is not null
            && data.Knowledge.HistoricalRecords is not null
            // List ELEMENTS must be non-null too — [null] deserializes to a one-element list
            // whose single entry is null, which then NREs in mapping/rendering.
            && data.Influence.All(x => x is not null)
            && data.Legado.All(x => x is not null)
            && data.Resources.Materials.All(x => x is not null);
    }
}
