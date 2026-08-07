# Guild Sheet — Foundation (Sub-plan #1) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Establish the guild sheet's data layer — the modified `GuildSheet` entity (1:1 with `Campaign`), five child entities, the `GuildSheetData` blob DTOs, two new catalog types with their official seed (20 installations + 8 doctrines), and a `GuildSheet` repository — with migrations and integration tests proving the schema and seed.

**Architecture:** Hybrid persistence: `GuildSheet.DataJson` holds a stable-modules blob (`Ruptura.Shared.Guilds.GuildSheetData`); high-churn, addressable lists (buildings, staff, research, crafting, expeditions) are dedicated child tables with `ON DELETE CASCADE` back to `GuildSheet`. Installations and doctrines reuse the existing `CatalogEntry` infrastructure via two new `CatalogEntryType` values. This sub-plan delivers **only the data layer** — the `GuildStatsCalculator`, services, controllers, and UI arrive in later sub-plans.

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, xUnit + FluentAssertions + Testcontainers.PostgreSql. Clean Architecture (Domain ← Application ← Infrastructure ← API; Shared referenced by API + Web).

**Spec:** `docs/superpowers/specs/2026-08-07-guild-sheet-design.md` (§3 Persistence, §10 Data-Model Impact, §11 Open Items, §12.1 ordering).

## Global Constraints

- **Clean Architecture dependency rule** — Domain has no framework deps; entities live in `Ruptura.Domain/Entities`, enums in `Ruptura.Domain/Enums`; EF config lives in `Ruptura.Infrastructure/Data/Configurations`; DTOs shared between API and Web live in `Ruptura.Shared`.
- **Repository pattern** — repositories implement an interface in `Ruptura.Application/Interfaces` and derive from `BaseRepository<T>`; register in `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`.
- **Soft-reference convention, with real FKs only where cascade/uniqueness matters** — `GuildSheet.CampaignId` gets a real FK + unique index (enforces 1:1); `GuildSheet.CreatedByGameMasterId` stays a bare `Guid` (soft, matches `Campaign.GameMasterId`). Child entities get real FKs to `GuildSheet` with `ON DELETE CASCADE`.
- **Enums stored as strings** — every guild enum column is configured `.HasConversion<string>()` so the DB stores readable names and appending/reordering enum members stays safe. `CatalogEntryType` keeps its existing (default int) storage — new members are **appended** to the end, never inserted.
- **Accented values that can't be C# identifiers stay plain `string`** (precedent: `CharacterGuildRegistry.Ranking`) — `ResearchProject.ResearchType` and `CraftingOrder.Quality` are `string` with documented valid values; enum members never carry accents (`Secundaria`, `Concluido` — no accent in the identifier).
- **Seed determinism** — seed rows use `CatalogSeedData.Entry(...)` with a fixed `SeedTimestamp` and hard-coded GUIDs; never `DateTime.UtcNow` or `Guid.NewGuid()` in seed data (breaks migration diffs).
- **Migration commands** (run from repo root):
  ```bash
  dotnet ef migrations add <Name> --project src/Ruptura.Infrastructure --startup-project src/Ruptura.API
  dotnet ef database update --project src/Ruptura.Infrastructure --startup-project src/Ruptura.API
  ```
- **Integration tests** use `WebApplicationFactory<Program>` + Testcontainers.PostgreSql and run with `parallelizeTestCollections: false` (already configured). A single flaky Serilog "logger already frozen" failure is a known pre-existing race — re-run once before assuming a regression.
- **Commit after each task.** End commit messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`. Work on `main` (this project's established workflow for these sub-plans).

## File Structure

**Create:**
- `src/Ruptura.Domain/Enums/GuildStaffKind.cs`, `ResearchComplexity.cs`, `ResearchStage.cs`, `CraftingCategory.cs`, `CraftingStatus.cs`, `ExpeditionKind.cs`
- `src/Ruptura.Domain/Entities/GuildBuilding.cs`, `GuildStaff.cs`, `ResearchProject.cs`, `CraftingOrder.cs`, `Expedition.cs`
- `src/Ruptura.Shared/Guilds/GuildSheetData.cs` (blob + submodule DTOs)
- `src/Ruptura.Infrastructure/Data/Configurations/GuildSheetConfiguration.cs`, `GuildBuildingConfiguration.cs`, `GuildStaffConfiguration.cs`, `ResearchProjectConfiguration.cs`, `CraftingOrderConfiguration.cs`, `ExpeditionConfiguration.cs`
- `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Installations.cs`, `CatalogSeedData.Doctrines.cs`
- `src/Ruptura.Application/Interfaces/IGuildSheetRepository.cs`
- `src/Ruptura.Infrastructure/Repositories/GuildSheetRepository.cs`
- `tests/Ruptura.IntegrationTests/Guilds/GuildSchemaTests.cs`, `GuildSeedTests.cs`, `GuildSheetRepositoryTests.cs`

**Modify:**
- `src/Ruptura.Domain/Enums/CatalogEntryType.cs` (append `Installation`, `Doctrine`)
- `src/Ruptura.Domain/Entities/GuildSheet.cs` (add `CampaignId`, `RowVersion`; remove `Memberships`)
- `src/Ruptura.Infrastructure/Data/AppDbContext.cs` (add child `DbSet`s; remove `GuildMemberships`)
- `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs` (register two new `HasData` calls)
- `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` (register `IGuildSheetRepository`)

**Delete:**
- `src/Ruptura.Domain/Entities/GuildMembership.cs`

---

### Task 1: Domain model + Shared blob DTOs

Pure POCOs/enums with no behavior — folded into one task, verified by a green build. Later tasks (EF config, seed, repo) depend on these names/types.

**Files:**
- Create: the six enum files, the five entity files, and `src/Ruptura.Shared/Guilds/GuildSheetData.cs`
- Modify: `src/Ruptura.Domain/Enums/CatalogEntryType.cs`, `src/Ruptura.Domain/Entities/GuildSheet.cs`
- Delete: `src/Ruptura.Domain/Entities/GuildMembership.cs`

**Interfaces:**
- Produces: `CatalogEntryType.Installation`, `CatalogEntryType.Doctrine`; entities `GuildSheet` (with `CampaignId: Guid`, `RowVersion: byte[]`, `DataJson: string`), `GuildBuilding`, `GuildStaff`, `ResearchProject`, `CraftingOrder`, `Expedition`; enums `GuildStaffKind`, `ResearchComplexity`, `ResearchStage`, `CraftingCategory`, `CraftingStatus`, `ExpeditionKind`; DTO `Ruptura.Shared.Guilds.GuildSheetData` and submodules.

- [ ] **Step 1: Append the two new catalog types**

Edit `src/Ruptura.Domain/Enums/CatalogEntryType.cs` — append after `EquipmentItem` (append only; do not reorder — values are stored as int):

```csharp
public enum CatalogEntryType
{
    Origin,
    Background,
    Lineage,
    Aptitude,
    Talent,
    Skill,
    Spell,
    Technique,
    EquipmentItem,
    Installation,
    Doctrine
}
```

- [ ] **Step 2: Create the guild enums**

`src/Ruptura.Domain/Enums/GuildStaffKind.cs`:
```csharp
namespace Ruptura.Domain.Enums;

