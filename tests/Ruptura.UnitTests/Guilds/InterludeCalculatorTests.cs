using FluentAssertions;
using Ruptura.Application.Services;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;
using Xunit;

namespace Ruptura.UnitTests.Guilds;

public class InterludeCalculatorTests
{
    private readonly InterludeCalculator _calc = new();

    private static ResearchProject Research(int required, int progress, int researchers, int points, bool complete = false) =>
        new() { Id = Guid.NewGuid(), Name = "R", Complexity = ResearchComplexity.Maior,
                RequiredDays = required, ProgressDays = progress, Researchers = researchers, Points = points, IsComplete = complete };

    private static CraftingOrder Crafting(int required, int progress, CraftingStatus status = CraftingStatus.EmAndamento) =>
        new() { Id = Guid.NewGuid(), Category = CraftingCategory.Forja, ItemName = "Sword",
                RequiredDays = required, ProgressDays = progress, Status = status };

    [Fact]
    public void Maintenance_And_Income_Scale_With_Days()
    {
        var derived = new GuildDerivedStats { DailyMaintenance = 15, WorkerIncomePerDay = 4 };
        var p = _calc.Project(derived, [], [], days: 30);
        p.Days.Should().Be(30);
        p.Indicators.Should().Contain(i => i.Kind == "Maintenance" && i.SilverDelta == -450);
        p.Indicators.Should().Contain(i => i.Kind == "Income" && i.SilverDelta == 120);
    }

    [Fact]
    public void Research_OneResearcher_AdvancesOnePerDay_CapsAtRequired()
    {
        var r = Research(required: 20, progress: 0, researchers: 1, points: 3);
        var p = _calc.Project(new GuildDerivedStats(), [r], [], days: 10);
        var ind = p.Indicators.Single(i => i.Kind == "ResearchProgress" && i.TargetId == r.Id);
        ind.DaysAdded.Should().Be(10);       // min(1,2)*10 = 10
        ind.WillComplete.Should().BeFalse();
        ind.PointsAwarded.Should().Be(0);
    }

    [Fact]
    public void Research_TwoResearchers_HalfTime_Completes_AwardsPoints()
    {
        var r = Research(required: 20, progress: 0, researchers: 2, points: 3);
        var p = _calc.Project(new GuildDerivedStats(), [r], [], days: 10);
        var ind = p.Indicators.Single(i => i.Kind == "ResearchProgress");
        ind.DaysAdded.Should().Be(20);       // min(2,2)*10 = 20 == required
        ind.WillComplete.Should().BeTrue();
        ind.PointsAwarded.Should().Be(3);
    }

    [Fact]
    public void Research_ThreeResearchers_StillCappedAtTwoPerDay()
    {
        var r = Research(required: 20, progress: 0, researchers: 5, points: 3);
        var p = _calc.Project(new GuildDerivedStats(), [r], [], days: 3);
        p.Indicators.Single(i => i.Kind == "ResearchProgress").DaysAdded.Should().Be(6); // min(5,2)*3
    }

    [Fact]
    public void CompletedResearch_ProducesNoIndicator()
    {
        var r = Research(required: 20, progress: 20, researchers: 1, points: 3, complete: true);
        var p = _calc.Project(new GuildDerivedStats(), [r], [], days: 5);
        p.Indicators.Should().NotContain(i => i.Kind == "ResearchProgress");
    }

    [Fact]
    public void Crafting_AdvancesOnePerDay_Completes()
    {
        var c = Crafting(required: 6, progress: 4);
        var p = _calc.Project(new GuildDerivedStats(), [], [c], days: 5);
        var ind = p.Indicators.Single(i => i.Kind == "CraftingProgress" && i.TargetId == c.Id);
        ind.DaysAdded.Should().Be(2);        // capped at required-progress
        ind.WillComplete.Should().BeTrue();
    }

    [Fact]
    public void FinishedOrCancelledCrafting_ProducesNoIndicator()
    {
        var done = Crafting(6, 6, CraftingStatus.Concluido);
        var cancelled = Crafting(6, 0, CraftingStatus.Cancelado);
        var p = _calc.Project(new GuildDerivedStats(), [], [done, cancelled], days: 5);
        p.Indicators.Should().NotContain(i => i.Kind == "CraftingProgress");
    }

    [Fact]
    public void HugeMaintenanceTimesDays_DoesNotOverflow()
    {
        var derived = new GuildDerivedStats { DailyMaintenance = int.MaxValue };
        var act = () => _calc.Project(derived, [], [], days: 3650);
        act.Should().NotThrow();
        _calc.Project(derived, [], [], 3650).Indicators.Single(i => i.Kind == "Maintenance")
            .SilverDelta.Should().Be(int.MinValue); // saturated negative
    }
}
