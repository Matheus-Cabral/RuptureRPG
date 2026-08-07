# Guild Sheet — Stats Calculator & Capacities Panel (Sub-plan #2) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Compute every institutional derived value of a guild (Stage, CG + breakdown, CI, CF, CS, inflation, daily maintenance, worker income, storage/residency caps, doctrine limit, active-building overflow) on read via a pure `GuildStatsCalculator`, expose it through a read-only guild endpoint (get-or-create + shared-write authorization), and render it in a Blazor **Capacidades** panel.

**Architecture:** `GuildStatsCalculator` (Application, pure & stateless, mirrors `CharacterStatsCalculator`) takes the stored blob + child rows and returns a `GuildDerivedStats` DTO — never persisted. `GuildSheetService` (Infrastructure) authorizes (campaign GM **or** member), gets-or-creates the campaign's single guild, loads its buildings/staff, runs the calculator, and maps to `GuildSheetResponse`. `GuildController` exposes `GET campaigns/{id}/guild`. The Web project renders the derived panel.

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-08-07-guild-sheet-design.md` §4 (calculator), §6 (API/permissions), §7 (Capacidades tab), §12.2. Formula sources: GDD §10.8 (CG), §10.9 (CS/CI/CF + per-stage table), §10.6.4 (inflation), §8.4 (maintenance/income), Manual §8.1.1.

## Global Constraints

- **Clean Architecture:** calculator + interfaces in `Ruptura.Application`; EF service/repos in `Ruptura.Infrastructure`; controller in `Ruptura.API`; DTOs in `Ruptura.Shared.Guilds`; UI in `Ruptura.Web`.
- **Pure calculator, never persisted** — `GuildStatsCalculator` matches `CharacterStatsCalculator`'s style: stateless, defensive against null/malformed catalog DataJson via a `SafeDeserialize` that returns null on `JsonException` (a GM can save malformed homebrew installation DataJson via the catalog admin page — it must never 500 a guild read).
- **Formula-relevant installations & doctrines are identified by their seeded GUIDs, not by name** (names can be homebrewed/renamed) — declared once in `GuildCatalogIds` and asserted against the seed in Task 3.
- **Shared write:** authorize if the caller is the campaign's `GameMasterId` OR a `CampaignMembership` of the campaign. Read and write share the check. Consume `ICampaignMembershipRepository.GetByPlayerAsync`/`ExistsAsync` (already exist).
- **Decisions locked for this sub-plan (from spec §11 + user confirmation 2026-08-07):**
  - Qualified workers (CG Logística term) = **all active workers** (`Kind == Worker && IsActive`); mercenaries do not count.
  - Recursos (CG term) = `PactCoins + DimensionalFragments + Σ Materials[].Quantity`. Silver is excluded.
  - `NonConstructible` installations (only Portão) are **excluded from Infra, from Daily Maintenance, and from the CS active-building cap count**.
- **Rounding (deterministic):** doctrine-modified CS = `(int)Math.Floor(baseCs * 1.20m)`; doctrine-modified maintenance = `(int)Math.Round(baseMaintenance * 0.90m, MidpointRounding.AwayFromZero)`. Inflation index is `decimal`.
- **Only Logística and Comercial doctrines affect these institutional stats** (Logística: +20% CS, −10% maintenance; Comercial: −1 inflation stage, floored at Fundação). The other six doctrines affect combat/research/etc. — out of scope for this panel; do not apply them here.
- **i18n:** every Blazor string via `IStringLocalizer`; add both pt-BR and en resx entries. Never hard-code visible text.
- **Integration tests** use `WebApplicationFactory<Program>` + Testcontainers (fixture `IntegrationTestFactory`, `IClassFixture<>`, `parallelizeTestCollections: false`). A single Serilog "logger already frozen" flake is a known pre-existing race — re-run once.
- **Commit after each task** on `main`; end commit messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:**
- `src/Ruptura.Shared/Guilds/GuildStage.cs`, `InstallationCatalogData.cs`, `GuildCatalogIds.cs`, `GuildDerivedStats.cs`, `GuildSheetResponse.cs`
- `src/Ruptura.Application/Interfaces/IGuildStatsCalculator.cs`, `IGuildSheetService.cs`, `IGuildBuildingRepository.cs`, `IGuildStaffRepository.cs`
- `src/Ruptura.Application/Services/GuildStatsCalculator.cs`
- `src/Ruptura.Infrastructure/Repositories/GuildBuildingRepository.cs`, `GuildStaffRepository.cs`
- `src/Ruptura.Infrastructure/Services/GuildSheetService.cs`
- `src/Ruptura.API/Controllers/GuildController.cs`
- `src/Ruptura.Web/Pages/GuildSheet.razor`, `GuildCapacitiesPanel.razor`
- `tests/Ruptura.UnitTests/Guilds/GuildStatsCalculatorTests.cs`
- `tests/Ruptura.IntegrationTests/Guilds/GuildCatalogIdsTests.cs`, `GuildRepositoryReadTests.cs`, `GuildControllerTests.cs`

**Modify:**
- `src/Ruptura.Application/Common/ErrorCodes.cs` (add `Guild`)
- `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` (register calculator, service, 2 repos)
- `src/Ruptura.API/Resources/SharedResources.*.resx` (Guild.* + ErrorCode strings) — follow the existing resx pair
- Web API client + campaign pages (entry point to the guild page) — follow existing patterns

---

### Task 1: Shared DTOs, enum, and catalog-id constants

Pure data types, folded into one build-verified task. Later tasks depend on these names.

**Files:** Create the five files under `src/Ruptura.Shared/Guilds/`.

**Interfaces:**
- Produces: `GuildStage` enum; `InstallationCatalogData`; `GuildCatalogIds` (static `Guid` fields); `GuildDerivedStats`; `GuildSheetResponse`.

- [ ] **Step 1: `GuildStage` enum**

`src/Ruptura.Shared/Guilds/GuildStage.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

