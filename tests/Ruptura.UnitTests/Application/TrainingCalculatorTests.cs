using FluentAssertions;
using Ruptura.Application.Services;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;
using Xunit;

namespace Ruptura.UnitTests.Application;

public class TrainingCalculatorTests
{
    private readonly TrainingCalculator _calc = new();
    private static readonly Guid SkillId = Guid.NewGuid();
    private static readonly Guid CharacterId = Guid.NewGuid();

    private static GuildBuilding Building(Guid catalogId, int level, bool active = true) =>
        new() { Id = Guid.NewGuid(), CatalogEntryId = catalogId, Level = level, IsActive = active };

    private static GuildStaff Instrutor(Guid? dedicatedCharacter, string? dedicatedArea, bool active = true) =>
        new()
        {
            Id = Guid.NewGuid(), Kind = GuildStaffKind.Worker, TypeOrRanking = GuildStaffTypes.Instrutor,
            IsActive = active, DedicatedCharacterSheetId = dedicatedCharacter, DedicatedSkillArea = dedicatedArea
        };

    [Fact]
    public void CampoDeTreinamentoII_MediaCorrelacao_SemInstrutor_Gives2PerDay()
    {
        // GDD §6.4 worked example: (1 + 1 + 0) × 1.0 = 2.
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 2) };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 15, buildings, [], CharacterId, 10, "Media");
        p.PointsPerDay.Should().Be(2);
        p.PointsToAdd.Should().Be(20);
        p.ProjectedTotalPoints.Should().Be(35);
    }

    [Fact]
    public void CampoDeTreinamentoV_InstrutorDedicado_AltaCorrelacao_GivesApprox6_75PerDay()
    {
        // GDD §6.4 worked example: (1 + 2.5 + 1) × 1.5 = 6.75.
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 5) };
        var staff = new List<GuildStaff> { Instrutor(CharacterId, "Combate — Armas") };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 20, buildings, staff, CharacterId, 1, "Alta");
        p.PointsPerDay.Should().Be(6.75);
    }

    [Fact]
    public void AcademiaMilitar_DoublesTheInstallationBonus_ForCombate()
    {
        var buildings = new List<GuildBuilding>
        {
            Building(GuildCatalogIds.CampoDeTreinamento, 5), Building(GuildCatalogIds.AcademiaMilitar, 3)
        };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 15, buildings, [], CharacterId, 1, "Media");
        // Academia Militar (avançada) wins: Level 3 × 1.0 = 3, not Campo's 5 × 0.5 = 2.5.
        p.PointsPerDay.Should().Be(4); // 1 + 3
    }

    [Fact]
    public void Exploracao_HalvesTheNormalInstallationBonusAgain()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 4) };
        var p = _calc.Project(SkillId, "Rastreamento", "Exploração", 15, buildings, [], CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(2); // 1 + (4 × 0.25)
    }

    [Fact]
    public void Alquimia_FallsBackToOficina_WhenJardimAlquimicoNotBuilt()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.Oficina, 2) };
        var p = _calc.Project(SkillId, "Poções", "Alquimia", 15, buildings, [], CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(2); // 1 + (2 × 0.5)
    }

    [Fact]
    public void Social_GivesNoInstallationBonus_UnlessSkillIsLideranca()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.AcademiaMilitar, 4) };
        var diplomacia = _calc.Project(SkillId, "Diplomacia", "Social", 15, buildings, [], CharacterId, 1, "Media");
        diplomacia.PointsPerDay.Should().Be(1); // Base only

        var lideranca = _calc.Project(SkillId, "Liderança", "Social", 15, buildings, [], CharacterId, 1, "Media");
        lideranca.PointsPerDay.Should().Be(5); // 1 + (4 × 1.0)
    }

    [Fact]
    public void UnmappedArea_GivesBaseOnly()
    {
        var p = _calc.Project(SkillId, "Homebrew", "Coisa Nova", 15, [], [], CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(1);
    }

    // GDD §6.5 "Sem Treinamento" tier (Points 0-9): Instalação/Instrutor are ignored entirely,
    // so the rate is just 1 × MultCorrelação, clamped by the "Teto por Correlação" table
    // (Nenhuma=1/Baixa=2/Média=3/Alta=5 — see SemTreinamentoCeiling). A built Campo de
    // Treinamento V is present here specifically to prove it has NO effect in this tier.
    // Note the ceiling never actually binds for any of the 4 correlations (1×mult is always
    // strictly below its own ceiling: 0.25<1, 0.5<2, 1.0<3, 1.5<5) — the MIN is implemented
    // anyway per the project convention of reproducing FECHADO tables literally, but there is
    // no reachable input that exercises the ceiling actually clamping something.
    [Theory]
    [InlineData("Nenhuma", 0.25)]
    [InlineData("Baixa", 0.5)]
    [InlineData("Media", 1.0)]
    [InlineData("Alta", 1.5)]
    public void SemTreinamento_IgnoresInstallationBonus_UsesCorrelationMultiplierOnly(string correlation, double expectedRate)
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 5) };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 5, buildings, [], CharacterId, 1, correlation);
        p.PointsPerDay.Should().Be(expectedRate);
    }

    [Fact]
    public void NenhumaCorrelacao_BecomesNormalProgression_AtFiftyPoints()
    {
        var below50 = _calc.Project(SkillId, "X", "Magia", 49, [], [], CharacterId, 1, "Nenhuma");
        below50.PointsPerDay.Should().Be(0.25);

        var at50 = _calc.Project(SkillId, "X", "Magia", 50, [], [], CharacterId, 1, "Nenhuma");
        at50.PointsPerDay.Should().Be(1);
    }

    [Fact]
    public void InactiveInstallation_GivesNoBonus()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 5, active: false) };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 15, buildings, [], CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(1);
    }

    [Fact]
    public void InstrutorDedicatedToDifferentCharacterOrArea_GivesNoBonus()
    {
        var otherCharacter = Guid.NewGuid();
        var staff = new List<GuildStaff>
        {
            Instrutor(otherCharacter, "Combate — Armas"),
            Instrutor(CharacterId, "Magia")
        };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 15, [], staff, CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(1);
    }

    [Fact]
    public void PointsToAdd_FloorsFractionalTotals()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 1) };
        // (1 + 0.5) × 1.0 = 1.5/day × 3 days = 4.5 → floors to 4.
        var p = _calc.Project(SkillId, "X", "Combate — Armas", 15, buildings, [], CharacterId, 3, "Media");
        p.PointsToAdd.Should().Be(4);
    }
}
