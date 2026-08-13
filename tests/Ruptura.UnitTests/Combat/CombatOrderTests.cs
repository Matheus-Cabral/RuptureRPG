using FluentAssertions;
using Ruptura.Application.Services;
using Ruptura.Shared.Combat;

namespace Ruptura.UnitTests.Combat;

public class CombatOrderTests
{
    private readonly CombatOrder _sut = new();

    private static Combatant Make(string name, int initiative, int percepcao = 0, bool defeated = false)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Initiative = initiative,
            Percepcao = percepcao,
            IsDefeated = defeated
        };

    // ---- Order ----

    [Fact]
    public void Order_SortsByInitiativeDescending()
    {
        var result = _sut.Order([Make("A", 5), Make("B", 20), Make("C", 12)]);

        result.Select(c => c.Name).Should().ContainInOrder("B", "C", "A");
    }

    [Fact]
    public void Order_TieBreaksByPercepcaoDescending()
    {
        var result = _sut.Order([
            Make("A", 10, percepcao: 3),
            Make("B", 10, percepcao: 9),
            Make("C", 10, percepcao: 6)
        ]);

        result.Select(c => c.Name).Should().ContainInOrder("B", "C", "A");
    }

    [Fact]
    public void Order_TieBreaksByNameOrdinalWhenInitiativeAndPercepcaoEqual()
    {
        var result = _sut.Order([
            Make("Charlie", 10, percepcao: 5),
            Make("Alice", 10, percepcao: 5),
            Make("Bravo", 10, percepcao: 5)
        ]);

        result.Select(c => c.Name).Should().ContainInOrder("Alice", "Bravo", "Charlie");
    }

    [Fact]
    public void Order_DoesNotSkipDefeatedCombatants()
    {
        var result = _sut.Order([
            Make("Dead", 30, defeated: true),
            Make("Alive", 10)
        ]);

        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().ContainInOrder("Dead", "Alive");
    }

    // ---- AdvanceTurn ----

    [Fact]
    public void AdvanceTurn_MovesToNextIndexSameRound()
    {
        var (round, index) = _sut.AdvanceTurn(round: 2, currentIndex: 1, count: 4);

        round.Should().Be(2);
        index.Should().Be(2);
    }

    [Fact]
    public void AdvanceTurn_FromLastIndexWrapsToZeroAndIncrementsRound()
    {
        var (round, index) = _sut.AdvanceTurn(round: 3, currentIndex: 3, count: 4);

        round.Should().Be(4);
        index.Should().Be(0);
    }

    [Fact]
    public void AdvanceTurn_CountZeroOrLessReturnsRoundUnchangedIndexZero()
    {
        var (round, index) = _sut.AdvanceTurn(round: 5, currentIndex: 2, count: 0);

        round.Should().Be(5);
        index.Should().Be(0);
    }

    // ---- PreviousTurn ----

    [Fact]
    public void PreviousTurn_MovesToPreviousIndexSameRound()
    {
        var (round, index) = _sut.PreviousTurn(round: 2, currentIndex: 2, count: 4);

        round.Should().Be(2);
        index.Should().Be(1);
    }

    [Fact]
    public void PreviousTurn_FromIndexZeroGoesToPreviousRoundLastIndex()
    {
        var (round, index) = _sut.PreviousTurn(round: 3, currentIndex: 0, count: 4);

        round.Should().Be(2);
        index.Should().Be(3);
    }

    [Fact]
    public void PreviousTurn_AtRoundOneIndexZeroStaysPut()
    {
        var (round, index) = _sut.PreviousTurn(round: 1, currentIndex: 0, count: 4);

        round.Should().Be(1);
        index.Should().Be(0);
    }

    [Fact]
    public void PreviousTurn_CountZeroOrLessReturnsRoundUnchangedIndexZero()
    {
        var (round, index) = _sut.PreviousTurn(round: 5, currentIndex: 2, count: 0);

        round.Should().Be(5);
        index.Should().Be(0);
    }
}