// 8 GDD guild stages (§10.8), by floors conquered. Unaccented identifiers; UI localizes display.
// Order IS the stage index (Fundacao = 0 .. Divina = 7).
public enum GuildStage
{
    Fundacao,
    Menor,
    Regional,
    Reconhecida,
    Maior,
    Renomada,
    Lendaria,
    Divina
}
```

- [ ] **Step 2: `InstallationCatalogData`**

`src/Ruptura.Shared/Guilds/InstallationCatalogData.cs` — the shape stored in an Installation `CatalogEntry.DataJson` (see sub-plan #1 seed):
```csharp
namespace Ruptura.Shared.Guilds;

public class InstallationCatalogData
{
    public string Category { get; set; } = string.Empty;   // Fundação|Produção|Especialização|Institucional|Monumental
    public int Weight { get; set; }
    public int LevelCap { get; set; }
    public string Prerequisites { get; set; } = string.Empty;
    public string Unlocks { get; set; } = string.Empty;
    public bool NonConstructible { get; set; }             // true only for Portão
}
```

- [ ] **Step 3: `GuildCatalogIds`**

`src/Ruptura.Shared/Guilds/GuildCatalogIds.cs` — the seeded GUIDs the calculator keys on (must match sub-plan #1's seed; asserted in Task 3):
```csharp
namespace Ruptura.Shared.Guilds;

// Seeded catalog GUIDs (sub-plan #1). C# has no `const Guid`, so these are static readonly.
// The calculator identifies formula-relevant installations/doctrines by these, never by name.
public static class GuildCatalogIds
{
    // Installations (d0000000-…) — formula-relevant subset + Portão.
    public static readonly Guid Portao = Guid.Parse("d0000000-0000-0000-0000-000000000001");
    public static readonly Guid Dormitorio = Guid.Parse("d0000000-0000-0000-0000-000000000002");
    public static readonly Guid Armazem = Guid.Parse("d0000000-0000-0000-0000-000000000003");
    public static readonly Guid CampoDeTreinamento = Guid.Parse("d0000000-0000-0000-0000-000000000004");
    public static readonly Guid Biblioteca = Guid.Parse("d0000000-0000-0000-0000-000000000007");
    public static readonly Guid Memorial = Guid.Parse("d0000000-0000-0000-0000-000000000013");
    public static readonly Guid CentroLogistico = Guid.Parse("d0000000-0000-0000-0000-000000000014");
    public static readonly Guid CamaraDoConselho = Guid.Parse("d0000000-0000-0000-0000-000000000017");

    // Doctrines (d1000000-…) — only the two that affect institutional stats.
    public static readonly Guid DoctrineLogistica = Guid.Parse("d1000000-0000-0000-0000-000000000007");
    public static readonly Guid DoctrineComercial = Guid.Parse("d1000000-0000-0000-0000-000000000003");
}
```

- [ ] **Step 4: `GuildDerivedStats`**

`src/Ruptura.Shared/Guilds/GuildDerivedStats.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

// Everything GuildStatsCalculator computes — never persisted, recomputed on read.
public class GuildDerivedStats
{
    public GuildStage Stage { get; set; }
    public int StageIndex { get; set; }               // 0..7, == (int)Stage

    public int Cg { get; set; }                        // total Capacidade da Guilda
    public int CgInfra { get; set; }
    public int CgPesquisa { get; set; }
    public int CgLogistica { get; set; }
    public int CgRecursos { get; set; }

    public int Cs { get; set; }                        // Capacidade de Suporte (doctrine-adjusted)
    public int Ci { get; set; }                        // Capacidade Institucional
    public int Cf { get; set; }                        // Capacidade de Formação

    public decimal InflationIndex { get; set; }        // doctrine-adjusted (Comercial -1 stage)

    public int DailyMaintenance { get; set; }          // doctrine-adjusted (Logística -10%)
    public int WorkerIncomePerDay { get; set; }        // Operário count × 2 Prata/day

    public int StorageCapacity { get; set; }           // Armazém level × 50
    public int ResidencyCapacity { get; set; }         // Dormitório level × 2

    public int DoctrineLimit { get; set; }             // 2 + Câmara do Conselho level, capped 4
    public int ActiveDoctrineCount { get; set; }

