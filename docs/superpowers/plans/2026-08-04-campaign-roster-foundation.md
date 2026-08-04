# Campaign & Roster Foundation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Introduce the `Campaign` entity as the root container for a GM's game, wire the Mestre↔Jogador roster relationship, and let a GM assign roster players to a Campaign — the hard prerequisite for the Character Sheet feature (see `docs/superpowers/specs/2026-08-04-character-sheet-design.md`, §4.1).

**Architecture:** Follows existing Clean Architecture conventions exactly (Domain entity → `IRepository<T>` + specific repository interface in Application, implemented in Infrastructure → `Result<T>`-returning Application service → API controller with `ApiResponse<T>` + FluentValidation + localized errors → Blazor client service → Razor pages). No new architectural patterns introduced.

**Tech Stack:** ASP.NET Core 8 Web API, EF Core 8 + Npgsql, ASP.NET Core Identity, FluentValidation, Blazor WebAssembly, xUnit + Moq + FluentAssertions + Bogus (unit), Testcontainers.PostgreSql + `WebApplicationFactory<Program>` (integration).

## Global Constraints

- Result pattern only — services return `Result`/`Result<T>`, never throw business exceptions across layer boundaries (`CLAUDE.md`).
- Every user-facing string goes through `IStringLocalizer` (API: `SharedResources`, Web: `AppStrings`) with **both** `en` and `pt-BR` `.resx` entries — this project is bilingual.
- New EF migrations: `dotnet ef migrations add <Name> --project src/Ruptura.Infrastructure --startup-project src/Ruptura.API`.
- A player can belong to exactly one GM's roster (`ApplicationUser.RecruitedByGameMasterId`, set once at registration from the invite code used).
- All new GM-only API routes: `[Authorize(Roles = "GameMaster")]`.

---

### Task 1: Domain entities — `Campaign` and `CampaignMembership`

**Files:**
- Create: `src/Ruptura.Domain/Entities/Campaign.cs`
- Create: `src/Ruptura.Domain/Entities/CampaignMembership.cs`

**Interfaces:**
- Produces: `Ruptura.Domain.Entities.Campaign { Id, Name, GameMasterId, CreatedAt, UpdatedAt }`
- Produces: `Ruptura.Domain.Entities.CampaignMembership { Id, CampaignId, PlayerId, AssignedAt }`

- [ ] **Step 1: Create the `Campaign` entity**

```csharp
// src/Ruptura.Domain/Entities/Campaign.cs
namespace Ruptura.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid GameMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Step 2: Create the `CampaignMembership` entity**

```csharp
// src/Ruptura.Domain/Entities/CampaignMembership.cs
namespace Ruptura.Domain.Entities;

public class CampaignMembership
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid PlayerId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Step 3: Build to verify it compiles**

Run: `dotnet build src/Ruptura.Domain/Ruptura.Domain.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Domain/Entities/Campaign.cs src/Ruptura.Domain/Entities/CampaignMembership.cs
git commit -m "feat: add Campaign and CampaignMembership domain entities"
```

---

### Task 2: EF Core wiring — `ApplicationUser.RecruitedByGameMasterId`, `DbContext`, unique index, migration

**Files:**
- Modify: `src/Ruptura.Infrastructure/Identity/ApplicationUser.cs`
- Modify: `src/Ruptura.Infrastructure/Data/AppDbContext.cs`
- Create: `src/Ruptura.Infrastructure/Data/Configurations/CampaignMembershipConfiguration.cs`
- Create (generated): `src/Ruptura.Infrastructure/Data/Migrations/<timestamp>_AddCampaignAndRoster.cs`

**Interfaces:**
- Consumes: `Ruptura.Domain.Entities.Campaign`, `Ruptura.Domain.Entities.CampaignMembership` (Task 1)
- Produces: `ApplicationUser.RecruitedByGameMasterId (Guid?)`; `AppDbContext.Campaigns (DbSet<Campaign>)`; `AppDbContext.CampaignMemberships (DbSet<CampaignMembership>)`; unique index on `(CampaignId, PlayerId)`

- [ ] **Step 1: Add `RecruitedByGameMasterId` to `ApplicationUser`**

Edit `src/Ruptura.Infrastructure/Identity/ApplicationUser.cs` — add one property after `CreatedAt`:

```csharp
public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? RecruitedByGameMasterId { get; set; }
}
```

- [ ] **Step 2: Register the new `DbSet`s in `AppDbContext`**

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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

- [ ] **Step 3: Add the `CampaignMembership` unique index configuration**

This is the first `IEntityTypeConfiguration<T>` in the project — `OnModelCreating` already calls `ApplyConfigurationsFromAssembly`, so it's picked up automatically.

```csharp
// src/Ruptura.Infrastructure/Data/Configurations/CampaignMembershipConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CampaignMembershipConfiguration : IEntityTypeConfiguration<CampaignMembership>
{
    public void Configure(EntityTypeBuilder<CampaignMembership> builder)
    {
        builder.HasIndex(m => new { m.CampaignId, m.PlayerId }).IsUnique();
    }
}
```

- [ ] **Step 4: Generate the migration**

Run:
```bash
dotnet ef migrations add AddCampaignAndRoster \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```
Expected: a new file `src/Ruptura.Infrastructure/Data/Migrations/<timestamp>_AddCampaignAndRoster.cs` is created, adding tables `Campaigns`, `CampaignMemberships`, a unique index `IX_CampaignMemberships_CampaignId_PlayerId`, and a new nullable `RecruitedByGameMasterId` column on `AspNetUsers`.

- [ ] **Step 5: Verify the migration applies cleanly**

Run: `dotnet build` (whole solution) to confirm the migration compiles.
Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Infrastructure/Identity/ApplicationUser.cs \
        src/Ruptura.Infrastructure/Data/AppDbContext.cs \
        src/Ruptura.Infrastructure/Data/Configurations/CampaignMembershipConfiguration.cs \
        src/Ruptura.Infrastructure/Data/Migrations/
git commit -m "feat: add Campaign/CampaignMembership tables and player roster column"
```

---

### Task 3: Repositories

**Files:**
- Create: `src/Ruptura.Application/Interfaces/ICampaignRepository.cs`
- Create: `src/Ruptura.Application/Interfaces/ICampaignMembershipRepository.cs`
- Create: `src/Ruptura.Infrastructure/Repositories/CampaignRepository.cs`
- Create: `src/Ruptura.Infrastructure/Repositories/CampaignMembershipRepository.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`

**Interfaces:**
- Consumes: `Campaign`, `CampaignMembership` (Task 1); `AppDbContext` (Task 2); `IRepository<T>`, `BaseRepository<T>` (existing)
- Produces:
  - `ICampaignRepository : IRepository<Campaign>` with `Task<IEnumerable<Campaign>> GetByGameMasterAsync(Guid gameMasterId, CancellationToken ct = default)`
  - `ICampaignMembershipRepository : IRepository<CampaignMembership>` with `Task<IEnumerable<CampaignMembership>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default)` and `Task<bool> ExistsAsync(Guid campaignId, Guid playerId, CancellationToken ct = default)`

- [ ] **Step 1: Define `ICampaignRepository`**

```csharp
// src/Ruptura.Application/Interfaces/ICampaignRepository.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICampaignRepository : IRepository<Campaign>
{
    Task<IEnumerable<Campaign>> GetByGameMasterAsync(Guid gameMasterId, CancellationToken ct = default);
}
```

- [ ] **Step 2: Define `ICampaignMembershipRepository`**

```csharp
// src/Ruptura.Application/Interfaces/ICampaignMembershipRepository.cs
using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICampaignMembershipRepository : IRepository<CampaignMembership>
{
    Task<IEnumerable<CampaignMembership>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid campaignId, Guid playerId, CancellationToken ct = default);
}
```

- [ ] **Step 3: Implement `CampaignRepository`**

```csharp
// src/Ruptura.Infrastructure/Repositories/CampaignRepository.cs
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CampaignRepository(AppDbContext db)
    : BaseRepository<Campaign>(db), ICampaignRepository
{
    public async Task<IEnumerable<Campaign>> GetByGameMasterAsync(
        Guid gameMasterId,
        CancellationToken ct = default) =>
        await Set
            .Where(c => c.GameMasterId == gameMasterId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
}
```

- [ ] **Step 4: Implement `CampaignMembershipRepository`**

```csharp
// src/Ruptura.Infrastructure/Repositories/CampaignMembershipRepository.cs
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CampaignMembershipRepository(AppDbContext db)
    : BaseRepository<CampaignMembership>(db), ICampaignMembershipRepository
{
    public async Task<IEnumerable<CampaignMembership>> GetByCampaignAsync(
        Guid campaignId,
        CancellationToken ct = default) =>
        await Set
            .Where(m => m.CampaignId == campaignId)
            .OrderBy(m => m.AssignedAt)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        Guid campaignId,
        Guid playerId,
        CancellationToken ct = default) =>
        await Set.AnyAsync(m => m.CampaignId == campaignId && m.PlayerId == playerId, ct);
}
```

- [ ] **Step 5: Register the repositories in DI**

Edit `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` — add two lines under the `// Repositories` comment:

