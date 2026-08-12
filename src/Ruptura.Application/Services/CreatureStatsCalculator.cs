using Ruptura.Application.Interfaces;
using Ruptura.Shared.Bestiary;

namespace Ruptura.Application.Services;

// Pure NP (Nível de Poder) calculator for bestiary creatures (§9.5.5). No I/O, no framework
// deps beyond Ruptura.Shared. BestiaryReference is the single source of truth for the weight
// maps and category ranges — this class never re-hardcodes those values.
public class CreatureStatsCalculator : ICreatureStatsCalculator
{
    public CreatureNpResult Calculate(CreatureData data)
    {
        // Defense-in-depth: tolerate null lists (e.g. `{"Abilities":null}`) AND null elements within
        // them (GM-2/GM-4 build CreatureData in-process and call Calculate directly, bypassing the
        // service's Sanitize) by skipping them rather than throwing.
        var naturalSkills = (data.NaturalSkills ?? []).Where(x => x is not null);
        var characteristics = (data.Characteristics ?? []).Where(x => x is not null);
        var abilities = (data.Abilities ?? []).Where(x => x is not null);
        var equipment = (data.Equipment ?? []).Where(x => x is not null);
        var attributes = data.Attributes ?? new CreatureAttributes();

        // Sum in long so int.MaxValue-scale attribute scores can't overflow mid-calculation;
        // clamp to the int range once at the end.
        long np = 0;

        // Term 1 — attribute grade bonus: Σ(score − 1) over the 8 GDD attributes.
        np += (long)attributes.Corpo - 1;
        np += (long)attributes.Controle - 1;
        np += (long)attributes.Vigor - 1;
        np += (long)attributes.Presenca - 1;
        np += (long)attributes.Intelecto - 1;
        np += (long)attributes.Percepcao - 1;
        np += (long)attributes.Vontade - 1;
        np += (long)attributes.Afinidade - 1;

        // Term 2 — natural skills grade bonus (same tier ladder as CharacterStatsCalculator).
        foreach (var skill in naturalSkills)
            np += SkillGradeBonus(skill.Points);

        // Term 3 — characteristic weights. Unknown weight → 0.
        foreach (var c in characteristics)
            np += BestiaryReference.WeightValues.GetValueOrDefault(c.Weight ?? string.Empty, 0);

        // Term 4 — ability tiers. Unknown tier → 0.
        foreach (var a in abilities)
            np += BestiaryReference.TierValues.GetValueOrDefault(a.Tier ?? string.Empty, 0);

        // Term 5 — equipment rarities. Unknown rarity → 0.
        foreach (var e in equipment)
            np += BestiaryReference.RarityValues.GetValueOrDefault(e.Rarity ?? string.Empty, 0);

        var clampedNp = ClampToInt(np);

        var (npMin, npMax) = BestiaryReference.CategoryRanges
            .TryGetValue(data.Category ?? string.Empty, out var range)
            ? range
            : (0, 0);

        var overflow = ExceedsCeiling(clampedNp, npMax);

        return new CreatureNpResult(clampedNp, npMin, npMax, overflow);
    }

    // Identical to CharacterStatsCalculator.SkillGradeBonus — kept in lock-step by design.
    private static int SkillGradeBonus(int points) => points switch
    {
        >= 100 => 4,
        >= 75 => 3,
        >= 50 => 2,
        >= 25 => 1,
        >= 10 => 0,
        _ => -2
    };

    // Open-ended categories (Chefe de Arco, Entidade Superior) use int.MaxValue as the upper
    // bound sentinel. This is a SEMANTIC guard, not overflow prevention: those categories have
    // no ceiling by definition, so CategoryOverflow is always false for them. (npMax <= 0 is the
    // unknown/unresolved-category case, likewise never an overflow.) Returning early here also
    // keeps the sentinel out of the `npMax * 115L` multiply below.
    //
    // Boundary: the spec is "more than 15% over" (strict). Comparing `np * 100 > npMax * 115` in
    // long avoids the floating-point drift of `np > npMax * 1.15` (e.g. 40*1.15 = 45.999…, which
    // would wrongly flag np=46 — exactly +15% — as overflow).
    private static bool ExceedsCeiling(int np, int npMax)
    {
        if (npMax == int.MaxValue || npMax <= 0) return false;
        return np * 100L > npMax * 115L;
    }

    private static int ClampToInt(long value) =>
        value > int.MaxValue ? int.MaxValue
        : value < int.MinValue ? int.MinValue
        : (int)value;
}