public enum GuildStaffKind
{
    Worker,
    Mercenary
}
```

`src/Ruptura.Domain/Enums/ResearchComplexity.cs`:
```csharp
namespace Ruptura.Domain.Enums;

// GDD §11.2 research tiers: base required days 5 / 10 / 20 / 40.
public enum ResearchComplexity
{
    Menor,
    Moderada,
    Maior,
    Suprema
}
```

`src/Ruptura.Domain/Enums/ResearchStage.cs`:
```csharp
namespace Ruptura.Domain.Enums;

// GDD §11.2 workflow: Descobrir -> Pesquisar -> Dominar -> Aplicar.
public enum ResearchStage
{
    Descobrir,
    Pesquisar,
    Dominar,
    Aplicar
}
```

`src/Ruptura.Domain/Enums/CraftingCategory.cs`:
```csharp
namespace Ruptura.Domain.Enums;

public enum CraftingCategory
{
    Forja,
    Alquimia,
    Encantamento,
    Engenharia,
    Artefatos
}
```

`src/Ruptura.Domain/Enums/CraftingStatus.cs`:
```csharp
namespace Ruptura.Domain.Enums;

// "Concluido" has no accent so it is a valid C# identifier; UI localizes the display.
public enum CraftingStatus
{
    EmAndamento,
    Concluido,
    Cancelado
}
```

`src/Ruptura.Domain/Enums/ExpeditionKind.cs`:
```csharp
namespace Ruptura.Domain.Enums;

// "Secundaria" has no accent so it is a valid C# identifier; UI localizes the display.
public enum ExpeditionKind
{
    Principal,
    Secundaria
}
```

- [ ] **Step 3: Modify the `GuildSheet` entity**

Replace `src/Ruptura.Domain/Entities/GuildSheet.cs`:
```csharp
namespace Ruptura.Domain.Entities;

public class GuildSheet
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }              // real FK + unique index (1 guild per campaign)
    public string GuildName { get; set; } = string.Empty;
    public Guid CreatedByGameMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Stable, low-churn modules (Identity, Prestige, Influence, Resources, active
    // doctrines, Knowledge, Legado, FloorsConquered) — see Ruptura.Shared.Guilds.GuildSheetData.
    // High-churn lists live in dedicated child tables, not here.
    public string DataJson { get; set; } = "{}";

    // Optimistic concurrency for the blob under shared write (GM + all campaign members).
    public byte[] RowVersion { get; set; } = [];
}
```

- [ ] **Step 4: Delete the obsolete `GuildMembership` entity**

```bash
git rm src/Ruptura.Domain/Entities/GuildMembership.cs
```
Membership is now derived from `CampaignMembership` (spec decision 3).

- [ ] **Step 5: Create the child entities**

`src/Ruptura.Domain/Entities/GuildBuilding.cs`:
```csharp
namespace Ruptura.Domain.Entities;

