# Catalog Subsystem Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the unified homebrew-extensible catalog (`CatalogEntry`) described in the character sheet spec (§4.2) — Origins, Backgrounds, Lineages, Aptitudes, Talents, Skills, Spells, Techniques, and Equipment Items all live in one table, seeded with the GDD's official closed lists, with per-Campaign homebrew CRUD restricted to the Campaign's GM.

**Architecture:** Same Clean Architecture layering and conventions as the Campaign & Roster Foundation plan (`docs/superpowers/plans/2026-08-04-campaign-roster-foundation.md`), which this plan builds directly on top of (`CatalogEntry.CampaignId` scopes homebrew to a `Campaign`; read access requires being a member — the GM or a `CampaignMembership` — of that Campaign).

**Tech Stack:** ASP.NET Core 8 Web API, EF Core 8 + Npgsql (with `HasData` model seeding), FluentValidation, Blazor WebAssembly, `System.Text.Json`, xUnit + Moq + FluentAssertions (unit), Testcontainers.PostgreSql + `WebApplicationFactory<Program>` (integration).

## Global Constraints

- Result pattern only — services return `Result`/`Result<T>`, never throw business exceptions across layer boundaries.
- Every user-facing string goes through `IStringLocalizer` (API: `SharedResources`, Web: `AppStrings`) with **both** `en` and `pt-BR` `.resx` entries.
- New EF migrations: `dotnet ef migrations add <Name> --project src/Ruptura.Infrastructure --startup-project src/Ruptura.API`.
- Seed content (Origin/Background/Lineage/Aptitude/Talent/Skill/Spell/Technique names and effect text) is transcribed **verbatim from `docs/GDD_Ruptura.md`, in Brazilian Portuguese** — this is game content, not UI chrome, and is never translated. Only UI labels/buttons/column headers get bilingual `en`/`pt-BR` treatment.
- `CatalogEntry.Type` is a plain `enum` stored as its default `int` in the database (matches the existing `ApplicationUser.Role`/`UserRole` convention — no `HasConversion<string>()`).
- Global (official) `CatalogEntry` rows have `CampaignId = null` and are seeded once via `HasData`; they are never created/edited/deleted through the API — only homebrew rows (`CampaignId` set) go through `CatalogController`'s write endpoints.
- A player or GM may **read** a Campaign's catalog (official + that Campaign's homebrew) if they are the Campaign's GM or hold a `CampaignMembership` for it. Only the GM may create/edit/delete homebrew entries, and only within their own Campaign.

---

### Task 1: Domain — `CatalogEntryType` enum and `CatalogEntry` entity

**Files:**
- Create: `src/Ruptura.Domain/Enums/CatalogEntryType.cs`
- Create: `src/Ruptura.Domain/Entities/CatalogEntry.cs`

**Interfaces:**
- Produces: `Ruptura.Domain.Enums.CatalogEntryType { Origin, Background, Lineage, Aptitude, Talent, Skill, Spell, Technique, EquipmentItem }`
- Produces: `Ruptura.Domain.Entities.CatalogEntry { Id, Type, CampaignId, Name, DataJson, CreatedByGameMasterId, CreatedAt, UpdatedAt }`

- [ ] **Step 1: Create the `CatalogEntryType` enum**

```csharp
// src/Ruptura.Domain/Enums/CatalogEntryType.cs
namespace Ruptura.Domain.Enums;

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
    EquipmentItem
}
```

- [ ] **Step 2: Create the `CatalogEntry` entity**

```csharp
// src/Ruptura.Domain/Entities/CatalogEntry.cs
using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class CatalogEntry
{
    public Guid Id { get; set; }
    public CatalogEntryType Type { get; set; }
    public Guid? CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public Guid? CreatedByGameMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Step 3: Build to verify it compiles**

Run: `dotnet build src/Ruptura.Domain/Ruptura.Domain.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Domain/Enums/CatalogEntryType.cs src/Ruptura.Domain/Entities/CatalogEntry.cs
git commit -m "feat: add CatalogEntryType enum and CatalogEntry domain entity"
```

---

### Task 2: EF Core wiring — `DbSet`, partial unique indexes, migration

**Files:**
- Modify: `src/Ruptura.Infrastructure/Data/AppDbContext.cs`
- Create: `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs`
- Create (generated): `src/Ruptura.Infrastructure/Data/Migrations/<timestamp>_AddCatalogEntries.cs`

**Interfaces:**
- Consumes: `CatalogEntry`, `CatalogEntryType` (Task 1)
- Produces: `AppDbContext.CatalogEntries (DbSet<CatalogEntry>)`; two partial unique indexes enforcing name-uniqueness within each scope

**Context:** A `CatalogEntry` is either **global** (`CampaignId == null`, the official GDD seed) or **homebrew** (`CampaignId` set, one Campaign's custom content). Names must be unique *within* each scope — two different Campaigns can each have their own homebrew "Ferreiro" origin, and that must never collide with each other or with an official entry. Postgres treats `NULL` as distinct from every other `NULL` in a plain unique index, so a single `(Type, CampaignId, Name)` unique index would **not** stop two global (`CampaignId = null`) rows from having the same name — hence two separate partial indexes, one per scope.

- [ ] **Step 1: Register the `DbSet`**

Edit `src/Ruptura.Infrastructure/Data/AppDbContext.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Identity;

namespace Ruptura.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<InviteCode> InviteCodes => Set<InviteCode>();
    public DbSet<CharacterSheet> CharacterSheets => Set<CharacterSheet>();
    public DbSet<GuildSheet> GuildSheets => Set<GuildSheet>();
    public DbSet<GuildMembership> GuildMemberships => Set<GuildMembership>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignMembership> CampaignMemberships => Set<CampaignMembership>();
    public DbSet<CatalogEntry> CatalogEntries => Set<CatalogEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

- [ ] **Step 2: Add the `CatalogEntryConfiguration` with the two partial unique indexes**

```csharp
// src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CatalogEntryConfiguration : IEntityTypeConfiguration<CatalogEntry>
{
    public void Configure(EntityTypeBuilder<CatalogEntry> builder)
    {
        // Global (official) entries: unique by (Type, Name) among CampaignId IS NULL rows.
        builder.HasIndex(c => new { c.Type, c.Name })
            .IsUnique()
            .HasFilter("\"CampaignId\" IS NULL")
            .HasDatabaseName("ux_catalog_entries_global_type_name");

        // Homebrew entries: unique by (Type, CampaignId, Name) among CampaignId IS NOT NULL rows.
        builder.HasIndex(c => new { c.Type, c.CampaignId, c.Name })
            .IsUnique()
            .HasFilter("\"CampaignId\" IS NOT NULL")
            .HasDatabaseName("ux_catalog_entries_scoped_type_campaign_name");
    }
}
```

- [ ] **Step 3: Generate the migration**

Run:
```bash
dotnet ef migrations add AddCatalogEntries \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```
Expected: a new migration creating the `CatalogEntries` table (`Id`, `Type` int, `CampaignId` uuid nullable, `Name`, `DataJson`, `CreatedByGameMasterId` uuid nullable, `CreatedAt`, `UpdatedAt`) and the two partial unique indexes described above. Open the generated migration file and confirm both `CreateIndex` calls carry a `filter:` argument matching the `HasFilter` strings above.

- [ ] **Step 4: Verify the solution builds**

Run: `dotnet build`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Infrastructure/Data/AppDbContext.cs \
        src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs \
        src/Ruptura.Infrastructure/Data/Migrations/
git commit -m "feat: add CatalogEntries table with scoped uniqueness constraints"
```

---

### Task 3: Repository

**Files:**
- Create: `src/Ruptura.Application/Interfaces/ICatalogEntryRepository.cs`
- Create: `src/Ruptura.Infrastructure/Repositories/CatalogEntryRepository.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`

**Interfaces:**
- Consumes: `CatalogEntry`, `CatalogEntryType` (Task 1); `AppDbContext` (Task 2)
- Produces:
  ```csharp
  public interface ICatalogEntryRepository : IRepository<CatalogEntry>
  {
      Task<IEnumerable<CatalogEntry>> GetByTypeAsync(CatalogEntryType type, Guid campaignId, CancellationToken ct = default);
      Task<bool> ExistsAsync(CatalogEntryType type, Guid? campaignId, string name, CancellationToken ct = default);
  }
  ```

- [ ] **Step 1: Define `ICatalogEntryRepository`**

```csharp
// src/Ruptura.Application/Interfaces/ICatalogEntryRepository.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICatalogEntryRepository : IRepository<CatalogEntry>
{
    Task<IEnumerable<CatalogEntry>> GetByTypeAsync(CatalogEntryType type, Guid campaignId, CancellationToken ct = default);
    Task<bool> ExistsAsync(CatalogEntryType type, Guid? campaignId, string name, CancellationToken ct = default);
}
```

- [ ] **Step 2: Implement `CatalogEntryRepository`**

`GetByTypeAsync` returns both the global (official) entries for that type AND the homebrew entries scoped to the given Campaign, combined — this is exactly "official + homebrew da Campaign" from the spec.

```csharp
// src/Ruptura.Infrastructure/Repositories/CatalogEntryRepository.cs
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CatalogEntryRepository(AppDbContext db)
    : BaseRepository<CatalogEntry>(db), ICatalogEntryRepository
{
    public async Task<IEnumerable<CatalogEntry>> GetByTypeAsync(
        CatalogEntryType type,
        Guid campaignId,
        CancellationToken ct = default) =>
        await Set
            .Where(c => c.Type == type && (c.CampaignId == null || c.CampaignId == campaignId))
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        CatalogEntryType type,
        Guid? campaignId,
        string name,
        CancellationToken ct = default) =>
        await Set.AnyAsync(c => c.Type == type && c.CampaignId == campaignId && c.Name == name, ct);
}
```

- [ ] **Step 3: Register the repository in DI**

Edit `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` — add under `// Repositories`:

```csharp
        // Repositories
        services.AddScoped<IInviteCodeRepository, InviteCodeRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ICampaignMembershipRepository, CampaignMembershipRepository>();
        services.AddScoped<ICatalogEntryRepository, CatalogEntryRepository>();
```

- [ ] **Step 4: Build to verify it compiles**

Run: `dotnet build`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Application/Interfaces/ICatalogEntryRepository.cs \
        src/Ruptura.Infrastructure/Repositories/CatalogEntryRepository.cs \
        src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs
git commit -m "feat: add CatalogEntry repository"
```

---

### Task 4: Shared DTOs, error codes, validators

**Files:**
- Create: `src/Ruptura.Shared/Catalog/CatalogEntryResponse.cs`
- Create: `src/Ruptura.Shared/Catalog/CreateCatalogEntryRequest.cs`
- Create: `src/Ruptura.Shared/Catalog/UpdateCatalogEntryRequest.cs`
- Modify: `src/Ruptura.Application/Common/ErrorCodes.cs`
- Create: `src/Ruptura.Application/Validators/Catalog/CreateCatalogEntryRequestValidator.cs`
- Create: `src/Ruptura.Application/Validators/Catalog/UpdateCatalogEntryRequestValidator.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`

**Interfaces:**
- Produces:
  - `Ruptura.Shared.Catalog.CatalogEntryResponse { Id, Type (string), CampaignId (Guid?), IsGlobal (bool), Name, DataJson, CreatedByGameMasterId (Guid?), CreatedAt }`
  - `Ruptura.Shared.Catalog.CreateCatalogEntryRequest { CampaignId, Type (string), Name, DataJson }`
  - `Ruptura.Shared.Catalog.UpdateCatalogEntryRequest { Name, DataJson }`
  - `ErrorCodes.Catalog.{NotFound, InvalidType, AlreadyExists, CannotModifyGlobalEntry}`

`Type` travels over the wire as a `string` (e.g. `"Talent"`), not the raw enum — this matches the existing convention where `ApplicationUser.Role` (a `UserRole` enum) is always exposed as `user.Role.ToString()` in `AuthResponse.UserInfo.Role`, never as a numeric enum value.

- [ ] **Step 1: Create the DTOs**

```csharp
// src/Ruptura.Shared/Catalog/CatalogEntryResponse.cs
namespace Ruptura.Shared.Catalog;

