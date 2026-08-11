using Ruptura.Shared.Bestiary;

namespace Ruptura.Application.Interfaces;

public interface ICreatureStatsCalculator
{
    CreatureNpResult Calculate(CreatureData data);
}

// Derived NP + the advisory category range it should fall in. CategoryOverflow is a soft
// warning: the computed NP exceeds the category's ceiling by more than 15% (§9.5.6).
public record CreatureNpResult(int Np, int NpMin, int NpMax, bool CategoryOverflow);