```csharp
        // Repositories
        services.AddScoped<IInviteCodeRepository, InviteCodeRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ICampaignMembershipRepository, CampaignMembershipRepository>();
```

- [ ] **Step 6: Build to verify it compiles**

Run: `dotnet build`
Expected: `Build succeeded.`

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Application/Interfaces/ICampaignRepository.cs \
        src/Ruptura.Application/Interfaces/ICampaignMembershipRepository.cs \
        src/Ruptura.Infrastructure/Repositories/CampaignRepository.cs \
        src/Ruptura.Infrastructure/Repositories/CampaignMembershipRepository.cs \
        src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs
git commit -m "feat: add Campaign and CampaignMembership repositories"
```

---

### Task 4: Shared DTOs, error codes, validators

**Files:**
- Create: `src/Ruptura.Shared/Campaigns/CreateCampaignRequest.cs`
- Create: `src/Ruptura.Shared/Campaigns/CampaignResponse.cs`
- Create: `src/Ruptura.Shared/Campaigns/PlayerRosterResponse.cs`
- Create: `src/Ruptura.Shared/Campaigns/AssignMemberRequest.cs`
- Create: `src/Ruptura.Shared/Campaigns/CampaignMemberResponse.cs`
- Modify: `src/Ruptura.Application/Common/ErrorCodes.cs`
- Create: `src/Ruptura.Application/Validators/Campaigns/CreateCampaignRequestValidator.cs`
- Create: `src/Ruptura.Application/Validators/Campaigns/AssignMemberRequestValidator.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`

**Interfaces:**
- Produces:
  - `Ruptura.Shared.Campaigns.CreateCampaignRequest { Name }`
  - `Ruptura.Shared.Campaigns.CampaignResponse { Id, Name, CreatedAt }`
  - `Ruptura.Shared.Campaigns.PlayerRosterResponse { Id, DisplayName, Email, RecruitedAt }`
  - `Ruptura.Shared.Campaigns.AssignMemberRequest { PlayerId }`
  - `Ruptura.Shared.Campaigns.CampaignMemberResponse { PlayerId, DisplayName, Email, AssignedAt }`
  - `ErrorCodes.Campaign.{NotFound, PlayerNotInRoster, AlreadyMember}`

- [ ] **Step 1: Create the DTOs**

```csharp
// src/Ruptura.Shared/Campaigns/CreateCampaignRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Campaigns;

public class CreateCampaignRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
```

```csharp
// src/Ruptura.Shared/Campaigns/CampaignResponse.cs
namespace Ruptura.Shared.Campaigns;

public class CampaignResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

```csharp
// src/Ruptura.Shared/Campaigns/PlayerRosterResponse.cs
namespace Ruptura.Shared.Campaigns;

public class PlayerRosterResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RecruitedAt { get; set; }
}
```

```csharp
// src/Ruptura.Shared/Campaigns/AssignMemberRequest.cs
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Campaigns;

public class AssignMemberRequest
{
    [Required]
    public Guid PlayerId { get; set; }
}
```

```csharp
// src/Ruptura.Shared/Campaigns/CampaignMemberResponse.cs
namespace Ruptura.Shared.Campaigns;

public class CampaignMemberResponse
{
    public Guid PlayerId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
```

- [ ] **Step 2: Add `ErrorCodes.Campaign`**

Edit `src/Ruptura.Application/Common/ErrorCodes.cs` — add a new nested class after `Invite`:

```csharp
namespace Ruptura.Application.Common;

public static class ErrorCodes
{
    public static class Auth
    {
        public const string InvalidCredentials = "Auth.InvalidCredentials";
        public const string EmailAlreadyExists = "Auth.EmailAlreadyExists";
        public const string InvalidInviteCode = "Auth.InvalidInviteCode";
        public const string InvalidRefreshToken = "Auth.InvalidRefreshToken";
        public const string UserNotFound = "Auth.UserNotFound";
    }

    public static class Invite
    {
        public const string NotFound = "Invite.NotFound";
        public const string AlreadyUsed = "Invite.AlreadyUsed";
        public const string Expired = "Invite.Expired";
        public const string Forbidden = "Invite.Forbidden";
    }

    public static class Campaign
    {
        public const string NotFound = "Campaign.NotFound";
        public const string PlayerNotInRoster = "Campaign.PlayerNotInRoster";
        public const string AlreadyMember = "Campaign.AlreadyMember";
    }
}
```

- [ ] **Step 3: Add FluentValidation validators**

```csharp
// src/Ruptura.Application/Validators/Campaigns/CreateCampaignRequestValidator.cs
using FluentValidation;
using Ruptura.Shared.Campaigns;

namespace Ruptura.Application.Validators.Campaigns;

public class CreateCampaignRequestValidator : AbstractValidator<CreateCampaignRequest>
{
    public CreateCampaignRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
    }
}
```

```csharp
// src/Ruptura.Application/Validators/Campaigns/AssignMemberRequestValidator.cs
using FluentValidation;
using Ruptura.Shared.Campaigns;

namespace Ruptura.Application.Validators.Campaigns;

public class AssignMemberRequestValidator : AbstractValidator<AssignMemberRequest>
{
    public AssignMemberRequestValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
    }
}
```

- [ ] **Step 4: Register the validators in DI**

Edit `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` — add under `// Validators` (and add the two `using` statements at the top: `using Ruptura.Application.Validators.Campaigns;` and `using Ruptura.Shared.Campaigns;`):

```csharp
        // Validators
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<RegisterPlayerRequest>, RegisterPlayerRequestValidator>();
        services.AddScoped<IValidator<CreateCampaignRequest>, CreateCampaignRequestValidator>();
        services.AddScoped<IValidator<AssignMemberRequest>, AssignMemberRequestValidator>();
```

- [ ] **Step 5: Build to verify it compiles**

Run: `dotnet build`
Expected: `Build succeeded.`

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Shared/Campaigns/ \
        src/Ruptura.Application/Common/ErrorCodes.cs \
        src/Ruptura.Application/Validators/Campaigns/ \
        src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs
