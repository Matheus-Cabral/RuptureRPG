using System.Text.Json;
using FluentAssertions;
using Ruptura.Application.Services;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;
using Xunit;

namespace Ruptura.UnitTests.Guilds;

public class GuildStatsCalculatorTests
{
    private readonly GuildStatsCalculator _calc = new();

    // Builds an installation catalog entry with a given weight/NonConstructible.
    private static CatalogEntry Install(Guid id, int weight, bool nonConstructible = false) => new()
    {
        Id = id,
        Type = CatalogEntryType.Installation,
        Name = id.ToString(),
        DataJson = JsonSerializer.Serialize(new InstallationCatalogData
        {
            Category = "Fundação", Weight = weight, LevelCap = 5, NonConstructible = nonConstructible
        })
    };

    private static GuildBuilding Building(Guid installationId, int level, bool active = true) =>
        new() { Id = Guid.NewGuid(), GuildSheetId = Guid.NewGuid(), CatalogEntryId = installationId, Level = level, IsActive = active };

    private static GuildStaff Worker(string type, int salary, bool active = true) =>
        new() { Id = Guid.NewGuid(), Kind = GuildStaffKind.Worker, TypeOrRanking = type, DailySalary = salary, IsActive = active };

    private static GuildStaff Merc(string ranking, int salary, bool active = true) =>
        new() { Id = Guid.NewGuid(), Kind = GuildStaffKind.Mercenary, TypeOrRanking = ranking, DailySalary = salary, IsActive = active };

    [Theory]
    [InlineData(0, GuildStage.Fundacao, 0)]
    [InlineData(4, GuildStage.Fundacao, 0)]
    [InlineData(5, GuildStage.Menor, 1)]
    [InlineData(10, GuildStage.Regional, 2)]
    [InlineData(15, GuildStage.Reconhecida, 3)]
    [InlineData(20, GuildStage.Maior, 4)]
    [InlineData(25, GuildStage.Renomada, 5)]
    [InlineData(30, GuildStage.Lendaria, 6)]
    [InlineData(35, GuildStage.Divina, 7)]
    [InlineData(99, GuildStage.Divina, 7)]
    public void Stage_DerivesFromFloorsConquered(int floors, GuildStage expected, int expectedIndex)
    {
        var data = new GuildSheetData { FloorsConquered = floors };
        var r = _calc.Calculate(data, [], [], 0, new Dictionary<Guid, CatalogEntry>());
        r.Stage.Should().Be(expected);
        r.StageIndex.Should().Be(expectedIndex);
    }

