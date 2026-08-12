using Ruptura.Application.Interfaces;
using Ruptura.Shared.Encounters;

namespace Ruptura.Application.Services;

// Pure encounter-threat calculator (GDD §9.8 PG/PE/R, §9.9 OA/FCE). No I/O, no framework
// deps beyond Ruptura.Shared. EncounterReference is the single source of truth for every
// multiplier map — this class never re-hardcodes those numbers. Server-authoritative: the
// service recomputes these values and never trusts a client-supplied PE/R.
public class EncounterCalculator : IEncounterCalculator
{
    public EncounterThreatResult Calculate(
        IEnumerable<int> partyNps,
        int partySize,
        IEnumerable<(int Np, int Qty)> creatures,
        string intelligence,
        string terrain,
        string objective,
        decimal pressureMult,
        string difficulty,
        string duration,
        string fceBand)
    {
        var creatureList = (creatures ?? []).ToList();

        // ---- PG = Σ NP(party) × Synergy(partySize) ----
        // Sum in long so a huge party can't overflow before the synergy multiply.
        long partySum = 0;
        foreach (var np in partyNps ?? [])
            partySum += np;

        var pg = ClampToInt(partySum * EncounterReference.Synergy(partySize));

        // ---- PE = baseNp × QuantityMult × Intelligence × Terrain × Objective × Pressure ----
        long baseNp = 0;
        long totalCount = 0;
        foreach (var (np, qty) in creatureList)
        {
            baseNp += (long)np * qty;
            totalCount += qty;
        }

        // Unknown enum keys fall back to the neutral 1.0 factor (fixed sets are validated on
        // write, but the calculator is defensive so a stray key can never throw).
        var peDecimal = baseNp
            * EncounterReference.QuantityMult((int)ClampCount(totalCount))
            * EncounterReference.IntelligenceMult.GetValueOrDefault(intelligence ?? string.Empty, 1.0m)
            * EncounterReference.TerrainMult.GetValueOrDefault(terrain ?? string.Empty, 1.0m)
            * EncounterReference.ObjectiveMult.GetValueOrDefault(objective ?? string.Empty, 1.0m)
            * pressureMult;

        var pe = ClampToInt(peDecimal);

        // ---- R = PE / PG (guard PG ≤ 0 → safe sentinel, never divide by zero) ----
        // Computed from the clamped int Pg/Pe so R stays consistent with the displayed values.
        var r = pg > 0 ? (decimal)pe / pg : 0m;
        var rLabel = EncounterReference.RLabelFor(r);

        // ---- OA = PG × DifficultyFactor × DurationFactor (§9.9) ----
        var oa = ClampToInt(
            (long)pg
            * EncounterReference.DifficultyFactor.GetValueOrDefault(difficulty ?? string.Empty, 1.0m)
            * EncounterReference.DurationFactor.GetValueOrDefault(duration ?? string.Empty, 1));

        // ---- FCE advisory: RealStatMultiplier = 1 + (R − 1) × FCE ----
        // Unknown band → FCE 0 (no advisory scaling: RealStatMultiplier collapses to 1).
        var fce = EncounterReference.FceByRankingBand.GetValueOrDefault(fceBand ?? string.Empty, 0m);
        var realStatMultiplier = 1m + (r - 1m) * fce;

        return new EncounterThreatResult(pg, pe, r, rLabel, oa, fce, realStatMultiplier);
    }

    private static int ClampToInt(decimal value)
    {
        var rounded = Math.Round(value, MidpointRounding.AwayFromZero);
        if (rounded > int.MaxValue) return int.MaxValue;
        if (rounded < int.MinValue) return int.MinValue;
        return (int)rounded;
    }

    // Total creature count feeds QuantityMult's banded switch; clamp to int so an absurd
    // aggregate quantity can't overflow the cast (the top band 21+ is open-ended anyway).
    private static long ClampCount(long value) =>
        value > int.MaxValue ? int.MaxValue : value < 0 ? 0 : value;
}