git commit -m "feat: add Campaign DTOs, error codes, and validators"
```

---

### Task 5: `CampaignService` (Application layer) with unit tests

**Files:**
- Create: `src/Ruptura.Application/Interfaces/ICampaignService.cs`
- Create: `src/Ruptura.Infrastructure/Services/CampaignService.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`
- Create: `tests/Ruptura.UnitTests/Application/CampaignServiceTests.cs`

**Interfaces:**
- Consumes: `ICampaignRepository`, `ICampaignMembershipRepository` (Task 3); DTOs + `ErrorCodes.Campaign` (Task 4); `UserManager<ApplicationUser>`, `ApplicationUser.RecruitedByGameMasterId` (Task 2)
- Produces:
  ```csharp
  public interface ICampaignService
  {
      Task<Result<CampaignResponse>> CreateAsync(Guid gameMasterId, CreateCampaignRequest request, CancellationToken ct = default);
      Task<Result<IEnumerable<CampaignResponse>>> GetByGameMasterAsync(Guid gameMasterId, CancellationToken ct = default);
      Task<Result<IEnumerable<PlayerRosterResponse>>> GetRosterAsync(Guid gameMasterId, CancellationToken ct = default);
      Task<Result<CampaignMemberResponse>> AssignMemberAsync(Guid gameMasterId, Guid campaignId, AssignMemberRequest request, CancellationToken ct = default);
      Task<Result<IEnumerable<CampaignMemberResponse>>> GetMembersAsync(Guid gameMasterId, Guid campaignId, CancellationToken ct = default);
  }
  ```

- [ ] **Step 1: Define `ICampaignService`**

```csharp
// src/Ruptura.Application/Interfaces/ICampaignService.cs
using Ruptura.Application.Common;
using Ruptura.Shared.Campaigns;

namespace Ruptura.Application.Interfaces;

public interface ICampaignService
{
    Task<Result<CampaignResponse>> CreateAsync(
        Guid gameMasterId, CreateCampaignRequest request, CancellationToken ct = default);

    Task<Result<IEnumerable<CampaignResponse>>> GetByGameMasterAsync(
        Guid gameMasterId, CancellationToken ct = default);

    Task<Result<IEnumerable<PlayerRosterResponse>>> GetRosterAsync(
        Guid gameMasterId, CancellationToken ct = default);

    Task<Result<CampaignMemberResponse>> AssignMemberAsync(
        Guid gameMasterId, Guid campaignId, AssignMemberRequest request, CancellationToken ct = default);

    Task<Result<IEnumerable<CampaignMemberResponse>>> GetMembersAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);
}
```

- [ ] **Step 2: Write the failing unit tests**

```csharp
// tests/Ruptura.UnitTests/Application/CampaignServiceTests.cs
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Identity;
using Ruptura.Infrastructure.Services;
using Ruptura.Shared.Campaigns;

namespace Ruptura.UnitTests.Application;

public class CampaignServiceTests
{
    private readonly Mock<ICampaignRepository> _campaignRepoMock = new();
    private readonly Mock<ICampaignMembershipRepository> _membershipRepoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly CampaignService _sut;

    private static readonly Faker Faker = new();

