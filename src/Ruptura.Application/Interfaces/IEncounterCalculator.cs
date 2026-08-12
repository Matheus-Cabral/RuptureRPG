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
// the calculator returns R = 0 and RLabel = RLabelFor(0) rather than dividing — it never
// throws; the SERVICE is responsible for surfacing PartyResolved = false in that case.
public record EncounterThreatResult(
    int Pg,
    int Pe,
    decimal R,
    string RLabel,
    int Oa,
    decimal Fce,
    decimal RealStatMultiplier);