public class CatalogEntryResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? CampaignId { get; set; }
    public bool IsGlobal { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public Guid? CreatedByGameMasterId { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

```csharp
// src/Ruptura.Shared/Catalog/CreateCatalogEntryRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Catalog;

public class CreateCatalogEntryRequest
{
    [Required]
    public Guid CampaignId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string DataJson { get; set; } = "{}";
}
```

```csharp
// src/Ruptura.Shared/Catalog/UpdateCatalogEntryRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Catalog;

public class UpdateCatalogEntryRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string DataJson { get; set; } = "{}";
}
```

- [ ] **Step 2: Add `ErrorCodes.Catalog`**

Edit `src/Ruptura.Application/Common/ErrorCodes.cs` — add a new nested class after `Campaign`:

```csharp
    public static class Catalog
    {
        public const string NotFound = "Catalog.NotFound";
        public const string InvalidType = "Catalog.InvalidType";
        public const string AlreadyExists = "Catalog.AlreadyExists";
        public const string CannotModifyGlobalEntry = "Catalog.CannotModifyGlobalEntry";
    }
```

(This goes inside the existing `public static class ErrorCodes { ... }` body, as a sibling to the `Campaign` nested class — do not duplicate the outer `ErrorCodes` declaration.)

- [ ] **Step 3: Add FluentValidation validators**

```csharp
// src/Ruptura.Application/Validators/Catalog/CreateCatalogEntryRequestValidator.cs
using System.Text.Json;
using FluentValidation;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Catalog;

namespace Ruptura.Application.Validators.Catalog;

public class CreateCatalogEntryRequestValidator : AbstractValidator<CreateCatalogEntryRequest>
{
    public CreateCatalogEntryRequestValidator()
    {
        RuleFor(x => x.CampaignId).NotEmpty();

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => Enum.TryParse<CatalogEntryType>(t, out _))
            .WithMessage("Invalid catalog entry type.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.DataJson)
            .NotEmpty()
            .Must(BeValidJson)
            .WithMessage("DataJson must be valid JSON.");
    }

    private static bool BeValidJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
```

```csharp
// src/Ruptura.Application/Validators/Catalog/UpdateCatalogEntryRequestValidator.cs
using System.Text.Json;
using FluentValidation;
using Ruptura.Shared.Catalog;

namespace Ruptura.Application.Validators.Catalog;

public class UpdateCatalogEntryRequestValidator : AbstractValidator<UpdateCatalogEntryRequest>
{
    public UpdateCatalogEntryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(150);

        RuleFor(x => x.DataJson)
            .NotEmpty()
            .Must(BeValidJson)
            .WithMessage("DataJson must be valid JSON.");
    }

    private static bool BeValidJson(string json)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
```

- [ ] **Step 4: Register the validators in DI**

Edit `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` — add the two new `using` statements (`using Ruptura.Application.Validators.Catalog;` and `using Ruptura.Shared.Catalog;`) alongside the existing ones, and add under `// Validators`:

```csharp
        services.AddScoped<IValidator<CreateCatalogEntryRequest>, CreateCatalogEntryRequestValidator>();
        services.AddScoped<IValidator<UpdateCatalogEntryRequest>, UpdateCatalogEntryRequestValidator>();
```

- [ ] **Step 5: Build to verify it compiles**

Run: `dotnet build`
Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Shared/Catalog/ \
        src/Ruptura.Application/Common/ErrorCodes.cs \
        src/Ruptura.Application/Validators/Catalog/ \
        src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs
git commit -m "feat: add CatalogEntry DTOs, error codes, and validators"
```

---

### Task 5: `CatalogEntryService` with unit tests

**Files:**
- Create: `src/Ruptura.Application/Interfaces/ICatalogEntryService.cs`
- Create: `src/Ruptura.Infrastructure/Services/CatalogEntryService.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`
- Create: `tests/Ruptura.UnitTests/Application/CatalogEntryServiceTests.cs`

**Interfaces:**
- Consumes: `ICatalogEntryRepository` (Task 3); `ICampaignRepository`, `ICampaignMembershipRepository` (from the Campaign & Roster Foundation plan — already exist); DTOs + `ErrorCodes.Catalog` (Task 4)
- Produces:
  ```csharp
  public interface ICatalogEntryService
  {
      Task<Result<IEnumerable<CatalogEntryResponse>>> GetByTypeAsync(Guid callerId, string type, Guid campaignId, CancellationToken ct = default);
      Task<Result<CatalogEntryResponse>> CreateAsync(Guid gameMasterId, CreateCatalogEntryRequest request, CancellationToken ct = default);
      Task<Result<CatalogEntryResponse>> UpdateAsync(Guid gameMasterId, Guid entryId, UpdateCatalogEntryRequest request, CancellationToken ct = default);
      Task<Result> DeleteAsync(Guid gameMasterId, Guid entryId, CancellationToken ct = default);
  }
  ```

**Permission rules this task implements** (from spec §6):
- `GetByTypeAsync`: caller must be the Campaign's GM or hold a `CampaignMembership` for it — otherwise `NotFound` (never leak whether the Campaign exists to a non-member).
- `CreateAsync`: caller must own (`GameMasterId`) the target Campaign — otherwise `NotFound`. Duplicate `(Type, CampaignId, Name)` → `AlreadyExists`.
- `UpdateAsync`/`DeleteAsync`: the entry must be homebrew (`CampaignId != null`) — global entries are immutable seed data, attempting to touch one → `CannotModifyGlobalEntry`. The caller must own the Campaign that entry belongs to — otherwise `NotFound`.

- [ ] **Step 1: Define `ICatalogEntryService`**

```csharp
// src/Ruptura.Application/Interfaces/ICatalogEntryService.cs
using Ruptura.Application.Common;
using Ruptura.Shared.Catalog;

namespace Ruptura.Application.Interfaces;

public interface ICatalogEntryService
{
    Task<Result<IEnumerable<CatalogEntryResponse>>> GetByTypeAsync(
        Guid callerId, string type, Guid campaignId, CancellationToken ct = default);

    Task<Result<CatalogEntryResponse>> CreateAsync(
        Guid gameMasterId, CreateCatalogEntryRequest request, CancellationToken ct = default);

    Task<Result<CatalogEntryResponse>> UpdateAsync(
        Guid gameMasterId, Guid entryId, UpdateCatalogEntryRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(Guid gameMasterId, Guid entryId, CancellationToken ct = default);
}
```

- [ ] **Step 2: Write the failing unit tests**

```csharp
// tests/Ruptura.UnitTests/Application/CatalogEntryServiceTests.cs
using FluentAssertions;
using Moq;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Services;
using Ruptura.Shared.Catalog;

namespace Ruptura.UnitTests.Application;

public class CatalogEntryServiceTests
{
    private readonly Mock<ICatalogEntryRepository> _catalogRepoMock = new();
    private readonly Mock<ICampaignRepository> _campaignRepoMock = new();
    private readonly Mock<ICampaignMembershipRepository> _membershipRepoMock = new();
    private readonly CatalogEntryService _sut;

    public CatalogEntryServiceTests()
    {
        _sut = new CatalogEntryService(
            _catalogRepoMock.Object, _campaignRepoMock.Object, _membershipRepoMock.Object);
    }

    // ── GetByTypeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByTypeAsync_WithInvalidType_ReturnsFailure()
    {
        var result = await _sut.GetByTypeAsync(Guid.NewGuid(), "NotARealType", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.InvalidType);
    }

    [Fact]
    public async Task GetByTypeAsync_WhenCallerIsGameMaster_ReturnsEntries()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };
        var entries = new List<CatalogEntry>
        {
            new() { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, Name = "Golpe Certeiro" }
        };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.GetByTypeAsync(CatalogEntryType.Talent, campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        var result = await _sut.GetByTypeAsync(gmId, "Talent", campaignId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(e => e.Name == "Golpe Certeiro");
        _membershipRepoMock.Verify(
            r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByTypeAsync_WhenCallerIsMember_ReturnsEntries()
    {
        var playerId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = Guid.NewGuid() };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaignId, playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _catalogRepoMock.Setup(r => r.GetByTypeAsync(CatalogEntryType.Skill, campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.GetByTypeAsync(playerId, "Skill", campaignId);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetByTypeAsync_WhenCallerNotMember_ReturnsNotFound()
    {
        var strangerId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = Guid.NewGuid() };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaignId, strangerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.GetByTypeAsync(strangerId, "Skill", campaignId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.NotFound);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesHomebrewEntry()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };

        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, campaignId, "Fôlego de Aço", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _catalogRepoMock.Setup(r => r.AddAsync(It.IsAny<CatalogEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _catalogRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.CreateAsync(gmId, new CreateCatalogEntryRequest
        {
            CampaignId = campaignId,
            Type = "Talent",
            Name = "Fôlego de Aço",
            DataJson = "{\"Category\":\"Combate\",\"Effect\":\"teste\",\"PowerTier\":\"menor\"}"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsGlobal.Should().BeFalse();
        result.Value.CampaignId.Should().Be(campaignId);
        _catalogRepoMock.Verify(r => r.AddAsync(
            It.Is<CatalogEntry>(e => e.Name == "Fôlego de Aço" && e.CreatedByGameMasterId == gmId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenCampaignNotOwnedByCaller_ReturnsNotFound()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = Guid.NewGuid() };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var result = await _sut.CreateAsync(Guid.NewGuid(), new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "X", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.NotFound);
    }

    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExistsInScope_ReturnsAlreadyExists()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, campaignId, "Duplicado", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.CreateAsync(gmId, new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "Duplicado", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.AlreadyExists);
    }

    // ── UpdateAsync / DeleteAsync ────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_OnGlobalEntry_ReturnsCannotModifyGlobalEntry()
    {
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Origin, CampaignId = null, Name = "Soldado" };
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), entry.Id, new UpdateCatalogEntryRequest
        {
            Name = "Soldado Editado", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.CannotModifyGlobalEntry);
    }

    [Fact]
    public async Task UpdateAsync_WhenCallerDoesNotOwnCampaign_ReturnsNotFound()
    {
        var campaignId = Guid.NewGuid();
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, CampaignId = campaignId, Name = "X" };
        var campaign = new Campaign { Id = campaignId, GameMasterId = Guid.NewGuid() };

        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), entry.Id, new UpdateCatalogEntryRequest
        {
            Name = "Y", DataJson = "{}"
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesEntry()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, CampaignId = campaignId, Name = "Old" };
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };

        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.ExistsAsync(CatalogEntryType.Talent, campaignId, "New", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _catalogRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(gmId, entry.Id, new UpdateCatalogEntryRequest
        {
            Name = "New", DataJson = "{\"a\":1}"
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("New");
        entry.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteAsync_OnGlobalEntry_ReturnsCannotModifyGlobalEntry()
    {
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Skill, CampaignId = null, Name = "Espadas" };
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), entry.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.CannotModifyGlobalEntry);
    }

    [Fact]
    public async Task DeleteAsync_WithValidData_RemovesEntry()
    {
        var gmId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var entry = new CatalogEntry { Id = Guid.NewGuid(), Type = CatalogEntryType.Talent, CampaignId = campaignId, Name = "X" };
        var campaign = new Campaign { Id = campaignId, GameMasterId = gmId };

        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaignId, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(gmId, entry.Id);

        result.IsSuccess.Should().BeTrue();
        _catalogRepoMock.Verify(r => r.Remove(entry), Times.Once);
    }
}
```

- [ ] **Step 3: Run tests to verify they fail (service doesn't exist yet)**

Run: `dotnet test tests/Ruptura.UnitTests --filter CatalogEntryServiceTests`
Expected: build error — `CatalogEntryService` does not exist.

- [ ] **Step 4: Implement `CatalogEntryService`**

```csharp
// src/Ruptura.Infrastructure/Services/CatalogEntryService.cs
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Catalog;

namespace Ruptura.Infrastructure.Services;

public class CatalogEntryService(
    ICatalogEntryRepository catalogRepo,
    ICampaignRepository campaignRepo,
    ICampaignMembershipRepository membershipRepo) : ICatalogEntryService
{
    public async Task<Result<IEnumerable<CatalogEntryResponse>>> GetByTypeAsync(
        Guid callerId,
        string type,
        Guid campaignId,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<CatalogEntryType>(type, out var parsedType))
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.InvalidType);

        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.NotFound);

        var isMember = campaign.GameMasterId == callerId
            || await membershipRepo.ExistsAsync(campaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.NotFound);

        var entries = await catalogRepo.GetByTypeAsync(parsedType, campaignId, ct);
        return Result.Success(entries.Select(MapToResponse));
    }

    public async Task<Result<CatalogEntryResponse>> CreateAsync(
        Guid gameMasterId,
        CreateCatalogEntryRequest request,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<CatalogEntryType>(request.Type, out var parsedType))
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.InvalidType);

        var campaign = await campaignRepo.GetByIdAsync(request.CampaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.NotFound);

        if (await catalogRepo.ExistsAsync(parsedType, request.CampaignId, request.Name, ct))
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.AlreadyExists);

        var entry = new CatalogEntry
        {
            Id = Guid.NewGuid(),
            Type = parsedType,
            CampaignId = request.CampaignId,
            Name = request.Name,
            DataJson = request.DataJson,
            CreatedByGameMasterId = gameMasterId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await catalogRepo.AddAsync(entry, ct);
        await catalogRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(entry));
    }

    public async Task<Result<CatalogEntryResponse>> UpdateAsync(
        Guid gameMasterId,
        Guid entryId,
        UpdateCatalogEntryRequest request,
        CancellationToken ct = default)
    {
        var entry = await catalogRepo.GetByIdAsync(entryId, ct);
        if (entry is null)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.NotFound);

        if (entry.CampaignId is null)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.CannotModifyGlobalEntry);

        var campaign = await campaignRepo.GetByIdAsync(entry.CampaignId.Value, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.NotFound);

        if (!string.Equals(entry.Name, request.Name, StringComparison.Ordinal)
            && await catalogRepo.ExistsAsync(entry.Type, entry.CampaignId, request.Name, ct))
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.AlreadyExists);

        entry.Name = request.Name;
        entry.DataJson = request.DataJson;
        entry.UpdatedAt = DateTime.UtcNow;
        catalogRepo.Update(entry);
        await catalogRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(entry));
    }

    public async Task<Result> DeleteAsync(
        Guid gameMasterId,
        Guid entryId,
        CancellationToken ct = default)
    {
        var entry = await catalogRepo.GetByIdAsync(entryId, ct);
        if (entry is null)
            return Result.Failure(ErrorCodes.Catalog.NotFound);

        if (entry.CampaignId is null)
            return Result.Failure(ErrorCodes.Catalog.CannotModifyGlobalEntry);

        var campaign = await campaignRepo.GetByIdAsync(entry.CampaignId.Value, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure(ErrorCodes.Catalog.NotFound);

        catalogRepo.Remove(entry);
        await catalogRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static CatalogEntryResponse MapToResponse(CatalogEntry c) => new()
    {
        Id = c.Id,
        Type = c.Type.ToString(),
        CampaignId = c.CampaignId,
        IsGlobal = c.CampaignId is null,
        Name = c.Name,
        DataJson = c.DataJson,
        CreatedByGameMasterId = c.CreatedByGameMasterId,
        CreatedAt = c.CreatedAt
    };
}
```

- [ ] **Step 5: Register `CatalogEntryService` in DI**

Edit `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` — add under `// Application services`:

```csharp
        services.AddScoped<ICatalogEntryService, CatalogEntryService>();
```

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter CatalogEntryServiceTests`
Expected: all 12 tests `Passed`.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Application/Interfaces/ICatalogEntryService.cs \
        src/Ruptura.Infrastructure/Services/CatalogEntryService.cs \
        src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs \
        tests/Ruptura.UnitTests/Application/CatalogEntryServiceTests.cs
git commit -m "feat: add CatalogEntryService with scoped read/write permissions"
```

---

### Task 6: Seed data — Origins (20) and Backgrounds (20)

**Files:**
- Create: `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.cs`
- Create: `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Origins.cs`
- Create: `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Backgrounds.cs`
- Modify: `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs`
- Create (generated): `src/Ruptura.Infrastructure/Data/Migrations/<timestamp>_SeedOriginsAndBackgrounds.cs`

**Interfaces:**
- Consumes: `CatalogEntry`, `CatalogEntryType` (Task 1)
- Produces: `CatalogSeedData.Origins` and `CatalogSeedData.Backgrounds` (`IReadOnlyList<CatalogEntry>`); `CatalogSeedData.Entry(...)` helper used by every subsequent seed task

**Context:** Content transcribed verbatim from `docs/GDD_Ruptura.md` §6.1.2 (Origens) and §6.1.4 (Históricos). IDs are fixed, human-readable GUIDs (not `Guid.NewGuid()`, which EF Core's `HasData` cannot use — seed data must be static/deterministic so the migration diff is stable across regenerations) — Origins use the `10000000-0000-0000-0000-0000000000XX` block, Backgrounds use `20000000-...`. Every field name matches spec §4.2.1's "Origin/Background... campos narrativos/mecânicos leves conforme GDD" — this task defines the concrete field names (`MainBenefit`/`PrimarySkill`/`SecondarySkill`/`StartingEquipment`/`NarrativeHook` for Origin; `TriggeringEvent`/`Benefit`/`Complication` for Background) since the spec left them open.

- [ ] **Step 1: Create the shared seed helper**

```csharp
// src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.cs
using System.Text.Json;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    // Fixed timestamp so HasData produces a stable migration diff — using
    // DateTime.UtcNow here would make every migration regeneration look
    // like every seed row changed.
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static CatalogEntry Entry(string id, CatalogEntryType type, string name, object data) => new()
    {
        Id = Guid.Parse(id),
        Type = type,
        CampaignId = null,
        Name = name,
        DataJson = JsonSerializer.Serialize(data),
        CreatedByGameMasterId = null,
        CreatedAt = SeedTimestamp,
        UpdatedAt = SeedTimestamp
    };
}
```

- [ ] **Step 2: Create the Origins seed data (GDD §6.1.2)**

```csharp
// src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Origins.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Origins =
    [
        Entry("10000000-0000-0000-0000-000000000001", CatalogEntryType.Origin, "Soldado", new { MainBenefit = "-1 dificuldade em testes de Disciplina/formação em combate organizado", PrimarySkill = "Espadas", SecondarySkill = "Armaduras", StartingEquipment = "Espada curta, armadura leve", NarrativeHook = "Desertou ou foi dispensado de uma força militar local" }),
        Entry("10000000-0000-0000-0000-000000000002", CatalogEntryType.Origin, "Caçador", new { MainBenefit = "-1 dificuldade em Rastreamento na natureza", PrimarySkill = "Rastreamento", SecondarySkill = "Arcos", StartingEquipment = "Arco simples, capa", NarrativeHook = "Vive das terras selvagens há anos" }),
        Entry("10000000-0000-0000-0000-000000000003", CatalogEntryType.Origin, "Artesão", new { MainBenefit = "Pode identificar qualidade de materiais sem teste", PrimarySkill = "Ferraria", SecondarySkill = "Avaliação", StartingEquipment = "Ferramentas de artesão", NarrativeHook = "Aprendeu um ofício com um mestre exigente" }),
        Entry("10000000-0000-0000-0000-000000000004", CatalogEntryType.Origin, "Camponês", new { MainBenefit = "+1 recuperação extra em descanso longo", PrimarySkill = "Sobrevivência", SecondarySkill = "Conhecimento de Animais", StartingEquipment = "Foice, roupas simples", NarrativeHook = "Cresceu trabalhando a terra" }),
        Entry("10000000-0000-0000-0000-000000000005", CatalogEntryType.Origin, "Estudioso", new { MainBenefit = "1x por interlúdio, resolve uma dúvida factual sem gastar tempo de pesquisa", PrimarySkill = "História (ou Teoria Arcana)", SecondarySkill = "Linguagens", StartingEquipment = "Livro pessoal", NarrativeHook = "Passou a juventude entre pergaminhos" }),
        Entry("10000000-0000-0000-0000-000000000006", CatalogEntryType.Origin, "Comerciante", new { MainBenefit = "Preços com o comerciante viajante 10% melhores", PrimarySkill = "Comércio", SecondarySkill = "Avaliação", StartingEquipment = "Bolsa de moedas extra", NarrativeHook = "Cresceu entre balcões e negociações" }),
        Entry("10000000-0000-0000-0000-000000000007", CatalogEntryType.Origin, "Nobre Decaído", new { MainBenefit = "Possui 1 contato de influência acionável (uso limitado)", PrimarySkill = "Liderança", SecondarySkill = "Diplomacia", StartingEquipment = "Anel de família (sem valor comercial)", NarrativeHook = "Perdeu título ou herança" }),
        Entry("10000000-0000-0000-0000-000000000008", CatalogEntryType.Origin, "Criminoso", new { MainBenefit = "-1 dificuldade em Furtividade em ambiente urbano", PrimarySkill = "Furtividade", SecondarySkill = "Manipulação", StartingEquipment = "Ferramentas de arrombamento", NarrativeHook = "Tem um passado que a Guilda desconhece" }),
        Entry("10000000-0000-0000-0000-000000000009", CatalogEntryType.Origin, "Sacerdote", new { MainBenefit = "1x por expedição, realiza uma pequena bênção ritual (efeito menor)", PrimarySkill = "Religião", SecondarySkill = "Rituais", StartingEquipment = "Símbolo sagrado", NarrativeHook = "Serviu um templo antes de ingressar na Guilda" }),
        Entry("10000000-0000-0000-0000-000000000010", CatalogEntryType.Origin, "Marinheiro", new { MainBenefit = "-1 dificuldade em Equilíbrio/terreno instável", PrimarySkill = "Natação", SecondarySkill = "Armas de Arremesso", StartingEquipment = "Corda, faca", NarrativeHook = "Passou anos em embarcações" }),
        Entry("10000000-0000-0000-0000-000000000011", CatalogEntryType.Origin, "Nômade", new { MainBenefit = "Nunca fica \"perdido\" narrativamente (sempre sabe a direção geral)", PrimarySkill = "Navegação", SecondarySkill = "Sobrevivência", StartingEquipment = "Cantil resistente", NarrativeHook = "Nunca teve um lar fixo" }),
        Entry("10000000-0000-0000-0000-000000000012", CatalogEntryType.Origin, "Mineiro", new { MainBenefit = "-1 dificuldade em identificar instabilidades em cavernas e túneis", PrimarySkill = "Construção", SecondarySkill = "Percepção", StartingEquipment = "Picareta", NarrativeHook = "Trabalhou em minas antes de se tornar aventureiro" }),
        Entry("10000000-0000-0000-0000-000000000013", CatalogEntryType.Origin, "Curandeiro", new { MainBenefit = "1x por expedição, estabiliza um ferido grave sem instalação", PrimarySkill = "Medicina", SecondarySkill = "Poções", StartingEquipment = "Kit médico básico", NarrativeHook = "Cuidou de doentes numa vila ou tropa" }),
        Entry("10000000-0000-0000-0000-000000000014", CatalogEntryType.Origin, "Menestrel", new { MainBenefit = "-1 dificuldade em testes sociais para obter informação de estranhos", PrimarySkill = "Diplomacia", SecondarySkill = "Manipulação", StartingEquipment = "Instrumento simples", NarrativeHook = "Viajou de vila em vila contando histórias" }),
        Entry("10000000-0000-0000-0000-000000000015", CatalogEntryType.Origin, "Órfão de Rua", new { MainBenefit = "-1 dificuldade em Percepção para notar armadilhas/emboscadas em ambientes fechados", PrimarySkill = "Percepção", SecondarySkill = "Furtividade", StartingEquipment = "Faca pequena escondida", NarrativeHook = "Sobreviveu sozinho nas ruas" }),
        Entry("10000000-0000-0000-0000-000000000016", CatalogEntryType.Origin, "Exilado", new { MainBenefit = "Conhece 1 idioma/símbolo raro exclusivo do grupo", PrimarySkill = "Linguagens", SecondarySkill = "Rastreamento", StartingEquipment = "Nenhum (perdeu tudo)", NarrativeHook = "Foi expulso de sua terra natal por um motivo que só ele sabe" }),
        Entry("10000000-0000-0000-0000-000000000017", CatalogEntryType.Origin, "Ex-Cultista", new { MainBenefit = "Reconhece automaticamente símbolos/rituais de cultos, sem teste", PrimarySkill = "Rituais", SecondarySkill = "Religião", StartingEquipment = "Adaga cerimonial", NarrativeHook = "Abandonou um culto antes que fosse tarde demais" }),
        Entry("10000000-0000-0000-0000-000000000018", CatalogEntryType.Origin, "Pupilo da Guilda", new { MainBenefit = "Recebe 5 pontos extras de perícia para investir em Dungeonologia", PrimarySkill = "Dungeonologia", SecondarySkill = "Estratégia", StartingEquipment = "Mapa desatualizado da Guilda", NarrativeHook = "Cresceu dentro da própria Guilda, filho de um veterano" }),
        Entry("10000000-0000-0000-0000-000000000019", CatalogEntryType.Origin, "Caçador de Recompensas", new { MainBenefit = "-1 dificuldade em Rastreamento de um alvo específico definido", PrimarySkill = "Rastreamento", SecondarySkill = "Intimidação", StartingEquipment = "Grilhões, arco leve", NarrativeHook = "Vivia de capturar fugitivos e criaturas fugidas" }),
        Entry("10000000-0000-0000-0000-000000000020", CatalogEntryType.Origin, "Estudante Arcano", new { MainBenefit = "-1 dificuldade no primeiro teste de qualquer nova magia aprendida", PrimarySkill = "Controle Mágico", SecondarySkill = "Teoria Arcana", StartingEquipment = "Grimório incompleto", NarrativeHook = "Estudou magia formalmente, mas nunca se formou" }),
    ];
}
```

- [ ] **Step 3: Create the Backgrounds seed data (GDD §6.1.4)**

```csharp
// src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Backgrounds.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Backgrounds =
    [
        Entry("20000000-0000-0000-0000-000000000001", CatalogEntryType.Background, "Sobrevivente de Ruína", new { TriggeringEvent = "Explorou uma construção antiga e escapou", Benefit = "-1 dificuldade para identificar riscos estruturais/desabamentos", Complication = "Algo daquela ruína ainda o procura" }),
        Entry("20000000-0000-0000-0000-000000000002", CatalogEntryType.Background, "Sobreviveu a uma Emboscada", new { TriggeringEvent = "Seu grupo anterior foi dizimado", Benefit = "1x por expedição, ignora a condição de Surpreendido", Complication = "Sofre reações intensas a situações que lembrem a emboscada" }),
        Entry("20000000-0000-0000-0000-000000000003", CatalogEntryType.Background, "Foi Preso", new { TriggeringEvent = "Passou tempo confinado, injustamente ou não", Benefit = "Vantagem para escapar de contenções físicas (cordas, algemas)", Complication = "Possui um registro criminal reconhecível por autoridades" }),
        Entry("20000000-0000-0000-0000-000000000004", CatalogEntryType.Background, "Serviu no Exército", new { TriggeringEvent = "Sua unidade foi dizimada em combate", Benefit = "Resistência maior ao medo em combate organizado", Complication = "Um superior sobrevivente o culpa pela derrota" }),
        Entry("20000000-0000-0000-0000-000000000005", CatalogEntryType.Background, "Estudou com um Mestre", new { TriggeringEvent = "Teve um mentor renomado que sumiu", Benefit = "Pode invocar o nome do mestre para abrir portas em um círculo específico", Complication = "O desaparecimento do mestre esconde algo perigoso" }),
        Entry("20000000-0000-0000-0000-000000000006", CatalogEntryType.Background, "Viveu nas Ruas", new { TriggeringEvent = "Período de miséria extrema", Benefit = "Aguenta mais tempo sem comida antes de sofrer penalidades", Complication = "Deve favores a uma rede do submundo" }),
        Entry("20000000-0000-0000-0000-000000000007", CatalogEntryType.Background, "Herdou uma Ferramenta", new { TriggeringEvent = "Recebeu um objeto de família com história", Benefit = "O item herdado carrega uma pequena propriedade extra", Complication = "Alguém mais também quer aquele objeto de volta" }),
        Entry("20000000-0000-0000-0000-000000000008", CatalogEntryType.Background, "Descobriu um Manuscrito", new { TriggeringEvent = "Achou um documento que não deveria ter achado", Benefit = "Conhece um fragmento raro de informação (nome, símbolo, local)", Complication = "Outros sabem que ele tem o manuscrito e o procuram" }),
        Entry("20000000-0000-0000-0000-000000000009", CatalogEntryType.Background, "Traído por um Aliado", new { TriggeringEvent = "Foi traído por alguém de confiança", Benefit = "-1 dificuldade para perceber traição/mentira de aliados próximos", Complication = "Penalidade em testes sociais para formar vínculos rápidos" }),
        Entry("20000000-0000-0000-0000-000000000010", CatalogEntryType.Background, "Salvou uma Vila", new { TriggeringEvent = "Feito heróico publicamente reconhecido", Benefit = "Reputação positiva e acesso a favores menores na região", Complication = "A vila cobra ajuda contínua; recusar custa reputação" }),
        Entry("20000000-0000-0000-0000-000000000011", CatalogEntryType.Background, "Perdeu Alguém na Dungeon", new { TriggeringEvent = "Um familiar desapareceu ou morreu em uma expedição", Benefit = "-1 dificuldade em testes ligados a rastrear aquele tipo de perigo específico", Complication = "Obsessão que pode levá-lo a riscos desnecessários" }),
        Entry("20000000-0000-0000-0000-000000000012", CatalogEntryType.Background, "Fez um Pacto Menor", new { TriggeringEvent = "Selou um pequeno acordo com uma entidade", Benefit = "Pequeno benefício sobrenatural (definido com o Mestre)", Complication = "A entidade cobrará algo em troca, em algum momento" }),
        Entry("20000000-0000-0000-0000-000000000013", CatalogEntryType.Background, "Sobreviveu a uma Doença Grave", new { TriggeringEvent = "Quase morreu de uma praga", Benefit = "Resistência aumentada contra doenças e venenos", Complication = "Carrega uma sequela física leve e permanente" }),
        Entry("20000000-0000-0000-0000-000000000014", CatalogEntryType.Background, "Acusado Injustamente", new { TriggeringEvent = "Teve a reputação manchada por um crime que não cometeu", Benefit = "Bônus em Diplomacia quando precisa se defender de acusações", Complication = "Ainda é malvisto ou procurado em determinado lugar" }),
        Entry("20000000-0000-0000-0000-000000000015", CatalogEntryType.Background, "Guardião de um Segredo", new { TriggeringEvent = "Sabe de algo perigoso que não devia saber", Benefit = "Possui informação valiosa, negociável", Complication = "Outros sabem que ele sabe — e isso o torna um alvo" }),
        Entry("20000000-0000-0000-0000-000000000016", CatalogEntryType.Background, "Marcado por um Ritual", new { TriggeringEvent = "Passou por um ritual incompleto", Benefit = "Sensibilidade leve a presenças mágicas próximas", Complication = "A marca do ritual é perceptível ou reage mal a certos estímulos" }),
        Entry("20000000-0000-0000-0000-000000000017", CatalogEntryType.Background, "Resgatado por Estranhos", new { TriggeringEvent = "Deve a vida a alguém que nunca identificou", Benefit = "Possui um contato misterioso que pode ajudar 1x", Complication = "Não sabe quem foi — a dívida pode ser cobrada a qualquer momento" }),
        Entry("20000000-0000-0000-0000-000000000018", CatalogEntryType.Background, "Perdeu Tudo em um Desastre", new { TriggeringEvent = "Um incêndio ou colapso destruiu sua vida anterior", Benefit = "Bônus de Vontade contra desespero e perda", Complication = "Não possui posses, contatos ou apoio financeiro antigos" }),
        Entry("20000000-0000-0000-0000-000000000019", CatalogEntryType.Background, "Testemunhou uma Ruptura", new { TriggeringEvent = "Viu de perto o fenômeno mais temido do mundo", Benefit = "Resistência a pânico diante de fenômenos dimensionais", Complication = "Hipervigilância: penalidade em ambientes que lembram o evento" }),
        Entry("20000000-0000-0000-0000-000000000020", CatalogEntryType.Background, "Criado pela Guilda", new { TriggeringEvent = "Cresceu dentro da própria instituição", Benefit = "Bônus em testes administrativos/burocráticos internos da Guilda", Complication = "Nunca teve vida \"normal\": penalidade leve em situações sociais fora da Guilda" }),
    ];
}
```

- [ ] **Step 4: Wire the seed data into the entity configuration**

Edit `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs` — add the `using` for `Ruptura.Infrastructure.Data.Seed` and, inside `Configure`, after the two `HasIndex` calls:

```csharp
        builder.HasData(CatalogSeedData.Origins);
        builder.HasData(CatalogSeedData.Backgrounds);
```

- [ ] **Step 5: Generate the migration**

Run:
```bash
dotnet ef migrations add SeedOriginsAndBackgrounds \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```
Expected: a migration with 40 `InsertData` rows into `CatalogEntries` (20 Origins + 20 Backgrounds), each with `CampaignId = null` and `CreatedByGameMasterId = null`.

- [ ] **Step 6: Verify the solution builds**

Run: `dotnet build`
Expected: `Build succeeded.`

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Infrastructure/Data/Seed/ \
        src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs \
        src/Ruptura.Infrastructure/Data/Migrations/
git commit -m "feat: seed official Origins and Backgrounds catalog entries"
```

---

### Task 7: Seed data — Lineages (10), Aptitudes (6), Initial Talents (20)

**Files:**
- Create: `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Lineages.cs`
- Create: `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Aptitudes.cs`
- Create: `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Talents.cs`
- Modify: `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs`
- Create (generated): `src/Ruptura.Infrastructure/Data/Migrations/<timestamp>_SeedLineagesAptitudesTalents.cs`

**Interfaces:**
- Consumes: `CatalogSeedData.Entry(...)` (Task 6)
- Produces: `CatalogSeedData.Lineages`, `CatalogSeedData.Aptitudes`, `CatalogSeedData.Talents`

Content from GDD §6.1.7 (Linhagens), §6.1.5 (Aptidões), §6.1.6 (Talentos Iniciais). Talent `PowerTier` is always `"menor"` for every Initial Talent, per the GDD's own rule ("equivale sempre a 'Talento menor'").

- [ ] **Step 1: Create the Lineages seed data (GDD §6.1.7)**

```csharp
// src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Lineages.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Lineages =
    [
        Entry("30000000-0000-0000-0000-000000000001", CatalogEntryType.Lineage, "Humano", new { RacialAdjustment = "Nenhum (todos os atributos no teto padrão 5)", RacialTrait = "Adaptável: pode trocar 1 Aptidão escolhida na criação, 1x na campanha" }),
        Entry("30000000-0000-0000-0000-000000000002", CatalogEntryType.Lineage, "Anão", new { RacialAdjustment = "+1 máx. Vigor / −1 máx. Controle", RacialTrait = "Resistência a venenos e doenças" }),
        Entry("30000000-0000-0000-0000-000000000003", CatalogEntryType.Lineage, "Elfo", new { RacialAdjustment = "+1 máx. Percepção / −1 máx. Corpo", RacialTrait = "Visão em baixa luminosidade" }),
        Entry("30000000-0000-0000-0000-000000000004", CatalogEntryType.Lineage, "Meio-Orc", new { RacialAdjustment = "+1 máx. Corpo / −1 máx. Intelecto", RacialTrait = "1x por expedição, ignora uma penalidade de ferimento leve" }),
        Entry("30000000-0000-0000-0000-000000000005", CatalogEntryType.Lineage, "Halfling", new { RacialAdjustment = "+1 máx. Controle / −1 máx. Presença", RacialTrait = "-1 dificuldade em testes de Furtividade" }),
        Entry("30000000-0000-0000-0000-000000000006", CatalogEntryType.Lineage, "Gnomo", new { RacialAdjustment = "+1 máx. Intelecto / −1 máx. Vigor", RacialTrait = "-1 dificuldade no primeiro teste de qualquer perícia de Artesanato aprendida" }),
        Entry("30000000-0000-0000-0000-000000000007", CatalogEntryType.Lineage, "Meio-Elfo", new { RacialAdjustment = "Jogador escolhe livremente qual atributo recebe +1 e qual recebe −1", RacialTrait = "Aptidão extra pode ser trocada 1x (versatilidade)" }),
        Entry("30000000-0000-0000-0000-000000000008", CatalogEntryType.Lineage, "Draconato", new { RacialAdjustment = "+1 máx. Presença / −1 máx. Controle", RacialTrait = "Resistência a um tipo elemental (escolhido na criação)" }),
        Entry("30000000-0000-0000-0000-000000000009", CatalogEntryType.Lineage, "Descendente Sombrio", new { RacialAdjustment = "+1 máx. Vontade / −1 máx. Presença", RacialTrait = "Resistência a medo sobrenatural" }),
        Entry("30000000-0000-0000-0000-000000000010", CatalogEntryType.Lineage, "Fragmentado", new { RacialAdjustment = "+1 máx. Afinidade / −1 máx. Vigor", RacialTrait = "Sente a proximidade de Rupturas e instabilidade dimensional — liga-se diretamente à cosmologia. Rara, exige aprovação do Mestre." }),
    ];
}
```

- [ ] **Step 2: Create the Aptitudes seed data (GDD §6.1.5)**

```csharp
// src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Aptitudes.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Aptitudes =
    [
        Entry("40000000-0000-0000-0000-000000000001", CatalogEntryType.Aptitude, "Combate", new { CoveredAreas = new[] { "Combate — Armas", "Combate — Defesa", "Combate Corporal", "Combate à Distância" } }),
        Entry("40000000-0000-0000-0000-000000000002", CatalogEntryType.Aptitude, "Exploração", new { CoveredAreas = new[] { "Exploração" } }),
        Entry("40000000-0000-0000-0000-000000000003", CatalogEntryType.Aptitude, "Conhecimento", new { CoveredAreas = new[] { "Conhecimento", "Cura" } }),
        Entry("40000000-0000-0000-0000-000000000004", CatalogEntryType.Aptitude, "Ofício", new { CoveredAreas = new[] { "Artesanato", "Alquimia" } }),
        Entry("40000000-0000-0000-0000-000000000005", CatalogEntryType.Aptitude, "Magia", new { CoveredAreas = new[] { "Magia" } }),
        Entry("40000000-0000-0000-0000-000000000006", CatalogEntryType.Aptitude, "Liderança", new { CoveredAreas = new[] { "Social" } }),
    ];
}
```

- [ ] **Step 3: Create the Initial Talents seed data (GDD §6.1.6)**

```csharp
// src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Talents.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Talents =
    [
        Entry("50000000-0000-0000-0000-000000000001", CatalogEntryType.Talent, "Golpe Certeiro", new { Category = "Combate", Effect = "1x por combate, repete um dado de ataque que considere ruim", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000002", CatalogEntryType.Talent, "Reflexos de Combate", new { Category = "Combate", Effect = "+1 na primeira Esquiva de cada combate", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000003", CatalogEntryType.Talent, "Fúria Contida", new { Category = "Combate", Effect = "1x por combate, ignora a primeira penalidade de ferimento leve", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000004", CatalogEntryType.Talent, "Faro para o Perigo", new { Category = "Exploração", Effect = "-1 dificuldade no primeiro teste de Percepção de cada andar", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000005", CatalogEntryType.Talent, "Pé Leve", new { Category = "Exploração", Effect = "Não sofre penalidade de terreno difícil ao se mover sozinho", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000006", CatalogEntryType.Talent, "Instinto de Sobrevivência", new { Category = "Exploração", Effect = "1x por expedição, evita ficar sem uma ração/tocha por um dia", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000007", CatalogEntryType.Talent, "Mãos Habilidosas", new { Category = "Produção", Effect = "Reduz em 1 dia o tempo do primeiro projeto de fabricação de cada interlúdio", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000008", CatalogEntryType.Talent, "Olho Clínico", new { Category = "Produção", Effect = "Identifica automaticamente a Qualidade de um item ao examiná-lo", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000009", CatalogEntryType.Talent, "Precisão Artesanal", new { Category = "Produção", Effect = "1x por interlúdio, trata um resultado \"Sucesso\" de fabricação como \"Grande Sucesso\"", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000010", CatalogEntryType.Talent, "Reciclador", new { Category = "Produção", Effect = "Recupera metade dos materiais ao falhar em uma fabricação", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000011", CatalogEntryType.Talent, "Vislumbre Arcano", new { Category = "Arcanos", Effect = "Sente a presença de magia ativa num raio curto, sem gastar ação", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000012", CatalogEntryType.Talent, "Fôlego Ritual", new { Category = "Arcanos", Effect = "+1 PA disponível especificamente para conjurar magia, 1x por expedição", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000013", CatalogEntryType.Talent, "Toque Elemental", new { Category = "Arcanos", Effect = "Gera um efeito elemental cosmético/mínimo (luz, calor leve, brisa) sem gastar PA", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000014", CatalogEntryType.Talent, "Memória Arcana", new { Category = "Arcanos", Effect = "1x por pesquisa, reduz o tempo necessário em 1 dia", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000015", CatalogEntryType.Talent, "Presença Firme", new { Category = "Social", Effect = "+1 em testes de Intimidação/Liderança quando em desvantagem numérica", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000016", CatalogEntryType.Talent, "Voz Confiável", new { Category = "Social", Effect = "1x por interlúdio, obtém uma informação de um NPC sem precisar de teste", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000017", CatalogEntryType.Talent, "Diplomata Nato", new { Category = "Social", Effect = "-1 dificuldade no primeiro teste de Diplomacia com uma facção desconhecida", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000018", CatalogEntryType.Talent, "Sorte de Recruta", new { Category = "Extraordinário", Effect = "1x por expedição, transforma uma Falha (não crítica) em Sucesso simples", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000019", CatalogEntryType.Talent, "Marca Estranha", new { Category = "Extraordinário", Effect = "Traço sobrenatural pequeno e inexplicado (definido com o Mestre) — narrativamente rico, mecanicamente neutro até ser investigado em jogo", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000020", CatalogEntryType.Talent, "Sina Protegida", new { Category = "Extraordinário", Effect = "1x na campanha inteira, sobrevive a um golpe que o mataria, ficando Incapacitado em vez de morto (efeito consumido após o uso)", PowerTier = "menor" }),
    ];
}
```

- [ ] **Step 4: Wire the new seed data into the entity configuration**

Edit `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs` — add after the existing two `HasData` calls:

```csharp
        builder.HasData(CatalogSeedData.Lineages);
        builder.HasData(CatalogSeedData.Aptitudes);
        builder.HasData(CatalogSeedData.Talents);
```

- [ ] **Step 5: Generate the migration**

Run:
```bash
dotnet ef migrations add SeedLineagesAptitudesTalents \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```
Expected: a migration with 36 `InsertData` rows (10 Lineages + 6 Aptitudes + 20 Talents).

- [ ] **Step 6: Verify the solution builds**

Run: `dotnet build`
Expected: `Build succeeded.`

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Lineages.cs \
        src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Aptitudes.cs \
        src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Talents.cs \
        src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs \
        src/Ruptura.Infrastructure/Data/Migrations/
git commit -m "feat: seed official Lineages, Aptitudes, and Initial Talents catalog entries"
```

---

### Task 8: Seed data — Fundamental Skills (59)

**Files:**
- Create: `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Skills.cs`
- Modify: `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs`
- Create (generated): `src/Ruptura.Infrastructure/Data/Migrations/<timestamp>_SeedSkills.cs`

**Interfaces:**
- Consumes: `CatalogSeedData.Entry(...)` (Task 6)
- Produces: `CatalogSeedData.Skills`

Content from GDD §6.4 — the "Perícia" (skill) names only, one `CatalogEntry` per skill (not per Especialização, which stays free text on the character sheet later, out of scope here). `RelatedAttribute` is the primary attribute the GDD lists for that Area (where the GDD lists two, e.g. "Controle; Corpo em golpes brutos", the first/primary one is used — the secondary use case is a GM/table judgment call, not modeled as data).

- [ ] **Step 1: Create the Skills seed data (GDD §6.4)**

```csharp
// src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Skills.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Skills =
    [
        // Combate — Armas (Controle; Corpo em golpes brutos)
        Entry("60000000-0000-0000-0000-000000000001", CatalogEntryType.Skill, "Espadas", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000002", CatalogEntryType.Skill, "Machados", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000003", CatalogEntryType.Skill, "Martelos", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000004", CatalogEntryType.Skill, "Lanças", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000005", CatalogEntryType.Skill, "Armas Improvisadas", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000006", CatalogEntryType.Skill, "Armas Exóticas", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),

        // Combate — Defesa (Controle/Vigor)
        Entry("60000000-0000-0000-0000-000000000007", CatalogEntryType.Skill, "Escudos", new { Area = "Combate — Defesa", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000008", CatalogEntryType.Skill, "Armaduras", new { Area = "Combate — Defesa", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000009", CatalogEntryType.Skill, "Esquiva", new { Area = "Combate — Defesa", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000010", CatalogEntryType.Skill, "Bloqueio", new { Area = "Combate — Defesa", RelatedAttribute = "Controle" }),

        // Combate Corporal (Corpo/Controle)
        Entry("60000000-0000-0000-0000-000000000011", CatalogEntryType.Skill, "Artes Marciais", new { Area = "Combate Corporal", RelatedAttribute = "Corpo" }),
        Entry("60000000-0000-0000-0000-000000000012", CatalogEntryType.Skill, "Luta Desarmada", new { Area = "Combate Corporal", RelatedAttribute = "Corpo" }),
        Entry("60000000-0000-0000-0000-000000000013", CatalogEntryType.Skill, "Agarramento", new { Area = "Combate Corporal", RelatedAttribute = "Corpo" }),

        // Combate à Distância (Controle)
        Entry("60000000-0000-0000-0000-000000000014", CatalogEntryType.Skill, "Arcos", new { Area = "Combate à Distância", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000015", CatalogEntryType.Skill, "Bestas", new { Area = "Combate à Distância", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000016", CatalogEntryType.Skill, "Armas de Arremesso", new { Area = "Combate à Distância", RelatedAttribute = "Controle" }),

        // Exploração (Percepção/Vigor/Controle)
        Entry("60000000-0000-0000-0000-000000000017", CatalogEntryType.Skill, "Percepção", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000018", CatalogEntryType.Skill, "Rastreamento", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000019", CatalogEntryType.Skill, "Sobrevivência", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000020", CatalogEntryType.Skill, "Navegação", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000021", CatalogEntryType.Skill, "Furtividade", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000022", CatalogEntryType.Skill, "Armadilhas", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000023", CatalogEntryType.Skill, "Exploração de Dungeon", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000024", CatalogEntryType.Skill, "Escalada", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000025", CatalogEntryType.Skill, "Natação", new { Area = "Exploração", RelatedAttribute = "Percepção" }),

        // Conhecimento (Intelecto)
        Entry("60000000-0000-0000-0000-000000000026", CatalogEntryType.Skill, "História", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000027", CatalogEntryType.Skill, "Geografia", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000028", CatalogEntryType.Skill, "Criaturas", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000029", CatalogEntryType.Skill, "Religião", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000030", CatalogEntryType.Skill, "Linguagens", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000031", CatalogEntryType.Skill, "Estratégia", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000032", CatalogEntryType.Skill, "Dungeonologia", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000033", CatalogEntryType.Skill, "Conhecimento de Animais", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000034", CatalogEntryType.Skill, "Ocultismo", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000035", CatalogEntryType.Skill, "Avaliação", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),

        // Cura (Intelecto/Percepção)
        Entry("60000000-0000-0000-0000-000000000036", CatalogEntryType.Skill, "Medicina", new { Area = "Cura", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000037", CatalogEntryType.Skill, "Cirurgia", new { Area = "Cura", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000038", CatalogEntryType.Skill, "Farmacologia", new { Area = "Cura", RelatedAttribute = "Intelecto" }),

        // Artesanato (Controle/Intelecto)
        Entry("60000000-0000-0000-0000-000000000039", CatalogEntryType.Skill, "Ferraria", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000040", CatalogEntryType.Skill, "Carpintaria", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000041", CatalogEntryType.Skill, "Alfaiataria", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000042", CatalogEntryType.Skill, "Engenharia", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000043", CatalogEntryType.Skill, "Construção", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000044", CatalogEntryType.Skill, "Criação de Equipamentos", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000045", CatalogEntryType.Skill, "Culinária", new { Area = "Artesanato", RelatedAttribute = "Controle" }),

        // Alquimia (Intelecto)
        Entry("60000000-0000-0000-0000-000000000046", CatalogEntryType.Skill, "Poções", new { Area = "Alquimia", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000047", CatalogEntryType.Skill, "Venenos", new { Area = "Alquimia", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000048", CatalogEntryType.Skill, "Materiais", new { Area = "Alquimia", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000049", CatalogEntryType.Skill, "Transmutação", new { Area = "Alquimia", RelatedAttribute = "Intelecto" }),

        // Magia (Afinidade)
        Entry("60000000-0000-0000-0000-000000000050", CatalogEntryType.Skill, "Controle Mágico", new { Area = "Magia", RelatedAttribute = "Afinidade" }),
        Entry("60000000-0000-0000-0000-000000000051", CatalogEntryType.Skill, "Teoria Arcana", new { Area = "Magia", RelatedAttribute = "Afinidade" }),
        Entry("60000000-0000-0000-0000-000000000052", CatalogEntryType.Skill, "Rituais", new { Area = "Magia", RelatedAttribute = "Afinidade" }),
        Entry("60000000-0000-0000-0000-000000000053", CatalogEntryType.Skill, "Afinidade Elemental", new { Area = "Magia", RelatedAttribute = "Afinidade" }),
        Entry("60000000-0000-0000-0000-000000000054", CatalogEntryType.Skill, "Encantamentos", new { Area = "Magia", RelatedAttribute = "Afinidade" }),

        // Social (Presença/Intelecto)
        Entry("60000000-0000-0000-0000-000000000055", CatalogEntryType.Skill, "Diplomacia", new { Area = "Social", RelatedAttribute = "Presença" }),
        Entry("60000000-0000-0000-0000-000000000056", CatalogEntryType.Skill, "Liderança", new { Area = "Social", RelatedAttribute = "Presença" }),
        Entry("60000000-0000-0000-0000-000000000057", CatalogEntryType.Skill, "Comércio", new { Area = "Social", RelatedAttribute = "Presença" }),
        Entry("60000000-0000-0000-0000-000000000058", CatalogEntryType.Skill, "Intimidação", new { Area = "Social", RelatedAttribute = "Presença" }),
        Entry("60000000-0000-0000-0000-000000000059", CatalogEntryType.Skill, "Manipulação", new { Area = "Social", RelatedAttribute = "Presença" }),
    ];
}
```

- [ ] **Step 2: Wire the new seed data into the entity configuration**

Edit `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs` — add after the existing `HasData` calls:

```csharp
        builder.HasData(CatalogSeedData.Skills);
```

- [ ] **Step 3: Generate the migration**

Run:
```bash
dotnet ef migrations add SeedSkills \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```
Expected: a migration with 59 `InsertData` rows.

- [ ] **Step 4: Verify the solution builds**

Run: `dotnet build`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Skills.cs \
        src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs \
        src/Ruptura.Infrastructure/Data/Migrations/
git commit -m "feat: seed official Fundamental Skills catalog entries"
```

---

### Task 9: Seed data — Example Spells (24) and Techniques (12)

**Files:**
- Create: `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Spells.cs`
- Create: `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Techniques.cs`
- Modify: `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs`
- Create (generated): `src/Ruptura.Infrastructure/Data/Migrations/<timestamp>_SeedSpellsAndTechniques.cs`

**Interfaces:**
- Consumes: `CatalogSeedData.Entry(...)` (Task 6)
- Produces: `CatalogSeedData.Spells`, `CatalogSeedData.Techniques`

Content from GDD §6.6.6 (Magias de Exemplo — 1 per School × 3 complexity tiers = 24) and §6.6.8 (Técnicas de Exemplo — 3 styles × 4 categories = 12). `ComplexityPaCost` follows §6.6.3 (Menor=1, Moderada=2, Maior=3). Technique `PaCost` is the Stage-I cost from the GDD table (Reação techniques use the Reaction, not PA, so `PaCost = 0`); the full staged effect (Stage I → Stage II) is captured in `Effect` text rather than split into separate entries, since the spec's `CharacterSheetData.Techniques[]` references one `CatalogEntryId` per known technique, not per stage.

- [ ] **Step 1: Create the Spells seed data (GDD §6.6.6)**

```csharp
// src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Spells.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Spells =
    [
        // Evocação
        Entry("70000000-0000-0000-0000-000000000001", CatalogEntryType.Spell, "Lança de Fogo", new { School = "Evocação", ComplexityPaCost = 1, Range = "Contato/Curta", Area = "Único Alvo", Duration = "Instantânea", Test = "Oposto vs. Vontade/Afinidade", Effect = "Dano de fogo instantâneo a 1 alvo" }),
        Entry("70000000-0000-0000-0000-000000000002", CatalogEntryType.Spell, "Rajada Flamejante", new { School = "Evocação", ComplexityPaCost = 2, Range = "Média", Area = "Linha", Duration = "Instantânea", Test = "Oposto vs. Vontade/Afinidade", Effect = "Dano maior + ignição leve" }),
        Entry("70000000-0000-0000-0000-000000000003", CatalogEntryType.Spell, "Tempestade de Chamas", new { School = "Evocação", ComplexityPaCost = 3, Range = "Média", Area = "Área Pequena", Duration = "2 turnos", Test = "Oposto vs. Vontade/Afinidade", Effect = "Dano contínuo por 2 turnos" }),

        // Abjuração
        Entry("70000000-0000-0000-0000-000000000004", CatalogEntryType.Spell, "Escudo Arcano", new { School = "Abjuração", ComplexityPaCost = 1, Range = "Pessoal", Area = "Único Alvo", Duration = "1 turno", Test = "Absoluto", Effect = "+2 Defesa Passiva, 1 turno" }),
        Entry("70000000-0000-0000-0000-000000000005", CatalogEntryType.Spell, "Barreira Protetora", new { School = "Abjuração", ComplexityPaCost = 2, Range = "Pessoal", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "+4 Defesa Passiva, Cena, só a si mesmo" }),
        Entry("70000000-0000-0000-0000-000000000006", CatalogEntryType.Spell, "Muralha Absoluta", new { School = "Abjuração", ComplexityPaCost = 3, Range = "Curta", Area = "Área Pequena", Duration = "Cena", Test = "Absoluto", Effect = "+4 Defesa Passiva à área pequena (aliados), Cena" }),

        // Controle
        Entry("70000000-0000-0000-0000-000000000007", CatalogEntryType.Spell, "Amarras de Vontade", new { School = "Controle", ComplexityPaCost = 1, Range = "Curta", Area = "Único Alvo", Duration = "1 turno", Test = "Oposto vs. Vontade", Effect = "Imobiliza 1 alvo, 1 turno" }),
        Entry("70000000-0000-0000-0000-000000000008", CatalogEntryType.Spell, "Grilhões Arcanos", new { School = "Controle", ComplexityPaCost = 2, Range = "Curta", Area = "Único Alvo", Duration = "2 turnos", Test = "Oposto vs. Vontade", Effect = "Imobiliza + Enfraquecido, 2 turnos" }),
        Entry("70000000-0000-0000-0000-000000000009", CatalogEntryType.Spell, "Prisão de Vontade", new { School = "Controle", ComplexityPaCost = 3, Range = "Curta", Area = "Área Pequena", Duration = "Cena", Test = "Oposto vs. Vontade", Effect = "Imobiliza área pequena, Cena" }),

        // Convocação
        Entry("70000000-0000-0000-0000-000000000010", CatalogEntryType.Spell, "Lâmina Espectral", new { School = "Convocação", ComplexityPaCost = 1, Range = "Pessoal", Area = "Único Alvo", Duration = "1 turno", Test = "Absoluto", Effect = "Invoca arma temporária (1 turno)" }),
        Entry("70000000-0000-0000-0000-000000000011", CatalogEntryType.Spell, "Familiar de Batalha", new { School = "Convocação", ComplexityPaCost = 2, Range = "Curta", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "Invoca criatura pequena, Cena" }),
        Entry("70000000-0000-0000-0000-000000000012", CatalogEntryType.Spell, "Avatar Convocado", new { School = "Convocação", ComplexityPaCost = 3, Range = "Curta", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "Invoca aliado poderoso, Cena, Conjuração Prolongada" }),

        // Transmutação
        Entry("70000000-0000-0000-0000-000000000013", CatalogEntryType.Spell, "Toque Deformante", new { School = "Transmutação", ComplexityPaCost = 1, Range = "Contato", Area = "Único Alvo", Duration = "Instantânea", Test = "Absoluto", Effect = "Altera superfície/objeto pequeno" }),
        Entry("70000000-0000-0000-0000-000000000014", CatalogEntryType.Spell, "Metamorfose Parcial", new { School = "Transmutação", ComplexityPaCost = 2, Range = "Pessoal", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "Altera parte do próprio corpo, ganho utilitário, Cena" }),
        Entry("70000000-0000-0000-0000-000000000015", CatalogEntryType.Spell, "Transfiguração Completa", new { School = "Transmutação", ComplexityPaCost = 3, Range = "Pessoal", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "Altera a forma por completo, Cena" }),

        // Ilusão
        Entry("70000000-0000-0000-0000-000000000016", CatalogEntryType.Spell, "Névoa Enganosa", new { School = "Ilusão", ComplexityPaCost = 1, Range = "Pessoal", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "Camufla 1 alvo, +Furtividade" }),
        Entry("70000000-0000-0000-0000-000000000017", CatalogEntryType.Spell, "Duplicata Ilusória", new { School = "Ilusão", ComplexityPaCost = 2, Range = "Pessoal", Area = "Único Alvo", Duration = "Instantânea", Test = "Absoluto", Effect = "Imagem falsa, confunde 1 ataque" }),
        Entry("70000000-0000-0000-0000-000000000018", CatalogEntryType.Spell, "Véu da Mentira", new { School = "Ilusão", ComplexityPaCost = 3, Range = "Curta", Area = "Área Grande", Duration = "Cena", Test = "Oposto vs. Vontade", Effect = "Ilude um grupo/área inteira, Cena" }),

        // Necromancia
        Entry("70000000-0000-0000-0000-000000000019", CatalogEntryType.Spell, "Toque Debilitante", new { School = "Necromancia", ComplexityPaCost = 1, Range = "Contato", Area = "Único Alvo", Duration = "Instantânea", Test = "Oposto vs. Vontade", Effect = "Dreno pequeno de PV/Vigor" }),
        Entry("70000000-0000-0000-0000-000000000020", CatalogEntryType.Spell, "Sopro Sombrio", new { School = "Necromancia", ComplexityPaCost = 2, Range = "Curta", Area = "Área Pequena", Duration = "Instantânea", Test = "Oposto vs. Vontade", Effect = "Dreno em área pequena" }),
        Entry("70000000-0000-0000-0000-000000000021", CatalogEntryType.Spell, "Chamado da Sepultura", new { School = "Necromancia", ComplexityPaCost = 3, Range = "Curta", Area = "Único Alvo", Duration = "Conjuração Prolongada", Test = "Absoluto", Effect = "Invoca mortos-vivos menores temporários, Conjuração Prolongada" }),

        // Adivinação
        Entry("70000000-0000-0000-0000-000000000022", CatalogEntryType.Spell, "Vislumbre", new { School = "Adivinação", ComplexityPaCost = 1, Range = "Curta", Area = "Único Alvo", Duration = "Instantânea", Test = "Absoluto", Effect = "Revela 1 informação simples sobre alvo/ambiente" }),
        Entry("70000000-0000-0000-0000-000000000023", CatalogEntryType.Spell, "Leitura do Fio do Destino", new { School = "Adivinação", ComplexityPaCost = 2, Range = "Curta", Area = "Único Alvo", Duration = "Instantânea", Test = "Absoluto", Effect = "Prevê a próxima ação de 1 alvo, concede Vantagem" }),
        Entry("70000000-0000-0000-0000-000000000024", CatalogEntryType.Spell, "Olho Onisciente", new { School = "Adivinação", ComplexityPaCost = 3, Range = "Cena", Area = "Área Grande", Duration = "Cena", Test = "Absoluto", Effect = "Revela mapa/segredos de uma área inteira, Cena" }),
    ];
}
```

- [ ] **Step 2: Create the Techniques seed data (GDD §6.6.8)**

```csharp
// src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Techniques.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Techniques =
    [
        // Espadas
        Entry("80000000-0000-0000-0000-000000000001", CatalogEntryType.Technique, "Postura Ofensiva", new { Style = "Espadas", Category = "Postura", PaCost = 1, Effect = "1 PA, +1 dano / −1 Defesa Passiva enquanto mantida" }),
        Entry("80000000-0000-0000-0000-000000000002", CatalogEntryType.Technique, "Golpe Giratório", new { Style = "Espadas", Category = "Técnica", PaCost = 1, Effect = "I (1 PA): atinge 2 alvos em Contato → II (2 PA, Mestre): atinge todos em Contato" }),
        Entry("80000000-0000-0000-0000-000000000003", CatalogEntryType.Technique, "Aparar", new { Style = "Espadas", Category = "Reação", PaCost = 0, Effect = "Reação, +Defesa Passiva contra 1 ataque; se suceder, permite contra-ataque com dano reduzido" }),
        Entry("80000000-0000-0000-0000-000000000004", CatalogEntryType.Technique, "Corte que Divide o Véu", new { Style = "Espadas", Category = "Suprema", PaCost = 3, Effect = "3 PA, 1x/combate: ignora metade da Redução de Dano da armadura e aplica Sangrando" }),

        // Combate Corporal (Luta Desarmada)
        Entry("80000000-0000-0000-0000-000000000005", CatalogEntryType.Technique, "Guarda Fechada", new { Style = "Combate Corporal", Category = "Postura", PaCost = 1, Effect = "1 PA, +2 Defesa Passiva / −1 dano enquanto mantida" }),
        Entry("80000000-0000-0000-0000-000000000006", CatalogEntryType.Technique, "Golpe Articulado", new { Style = "Combate Corporal", Category = "Técnica", PaCost = 1, Effect = "I (1 PA): ataque com chance de Atordoado leve → II (2 PA, Mestre): chance/efeito maior" }),
        Entry("80000000-0000-0000-0000-000000000007", CatalogEntryType.Technique, "Contragolpe", new { Style = "Combate Corporal", Category = "Reação", PaCost = 0, Effect = "Reação, se a Defesa Ativa suceder, aplica dano imediato ao atacante" }),
        Entry("80000000-0000-0000-0000-000000000008", CatalogEntryType.Technique, "Ruptura de Pontos Vitais", new { Style = "Combate Corporal", Category = "Suprema", PaCost = 3, Effect = "3 PA, 1x/combate: ignora totalmente a Redução de Dano da armadura, aplica Ferido Grave" }),

        // Arcos (Distância)
        Entry("80000000-0000-0000-0000-000000000009", CatalogEntryType.Technique, "Mira Calculada", new { Style = "Arcos", Category = "Postura", PaCost = 1, Effect = "1 PA, +1 precisão contra um alvo marcado, mantida até trocar de alvo" }),
        Entry("80000000-0000-0000-0000-000000000010", CatalogEntryType.Technique, "Tiro Encadeado", new { Style = "Arcos", Category = "Técnica", PaCost = 2, Effect = "I (2 PA): atinge 2 alvos na mesma linha → II (3 PA, Mestre): atinge até 4 alvos" }),
        Entry("80000000-0000-0000-0000-000000000011", CatalogEntryType.Technique, "Disparo de Interceptação", new { Style = "Arcos", Category = "Reação", PaCost = 0, Effect = "Reação, ataca um inimigo que entra na Zona Curta" }),
        Entry("80000000-0000-0000-0000-000000000012", CatalogEntryType.Technique, "Flecha que Perfura o Véu", new { Style = "Arcos", Category = "Suprema", PaCost = 3, Effect = "3 PA, 1x/combate: ignora Cobertura (Parcial/Total) e a Redução de Dano da armadura" }),
    ];
}
```

- [ ] **Step 3: Wire the new seed data into the entity configuration**

Edit `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs` — add after the existing `HasData` calls:

```csharp
        builder.HasData(CatalogSeedData.Spells);
        builder.HasData(CatalogSeedData.Techniques);
```

- [ ] **Step 4: Generate the migration**

Run:
```bash
dotnet ef migrations add SeedSpellsAndTechniques \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```
Expected: a migration with 36 `InsertData` rows (24 Spells + 12 Techniques).

- [ ] **Step 5: Verify the solution builds and unit tests pass**

Run: `dotnet build && dotnet test tests/Ruptura.UnitTests`
Expected: `Build succeeded.`; all unit tests pass. (Integration tests for the Catalog API endpoints don't exist yet — those are Task 10; the seeded data landing here is what lets Task 10's integration tests assert against real official entries from the moment they're written, instead of needing a follow-up fix.)

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Spells.cs \
        src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Techniques.cs \
        src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs \
        src/Ruptura.Infrastructure/Data/Migrations/
git commit -m "feat: seed official example Spells and Techniques catalog entries"
```

---

### Task 10: API controller, localization, integration tests

**Files:**
- Create: `src/Ruptura.API/Controllers/CatalogController.cs`
- Modify: `src/Ruptura.API/Resources/SharedResources.resx`
- Modify: `src/Ruptura.API/Resources/SharedResources.pt-BR.resx`
- Create: `tests/Ruptura.IntegrationTests/Controllers/CatalogControllerTests.cs`

**Interfaces:**
- Consumes: `ICatalogEntryService` (Task 5); DTOs (Task 4)
- Produces (HTTP):
  ```
  GET    /api/catalog?type={type}&campaignId={id}   [Authorize]
  POST   /api/catalog                                [Authorize(Roles=GameMaster)]
  PUT    /api/catalog/{id:guid}                       [Authorize(Roles=GameMaster)]
  DELETE /api/catalog/{id:guid}                       [Authorize(Roles=GameMaster)]
  ```

- [ ] **Step 1: Add localized messages (English)**

Edit `src/Ruptura.API/Resources/SharedResources.resx` — add before `<!-- Generic -->`:

```xml
  <!-- Catalog -->
  <data name="Catalog.NotFound"><value>Catalog entry not found.</value></data>
  <data name="Catalog.InvalidType"><value>Invalid catalog entry type.</value></data>
  <data name="Catalog.AlreadyExists"><value>An entry with this name already exists for this type.</value></data>
  <data name="Catalog.CannotModifyGlobalEntry"><value>Official catalog entries cannot be modified.</value></data>
  <data name="Catalog.Created"><value>Catalog entry created successfully.</value></data>
  <data name="Catalog.Updated"><value>Catalog entry updated successfully.</value></data>
  <data name="Catalog.Deleted"><value>Catalog entry deleted successfully.</value></data>
```

- [ ] **Step 2: Add localized messages (Portuguese)**

Edit `src/Ruptura.API/Resources/SharedResources.pt-BR.resx` — add before `<!-- Generic -->`:

```xml
  <!-- Catalog -->
  <data name="Catalog.NotFound"><value>Item de catálogo não encontrado.</value></data>
  <data name="Catalog.InvalidType"><value>Tipo de item de catálogo inválido.</value></data>
  <data name="Catalog.AlreadyExists"><value>Já existe um item com este nome para este tipo.</value></data>
  <data name="Catalog.CannotModifyGlobalEntry"><value>Itens oficiais do catálogo não podem ser modificados.</value></data>
  <data name="Catalog.Created"><value>Item de catálogo criado com sucesso.</value></data>
  <data name="Catalog.Updated"><value>Item de catálogo atualizado com sucesso.</value></data>
  <data name="Catalog.Deleted"><value>Item de catálogo apagado com sucesso.</value></data>
```

- [ ] **Step 3: Create `CatalogController`**

```csharp
// src/Ruptura.API/Controllers/CatalogController.cs
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/catalog")]
[Authorize]
public class CatalogController(
    ICatalogEntryService catalogService,
    IStringLocalizer<SharedResources> localizer,
    IValidator<CreateCatalogEntryRequest> createValidator,
    IValidator<UpdateCatalogEntryRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CatalogEntryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByType(
        [FromQuery] string type, [FromQuery] Guid campaignId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await catalogService.GetByTypeAsync(callerId, type, campaignId, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Catalog.InvalidType
                ? BadRequest(ApiResponse.Fail(localizer[result.Error!]))
                : NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<IEnumerable<CatalogEntryResponse>>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<CatalogEntryResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateCatalogEntryRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await catalogService.CreateAsync(gameMasterId, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Catalog.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CatalogEntryResponse>.Ok(result.Value!, localizer["Catalog.Created"]));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<CatalogEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCatalogEntryRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await catalogService.UpdateAsync(gameMasterId, id, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Catalog.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<CatalogEntryResponse>.Ok(result.Value!, localizer["Catalog.Updated"]));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await catalogService.DeleteAsync(gameMasterId, id, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Catalog.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse.Ok(localizer["Catalog.Deleted"]));
    }
}
```

- [ ] **Step 4: Write the integration tests**

These reuse the `SetupGameMasterWithInviteAsync`-style pattern from `CampaignControllerTests.cs` (register a GM, create a Campaign) — write it fresh here rather than sharing code across test classes, matching the existing style where each test class is self-contained.

```csharp
// tests/Ruptura.IntegrationTests/Controllers/CatalogControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.Common;

namespace Ruptura.IntegrationTests.Controllers;

public class CatalogControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, Guid CampaignId)> SetupGameMasterWithCampaignAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "Catalog Test Campaign"
        });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        return (client, campaign.Id);
    }

    [Fact]
    public async Task GetByType_ReturnsOfficialSeedEntries()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();

        var response = await client.GetAsync($"api/catalog?type=Origin&campaignId={campaignId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>();
        body!.Data.Should().NotBeEmpty();
        body.Data.Should().OnlyContain(e => e.Type == "Origin");
        body.Data.Should().Contain(e => e.IsGlobal);
    }

    [Fact]
    public async Task GetByType_WithInvalidType_Returns400()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();

        var response = await client.GetAsync($"api/catalog?type=NotAType&campaignId={campaignId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByType_WhenCallerNotCampaignMember_Returns404()
    {
        var (_, campaignId) = await SetupGameMasterWithCampaignAsync();
        var (strangerClient, _) = await SetupGameMasterWithCampaignAsync();

        var response = await strangerClient.GetAsync($"api/catalog?type=Origin&campaignId={campaignId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateHomebrewEntry_ThenListIncludesIt_Returns201()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();

        var createResponse = await client.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaignId,
            Type = "Talent",
            Name = "Talento Homebrew de Teste",
            DataJson = "{\"Category\":\"Combate\",\"Effect\":\"teste\",\"PowerTier\":\"menor\"}"
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await client.GetAsync($"api/catalog?type=Talent&campaignId={campaignId}");
        var list = (await listResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!;
        list.Should().Contain(e => e.Name == "Talento Homebrew de Teste" && !e.IsGlobal);
    }

    [Fact]
    public async Task CreateHomebrewEntry_WithDuplicateName_Returns400()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();
        var request = new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "Duplicado", DataJson = "{}"
        };
        await client.PostAsJsonAsync("api/catalog", request);

        var response = await client.PostAsJsonAsync("api/catalog", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateGlobalEntry_Returns400()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();
        var listResponse = await client.GetAsync($"api/catalog?type=Origin&campaignId={campaignId}");
        var globalEntry = (await listResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!.First();

        var response = await client.PutAsJsonAsync($"api/catalog/{globalEntry.Id}", new UpdateCatalogEntryRequest
        {
            Name = "Tentativa de Edição", DataJson = "{}"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateHomebrewEntry_ByAnotherGameMaster_Returns404()
    {
        var (owner, campaignId) = await SetupGameMasterWithCampaignAsync();
        var createResponse = await owner.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "Meu Talento", DataJson = "{}"
        });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>())!.Data!;

        var (stranger, _) = await SetupGameMasterWithCampaignAsync();
        var response = await stranger.PutAsJsonAsync($"api/catalog/{entry.Id}", new UpdateCatalogEntryRequest
        {
            Name = "Roubado", DataJson = "{}"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteHomebrewEntry_RemovesIt()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();
        var createResponse = await client.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "Para Apagar", DataJson = "{}"
        });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>())!.Data!;

        var deleteResponse = await client.DeleteAsync($"api/catalog/{entry.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await client.GetAsync($"api/catalog?type=Talent&campaignId={campaignId}");
        var list = (await listResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!;
        list.Should().NotContain(e => e.Id == entry.Id);
    }

    [Fact]
    public async Task WriteEndpoints_WithoutGameMasterRole_Return403()
    {
        var (gmClient, campaignId) = await SetupGameMasterWithCampaignAsync();

        // Register a player under this GM and try to create a homebrew entry as them.
        var inviteResponse = await gmClient.PostAsync("api/invites", null);
        var invite = (await inviteResponse.Content
            .ReadFromJsonAsync<ApiResponse<Ruptura.Shared.Invites.InviteCodeResponse>>())!.Data!;
        var player = await AuthHelper.RegisterPlayerAsync(factory.CreateClient(), invite.Code, Faker.Internet.Email());

        var playerClient = factory.CreateClient();
        AuthHelper.SetBearerToken(playerClient, player.AccessToken);

        var response = await playerClient.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "X", DataJson = "{}"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
```

- [ ] **Step 5: Run the integration tests**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter CatalogControllerTests`
Expected: all tests `Passed`. (Requires Docker for Testcontainers.) `GetByType_ReturnsOfficialSeedEntries` and `UpdateGlobalEntry_Returns400` rely on the official seed data landing in Tasks 6-9, which already ran by this point in the task order.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.API/Controllers/CatalogController.cs \
        src/Ruptura.API/Resources/SharedResources.resx \
        src/Ruptura.API/Resources/SharedResources.pt-BR.resx \
        tests/Ruptura.IntegrationTests/Controllers/CatalogControllerTests.cs
git commit -m "feat: add Catalog API endpoints"
```

---
### Task 11: Blazor client service

**Files:**
- Create: `src/Ruptura.Web/Services/ICatalogClientService.cs`
- Create: `src/Ruptura.Web/Services/CatalogClientService.cs`
- Modify: `src/Ruptura.Web/Program.cs`

**Interfaces:**
- Consumes: DTOs from `Ruptura.Shared.Catalog` (Task 4); HTTP routes from Task 10
- Produces:
  ```csharp
  public interface ICatalogClientService
  {
      Task<ApiResponse<IEnumerable<CatalogEntryResponse>>?> GetByTypeAsync(string type, Guid campaignId);
      Task<ApiResponse<CatalogEntryResponse>?> CreateAsync(CreateCatalogEntryRequest request);
      Task<ApiResponse<CatalogEntryResponse>?> UpdateAsync(Guid id, UpdateCatalogEntryRequest request);
      Task<ApiResponse?> DeleteAsync(Guid id);
  }
  ```

- [ ] **Step 1: Define `ICatalogClientService`**

```csharp
// src/Ruptura.Web/Services/ICatalogClientService.cs
using Ruptura.Shared.Catalog;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public interface ICatalogClientService
{
    Task<ApiResponse<IEnumerable<CatalogEntryResponse>>?> GetByTypeAsync(string type, Guid campaignId);
    Task<ApiResponse<CatalogEntryResponse>?> CreateAsync(CreateCatalogEntryRequest request);
    Task<ApiResponse<CatalogEntryResponse>?> UpdateAsync(Guid id, UpdateCatalogEntryRequest request);
    Task<ApiResponse?> DeleteAsync(Guid id);
}
```

- [ ] **Step 2: Implement `CatalogClientService`**

```csharp
// src/Ruptura.Web/Services/CatalogClientService.cs
using System.Net.Http.Json;
using System.Web;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public class CatalogClientService(IHttpClientFactory factory) : ICatalogClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<CatalogEntryResponse>>?> GetByTypeAsync(string type, Guid campaignId)
    {
        var query = $"api/catalog?type={HttpUtility.UrlEncode(type)}&campaignId={campaignId}";
        var response = await Http.GetAsync(query);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>();
    }

    public async Task<ApiResponse<CatalogEntryResponse>?> CreateAsync(CreateCatalogEntryRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/catalog", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>();
    }

    public async Task<ApiResponse<CatalogEntryResponse>?> UpdateAsync(Guid id, UpdateCatalogEntryRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/catalog/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>();
    }

    public async Task<ApiResponse?> DeleteAsync(Guid id)
    {
        var response = await Http.DeleteAsync($"api/catalog/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse>();
    }
}
```

`CreateAsync`/`UpdateAsync` deliberately read the response body regardless of status code (unlike the pre-Task-12-fix version of `CampaignClientService`) — this matches the fix already applied to `CampaignClientService.AssignMemberAsync` so `Catalog.AlreadyExists`/`Catalog.CannotModifyGlobalEntry` messages reach the admin UI instead of being discarded.

- [ ] **Step 3: Register the service in `Program.cs`**

Edit `src/Ruptura.Web/Program.cs` — add one line right after the existing `builder.Services.AddScoped<ICampaignClientService, CampaignClientService>();`:

```csharp
builder.Services.AddScoped<ICatalogClientService, CatalogClientService>();
```

- [ ] **Step 4: Build to verify it compiles**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/Services/ICatalogClientService.cs \
        src/Ruptura.Web/Services/CatalogClientService.cs \
        src/Ruptura.Web/Program.cs
git commit -m "feat: add Blazor client service for Catalog API"
```

---

### Task 12: GM Catalog admin page, nav link, end-to-end test

**Files:**
- Create: `src/Ruptura.Web/Pages/GmCatalog.razor`
- Modify: `src/Ruptura.Web/Layout/NavMenu.razor` (only if a direct link is desired — see Step 2)
- Modify: `src/Ruptura.Web/Pages/GmCampaignDetail.razor` (add a "Catalog" link into the Campaign)
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`
- Create: `tests/Ruptura.IntegrationTests/Controllers/CatalogFlowTests.cs`

**Interfaces:**
- Consumes: `ICatalogClientService` (Task 11)

**UX:** per spec §8, the route is `/gm/campaigns/{id}/catalog`, reached from the Campaign detail page (`GmCampaignDetail.razor`, built in `docs/superpowers/plans/2026-08-04-campaign-roster-foundation.md`) rather than the top-level sidebar — a catalog only makes sense in the context of a specific Campaign. A type selector (`<select>`) switches between the 9 `CatalogEntryType` values; the list shows official (read-only) rows first, then homebrew rows with Edit/Delete; a form below creates a new homebrew entry (`Name` + a raw-JSON `DataJson` textarea — a plain-text power-user field is intentional here: nine different types would otherwise need nine different structured forms, which is unwarranted scope for this slice).

- [ ] **Step 1: Add localized strings (English)**

Edit `src/Ruptura.Web/Resources/AppStrings.resx` — add near the `Gm.CampaignDetail.*` keys:

```xml
  <data name="Gm.Catalog.Title"><value>Catalog</value></data>
  <data name="Gm.Catalog.TypeLabel"><value>Type</value></data>
  <data name="Gm.Catalog.Official"><value>Official</value></data>
  <data name="Gm.Catalog.Homebrew"><value>Homebrew</value></data>
  <data name="Gm.Catalog.Empty"><value>No entries of this type yet.</value></data>
  <data name="Gm.Catalog.NamePlaceholder"><value>Name</value></data>
  <data name="Gm.Catalog.DataJsonPlaceholder"><value>{ "Field": "value" }</value></data>
  <data name="Gm.Catalog.Create"><value>Add Homebrew Entry</value></data>
  <data name="Gm.Catalog.Edit"><value>Edit</value></data>
  <data name="Gm.Catalog.Save"><value>Save</value></data>
  <data name="Gm.Catalog.Cancel"><value>Cancel</value></data>
  <data name="Gm.Catalog.Delete"><value>Delete</value></data>
  <data name="Gm.CampaignDetail.ViewCatalog"><value>Catalog</value></data>
```

- [ ] **Step 2: Add localized strings (Portuguese)**

Edit `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`:

```xml
  <data name="Gm.Catalog.Title"><value>Catálogo</value></data>
  <data name="Gm.Catalog.TypeLabel"><value>Tipo</value></data>
  <data name="Gm.Catalog.Official"><value>Oficial</value></data>
  <data name="Gm.Catalog.Homebrew"><value>Homebrew</value></data>
  <data name="Gm.Catalog.Empty"><value>Nenhum item deste tipo ainda.</value></data>
  <data name="Gm.Catalog.NamePlaceholder"><value>Nome</value></data>
  <data name="Gm.Catalog.DataJsonPlaceholder"><value>{ "Campo": "valor" }</value></data>
  <data name="Gm.Catalog.Create"><value>Adicionar Item Homebrew</value></data>
  <data name="Gm.Catalog.Edit"><value>Editar</value></data>
  <data name="Gm.Catalog.Save"><value>Salvar</value></data>
  <data name="Gm.Catalog.Cancel"><value>Cancelar</value></data>
  <data name="Gm.Catalog.Delete"><value>Apagar</value></data>
  <data name="Gm.CampaignDetail.ViewCatalog"><value>Catálogo</value></data>
```

- [ ] **Step 3: Add a link from the Campaign detail page**

Edit `src/Ruptura.Web/Pages/GmCampaignDetail.razor` — inside the `<div class="page-heading">` block, add a link to the catalog page for this Campaign, right after the closing `</h1>`:

```razor
    <div class="page-heading">
        <h1>@L["Gm.CampaignDetail.Members"]</h1>
        <a href="/gm/campaigns/@Id/catalog" class="btn btn-outline-secondary btn-sm" style="margin-top:.75rem">
            @L["Gm.CampaignDetail.ViewCatalog"]
        </a>
    </div>
```

- [ ] **Step 4: Create the Catalog admin page**

```razor
@page "/gm/campaigns/{CampaignId:guid}/catalog"
@attribute [Authorize(Roles = "GameMaster")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Catalog
@inject IStringLocalizer<AppStrings> L
@inject ICatalogClientService CatalogService

<PageTitle>@L["Gm.Catalog.Title"] — RUPTURA</PageTitle>

<div class="page-content">
    <div class="page-heading">
        <h1>@L["Gm.Catalog.Title"]</h1>
    </div>

    @if (!string.IsNullOrEmpty(_errorMessage))
    {
        <div class="alert-danger mb-4">@_errorMessage</div>
    }

    <div class="section-header">
        <span class="section-title">@L["Gm.Catalog.TypeLabel"]</span>
        <select class="form-select" style="width:220px" value="@_selectedType" @onchange="OnTypeChanged">
            @foreach (var type in Types)
            {
                <option value="@type">@type</option>
            }
        </select>
    </div>

    @if (_loading)
    {
        <div class="ledger-empty">
            <span class="spinner-border spinner-border-sm me-2"></span>@L["Common.Loading"]
        </div>
    }
    else if (_entries.Count == 0)
    {
        <div class="ledger-empty">
            <p>@L["Gm.Catalog.Empty"]</p>
        </div>
    }
    else
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table">
                <thead>
                    <tr>
                        <th>@L["Gm.CampaignDetail.Col.Name"]</th>
                        <th>@L["Gm.Catalog.TypeLabel"]</th>
                        <th>DataJson</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var entry in _entries)
                    {
                        <tr>
                            <td>@entry.Name</td>
                            <td>@(entry.IsGlobal ? L["Gm.Catalog.Official"] : L["Gm.Catalog.Homebrew"])</td>
                            <td style="color:var(--text-muted);font-size:.75rem;max-width:320px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap">
                                @entry.DataJson
                            </td>
                            <td>
                                @if (!entry.IsGlobal)
                                {
                                    <button class="btn btn-outline-secondary btn-sm" @onclick="() => StartEdit(entry)">@L["Gm.Catalog.Edit"]</button>
                                    <button class="btn btn-outline-secondary btn-sm" @onclick="() => DeleteAsync(entry.Id)">@L["Gm.Catalog.Delete"]</button>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }

    <div class="section-header" style="margin-top:2rem">
        <span class="section-title">@(_editingId is null ? L["Gm.Catalog.Create"] : L["Gm.Catalog.Edit"])</span>
    </div>
    <div style="display:flex;flex-direction:column;gap:.75rem;max-width:480px">
        <input class="form-control" placeholder="@L["Gm.Catalog.NamePlaceholder"]" @bind="_formName" @bind:event="oninput" />
        <textarea class="form-control" rows="4" placeholder="@L["Gm.Catalog.DataJsonPlaceholder"]" @bind="_formDataJson" @bind:event="oninput"></textarea>
        <div style="display:flex;gap:.5rem">
            <button class="btn btn-primary btn-sm" @onclick="SaveAsync" disabled="@(_saving || string.IsNullOrWhiteSpace(_formName))">
                @if (_saving) { <span class="spinner-border spinner-border-sm me-1"></span> }
                @(_editingId is null ? L["Gm.Catalog.Create"] : L["Gm.Catalog.Save"])
            </button>
            @if (_editingId is not null)
            {
                <button class="btn btn-outline-secondary btn-sm" @onclick="CancelEdit">@L["Gm.Catalog.Cancel"]</button>
            }
        </div>
    </div>
</div>

@code {
    [Parameter] public Guid CampaignId { get; set; }

    private static readonly string[] Types =
    [
        "Origin", "Background", "Lineage", "Aptitude", "Talent",
        "Skill", "Spell", "Technique", "EquipmentItem"
    ];

    private string _selectedType = Types[0];
    private List<CatalogEntryResponse> _entries = [];
    private bool _loading = true;
    private bool _saving;
    private Guid? _editingId;
    private string _formName = string.Empty;
    private string _formDataJson = "{}";
    private string? _errorMessage;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task OnTypeChanged(ChangeEventArgs e)
    {
        _selectedType = e.Value?.ToString() ?? Types[0];
        CancelEdit();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        var result = await CatalogService.GetByTypeAsync(_selectedType, CampaignId);
        _entries = result?.Data?.ToList() ?? [];
        _loading = false;
    }

    private void StartEdit(CatalogEntryResponse entry)
    {
        _editingId = entry.Id;
        _formName = entry.Name;
        _formDataJson = entry.DataJson;
    }

    private void CancelEdit()
    {
        _editingId = null;
        _formName = string.Empty;
        _formDataJson = "{}";
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_formName)) return;

        _saving = true;
        _errorMessage = null;

        var result = _editingId is null
            ? await CatalogService.CreateAsync(new CreateCatalogEntryRequest
            {
                CampaignId = CampaignId, Type = _selectedType, Name = _formName, DataJson = _formDataJson
            })
            : await CatalogService.UpdateAsync(_editingId.Value, new UpdateCatalogEntryRequest
            {
                Name = _formName, DataJson = _formDataJson
            });

        if (result?.Data is not null)
        {
            CancelEdit();
            await LoadAsync();
        }
        else
        {
            _errorMessage = result?.Message ?? L["Common.Error"];
        }

        _saving = false;
    }

    private async Task DeleteAsync(Guid id)
    {
        _errorMessage = null;
        var result = await CatalogService.DeleteAsync(id);
        if (result?.Success == true)
        {
            await LoadAsync();
        }
        else
        {
            _errorMessage = result?.Message ?? L["Common.Error"];
        }
    }
}
```

- [ ] **Step 5: Build to verify it compiles**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 6: Write the end-to-end integration test**

```csharp
// tests/Ruptura.IntegrationTests/Controllers/CatalogFlowTests.cs
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.Common;

namespace Ruptura.IntegrationTests.Controllers;

public class CatalogFlowTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    [Fact]
    public async Task FullFlow_ReadOfficialCreateHomebrewEditDelete_Succeeds()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "Catalog Flow Campaign"
        });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        // 1. Official skills are visible immediately (seeded data).
        var skillsResponse = await client.GetAsync($"api/catalog?type=Skill&campaignId={campaign.Id}");
        var skills = (await skillsResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!.ToList();
        skills.Should().Contain(s => s.Name == "Espadas" && s.IsGlobal);

        // 2. Create a homebrew Talent.
        var createResponse = await client.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaign.Id,
            Type = "Talent",
            Name = "Coragem Inabalável",
            DataJson = "{\"Category\":\"Combate\",\"Effect\":\"teste\",\"PowerTier\":\"menor\"}"
        });
        var created = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>())!.Data!;
        created.IsGlobal.Should().BeFalse();

        // 3. Edit it.
        var updateResponse = await client.PutAsJsonAsync($"api/catalog/{created.Id}", new UpdateCatalogEntryRequest
        {
            Name = "Coragem Inabalável (Revisado)", DataJson = created.DataJson
        });
        updateResponse.EnsureSuccessStatusCode();

        var talentsResponse = await client.GetAsync($"api/catalog?type=Talent&campaignId={campaign.Id}");
        var talents = (await talentsResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!;
        talents.Should().Contain(t => t.Name == "Coragem Inabalável (Revisado)");

        // 4. Delete it.
        var deleteResponse = await client.DeleteAsync($"api/catalog/{created.Id}");
        deleteResponse.EnsureSuccessStatusCode();

        var talentsAfterDelete = (await (await client.GetAsync($"api/catalog?type=Talent&campaignId={campaign.Id}")).Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!;
        talentsAfterDelete.Should().NotContain(t => t.Id == created.Id);
    }
}
```

- [ ] **Step 7: Run the full test suite**

Run: `dotnet test`
Expected: all unit and integration tests `Passed` (aside from the pre-existing, documented Serilog `ReloadableLogger` parallel-host flake — see `docs/superpowers/plans/2026-08-04-campaign-roster-foundation.md`'s final review notes; re-run once if a single unrelated test fails).

- [ ] **Step 8: Commit**

```bash
git add src/Ruptura.Web/Pages/GmCatalog.razor \
        src/Ruptura.Web/Pages/GmCampaignDetail.razor \
        src/Ruptura.Web/Resources/AppStrings.resx \
        src/Ruptura.Web/Resources/AppStrings.pt-BR.resx \
        tests/Ruptura.IntegrationTests/Controllers/CatalogFlowTests.cs
git commit -m "feat: add GM catalog admin page and end-to-end flow test"
```

---

## What this plan does not cover (next plans in the sequence)

- `CharacterSheet` core (entity, `CharacterStatsCalculator`, all 11 module tabs) — spec §4.3, §5. This is the next plan; it references `CatalogEntry` rows by Id for Skills/Talents/Spells/Techniques/Equipment.
- `CharacterJournalEntry` and media storage (`IFileStorageService`) — spec §4.4, §7.
- `Notification` / rank-promotion — spec §4.5.
- Official `EquipmentItem` seed data (weapons/armor) — deliberately excluded from this plan's seed list per spec §3 ("Bestiário, lista de materiais e outros catálogos globais... populá-los é trabalho futuro"); the `EquipmentItem` type exists in the enum and is fully usable for homebrew today, just has no official rows yet.
