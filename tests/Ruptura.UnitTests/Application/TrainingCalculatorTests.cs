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
    public void AdvancedInstallation_NeverScoresLowerThanNormalTier()
    {
        // Campo de Treinamento V (5 × 0.5 = 2.5) vs. a freshly-built Academia Militar I
        // (1 × 1.0 = 1.0). The "avançada dobra o bônus" rule must never make the result WORSE
        // than staying on the normal tier — regression guard for the bug where any AdvancedId
        // built at all (even Level 1) unconditionally short-circuited to the advanced value.
        var buildings = new List<GuildBuilding>
        {
            Building(GuildCatalogIds.CampoDeTreinamento, 5), Building(GuildCatalogIds.AcademiaMilitar, 1)
        };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 15, buildings, [], CharacterId, 1, "Media");
        // (1 + 2.5 + 0) × 1.0 = 3.5, NOT (1 + 1.0 + 0) × 1.0 = 2.
        p.PointsPerDay.Should().Be(3.5);
    }

    [Fact]
    public void Exploracao_HalvesTheNormalInstallationBonusAgain()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 4) };
        var p = _calc.Project(SkillId, "Rastreamento", "Exploração", 15, buildings, [], CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(2); // 1 + (4 × 0.25)
    }

    [Fact]
    public void OficinaDeRunas_DoublesTheInstallationBonus_ForArtesanato()
    {
        var buildings = new List<GuildBuilding>
        {
            Building(GuildCatalogIds.Oficina, 5), Building(GuildCatalogIds.OficinaDeRunas, 3)
        };
        var p = _calc.Project(SkillId, "Ferraria", "Artesanato", 15, buildings, [], CharacterId, 1, "Media");
        // Oficina de Runas (avançada) wins: Level 3 × 1.0 = 3, not Oficina's 5 × 0.5 = 2.5.
        p.PointsPerDay.Should().Be(4); // 1 + 3
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
    // and the rate IS the Teto value directly (not a correlation-multiplied-and-capped rate —
    // that older reading was mathematically almost inert, since 1×MultCorrelação never actually
    // reached its own ceiling, and it didn't match the GDD's own "Dias até Básico" column). Using
    // the Teto directly reconciles exactly: Nenhuma=10d → 10÷1=10✓, Baixa=5d → 10÷2=5✓,
    // Média=~4d → 10÷3≈3.3✓ (rounded), Alta=2d → 10÷5=2✓. A built Campo de Treinamento V is
    // present here specifically to prove it has NO effect in this tier.
    [Theory]
    [InlineData("Nenhuma", 1.0)]
    [InlineData("Baixa", 2.0)]
    [InlineData("Media", 3.0)]
    [InlineData("Alta", 5.0)]
    public void SemTreinamento_IgnoresInstallationBonus_UsesCeilingDirectly(string correlation, double expectedRate)
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