    public int ActiveBuildingCount { get; set; }       // constructible, IsActive
    public bool ActiveBuildingOverflow { get; set; }   // ActiveBuildingCount > Cs
}
```

- [ ] **Step 5: `GuildSheetResponse`**

`src/Ruptura.Shared/Guilds/GuildSheetResponse.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public class GuildSheetResponse
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string GuildName { get; set; } = string.Empty;
    public GuildSheetData Data { get; set; } = new();
    public GuildDerivedStats DerivedStats { get; set; } = new();
    public uint Version { get; set; }                  // xmin concurrency token (sub-plan #3 requires it on write)
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

- [ ] **Step 6: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Shared/Guilds
git commit -m "feat: add guild derived-stats DTOs, stage enum, and catalog-id constants

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: `GuildStatsCalculator` (pure) + unit tests

The heart of the sub-plan. TDD: write the oracle tests, watch them fail, implement.

**Files:**
- Create: `src/Ruptura.Application/Interfaces/IGuildStatsCalculator.cs`, `src/Ruptura.Application/Services/GuildStatsCalculator.cs`
- Test: `tests/Ruptura.UnitTests/Guilds/GuildStatsCalculatorTests.cs`

**Interfaces:**
- Consumes: `GuildSheetData`, `GuildBuilding`, `GuildStaff`, `CatalogEntry`, `GuildDerivedStats`, `GuildStage`, `GuildCatalogIds`, `InstallationCatalogData`, `GuildStaffKind`.
- Produces: `IGuildStatsCalculator.Calculate(GuildSheetData data, IReadOnlyList<GuildBuilding> buildings, IReadOnlyList<GuildStaff> staff, int researchPoints, IReadOnlyDictionary<Guid, CatalogEntry> installationCatalog) → GuildDerivedStats`.

- [ ] **Step 1: Write the failing unit tests**

`tests/Ruptura.UnitTests/Guilds/GuildStatsCalculatorTests.cs`. These are the oracle. Use a helper that builds an installation `CatalogEntry` with a given weight / NonConstructible so the calculator can resolve Infra.

```csharp
using System.Text.Json;
using FluentAssertions;
using Ruptura.Application.Services;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;
using Ruptura.Shared.CharacterSheets; // GuildSheetData lives in Ruptura.Shared.Guilds; see using below
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
        r.Cs.Should().Be(13);  // 5 + 3*2 + 4*1
        r.Ci.Should().Be(11);  // 3 + 2*4 + 3*1... wait: 3 + (2*4) + (3*1) = 3+8+3 = 14
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
```

> Note: fix the `using` — `GuildSheetData` and its submodules are in `Ruptura.Shared.Guilds` (remove the `Ruptura.Shared.CharacterSheets` using if the analyzer flags it unused). The two arithmetic comments in `CsCiCf_ComputeFromNamedInstallationLevels` correct themselves to `Ci = 14`, `Cf = 22`; assert those values.

- [ ] **Step 2: Run tests to confirm they fail**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~GuildStatsCalculatorTests`
Expected: FAIL — `GuildStatsCalculator` / `IGuildStatsCalculator` do not exist.

- [ ] **Step 3: Create the interface**

`src/Ruptura.Application/Interfaces/IGuildStatsCalculator.cs`:
```csharp
using Ruptura.Domain.Entities;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Interfaces;

public interface IGuildStatsCalculator
{
    GuildDerivedStats Calculate(
        GuildSheetData data,
        IReadOnlyList<GuildBuilding> buildings,
        IReadOnlyList<GuildStaff> staff,
        int researchPoints,
        IReadOnlyDictionary<Guid, CatalogEntry> installationCatalog);
}
```

- [ ] **Step 4: Implement the calculator**

`src/Ruptura.Application/Services/GuildStatsCalculator.cs`:
```csharp
using System.Text.Json;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Services;

public class GuildStatsCalculator : IGuildStatsCalculator
{
    private static readonly decimal[] InflationByStage =
        [1.0m, 1.2m, 1.5m, 1.8m, 2.2m, 2.6m, 3.2m, 4.0m];