// One built installation. CatalogEntryId references a CatalogEntry of type Installation.
public class GuildBuilding
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public Guid CatalogEntryId { get; set; }
    public int Level { get; set; } = 1;
    public bool IsActive { get; set; } = true; // CS caps active buildings (§10.9)
}
```

`src/Ruptura.Domain/Entities/GuildStaff.cs`:
```csharp
using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class GuildStaff
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public GuildStaffKind Kind { get; set; }
    public string TypeOrRanking { get; set; } = string.Empty; // worker type or merc ranking
    public string Name { get; set; } = string.Empty;
    public int DailySalary { get; set; }                      // pre-filled from GDD default, overridable
    public bool IsActive { get; set; } = true;
    public int? Efficiency { get; set; }                      // workers only, optional
    public int? Morale { get; set; }                          // workers only, optional
}
```

`src/Ruptura.Domain/Entities/ResearchProject.cs`:
```csharp
using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class ResearchProject
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResearchType { get; set; } = string.Empty;  // Arcana|Biológica|Tecnológica|Dimensional|Histórica|Militar
    public ResearchComplexity Complexity { get; set; }
    public ResearchStage Stage { get; set; } = ResearchStage.Descobrir;
    public int ProgressDays { get; set; }
    public int RequiredDays { get; set; }                     // from complexity tier (5/10/20/40)
    public int Researchers { get; set; } = 1;                 // splits time, floor 50% of base
    public int Points { get; set; }                           // awarded to CG's Pesquisa term on completion
    public bool IsComplete { get; set; }
}
```

`src/Ruptura.Domain/Entities/CraftingOrder.cs`:
```csharp
using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class CraftingOrder
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public CraftingCategory Category { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;      // Comum|Superior|Raro|Épico|Lendário|Divino
    public int ProgressDays { get; set; }
    public int RequiredDays { get; set; }
    public CraftingStatus Status { get; set; } = CraftingStatus.EmAndamento;
}
```

`src/Ruptura.Domain/Entities/Expedition.cs`:
```csharp
using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class Expedition
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public ExpeditionKind Kind { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Participants { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Losses { get; set; } = string.Empty;
    public string ResourcesGained { get; set; } = string.Empty;
}
```

- [ ] **Step 6: Create the `GuildSheetData` blob DTOs**

`src/Ruptura.Shared/Guilds/GuildSheetData.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

// Stable, low-churn modules stored in GuildSheet.DataJson. High-churn lists
// (buildings, staff, research, crafting, expeditions) are child entities, not here.
public class GuildSheetData
{
    public GuildIdentity Identity { get; set; } = new();
    public GuildPrestige Prestige { get; set; } = new();
    public List<InfluenceRelation> Influence { get; set; } = [];
    public GuildResources Resources { get; set; } = new();
    public List<Guid> ActiveDoctrineIds { get; set; } = [];   // Doctrine catalog refs, <= derived limit
    public GuildKnowledge Knowledge { get; set; } = new();
    public List<LegacyFeat> Legado { get; set; } = [];
    public int FloorsConquered { get; set; }                  // drives Guild Stage (§10.8)
}

public class GuildIdentity
{
    public string EmblemImagePath { get; set; } = string.Empty;
    public string PatronDeity { get; set; } = string.Empty;
    public Guid? MainDoctrineId { get; set; }                 // Doctrine catalog ref
    public DateTime? FoundingDate { get; set; }
    // 8 GDD ranks: "Bronze"|"Ferro"|"Aço"|"Prata"|"Ouro"|"Mithril"|"Adamante"|"Lendário".
    public string GuildRanking { get; set; } = "Bronze";
}

public class GuildPrestige
{
    public int Value { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class InfluenceRelation
{
    public string Name { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;         // Cidade|Facção|Guilda|Divindade
    public int Reputation { get; set; }                      // -100..100
    public string Notes { get; set; } = string.Empty;
}

public class GuildResources
{
    public int Silver { get; set; }
    public int PactCoins { get; set; }
    public List<MaterialStock> Materials { get; set; } = [];
    public int DimensionalFragments { get; set; }
    public List<string> Artifacts { get; set; } = [];
    public string StrategicReserveNotes { get; set; } = string.Empty;
}

public class MaterialStock
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class GuildKnowledge
{
    public List<string> Maps { get; set; } = [];
    public List<string> Recipes { get; set; } = [];
    public List<string> CataloguedEnemies { get; set; } = [];
    public List<string> DefeatedBosses { get; set; } = [];
    public List<string> HistoricalRecords { get; set; } = [];
}

public class LegacyFeat
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PermanentBenefit { get; set; } = string.Empty;
}
```

- [ ] **Step 7: Verify the solution still builds**

Run: `dotnet build`
Expected: build succeeds. `AppDbContext` still references `GuildMembership` (removed in Task 1) — it will FAIL to compile until Task 2 fixes the `DbSet`. **If the build fails only on `AppDbContext.cs` referencing `GuildMembership`/`GuildMemberships`, that is expected — proceed to Task 2 and re-run the build there.** Any other error must be fixed now.

- [ ] **Step 8: Commit**

```bash
git add src/Ruptura.Domain src/Ruptura.Shared
git commit -m "feat: add guild sheet domain model and blob DTOs

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: EF configuration, DbContext wiring, and schema migration

Deliverable: the schema exists and an integration test proves the tables, the 1:1 unique index, and cascade delete. No seed yet (Task 3).

**Files:**
- Create: `GuildSheetConfiguration.cs`, `GuildBuildingConfiguration.cs`, `GuildStaffConfiguration.cs`, `ResearchProjectConfiguration.cs`, `CraftingOrderConfiguration.cs`, `ExpeditionConfiguration.cs` (all in `src/Ruptura.Infrastructure/Data/Configurations/`)
- Modify: `src/Ruptura.Infrastructure/Data/AppDbContext.cs`
- Create: `tests/Ruptura.IntegrationTests/Guilds/GuildSchemaTests.cs`
- Migration: `src/Ruptura.Infrastructure/Data/Migrations/*_AddGuildSheetTables.cs` (generated)

**Interfaces:**
- Consumes: all entities/enums from Task 1.
- Produces: `AppDbContext.GuildSheets`, `.GuildBuildings`, `.GuildStaff`, `.ResearchProjects`, `.CraftingOrders`, `.Expeditions`; DB tables with the constraints below.

- [ ] **Step 1: Wire the DbContext**

In `src/Ruptura.Infrastructure/Data/AppDbContext.cs`, remove the `GuildMemberships` set and add the child sets:
```csharp
    public DbSet<GuildSheet> GuildSheets => Set<GuildSheet>();
    public DbSet<GuildBuilding> GuildBuildings => Set<GuildBuilding>();
    public DbSet<GuildStaff> GuildStaff => Set<GuildStaff>();
    public DbSet<ResearchProject> ResearchProjects => Set<ResearchProject>();
    public DbSet<CraftingOrder> CraftingOrders => Set<CraftingOrder>();
    public DbSet<Expedition> Expeditions => Set<Expedition>();
```
Delete the line `public DbSet<GuildMembership> GuildMemberships => Set<GuildMembership>();`.

- [ ] **Step 2: Create `GuildSheetConfiguration`**

`src/Ruptura.Infrastructure/Data/Configurations/GuildSheetConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class GuildSheetConfiguration : IEntityTypeConfiguration<GuildSheet>
{
    public void Configure(EntityTypeBuilder<GuildSheet> builder)
    {
        // 1 guild per campaign — enforced at the DB level (unlike CharacterSheet.CampaignId,
        // which is a soft reference; here the 1:1 invariant must hold).
        builder.HasIndex(g => g.CampaignId)
            .IsUnique()
            .HasDatabaseName("ux_guild_sheets_campaign");

        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(g => g.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optimistic concurrency for the blob under shared write.
        builder.Property(g => g.RowVersion).IsRowVersion();
    }
}
```

> **Npgsql note:** `.IsRowVersion()` maps to a PostgreSQL `xmin` system column via a shadow concurrency token. EF Core's Npgsql provider supports this by mapping the CLR `byte[] RowVersion` to `xmin`; the generated migration adds no physical column but marks the concurrency token. If the generated migration instead tries to add a real `RowVersion bytea` column, that is also acceptable — either mapping enforces optimistic concurrency. Do not hand-edit the generated migration.

- [ ] **Step 3: Create the five child configurations**

`GuildBuildingConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class GuildBuildingConfiguration : IEntityTypeConfiguration<GuildBuilding>
{
    public void Configure(EntityTypeBuilder<GuildBuilding> builder)
    {
        builder.HasIndex(b => b.GuildSheetId);
        builder.HasOne<GuildSheet>()
            .WithMany()
            .HasForeignKey(b => b.GuildSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

`GuildStaffConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class GuildStaffConfiguration : IEntityTypeConfiguration<GuildStaff>
{
    public void Configure(EntityTypeBuilder<GuildStaff> builder)
    {
        builder.HasIndex(s => s.GuildSheetId);
        builder.Property(s => s.Kind).HasConversion<string>();
        builder.HasOne<GuildSheet>()
            .WithMany()
            .HasForeignKey(s => s.GuildSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

`ResearchProjectConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class ResearchProjectConfiguration : IEntityTypeConfiguration<ResearchProject>
{
    public void Configure(EntityTypeBuilder<ResearchProject> builder)
    {
        builder.HasIndex(p => p.GuildSheetId);
        builder.Property(p => p.Complexity).HasConversion<string>();
        builder.Property(p => p.Stage).HasConversion<string>();
        builder.HasOne<GuildSheet>()
            .WithMany()
            .HasForeignKey(p => p.GuildSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

`CraftingOrderConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CraftingOrderConfiguration : IEntityTypeConfiguration<CraftingOrder>
{
    public void Configure(EntityTypeBuilder<CraftingOrder> builder)
    {
        builder.HasIndex(o => o.GuildSheetId);
        builder.Property(o => o.Category).HasConversion<string>();
        builder.Property(o => o.Status).HasConversion<string>();
        builder.HasOne<GuildSheet>()
            .WithMany()
            .HasForeignKey(o => o.GuildSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

`ExpeditionConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class ExpeditionConfiguration : IEntityTypeConfiguration<Expedition>
{
    public void Configure(EntityTypeBuilder<Expedition> builder)
    {
        builder.HasIndex(e => e.GuildSheetId);
        builder.Property(e => e.Kind).HasConversion<string>();
        builder.HasOne<GuildSheet>()
            .WithMany()
            .HasForeignKey(e => e.GuildSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 4: Verify the build**

Run: `dotnet build`
Expected: PASS (the Task 1 `AppDbContext` compile error is now resolved).

- [ ] **Step 5: Generate the schema migration**

Run:
```bash
dotnet ef migrations add AddGuildSheetTables --project src/Ruptura.Infrastructure --startup-project src/Ruptura.API
```
Expected: a new `*_AddGuildSheetTables.cs` that **drops the `GuildMemberships` table**, **adds `CampaignId` + concurrency token to `GuildSheets`** (plus the unique index + FK), and **creates** `GuildBuildings`, `GuildStaff`, `ResearchProjects`, `CraftingOrders`, `Expeditions` with cascade FKs. Skim it to confirm; do not hand-edit.

- [ ] **Step 6: Write the schema integration test**

`tests/Ruptura.IntegrationTests/Guilds/GuildSchemaTests.cs` — follow the existing integration-test base pattern in `tests/Ruptura.IntegrationTests` (reuse whatever `WebApplicationFactory`/Testcontainers fixture the other tests use; e.g. the same base class `CharacterSheetControllerTests` derives from). Resolve `AppDbContext` from the factory's service scope:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildSchemaTests(/* same fixture as other integration tests */)
{
    [Fact]
    public async Task DeletingCampaign_CascadeDeletes_GuildSheetAndChildren()
    {
        // Arrange: create a Campaign, a GuildSheet for it, and one child of each kind.
        // Use the factory's AppDbContext scope. (Campaign requires a GameMasterId Guid.)
        using var scope = /* factory */.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var campaign = new Campaign { Id = Guid.NewGuid(), Name = "Cascade Test", GameMasterId = Guid.NewGuid() };
        db.Campaigns.Add(campaign);
        var guild = new GuildSheet { Id = Guid.NewGuid(), CampaignId = campaign.Id, GuildName = "G", CreatedByGameMasterId = campaign.GameMasterId };
        db.GuildSheets.Add(guild);
        db.GuildBuildings.Add(new GuildBuilding { Id = Guid.NewGuid(), GuildSheetId = guild.Id, CatalogEntryId = Guid.NewGuid(), Level = 1 });
        db.Expeditions.Add(new Expedition { Id = Guid.NewGuid(), GuildSheetId = guild.Id });
        await db.SaveChangesAsync();

        // Act: delete the campaign.
        db.Campaigns.Remove(campaign);
        await db.SaveChangesAsync();

        // Assert: guild and its children are gone.
        (await db.GuildSheets.CountAsync(g => g.Id == guild.Id)).Should().Be(0);
        (await db.GuildBuildings.CountAsync(b => b.GuildSheetId == guild.Id)).Should().Be(0);
        (await db.Expeditions.CountAsync(e => e.GuildSheetId == guild.Id)).Should().Be(0);
    }

    [Fact]
    public async Task SecondGuildForSameCampaign_ViolatesUniqueIndex()
    {
        using var scope = /* factory */.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var campaign = new Campaign { Id = Guid.NewGuid(), Name = "Unique Test", GameMasterId = Guid.NewGuid() };
        db.Campaigns.Add(campaign);
        db.GuildSheets.Add(new GuildSheet { Id = Guid.NewGuid(), CampaignId = campaign.Id, GuildName = "A", CreatedByGameMasterId = campaign.GameMasterId });
        await db.SaveChangesAsync();

        db.GuildSheets.Add(new GuildSheet { Id = Guid.NewGuid(), CampaignId = campaign.Id, GuildName = "B", CreatedByGameMasterId = campaign.GameMasterId });

        var act = async () => await db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
```

> Adjust the constructor/fixture to match the project's integration-test base (check `CharacterSheetControllerTests` or similar for the exact fixture type and how it exposes the factory/db). Confirm `Campaign`'s required properties (`Name`, `GameMasterId`) against `src/Ruptura.Domain/Entities/Campaign.cs` before finalizing.

- [ ] **Step 7: Run the schema tests**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildSchemaTests`
Expected: PASS (2 tests). Testcontainers applies migrations to a fresh Postgres, so the new schema is exercised end-to-end. If a single unrelated Serilog-race test fails, re-run.

- [ ] **Step 8: Commit**

```bash
git add src/Ruptura.Infrastructure tests/Ruptura.IntegrationTests/Guilds/GuildSchemaTests.cs
git commit -m "feat: add guild sheet EF config, child tables, and schema migration

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Seed the 20 installations and 8 doctrines

Deliverable: the official FECHADA reference data exists as `CatalogEntry` rows, proven by an integration test counting and shape-checking the seed.

**Files:**
- Create: `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Installations.cs`, `CatalogSeedData.Doctrines.cs`
- Modify: `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs`
- Create: `tests/Ruptura.IntegrationTests/Guilds/GuildSeedTests.cs`
- Migration: `*_SeedInstallationsAndDoctrines.cs` (generated)

**Interfaces:**
- Consumes: `CatalogEntryType.Installation`/`.Doctrine`, `CatalogSeedData.Entry(...)`.
- Produces: `CatalogSeedData.Installations` (20 rows), `CatalogSeedData.Doctrines` (8 rows).

- [ ] **Step 1: Create the installations seed**

`src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Installations.cs` — GUID prefix `d0000000-…`; `DataJson` carries `Category`, `Weight`, `LevelCap` (int), `Prerequisites`, `Unlocks`, and `NonConstructible` for Portão. Values from GDD §10.3.1 / Manual §8.2.1:

```csharp
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Installations =
    [
        // Fundação (Peso 1)
        Entry("d0000000-0000-0000-0000-000000000001", CatalogEntryType.Installation, "Portão", new { Category = "Fundação", Weight = 1, LevelCap = 1, Prerequisites = "Existe desde o início", Unlocks = "Núcleo da Dungeon; não se constrói nem melhora", NonConstructible = true }),
        Entry("d0000000-0000-0000-0000-000000000002", CatalogEntryType.Installation, "Dormitório", new { Category = "Fundação", Weight = 1, LevelCap = 5, Prerequisites = "Nenhum", Unlocks = "Vagas de personagens/trabalhadores (Nível × 2)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000003", CatalogEntryType.Installation, "Armazém", new { Category = "Fundação", Weight = 1, LevelCap = 5, Prerequisites = "Nenhum", Unlocks = "Armazenamento (Nível × 50 unidades)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000004", CatalogEntryType.Installation, "Campo de Treinamento", new { Category = "Fundação", Weight = 1, LevelCap = 5, Prerequisites = "Nenhum", Unlocks = "Treino de combate; Provações de Corpo/Controle", NonConstructible = false }),
        // Produção (Peso 2)
        Entry("d0000000-0000-0000-0000-000000000005", CatalogEntryType.Installation, "Ferraria", new { Category = "Produção", Weight = 2, LevelCap = 5, Prerequisites = "Armazém I", Unlocks = "Crafting de armas/armaduras (Comum→Raro em I-II, Épico em III+)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000006", CatalogEntryType.Installation, "Oficina", new { Category = "Produção", Weight = 2, LevelCap = 5, Prerequisites = "Armazém I", Unlocks = "Crafting geral (Comum/Incomum)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000007", CatalogEntryType.Installation, "Biblioteca", new { Category = "Produção", Weight = 2, LevelCap = 7, Prerequisites = "Dormitório I", Unlocks = "Pesquisa Menor/Moderada; Provações de Intelecto/Percepção", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000008", CatalogEntryType.Installation, "Enfermaria", new { Category = "Produção", Weight = 2, LevelCap = 5, Prerequisites = "Dormitório I", Unlocks = "Cura avançada, recuperação de PV no Interlúdio; Provação de Vigor", NonConstructible = false }),
        // Especialização (Peso 3)
        Entry("d0000000-0000-0000-0000-000000000009", CatalogEntryType.Installation, "Laboratório Arcano", new { Category = "Especialização", Weight = 3, LevelCap = 5, Prerequisites = "Biblioteca II", Unlocks = "Pesquisa Arcana Maior; Provação de Afinidade; Encantamento", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000010", CatalogEntryType.Installation, "Academia Militar", new { Category = "Especialização", Weight = 3, LevelCap = 5, Prerequisites = "Campo de Treinamento II + Enfermaria I", Unlocks = "Provações de Presença/Vontade; Técnicas Supremas; mercenários avançados", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000011", CatalogEntryType.Installation, "Jardim Alquímico", new { Category = "Especialização", Weight = 3, LevelCap = 4, Prerequisites = "Oficina II", Unlocks = "Alquimia avançada (Venenos/Transmutação)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000012", CatalogEntryType.Installation, "Oficina de Runas", new { Category = "Especialização", Weight = 3, LevelCap = 4, Prerequisites = "Ferraria II", Unlocks = "Crafting Épico+; Encantamento de armas", NonConstructible = false }),
        // Institucional (Peso 5)
        Entry("d0000000-0000-0000-0000-000000000013", CatalogEntryType.Installation, "Memorial", new { Category = "Institucional", Weight = 5, LevelCap = 4, Prerequisites = "Biblioteca III", Unlocks = "Cristais de Memória; aumenta Capacidade de Formação (CF)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000014", CatalogEntryType.Installation, "Centro Logístico", new { Category = "Institucional", Weight = 5, LevelCap = 4, Prerequisites = "Armazém III + Oficina II", Unlocks = "Aumenta Capacidade de Suporte (CS); mais Expedições Secundárias", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000015", CatalogEntryType.Installation, "Quartel dos Mercenários", new { Category = "Institucional", Weight = 5, LevelCap = 4, Prerequisites = "Academia Militar II", Unlocks = "Mercenários de Ranking mais alto; aumenta limite", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000016", CatalogEntryType.Installation, "Torre dos Magos", new { Category = "Institucional", Weight = 5, LevelCap = 4, Prerequisites = "Laboratório Arcano III", Unlocks = "Pesquisa Suprema; Rituais avançados; Grimórios raros", NonConstructible = false }),
        // Monumental (Peso 8)
        Entry("d0000000-0000-0000-0000-000000000017", CatalogEntryType.Installation, "Câmara do Conselho", new { Category = "Monumental", Weight = 8, LevelCap = 2, Prerequisites = "Centro Logístico III + Memorial II", Unlocks = "Aumenta Capacidade Institucional (CI); mais Patronos/projetos simultâneos", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000018", CatalogEntryType.Installation, "Cofre Divino", new { Category = "Monumental", Weight = 8, LevelCap = 2, Prerequisites = "Memorial III", Unlocks = "Guarda Moedas de Pacto com segurança; habilita Crafting Divino", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000019", CatalogEntryType.Installation, "Observatório Dimensional", new { Category = "Monumental", Weight = 8, LevelCap = 2, Prerequisites = "Torre dos Magos III", Unlocks = "Prevê Rupturas; reduz a Pressão base de andares explorados", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000020", CatalogEntryType.Installation, "Santuário do Patrono", new { Category = "Monumental", Weight = 8, LevelCap = 2, Prerequisites = "Câmara do Conselho I + Cofre Divino I", Unlocks = "Fortalece o Pacto Divino; resistência a eventos Divinos negativos", NonConstructible = false }),
    ];
}
```

- [ ] **Step 2: Create the doctrines seed**

`src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Doctrines.cs` — GUID prefix `d1000000-…`; `DataJson` carries `Bonus` (display text; the calculator keys mechanical effects to these ids in sub-plan #2). Values from GDD §10.7 / Manual §8.5:

```csharp
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Doctrines =
    [
        Entry("d1000000-0000-0000-0000-000000000001", CatalogEntryType.Doctrine, "Militar", new { Bonus = "+10% em Ataque/Dano de Mercenários e NPCs de combate da Guilda; -1 dia no tempo de Provações de Corpo/Controle/Presença/Vontade" }),
        Entry("d1000000-0000-0000-0000-000000000002", CatalogEntryType.Doctrine, "Acadêmica", new { Bonus = "+15% de velocidade em projetos de Pesquisa (reduz tempo); -10% de custo em Recursos para Provações de Intelecto/Percepção" }),
        Entry("d1000000-0000-0000-0000-000000000003", CatalogEntryType.Doctrine, "Comercial", new { Bonus = "+10% em toda venda de materiais excedentes; reduz o Índice de Preços de Inflação em 1 estágio para compras da própria Guilda" }),
        Entry("d1000000-0000-0000-0000-000000000004", CatalogEntryType.Doctrine, "Exploração", new { Bonus = "+15% de chance de sucesso em Expedições Secundárias; -10% no consumo de Comida/Água/Tochas do grupo principal" }),
        Entry("d1000000-0000-0000-0000-000000000005", CatalogEntryType.Doctrine, "Arcana", new { Bonus = "-1 PA adicional em conjuração para todos os personagens da Guilda; -25% no tempo de Provação de Afinidade" }),
        Entry("d1000000-0000-0000-0000-000000000006", CatalogEntryType.Doctrine, "Engenharia", new { Bonus = "-15% no Tempo de Construção/Melhoria de instalações; +10% de chance de Grande Sucesso em Crafting" }),
        Entry("d1000000-0000-0000-0000-000000000007", CatalogEntryType.Doctrine, "Logística", new { Bonus = "+20% na Capacidade de Suporte (CS); -10% na Manutenção Diária" }),
        Entry("d1000000-0000-0000-0000-000000000008", CatalogEntryType.Doctrine, "Diplomática", new { Bonus = "Facções recém-descobertas começam com +15 de Reputação; ganhos de Reputação de peso Moderado contam como Maior (perdas continuam normais)" }),
    ];
}
```

> **GUID collision check:** before generating the migration, confirm no existing seed uses the `d0000000`/`d1000000` prefixes: `grep -rn "d0000000-\|d1000000-" src/Ruptura.Infrastructure/Data/Seed`. Expect no matches outside the two new files.

- [ ] **Step 3: Register the seed**

In `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs`, add after the existing `HasData` calls:
```csharp
        builder.HasData(CatalogSeedData.Installations);
        builder.HasData(CatalogSeedData.Doctrines);
```

- [ ] **Step 4: Generate the seed migration**

Run:
```bash
dotnet ef migrations add SeedInstallationsAndDoctrines --project src/Ruptura.Infrastructure --startup-project src/Ruptura.API
```
Expected: a migration of 28 `InsertData` rows into `CatalogEntries` (20 installations + 8 doctrines), no schema changes. Skim to confirm only inserts.

- [ ] **Step 5: Write the seed integration test**

`tests/Ruptura.IntegrationTests/Guilds/GuildSeedTests.cs`:
```csharp
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Data;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildSeedTests(/* same fixture as other integration tests */)
{
    [Fact]
    public async Task Seed_Has20Installations_AllGlobalWithValidShape()
    {
        using var scope = /* factory */.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var installations = await db.CatalogEntries
            .Where(c => c.Type == CatalogEntryType.Installation && c.CampaignId == null)
            .ToListAsync();

        installations.Should().HaveCount(20);
        foreach (var i in installations)
        {
            using var doc = JsonDocument.Parse(i.DataJson);
            doc.RootElement.GetProperty("Category").GetString().Should().NotBeNullOrEmpty();
            doc.RootElement.GetProperty("Weight").GetInt32().Should().BeGreaterThan(0);
            doc.RootElement.GetProperty("LevelCap").GetInt32().Should().BeGreaterThan(0);
        }
        installations.Should().ContainSingle(i => i.Name == "Portão");
    }

    [Fact]
    public async Task Seed_Has8Doctrines_AllGlobalWithBonusText()
    {
        using var scope = /* factory */.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var doctrines = await db.CatalogEntries
            .Where(c => c.Type == CatalogEntryType.Doctrine && c.CampaignId == null)
            .ToListAsync();

        doctrines.Should().HaveCount(8);
        doctrines.Select(d => d.Name).Should().Contain(new[] { "Militar", "Logística", "Comercial" });
        foreach (var d in doctrines)
        {
            using var doc = JsonDocument.Parse(d.DataJson);
            doc.RootElement.GetProperty("Bonus").GetString().Should().NotBeNullOrEmpty();
        }
    }
}
```

- [ ] **Step 6: Run the seed tests**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildSeedTests`
Expected: PASS (2 tests).

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Infrastructure tests/Ruptura.IntegrationTests/Guilds/GuildSeedTests.cs
git commit -m "feat: seed 20 guild installations and 8 doctrines as catalog entries

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 4: `GuildSheet` repository + DI

Deliverable: a repository the later sub-plans (service/controller) consume to fetch a campaign's guild, proven by an integration test.

**Files:**
- Create: `src/Ruptura.Application/Interfaces/IGuildSheetRepository.cs`
- Create: `src/Ruptura.Infrastructure/Repositories/GuildSheetRepository.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`
- Create: `tests/Ruptura.IntegrationTests/Guilds/GuildSheetRepositoryTests.cs`

**Interfaces:**
- Consumes: `BaseRepository<GuildSheet>`, `IRepository<T>`, `AppDbContext`.
- Produces: `IGuildSheetRepository.GetByCampaignAsync(Guid campaignId, CancellationToken)` → `Task<GuildSheet?>`.

- [ ] **Step 1: Write the failing repository test**

`tests/Ruptura.IntegrationTests/Guilds/GuildSheetRepositoryTests.cs`:
```csharp
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildSheetRepositoryTests(/* same fixture as other integration tests */)
{
    [Fact]
    public async Task GetByCampaignAsync_ReturnsTheCampaignsGuild_OrNull()
    {
        using var scope = /* factory */.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IGuildSheetRepository>();

        var campaign = new Campaign { Id = Guid.NewGuid(), Name = "Repo Test", GameMasterId = Guid.NewGuid() };
        db.Campaigns.Add(campaign);
        db.GuildSheets.Add(new GuildSheet { Id = Guid.NewGuid(), CampaignId = campaign.Id, GuildName = "Repo Guild", CreatedByGameMasterId = campaign.GameMasterId });
        await db.SaveChangesAsync();

        (await repo.GetByCampaignAsync(campaign.Id)).Should().NotBeNull();
        (await repo.GetByCampaignAsync(Guid.NewGuid())).Should().BeNull();
    }
}
```

- [ ] **Step 2: Run it to verify it fails to compile**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildSheetRepositoryTests`
Expected: FAIL — `IGuildSheetRepository` does not exist yet.

- [ ] **Step 3: Create the repository interface**

`src/Ruptura.Application/Interfaces/IGuildSheetRepository.cs`:
```csharp
using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IGuildSheetRepository : IRepository<GuildSheet>
{
    Task<GuildSheet?> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
}
```

- [ ] **Step 4: Create the repository implementation**

`src/Ruptura.Infrastructure/Repositories/GuildSheetRepository.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class GuildSheetRepository(AppDbContext db)
    : BaseRepository<GuildSheet>(db), IGuildSheetRepository
{
    public async Task<GuildSheet?> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(g => g.CampaignId == campaignId, ct);
}
```

> Confirm `BaseRepository<T>` exposes a protected `Set` (used by `CampaignMembershipRepository`); if the member name differs, match the existing repositories.

- [ ] **Step 5: Register in DI**

In `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`, add alongside the other repository registrations:
```csharp
        services.AddScoped<IGuildSheetRepository, GuildSheetRepository>();
```

- [ ] **Step 6: Run the test to verify it passes**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildSheetRepositoryTests`
Expected: PASS.

- [ ] **Step 7: Full build + test sweep**

Run: `dotnet build && dotnet test`
Expected: PASS. Re-run once if a single Serilog-race integration test flakes.

- [ ] **Step 8: Commit**

```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure tests/Ruptura.IntegrationTests/Guilds/GuildSheetRepositoryTests.cs
git commit -m "feat: add GuildSheet repository and DI registration

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage (against §3, §10, §12.1 — the Foundation slice):**
- §3.1 `GuildSheet` mods (CampaignId FK+unique, RowVersion, drop Memberships) → Task 1 (entity) + Task 2 (config/migration). ✓
- §3.1 drop `GuildMembership` → Task 1 Step 4 + Task 2 migration. ✓
- §3.2 five child entities → Task 1 + Task 2 config/migration. ✓
- §3.3 `CatalogEntryType` + Installation/Doctrine seed → Task 1 Step 1, Task 3. ✓
- §3.4 `GuildSheetData` blob DTOs → Task 1 Step 6. ✓
- §10 data-model impact rows → covered across Tasks 1–3. ✓
- Repository access for later sub-plans → Task 4. ✓
- **Deferred by design (later sub-plans, not gaps):** `GuildStatsCalculator` (#2), record-keeping services/controllers (#3), QG/staff/doctrines write paths (#4), research/crafting (#5), interlude (#6), UI/i18n (#7). Child-entity repositories are added in the sub-plan that first consumes each (calculator #2 reads buildings/staff; #3 uses expeditions; #5 uses research/crafting).

**2. Placeholder scan:** No "TBD"/"handle appropriately". The only intentional `/* same fixture as other integration tests */` markers are because the concrete integration-test base class must be read from the repo at execution time (it is not visible in the spec); each is accompanied by an instruction to match the existing pattern. Every code step has real code.

**3. Type consistency:** `GuildSheet.CampaignId: Guid`, `.RowVersion: byte[]`, `.DataJson: string` used consistently in entity, config, and tests. `IGuildSheetRepository.GetByCampaignAsync` signature matches between interface, impl, and test. Enum `.HasConversion<string>()` applied in every child config that has an enum. `CatalogEntryType.Installation`/`.Doctrine` used identically in seed and tests. Namespaces: entities `Ruptura.Domain.Entities`, enums `Ruptura.Domain.Enums`, blob DTOs `Ruptura.Shared.Guilds`.

**Note carried to later sub-plans:** `ICampaignMembershipRepository.GetByPlayerAsync` already exists — the shared-write authorization in sub-plan #3+ can consume it directly (no additive work needed, contrary to the spec §6 note).
