using Ruptura.Shared.Combat;

namespace Ruptura.Application.Services;

// Pure initiative-ordering and turn-cursor helper for the combat tracker. No I/O, no
// framework deps beyond Ruptura.Shared. Ordering does NOT skip defeated combatants — they
// keep their slot in the initiative order so the GM can still act on them.
public class CombatOrder
{
    // Sorted by Initiative descending, tie-broken by Percepcao descending, then stably by
    // Name (ascending, ordinal).
    public IReadOnlyList<Combatant> Order(IEnumerable<Combatant> combatants)
        => combatants
            .OrderByDescending(c => c.Initiative)
            .ThenByDescending(c => c.Percepcao)
            .ThenBy(c => c.Name, StringComparer.Ordinal)
            .ToList();

    // Advance the turn cursor. Advancing past the last index (count-1) wraps the index to 0
    // and increments the round. Returns the next (Round, Index). Guards count <= 0 by
    // returning the round unchanged with index 0.
    public (int Round, int Index) AdvanceTurn(int round, int currentIndex, int count)
    {
        if (count <= 0)
            return (round, 0);

        if (currentIndex >= count - 1)
            return (round + 1, 0);

        return (round, currentIndex + 1);
    }

    // Step the turn cursor back. From index 0, goes to the previous round's last index
    // (count-1) and decrements the round, flooring the round at 1 (at round 1 index 0 it
    // stays put — never goes negative). Guards count <= 0 by returning the round unchanged
    // with index 0.
    public (int Round, int Index) PreviousTurn(int round, int currentIndex, int count)
    {
        if (count <= 0)
            return (round, 0);

        if (currentIndex > 0)
            return (round, currentIndex - 1);

        // currentIndex == 0
        if (round <= 1)
            return (1, 0);

        return (round - 1, count - 1);
    }
}
