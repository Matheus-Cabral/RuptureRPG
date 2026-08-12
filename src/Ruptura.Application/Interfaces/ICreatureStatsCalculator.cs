using Ruptura.Shared.Bestiary;

namespace Ruptura.Application.Interfaces;

public interface ICreatureStatsCalculator
{
    CreatureNpResult Calculate(CreatureData data);
}

// Derived NP + the advisory category range it should fall in. NpMax uses int.MaxValue as the
// open-ended sentinel for the top tiers (mapped to null at the CreatureResponse boundary).
// CategoryOverflow is a soft warning meaning ONLY "over the +15% ceiling" (§9.5.6 Regra do Teto):
// it trips solely when Np exceeds NpMax by more than 15%. It does NOT flag Np below NpMin, and is
// always false for open-ended categories. GM-2 consumers must not read it as a generic
// "out of range" flag.
public record CreatureNpResult(int Np, int NpMin, int NpMax, bool CategoryOverflow);