    public CampaignServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new CampaignService(
            _campaignRepoMock.Object,
            _membershipRepoMock.Object,
            _userManagerMock.Object);
    }

    // ── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_PersistsCampaignOwnedByGameMaster()
    {
        _campaignRepoMock.Setup(r => r.AddAsync(It.IsAny<Campaign>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _campaignRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var gmId = Guid.NewGuid();
        var result = await _sut.CreateAsync(gmId, new CreateCampaignRequest { Name = "The Sunken Gate" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("The Sunken Gate");

        _campaignRepoMock.Verify(r => r.AddAsync(
            It.Is<Campaign>(c => c.GameMasterId == gmId && c.Name == "The Sunken Gate"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetByGameMasterAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetByGameMasterAsync_ReturnsOnlyThatGameMastersCampaigns()
    {
        var gmId = Guid.NewGuid();
        var campaigns = new List<Campaign>
        {
            new() { Id = Guid.NewGuid(), Name = "Arc One", GameMasterId = gmId },
            new() { Id = Guid.NewGuid(), Name = "Arc Two", GameMasterId = gmId }
        };
        _campaignRepoMock.Setup(r => r.GetByGameMasterAsync(gmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaigns);

        var result = await _sut.GetByGameMasterAsync(gmId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
    }

    // ── GetRosterAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetRosterAsync_ReturnsOnlyPlayersRecruitedByThatGameMaster()
    {
        var gmId = Guid.NewGuid();
        var mine = BuildPlayer(gmId);
        var someoneElses = BuildPlayer(Guid.NewGuid());
        _userManagerMock.Setup(m => m.Users).Returns(new[] { mine, someoneElses }.AsQueryable());

        var result = await _sut.GetRosterAsync(gmId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(p => p.Id == mine.Id);
    }

    // ── AssignMemberAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task AssignMemberAsync_WhenCampaignNotOwnedByCaller_ReturnsNotFound()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var result = await _sut.AssignMemberAsync(
            gmId, campaign.Id, new AssignMemberRequest { PlayerId = Guid.NewGuid() });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Campaign.NotFound);
    }

    [Fact]
    public async Task AssignMemberAsync_WhenPlayerNotInRoster_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var notMyPlayer = BuildPlayer(Guid.NewGuid());
        _userManagerMock.Setup(m => m.FindByIdAsync(notMyPlayer.Id.ToString()))
            .ReturnsAsync(notMyPlayer);

        var result = await _sut.AssignMemberAsync(
            gmId, campaign.Id, new AssignMemberRequest { PlayerId = notMyPlayer.Id });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Campaign.PlayerNotInRoster);
    }

    [Fact]
    public async Task AssignMemberAsync_WhenAlreadyMember_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var player = BuildPlayer(gmId);
        _userManagerMock.Setup(m => m.FindByIdAsync(player.Id.ToString())).ReturnsAsync(player);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaign.Id, player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.AssignMemberAsync(
            gmId, campaign.Id, new AssignMemberRequest { PlayerId = player.Id });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Campaign.AlreadyMember);
    }

    [Fact]
    public async Task AssignMemberAsync_WithValidData_CreatesMembership()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var player = BuildPlayer(gmId);
        _userManagerMock.Setup(m => m.FindByIdAsync(player.Id.ToString())).ReturnsAsync(player);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaign.Id, player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _membershipRepoMock.Setup(r => r.AddAsync(It.IsAny<CampaignMembership>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _membershipRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.AssignMemberAsync(
            gmId, campaign.Id, new AssignMemberRequest { PlayerId = player.Id });

        result.IsSuccess.Should().BeTrue();
        result.Value!.PlayerId.Should().Be(player.Id);
        _membershipRepoMock.Verify(r => r.AddAsync(
            It.Is<CampaignMembership>(m => m.CampaignId == campaign.Id && m.PlayerId == player.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetMembersAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetMembersAsync_ReturnsMembersWithDisplayInfo()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var player = BuildPlayer(gmId);
        var membership = new CampaignMembership
        {
            Id = Guid.NewGuid(), CampaignId = campaign.Id, PlayerId = player.Id
        };
        _membershipRepoMock.Setup(r => r.GetByCampaignAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { membership });
        _userManagerMock.Setup(m => m.FindByIdAsync(player.Id.ToString())).ReturnsAsync(player);

        var result = await _sut.GetMembersAsync(gmId, campaign.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(m => m.PlayerId == player.Id && m.DisplayName == player.DisplayName);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static ApplicationUser BuildPlayer(Guid recruitedBy) => new()
    {
        Id = Guid.NewGuid(),
        Email = Faker.Internet.Email(),
        UserName = Faker.Internet.UserName(),
        DisplayName = Faker.Name.FullName(),
        Role = UserRole.Player,
        RecruitedByGameMasterId = recruitedBy,
        CreatedAt = DateTime.UtcNow
    };
}
```

- [ ] **Step 3: Run tests to verify they fail (service doesn't exist yet)**

Run: `dotnet test tests/Ruptura.UnitTests --filter CampaignServiceTests`
Expected: build error — `CampaignService` does not exist.

- [ ] **Step 4: Implement `CampaignService`**

```csharp
// src/Ruptura.Infrastructure/Services/CampaignService.cs
using Microsoft.AspNetCore.Identity;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Identity;
using Ruptura.Shared.Campaigns;

namespace Ruptura.Infrastructure.Services;

public class CampaignService(
    ICampaignRepository campaignRepo,
    ICampaignMembershipRepository membershipRepo,
    UserManager<ApplicationUser> userManager) : ICampaignService
{
    public async Task<Result<CampaignResponse>> CreateAsync(
        Guid gameMasterId,
        CreateCampaignRequest request,
        CancellationToken ct = default)
    {
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            GameMasterId = gameMasterId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await campaignRepo.AddAsync(campaign, ct);
        await campaignRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(campaign));
    }

    public async Task<Result<IEnumerable<CampaignResponse>>> GetByGameMasterAsync(
        Guid gameMasterId,
        CancellationToken ct = default)
    {
        var campaigns = await campaignRepo.GetByGameMasterAsync(gameMasterId, ct);
        return Result.Success(campaigns.Select(MapToResponse));
    }

    public Task<Result<IEnumerable<PlayerRosterResponse>>> GetRosterAsync(
        Guid gameMasterId,
        CancellationToken ct = default)
    {
        // NOTE: userManager.Users is IQueryable; only synchronous LINQ is used here
        // (not ToListAsync) so this stays testable against a plain in-memory queryable
        // in unit tests, matching the existing convention in AuthService.RefreshTokenAsync.
        var players = userManager.Users
            .Where(u => u.RecruitedByGameMasterId == gameMasterId)
            .ToList();

        var response = players.Select(p => new PlayerRosterResponse
        {
            Id = p.Id,
            DisplayName = p.DisplayName,
            Email = p.Email!,
            RecruitedAt = p.CreatedAt
        });

        return Task.FromResult(Result.Success(response));
    }

    public async Task<Result<CampaignMemberResponse>> AssignMemberAsync(
        Guid gameMasterId,
        Guid campaignId,
        AssignMemberRequest request,
        CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CampaignMemberResponse>(ErrorCodes.Campaign.NotFound);

        var player = await userManager.FindByIdAsync(request.PlayerId.ToString());
        if (player is null || player.RecruitedByGameMasterId != gameMasterId)
            return Result.Failure<CampaignMemberResponse>(ErrorCodes.Campaign.PlayerNotInRoster);

        if (await membershipRepo.ExistsAsync(campaignId, request.PlayerId, ct))
            return Result.Failure<CampaignMemberResponse>(ErrorCodes.Campaign.AlreadyMember);

        var membership = new CampaignMembership
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            PlayerId = request.PlayerId,
            AssignedAt = DateTime.UtcNow
        };

        await membershipRepo.AddAsync(membership, ct);
        await membershipRepo.SaveChangesAsync(ct);

        return Result.Success(new CampaignMemberResponse
        {
            PlayerId = player.Id,
            DisplayName = player.DisplayName,
            Email = player.Email!,
            AssignedAt = membership.AssignedAt
        });
    }

    public async Task<Result<IEnumerable<CampaignMemberResponse>>> GetMembersAsync(
        Guid gameMasterId,
        Guid campaignId,
        CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<IEnumerable<CampaignMemberResponse>>(ErrorCodes.Campaign.NotFound);

        var memberships = await membershipRepo.GetByCampaignAsync(campaignId, ct);

        var responses = new List<CampaignMemberResponse>();
        foreach (var membership in memberships)
        {
            var player = await userManager.FindByIdAsync(membership.PlayerId.ToString());
            if (player is null) continue;

            responses.Add(new CampaignMemberResponse
            {
                PlayerId = player.Id,
                DisplayName = player.DisplayName,
                Email = player.Email!,
                AssignedAt = membership.AssignedAt
            });
        }

        return Result.Success(responses.AsEnumerable());
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static CampaignResponse MapToResponse(Campaign c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        CreatedAt = c.CreatedAt
    };
}
```

- [ ] **Step 5: Register `CampaignService` in DI**

Edit `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` — add under `// Application services`:

```csharp
        // Application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInviteCodeService, InviteCodeService>();
        services.AddScoped<ICampaignService, CampaignService>();
```

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter CampaignServiceTests`
Expected: all 7 tests `Passed`.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Application/Interfaces/ICampaignService.cs \
        src/Ruptura.Infrastructure/Services/CampaignService.cs \
        src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs \
        tests/Ruptura.UnitTests/Application/CampaignServiceTests.cs
git commit -m "feat: add CampaignService with roster and membership logic"
```

---

### Task 6: Wire player registration to the recruiting GM

**Files:**
- Modify: `src/Ruptura.Infrastructure/Services/AuthService.cs`
- Modify: `tests/Ruptura.UnitTests/Application/AuthServiceTests.cs`

**Interfaces:**
- Consumes: `ApplicationUser.RecruitedByGameMasterId` (Task 2)
- Produces: no new public signature — `RegisterPlayerAsync` now also sets `RecruitedByGameMasterId` on the created user

- [ ] **Step 1: Write the failing test**

Add to `tests/Ruptura.UnitTests/Application/AuthServiceTests.cs`, right after `RegisterPlayerAsync_WithValidInviteCode_CreatesPlayerUser`:

```csharp
    [Fact]
    public async Task RegisterPlayerAsync_WithValidInviteCode_SetsRecruitingGameMaster()
    {
        var gmId = Guid.NewGuid();
        var invite = new InviteCode
        {
            Code = "VALID123",
            CreatedByGameMasterId = gmId,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _inviteRepoMock.Setup(r => r.GetByCodeAsync("VALID123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        ApplicationUser? createdUser = null;
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((u, _) => createdUser = u)
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _inviteRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _sut.RegisterPlayerAsync(new RegisterPlayerRequest
        {
            DisplayName = "Brave Hero",
            Email = "hero2@example.com",
            Password = "ValidPass1",
            ConfirmPassword = "ValidPass1",
            InviteCode = "VALID123"
        });

        result.IsSuccess.Should().BeTrue();
        createdUser.Should().NotBeNull();
        createdUser!.RecruitedByGameMasterId.Should().Be(gmId);
    }
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests/Ruptura.UnitTests --filter RegisterPlayerAsync_WithValidInviteCode_SetsRecruitingGameMaster`
Expected: FAIL — `createdUser.RecruitedByGameMasterId` is `null`, expected the GM's id.

- [ ] **Step 3: Set `RecruitedByGameMasterId` in `RegisterPlayerAsync`**

Edit `src/Ruptura.Infrastructure/Services/AuthService.cs` — in `RegisterPlayerAsync`, add one line to the `ApplicationUser` initializer:

```csharp
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            Role = UserRole.Player,
            RecruitedByGameMasterId = invite.CreatedByGameMasterId
        };
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter AuthServiceTests`
Expected: all tests `Passed`.

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Infrastructure/Services/AuthService.cs \
        tests/Ruptura.UnitTests/Application/AuthServiceTests.cs
git commit -m "feat: record recruiting game master on player registration"
```

---

### Task 7: API controllers, localization, integration tests

**Files:**
- Create: `src/Ruptura.API/Controllers/CampaignController.cs`
- Create: `src/Ruptura.API/Controllers/GameMasterController.cs`
- Modify: `src/Ruptura.API/Resources/SharedResources.resx`
- Modify: `src/Ruptura.API/Resources/SharedResources.pt-BR.resx`
- Modify: `tests/Ruptura.IntegrationTests/Helpers/AuthHelper.cs`
- Create: `tests/Ruptura.IntegrationTests/Controllers/CampaignControllerTests.cs`

**Interfaces:**
- Consumes: `ICampaignService` (Task 5); DTOs (Task 4)
- Produces (HTTP):
  ```
  GET    /api/gamemaster/players
  POST   /api/campaigns
  GET    /api/campaigns
  GET    /api/campaigns/{campaignId:guid}/members
  POST   /api/campaigns/{campaignId:guid}/members
  ```
  `AuthHelper.RegisterPlayerAsync(HttpClient, string inviteCode, string email, ...)` for use by later integration test files.

- [ ] **Step 1: Add localized error/success messages (English)**

Edit `src/Ruptura.API/Resources/SharedResources.resx` — add before `<!-- Generic -->`:

```xml
  <!-- Campaign -->
  <data name="Campaign.NotFound"><value>Campaign not found.</value></data>
  <data name="Campaign.PlayerNotInRoster"><value>This player is not in your roster.</value></data>
  <data name="Campaign.AlreadyMember"><value>This player is already a member of this campaign.</value></data>
  <data name="Campaign.Created"><value>Campaign created successfully.</value></data>
  <data name="Campaign.MemberAssigned"><value>Player assigned to campaign successfully.</value></data>
```

- [ ] **Step 2: Add localized messages (Portuguese)**

Edit `src/Ruptura.API/Resources/SharedResources.pt-BR.resx` — add before `<!-- Generic -->`:

```xml
  <!-- Campaign -->
  <data name="Campaign.NotFound"><value>Campanha não encontrada.</value></data>
  <data name="Campaign.PlayerNotInRoster"><value>Este jogador não está no seu roster.</value></data>
  <data name="Campaign.AlreadyMember"><value>Este jogador já é membro desta campanha.</value></data>
  <data name="Campaign.Created"><value>Campanha criada com sucesso.</value></data>
  <data name="Campaign.MemberAssigned"><value>Jogador atribuído à campanha com sucesso.</value></data>
```

- [ ] **Step 3: Create `CampaignController`**

```csharp
// src/Ruptura.API/Controllers/CampaignController.cs
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/campaigns")]
[Authorize(Roles = "GameMaster")]
public class CampaignController(
    ICampaignService campaignService,
    IStringLocalizer<SharedResources> localizer,
    IValidator<CreateCampaignRequest> createValidator,
    IValidator<AssignMemberRequest> assignValidator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CampaignResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.CreateAsync(gameMasterId, request, ct);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CampaignResponse>.Ok(result.Value!, localizer["Campaign.Created"]));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CampaignResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.GetByGameMasterAsync(gameMasterId, ct);

        return Ok(ApiResponse<IEnumerable<CampaignResponse>>.Ok(result.Value!));
    }

    [HttpGet("{campaignId:guid}/members")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CampaignMemberResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Members(Guid campaignId, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.GetMembersAsync(gameMasterId, campaignId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<IEnumerable<CampaignMemberResponse>>.Ok(result.Value!));
    }

    [HttpPost("{campaignId:guid}/members")]
    [ProducesResponseType(typeof(ApiResponse<CampaignMemberResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignMember(
        Guid campaignId, [FromBody] AssignMemberRequest request, CancellationToken ct)
    {
        var validation = await assignValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.AssignMemberAsync(gameMasterId, campaignId, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Campaign.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CampaignMemberResponse>.Ok(result.Value!, localizer["Campaign.MemberAssigned"]));
    }
}
```

- [ ] **Step 4: Create `GameMasterController`**

```csharp
// src/Ruptura.API/Controllers/GameMasterController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/gamemaster")]
[Authorize(Roles = "GameMaster")]
public class GameMasterController(ICampaignService campaignService) : ControllerBase
{
    [HttpGet("players")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PlayerRosterResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Players(CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.GetRosterAsync(gameMasterId, ct);

        return Ok(ApiResponse<IEnumerable<PlayerRosterResponse>>.Ok(result.Value!));
    }
}
```

- [ ] **Step 5: Extend `AuthHelper` with a player-registration helper**

Edit `tests/Ruptura.IntegrationTests/Helpers/AuthHelper.cs`, add after `RegisterGameMasterAsync`:

```csharp
    public static async Task<AuthResponse> RegisterPlayerAsync(
        HttpClient client,
        string inviteCode,
        string email,
        string password = "TestPass1",
        string displayName = "Test Player")
    {
        var response = await client.PostAsJsonAsync("api/auth/register/player", new RegisterPlayerRequest
        {
            DisplayName = displayName,
            Email = email,
            Password = password,
            ConfirmPassword = password,
            InviteCode = inviteCode
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return result!.Data!;
    }
```

- [ ] **Step 6: Write the integration tests**

```csharp
// tests/Ruptura.IntegrationTests/Controllers/CampaignControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class CampaignControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, string GmToken, string InviteCode)> SetupGameMasterWithInviteAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var inviteResponse = await client.PostAsync("api/invites", null);
        var invite = (await inviteResponse.Content
            .ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!;

        return (client, gm.AccessToken, invite.Code);
    }

    [Fact]
    public async Task Players_ReturnsOnlyRosterOfCallingGameMaster()
    {
        var (client, gmToken, inviteCode) = await SetupGameMasterWithInviteAsync();
        var playerEmail = Faker.Internet.Email();
        await AuthHelper.RegisterPlayerAsync(client, inviteCode, playerEmail);

        AuthHelper.SetBearerToken(client, gmToken);
        var response = await client.GetAsync("api/gamemaster/players");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>();
        body!.Data.Should().ContainSingle(p => p.Email == playerEmail);
    }

    [Fact]
    public async Task Players_DoesNotIncludePlayersRecruitedByAnotherGameMaster()
    {
        var (client1, gm1Token, invite1) = await SetupGameMasterWithInviteAsync();
        await AuthHelper.RegisterPlayerAsync(client1, invite1, Faker.Internet.Email());

        var (client2, gm2Token, _) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client2, gm2Token);

        var response = await client2.GetAsync("api/gamemaster/players");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>();
        body!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCampaign_ReturnsCampaignOwnedByCaller()
    {
        var (client, gmToken, _) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var response = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "The Sunken Gate"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>();
        body!.Data!.Name.Should().Be("The Sunken Gate");
    }

    [Fact]
    public async Task AssignMember_WithPlayerInRoster_Returns201AndListsInMembers()
    {
        var (client, gmToken, inviteCode) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        var playerEmail = Faker.Internet.Email();
        await AuthHelper.RegisterPlayerAsync(client, inviteCode, playerEmail);

        var playersResponse = await client.GetAsync("api/gamemaster/players");
        var players = (await playersResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>())!.Data!.ToList();
        var playerId = players.Single(p => p.Email == playerEmail).Id;

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "The Sunken Gate"
        });
        var campaignId = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!.Id;

        var assignResponse = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/members", new AssignMemberRequest { PlayerId = playerId });
        assignResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var membersResponse = await client.GetAsync($"api/campaigns/{campaignId}/members");
        var members = (await membersResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignMemberResponse>>>())!.Data!;
        members.Should().ContainSingle(m => m.PlayerId == playerId);
    }

    [Fact]
    public async Task AssignMember_WithPlayerNotInRoster_Returns400()
    {
        var (client, gmToken, _) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "The Sunken Gate"
        });
        var campaignId = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!.Id;

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/members", new AssignMemberRequest { PlayerId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignMember_ToCampaignNotOwnedByCaller_Returns404()
    {
        var (client1, gm1Token, invite1) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client1, gm1Token);
        var campaignResponse = await client1.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "GM1's Campaign"
        });
        var campaignId = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!.Id;

        var (client2, gm2Token, invite2) = await SetupGameMasterWithInviteAsync();
        var playerEmail = Faker.Internet.Email();
        await AuthHelper.RegisterPlayerAsync(client2, invite2, playerEmail);
        AuthHelper.SetBearerToken(client2, gm2Token);
        var players = (await (await client2.GetAsync("api/gamemaster/players")).Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>())!.Data!.ToList();

        var response = await client2.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/members",
            new AssignMemberRequest { PlayerId = players.Single(p => p.Email == playerEmail).Id });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CampaignEndpoints_WithoutGameMasterRole_Return403()
    {
        var (client, _, inviteCode) = await SetupGameMasterWithInviteAsync();
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        var playerClient = factory.CreateClient();
        AuthHelper.SetBearerToken(playerClient, player.AccessToken);

        var response = await playerClient.GetAsync("api/campaigns");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
```

- [ ] **Step 7: Run the integration tests**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter CampaignControllerTests`
Expected: all tests `Passed`. (Requires Docker available for Testcontainers, per `CLAUDE.md`.)

- [ ] **Step 8: Commit**

```bash
git add src/Ruptura.API/Controllers/CampaignController.cs \
        src/Ruptura.API/Controllers/GameMasterController.cs \
        src/Ruptura.API/Resources/SharedResources.resx \
        src/Ruptura.API/Resources/SharedResources.pt-BR.resx \
        tests/Ruptura.IntegrationTests/Helpers/AuthHelper.cs \
        tests/Ruptura.IntegrationTests/Controllers/CampaignControllerTests.cs
git commit -m "feat: add Campaign and GameMaster roster API endpoints"
```

---

### Task 8: Blazor client service

**Files:**
- Create: `src/Ruptura.Web/Services/ICampaignClientService.cs`
- Create: `src/Ruptura.Web/Services/CampaignClientService.cs`
- Modify: `src/Ruptura.Web/Program.cs`

**Interfaces:**
- Consumes: DTOs from `Ruptura.Shared.Campaigns` (Task 4); HTTP routes from Task 7
- Produces:
  ```csharp
  public interface ICampaignClientService
  {
      Task<ApiResponse<IEnumerable<PlayerRosterResponse>>?> GetRosterAsync();
      Task<ApiResponse<CampaignResponse>?> CreateAsync(CreateCampaignRequest request);
      Task<ApiResponse<IEnumerable<CampaignResponse>>?> GetAllAsync();
      Task<ApiResponse<IEnumerable<CampaignMemberResponse>>?> GetMembersAsync(Guid campaignId);
      Task<ApiResponse<CampaignMemberResponse>?> AssignMemberAsync(Guid campaignId, AssignMemberRequest request);
  }
  ```

- [ ] **Step 1: Define `ICampaignClientService`**

```csharp
// src/Ruptura.Web/Services/ICampaignClientService.cs
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public interface ICampaignClientService
{
    Task<ApiResponse<IEnumerable<PlayerRosterResponse>>?> GetRosterAsync();
    Task<ApiResponse<CampaignResponse>?> CreateAsync(CreateCampaignRequest request);
    Task<ApiResponse<IEnumerable<CampaignResponse>>?> GetAllAsync();
    Task<ApiResponse<IEnumerable<CampaignMemberResponse>>?> GetMembersAsync(Guid campaignId);
    Task<ApiResponse<CampaignMemberResponse>?> AssignMemberAsync(Guid campaignId, AssignMemberRequest request);
}
```

- [ ] **Step 2: Implement `CampaignClientService`**

```csharp
// src/Ruptura.Web/Services/CampaignClientService.cs
using System.Net.Http.Json;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public class CampaignClientService(IHttpClientFactory factory) : ICampaignClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<IEnumerable<PlayerRosterResponse>>?> GetRosterAsync()
    {
        var response = await Http.GetAsync("api/gamemaster/players");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>();
    }

    public async Task<ApiResponse<CampaignResponse>?> CreateAsync(CreateCampaignRequest request)
    {
        var response = await Http.PostAsJsonAsync("api/campaigns", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>();
    }

    public async Task<ApiResponse<IEnumerable<CampaignResponse>>?> GetAllAsync()
    {
        var response = await Http.GetAsync("api/campaigns");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignResponse>>>();
    }

    public async Task<ApiResponse<IEnumerable<CampaignMemberResponse>>?> GetMembersAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/members");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignMemberResponse>>>();
    }

    public async Task<ApiResponse<CampaignMemberResponse>?> AssignMemberAsync(
        Guid campaignId, AssignMemberRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/members", request);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<CampaignMemberResponse>>();
    }
}
```

- [ ] **Step 3: Register the service in `Program.cs`**

Edit `src/Ruptura.Web/Program.cs` — add one line right after the existing `builder.Services.AddScoped<IInviteClientService, InviteClientService>();`:

```csharp
builder.Services.AddScoped<ICampaignClientService, CampaignClientService>();
```

- [ ] **Step 4: Build to verify it compiles**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/Services/ICampaignClientService.cs \
        src/Ruptura.Web/Services/CampaignClientService.cs \
        src/Ruptura.Web/Program.cs
git commit -m "feat: add Blazor client service for Campaign API"
```

---

### Task 9: GM Players page (`/gm/players`)

**Files:**
- Create: `src/Ruptura.Web/Pages/GmPlayers.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ICampaignClientService.GetRosterAsync()` (Task 8)

- [ ] **Step 1: Add localized strings (English)**

Edit `src/Ruptura.Web/Resources/AppStrings.resx` — add near the existing `Nav.Invites` entry:

```xml
  <data name="Nav.Players"><value>Players</value></data>
  <data name="Gm.Players.Title"><value>Player Roster</value></data>
  <data name="Gm.Players.Empty"><value>No players recruited yet. Generate an invite code to get started.</value></data>
  <data name="Gm.Players.Col.Name"><value>Name</value></data>
  <data name="Gm.Players.Col.Email"><value>Email</value></data>
  <data name="Gm.Players.Col.RecruitedAt"><value>Joined</value></data>
```

- [ ] **Step 2: Add localized strings (Portuguese)**

Edit `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx` — add the matching keys:

```xml
  <data name="Nav.Players"><value>Jogadores</value></data>
  <data name="Gm.Players.Title"><value>Roster de Jogadores</value></data>
  <data name="Gm.Players.Empty"><value>Nenhum jogador recrutado ainda. Gere um código de convite para começar.</value></data>
  <data name="Gm.Players.Col.Name"><value>Nome</value></data>
  <data name="Gm.Players.Col.Email"><value>E-mail</value></data>
  <data name="Gm.Players.Col.RecruitedAt"><value>Ingressou</value></data>
```

- [ ] **Step 3: Create the page**

```razor
@page "/gm/players"
@attribute [Authorize(Roles = "GameMaster")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Campaigns
@inject IStringLocalizer<AppStrings> L
@inject ICampaignClientService CampaignService

<PageTitle>@L["Gm.Players.Title"] — RUPTURA</PageTitle>

<div class="page-content">
    <div class="page-heading">
        <h1>@L["Gm.Players.Title"]</h1>
    </div>

    @if (_loading)
    {
        <div class="ledger-empty">
            <span class="spinner-border spinner-border-sm me-2"></span>@L["Common.Loading"]
        </div>
    }
    else if (_players.Count == 0)
    {
        <div class="ledger-empty">
            <p>@L["Gm.Players.Empty"]</p>
        </div>
    }
    else
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table">
                <thead>
                    <tr>
                        <th>@L["Gm.Players.Col.Name"]</th>
                        <th>@L["Gm.Players.Col.Email"]</th>
                        <th>@L["Gm.Players.Col.RecruitedAt"]</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var player in _players)
                    {
                        <tr>
                            <td>@player.DisplayName</td>
                            <td>@player.Email</td>
                            <td style="color:var(--text-muted);font-size:.78rem;white-space:nowrap">
                                @player.RecruitedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
</div>

@code {
    private List<PlayerRosterResponse> _players = [];
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        var result = await CampaignService.GetRosterAsync();
        _players = result?.Data?.ToList() ?? [];
        _loading = false;
    }
}
```

- [ ] **Step 4: Build to verify it compiles**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/Pages/GmPlayers.razor \
        src/Ruptura.Web/Resources/AppStrings.resx \
        src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add GM player roster page"
```

---

### Task 10: GM Campaigns list + create page (`/gm/campaigns`)

**Files:**
- Create: `src/Ruptura.Web/Pages/GmCampaigns.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ICampaignClientService.GetAllAsync()`, `.CreateAsync(CreateCampaignRequest)` (Task 8)

- [ ] **Step 1: Add localized strings (English)**

Edit `src/Ruptura.Web/Resources/AppStrings.resx`:

```xml
  <data name="Nav.Campaigns"><value>Campaigns</value></data>
  <data name="Gm.Campaigns.Title"><value>Campaigns</value></data>
  <data name="Gm.Campaigns.NamePlaceholder"><value>Campaign name</value></data>
  <data name="Gm.Campaigns.Create"><value>Create Campaign</value></data>
  <data name="Gm.Campaigns.Empty"><value>No campaigns yet.</value></data>
  <data name="Gm.Campaigns.Col.Name"><value>Name</value></data>
  <data name="Gm.Campaigns.Col.CreatedAt"><value>Created</value></data>
  <data name="Gm.Campaigns.View"><value>View</value></data>
```

- [ ] **Step 2: Add localized strings (Portuguese)**

Edit `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`:

```xml
  <data name="Nav.Campaigns"><value>Campanhas</value></data>
  <data name="Gm.Campaigns.Title"><value>Campanhas</value></data>
  <data name="Gm.Campaigns.NamePlaceholder"><value>Nome da campanha</value></data>
  <data name="Gm.Campaigns.Create"><value>Criar Campanha</value></data>
  <data name="Gm.Campaigns.Empty"><value>Nenhuma campanha ainda.</value></data>
  <data name="Gm.Campaigns.Col.Name"><value>Nome</value></data>
  <data name="Gm.Campaigns.Col.CreatedAt"><value>Criada em</value></data>
  <data name="Gm.Campaigns.View"><value>Ver</value></data>
```

- [ ] **Step 3: Create the page**

```razor
@page "/gm/campaigns"
@attribute [Authorize(Roles = "GameMaster")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Campaigns
@inject IStringLocalizer<AppStrings> L
@inject ICampaignClientService CampaignService
@inject NavigationManager Nav

<PageTitle>@L["Gm.Campaigns.Title"] — RUPTURA</PageTitle>

<div class="page-content">
    <div class="page-heading">
        <h1>@L["Gm.Campaigns.Title"]</h1>
    </div>

    <div class="section-header">
        <span class="section-title">@L["Gm.Campaigns.Title"]</span>
        <div style="display:flex;gap:.5rem">
            <input class="form-control" style="width:220px" placeholder="@L["Gm.Campaigns.NamePlaceholder"]"
                   @bind="_newName" @bind:event="oninput" />
            <button class="btn btn-primary btn-sm" @onclick="CreateAsync" disabled="@(_creating || string.IsNullOrWhiteSpace(_newName))">
                @if (_creating) { <span class="spinner-border spinner-border-sm me-1"></span> }
                @L["Gm.Campaigns.Create"]
            </button>
        </div>
    </div>

    @if (!string.IsNullOrEmpty(_errorMessage))
    {
        <div class="alert-danger mb-4">@_errorMessage</div>
    }

    @if (_loading)
    {
        <div class="ledger-empty">
            <span class="spinner-border spinner-border-sm me-2"></span>@L["Common.Loading"]
        </div>
    }
    else if (_campaigns.Count == 0)
    {
        <div class="ledger-empty">
            <p>@L["Gm.Campaigns.Empty"]</p>
        </div>
    }
    else
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table">
                <thead>
                    <tr>
                        <th>@L["Gm.Campaigns.Col.Name"]</th>
                        <th>@L["Gm.Campaigns.Col.CreatedAt"]</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var campaign in _campaigns)
                    {
                        <tr>
                            <td>@campaign.Name</td>
                            <td style="color:var(--text-muted);font-size:.78rem;white-space:nowrap">
                                @campaign.CreatedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")
                            </td>
                            <td>
                                <button class="btn btn-outline-secondary btn-sm"
                                        @onclick="() => Nav.NavigateTo($"/gm/campaigns/{campaign.Id}")">
                                    @L["Gm.Campaigns.View"]
                                </button>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
</div>

@code {
    private List<CampaignResponse> _campaigns = [];
    private bool _loading = true;
    private bool _creating;
    private string _newName = string.Empty;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        var result = await CampaignService.GetAllAsync();
        _campaigns = result?.Data?.ToList() ?? [];
        _loading = false;
    }

    private async Task CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(_newName)) return;

        _creating = true;
        _errorMessage = null;

        var result = await CampaignService.CreateAsync(new CreateCampaignRequest { Name = _newName });
        if (result?.Data is not null)
        {
            _campaigns.Insert(0, result.Data);
            _newName = string.Empty;
        }
        else
        {
            _errorMessage = L["Common.Error"];
        }

        _creating = false;
    }
}
```

- [ ] **Step 4: Build to verify it compiles**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/Pages/GmCampaigns.razor \
        src/Ruptura.Web/Resources/AppStrings.resx \
        src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add GM campaigns list and create page"
```

---

### Task 11: GM Campaign detail page — members + assign from roster (`/gm/campaigns/{Id}`)

**Files:**
- Create: `src/Ruptura.Web/Pages/GmCampaignDetail.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ICampaignClientService.GetRosterAsync()`, `.GetMembersAsync(Guid)`, `.AssignMemberAsync(Guid, AssignMemberRequest)` (Task 8)

- [ ] **Step 1: Add localized strings (English)**

Edit `src/Ruptura.Web/Resources/AppStrings.resx`:

```xml
  <data name="Gm.CampaignDetail.Members"><value>Members</value></data>
  <data name="Gm.CampaignDetail.Empty"><value>No members yet.</value></data>
  <data name="Gm.CampaignDetail.Col.Name"><value>Name</value></data>
  <data name="Gm.CampaignDetail.Col.Email"><value>Email</value></data>
  <data name="Gm.CampaignDetail.Col.AssignedAt"><value>Assigned</value></data>
  <data name="Gm.CampaignDetail.AssignPlayer"><value>Assign Player</value></data>
  <data name="Gm.CampaignDetail.NoAvailablePlayers"><value>All roster players are already members of this campaign.</value></data>
  <data name="Gm.CampaignDetail.SelectPlayer"><value>Select a player…</value></data>
```

- [ ] **Step 2: Add localized strings (Portuguese)**

Edit `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`:

```xml
  <data name="Gm.CampaignDetail.Members"><value>Membros</value></data>
  <data name="Gm.CampaignDetail.Empty"><value>Nenhum membro ainda.</value></data>
  <data name="Gm.CampaignDetail.Col.Name"><value>Nome</value></data>
  <data name="Gm.CampaignDetail.Col.Email"><value>E-mail</value></data>
  <data name="Gm.CampaignDetail.Col.AssignedAt"><value>Atribuído em</value></data>
  <data name="Gm.CampaignDetail.AssignPlayer"><value>Atribuir Jogador</value></data>
  <data name="Gm.CampaignDetail.NoAvailablePlayers"><value>Todos os jogadores do roster já são membros desta campanha.</value></data>
  <data name="Gm.CampaignDetail.SelectPlayer"><value>Selecione um jogador…</value></data>
```

- [ ] **Step 3: Create the page**

```razor
@page "/gm/campaigns/{Id:guid}"
@attribute [Authorize(Roles = "GameMaster")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Campaigns
@inject IStringLocalizer<AppStrings> L
@inject ICampaignClientService CampaignService

<PageTitle>@L["Gm.CampaignDetail.Members"] — RUPTURA</PageTitle>

<div class="page-content">
    <div class="page-heading">
        <h1>@L["Gm.CampaignDetail.Members"]</h1>
    </div>

    @if (!string.IsNullOrEmpty(_errorMessage))
    {
        <div class="alert-danger mb-4">@_errorMessage</div>
    }

    <div class="section-header">
        <span class="section-title">@L["Gm.CampaignDetail.Members"]</span>
        @if (_availablePlayers.Count > 0)
        {
            <div style="display:flex;gap:.5rem">
                <select class="form-select" style="width:240px" @bind="_selectedPlayerId">
                    <option value="">@L["Gm.CampaignDetail.SelectPlayer"]</option>
                    @foreach (var player in _availablePlayers)
                    {
                        <option value="@player.Id">@player.DisplayName (@player.Email)</option>
                    }
                </select>
                <button class="btn btn-primary btn-sm" @onclick="AssignAsync"
                        disabled="@(_assigning || _selectedPlayerId == Guid.Empty)">
                    @if (_assigning) { <span class="spinner-border spinner-border-sm me-1"></span> }
                    @L["Gm.CampaignDetail.AssignPlayer"]
                </button>
            </div>
        }
    </div>

    @if (_loading)
    {
        <div class="ledger-empty">
            <span class="spinner-border spinner-border-sm me-2"></span>@L["Common.Loading"]
        </div>
    }
    else if (_members.Count == 0)
    {
        <div class="ledger-empty">
            <p>@L["Gm.CampaignDetail.Empty"]</p>
        </div>
    }
    else
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table">
                <thead>
                    <tr>
                        <th>@L["Gm.CampaignDetail.Col.Name"]</th>
                        <th>@L["Gm.CampaignDetail.Col.Email"]</th>
                        <th>@L["Gm.CampaignDetail.Col.AssignedAt"]</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var member in _members)
                    {
                        <tr>
                            <td>@member.DisplayName</td>
                            <td>@member.Email</td>
                            <td style="color:var(--text-muted);font-size:.78rem;white-space:nowrap">
                                @member.AssignedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }

    @if (!_loading && _members.Count > 0 && _availablePlayers.Count == 0)
    {
        <p style="color:var(--text-muted);font-size:.8rem;margin-top:1rem">
            @L["Gm.CampaignDetail.NoAvailablePlayers"]
        </p>
    }
</div>

@code {
    [Parameter] public Guid Id { get; set; }

    private List<CampaignMemberResponse> _members = [];
    private List<PlayerRosterResponse> _availablePlayers = [];
    private bool _loading = true;
    private bool _assigning;
    private Guid _selectedPlayerId;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;

        var membersResult = await CampaignService.GetMembersAsync(Id);
        _members = membersResult?.Data?.ToList() ?? [];

        var rosterResult = await CampaignService.GetRosterAsync();
        var roster = rosterResult?.Data?.ToList() ?? [];
        var memberIds = _members.Select(m => m.PlayerId).ToHashSet();
        _availablePlayers = roster.Where(p => !memberIds.Contains(p.Id)).ToList();

        _loading = false;
    }

    private async Task AssignAsync()
    {
        if (_selectedPlayerId == Guid.Empty) return;

        _assigning = true;
        _errorMessage = null;

        var result = await CampaignService.AssignMemberAsync(
            Id, new AssignMemberRequest { PlayerId = _selectedPlayerId });

        if (result?.Data is not null)
        {
            _selectedPlayerId = Guid.Empty;
            await LoadAsync();
        }
        else
        {
            _errorMessage = L["Common.Error"];
        }

        _assigning = false;
    }
}
```

- [ ] **Step 4: Build to verify it compiles**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/Pages/GmCampaignDetail.razor \
        src/Ruptura.Web/Resources/AppStrings.resx \
        src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add GM campaign detail page with member assignment"
```

---

### Task 12: Navigation links and full end-to-end verification

**Files:**
- Modify: `src/Ruptura.Web/Layout/NavMenu.razor`
- Create: `tests/Ruptura.IntegrationTests/Controllers/CampaignFlowTests.cs`

**Interfaces:**
- Consumes: everything from Tasks 1–11

- [ ] **Step 1: Add nav links for the Mestre**

Edit `src/Ruptura.Web/Layout/NavMenu.razor` — inside the `<AuthorizeView Roles="GameMaster">` block, replace the existing single "Invites" `NavLink` with links for Players, Campaigns, and Invites:

```razor
                <AuthorizeView Roles="GameMaster">
                    <Authorized Context="gmCtx">
                        <span class="nav-section-label" style="margin-top:.75rem">Mestre</span>
                        <NavLink class="nav-link" href="/gm/players">
                            @L["Nav.Players"]
                        </NavLink>
                        <NavLink class="nav-link" href="/gm/campaigns">
                            @L["Nav.Campaigns"]
                        </NavLink>
                        <NavLink class="nav-link" href="/gm/invites">
                            @L["Nav.Invites"]
                        </NavLink>
                    </Authorized>
                </AuthorizeView>
```

- [ ] **Step 2: Write the end-to-end integration test**

```csharp
// tests/Ruptura.IntegrationTests/Controllers/CampaignFlowTests.cs
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class CampaignFlowTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    [Fact]
    public async Task FullFlow_RegisterRecruitCreateCampaignAssign_Succeeds()
    {
        var client = factory.CreateClient();

        // 1. GM registers
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        // 2. GM generates an invite
        var inviteResponse = await client.PostAsync("api/invites", null);
        var invite = (await inviteResponse.Content
            .ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!;

        // 3. Player registers with that invite → appears in GM's roster
        var playerEmail = Faker.Internet.Email();
        await AuthHelper.RegisterPlayerAsync(client, invite.Code, playerEmail);

        var rosterResponse = await client.GetAsync("api/gamemaster/players");
        var roster = (await rosterResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>())!.Data!.ToList();
        roster.Should().ContainSingle(p => p.Email == playerEmail);
        var playerId = roster.Single(p => p.Email == playerEmail).Id;

        // 4. GM creates a Campaign
        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "The Sunken Gate"
        });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        // 5. GM assigns the player to the Campaign
        var assignResponse = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = playerId });
        assignResponse.EnsureSuccessStatusCode();

        // 6. Campaign now lists the player as a member
        var membersResponse = await client.GetAsync($"api/campaigns/{campaign.Id}/members");
        var members = (await membersResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignMemberResponse>>>())!.Data!;
        members.Should().ContainSingle(m => m.PlayerId == playerId && m.Email == playerEmail);
    }
}
```

- [ ] **Step 3: Run the full test suite**

Run: `dotnet test`
Expected: all unit and integration tests `Passed`, including the new `CampaignFlowTests`.

- [ ] **Step 4: Build the Web project to catch any Razor compilation issues from the nav change**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/Layout/NavMenu.razor \
        tests/Ruptura.IntegrationTests/Controllers/CampaignFlowTests.cs
git commit -m "feat: add GM navigation links and end-to-end campaign flow test"
```

---

## What this plan does not cover (next plans in the sequence)

- The homebrew `CatalogEntry` subsystem (spec §4.2) — next plan.
- The `CharacterSheet` core rewrite (`CampaignId`, `IsDead`/`IsRetired` columns, `CharacterStatsCalculator`, all 11 module tabs) — spec §4.3, §5.
- `CharacterJournalEntry` and media storage (`IFileStorageService`) — spec §4.4, §7.
- `Notification` / rank-promotion — spec §4.5.
- `GuildSheet.CampaignId` — trivial column addition, deferred to whichever plan first touches `GuildSheet` again.