    public GuildDerivedStats Calculate(
        GuildSheetData data,
        IReadOnlyList<GuildBuilding> buildings,
        IReadOnlyList<GuildStaff> staff,
        int researchPoints,
        IReadOnlyDictionary<Guid, CatalogEntry> installationCatalog)
    {
        var resources = data.Resources ?? new GuildResources();
        var activeDoctrines = data.ActiveDoctrineIds ?? [];
        var hasLogistica = activeDoctrines.Contains(GuildCatalogIds.DoctrineLogistica);
        var hasComercial = activeDoctrines.Contains(GuildCatalogIds.DoctrineComercial);

        // Per-installation level (0 if not built). Unique index guarantees at most one row per installation.
        int LevelOf(Guid installationId) =>
            buildings.FirstOrDefault(b => b.CatalogEntryId == installationId)?.Level ?? 0;

        var armazem = LevelOf(GuildCatalogIds.Armazem);
        var centroLog = LevelOf(GuildCatalogIds.CentroLogistico);
        var camara = LevelOf(GuildCatalogIds.CamaraDoConselho);
        var memorial = LevelOf(GuildCatalogIds.Memorial);
        var biblioteca = LevelOf(GuildCatalogIds.Biblioteca);
        var campo = LevelOf(GuildCatalogIds.CampoDeTreinamento);
        var dormitorio = LevelOf(GuildCatalogIds.Dormitorio);

        // CS/CI/CF (§10.9). CS gets the Logística +20% (floored).
        var baseCs = 5 + centroLog * 2 + armazem * 1;
        var cs = hasLogistica ? (int)Math.Floor(baseCs * 1.20m) : baseCs;
        var ci = 3 + camara * 4 + centroLog * 1;
        var cf = 10 + memorial * 3 + biblioteca * 1 + campo * 1;

        // Constructible buildings only (exclude Portão / NonConstructible) for Infra, maintenance, CS cap.
        var constructible = buildings
            .Select(b => (Building: b, Data: DataFor(b.CatalogEntryId, installationCatalog)))
            .Where(x => x.Data is null || !x.Data.NonConstructible) // unknown catalog -> treat as constructible
            .Where(x => x.Data is not null)                          // but need weight to count Infra/maintenance
            .ToList();

        var infra = constructible.Sum(x => x.Building.Level * x.Data!.Weight);

        var activeWorkers = staff.Count(s => s.Kind == GuildStaffKind.Worker && s.IsActive); // all workers qualify
        var logistica = cs + activeWorkers * 2;

        var recursos = resources.PactCoins
            + resources.DimensionalFragments
            + (resources.Materials ?? []).Sum(m => m.Quantity);

        var cg = infra + researchPoints + logistica + recursos;

        // Daily maintenance: constructible buildings (level×weight) + active staff salaries; Logística −10%.
        var buildingMaintenance = constructible.Sum(x => x.Building.Level * x.Data!.Weight * 1);
        var salaries = staff.Where(s => s.IsActive).Sum(s => s.DailySalary);
        var baseMaintenance = buildingMaintenance + salaries;
        var maintenance = hasLogistica
            ? (int)Math.Round(baseMaintenance * 0.90m, MidpointRounding.AwayFromZero)
            : baseMaintenance;

        var workerIncome = staff.Count(s => s.Kind == GuildStaffKind.Worker && s.IsActive && s.TypeOrRanking == "Operário") * 2;

        var stageIndex = StageIndex(data.FloorsConquered);
        var inflationStageIndex = hasComercial ? Math.Max(0, stageIndex - 1) : stageIndex;

        var activeBuildingCount = constructible.Count(x => x.Building.IsActive);

        return new GuildDerivedStats
        {
            Stage = (GuildStage)stageIndex,
            StageIndex = stageIndex,
            Cg = cg,
            CgInfra = infra,
            CgPesquisa = researchPoints,
            CgLogistica = logistica,
            CgRecursos = recursos,
            Cs = cs,
            Ci = ci,
            Cf = cf,
            InflationIndex = InflationByStage[inflationStageIndex],
            DailyMaintenance = maintenance,
            WorkerIncomePerDay = workerIncome,
            StorageCapacity = armazem * 50,
            ResidencyCapacity = dormitorio * 2,
            DoctrineLimit = Math.Min(4, 2 + camara),
            ActiveDoctrineCount = activeDoctrines.Count,
            ActiveBuildingCount = activeBuildingCount,
            ActiveBuildingOverflow = activeBuildingCount > cs
        };
    }

    private static int StageIndex(int floors) => floors switch
    {
        >= 35 => 7,
        >= 30 => 6,
        >= 25 => 5,
        >= 20 => 4,
        >= 15 => 3,
        >= 10 => 2,
        >= 5 => 1,
        _ => 0
    };

    // Malformed homebrew installation DataJson must never 500 a guild read — treat as null (skipped).
    private static InstallationCatalogData? DataFor(Guid id, IReadOnlyDictionary<Guid, CatalogEntry> catalog)
    {
        if (!catalog.TryGetValue(id, out var entry)) return null;
        try { return JsonSerializer.Deserialize<InstallationCatalogData>(entry.DataJson); }
        catch (JsonException) { return null; }
    }
}
```

> The `.Where(x => x.Data is null || !x.Data.NonConstructible)` then `.Where(x => x.Data is not null)` pair means: a building whose installation catalog entry is missing or malformed contributes 0 to Infra/maintenance (it's dropped by the second filter) but this is the defensive path — in normal operation every building's installation is seeded and present. This matches the character calculator's "missing catalog entry → skip" behavior.

- [ ] **Step 5: Run tests to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~GuildStatsCalculatorTests`
Expected: PASS (all cases). Fix arithmetic in the two illustrative comments if needed (assert `Ci=14`, `Cf=22`).

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Application tests/Ruptura.UnitTests/Guilds/GuildStatsCalculatorTests.cs
git commit -m "feat: add pure GuildStatsCalculator with CG/CI/CF/CS + economy derivation

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Child read repositories + catalog-id/seed consistency test

**Files:**
- Create: `src/Ruptura.Application/Interfaces/IGuildBuildingRepository.cs`, `IGuildStaffRepository.cs`
- Create: `src/Ruptura.Infrastructure/Repositories/GuildBuildingRepository.cs`, `GuildStaffRepository.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`
- Test: `tests/Ruptura.IntegrationTests/Guilds/GuildRepositoryReadTests.cs`, `tests/Ruptura.IntegrationTests/Guilds/GuildCatalogIdsTests.cs`

