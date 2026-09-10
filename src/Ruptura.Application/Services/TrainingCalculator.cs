using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Services;

public class TrainingCalculator : ITrainingCalculator
{
    // GDD §6.5 "Tabela de Treinamento em Sem Treinamento" — teto de pontos/dia while Points < 10.
    private static readonly IReadOnlyDictionary<string, double> SemTreinamentoCeiling = new Dictionary<string, double>
    {
        ["Nenhuma"] = 1, ["Baixa"] = 2, ["Media"] = 3, ["Alta"] = 5
    };

    public TrainingProjection Project(
        Guid skillCatalogEntryId, string skillName, string skillArea, int currentPoints,
        IReadOnlyList<GuildBuilding> buildings, IReadOnlyList<GuildStaff> staff,
        Guid characterSheetId, int days, string correlation)
    {
        var perDay = RatePerDay(skillArea, skillName, currentPoints, buildings, staff, characterSheetId, correlation);
        var toAdd = (int)Math.Floor(perDay * days);

        return new TrainingProjection
        {
            SkillCatalogEntryId = skillCatalogEntryId,
            SkillName = skillName,
            CurrentPoints = currentPoints,
            PointsPerDay = perDay,
            Days = days,
            Correlation = correlation,
            PointsToAdd = toAdd,
            ProjectedTotalPoints = currentPoints + toAdd
        };
    }

    private static double RatePerDay(
        string area, string skillName, int currentPoints, IReadOnlyList<GuildBuilding> buildings,
        IReadOnlyList<GuildStaff> staff, Guid characterSheetId, string correlation)
    {
        var multiplier = CorrelationMultiplier(currentPoints, correlation);

        if (currentPoints < 10)
        {
            var ceiling = SemTreinamentoCeiling.GetValueOrDefault(correlation, 1);
            return Math.Min(1 * multiplier, ceiling);
        }

        var installationBonus = InstallationBonus(area, skillName, buildings);
        var instructorBonus = HasDedicatedInstructor(area, staff, characterSheetId) ? 1 : 0;
        return (1 + installationBonus + instructorBonus) * multiplier;
    }

    private static double CorrelationMultiplier(int currentPoints, string correlation) => correlation switch
    {
        "Alta" => 1.5,
        "Baixa" => 0.5,
        "Nenhuma" => currentPoints < 50 ? 0.25 : 1.0,
        _ => 1.0 // "Media" and any unrecognized value (the service layer validates before calling) default here.
    };

    private static double InstallationBonus(string area, string skillName, IReadOnlyList<GuildBuilding> buildings)
    {
        // GDD §6.5 — Social has no normal installation at all; only "Liderança" gets the
        // Academia Militar avançada bonus (unlike every other Área's advanced tier, which
        // applies to the whole Área regardless of the specific skill).
        if (AreaEquals(area, "Social"))
            return AreaEquals(skillName, "Liderança") ? LevelOf(buildings, GuildCatalogIds.AcademiaMilitar) : 0;

        if (!TrainingReference.InstallationByArea.TryGetValue(area.Trim(), out var mapping))
            return 0;

        if (mapping.AdvancedId is { } advancedId)
        {
            var advancedLevel = LevelOf(buildings, advancedId);
            if (advancedLevel > 0) return advancedLevel * 1.0;
        }

        var normalLevel = LevelOf(buildings, mapping.NormalId);
        if (normalLevel == 0 && mapping.FallbackId is { } fallbackId)
            normalLevel = LevelOf(buildings, fallbackId);

        return normalLevel * (mapping.HalvedAgain ? 0.25 : 0.5);
    }

    private static int LevelOf(IReadOnlyList<GuildBuilding> buildings, Guid catalogEntryId) =>
        buildings.FirstOrDefault(b => b.CatalogEntryId == catalogEntryId && b.IsActive)?.Level ?? 0;

    private static bool HasDedicatedInstructor(
        string area, IReadOnlyList<GuildStaff> staff, Guid characterSheetId) =>
        staff.Any(s =>
            s.IsActive && s.Kind == GuildStaffKind.Worker && s.TypeOrRanking == GuildStaffTypes.Instrutor &&
            s.DedicatedCharacterSheetId == characterSheetId && AreaEquals(s.DedicatedSkillArea, area));

    // Free-text Área matching, case/whitespace-insensitive — same reasoning as
    // CharacterStatsCalculator.CategoryIs (a GM-typed or catalog-authored value's casing must
    // never silently zero out a mechanic).
    private static bool AreaEquals(string? a, string b) =>
        a is not null && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
}
