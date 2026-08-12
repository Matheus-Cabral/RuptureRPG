namespace Ruptura.Application.Interfaces;

public interface IEncounterCalculator
{
    // Pure GDD §9.8/§9.9 threat math. `pressureMult` is passed IN (1.0 when Pressão is
    // off, otherwise the campaign's DungeonPressure PE multiplier) — the calculator never
    // reads the campaign. `partySize` is supplied separately from `partyNps` because the
    // caller may override it (§9.8 synergy is by alive-party size).
    EncounterThreatResult Calculate(
        IEnumerable<int> partyNps,
        int partySize,
        IEnumerable<(int Np, int Qty)> creatures,
        string intelligence,
        string terrain,
        string objective,
        decimal pressureMult,
        string difficulty,
        string duration,
        string fceBand);
}

// Computed encounter threat values (§9.8 PG/PE/R, §9.9 OA/FCE). Pg/Pe/Oa are long-summed
// and clamped to int. R and the multipliers are decimal. When PG ≤ 0 (no resolvable party)
// the calculator returns a NEUTRAL, self-describing record — R = 0, RLabel = "" (no verdict),
// RealStatMultiplier = 1 — rather than dividing or emitting a misleading "MuitoFacil"/0.60
// sentinel; Pe is still returned so the real enemy power is visible. It never throws; the
// SERVICE additionally surfaces PartyResolved = false in that case.
public record EncounterThreatResult(
    int Pg,
    int Pe,
    decimal R,
    string RLabel,
    int Oa,
    decimal Fce,
    decimal RealStatMultiplier);