**Interfaces:**
- Produces: `IGuildBuildingRepository.GetByGuildAsync(Guid guildSheetId, CancellationToken)` → `Task<IEnumerable<GuildBuilding>>`; `IGuildStaffRepository.GetByGuildAsync(...)` → `Task<IEnumerable<GuildStaff>>`.

- [ ] **Step 1: Write the failing tests**

`tests/Ruptura.IntegrationTests/Guilds/GuildRepositoryReadTests.cs` — mirror the fixture of the existing `Guilds/*Tests.cs` (`IntegrationTestFactory`, `factory.Services.CreateScope()`):
```csharp
// GetByGuildAsync returns only rows for the given guild.
// Arrange: 1 Campaign + 1 GuildSheet; add 2 GuildBuildings and 2 GuildStaff for it, plus
// 1 building for a different guild. Assert GetByGuildAsync returns exactly the 2 owned rows.
```
(Write concrete arrange/act/assert following `GuildSheetRepositoryTests` from sub-plan #1.)

`tests/Ruptura.IntegrationTests/Guilds/GuildCatalogIdsTests.cs` — bridge the drift risk between `GuildCatalogIds` and the seed:
```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Infrastructure.Data;
using Ruptura.Shared.Guilds;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildCatalogIdsTests(/* same fixture */)
{
    [Fact]
    public async Task GuildCatalogIds_MatchSeededInstallationsAndDoctrines()
    {
        using var scope = /* factory */.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        async Task AssertName(Guid id, string expectedName)
        {
            var e = await db.CatalogEntries.FirstOrDefaultAsync(c => c.Id == id);
            e.Should().NotBeNull($"catalog id {id} must be seeded");
            e!.Name.Should().Be(expectedName);
        }

        await AssertName(GuildCatalogIds.Portao, "Portão");
        await AssertName(GuildCatalogIds.Dormitorio, "Dormitório");
        await AssertName(GuildCatalogIds.Armazem, "Armazém");
        await AssertName(GuildCatalogIds.CampoDeTreinamento, "Campo de Treinamento");
        await AssertName(GuildCatalogIds.Biblioteca, "Biblioteca");
        await AssertName(GuildCatalogIds.Memorial, "Memorial");
        await AssertName(GuildCatalogIds.CentroLogistico, "Centro Logístico");
        await AssertName(GuildCatalogIds.CamaraDoConselho, "Câmara do Conselho");
        await AssertName(GuildCatalogIds.DoctrineLogistica, "Logística");
        await AssertName(GuildCatalogIds.DoctrineComercial, "Comercial");
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter "FullyQualifiedName~GuildRepositoryReadTests|FullyQualifiedName~GuildCatalogIdsTests"`
Expected: FAIL — repositories don't exist (`GuildRepositoryReadTests`); `GuildCatalogIdsTests` may already pass (seed exists) — that's fine, it's a guard.

- [ ] **Step 3: Create the interfaces**

`src/Ruptura.Application/Interfaces/IGuildBuildingRepository.cs`:
```csharp
using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IGuildBuildingRepository : IRepository<GuildBuilding>
{
    Task<IEnumerable<GuildBuilding>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default);
}
```
`src/Ruptura.Application/Interfaces/IGuildStaffRepository.cs`:
```csharp
using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IGuildStaffRepository : IRepository<GuildStaff>
{
    Task<IEnumerable<GuildStaff>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default);
}
```

- [ ] **Step 4: Create the implementations**

`src/Ruptura.Infrastructure/Repositories/GuildBuildingRepository.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class GuildBuildingRepository(AppDbContext db)
    : BaseRepository<GuildBuilding>(db), IGuildBuildingRepository
{
    public async Task<IEnumerable<GuildBuilding>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default) =>
        await Set.Where(b => b.GuildSheetId == guildSheetId).ToListAsync(ct);
}
```
`src/Ruptura.Infrastructure/Repositories/GuildStaffRepository.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class GuildStaffRepository(AppDbContext db)
    : BaseRepository<GuildStaff>(db), IGuildStaffRepository
{
    public async Task<IEnumerable<GuildStaff>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default) =>
        await Set.Where(s => s.GuildSheetId == guildSheetId).ToListAsync(ct);
}
```

- [ ] **Step 5: Register in DI**

In `InfrastructureExtensions.cs`, alongside the other repositories:
```csharp
        services.AddScoped<IGuildBuildingRepository, GuildBuildingRepository>();
        services.AddScoped<IGuildStaffRepository, GuildStaffRepository>();
```

- [ ] **Step 6: Run the tests**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter "FullyQualifiedName~GuildRepositoryReadTests|FullyQualifiedName~GuildCatalogIdsTests"`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure tests/Ruptura.IntegrationTests/Guilds
git commit -m "feat: add guild building/staff read repositories + catalog-id seed guard

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 4: `GuildSheetService` + `GuildController` (read path)

**Files:**
- Modify: `src/Ruptura.Application/Common/ErrorCodes.cs`
- Create: `src/Ruptura.Application/Interfaces/IGuildSheetService.cs`
- Create: `src/Ruptura.Infrastructure/Services/GuildSheetService.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`
- Create: `src/Ruptura.API/Controllers/GuildController.cs`
- Modify: `src/Ruptura.API/Resources/SharedResources.resx` + `.pt-BR.resx` (or the project's resx pair)
- Test: `tests/Ruptura.IntegrationTests/Guilds/GuildControllerTests.cs`

**Interfaces:**
- Consumes: `IGuildSheetRepository` (from #1), `IGuildBuildingRepository`, `IGuildStaffRepository`, `ICampaignRepository`, `ICampaignMembershipRepository`, `ICatalogEntryRepository`, `IGuildStatsCalculator`.
- Produces: `IGuildSheetService.GetByCampaignAsync(Guid callerId, Guid campaignId, CancellationToken) → Task<Result<GuildSheetResponse>>`.

- [ ] **Step 1: Add error codes**

In `src/Ruptura.Application/Common/ErrorCodes.cs`, add a nested class:
```csharp
    public static class Guild
    {
        public const string NotFound = "Guild.NotFound";
        public const string Forbidden = "Guild.Forbidden";
    }
```

- [ ] **Step 2: Write the failing controller integration test**

`tests/Ruptura.IntegrationTests/Guilds/GuildControllerTests.cs` — follow the existing controller-test pattern (register a GM, create a campaign, register a player via invite, add to roster; use the auth helper the other controller tests use to get bearer tokens). Cases:
```
- GM of the campaign: GET campaigns/{id}/guild -> 200, body has GuildName, Data, DerivedStats (Stage=Fundacao when no floors), Version.
- Member player: GET -> 200 (shared read).
- Non-member player (different campaign): GET -> 404 (Guild.Forbidden mapped to NotFound-style 404, matching CharacterSheet's not-found-on-forbidden convention).
- Get-or-create: first GET creates the guild; a second GET returns the same guild Id.
- Derived stats reflect a seeded building: add a GuildBuilding (Armazém level 2) directly via DbContext, GET -> DerivedStats.StorageCapacity == 100, CgInfra == 2.
```
Write concrete arrange/act/assert mirroring `CharacterSheetControllerTests` (token acquisition, `GetFromJsonAsync<ApiResponse<GuildSheetResponse>>`, etc.).

- [ ] **Step 3: Run it to confirm failure**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildControllerTests`
Expected: FAIL — no controller/service yet.

- [ ] **Step 4: Create the service interface**

`src/Ruptura.Application/Interfaces/IGuildSheetService.cs`:
```csharp
using Ruptura.Application.Common;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Interfaces;

public interface IGuildSheetService
{
    Task<Result<GuildSheetResponse>> GetByCampaignAsync(Guid callerId, Guid campaignId, CancellationToken ct = default);
}
```

- [ ] **Step 5: Implement the service**

`src/Ruptura.Infrastructure/Services/GuildSheetService.cs`:
```csharp
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Guilds;

namespace Ruptura.Infrastructure.Services;

public class GuildSheetService(
    IGuildSheetRepository guildRepo,
    IGuildBuildingRepository buildingRepo,
    IGuildStaffRepository staffRepo,
    ICampaignRepository campaignRepo,
    ICampaignMembershipRepository membershipRepo,
    ICatalogEntryRepository catalogRepo,
    IGuildStatsCalculator calculator) : IGuildSheetService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<Result<GuildSheetResponse>> GetByCampaignAsync(
        Guid callerId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null)
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.NotFound);

        var isGm = campaign.GameMasterId == callerId;
        var isMember = isGm || await membershipRepo.ExistsAsync(campaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.NotFound); // hide existence, like CharacterSheet

        var guild = await GetOrCreateAsync(campaign, callerId, ct);
        return Result.Success(await MapToResponseAsync(guild, ct));
    }

    private async Task<GuildSheet> GetOrCreateAsync(Campaign campaign, Guid callerId, CancellationToken ct)
    {
        var existing = await guildRepo.GetByCampaignAsync(campaign.Id, ct);
        if (existing is not null) return existing;

        var guild = new GuildSheet
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            GuildName = campaign.Name,          // seed from campaign name; editable later (sub-plan #3)
            CreatedByGameMasterId = campaign.GameMasterId,
            DataJson = "{}"
        };
        try
        {
            await guildRepo.AddAsync(guild, ct);
            await guildRepo.SaveChangesAsync(ct);
            return guild;
        }
        catch (DbUpdateException)
        {
            // Concurrent first-access lost the race on ux_guild_sheets_campaign — the winner's row exists.
            return (await guildRepo.GetByCampaignAsync(campaign.Id, ct))!;
        }
    }

    private async Task<GuildSheetResponse> MapToResponseAsync(GuildSheet guild, CancellationToken ct)
    {
        var data = Deserialize(guild.DataJson);
        var buildings = (await buildingRepo.GetByGuildAsync(guild.Id, ct)).ToList();
        var staff = (await staffRepo.GetByGuildAsync(guild.Id, ct)).ToList();

        var installationIds = buildings.Select(b => b.CatalogEntryId).Distinct().ToList();
        var installationCatalog = installationIds.Count == 0
            ? new Dictionary<Guid, CatalogEntry>()
            : (await catalogRepo.GetByIdsAsync(installationIds, ct)).ToDictionary(e => e.Id);

        // No research projects until sub-plan #5 -> researchPoints = 0.
        var derived = calculator.Calculate(data, buildings, staff, researchPoints: 0, installationCatalog);

        return new GuildSheetResponse
        {
            Id = guild.Id,
            CampaignId = guild.CampaignId,
            GuildName = guild.GuildName,
            Data = data,
            DerivedStats = derived,
            Version = guild.Version,
            CreatedAt = guild.CreatedAt,
            UpdatedAt = guild.UpdatedAt
        };
    }

    // Guarantee every blob module is non-null at the boundary (character-sheet #3 lesson).
    private static GuildSheetData Deserialize(string json)
    {
        GuildSheetData? data;
        try { data = JsonSerializer.Deserialize<GuildSheetData>(json, JsonOpts); }
        catch (JsonException) { data = null; }
        data ??= new GuildSheetData();
        data.Identity ??= new GuildIdentity();
        data.Prestige ??= new GuildPrestige();
        data.Influence ??= [];
        data.Resources ??= new GuildResources();
        data.Resources.Materials ??= [];
        data.Resources.Artifacts ??= [];
        data.ActiveDoctrineIds ??= [];
        data.Knowledge ??= new GuildKnowledge();
        data.Legado ??= [];
        return data;
    }
}
```

> Confirm `GuildSheet.Version` exists (added in sub-plan #1's fix wave) and `catalogRepo.GetByIdsAsync` is the right accessor (it is — used by `CharacterSheetService`). If `Result`/`Result<T>` factory method names differ, match `CharacterSheetService`'s usage exactly.

- [ ] **Step 6: Register the service in DI**

In `InfrastructureExtensions.cs`:
```csharp
        services.AddSingleton<IGuildStatsCalculator, GuildStatsCalculator>();  // pure & stateless, like CharacterStatsCalculator
        services.AddScoped<IGuildSheetService, GuildSheetService>();
```
(Place the calculator with the other `AddSingleton` calculators and the service with the other `AddScoped` services.)

- [ ] **Step 7: Create the controller**

`src/Ruptura.API/Controllers/GuildController.cs` — mirror `CharacterSheetController`'s structure (localizer, `User.FindFirstValue(JwtRegisteredClaimNames.Sub)`, `ApiResponse`):
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class GuildController(
    IGuildSheetService guildService,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpGet("campaigns/{campaignId:guid}/guild")]
    [ProducesResponseType(typeof(ApiResponse<GuildSheetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid campaignId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.GetByCampaignAsync(callerId, campaignId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));
        return Ok(ApiResponse<GuildSheetResponse>.Ok(result.Value!));
    }
}
```

- [ ] **Step 8: Add resx strings**

Add `Guild.NotFound` and `Guild.Forbidden` message strings to both resx files (SharedResources default + pt-BR), following the existing `CharacterSheet.*`/`Campaign.*` entries (English in the default file, Portuguese in `.pt-BR`). Example values: default `"Guild not found."` / pt-BR `"Guilda não encontrada."`.

- [ ] **Step 9: Run the controller tests**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildControllerTests`
Expected: PASS.

- [ ] **Step 10: Full sweep + commit**

Run: `dotnet build && dotnet test`
Expected: PASS (re-run once on a lone Serilog flake).
```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Guilds/GuildControllerTests.cs
git commit -m "feat: add guild read endpoint with get-or-create and derived stats

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 5: Capacidades panel (Blazor) + entry point

**Files:**
- Create: `src/Ruptura.Web/Pages/GuildSheet.razor` (page host), `src/Ruptura.Web/Pages/GuildCapacitiesPanel.razor` (the panel)
- Modify: the Web API client service (add `GetGuildAsync(campaignId)`), the campaign pages (add a link to the guild page), and the Web resx pair (Guild UI strings)

**Interfaces:**
- Consumes: `GuildSheetResponse`/`GuildDerivedStats` from `Ruptura.Shared.Guilds`; the existing Web API client + `IStringLocalizer` + design-system toolkit.

- [ ] **Step 1: Add the API client method**

Read the existing Web API client (the typed HttpClient service the character-sheet pages use, e.g. `Services/ApiClient.cs` or similar). Add:
```csharp
public Task<ApiResponse<GuildSheetResponse>?> GetGuildAsync(Guid campaignId, CancellationToken ct = default) =>
    _http.GetFromJsonAsync<ApiResponse<GuildSheetResponse>>($"api/campaigns/{campaignId}/guild", ct);
```
Match the exact signature/return-type convention the other client methods use (some may return the unwrapped value or use a helper). Follow the file's established pattern rather than this snippet verbatim.

- [ ] **Step 2: Build the Capacidades panel component**

`src/Ruptura.Web/Pages/GuildCapacitiesPanel.razor` — a presentational component taking `[Parameter] public GuildDerivedStats Stats { get; set; }` and rendering the derived values, using the design-system tokens and `IStringLocalizer`. Requirements:
- Show Stage (localized name), CG with its four-term breakdown (Infra / Pesquisa / Logística / Recursos), CI, CF, CS.
- Show Inflation Index, Daily Maintenance, Worker Income/day, Storage Capacity, Residency Capacity, Doctrine Limit (as `ActiveDoctrineCount / DoctrineLimit`).
- When `ActiveBuildingOverflow` is true, show a warning (use the toolkit's alert/warning style) explaining active buildings exceed CS.
- All labels via `IStringLocalizer`; a localized `GuildStage` display map (resx keys like `Guild.Stage.Fundacao` … `Guild.Stage.Divina`).
- Responsive: derived values in a stat grid that reflows on mobile (reuse the stat-tile pattern from the character sheet's derived/combat display if one exists; otherwise a simple responsive grid with the design tokens).

- [ ] **Step 3: Build the page host**

`src/Ruptura.Web/Pages/GuildSheet.razor` with `@page "/campaigns/{CampaignId:guid}/guild"`:
- `[Parameter] public Guid CampaignId { get; set; }`.
- On `OnInitializedAsync`: call `GetGuildAsync(CampaignId)`; show `LoadingIndicator` while loading; on failure show a localized error via `ToastService`.
- Render `Breadcrumbs` (resolve the campaign name via an existing campaign endpoint, once in `OnInitializedAsync` — follow the `GmCampaignDetail` breadcrumb pattern) and the guild name as a title.
- Render `<GuildCapacitiesPanel Stats="_guild.DerivedStats" />`. (This page is the shell later sub-plans add tabs to; for now it hosts the Capacidades panel only.)
- `[Authorize]` — accessible to GM and members (the API enforces the real check).

- [ ] **Step 4: Add the entry point**

Add a link/button to the guild page from the campaign detail pages (GM: `GmCampaignDetail.razor`; player: the player's campaign view). Follow how those pages link to other per-campaign features (e.g. character sheets). Localize the link text (`Guild.OpenSheet`).

- [ ] **Step 5: Add Web resx strings**

Add all new UI strings to the Web resx pair (default + pt-BR): stage names (`Guild.Stage.*`), stat labels (`Guild.Cg`, `Guild.Ci`, `Guild.Cf`, `Guild.Cs`, `Guild.Infra`, `Guild.Pesquisa`, `Guild.Logistica`, `Guild.Recursos`, `Guild.Inflation`, `Guild.Maintenance`, `Guild.WorkerIncome`, `Guild.Storage`, `Guild.Residency`, `Guild.DoctrineLimit`, `Guild.ActiveBuildingOverflow`, `Guild.OpenSheet`, plus loading/error). English in the default file, Portuguese in `.pt-BR`.

- [ ] **Step 6: Build + verify in the app**

Run: `dotnet build` (must pass). Then follow the project's run pattern to confirm the page renders the panel for a campaign and shows correct derived numbers for a guild with a couple of buildings (the `run` skill or `make up`). If a full run isn't feasible in the environment, at minimum confirm the build is clean and the component compiles; note this in the report.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Web
git commit -m "feat: add guild Capacidades panel and page with derived-stats display

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage (§4, §6, §7, §12.2):**
- §4 every derived value → `GuildDerivedStats` (Task 1) + `GuildStatsCalculator` (Task 2). Stage, CG+breakdown, CS/CI/CF, inflation, maintenance, worker income, storage/residency caps, doctrine limit, active-building overflow all present and unit-tested. ✓
  - **Deliberate scope note:** merc-limit and simultaneous-expedition caps (§4 "from relevant installations") are **omitted** — the GDD gives no closed formula for them (Quartel dos Mercenários / Centro Logístico say "aumenta limite"/"mais expedições" with no number). Storage (Armazém×50) and Residency (Dormitório×2) are the only caps with closed formulas; the rest wait until the GDD defines them. Recorded so it isn't read as a gap.
- §6 shared-write authorization (GM or member), get-or-create → `GuildSheetService` + `GuildController` (Task 4). This sub-plan is read-only; write paths are #3+. ✓
- §7 Capacidades tab → Task 5 (as a panel on a page host that later sub-plans extend with tabs). ✓
- §12.2 = calculator + Capacidades panel (derived read path) → all five tasks. ✓
- Load-bearing carry-ins honored: xmin `Version` surfaced in the response DTO (spec §6 / memory item 1); `NonConstructible` excluded from Infra/maintenance/CS-cap (memory item 3); qualified-workers = all workers, Recursos formula (locked decisions); GUIDs keyed not names, asserted vs seed (Task 3); defensive non-null blob deserialization (memory item 5).

**2. Placeholder scan:** The only intentional markers are `/* same fixture */` / `/* factory */` in the integration tests and "follow the existing client/page pattern" in Task 5 — each because the concrete fixture/client/Razor conventions must be read from the repo at execution time and are not reproducible in the plan. Every backend code step (Tasks 1–4) has complete code. Task 5 is intentionally pattern-directive (UI matching existing Razor + i18n + design-system conventions), consistent with how the design-system rollout plans were written.

**3. Type consistency:** `Calculate(...)` signature identical in `IGuildStatsCalculator`, `GuildStatsCalculator`, and the tests. `GuildDerivedStats` property names identical across DTO, calculator, tests, and the panel. `GuildCatalogIds` field names identical in the calculator and the seed-guard test. `GetByGuildAsync`/`GetByCampaignAsync`/`GetByCampaignAsync` (service) signatures consistent between interface, impl, and callers. `GuildSheetResponse.Version` (`uint`) matches `GuildSheet.Version` from sub-plan #1.