    [Fact]
    public void CsCiCf_ReproduceFundacaoCanonicalRow()
    {
        // §10.9 Fundação row: CS 6, CI 3, CF 11 — canonical minimal build: Armazém I, Campo de Treinamento I.
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [GuildCatalogIds.Armazem] = Install(GuildCatalogIds.Armazem, 1),
            [GuildCatalogIds.CampoDeTreinamento] = Install(GuildCatalogIds.CampoDeTreinamento, 1),
        };
        var buildings = new List<GuildBuilding>
        {
            Building(GuildCatalogIds.Armazem, 1),
            Building(GuildCatalogIds.CampoDeTreinamento, 1),
        };
        var r = _calc.Calculate(new GuildSheetData(), buildings, [], 0, catalog);
        r.Cs.Should().Be(6);   // 5 + 0*2 + 1*1
        r.Ci.Should().Be(3);   // 3 + 0*4 + 0*1
        r.Cf.Should().Be(11);  // 10 + 0*3 + 0*1 + 1*1
    }

    [Fact]
    public void CsCiCf_ComputeFromNamedInstallationLevels()
    {
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [GuildCatalogIds.CentroLogistico] = Install(GuildCatalogIds.CentroLogistico, 5),
            [GuildCatalogIds.Armazem] = Install(GuildCatalogIds.Armazem, 1),
            [GuildCatalogIds.CamaraDoConselho] = Install(GuildCatalogIds.CamaraDoConselho, 8),
            [GuildCatalogIds.Memorial] = Install(GuildCatalogIds.Memorial, 5),
            [GuildCatalogIds.Biblioteca] = Install(GuildCatalogIds.Biblioteca, 2),
            [GuildCatalogIds.CampoDeTreinamento] = Install(GuildCatalogIds.CampoDeTreinamento, 1),
        };
        var buildings = new List<GuildBuilding>
        {
            Building(GuildCatalogIds.CentroLogistico, 3),
            Building(GuildCatalogIds.Armazem, 4),
            Building(GuildCatalogIds.CamaraDoConselho, 2),
            Building(GuildCatalogIds.Memorial, 3),
            Building(GuildCatalogIds.Biblioteca, 2),
            Building(GuildCatalogIds.CampoDeTreinamento, 1),
        };
        var r = _calc.Calculate(new GuildSheetData(), buildings, [], 0, catalog);
        r.Cs.Should().Be(15);  // 5 + 3*2 + 4*1 = 5+6+4 = 15
        r.Ci.Should().Be(14);  // 3 + 2*4 + 3*1 = 3+8+3 = 14
        r.Cf.Should().Be(22);  // 10 + 3*3 + 2*1 + 1*1 = 10+9+2+1 = 22
    }

    [Fact]
    public void Logistica_Doctrine_Boosts_Cs_And_Reduces_Maintenance()
    {
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [GuildCatalogIds.CentroLogistico] = Install(GuildCatalogIds.CentroLogistico, 5),
            [GuildCatalogIds.Armazem] = Install(GuildCatalogIds.Armazem, 1),
        };
        var buildings = new List<GuildBuilding>
        {
            Building(GuildCatalogIds.CentroLogistico, 3),  // weight 5, level 3 -> maintenance 15
            Building(GuildCatalogIds.Armazem, 3),          // weight 1, level 3 -> maintenance 3
        };
        var data = new GuildSheetData { ActiveDoctrineIds = [GuildCatalogIds.DoctrineLogistica] };
        var r = _calc.Calculate(data, buildings, [], 0, catalog);
        // base CS = 5 + 3*2 + 3*1 = 14; ×1.20 = 16.8 -> floor 16
        r.Cs.Should().Be(16);
        // base maintenance = 15 + 3 = 18; ×0.90 = 16.2 -> round away-from-zero = 16
        r.DailyMaintenance.Should().Be(16);
    }

    [Fact]
    public void Comercial_Doctrine_Drops_Inflation_One_Stage_Floored()
    {
        var data = new GuildSheetData { FloorsConquered = 0, ActiveDoctrineIds = [GuildCatalogIds.DoctrineComercial] };
        var r = _calc.Calculate(data, [], [], 0, new Dictionary<Guid, CatalogEntry>());
        r.InflationIndex.Should().Be(1.0m); // Fundação (idx 0) -1 floored at 0 -> still 1.0
    }

    [Fact]
    public void Inflation_By_Stage()
    {
        var data = new GuildSheetData { FloorsConquered = 20 }; // Maior, idx 4 -> 2.2
        var r = _calc.Calculate(data, [], [], 0, new Dictionary<Guid, CatalogEntry>());
        r.InflationIndex.Should().Be(2.2m);
    }

    [Fact]
    public void Infra_And_Maintenance_Exclude_NonConstructible_Portao()
    {
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [GuildCatalogIds.Portao] = Install(GuildCatalogIds.Portao, 1, nonConstructible: true),
            [GuildCatalogIds.Armazem] = Install(GuildCatalogIds.Armazem, 1),
        };
        var buildings = new List<GuildBuilding>
        {
            Building(GuildCatalogIds.Portao, 1),
            Building(GuildCatalogIds.Armazem, 2),
        };
        var r = _calc.Calculate(new GuildSheetData(), buildings, [], 0, catalog);
        r.CgInfra.Should().Be(2);            // only Armazém 2*1; Portão excluded
        r.DailyMaintenance.Should().Be(2);   // only Armazém; Portão excluded
        r.ActiveBuildingCount.Should().Be(1);// Portão not counted toward CS cap
    }

    [Fact]
    public void Cg_Breakdown_Sums_All_Four_Terms()
    {
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [GuildCatalogIds.CentroLogistico] = Install(GuildCatalogIds.CentroLogistico, 5),
            [GuildCatalogIds.Armazem] = Install(GuildCatalogIds.Armazem, 1),
        };
        var buildings = new List<GuildBuilding>
        {
            Building(GuildCatalogIds.CentroLogistico, 2),  // infra 10
            Building(GuildCatalogIds.Armazem, 1),          // infra 1
        };
        var staff = new List<GuildStaff> { Worker("Artesão", 8), Worker("Operário", 3) }; // 2 workers
        var data = new GuildSheetData
        {
            Resources = new GuildResources
            {
                PactCoins = 5, DimensionalFragments = 2,
                Materials = [ new MaterialStock { Name = "Ferro", Quantity = 10 } ]
            }
        };
        var r = _calc.Calculate(data, buildings, staff, researchPoints: 7, catalog);
        // CS = 5 + 2*2 + 1*1 = 10; Logistica = CS + workers*2 = 10 + 2*2 = 14
        r.CgInfra.Should().Be(11);
        r.CgPesquisa.Should().Be(7);
        r.CgLogistica.Should().Be(14);
        r.CgRecursos.Should().Be(17);        // 5 + 2 + 10
        r.Cg.Should().Be(11 + 7 + 14 + 17);  // 49
    }

    [Fact]
    public void Maintenance_Includes_Only_Active_Staff_Salaries()
    {
        var staff = new List<GuildStaff> { Worker("Artesão", 8), Merc("Bronze", 10), Worker("Operário", 3, active: false) };
        var r = _calc.Calculate(new GuildSheetData(), [], staff, 0, new Dictionary<Guid, CatalogEntry>());
        r.DailyMaintenance.Should().Be(18); // 8 + 10; inactive Operário excluded
    }

    [Fact]
    public void WorkerIncome_Is_Two_Per_Active_Operario()
    {
        var staff = new List<GuildStaff> { Worker("Operário", 3), Worker("Operário", 3), Worker("Artesão", 8), Worker("Operário", 3, active: false) };
        var r = _calc.Calculate(new GuildSheetData(), [], staff, 0, new Dictionary<Guid, CatalogEntry>());
        r.WorkerIncomePerDay.Should().Be(4); // 2 active Operários × 2
    }

    [Fact]
    public void Caps_And_DoctrineLimit()
    {
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [GuildCatalogIds.Armazem] = Install(GuildCatalogIds.Armazem, 1),
            [GuildCatalogIds.Dormitorio] = Install(GuildCatalogIds.Dormitorio, 1),
            [GuildCatalogIds.CamaraDoConselho] = Install(GuildCatalogIds.CamaraDoConselho, 8),
        };
        var buildings = new List<GuildBuilding>
        {
            Building(GuildCatalogIds.Armazem, 4),          // storage 200
            Building(GuildCatalogIds.Dormitorio, 3),       // residency 6
            Building(GuildCatalogIds.CamaraDoConselho, 2), // doctrine limit 2+2=4
        };
        var r = _calc.Calculate(new GuildSheetData(), buildings, [], 0, catalog);
        r.StorageCapacity.Should().Be(200);
        r.ResidencyCapacity.Should().Be(6);
        r.DoctrineLimit.Should().Be(4);
    }

    [Fact]
    public void DoctrineLimit_CapsAtFour()
    {
        var catalog = new Dictionary<Guid, CatalogEntry> { [GuildCatalogIds.CamaraDoConselho] = Install(GuildCatalogIds.CamaraDoConselho, 8) };
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CamaraDoConselho, 5) }; // 2+5=7 -> capped 4
        var r = _calc.Calculate(new GuildSheetData(), buildings, [], 0, catalog);
        r.DoctrineLimit.Should().Be(4);
    }

    [Fact]
    public void ActiveBuildingOverflow_When_Active_Exceeds_Cs()
    {
        // CS is 5 with no CentroLogistico/Armazem; 6 active constructible buildings -> overflow.
        var catalog = new Dictionary<Guid, CatalogEntry>();
        var buildings = new List<GuildBuilding>();
        for (var i = 0; i < 6; i++)
        {
            var id = Guid.NewGuid();
            catalog[id] = Install(id, 1);
            buildings.Add(Building(id, 1));
        }
        var r = _calc.Calculate(new GuildSheetData(), buildings, [], 0, catalog);
        r.Cs.Should().Be(5);
        r.ActiveBuildingCount.Should().Be(6);
        r.ActiveBuildingOverflow.Should().BeTrue();
    }
}
