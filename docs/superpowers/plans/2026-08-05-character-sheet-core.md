# Character Sheet Core Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the Character Sheet core — the `CharacterSheet` entity, the granting flow, `CharacterStatsCalculator` (all GDD-derived stats computed on read, never stored), and the 9 non-Journal/non-Notification module tabs (Identity, Attributes, Combat, Skills, Talents, Spells, Techniques, Equipment, Attribute Trial, Guild Registry), for both the GM and Player sides of the app.

**Architecture:** Same Clean Architecture layering as sub-plans #1-2. `CharacterSheet` stores one `DataJson` blob (deserialized to `CharacterSheetData`, a typed record tree in `Ruptura.Shared`) plus a few real columns (`IsDead`/`IsRetired`/`CampaignId`/`OwnerId`) needed for the uniqueness rule and permission checks. `CharacterStatsCalculator` is a pure, stateless service in `Ruptura.Application` — it takes `CharacterSheetData` plus the `CatalogEntry` rows it references and returns a `CharacterDerivedStats` DTO; it never touches the database and never persists anything. The UI is one shared `CharacterSheetEditor` Blazor component (tab shell + save) reused by both the GM's edit page and the player's own-character page, with one child component per tab.

**Tech Stack:** Same as the rest of the repo — ASP.NET Core 8 / EF Core 8 / Npgsql, FluentValidation, xUnit + Moq + FluentAssertions + Bogus, Testcontainers.PostgreSql, Blazor WASM + Blazored.LocalStorage.

## Global Constraints

- **Result pattern**: every Application/Infrastructure service method returns `Result` or `Result<T>` from `Ruptura.Application.Common` — never throw business exceptions across layer boundaries.
- **Bilingual localization**: every user-facing string goes through `IStringLocalizer` — API error/success messages via `IStringLocalizer<SharedResources>` (`src/Ruptura.API/Resources/SharedResources.resx` + `.pt-BR.resx`), Blazor UI strings via `IStringLocalizer<AppStrings>` (`src/Ruptura.Web/Resources/AppStrings.resx` + `.pt-BR.resx`). Every task that adds a user-facing string adds both the `en` and `pt-BR` resx entries in the same task — no follow-up localization pass.
- **GDD is the source of truth**: numeric formulas, thresholds, and Portuguese field values in this plan are transcribed verbatim from `docs/superpowers/specs/2026-08-04-character-sheet-design.md` §5 (which itself transcribes `docs/GDD_Ruptura.md`). Do not "round" or "simplify" a threshold or a Portuguese string value — copy it exactly as given in this plan.
- **Plain-value catalog storage**: `CatalogEntryType` and other domain enums are stored as `int` in Postgres (default EF Core convention) — do not add `HasConversion<string>()`.
- **Official catalog rows are immutable via the API**: `CatalogEntry` rows with `CampaignId == null` can never be created/edited/deleted through `POST/PUT/DELETE /api/catalog` — this is pre-existing behavior (`Catalog.CannotModifyGlobalEntry`), do not weaken it.
- **Permission matrix** (from the design spec §6, restated here because every service/controller task in this plan implements a slice of it):
  | Resource | Read | Create/Edit | Delete |
  |---|---|---|---|
  | `CharacterSheet` (general fields) | Owner or Campaign's GM | Owner or GM | GM (via `IsDead`/`IsRetired`) |
  | `CharacterSheet.IsDead`/`IsRetired` | Owner or GM | **GM only** | — |
  | `CatalogEntry` (official) | All authenticated | — (seed, immutable) | — |
  | `CatalogEntry` (homebrew) | Campaign members | GM only | GM only (soft-delete) |
  | `Campaign` | GM (owns) or member | GM | GM |
- **Unauthorized reads return `NotFound`, not `Forbidden`**: matches the existing `CatalogEntryService`/`CampaignService` convention of not revealing whether a resource exists to a caller with no relationship to it. Reserve an explicit failure code for the one case where the caller *is* authorized to write some fields but not others (`IsDead`/`IsRetired` by a non-GM).
- **No file-upload / media storage in this plan** — `PortraitImagePath` is a plain string field (path/URL), edited as a text input. Wiring it to actual file upload is sub-plan #4 (Journal & media storage).
- **No Notification wiring in this plan** — NP recalculation on save does not trigger any promotion notification. That's sub-plan #5.

---

## File Structure

```
src/Ruptura.Domain/Entities/CharacterSheet.cs                       (modify — add CampaignId, IsDead, IsRetired, PortraitImagePath)
src/Ruptura.Domain/Entities/CatalogEntry.cs                         (modify — add IsArchived)

src/Ruptura.Infrastructure/Data/Configurations/CharacterSheetConfiguration.cs   (new)
src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs     (modify — FK CampaignId→Campaign)
src/Ruptura.Infrastructure/Data/AppDbContext.cs                     (modify — DbSet<CharacterSheet> already exists, no change needed there)
src/Ruptura.Infrastructure/Data/Migrations/...                      (new migration)

src/Ruptura.Infrastructure/Repositories/CharacterSheetRepository.cs (new)
src/Ruptura.Infrastructure/Repositories/CampaignMembershipRepository.cs (modify — GetByPlayerAsync)
src/Ruptura.Infrastructure/Repositories/CatalogEntryRepository.cs   (modify — GetByIdsAsync, includeArchived param)
src/Ruptura.Infrastructure/Services/CharacterSheetService.cs        (new)
src/Ruptura.Infrastructure/Services/CampaignService.cs              (modify — GetMyMembershipsAsync)
src/Ruptura.Infrastructure/Services/CatalogEntryService.cs          (modify — soft-delete, includeArchived)
src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs   (modify — new DI registrations)

src/Ruptura.Application/Interfaces/ICharacterSheetRepository.cs     (new)
src/Ruptura.Application/Interfaces/ICharacterSheetService.cs        (new)
src/Ruptura.Application/Interfaces/ICharacterStatsCalculator.cs     (new)
src/Ruptura.Application/Interfaces/ICampaignMembershipRepository.cs (modify — GetByPlayerAsync)
src/Ruptura.Application/Interfaces/ICampaignService.cs              (modify — GetMyMembershipsAsync)
src/Ruptura.Application/Interfaces/ICatalogEntryRepository.cs       (modify — GetByIdsAsync, includeArchived)
src/Ruptura.Application/Interfaces/ICatalogEntryService.cs          (modify — includeArchived)
src/Ruptura.Application/Services/CharacterStatsCalculator.cs        (new)
src/Ruptura.Application/Common/ErrorCodes.cs                        (modify — CharacterSheet.*, Catalog.AlreadyArchived)
src/Ruptura.Application/Validators/CharacterSheets/GrantCharacterSheetRequestValidator.cs   (new)
src/Ruptura.Application/Validators/CharacterSheets/UpdateCharacterSheetRequestValidator.cs  (new)

src/Ruptura.Shared/CharacterSheets/CharacterSheetData.cs            (new — whole nested data tree)
src/Ruptura.Shared/CharacterSheets/CharacterDerivedStats.cs         (new)
src/Ruptura.Shared/CharacterSheets/GrantCharacterSheetRequest.cs    (new)
src/Ruptura.Shared/CharacterSheets/UpdateCharacterSheetRequest.cs   (new)
src/Ruptura.Shared/CharacterSheets/CharacterSheetResponse.cs        (new)
src/Ruptura.Shared/Catalog/SkillCatalogData.cs                      (new)
src/Ruptura.Shared/Catalog/TalentCatalogData.cs                     (new)
src/Ruptura.Shared/Catalog/EquipmentItemCatalogData.cs              (new)
src/Ruptura.Shared/Catalog/CatalogEntryResponse.cs                  (modify — add IsArchived)
src/Ruptura.Shared/Campaigns/CampaignResponse.cs                    (unchanged, reused)

src/Ruptura.API/Controllers/CharacterSheetController.cs             (new)
src/Ruptura.API/Controllers/CampaignController.cs                   (modify — class-level auth relaxed, GET mine)
src/Ruptura.API/Controllers/CatalogController.cs                    (modify — includeArchived query param)
src/Ruptura.API/Resources/SharedResources.resx / .pt-BR.resx        (modify — new keys)

src/Ruptura.Web/Services/ICharacterSheetClientService.cs            (new)
src/Ruptura.Web/Services/CharacterSheetClientService.cs             (new)
src/Ruptura.Web/Services/ICampaignClientService.cs                  (modify — GetMineAsync)
src/Ruptura.Web/Services/CampaignClientService.cs                   (modify — GetMineAsync)
src/Ruptura.Web/Services/ICatalogClientService.cs                   (modify — includeArchived param)
src/Ruptura.Web/Services/CatalogClientService.cs                    (modify — includeArchived param)
src/Ruptura.Web/Program.cs                                          (modify — DI registration)
src/Ruptura.Web/Pages/CharacterSheetEditor.razor                    (new — shared shell)
src/Ruptura.Web/Pages/CharacterSheetIdentityTab.razor                (new)
src/Ruptura.Web/Pages/CharacterSheetAttributesTab.razor              (new)
src/Ruptura.Web/Pages/CharacterSheetCombatTab.razor                  (new)
src/Ruptura.Web/Pages/CharacterSheetSkillsTab.razor                  (new)
src/Ruptura.Web/Pages/CharacterSheetCatalogRefListTab.razor          (new — shared: Talents/Spells/Techniques)
src/Ruptura.Web/Pages/CharacterSheetEquipmentTab.razor               (new)
src/Ruptura.Web/Pages/CharacterSheetTrialTab.razor                   (new)
src/Ruptura.Web/Pages/CharacterSheetGuildRegistryTab.razor           (new)
src/Ruptura.Web/Pages/PlayerCampaigns.razor                          (new — "/campaigns")
src/Ruptura.Web/Pages/PlayerCharacter.razor                          (new — "/campaigns/{CampaignId}/character")
src/Ruptura.Web/Pages/GmCharacterSheet.razor                         (new — "/gm/campaigns/{CampaignId}/character-sheets/{SheetId}")
src/Ruptura.Web/Pages/GmCampaignDetail.razor                         (modify — grant form + sheet list)
src/Ruptura.Web/Layout/NavMenu.razor                                 (modify — player "Campaigns" link)
src/Ruptura.Web/Resources/AppStrings.resx / .pt-BR.resx              (modify — new keys)

tests/Ruptura.UnitTests/Application/CharacterStatsCalculatorTests.cs (new)
tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs   (new)
tests/Ruptura.UnitTests/Application/CampaignServiceTests.cs         (modify — GetMyMembershipsAsync tests)
tests/Ruptura.UnitTests/Application/CatalogEntryServiceTests.cs     (modify — soft-delete tests)
tests/Ruptura.IntegrationTests/Controllers/CharacterSheetControllerTests.cs (new)
tests/Ruptura.IntegrationTests/Controllers/CharacterSheetFlowTests.cs       (new)
tests/Ruptura.IntegrationTests/Controllers/CampaignControllerTests.cs      (modify — /mine tests)
```

---

## Task 1: Domain model — `CharacterSheet` fields, `CatalogEntry.IsArchived`, FK, unique index, migration

**Files:**
- Modify: `src/Ruptura.Domain/Entities/CharacterSheet.cs`
- Modify: `src/Ruptura.Domain/Entities/CatalogEntry.cs`
- Create: `src/Ruptura.Infrastructure/Data/Configurations/CharacterSheetConfiguration.cs`
- Modify: `src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs`
- Create: migration via `dotnet ef migrations add`

**Interfaces:**
- Produces: `CharacterSheet.CampaignId (Guid)`, `.IsDead (bool)`, `.IsRetired (bool)`, `.PortraitImagePath (string?)`; `CatalogEntry.IsArchived (bool)` — every later task in this plan depends on these fields existing.

- [ ] **Step 1: Update the `CharacterSheet` entity**

```csharp
namespace Ruptura.Domain.Entities;

public class CharacterSheet
{
    public Guid Id { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid GrantedByGameMasterId { get; set; }
    public bool IsDead { get; set; }
    public bool IsRetired { get; set; }
    public string? PortraitImagePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Everything else (attributes, skills, talents, spells, techniques,
    // equipment, currency, attribute trial, guild registry) lives here as
    // JSON — see Ruptura.Shared.CharacterSheets.CharacterSheetData.
    public string DataJson { get; set; } = "{}";
}
```

- [ ] **Step 2: Add `IsArchived` to `CatalogEntry`**

```csharp
using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class CatalogEntry
{
    public Guid Id { get; set; }
    public CatalogEntryType Type { get; set; }
    public Guid? CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public bool IsArchived { get; set; }
    public Guid? CreatedByGameMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Step 3: Create `CharacterSheetConfiguration`**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CharacterSheetConfiguration : IEntityTypeConfiguration<CharacterSheet>
{
    public void Configure(EntityTypeBuilder<CharacterSheet> builder)
    {
        // "1 personagem vivo/não aposentado por jogador por Campaign" — application-level
        // check happens in CharacterSheetService; this is the concurrency safety net.
        builder.HasIndex(c => new { c.OwnerId, c.CampaignId })
            .IsUnique()
            .HasFilter("NOT \"IsDead\" AND NOT \"IsRetired\"")
            .HasDatabaseName("ux_character_sheets_owner_campaign_alive");
    }
}
```

- [ ] **Step 4: Add the `CampaignId` FK to `CatalogEntryConfiguration`**

Add this inside the existing `Configure` method, alongside the two `HasIndex` calls already there (do not remove or change those):

```csharp
        // Homebrew entries belong to exactly one Campaign; if the Campaign is ever
        // deleted, its homebrew catalog goes with it (CatalogEntry.CampaignId was a
        // bare Guid? before this — decided 2026-08-05, see design spec §4.2).
        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(c => c.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
```

(`Campaign` is already `using Ruptura.Domain.Entities;` — no new using needed.)

- [ ] **Step 5: Build to confirm it compiles**

Run: `dotnet build`
Expected: builds with no errors (existing code that constructs `CharacterSheet`/`CatalogEntry` object initializers still compiles — the new properties all have safe defaults).

- [ ] **Step 6: Generate the migration**

```bash
dotnet ef migrations add AddCharacterSheetFieldsAndCatalogArchive \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```

- [ ] **Step 7: Verify the migration content directly — do not trust that it "looks right"**

```bash
grep -n "AddColumn\|CreateIndex\|AddForeignKey" src/Ruptura.Infrastructure/Data/Migrations/*_AddCharacterSheetFieldsAndCatalogArchive.cs
```

Expected: `AddColumn` for `CampaignId`, `IsDead`, `IsRetired`, `PortraitImagePath` on `CharacterSheets`; `AddColumn` for `IsArchived` on `CatalogEntries`; `CreateIndex` for `ux_character_sheets_owner_campaign_alive`; `AddForeignKey` from `CatalogEntries.CampaignId` to `Campaigns.Id` with `onDelete: ReferentialAction.Cascade`. If any of these four are missing, the migration is wrong — do not proceed to Step 8 until they're all present.

Since `CharacterSheets` and `CatalogEntries` already have rows in any environment that ran sub-plan #2's seed migrations, also confirm the new `CharacterSheets.CampaignId` column either has a default or the migration doesn't fail on existing empty `CharacterSheets` (there are no rows yet in production since granting doesn't exist until this plan ships, but local dev DBs may have test rows) — a plain `AddColumn<Guid>` without a default will fail on a non-empty table. If `CharacterSheets` has any existing rows in your local dev DB, either truncate the table first (`docker exec -it <postgres container> psql -U <user> -d <db> -c 'TRUNCATE "CharacterSheets";'`) or add `defaultValue: Guid.Empty` to that specific `AddColumn` call before applying.

- [ ] **Step 8: Apply the migration and verify against a real database**

```bash
dotnet ef database update \
  --project src/Ruptura.Infrastructure \
  --startup-project src/Ruptura.API
```

Expected: no errors. Then confirm the FK and index actually exist in Postgres:

```bash
docker compose exec -T db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "\d \"CatalogEntries\"" | grep -i "foreign\|cascade"
docker compose exec -T db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "\d \"CharacterSheets\"" | grep -i "ux_character_sheets"
```

(Adjust the `docker compose exec` invocation to however Postgres is actually reachable in your environment — via the `make` targets' container name, or a local `psql` connection string from `.env`, whichever this repo's Docker setup uses.)

- [ ] **Step 9: Commit**

```bash
git add src/Ruptura.Domain/Entities/CharacterSheet.cs src/Ruptura.Domain/Entities/CatalogEntry.cs \
  src/Ruptura.Infrastructure/Data/Configurations/CharacterSheetConfiguration.cs \
  src/Ruptura.Infrastructure/Data/Configurations/CatalogEntryConfiguration.cs \
  src/Ruptura.Infrastructure/Data/Migrations/
git commit -m "feat: add CharacterSheet campaign/status fields and CatalogEntry soft-delete"
```

## Task 2: Player-facing Campaign read access (`GET /api/campaigns/mine`)

Closes a real gap: the player-side pages this plan builds (`/campaigns`, `/campaigns/{id}/character`) need a way to list "campaigns I'm in", and today `CampaignController` is entirely `[Authorize(Roles = "GameMaster")]` with no player-facing read endpoint at all.

**Files:**
- Modify: `src/Ruptura.Application/Interfaces/ICampaignMembershipRepository.cs`
- Modify: `src/Ruptura.Infrastructure/Repositories/CampaignMembershipRepository.cs`
- Modify: `src/Ruptura.Application/Interfaces/ICampaignService.cs`
- Modify: `src/Ruptura.Infrastructure/Services/CampaignService.cs`
- Modify: `src/Ruptura.API/Controllers/CampaignController.cs`
- Test: `tests/Ruptura.UnitTests/Application/CampaignServiceTests.cs`
- Test: `tests/Ruptura.IntegrationTests/Controllers/CampaignControllerTests.cs`

**Interfaces:**
- Consumes: `ICampaignRepository.GetByGameMasterAsync(Guid, CancellationToken)` (existing), `ICampaignRepository.GetByIdAsync(Guid, CancellationToken)` (existing, from `IRepository<T>`).
- Produces: `ICampaignService.GetMyMembershipsAsync(Guid callerId, bool isGameMaster, CancellationToken ct = default) -> Result<IEnumerable<CampaignResponse>>` — used by Task 18's player pages.

- [ ] **Step 1: Write the failing unit test for the new repository method**

Add to `tests/Ruptura.UnitTests/Application/CampaignServiceTests.cs`, in the `// ── GetRosterAsync ──` region area (new region below it):

```csharp
    // ── GetMyMembershipsAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetMyMembershipsAsync_AsGameMaster_ReturnsCampaignsTheyRun()
    {
        var gmId = Guid.NewGuid();
        var campaigns = new List<Campaign>
        {
            new() { Id = Guid.NewGuid(), Name = "Arc One", GameMasterId = gmId }
        };
        _campaignRepoMock.Setup(r => r.GetByGameMasterAsync(gmId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaigns);

        var result = await _sut.GetMyMembershipsAsync(gmId, isGameMaster: true);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(c => c.Name == "Arc One");
    }

    [Fact]
    public async Task GetMyMembershipsAsync_AsPlayer_ReturnsCampaignsTheyreAMemberOf()
    {
        var playerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), Name = "Sunken Gate", GameMasterId = Guid.NewGuid() };
        _membershipRepoMock.Setup(r => r.GetByPlayerAsync(playerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new CampaignMembership { CampaignId = campaign.Id, PlayerId = playerId }]);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaign);

        var result = await _sut.GetMyMembershipsAsync(playerId, isGameMaster: false);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().ContainSingle(c => c.Name == "Sunken Gate");
    }
```

- [ ] **Step 2: Run it to confirm it fails to compile (method doesn't exist yet)**

Run: `dotnet test tests/Ruptura.UnitTests --filter GetMyMembershipsAsync`
Expected: build error — `ICampaignService` has no `GetMyMembershipsAsync`, `ICampaignMembershipRepository` has no `GetByPlayerAsync`.

- [ ] **Step 3: Add `GetByPlayerAsync` to the repository interface and implementation**

In `src/Ruptura.Application/Interfaces/ICampaignMembershipRepository.cs`, add alongside the existing two methods:

```csharp
    Task<IEnumerable<CampaignMembership>> GetByPlayerAsync(Guid playerId, CancellationToken ct = default);
```

In `src/Ruptura.Infrastructure/Repositories/CampaignMembershipRepository.cs`, add:

```csharp
    public async Task<IEnumerable<CampaignMembership>> GetByPlayerAsync(
        Guid playerId,
        CancellationToken ct = default) =>
        await Set
            .Where(m => m.PlayerId == playerId)
            .OrderBy(m => m.AssignedAt)
            .ToListAsync(ct);
```

- [ ] **Step 4: Add `GetMyMembershipsAsync` to the service interface and implementation**

In `src/Ruptura.Application/Interfaces/ICampaignService.cs`, add:

```csharp
    Task<Result<IEnumerable<CampaignResponse>>> GetMyMembershipsAsync(
        Guid callerId, bool isGameMaster, CancellationToken ct = default);
```

In `src/Ruptura.Infrastructure/Services/CampaignService.cs`, add (uses the existing private `MapToResponse` helper already in this file):

```csharp
    public async Task<Result<IEnumerable<CampaignResponse>>> GetMyMembershipsAsync(
        Guid callerId,
        bool isGameMaster,
        CancellationToken ct = default)
    {
        if (isGameMaster)
        {
            var owned = await campaignRepo.GetByGameMasterAsync(callerId, ct);
            return Result.Success(owned.Select(MapToResponse));
        }

        var memberships = await membershipRepo.GetByPlayerAsync(callerId, ct);
        var campaigns = new List<Campaign>();
        foreach (var membership in memberships)
        {
            var campaign = await campaignRepo.GetByIdAsync(membership.CampaignId, ct);
            if (campaign is not null) campaigns.Add(campaign);
        }

        return Result.Success(campaigns.Select(MapToResponse));
    }
```

- [ ] **Step 5: Run the unit tests to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter GetMyMembershipsAsync`
Expected: PASS (2/2).

- [ ] **Step 6: Relax `CampaignController`'s authorization and add the `mine` endpoint**

Change the class-level attribute and add explicit per-action attributes so the three existing GM-only actions keep their exact current behavior:

```csharp
[ApiController]
[Route("api/campaigns")]
[Authorize]
public class CampaignController(
    ICampaignService campaignService,
    IStringLocalizer<SharedResources> localizer,
    IValidator<CreateCampaignRequest> createValidator,
    IValidator<AssignMemberRequest> assignValidator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "GameMaster")]
    // ...unchanged body...

    [HttpGet]
    [Authorize(Roles = "GameMaster")]
    // ...unchanged body...

    [HttpGet("{campaignId:guid}/members")]
    [Authorize(Roles = "GameMaster")]
    // ...unchanged body...

    [HttpPost("{campaignId:guid}/members")]
    [Authorize(Roles = "GameMaster")]
    // ...unchanged body...

    [HttpGet("mine")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CampaignResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var isGameMaster = User.IsInRole("GameMaster");
        var result = await campaignService.GetMyMembershipsAsync(callerId, isGameMaster, ct);

        return Ok(ApiResponse<IEnumerable<CampaignResponse>>.Ok(result.Value!));
    }
}
```

(Only the class-level `[Authorize(Roles = "GameMaster")]` → `[Authorize]` change and the four added `[Authorize(Roles = "GameMaster")]` lines and the new `Mine` action are new — every other action body is untouched.)

- [ ] **Step 7: Write the integration test**

Add to `tests/Ruptura.IntegrationTests/Controllers/CampaignControllerTests.cs` (add `using Ruptura.Shared.Invites;` to the file's usings if not already present, for `InviteCodeResponse`):

```csharp
    [Fact]
    public async Task GetMine_AsPlayerMemberOfACampaign_ReturnsThatCampaign()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Mine Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var invite = await client.PostAsync("api/invites", null);
        var inviteCode = (await invite.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        AuthHelper.SetBearerToken(client, player.AccessToken);
        var mineResponse = await client.GetAsync("api/campaigns/mine");
        var mine = (await mineResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignResponse>>>())!.Data!;

        mine.Should().ContainSingle(c => c.Id == campaign.Id);
    }
```

(Verified against the existing `CampaignFlowTests.FullFlow_RegisterRecruitCreateCampaignAssign_Succeeds` — `POST api/invites` with a `null` body returns `ApiResponse<InviteCodeResponse>`, and `AuthResponse.User.Id` — from `AuthHelper.RegisterPlayerAsync`'s return value — is the registered player's id directly, no roster round-trip needed.)

- [ ] **Step 8: Run the integration test**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter GetMine_AsPlayerMemberOfACampaign_ReturnsThatCampaign`
Expected: PASS. If it fails on an unrelated Serilog "logger already frozen" error, re-run once before treating it as a real failure — this is a documented pre-existing flake in this test project.

- [ ] **Step 9: Commit**

```bash
git add src/Ruptura.Application/Interfaces/ICampaignMembershipRepository.cs \
  src/Ruptura.Infrastructure/Repositories/CampaignMembershipRepository.cs \
  src/Ruptura.Application/Interfaces/ICampaignService.cs \
  src/Ruptura.Infrastructure/Services/CampaignService.cs \
  src/Ruptura.API/Controllers/CampaignController.cs \
  tests/Ruptura.UnitTests/Application/CampaignServiceTests.cs \
  tests/Ruptura.IntegrationTests/Controllers/CampaignControllerTests.cs
git commit -m "feat: add player-facing GET /api/campaigns/mine"
```

## Task 3: `CharacterSheetData` shared record tree + catalog calculation DTOs

Pure data-shape task — no logic, no tests beyond "it compiles and round-trips through `JsonSerializer`". Every later task in this plan depends on these exact type/property names.

**Files:**
- Create: `src/Ruptura.Shared/CharacterSheets/CharacterSheetData.cs`
- Create: `src/Ruptura.Shared/CharacterSheets/CharacterDerivedStats.cs`
- Create: `src/Ruptura.Shared/Catalog/SkillCatalogData.cs`
- Create: `src/Ruptura.Shared/Catalog/TalentCatalogData.cs`
- Create: `src/Ruptura.Shared/Catalog/EquipmentItemCatalogData.cs`
- Test: `tests/Ruptura.UnitTests/Application/CharacterSheetDataSerializationTests.cs`

**Interfaces:**
- Produces: every type below, used verbatim (same namespace, same property names) by every remaining task in this plan. Do not rename anything here without updating every later task's code samples.

- [ ] **Step 1: Create the `CharacterSheetData` tree**

```csharp
namespace Ruptura.Shared.CharacterSheets;

public class CharacterSheetData
{
    public CharacterIdentity Identity { get; set; } = new();
    public CharacterAttributes Attributes { get; set; } = new();
    public CharacterCombat Combat { get; set; } = new();
    public List<CharacterSkillEntry> Skills { get; set; } = [];
    public List<CharacterCatalogRefEntry> Talents { get; set; } = [];
    public List<CharacterCatalogRefEntry> Spells { get; set; } = [];
    public List<CharacterCatalogRefEntry> Techniques { get; set; } = [];
    public List<CharacterEquipmentEntry> Equipment { get; set; } = [];
    public CharacterCurrency Currency { get; set; } = new();
    public CharacterAttributeTrial? AttributeTrial { get; set; }
    public CharacterGuildRegistry GuildRegistry { get; set; } = new();
}

// Module 1: Identidade. Origin/Background/Lineage/Aptitude/InitialTalent are CatalogEntry
// references (Origin, Background, Lineage, Aptitude, Talent types respectively).
// PatronDisplayName is flavor text for the printed sheet's "Jogador/Patrono" field —
// CharacterSheet.OwnerId is always the real owner for authorization purposes.
public class CharacterIdentity
{
    public Guid? OriginId { get; set; }
    public Guid? BackgroundId { get; set; }
    public Guid? LineageId { get; set; }
    public List<Guid> AptitudeIds { get; set; } = []; // GDD: exactly 2, not enforced server-side in this slice
    public Guid? InitialTalentId { get; set; }
    public string PatronDisplayName { get; set; } = string.Empty;
}

// Module 2: Atributos. Base score 1-6 (GDD); grade/modifier are always calculated
// (CharacterStatsCalculator), never stored.
public class CharacterAttributes
{
    public int Corpo { get; set; } = 1;
    public int Controle { get; set; } = 1;
    public int Vigor { get; set; } = 1;
    public int Presenca { get; set; } = 1;
    public int Intelecto { get; set; } = 1;
    public int Percepcao { get; set; } = 1;
    public int Vontade { get; set; } = 1;
    public int Afinidade { get; set; } = 1;
}

// Module 3: Combate. Only what's NOT derivable (current HP, active conditions) — PV
// max, Defesa Passiva, Deslocamento, Iniciativa, and the weapon table are all calculated.
public class CharacterCombat
{
    public int CurrentHp { get; set; }
    public List<string> ActiveConditions { get; set; } = [];
}

// Module 4: Perícias. Points invested → grade calculated by CharacterStatsCalculator.
public class CharacterSkillEntry
{
    public Guid CatalogEntryId { get; set; }
    public int Points { get; set; }
}

// Modules 5-7: Talentos, Magias Conhecidas, Técnicas/Posturas — just a reference to the
// CatalogEntry (Talent/Spell/Technique type respectively); everything else about them
// (Effect, School, PaCost, ...) is looked up from the CatalogEntry's DataJson on read.
public class CharacterCatalogRefEntry
{
    public Guid CatalogEntryId { get; set; }
}

// Module 8: Equipamentos e Inventário.
// IsEquipped: only equipped items feed Combat derived stats (weapon table row; armor/shield
// DefenseBonus + ArmorDamageReduction into Defesa Passiva).
// LinkedSkillEntryId: which invested Skill (a CatalogEntryId also present in Skills[])
// governs this weapon's attack/damage — the catalog doesn't tie an item to a Skill, the
// player picks per equipped item. Null for non-weapons or unassigned weapons.
public class CharacterEquipmentEntry
{
    public Guid CatalogEntryId { get; set; }
    public int Quantity { get; set; } = 1;
    public int DurabilityRemaining { get; set; }
    public bool IsEquipped { get; set; }
    public Guid? LinkedSkillEntryId { get; set; }
}

public class CharacterCurrency
{
    public int PactCoins { get; set; }
}

// Module 9: Provação de Atributo — manual entry, no campaign calendar in this slice.
public class CharacterAttributeTrial
{
    public string AttributeName { get; set; } = string.Empty;
    public string TargetGrade { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
}

// Module 10: Registro da Guilda. Ranking is one of the 8 GDD rank names below — stored as
// a plain string (not an enum) because two of them contain accented characters that aren't
// valid C# enum-member identifiers ("Aço", "Lendário"). Valid values:
// "Bronze" | "Ferro" | "Aço" | "Prata" | "Ouro" | "Mithril" | "Adamante" | "Lendário".
// State is free descriptive text (ativo/ferido/ausente/desaparecido, ...) with no mechanical
// effect — distinct from CharacterSheet.IsDead/IsRetired, which are the real columns that
// matter for the uniqueness rule.
public class CharacterGuildRegistry
{
    public string Ranking { get; set; } = "Bronze";
    public DateTime? JoinedDate { get; set; }
    public string State { get; set; } = string.Empty;
    public int Expeditions { get; set; }
    public int FloorsCleared { get; set; }
}
```

- [ ] **Step 2: Create the `CharacterDerivedStats` output shape**

```csharp
namespace Ruptura.Shared.CharacterSheets;

// Everything CharacterStatsCalculator computes — never persisted, always recomputed on read.
public class CharacterDerivedStats
{
    public Dictionary<string, int> AttributeModifiers { get; set; } = [];    // key: CharacterAttributes property name
    public Dictionary<string, int> AttributeGradeBonuses { get; set; } = []; // key: CharacterAttributes property name
    public int MaxHp { get; set; }
    public int Movement { get; set; }
    public int Initiative { get; set; }
    public int PassiveDefense { get; set; }
    public int DamageReduction { get; set; }
    public int CarryCapacity { get; set; }
    public decimal CurrentWeight { get; set; }
    public int Np { get; set; }
    public Dictionary<Guid, int> SkillGradeBonuses { get; set; } = []; // key: Skills[].CatalogEntryId
    public List<WeaponCombatRow> Weapons { get; set; } = [];
}

public class WeaponCombatRow
{
    public Guid CatalogEntryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AttackBonus { get; set; }
    public string DamageFormula { get; set; } = string.Empty; // e.g. "1d8 +3" — the dice itself is rolled at the table
}
```

- [ ] **Step 3: Create the three catalog calculation DTOs**

These deserialize `CatalogEntry.DataJson` for the three types the calculator actually reads (`Skill`, `Talent`, `EquipmentItem` — Origin/Background/Lineage/Aptitude/Spell/Technique are narrative-only in this slice per the design spec §4.2.1 and are never deserialized to a typed shape). Property names must match exactly what `src/Ruptura.Infrastructure/Data/Seed/CatalogSeedData.Skills.cs` and `.Talents.cs` already serialize (verified: `Area`, `RelatedAttribute`, `Category`, `Effect`, `PowerTier`) — `System.Text.Json.JsonSerializer.Deserialize` is case-sensitive by default with no custom options anywhere in this codebase, so an exact match matters.

```csharp
namespace Ruptura.Shared.Catalog;

public class SkillCatalogData
{
    public string Area { get; set; } = string.Empty;
    public string RelatedAttribute { get; set; } = string.Empty; // one of the 8 GDD attribute names, accented
}
```

```csharp
namespace Ruptura.Shared.Catalog;

public class TalentCatalogData
{
    public string Category { get; set; } = string.Empty;
    public string Effect { get; set; } = string.Empty;
    public string PowerTier { get; set; } = string.Empty; // "menor" | "médio" | "maior"
}
```

```csharp
namespace Ruptura.Shared.Catalog;

// EquipmentItem is deliberately unseeded (homebrew-only, see sub-plan #2 notes) — this
// shape matches the design spec §4.2.1 field list exactly, for when a GM creates one via
// the existing raw-DataJson Catalog admin page.
public class EquipmentItemCatalogData
{
    public string Category { get; set; } = string.Empty; // "arma" | "armadura" | "escudo" | "item"
    public string Rarity { get; set; } = string.Empty;    // Comum/Incomum/Raro/Épico/Lendário/Divino
    public int AttackBonus { get; set; }
    public int DamageBonus { get; set; }
    public int DefenseBonus { get; set; }
    public string? WeaponDiceCategory { get; set; }  // Leve/Média/Pesada/DuasMãos — set only if Category == "arma"
    public int? ArmorDamageReduction { get; set; }    // set only if Category == "armadura"
    public decimal Weight { get; set; }
}
```

- [ ] **Step 4: Write a round-trip serialization test**

```csharp
using System.Text.Json;
using FluentAssertions;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.UnitTests.Application;

public class CharacterSheetDataSerializationTests
{
    [Fact]
    public void CharacterSheetData_RoundTripsThroughJson_PreservingAllModules()
    {
        var original = new CharacterSheetData
        {
            Identity = new CharacterIdentity
            {
                OriginId = Guid.NewGuid(),
                AptitudeIds = [Guid.NewGuid(), Guid.NewGuid()],
                PatronDisplayName = "Dom Alric"
            },
            Attributes = new CharacterAttributes { Corpo = 3, Controle = 4 },
            Combat = new CharacterCombat { CurrentHp = 12, ActiveConditions = ["Ferido"] },
            Skills = [new CharacterSkillEntry { CatalogEntryId = Guid.NewGuid(), Points = 30 }],
            Talents = [new CharacterCatalogRefEntry { CatalogEntryId = Guid.NewGuid() }],
            Equipment =
            [
                new CharacterEquipmentEntry
                {
                    CatalogEntryId = Guid.NewGuid(), Quantity = 1, IsEquipped = true,
                    LinkedSkillEntryId = Guid.NewGuid()
                }
            ],
            GuildRegistry = new CharacterGuildRegistry { Ranking = "Aço", Expeditions = 2 }
        };

        var json = JsonSerializer.Serialize(original);
        var roundTripped = JsonSerializer.Deserialize<CharacterSheetData>(json);

        roundTripped.Should().NotBeNull();
        roundTripped!.Identity.PatronDisplayName.Should().Be("Dom Alric");
        roundTripped.Identity.AptitudeIds.Should().HaveCount(2);
        roundTripped.Attributes.Corpo.Should().Be(3);
        roundTripped.Combat.ActiveConditions.Should().ContainSingle().Which.Should().Be("Ferido");
        roundTripped.Skills.Should().ContainSingle().Which.Points.Should().Be(30);
        roundTripped.Equipment.Should().ContainSingle().Which.IsEquipped.Should().BeTrue();
        roundTripped.GuildRegistry.Ranking.Should().Be("Aço");
    }

    [Fact]
    public void SkillCatalogData_DeserializesExistingSeedJsonShape()
    {
        const string json = """{"Area":"Combate — Armas","RelatedAttribute":"Controle"}""";

        var data = JsonSerializer.Deserialize<Ruptura.Shared.Catalog.SkillCatalogData>(json);

        data.Should().NotBeNull();
        data!.RelatedAttribute.Should().Be("Controle");
    }
}
```

- [ ] **Step 5: Run the tests**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterSheetDataSerializationTests`
Expected: PASS (2/2).

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Shared/CharacterSheets/ src/Ruptura.Shared/Catalog/SkillCatalogData.cs \
  src/Ruptura.Shared/Catalog/TalentCatalogData.cs src/Ruptura.Shared/Catalog/EquipmentItemCatalogData.cs \
  tests/Ruptura.UnitTests/Application/CharacterSheetDataSerializationTests.cs
git commit -m "feat: add CharacterSheetData record tree and catalog calculation DTOs"
```

## Task 4: `CharacterStatsCalculator`

Pure calculation service — no DB, no I/O. This is the highest-value piece of the whole feature (the design spec's §1: "o maior valor sobre a ficha em papel"), so it gets the most thorough test coverage in the plan. Every formula and threshold below is copied verbatim from the design spec §5.

**Files:**
- Create: `src/Ruptura.Application/Interfaces/ICharacterStatsCalculator.cs`
- Create: `src/Ruptura.Application/Services/CharacterStatsCalculator.cs`
- Test: `tests/Ruptura.UnitTests/Application/CharacterStatsCalculatorTests.cs`

**Interfaces:**
- Consumes: `CharacterSheetData` and all its nested types, `SkillCatalogData`/`TalentCatalogData`/`EquipmentItemCatalogData` (Task 3); `Ruptura.Domain.Entities.CatalogEntry` (existing).
- Produces: `ICharacterStatsCalculator.Calculate(CharacterSheetData data, IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries) -> CharacterDerivedStats` — consumed by `CharacterSheetService` in Tasks 6-8. The `catalogEntries` dictionary must contain every `CatalogEntry` referenced anywhere in `data` (Skills/Talents/Spells/Techniques/Equipment ids, plus `Equipment[].LinkedSkillEntryId`), keyed by `Id` — building that dictionary is the caller's job (Task 6), not the calculator's.

- [ ] **Step 1: Write the interface**

```csharp
using Ruptura.Domain.Entities;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Application.Interfaces;

public interface ICharacterStatsCalculator
{
    CharacterDerivedStats Calculate(CharacterSheetData data, IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries);
}
```

- [ ] **Step 2: Write the failing tests first — one per formula, plus the skill-grade threshold table**

```csharp
using System.Text.Json;
using FluentAssertions;
using Ruptura.Application.Services;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.UnitTests.Application;

public class CharacterStatsCalculatorTests
{
    private readonly CharacterStatsCalculator _sut = new();

    private static CatalogEntry Skill(Guid id, string relatedAttribute) => new()
    {
        Id = id, Type = CatalogEntryType.Skill, Name = "Test Skill",
        DataJson = JsonSerializer.Serialize(new { Area = "Combate — Armas", RelatedAttribute = relatedAttribute })
    };

    private static CatalogEntry Talent(Guid id, string powerTier) => new()
    {
        Id = id, Type = CatalogEntryType.Talent, Name = "Test Talent",
        DataJson = JsonSerializer.Serialize(new { Category = "Combate", Effect = "x", PowerTier = powerTier })
    };

    private static CatalogEntry Equipment(
        Guid id, string category, string rarity, int attackBonus = 0, int damageBonus = 0,
        int defenseBonus = 0, string? diceCategory = null, int? armorReduction = null, decimal weight = 0) => new()
    {
        Id = id, Type = CatalogEntryType.EquipmentItem, Name = "Test Item",
        DataJson = JsonSerializer.Serialize(new
        {
            Category = category, Rarity = rarity, AttackBonus = attackBonus, DamageBonus = damageBonus,
            DefenseBonus = defenseBonus, WeaponDiceCategory = diceCategory,
            ArmorDamageReduction = armorReduction, Weight = weight
        })
    };

    // ── Attribute modifier / grade bonus ────────────────────────────────────

    [Theory]
    [InlineData(1, -1, 0)]
    [InlineData(2, 0, 1)]
    [InlineData(5, 3, 4)]
    public void Calculate_AttributeModifierAndGradeBonus_MatchGdd(int score, int expectedModifier, int expectedGrade)
    {
        var data = new CharacterSheetData { Attributes = new CharacterAttributes { Corpo = score } };

        var result = _sut.Calculate(data, new Dictionary<Guid, CatalogEntry>());

        result.AttributeModifiers["Corpo"].Should().Be(expectedModifier);
        result.AttributeGradeBonuses["Corpo"].Should().Be(expectedGrade);
    }

    // ── Skill grade bonus thresholds ────────────────────────────────────────

    [Theory]
    [InlineData(0, -2)]
    [InlineData(9, -2)]
    [InlineData(10, 0)]
    [InlineData(24, 0)]
    [InlineData(25, 1)]
    [InlineData(49, 1)]
    [InlineData(50, 2)]
    [InlineData(74, 2)]
    [InlineData(75, 3)]
    [InlineData(99, 3)]
    [InlineData(100, 4)]
    [InlineData(250, 4)]
    public void Calculate_SkillGradeBonus_MatchesGdrThresholdTable(int points, int expectedGrade)
    {
        var skillId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Skills = [new CharacterSkillEntry { CatalogEntryId = skillId, Points = points }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry> { [skillId] = Skill(skillId, "Controle") };

        var result = _sut.Calculate(data, catalog);

        result.SkillGradeBonuses[skillId].Should().Be(expectedGrade);
    }

    // ── PV Máximo (per Ranking) ──────────────────────────────────────────────

    [Theory]
    [InlineData("Bronze", 0)]
    [InlineData("Ferro", 5)]
    [InlineData("Aço", 10)]
    [InlineData("Prata", 15)]
    [InlineData("Ouro", 20)]
    [InlineData("Mithril", 25)]
    [InlineData("Adamante", 30)]
    [InlineData("Lendário", 35)]
    public void Calculate_MaxHp_Is10PlusVigorTimes2PlusRankingBonus(string ranking, int expectedRankingBonus)
    {
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Vigor = 3 },
            GuildRegistry = new CharacterGuildRegistry { Ranking = ranking }
        };

        var result = _sut.Calculate(data, new Dictionary<Guid, CatalogEntry>());

        result.MaxHp.Should().Be(10 + 3 * 2 + expectedRankingBonus);
    }

    // ── Movement / Initiative ────────────────────────────────────────────────

    [Fact]
    public void Calculate_Movement_Is4PlusVigorModifier()
    {
        var data = new CharacterSheetData { Attributes = new CharacterAttributes { Vigor = 4 } };

        var result = _sut.Calculate(data, new Dictionary<Guid, CatalogEntry>());

        result.Movement.Should().Be(4 + (4 - 2));
    }

    [Fact]
    public void Calculate_Initiative_IsControleModifier()
    {
        var data = new CharacterSheetData { Attributes = new CharacterAttributes { Controle = 5 } };

        var result = _sut.Calculate(data, new Dictionary<Guid, CatalogEntry>());

        result.Initiative.Should().Be(5 - 2);
    }

    // ── Passive Defense + Damage Reduction from equipped armor/shield ───────

    [Fact]
    public void Calculate_PassiveDefenseAndDamageReduction_OnlyCountEquippedArmorAndShield()
    {
        var armorId = Guid.NewGuid();
        var shieldId = Guid.NewGuid();
        var unequippedArmorId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 3 },
            Equipment =
            [
                new CharacterEquipmentEntry { CatalogEntryId = armorId, IsEquipped = true },
                new CharacterEquipmentEntry { CatalogEntryId = shieldId, IsEquipped = true },
                new CharacterEquipmentEntry { CatalogEntryId = unequippedArmorId, IsEquipped = false }
            ]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [armorId] = Equipment(armorId, "armadura", "Comum", defenseBonus: 2, armorReduction: 2),
            [shieldId] = Equipment(shieldId, "escudo", "Comum", defenseBonus: 1),
            [unequippedArmorId] = Equipment(unequippedArmorId, "armadura", "Comum", defenseBonus: 99, armorReduction: 99)
        };

        var result = _sut.Calculate(data, catalog);

        result.PassiveDefense.Should().Be(10 + (3 - 2) + 2 + 1);
        result.DamageReduction.Should().Be(2);
    }

    // ── Carry capacity / current weight ─────────────────────────────────────

    [Fact]
    public void Calculate_CarryCapacity_IsCorpoScoreTimes5_AndWeightSumsQuantityTimesItemWeight()
    {
        var itemId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Corpo = 4 },
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = itemId, Quantity = 3 }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [itemId] = Equipment(itemId, "item", "Comum", weight: 1.5m)
        };

        var result = _sut.Calculate(data, catalog);

        result.CarryCapacity.Should().Be(4 * 5);
        result.CurrentWeight.Should().Be(4.5m);
    }

    // ── Weapon attack/damage row ─────────────────────────────────────────────

    [Fact]
    public void Calculate_EquippedWeaponWithLinkedSkill_ProducesAttackBonusAndDamageFormula()
    {
        var skillId = Guid.NewGuid();
        var weaponId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 4 }, // modifier +2, grade bonus +3
            Skills = [new CharacterSkillEntry { CatalogEntryId = skillId, Points = 30 }], // grade bonus +1
            Equipment =
            [
                new CharacterEquipmentEntry
                {
                    CatalogEntryId = weaponId, IsEquipped = true, LinkedSkillEntryId = skillId
                }
            ]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [skillId] = Skill(skillId, "Controle"),
            [weaponId] = Equipment(weaponId, "arma", "Comum", damageBonus: 2, diceCategory: "Média")
        };

        var result = _sut.Calculate(data, catalog);

        var row = result.Weapons.Should().ContainSingle().Subject;
        row.CatalogEntryId.Should().Be(weaponId);
        row.AttackBonus.Should().Be(3 + 1); // attribute grade bonus + skill grade bonus
        row.DamageFormula.Should().Be("1d8 +5"); // dice(Média) + (attr modifier +2 + skill grade +1 + item damageBonus +2)
    }

    [Fact]
    public void Calculate_UnequippedWeapon_DoesNotAppearInWeaponsTable()
    {
        var weaponId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = weaponId, IsEquipped = false }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [weaponId] = Equipment(weaponId, "arma", "Comum", diceCategory: "Leve")
        };

        var result = _sut.Calculate(data, catalog);

        result.Weapons.Should().BeEmpty();
    }

    // ── NP ────────────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_Np_SumsAttributeAndSkillGradeBonusesPlusTalentAndEquipmentWeights()
    {
        var talentId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var skillId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            // 8 attributes at score 2 → grade bonus 1 each → 8 total
            Attributes = new CharacterAttributes
            {
                Corpo = 2, Controle = 2, Vigor = 2, Presenca = 2,
                Intelecto = 2, Percepcao = 2, Vontade = 2, Afinidade = 2
            },
            Skills = [new CharacterSkillEntry { CatalogEntryId = skillId, Points = 25 }], // grade +1
            Talents = [new CharacterCatalogRefEntry { CatalogEntryId = talentId }],       // "maior" → 5
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = itemId, Quantity = 1 }] // "Raro" → 7
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [skillId] = Skill(skillId, "Controle"),
            [talentId] = Talent(talentId, "maior"),
            [itemId] = Equipment(itemId, "item", "Raro")
        };

        var result = _sut.Calculate(data, catalog);

        result.Np.Should().Be(8 + 1 + 5 + 7);
    }
}
```

- [ ] **Step 3: Run the tests to confirm they fail (implementation doesn't exist)**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterStatsCalculatorTests`
Expected: build error — `CharacterStatsCalculator` doesn't exist yet.

- [ ] **Step 4: Implement `CharacterStatsCalculator`**

```csharp
using System.Text.Json;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Catalog;

namespace Ruptura.Application.Services;

public class CharacterStatsCalculator : ICharacterStatsCalculator
{
    private static readonly Dictionary<string, int> RankingHpBonus = new()
    {
        ["Bronze"] = 0, ["Ferro"] = 5, ["Aço"] = 10, ["Prata"] = 15,
        ["Ouro"] = 20, ["Mithril"] = 25, ["Adamante"] = 30, ["Lendário"] = 35
    };

    private static readonly Dictionary<string, string> WeaponDiceByCategory = new()
    {
        ["Leve"] = "1d6", ["Média"] = "1d8", ["Pesada"] = "1d10", ["DuasMãos"] = "2d6"
    };

    private static readonly Dictionary<string, int> TalentNpWeight = new()
    {
        ["menor"] = 1, ["médio"] = 3, ["maior"] = 5
    };

    private static readonly Dictionary<string, int> EquipmentNpWeight = new()
    {
        ["Comum"] = 1, ["Incomum"] = 3, ["Raro"] = 7, ["Épico"] = 15, ["Lendário"] = 30, ["Divino"] = 50
    };

    public CharacterDerivedStats Calculate(
        CharacterSheetData data,
        IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries)
    {
        var attributeScores = GetAttributeScores(data.Attributes);
        var attributeModifiers = attributeScores.ToDictionary(kv => kv.Key, kv => kv.Value - 2);
        var attributeGradeBonuses = attributeScores.ToDictionary(kv => kv.Key, kv => kv.Value - 1);

        var skillGradeBonuses = data.Skills.ToDictionary(s => s.CatalogEntryId, s => SkillGradeBonus(s.Points));

        var rankingBonus = RankingHpBonus.GetValueOrDefault(data.GuildRegistry.Ranking, 0);
        var maxHp = 10 + attributeScores["Vigor"] * 2 + rankingBonus;
        var movement = 4 + attributeModifiers["Vigor"];
        var initiative = attributeModifiers["Controle"];

        var equipped = data.Equipment
            .Where(e => e.IsEquipped)
            .Select(e => (Entry: e, Data: DeserializeEquipment(e.CatalogEntryId, catalogEntries)))
            .Where(x => x.Data is not null)
            .ToList();

        var armorAndShieldDefense = equipped
            .Where(x => x.Data!.Category is "armadura" or "escudo")
            .Sum(x => x.Data!.DefenseBonus);
        var passiveDefense = 10 + attributeModifiers["Controle"] + armorAndShieldDefense;

        var damageReduction = equipped
            .Where(x => x.Data!.Category == "armadura")
            .Sum(x => x.Data!.ArmorDamageReduction ?? 0);

        var carryCapacity = attributeScores["Corpo"] * 5;
        var currentWeight = data.Equipment.Sum(e =>
            (DeserializeEquipment(e.CatalogEntryId, catalogEntries)?.Weight ?? 0) * e.Quantity);

        var weapons = equipped
            .Where(x => x.Data!.Category == "arma")
            .Select(x =>
            {
                var name = catalogEntries.TryGetValue(x.Entry.CatalogEntryId, out var itemEntry)
                    ? itemEntry.Name : string.Empty;
                return BuildWeaponRow(
                    x.Entry, x.Data!, name, attributeModifiers, attributeGradeBonuses,
                    skillGradeBonuses, catalogEntries);
            })
            .ToList();

        var np = attributeGradeBonuses.Values.Sum()
            + skillGradeBonuses.Values.Sum()
            + data.Talents.Sum(t => TalentNpWeightFor(t.CatalogEntryId, catalogEntries))
            + data.Equipment.Sum(e => EquipmentNpWeightFor(e.CatalogEntryId, catalogEntries));

        return new CharacterDerivedStats
        {
            AttributeModifiers = attributeModifiers,
            AttributeGradeBonuses = attributeGradeBonuses,
            MaxHp = maxHp,
            Movement = movement,
            Initiative = initiative,
            PassiveDefense = passiveDefense,
            DamageReduction = damageReduction,
            CarryCapacity = carryCapacity,
            CurrentWeight = currentWeight,
            Np = np,
            SkillGradeBonuses = skillGradeBonuses,
            Weapons = weapons
        };
    }

    private static WeaponCombatRow BuildWeaponRow(
        CharacterEquipmentEntry entry,
        EquipmentItemCatalogData eqData,
        string itemName,
        IReadOnlyDictionary<string, int> attributeModifiers,
        IReadOnlyDictionary<string, int> attributeGradeBonuses,
        IReadOnlyDictionary<Guid, int> skillGradeBonuses,
        IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries)
    {
        var skillGrade = 0;
        var attributeGrade = 0;
        var attributeModifier = 0;

        if (entry.LinkedSkillEntryId is { } skillId && catalogEntries.TryGetValue(skillId, out var skillEntry))
        {
            var skillData = JsonSerializer.Deserialize<SkillCatalogData>(skillEntry.DataJson);
            if (skillData is not null)
            {
                var attributeName = NormalizeAttributeName(skillData.RelatedAttribute);
                skillGrade = skillGradeBonuses.GetValueOrDefault(skillId);
                attributeGrade = attributeGradeBonuses.GetValueOrDefault(attributeName);
                attributeModifier = attributeModifiers.GetValueOrDefault(attributeName);
            }
        }

        var dice = eqData.WeaponDiceCategory is not null
            && WeaponDiceByCategory.TryGetValue(eqData.WeaponDiceCategory, out var d)
            ? d
            : "1d6";

        var damage = attributeModifier + skillGrade + eqData.DamageBonus;

        return new WeaponCombatRow
        {
            CatalogEntryId = entry.CatalogEntryId,
            Name = itemName,
            AttackBonus = attributeGrade + skillGrade,
            DamageFormula = $"{dice}{FormatModifier(damage)}"
        };
    }

    private static string FormatModifier(int value) => value switch
    {
        > 0 => $" +{value}",
        < 0 => $" {value}",
        _ => string.Empty
    };

    // Skill.RelatedAttribute values in the catalog are accented GDD names
    // ("Presença", "Percepção"); CharacterAttributes property names drop the accent
    // (C# identifiers can't contain "ç"/"ã") — this bridges the two.
    private static string NormalizeAttributeName(string raw) => raw switch
    {
        "Presença" => "Presenca",
        "Percepção" => "Percepcao",
        _ => raw
    };

    private static Dictionary<string, int> GetAttributeScores(CharacterAttributes attrs) => new()
    {
        ["Corpo"] = attrs.Corpo,
        ["Controle"] = attrs.Controle,
        ["Vigor"] = attrs.Vigor,
        ["Presenca"] = attrs.Presenca,
        ["Intelecto"] = attrs.Intelecto,
        ["Percepcao"] = attrs.Percepcao,
        ["Vontade"] = attrs.Vontade,
        ["Afinidade"] = attrs.Afinidade
    };

    private static int SkillGradeBonus(int points) => points switch
    {
        >= 100 => 4,
        >= 75 => 3,
        >= 50 => 2,
        >= 25 => 1,
        >= 10 => 0,
        _ => -2
    };

    private static EquipmentItemCatalogData? DeserializeEquipment(
        Guid id, IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries) =>
        catalogEntries.TryGetValue(id, out var entry)
            ? JsonSerializer.Deserialize<EquipmentItemCatalogData>(entry.DataJson)
            : null;

    private static int TalentNpWeightFor(Guid id, IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries)
    {
        if (!catalogEntries.TryGetValue(id, out var entry)) return 0;
        var data = JsonSerializer.Deserialize<TalentCatalogData>(entry.DataJson);
        return data is null ? 0 : TalentNpWeight.GetValueOrDefault(data.PowerTier, 0);
    }

    private static int EquipmentNpWeightFor(Guid id, IReadOnlyDictionary<Guid, CatalogEntry> catalogEntries)
    {
        var data = DeserializeEquipment(id, catalogEntries);
        return data is null ? 0 : EquipmentNpWeight.GetValueOrDefault(data.Rarity, 0);
    }
}
```

- [ ] **Step 5: Run the tests again to confirm they all pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterStatsCalculatorTests`
Expected: PASS (all cases, including the 12-row skill-grade threshold theory and the 8-ranking PV theory).

- [ ] **Step 6: Register the calculator in DI**

In `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`, add under "Core services" (alongside `services.AddSingleton<JwtService>();`):

```csharp
        services.AddSingleton<ICharacterStatsCalculator, CharacterStatsCalculator>();
```

Add `using Ruptura.Application.Services;` to that file's usings if not already present via a wildcard.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Application/Interfaces/ICharacterStatsCalculator.cs \
  src/Ruptura.Application/Services/CharacterStatsCalculator.cs \
  src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs \
  tests/Ruptura.UnitTests/Application/CharacterStatsCalculatorTests.cs
git commit -m "feat: add CharacterStatsCalculator with full GDD derived-stat formulas"
```

## Task 5: `CharacterSheetRepository` + `ICatalogEntryRepository` additions (`GetByIdsAsync`, `includeArchived`)

**Files:**
- Create: `src/Ruptura.Application/Interfaces/ICharacterSheetRepository.cs`
- Create: `src/Ruptura.Infrastructure/Repositories/CharacterSheetRepository.cs`
- Modify: `src/Ruptura.Application/Interfaces/ICatalogEntryRepository.cs`
- Modify: `src/Ruptura.Infrastructure/Repositories/CatalogEntryRepository.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`

**Interfaces:**
- Consumes: `CharacterSheet` (Task 1), `BaseRepository<T>` (existing).
- Produces: `ICharacterSheetRepository.GetByCampaignAsync(Guid, CancellationToken)`, `.GetAliveByOwnerAndCampaignAsync(Guid, Guid, CancellationToken)`; `ICatalogEntryRepository.GetByIdsAsync(IEnumerable<Guid>, CancellationToken)`. Consumed by `CharacterSheetService` (Tasks 6-8).

- [ ] **Step 1: Add `ICharacterSheetRepository`**

```csharp
using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICharacterSheetRepository : IRepository<CharacterSheet>
{
    Task<IEnumerable<CharacterSheet>> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);

    Task<CharacterSheet?> GetAliveByOwnerAndCampaignAsync(
        Guid ownerId, Guid campaignId, CancellationToken ct = default);
}
```

- [ ] **Step 2: Implement `CharacterSheetRepository`**

```csharp
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CharacterSheetRepository(AppDbContext db)
    : BaseRepository<CharacterSheet>(db), ICharacterSheetRepository
{
    public async Task<IEnumerable<CharacterSheet>> GetByCampaignAsync(
        Guid campaignId, CancellationToken ct = default) =>
        await Set
            .Where(c => c.CampaignId == campaignId)
            .OrderBy(c => c.CharacterName)
            .ToListAsync(ct);

    public async Task<CharacterSheet?> GetAliveByOwnerAndCampaignAsync(
        Guid ownerId, Guid campaignId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(
            c => c.OwnerId == ownerId && c.CampaignId == campaignId && !c.IsDead && !c.IsRetired, ct);
}
```

- [ ] **Step 3: Add `GetByIdsAsync` and `includeArchived` to `ICatalogEntryRepository`**

```csharp
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICatalogEntryRepository : IRepository<CatalogEntry>
{
    Task<IEnumerable<CatalogEntry>> GetByTypeAsync(
        CatalogEntryType type, Guid campaignId, bool includeArchived, CancellationToken ct = default);

    Task<bool> ExistsAsync(CatalogEntryType type, Guid? campaignId, string name, CancellationToken ct = default);

    Task<IEnumerable<CatalogEntry>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
```

- [ ] **Step 4: Update `CatalogEntryRepository`**

```csharp
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
        bool includeArchived,
        CancellationToken ct = default) =>
        await Set
            .Where(c => c.Type == type && (c.CampaignId == null || c.CampaignId == campaignId))
            .Where(c => includeArchived || !c.IsArchived)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        CatalogEntryType type,
        Guid? campaignId,
        string name,
        CancellationToken ct = default) =>
        await Set.AnyAsync(c => c.Type == type && c.CampaignId == campaignId && c.Name == name, ct);

    public async Task<IEnumerable<CatalogEntry>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0) return [];
        return await Set.Where(c => idList.Contains(c.Id)).ToListAsync(ct);
    }
}
```

This changes `GetByTypeAsync`'s signature (adds `includeArchived`) — every existing caller (`CatalogEntryService.GetByTypeAsync`, and any test that mocks this method) needs updating. That happens in Task 9; this task alone will not compile against `CatalogEntryService` until Task 9 lands, which is expected and fine mid-plan (each task is reviewed on its own diff, but the whole branch only needs to build green at the end — see Task 9's Step 1 for the compile fix).

Actually — to keep every task independently buildable (a hard requirement of this plan's task boundaries), fix the one existing call site now instead of deferring it:

- [ ] **Step 5: Fix the one existing call site so the solution still builds**

In `src/Ruptura.Infrastructure/Services/CatalogEntryService.cs`, `GetByTypeAsync` currently calls:

```csharp
        var entries = await catalogRepo.GetByTypeAsync(parsedType, campaignId, ct);
```

Change it to pass `includeArchived: false` (Task 9 will add a real parameter for this later — for now, preserve exact current behavior, which never showed archived entries because they didn't exist yet):

```csharp
        var entries = await catalogRepo.GetByTypeAsync(parsedType, campaignId, includeArchived: false, ct);
```

- [ ] **Step 6: Build to confirm everything still compiles**

Run: `dotnet build`
Expected: no errors.

- [ ] **Step 7: Register `CharacterSheetRepository` in DI**

In `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`, under "Repositories":

```csharp
        services.AddScoped<ICharacterSheetRepository, CharacterSheetRepository>();
```

- [ ] **Step 8: Run the full unit test suite to confirm nothing broke**

Run: `dotnet test tests/Ruptura.UnitTests`
Expected: all existing tests still PASS (the `CatalogEntryService` behavior is unchanged, just the repo call signature).

- [ ] **Step 9: Commit**

```bash
git add src/Ruptura.Application/Interfaces/ICharacterSheetRepository.cs \
  src/Ruptura.Infrastructure/Repositories/CharacterSheetRepository.cs \
  src/Ruptura.Application/Interfaces/ICatalogEntryRepository.cs \
  src/Ruptura.Infrastructure/Repositories/CatalogEntryRepository.cs \
  src/Ruptura.Infrastructure/Services/CatalogEntryService.cs \
  src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs
git commit -m "feat: add CharacterSheetRepository and catalog batch-fetch/archive-filter support"
```

## Task 6: `CharacterSheetService` — granting flow (`CreateAsync`)

**Files:**
- Modify: `src/Ruptura.Application/Common/ErrorCodes.cs`
- Create: `src/Ruptura.Application/Interfaces/ICharacterSheetService.cs`
- Create: `src/Ruptura.Shared/CharacterSheets/GrantCharacterSheetRequest.cs`
- Create: `src/Ruptura.Shared/CharacterSheets/CharacterSheetResponse.cs`
- Create: `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs`
- Test: `tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs`

**Interfaces:**
- Consumes: `ICharacterSheetRepository`, `ICampaignRepository`, `ICampaignMembershipRepository`, `ICatalogEntryRepository` (Task 5), `ICharacterStatsCalculator` (Task 4).
- Produces: `ICharacterSheetService.CreateAsync(Guid gameMasterId, Guid campaignId, GrantCharacterSheetRequest request, CancellationToken ct = default) -> Result<CharacterSheetResponse>`; the private `MapToResponseAsync`/`CollectReferencedCatalogIds` helpers this task adds to `CharacterSheetService.cs` are reused (unchanged) by Tasks 7 and 8.

- [ ] **Step 1: Add error codes**

In `src/Ruptura.Application/Common/ErrorCodes.cs`, add a new nested class:

```csharp
    public static class CharacterSheet
    {
        public const string NotFound = "CharacterSheet.NotFound";
        public const string PlayerNotMember = "CharacterSheet.PlayerNotMember";
        public const string AlreadyHasAliveCharacter = "CharacterSheet.AlreadyHasAliveCharacter";
        public const string OnlyGameMasterCanChangeStatus = "CharacterSheet.OnlyGameMasterCanChangeStatus";
    }
```

- [ ] **Step 2: Add the request/response DTOs**

```csharp
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.CharacterSheets;

public class GrantCharacterSheetRequest
{
    [Required]
    public Guid PlayerId { get; set; }

    [Required, MinLength(2), MaxLength(100)]
    public string CharacterName { get; set; } = string.Empty;
}
```

```csharp
namespace Ruptura.Shared.CharacterSheets;

public class CharacterSheetResponse
{
    public Guid Id { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid GrantedByGameMasterId { get; set; }
    public bool IsDead { get; set; }
    public bool IsRetired { get; set; }
    public string? PortraitImagePath { get; set; }
    public CharacterSheetData Data { get; set; } = new();
    public CharacterDerivedStats DerivedStats { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

- [ ] **Step 3: Add `ICharacterSheetService` (full interface — Tasks 7-8 fill in the other methods' bodies)**

```csharp
using Ruptura.Application.Common;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Application.Interfaces;

public interface ICharacterSheetService
{
    Task<Result<CharacterSheetResponse>> CreateAsync(
        Guid gameMasterId, Guid campaignId, GrantCharacterSheetRequest request, CancellationToken ct = default);

    Task<Result<CharacterSheetResponse>> GetAsync(
        Guid callerId, Guid sheetId, CancellationToken ct = default);

    Task<Result<IEnumerable<CharacterSheetResponse>>> GetByCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);

    Task<Result<CharacterSheetResponse>> GetMineAsync(
        Guid playerId, Guid campaignId, CancellationToken ct = default);

    Task<Result<CharacterSheetResponse>> UpdateAsync(
        Guid callerId, Guid sheetId, UpdateCharacterSheetRequest request, CancellationToken ct = default);
}
```

(`UpdateCharacterSheetRequest` doesn't exist yet — Task 8 adds it. This interface won't compile until then. Since this task and Task 8 are adjacent and reviewed as a pair in practice, that's acceptable — but to keep this task independently buildable per this plan's own rule, **add a minimal placeholder-free stub now instead of waiting for Task 8**: create `src/Ruptura.Shared/CharacterSheets/UpdateCharacterSheetRequest.cs` in this task with its real, final shape (Task 8 will not need to change it):

```csharp
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.CharacterSheets;

public class UpdateCharacterSheetRequest
{
    [Required, MinLength(2), MaxLength(100)]
    public string CharacterName { get; set; } = string.Empty;

    [Required]
    public string DataJson { get; set; } = "{}";

    public bool IsDead { get; set; }
    public bool IsRetired { get; set; }
    public string? PortraitImagePath { get; set; }
}
```

Task 8 implements `UpdateAsync`'s body; this task only needs the type to exist so `ICharacterSheetService` compiles.)

- [ ] **Step 4: Write the failing unit tests**

```csharp
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Moq;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Services;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.UnitTests.Application;

public class CharacterSheetServiceTests
{
    private readonly Mock<ICharacterSheetRepository> _sheetRepoMock = new();
    private readonly Mock<ICampaignRepository> _campaignRepoMock = new();
    private readonly Mock<ICampaignMembershipRepository> _membershipRepoMock = new();
    private readonly Mock<ICatalogEntryRepository> _catalogRepoMock = new();
    private readonly Mock<ICharacterStatsCalculator> _calculatorMock = new();
    private readonly CharacterSheetService _sut;

    private static readonly Faker Faker = new();

    public CharacterSheetServiceTests()
    {
        _calculatorMock
            .Setup(c => c.Calculate(It.IsAny<CharacterSheetData>(), It.IsAny<IReadOnlyDictionary<Guid, CatalogEntry>>()))
            .Returns(new CharacterDerivedStats());

        _sut = new CharacterSheetService(
            _sheetRepoMock.Object, _campaignRepoMock.Object, _membershipRepoMock.Object,
            _catalogRepoMock.Object, _calculatorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WhenCampaignNotOwnedByCaller_ReturnsNotFound()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.CreateAsync(
            gmId, campaign.Id, new GrantCharacterSheetRequest { PlayerId = Guid.NewGuid(), CharacterName = "X" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    [Fact]
    public async Task CreateAsync_WhenPlayerNotCampaignMember_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        var playerId = Guid.NewGuid();
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaign.Id, playerId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.CreateAsync(
            gmId, campaign.Id, new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "X" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.PlayerNotMember);
    }

    [Fact]
    public async Task CreateAsync_WhenPlayerAlreadyHasAliveCharacterInCampaign_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var playerId = Guid.NewGuid();
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaign.Id, playerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sheetRepoMock.Setup(r => r.GetAliveByOwnerAndCampaignAsync(playerId, campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CharacterSheet { Id = Guid.NewGuid() });

        var result = await _sut.CreateAsync(
            gmId, campaign.Id, new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "X" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.AlreadyHasAliveCharacter);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_PersistsSheetWithEmptyDefaultData()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var playerId = Guid.NewGuid();
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _membershipRepoMock.Setup(r => r.ExistsAsync(campaign.Id, playerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _sheetRepoMock.Setup(r => r.GetAliveByOwnerAndCampaignAsync(playerId, campaign.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CharacterSheet?)null);
        _sheetRepoMock.Setup(r => r.AddAsync(It.IsAny<CharacterSheet>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CreateAsync(
            gmId, campaign.Id, new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });

        result.IsSuccess.Should().BeTrue();
        result.Value!.CharacterName.Should().Be("Sir Aldric");
        result.Value.OwnerId.Should().Be(playerId);
        result.Value.CampaignId.Should().Be(campaign.Id);
        result.Value.GrantedByGameMasterId.Should().Be(gmId);
        _sheetRepoMock.Verify(r => r.AddAsync(
            It.Is<CharacterSheet>(s => s.OwnerId == playerId && s.CampaignId == campaign.Id && !s.IsDead && !s.IsRetired),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 5: Run the tests to confirm they fail (implementation doesn't exist)**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterSheetServiceTests`
Expected: build error — `CharacterSheetService` doesn't exist.

- [ ] **Step 6: Implement `CharacterSheetService.CreateAsync` and its shared helpers**

```csharp
using System.Text.Json;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Infrastructure.Services;

public class CharacterSheetService(
    ICharacterSheetRepository sheetRepo,
    ICampaignRepository campaignRepo,
    ICampaignMembershipRepository membershipRepo,
    ICatalogEntryRepository catalogRepo,
    ICharacterStatsCalculator calculator) : ICharacterSheetService
{
    public async Task<Result<CharacterSheetResponse>> CreateAsync(
        Guid gameMasterId,
        Guid campaignId,
        GrantCharacterSheetRequest request,
        CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        if (!await membershipRepo.ExistsAsync(campaignId, request.PlayerId, ct))
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.PlayerNotMember);

        var existingAlive = await sheetRepo.GetAliveByOwnerAndCampaignAsync(request.PlayerId, campaignId, ct);
        if (existingAlive is not null)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.AlreadyHasAliveCharacter);

        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(),
            CharacterName = request.CharacterName,
            OwnerId = request.PlayerId,
            CampaignId = campaignId,
            GrantedByGameMasterId = gameMasterId,
            IsDead = false,
            IsRetired = false,
            DataJson = JsonSerializer.Serialize(new CharacterSheetData()),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await sheetRepo.AddAsync(sheet, ct);
        await sheetRepo.SaveChangesAsync(ct);

        return Result.Success(await MapToResponseAsync(sheet, ct));
    }

    // ── Private helpers (shared with Tasks 7-8) ─────────────────────────────

    private async Task<CharacterSheetResponse> MapToResponseAsync(CharacterSheet sheet, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<CharacterSheetData>(sheet.DataJson) ?? new CharacterSheetData();
        var referencedIds = CollectReferencedCatalogIds(data);
        var catalogEntries = referencedIds.Count == 0
            ? new Dictionary<Guid, CatalogEntry>()
            : (await catalogRepo.GetByIdsAsync(referencedIds, ct)).ToDictionary(e => e.Id);
        var derived = calculator.Calculate(data, catalogEntries);

        return new CharacterSheetResponse
        {
            Id = sheet.Id,
            CharacterName = sheet.CharacterName,
            OwnerId = sheet.OwnerId,
            CampaignId = sheet.CampaignId,
            GrantedByGameMasterId = sheet.GrantedByGameMasterId,
            IsDead = sheet.IsDead,
            IsRetired = sheet.IsRetired,
            PortraitImagePath = sheet.PortraitImagePath,
            Data = data,
            DerivedStats = derived,
            CreatedAt = sheet.CreatedAt,
            UpdatedAt = sheet.UpdatedAt
        };
    }

    private static List<Guid> CollectReferencedCatalogIds(CharacterSheetData data)
    {
        var ids = new List<Guid>();
        if (data.Identity.OriginId is { } origin) ids.Add(origin);
        if (data.Identity.BackgroundId is { } background) ids.Add(background);
        if (data.Identity.LineageId is { } lineage) ids.Add(lineage);
        ids.AddRange(data.Identity.AptitudeIds);
        if (data.Identity.InitialTalentId is { } initialTalent) ids.Add(initialTalent);
        ids.AddRange(data.Skills.Select(s => s.CatalogEntryId));
        ids.AddRange(data.Talents.Select(t => t.CatalogEntryId));
        ids.AddRange(data.Spells.Select(s => s.CatalogEntryId));
        ids.AddRange(data.Techniques.Select(t => t.CatalogEntryId));
        ids.AddRange(data.Equipment.Select(e => e.CatalogEntryId));
        ids.AddRange(data.Equipment.Where(e => e.LinkedSkillEntryId.HasValue).Select(e => e.LinkedSkillEntryId!.Value));
        return ids.Distinct().ToList();
    }
}
```

This will not fully implement `ICharacterSheetService` yet (`GetAsync`/`GetByCampaignAsync`/`GetMineAsync`/`UpdateAsync` are still missing bodies) — add temporary throwing stubs so the class compiles, which Tasks 7 and 8 will replace:

```csharp
    public Task<Result<CharacterSheetResponse>> GetAsync(Guid callerId, Guid sheetId, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 7.");

    public Task<Result<IEnumerable<CharacterSheetResponse>>> GetByCampaignAsync(Guid gameMasterId, Guid campaignId, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 7.");

    public Task<Result<CharacterSheetResponse>> GetMineAsync(Guid playerId, Guid campaignId, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 7.");

    public Task<Result<CharacterSheetResponse>> UpdateAsync(Guid callerId, Guid sheetId, UpdateCharacterSheetRequest request, CancellationToken ct = default) =>
        throw new NotImplementedException("Implemented in Task 8.");
```

- [ ] **Step 7: Run the tests to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterSheetServiceTests`
Expected: PASS (4/4).

- [ ] **Step 8: Register in DI**

In `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`, under "Application services":

```csharp
        services.AddScoped<ICharacterSheetService, CharacterSheetService>();
```

- [ ] **Step 9: Build the whole solution to confirm it's still green**

Run: `dotnet build`
Expected: no errors (the `NotImplementedException` stubs make the class satisfy the interface; nothing calls those methods yet since no controller exists).

- [ ] **Step 10: Commit**

```bash
git add src/Ruptura.Application/Common/ErrorCodes.cs src/Ruptura.Application/Interfaces/ICharacterSheetService.cs \
  src/Ruptura.Shared/CharacterSheets/GrantCharacterSheetRequest.cs \
  src/Ruptura.Shared/CharacterSheets/CharacterSheetResponse.cs \
  src/Ruptura.Shared/CharacterSheets/UpdateCharacterSheetRequest.cs \
  src/Ruptura.Infrastructure/Services/CharacterSheetService.cs \
  src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs \
  tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs
git commit -m "feat: add CharacterSheetService granting flow (CreateAsync)"
```

## Task 7: `CharacterSheetService` — reads (`GetAsync`, `GetByCampaignAsync`, `GetMineAsync`)

**Files:**
- Modify: `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs`
- Test: `tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs`

**Interfaces:**
- Consumes: same constructor dependencies as Task 6 (unchanged), `MapToResponseAsync`/`CollectReferencedCatalogIds` (Task 6, unchanged).
- Produces: replaces the three `GetAsync`/`GetByCampaignAsync`/`GetMineAsync` `NotImplementedException` stubs from Task 6 with real bodies. Consumed by `CharacterSheetController` (Task 11).

- [ ] **Step 1: Write the failing unit tests**

Add to `tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs`:

```csharp
    // ── GetAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_AsOwner_ReturnsSheet()
    {
        var ownerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(), OwnerId = ownerId, CampaignId = campaign.Id,
            DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.GetAsync(ownerId, sheet.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(sheet.Id);
    }

    [Fact]
    public async Task GetAsync_AsCampaignGameMaster_ReturnsSheet()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id,
            DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.GetAsync(gmId, sheet.Id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetAsync_AsUnrelatedCaller_ReturnsNotFound()
    {
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id,
            DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.GetAsync(Guid.NewGuid(), sheet.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    [Fact]
    public async Task GetAsync_WhenSheetDoesNotExist_ReturnsNotFound()
    {
        _sheetRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CharacterSheet?)null);

        var result = await _sut.GetAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    // ── GetByCampaignAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByCampaignAsync_AsOwningGameMaster_ReturnsAllSheetsInCampaign()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheets = new List<CharacterSheet>
        {
            new() { Id = Guid.NewGuid(), CampaignId = campaign.Id, DataJson = JsonSerializer.Serialize(new CharacterSheetData()) }
        };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _sheetRepoMock.Setup(r => r.GetByCampaignAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheets);

        var result = await _sut.GetByCampaignAsync(gmId, campaign.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByCampaignAsync_WhenCallerIsNotTheGameMaster_ReturnsNotFound()
    {
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.GetByCampaignAsync(Guid.NewGuid(), campaign.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    // ── GetMineAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMineAsync_WhenPlayerHasAnAliveCharacterInCampaign_ReturnsIt()
    {
        var playerId = Guid.NewGuid();
        var campaignId = Guid.NewGuid();
        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(), OwnerId = playerId, CampaignId = campaignId,
            DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        };
        _sheetRepoMock.Setup(r => r.GetAliveByOwnerAndCampaignAsync(playerId, campaignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sheet);

        var result = await _sut.GetMineAsync(playerId, campaignId);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(sheet.Id);
    }

    [Fact]
    public async Task GetMineAsync_WhenNoCharacterGrantedYet_ReturnsNotFound()
    {
        _sheetRepoMock.Setup(r => r.GetAliveByOwnerAndCampaignAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CharacterSheet?)null);

        var result = await _sut.GetMineAsync(Guid.NewGuid(), Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }
```

- [ ] **Step 2: Run the tests to confirm they fail**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterSheetServiceTests`
Expected: the new tests throw `NotImplementedException` (from Task 6's stubs) and FAIL.

- [ ] **Step 3: Replace the three stubs with real implementations**

In `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs`, replace the `GetAsync`/`GetByCampaignAsync`/`GetMineAsync` stub bodies:

```csharp
    public async Task<Result<CharacterSheetResponse>> GetAsync(
        Guid callerId, Guid sheetId, CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetByIdAsync(sheetId, ct);
        if (sheet is null)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(sheet.CampaignId, ct);
        var authorized = sheet.OwnerId == callerId || campaign?.GameMasterId == callerId;
        if (!authorized)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        return Result.Success(await MapToResponseAsync(sheet, ct));
    }

    public async Task<Result<IEnumerable<CharacterSheetResponse>>> GetByCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<IEnumerable<CharacterSheetResponse>>(ErrorCodes.CharacterSheet.NotFound);

        var sheets = await sheetRepo.GetByCampaignAsync(campaignId, ct);
        var responses = new List<CharacterSheetResponse>();
        foreach (var sheet in sheets)
            responses.Add(await MapToResponseAsync(sheet, ct));

        return Result.Success(responses.AsEnumerable());
    }

    public async Task<Result<CharacterSheetResponse>> GetMineAsync(
        Guid playerId, Guid campaignId, CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetAliveByOwnerAndCampaignAsync(playerId, campaignId, ct);
        if (sheet is null)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        return Result.Success(await MapToResponseAsync(sheet, ct));
    }
```

(Leave the `UpdateAsync` `NotImplementedException` stub untouched — Task 8 replaces it.)

- [ ] **Step 4: Run the tests to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterSheetServiceTests`
Expected: PASS (all `GetAsync`/`GetByCampaignAsync`/`GetMineAsync` cases plus the 4 from Task 6).

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Infrastructure/Services/CharacterSheetService.cs \
  tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs
git commit -m "feat: add CharacterSheetService read paths (Get/GetByCampaign/GetMine)"
```

## Task 8: `CharacterSheetService` — `UpdateAsync` (permission matrix, `IsDead`/`IsRetired` guard)

**Files:**
- Modify: `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs`
- Test: `tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs`

**Interfaces:**
- Consumes: same as Tasks 6-7 (unchanged); `Microsoft.EntityFrameworkCore.DbUpdateException` (framework type, for the concurrency safety net).
- Produces: replaces `UpdateAsync`'s `NotImplementedException` stub. Consumed by `CharacterSheetController` (Task 11).

**Design note carried from the design spec §6**: "if the caller isn't the Campaign's GM and the payload tries to change `IsDead`/`IsRetired`, the request fails — the rest of the payload proceeds normally [when it doesn't]." Since `Result<T>` is all-or-nothing (no partial-apply), this plan implements it as: **a non-GM payload that actually changes either flag's value is rejected outright** (`Result.Failure`, nothing saved); a non-GM payload that leaves both flags at their current value updates every other field normally. A GM caller may always change both flags.

- [ ] **Step 1: Write the failing unit tests**

Add to `tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs`:

```csharp
    // ── UpdateAsync ──────────────────────────────────────────────────────────

    private static CharacterSheet BuildAliveSheet(Guid ownerId, Guid campaignId) => new()
    {
        Id = Guid.NewGuid(), OwnerId = ownerId, CampaignId = campaignId, CharacterName = "Old Name",
        IsDead = false, IsRetired = false, DataJson = JsonSerializer.Serialize(new CharacterSheetData())
    };

    [Fact]
    public async Task UpdateAsync_AsOwner_UpdatesGeneralFieldsWithoutTouchingStatus()
    {
        var ownerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = BuildAliveSheet(ownerId, campaign.Id);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(ownerId, sheet.Id, new UpdateCharacterSheetRequest
        {
            CharacterName = "New Name", DataJson = JsonSerializer.Serialize(new CharacterSheetData()),
            IsDead = false, IsRetired = false
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.CharacterName.Should().Be("New Name");
        sheet.IsDead.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_AsOwnerAttemptingToMarkDead_ReturnsFailureAndDoesNotSave()
    {
        var ownerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = BuildAliveSheet(ownerId, campaign.Id);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.UpdateAsync(ownerId, sheet.Id, new UpdateCharacterSheetRequest
        {
            CharacterName = "New Name", DataJson = JsonSerializer.Serialize(new CharacterSheetData()),
            IsDead = true, IsRetired = false
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.OnlyGameMasterCanChangeStatus);
        sheet.CharacterName.Should().Be("Old Name");
        _sheetRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_AsGameMaster_CanMarkCharacterDead()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheet = BuildAliveSheet(Guid.NewGuid(), campaign.Id);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(gmId, sheet.Id, new UpdateCharacterSheetRequest
        {
            CharacterName = "Old Name", DataJson = JsonSerializer.Serialize(new CharacterSheetData()),
            IsDead = true, IsRetired = false
        });

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsDead.Should().BeTrue();
        sheet.IsDead.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_AsUnrelatedCaller_ReturnsNotFound()
    {
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = Guid.NewGuid() };
        var sheet = BuildAliveSheet(Guid.NewGuid(), campaign.Id);
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), sheet.Id, new UpdateCharacterSheetRequest
        {
            CharacterName = "X", DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.NotFound);
    }

    [Fact]
    public async Task UpdateAsync_WhenSaveViolatesUniqueAliveIndex_ReturnsAlreadyHasAliveCharacter()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var sheet = new CharacterSheet
        {
            Id = Guid.NewGuid(), OwnerId = Guid.NewGuid(), CampaignId = campaign.Id, CharacterName = "Resurrected",
            IsDead = true, IsRetired = false, DataJson = JsonSerializer.Serialize(new CharacterSheetData())
        };
        _sheetRepoMock.Setup(r => r.GetByIdAsync(sheet.Id, It.IsAny<CancellationToken>())).ReturnsAsync(sheet);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _sheetRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateException("unique violation"));

        // GM tries to un-kill this character back to alive, while another alive sheet
        // for the same owner+campaign already exists (simulated by the DB throwing).
        var result = await _sut.UpdateAsync(gmId, sheet.Id, new UpdateCharacterSheetRequest
        {
            CharacterName = "Resurrected", DataJson = JsonSerializer.Serialize(new CharacterSheetData()),
            IsDead = false, IsRetired = false
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.CharacterSheet.AlreadyHasAliveCharacter);
    }
```

- [ ] **Step 2: Run the tests to confirm they fail**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterSheetServiceTests`
Expected: the new tests throw `NotImplementedException` and FAIL.

- [ ] **Step 3: Implement `UpdateAsync`**

Replace the `UpdateAsync` stub in `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs`. Add `using Microsoft.EntityFrameworkCore;` to the file's usings.

```csharp
    public async Task<Result<CharacterSheetResponse>> UpdateAsync(
        Guid callerId, Guid sheetId, UpdateCharacterSheetRequest request, CancellationToken ct = default)
    {
        var sheet = await sheetRepo.GetByIdAsync(sheetId, ct);
        if (sheet is null)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(sheet.CampaignId, ct);
        var isOwner = sheet.OwnerId == callerId;
        var isGameMaster = campaign?.GameMasterId == callerId;
        if (!isOwner && !isGameMaster)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.NotFound);

        var statusChanged = request.IsDead != sheet.IsDead || request.IsRetired != sheet.IsRetired;
        if (statusChanged && !isGameMaster)
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.OnlyGameMasterCanChangeStatus);

        sheet.CharacterName = request.CharacterName;
        sheet.DataJson = request.DataJson;
        sheet.PortraitImagePath = request.PortraitImagePath;
        if (isGameMaster)
        {
            sheet.IsDead = request.IsDead;
            sheet.IsRetired = request.IsRetired;
        }
        sheet.UpdatedAt = DateTime.UtcNow;

        try
        {
            sheetRepo.Update(sheet);
            await sheetRepo.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Only the alive-per-owner-per-campaign partial unique index is on this table,
            // so any DbUpdateException on this save path means that race — see design spec §4.1.
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.AlreadyHasAliveCharacter);
        }

        return Result.Success(await MapToResponseAsync(sheet, ct));
    }
```

- [ ] **Step 4: Run the tests to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterSheetServiceTests`
Expected: PASS (all cases across Tasks 6-8).

- [ ] **Step 5: Run the full unit test suite**

Run: `dotnet test tests/Ruptura.UnitTests`
Expected: all PASS, no regressions.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Infrastructure/Services/CharacterSheetService.cs \
  tests/Ruptura.UnitTests/Application/CharacterSheetServiceTests.cs
git commit -m "feat: add CharacterSheetService.UpdateAsync with status-field permission guard"
```

## Task 9: `CatalogEntryService` soft-delete (`IsArchived`) + `includeArchived` read parameter

**Files:**
- Modify: `src/Ruptura.Application/Common/ErrorCodes.cs`
- Modify: `src/Ruptura.Application/Interfaces/ICatalogEntryService.cs`
- Modify: `src/Ruptura.Infrastructure/Services/CatalogEntryService.cs`
- Modify: `src/Ruptura.Shared/Catalog/CatalogEntryResponse.cs`
- Test: `tests/Ruptura.UnitTests/Application/CatalogEntryServiceTests.cs`

**Interfaces:**
- Consumes: `ICatalogEntryRepository.GetByTypeAsync(..., bool includeArchived, ...)` (Task 5).
- Produces: `ICatalogEntryService.GetByTypeAsync` gains an `includeArchived` parameter; `DeleteAsync` now archives instead of removing. Consumed by `CatalogController` (Task 11) and `GmCatalog.razor` (Task 18).

- [ ] **Step 1: Add the new error code**

In `src/Ruptura.Application/Common/ErrorCodes.cs`, add to the existing `Catalog` nested class:

```csharp
        public const string AlreadyArchived = "Catalog.AlreadyArchived";
```

- [ ] **Step 2: Add `IsArchived` to `CatalogEntryResponse`**

In `src/Ruptura.Shared/Catalog/CatalogEntryResponse.cs`, add:

```csharp
    public bool IsArchived { get; set; }
```

- [ ] **Step 3: Update `ICatalogEntryService.GetByTypeAsync`'s signature**

```csharp
    Task<Result<IEnumerable<CatalogEntryResponse>>> GetByTypeAsync(
        Guid callerId, string type, Guid campaignId, bool includeArchived, CancellationToken ct = default);
```

- [ ] **Step 4: Write the failing unit tests**

Add to `tests/Ruptura.UnitTests/Application/CatalogEntryServiceTests.cs` (check the existing file first for the exact mock field names — likely `_catalogRepoMock`, `_campaignRepoMock`, `_membershipRepoMock` — and match them):

```csharp
    [Fact]
    public async Task DeleteAsync_ArchivesTheEntryInsteadOfRemovingIt()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var entry = new CatalogEntry { Id = Guid.NewGuid(), CampaignId = campaign.Id, Type = CatalogEntryType.Talent, Name = "Homebrew Talent" };
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(gmId, entry.Id);

        result.IsSuccess.Should().BeTrue();
        entry.IsArchived.Should().BeTrue();
        _catalogRepoMock.Verify(r => r.Remove(It.IsAny<CatalogEntry>()), Times.Never);
        _catalogRepoMock.Verify(r => r.Update(It.Is<CatalogEntry>(e => e.IsArchived)), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenAlreadyArchived_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var entry = new CatalogEntry { Id = Guid.NewGuid(), CampaignId = campaign.Id, Type = CatalogEntryType.Talent, Name = "X", IsArchived = true };
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.DeleteAsync(gmId, entry.Id);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.AlreadyArchived);
    }

    [Fact]
    public async Task UpdateAsync_WhenEntryIsArchived_ReturnsFailure()
    {
        var gmId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = gmId };
        var entry = new CatalogEntry { Id = Guid.NewGuid(), CampaignId = campaign.Id, Type = CatalogEntryType.Talent, Name = "X", IsArchived = true };
        _catalogRepoMock.Setup(r => r.GetByIdAsync(entry.Id, It.IsAny<CancellationToken>())).ReturnsAsync(entry);
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);

        var result = await _sut.UpdateAsync(gmId, entry.Id, new UpdateCatalogEntryRequest { Name = "Y", DataJson = "{}" });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCodes.Catalog.AlreadyArchived);
    }

    [Fact]
    public async Task GetByTypeAsync_WithIncludeArchivedFalse_PassesFalseToRepository()
    {
        var callerId = Guid.NewGuid();
        var campaign = new Campaign { Id = Guid.NewGuid(), GameMasterId = callerId };
        _campaignRepoMock.Setup(r => r.GetByIdAsync(campaign.Id, It.IsAny<CancellationToken>())).ReturnsAsync(campaign);
        _catalogRepoMock.Setup(r => r.GetByTypeAsync(CatalogEntryType.Talent, campaign.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.GetByTypeAsync(callerId, "Talent", campaign.Id, includeArchived: false);

        result.IsSuccess.Should().BeTrue();
        _catalogRepoMock.Verify(r => r.GetByTypeAsync(CatalogEntryType.Talent, campaign.Id, false, It.IsAny<CancellationToken>()), Times.Once);
    }
```

If the existing test file's constructor doesn't already build `_sut` with the exact same mocks, adapt these to match — don't restructure the existing test class's setup, add to it.

- [ ] **Step 5: Run the tests to confirm they fail**

Run: `dotnet test tests/Ruptura.UnitTests --filter CatalogEntryServiceTests`
Expected: build errors (signature mismatches) and/or behavioral failures (delete still removes).

- [ ] **Step 6: Update `CatalogEntryService`**

In `src/Ruptura.Infrastructure/Services/CatalogEntryService.cs`:

Change `GetByTypeAsync`'s signature and repo call:

```csharp
    public async Task<Result<IEnumerable<CatalogEntryResponse>>> GetByTypeAsync(
        Guid callerId,
        string type,
        Guid campaignId,
        bool includeArchived,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<CatalogEntryType>(type, out var parsedType) || !Enum.IsDefined(parsedType))
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.InvalidType);

        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.NotFound);

        var isMember = campaign.GameMasterId == callerId
            || await membershipRepo.ExistsAsync(campaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<IEnumerable<CatalogEntryResponse>>(ErrorCodes.Catalog.NotFound);

        var entries = await catalogRepo.GetByTypeAsync(parsedType, campaignId, includeArchived, ct);
        return Result.Success(entries.Select(MapToResponse));
    }
```

Add the archived-check to `UpdateAsync`, right after the existing global-entry check (`if (entry.CampaignId is null) return ...CannotModifyGlobalEntry;`):

```csharp
        if (entry.IsArchived)
            return Result.Failure<CatalogEntryResponse>(ErrorCodes.Catalog.AlreadyArchived);
```

Add the same check to `DeleteAsync`, right after its own global-entry check, and change the delete body from `Remove` to archiving:

```csharp
        if (entry.IsArchived)
            return Result.Failure(ErrorCodes.Catalog.AlreadyArchived);
```

```csharp
        entry.IsArchived = true;
        entry.UpdatedAt = DateTime.UtcNow;
        catalogRepo.Update(entry);
        await catalogRepo.SaveChangesAsync(ct);

        return Result.Success();
```

(This replaces the previous `catalogRepo.Remove(entry); await catalogRepo.SaveChangesAsync(ct); return Result.Success();` body.)

Add `IsArchived = c.IsArchived,` to the private `MapToResponse` helper's object initializer.

- [ ] **Step 7: Run the tests to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter CatalogEntryServiceTests`
Expected: PASS, including all pre-existing tests in this file (none of their assertions about non-archived behavior should have changed).

- [ ] **Step 8: Build the whole solution**

Run: `dotnet build`
Expected: the `CatalogController.GetByType` call site (Task 11 will properly wire this — for now, fix the compile error by passing `includeArchived: false` at the one existing call site in `CatalogController.cs`):

```csharp
        var result = await catalogService.GetByTypeAsync(callerId, type, campaignId, includeArchived: false, ct);
```

Run `dotnet build` again to confirm it's clean.

- [ ] **Step 9: Commit**

```bash
git add src/Ruptura.Application/Common/ErrorCodes.cs src/Ruptura.Application/Interfaces/ICatalogEntryService.cs \
  src/Ruptura.Infrastructure/Services/CatalogEntryService.cs src/Ruptura.Shared/Catalog/CatalogEntryResponse.cs \
  src/Ruptura.API/Controllers/CatalogController.cs \
  tests/Ruptura.UnitTests/Application/CatalogEntryServiceTests.cs
git commit -m "feat: soft-delete CatalogEntry via IsArchived instead of hard delete"
```

## Task 10: FluentValidation validators for the CharacterSheet requests

**Files:**
- Create: `src/Ruptura.Application/Validators/CharacterSheets/GrantCharacterSheetRequestValidator.cs`
- Create: `src/Ruptura.Application/Validators/CharacterSheets/UpdateCharacterSheetRequestValidator.cs`
- Modify: `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`
- Test: `tests/Ruptura.UnitTests/Application/CharacterSheetValidatorsTests.cs`

**Interfaces:**
- Consumes: `GrantCharacterSheetRequest`, `UpdateCharacterSheetRequest` (Task 6).
- Produces: `IValidator<GrantCharacterSheetRequest>`, `IValidator<UpdateCharacterSheetRequest>` — consumed by `CharacterSheetController` (Task 11).

- [ ] **Step 1: Write the failing tests**

```csharp
using FluentAssertions;
using Ruptura.Application.Validators.CharacterSheets;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.UnitTests.Application;

public class CharacterSheetValidatorsTests
{
    private readonly GrantCharacterSheetRequestValidator _grantValidator = new();
    private readonly UpdateCharacterSheetRequestValidator _updateValidator = new();

    [Fact]
    public void GrantValidator_WithEmptyPlayerId_Fails()
    {
        var result = _grantValidator.Validate(new GrantCharacterSheetRequest { PlayerId = Guid.Empty, CharacterName = "Aldric" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GrantValidator_WithTooShortName_Fails()
    {
        var result = _grantValidator.Validate(new GrantCharacterSheetRequest { PlayerId = Guid.NewGuid(), CharacterName = "A" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GrantValidator_WithValidData_Succeeds()
    {
        var result = _grantValidator.Validate(new GrantCharacterSheetRequest { PlayerId = Guid.NewGuid(), CharacterName = "Aldric" });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateValidator_WithInvalidJson_Fails()
    {
        var result = _updateValidator.Validate(new UpdateCharacterSheetRequest { CharacterName = "Aldric", DataJson = "not json" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateValidator_WithValidData_Succeeds()
    {
        var result = _updateValidator.Validate(new UpdateCharacterSheetRequest { CharacterName = "Aldric", DataJson = "{}" });
        result.IsValid.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run to confirm failure (types don't exist)**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterSheetValidatorsTests`
Expected: build error.

- [ ] **Step 3: Implement the validators**

```csharp
using FluentValidation;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Application.Validators.CharacterSheets;

public class GrantCharacterSheetRequestValidator : AbstractValidator<GrantCharacterSheetRequest>
{
    public GrantCharacterSheetRequestValidator()
    {
        RuleFor(x => x.PlayerId).NotEmpty();
        RuleFor(x => x.CharacterName).NotEmpty().MinimumLength(2).MaximumLength(100);
    }
}
```

```csharp
using System.Text.Json;
using FluentValidation;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Application.Validators.CharacterSheets;

public class UpdateCharacterSheetRequestValidator : AbstractValidator<UpdateCharacterSheetRequest>
{
    public UpdateCharacterSheetRequestValidator()
    {
        RuleFor(x => x.CharacterName).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.DataJson).NotEmpty().Must(BeValidJson).WithMessage("DataJson must be valid JSON.");
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

- [ ] **Step 4: Run to confirm they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter CharacterSheetValidatorsTests`
Expected: PASS (5/5).

- [ ] **Step 5: Register in DI**

In `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`, under "Validators":

```csharp
        services.AddScoped<IValidator<GrantCharacterSheetRequest>, GrantCharacterSheetRequestValidator>();
        services.AddScoped<IValidator<UpdateCharacterSheetRequest>, UpdateCharacterSheetRequestValidator>();
```

Add `using Ruptura.Application.Validators.CharacterSheets;` and `using Ruptura.Shared.CharacterSheets;` to that file's usings.

- [ ] **Step 6: Build and commit**

Run: `dotnet build` — expect no errors.

```bash
git add src/Ruptura.Application/Validators/CharacterSheets/ src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs \
  tests/Ruptura.UnitTests/Application/CharacterSheetValidatorsTests.cs
git commit -m "feat: add FluentValidation validators for CharacterSheet requests"
```

## Task 11: `CharacterSheetController` + localization + integration tests

**Files:**
- Create: `src/Ruptura.API/Controllers/CharacterSheetController.cs`
- Modify: `src/Ruptura.API/Resources/SharedResources.resx`
- Modify: `src/Ruptura.API/Resources/SharedResources.pt-BR.resx`
- Test: `tests/Ruptura.IntegrationTests/Controllers/CharacterSheetControllerTests.cs`

**Interfaces:**
- Consumes: `ICharacterSheetService` (Tasks 6-8), `IValidator<GrantCharacterSheetRequest>`/`IValidator<UpdateCharacterSheetRequest>` (Task 10).
- Produces: the 5 HTTP endpoints below. Consumed by `CharacterSheetClientService` (Task 13).

Endpoints (extends the design spec §6 endpoint list — the spec only listed `GET/PUT /api/character-sheets/{id}`; the other three are additive, needed so the player UI can discover and the GM UI can list/grant sheets, matching how `CampaignController`'s existing `/members` sub-resource pattern already works):

```
POST /api/campaigns/{campaignId:guid}/character-sheets          (GM grants)
GET  /api/campaigns/{campaignId:guid}/character-sheets          (GM lists all sheets in the campaign)
GET  /api/campaigns/{campaignId:guid}/character-sheets/mine     (player's own alive sheet in that campaign)
GET  /api/character-sheets/{id:guid}                             (owner or campaign's GM)
PUT  /api/character-sheets/{id:guid}                             (owner or campaign's GM; IsDead/IsRetired GM-only)
```

- [ ] **Step 1: Add the resx keys (both `en` and `pt-BR`)**

In `src/Ruptura.API/Resources/SharedResources.resx`, add (matching the existing `<data name="Catalog...">` block style):

```xml
  <data name="CharacterSheet.NotFound"><value>Character sheet not found.</value></data>
  <data name="CharacterSheet.PlayerNotMember"><value>This player is not a member of the campaign.</value></data>
  <data name="CharacterSheet.AlreadyHasAliveCharacter"><value>This player already has a living character in this campaign.</value></data>
  <data name="CharacterSheet.OnlyGameMasterCanChangeStatus"><value>Only the Game Master can mark a character as dead or retired.</value></data>
  <data name="CharacterSheet.Granted"><value>Character sheet granted successfully.</value></data>
  <data name="CharacterSheet.Updated"><value>Character sheet updated successfully.</value></data>
```

In `src/Ruptura.API/Resources/SharedResources.pt-BR.resx`, add the matching pt-BR entries:

```xml
  <data name="CharacterSheet.NotFound"><value>Ficha de personagem não encontrada.</value></data>
  <data name="CharacterSheet.PlayerNotMember"><value>Este jogador não é membro da campanha.</value></data>
  <data name="CharacterSheet.AlreadyHasAliveCharacter"><value>Este jogador já possui um personagem vivo nesta campanha.</value></data>
  <data name="CharacterSheet.OnlyGameMasterCanChangeStatus"><value>Somente o Mestre pode marcar um personagem como morto ou aposentado.</value></data>
  <data name="CharacterSheet.Granted"><value>Ficha de personagem concedida com sucesso.</value></data>
  <data name="CharacterSheet.Updated"><value>Ficha de personagem atualizada com sucesso.</value></data>
```

- [ ] **Step 2: Implement the controller**

```csharp
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class CharacterSheetController(
    ICharacterSheetService characterSheetService,
    IStringLocalizer<SharedResources> localizer,
    IValidator<GrantCharacterSheetRequest> grantValidator,
    IValidator<UpdateCharacterSheetRequest> updateValidator) : ControllerBase
{
    [HttpPost("campaigns/{campaignId:guid}/character-sheets")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<CharacterSheetResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Grant(
        Guid campaignId, [FromBody] GrantCharacterSheetRequest request, CancellationToken ct)
    {
        var validation = await grantValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.CreateAsync(gameMasterId, campaignId, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.CharacterSheet.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CharacterSheetResponse>.Ok(result.Value!, localizer["CharacterSheet.Granted"]));
    }

    [HttpGet("campaigns/{campaignId:guid}/character-sheets")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CharacterSheetResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCampaign(Guid campaignId, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.GetByCampaignAsync(gameMasterId, campaignId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<IEnumerable<CharacterSheetResponse>>.Ok(result.Value!));
    }

    [HttpGet("campaigns/{campaignId:guid}/character-sheets/mine")]
    [ProducesResponseType(typeof(ApiResponse<CharacterSheetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMine(Guid campaignId, CancellationToken ct)
    {
        var playerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.GetMineAsync(playerId, campaignId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<CharacterSheetResponse>.Ok(result.Value!));
    }

    [HttpGet("character-sheets/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CharacterSheetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.GetAsync(callerId, id, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<CharacterSheetResponse>.Ok(result.Value!));
    }

    [HttpPut("character-sheets/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CharacterSheetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCharacterSheetRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.UpdateAsync(callerId, id, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.CharacterSheet.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<CharacterSheetResponse>.Ok(result.Value!, localizer["CharacterSheet.Updated"]));
    }
}
```

- [ ] **Step 3: Write the integration tests**

```csharp
using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class CharacterSheetControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, CampaignResponse Campaign, Guid PlayerId, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Sheet Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        return (client, campaign, player.User.Id, player.AccessToken, gm.AccessToken);
    }

    [Fact]
    public async Task Grant_AsCampaignGameMaster_Returns201WithEmptyDefaultData()
    {
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        body.CharacterName.Should().Be("Sir Aldric");
        body.DerivedStats.MaxHp.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Grant_ASecondAliveCharacterForTheSamePlayer_Returns400()
    {
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "First" });

        var second = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Second" });

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMine_AsThePlayerWithAGrantedSheet_ReturnsIt()
    {
        var (client, campaign, playerId, playerToken, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/character-sheets/mine");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        body.CharacterName.Should().Be("Sir Aldric");
    }

    [Fact]
    public async Task Update_AsPlayerAttemptingToMarkOwnCharacterDead_Returns400()
    {
        var (client, campaign, playerId, playerToken, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        AuthHelper.SetBearerToken(client, playerToken);
        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName, DataJson = "{}", IsDead = true
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_AsCampaignGameMaster_CanMarkCharacterDead()
    {
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName, DataJson = "{}", IsDead = true
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        body.IsDead.Should().BeTrue();
    }

    [Fact]
    public async Task Get_AsUnrelatedPlayer_Returns404()
    {
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        var outsider = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var response = await client.GetAsync($"api/character-sheets/{sheet.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

- [ ] **Step 4: Run the tests**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter CharacterSheetControllerTests`
Expected: PASS (6/6). Re-run once if a failure looks like the documented Serilog flake before treating it as real.

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.API/Controllers/CharacterSheetController.cs \
  src/Ruptura.API/Resources/SharedResources.resx src/Ruptura.API/Resources/SharedResources.pt-BR.resx \
  tests/Ruptura.IntegrationTests/Controllers/CharacterSheetControllerTests.cs
git commit -m "feat: add CharacterSheetController with grant/list/get/update endpoints"
```

## Task 12: Integration tests — uniqueness under concurrency, archived catalog entries still resolve

**Files:**
- Modify: `tests/Ruptura.IntegrationTests/Controllers/CharacterSheetControllerTests.cs`

**Interfaces:**
- Consumes: everything from Task 11 (unchanged).
- Produces: nothing new — this task is pure test coverage for two edge cases the design spec calls out explicitly (§9: "índice único parcial sob concorrência"; memory decision: archived homebrew entries must keep resolving on sheets that reference them).

- [ ] **Step 1: Write the concurrency test**

Add to `tests/Ruptura.IntegrationTests/Controllers/CharacterSheetControllerTests.cs`:

```csharp
    [Fact]
    public async Task Grant_TwoSimultaneousGrantsForTheSamePlayer_OnlyOneSucceeds()
    {
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var request = () => client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Race Condition" });

        var results = await Task.WhenAll(request(), request());

        results.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
        results.Count(r => r.StatusCode == HttpStatusCode.BadRequest).Should().Be(1);
    }
```

This relies on the application-level check in `CharacterSheetService.CreateAsync` (Task 6) racing against itself under `Task.WhenAll` — if both requests happen to pass the pre-check before either saves, the DB's `ux_character_sheets_owner_campaign_alive` partial unique index (Task 1) is the actual backstop. `CreateAsync` doesn't currently catch `DbUpdateException` the way `UpdateAsync` does (Task 8) — if this test is flaky (sometimes both succeed, or one throws an unhandled 500 instead of 400), that means the same `try/catch DbUpdateException` pattern from `UpdateAsync` needs to be added around `CreateAsync`'s `SaveChangesAsync` call too. Add it now if the test doesn't reliably show exactly one `Created` and one `BadRequest`:

```csharp
        try
        {
            await sheetRepo.AddAsync(sheet, ct);
            await sheetRepo.SaveChangesAsync(ct);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Result.Failure<CharacterSheetResponse>(ErrorCodes.CharacterSheet.AlreadyHasAliveCharacter);
        }
```

(replacing the plain `await sheetRepo.AddAsync(sheet, ct); await sheetRepo.SaveChangesAsync(ct);` pair in `CreateAsync` from Task 6 — add `using Microsoft.EntityFrameworkCore;` to `CharacterSheetService.cs` if Task 8 didn't already add it).

- [ ] **Step 2: Write the archived-entry-still-resolves test**

```csharp
    [Fact]
    public async Task ArchivedHomebrewCatalogEntry_StillResolvesOnASheetThatReferencesIt()
    {
        var (client, campaign, playerId, playerToken, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var talentResponse = await client.PostAsJsonAsync("api/catalog", new Ruptura.Shared.Catalog.CreateCatalogEntryRequest
        {
            CampaignId = campaign.Id, Type = "Talent", Name = "Soon To Be Retired",
            DataJson = """{"Category":"Combate","Effect":"x","PowerTier":"menor"}"""
        });
        var talent = (await talentResponse.Content.ReadFromJsonAsync<ApiResponse<Ruptura.Shared.Catalog.CatalogEntryResponse>>())!.Data!;

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        sheet.Data.Talents.Add(new Ruptura.Shared.CharacterSheets.CharacterCatalogRefEntry { CatalogEntryId = talent.Id });
        var putResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName,
            DataJson = System.Text.Json.JsonSerializer.Serialize(sheet.Data)
        });
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // GM archives the homebrew talent (Task 9's soft-delete).
        await client.DeleteAsync($"api/catalog/{talent.Id}");

        // The character sheet still resolves the reference — no 500, NP still includes it.
        var getResponse = await client.GetAsync($"api/character-sheets/{sheet.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reread = (await getResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        reread.Data.Talents.Should().ContainSingle(t => t.CatalogEntryId == talent.Id);
    }
```

This works because `ICatalogEntryRepository.GetByIdsAsync` (Task 5) doesn't filter by `IsArchived` at all — it's an unconditional id lookup, unlike `GetByTypeAsync` — so `CharacterSheetService.MapToResponseAsync` (Task 6) always resolves archived entries fine. If this test fails, that's the first place to check.

- [ ] **Step 3: Run both tests**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter "Grant_TwoSimultaneousGrantsForTheSamePlayer_OnlyOneSucceeds|ArchivedHomebrewCatalogEntry_StillResolvesOnASheetThatReferencesIt"`
Expected: PASS (2/2). Re-run once if a failure looks like the documented Serilog flake.

- [ ] **Step 4: Run the full integration suite**

Run: `dotnet test tests/Ruptura.IntegrationTests`
Expected: all PASS (modulo the documented pre-existing flake — re-run once if 1-2 unrelated tests fail).

- [ ] **Step 5: Commit**

```bash
git add tests/Ruptura.IntegrationTests/Controllers/CharacterSheetControllerTests.cs \
  src/Ruptura.Infrastructure/Services/CharacterSheetService.cs
git commit -m "test: add concurrency and archived-entry-resolution coverage for character sheets"
```

## Task 13: Web client services (`ICharacterSheetClientService`, `ICampaignClientService.GetMineAsync`, `includeArchived`)

**Files:**
- Create: `src/Ruptura.Web/Services/ICharacterSheetClientService.cs`
- Create: `src/Ruptura.Web/Services/CharacterSheetClientService.cs`
- Modify: `src/Ruptura.Web/Services/ICampaignClientService.cs`
- Modify: `src/Ruptura.Web/Services/CampaignClientService.cs`
- Modify: `src/Ruptura.Web/Services/ICatalogClientService.cs`
- Modify: `src/Ruptura.Web/Services/CatalogClientService.cs`
- Modify: `src/Ruptura.Web/Program.cs`

**Interfaces:**
- Consumes: `CharacterSheetResponse`, `GrantCharacterSheetRequest`, `UpdateCharacterSheetRequest` (Task 6), `CampaignResponse` (existing), `CatalogEntryResponse` (existing).
- Produces: `ICharacterSheetClientService` — consumed by every Web task from here on (14-18).

- [ ] **Step 1: Create `ICharacterSheetClientService`**

```csharp
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public interface ICharacterSheetClientService
{
    Task<ApiResponse<CharacterSheetResponse>?> GrantAsync(Guid campaignId, GrantCharacterSheetRequest request);
    Task<ApiResponse<IEnumerable<CharacterSheetResponse>>?> GetByCampaignAsync(Guid campaignId);
    Task<ApiResponse<CharacterSheetResponse>?> GetMineAsync(Guid campaignId);
    Task<ApiResponse<CharacterSheetResponse>?> GetAsync(Guid id);
    Task<ApiResponse<CharacterSheetResponse>?> UpdateAsync(Guid id, UpdateCharacterSheetRequest request);
}
```

- [ ] **Step 2: Implement `CharacterSheetClientService`**

```csharp
using System.Net.Http.Json;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public class CharacterSheetClientService(IHttpClientFactory factory) : ICharacterSheetClientService
{
    private HttpClient Http => factory.CreateClient("RupturaApi");

    public async Task<ApiResponse<CharacterSheetResponse>?> GrantAsync(Guid campaignId, GrantCharacterSheetRequest request)
    {
        var response = await Http.PostAsJsonAsync($"api/campaigns/{campaignId}/character-sheets", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>();
    }

    public async Task<ApiResponse<IEnumerable<CharacterSheetResponse>>?> GetByCampaignAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/character-sheets");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CharacterSheetResponse>>>();
    }

    public async Task<ApiResponse<CharacterSheetResponse>?> GetMineAsync(Guid campaignId)
    {
        var response = await Http.GetAsync($"api/campaigns/{campaignId}/character-sheets/mine");
        return await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>();
    }

    public async Task<ApiResponse<CharacterSheetResponse>?> GetAsync(Guid id)
    {
        var response = await Http.GetAsync($"api/character-sheets/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>();
    }

    public async Task<ApiResponse<CharacterSheetResponse>?> UpdateAsync(Guid id, UpdateCharacterSheetRequest request)
    {
        var response = await Http.PutAsJsonAsync($"api/character-sheets/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>();
    }
}
```

Note `GetMineAsync` and `GetAsync` deliberately read the body even on a non-success status (a 404 "no character granted yet" still needs its `Message` surfaced to the UI as "awaiting GM"), matching the existing `CatalogClientService.CreateAsync`/`UpdateAsync` convention — while `GetByCampaignAsync` returns `null` on failure since the GM sheet list page just shows empty on error, same as `CampaignClientService.GetAllAsync`.

- [ ] **Step 3: Add `GetMineAsync` to `ICampaignClientService`/`CampaignClientService`**

In `ICampaignClientService.cs`:

```csharp
    Task<ApiResponse<IEnumerable<CampaignResponse>>?> GetMineAsync();
```

In `CampaignClientService.cs`:

```csharp
    public async Task<ApiResponse<IEnumerable<CampaignResponse>>?> GetMineAsync()
    {
        var response = await Http.GetAsync("api/campaigns/mine");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignResponse>>>();
    }
```

- [ ] **Step 4: Add `includeArchived` to `ICatalogClientService`/`CatalogClientService`**

In `ICatalogClientService.cs`, change:

```csharp
    Task<ApiResponse<IEnumerable<CatalogEntryResponse>>?> GetByTypeAsync(string type, Guid campaignId, bool includeArchived = false);
```

In `CatalogClientService.cs`, change `GetByTypeAsync`:

```csharp
    public async Task<ApiResponse<IEnumerable<CatalogEntryResponse>>?> GetByTypeAsync(
        string type, Guid campaignId, bool includeArchived = false)
    {
        var query = $"api/catalog?type={HttpUtility.UrlEncode(type)}&campaignId={campaignId}&includeArchived={includeArchived}";
        var response = await Http.GetAsync(query);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>();
    }
```

`GmCatalog.razor`'s existing call site `CatalogService.GetByTypeAsync(_selectedType, CampaignId)` still compiles unchanged since `includeArchived` defaults to `false` — Task 18 will pass `true` explicitly there.

Also update `CatalogController.GetByType` (Task 9 left this hardcoded to `includeArchived: false`) to actually read the query parameter:

```csharp
    public async Task<IActionResult> GetByType(
        [FromQuery] string type, [FromQuery] Guid campaignId, [FromQuery] bool includeArchived, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await catalogService.GetByTypeAsync(callerId, type, campaignId, includeArchived, ct);
```

- [ ] **Step 5: Register in DI**

In `src/Ruptura.Web/Program.cs`, alongside the existing `AddScoped<ICatalogClientService, ...>` line:

```csharp
builder.Services.AddScoped<ICharacterSheetClientService, CharacterSheetClientService>();
```

- [ ] **Step 6: Build**

Run: `dotnet build`
Expected: no errors.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Web/Services/ src/Ruptura.Web/Program.cs src/Ruptura.API/Controllers/CatalogController.cs
git commit -m "feat: add CharacterSheetClientService and Campaign/Catalog client additions"
```

## Task 14: `CharacterSheetEditor` shell + Identity tab + Attributes tab

This is the shared component both the GM's and the player's pages render (Task 18). It owns loading/saving the whole sheet; each tab component just edits a slice of the shared `CharacterSheetData` object passed down by reference (Blazor's `@bind` on a nested reference-type property mutates the same object the parent holds — no `EventCallback` plumbing needed for these).

**Files:**
- Create: `src/Ruptura.Web/Pages/CharacterSheetEditor.razor`
- Create: `src/Ruptura.Web/Pages/CharacterSheetIdentityTab.razor`
- Create: `src/Ruptura.Web/Pages/CharacterSheetAttributesTab.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ICharacterSheetClientService` (Task 13), `ICatalogClientService` (existing), `CharacterSheetData`/`CharacterDerivedStats` (Task 3).
- Produces: `CharacterSheetEditor` with `[Parameter] Guid SheetId`, `[Parameter] Guid CampaignId`, `[Parameter] bool CanEditStatus` — consumed by `PlayerCharacter.razor` and `GmCharacterSheet.razor` (Task 18). The tab-nav pattern (Bootstrap `nav nav-tabs`) and the `Data`/`CampaignId` child-component parameter pair established here is reused by every tab in Tasks 15-17.

- [ ] **Step 1: Add the localization keys**

In `src/Ruptura.Web/Resources/AppStrings.resx`:

```xml
  <data name="Sheet.Loading"><value>Loading character sheet…</value></data>
  <data name="Sheet.NotFound"><value>Character sheet not found.</value></data>
  <data name="Sheet.Save"><value>Save</value></data>
  <data name="Sheet.Saved"><value>Saved.</value></data>
  <data name="Sheet.NameLabel"><value>Character Name</value></data>
  <data name="Sheet.PortraitLabel"><value>Portrait (path or URL)</value></data>
  <data name="Sheet.RankLabel"><value>Rank</value></data>
  <data name="Sheet.NpLabel"><value>NP</value></data>
  <data name="Sheet.MarkDead"><value>Dead</value></data>
  <data name="Sheet.MarkRetired"><value>Retired</value></data>
  <data name="Sheet.Tab.Identity"><value>Identity</value></data>
  <data name="Sheet.Tab.Attributes"><value>Attributes</value></data>
  <data name="Sheet.Tab.Combat"><value>Combat</value></data>
  <data name="Sheet.Tab.Skills"><value>Skills</value></data>
  <data name="Sheet.Tab.Talents"><value>Talents</value></data>
  <data name="Sheet.Tab.Spells"><value>Spells</value></data>
  <data name="Sheet.Tab.Techniques"><value>Techniques</value></data>
  <data name="Sheet.Tab.Equipment"><value>Equipment</value></data>
  <data name="Sheet.Tab.Trial"><value>Attribute Trial</value></data>
  <data name="Sheet.Tab.GuildRegistry"><value>Guild Registry</value></data>
  <data name="Sheet.Identity.Origin"><value>Origin</value></data>
  <data name="Sheet.Identity.Background"><value>Background</value></data>
  <data name="Sheet.Identity.Lineage"><value>Lineage</value></data>
  <data name="Sheet.Identity.Aptitudes"><value>Aptitudes (up to 2)</value></data>
  <data name="Sheet.Identity.InitialTalent"><value>Initial Talent</value></data>
  <data name="Sheet.Identity.Patron"><value>Patron (display name)</value></data>
  <data name="Sheet.Identity.None"><value>— none —</value></data>
  <data name="Sheet.Attributes.Corpo"><value>Body</value></data>
  <data name="Sheet.Attributes.Controle"><value>Control</value></data>
  <data name="Sheet.Attributes.Vigor"><value>Vigor</value></data>
  <data name="Sheet.Attributes.Presenca"><value>Presence</value></data>
  <data name="Sheet.Attributes.Intelecto"><value>Intellect</value></data>
  <data name="Sheet.Attributes.Percepcao"><value>Perception</value></data>
  <data name="Sheet.Attributes.Vontade"><value>Will</value></data>
  <data name="Sheet.Attributes.Afinidade"><value>Affinity</value></data>
  <data name="Sheet.Attributes.Modifier"><value>Mod.</value></data>
  <data name="Sheet.Attributes.Grade"><value>Grade Bonus</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, the matching pt-BR entries:

```xml
  <data name="Sheet.Loading"><value>Carregando ficha…</value></data>
  <data name="Sheet.NotFound"><value>Ficha não encontrada.</value></data>
  <data name="Sheet.Save"><value>Salvar</value></data>
  <data name="Sheet.Saved"><value>Salvo.</value></data>
  <data name="Sheet.NameLabel"><value>Nome do Personagem</value></data>
  <data name="Sheet.PortraitLabel"><value>Retrato (caminho ou URL)</value></data>
  <data name="Sheet.RankLabel"><value>Ranking</value></data>
  <data name="Sheet.NpLabel"><value>NP</value></data>
  <data name="Sheet.MarkDead"><value>Morto</value></data>
  <data name="Sheet.MarkRetired"><value>Aposentado</value></data>
  <data name="Sheet.Tab.Identity"><value>Identidade</value></data>
  <data name="Sheet.Tab.Attributes"><value>Atributos</value></data>
  <data name="Sheet.Tab.Combat"><value>Combate</value></data>
  <data name="Sheet.Tab.Skills"><value>Perícias</value></data>
  <data name="Sheet.Tab.Talents"><value>Talentos</value></data>
  <data name="Sheet.Tab.Spells"><value>Magias</value></data>
  <data name="Sheet.Tab.Techniques"><value>Técnicas</value></data>
  <data name="Sheet.Tab.Equipment"><value>Equipamento</value></data>
  <data name="Sheet.Tab.Trial"><value>Provação de Atributo</value></data>
  <data name="Sheet.Tab.GuildRegistry"><value>Registro da Guilda</value></data>
  <data name="Sheet.Identity.Origin"><value>Origem</value></data>
  <data name="Sheet.Identity.Background"><value>Histórico</value></data>
  <data name="Sheet.Identity.Lineage"><value>Linhagem</value></data>
  <data name="Sheet.Identity.Aptitudes"><value>Aptidões (até 2)</value></data>
  <data name="Sheet.Identity.InitialTalent"><value>Talento Inicial</value></data>
  <data name="Sheet.Identity.Patron"><value>Patrono (nome de exibição)</value></data>
  <data name="Sheet.Identity.None"><value>— nenhum —</value></data>
  <data name="Sheet.Attributes.Corpo"><value>Corpo</value></data>
  <data name="Sheet.Attributes.Controle"><value>Controle</value></data>
  <data name="Sheet.Attributes.Vigor"><value>Vigor</value></data>
  <data name="Sheet.Attributes.Presenca"><value>Presença</value></data>
  <data name="Sheet.Attributes.Intelecto"><value>Intelecto</value></data>
  <data name="Sheet.Attributes.Percepcao"><value>Percepção</value></data>
  <data name="Sheet.Attributes.Vontade"><value>Vontade</value></data>
  <data name="Sheet.Attributes.Afinidade"><value>Afinidade</value></data>
  <data name="Sheet.Attributes.Modifier"><value>Mod.</value></data>
  <data name="Sheet.Attributes.Grade"><value>Bônus de Grau</value></data>
```

- [ ] **Step 2: Create `CharacterSheetEditor.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@inject IStringLocalizer<AppStrings> L
@inject ICharacterSheetClientService SheetService

@if (_loading)
{
    <div class="ledger-empty"><span class="spinner-border spinner-border-sm me-2"></span>@L["Sheet.Loading"]</div>
}
else if (_sheet is null)
{
    <div class="alert-danger">@(_errorMessage ?? L["Sheet.NotFound"])</div>
}
else
{
    @if (!string.IsNullOrEmpty(_errorMessage))
    {
        <div class="alert-danger mb-4">@_errorMessage</div>
    }
    @if (!string.IsNullOrEmpty(_successMessage))
    {
        <div class="mb-4" style="color:var(--text-muted);font-size:.85rem">@_successMessage</div>
    }

    <div style="display:flex;flex-wrap:wrap;gap:1rem;align-items:flex-end;margin-bottom:1.5rem">
        <div>
            <label class="form-label">@L["Sheet.NameLabel"]</label>
            <input class="form-control" @bind="_characterName" @bind:event="oninput" />
        </div>
        <div>
            <label class="form-label">@L["Sheet.PortraitLabel"]</label>
            <input class="form-control" @bind="_portraitImagePath" @bind:event="oninput" />
        </div>
        <div>
            <span class="section-title" style="display:block">@L["Sheet.RankLabel"]</span>
            <span>@_data.GuildRegistry.Ranking</span>
        </div>
        <div>
            <span class="section-title" style="display:block">@L["Sheet.NpLabel"]</span>
            <span>@_derived?.Np</span>
        </div>
        @if (CanEditStatus)
        {
            <div class="form-check">
                <input class="form-check-input" type="checkbox" id="isDead" @bind="_isDead" />
                <label class="form-check-label" for="isDead">@L["Sheet.MarkDead"]</label>
            </div>
            <div class="form-check">
                <input class="form-check-input" type="checkbox" id="isRetired" @bind="_isRetired" />
                <label class="form-check-label" for="isRetired">@L["Sheet.MarkRetired"]</label>
            </div>
        }
        <button class="btn btn-primary btn-sm" @onclick="SaveAsync" disabled="@_saving">
            @if (_saving) { <span class="spinner-border spinner-border-sm me-1"></span> }
            @L["Sheet.Save"]
        </button>
    </div>

    <ul class="nav nav-tabs">
        @foreach (var tab in Tabs)
        {
            <li class="nav-item">
                <button class="nav-link @(_activeTab == tab.Key ? "active" : "")" @onclick="() => _activeTab = tab.Key">
                    @L[tab.Value]
                </button>
            </li>
        }
    </ul>

    <div style="padding:1.5rem 0">
        @if (_activeTab == "identity")
        {
            <CharacterSheetIdentityTab Data="_data" CampaignId="CampaignId" />
        }
        else if (_activeTab == "attributes")
        {
            <CharacterSheetAttributesTab Data="_data" Derived="_derived" />
        }
    </div>
}

@code {
    [Parameter] public Guid SheetId { get; set; }
    [Parameter] public Guid CampaignId { get; set; }
    [Parameter] public bool CanEditStatus { get; set; }

    private static readonly Dictionary<string, string> Tabs = new()
    {
        ["identity"] = "Sheet.Tab.Identity",
        ["attributes"] = "Sheet.Tab.Attributes"
        // Tasks 15-17 add: combat, skills, talents, spells, techniques, equipment, trial, guildRegistry
    };

    private bool _loading = true;
    private bool _saving;
    private string? _errorMessage;
    private string? _successMessage;
    private CharacterSheetResponse? _sheet;
    private CharacterSheetData _data = new();
    private CharacterDerivedStats? _derived;
    private string _characterName = string.Empty;
    private string? _portraitImagePath;
    private bool _isDead;
    private bool _isRetired;
    private string _activeTab = "identity";

    protected override async Task OnParametersSetAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        _errorMessage = null;
        var result = await SheetService.GetAsync(SheetId);
        if (result?.Data is null)
        {
            _sheet = null;
            _errorMessage = result?.Message;
        }
        else
        {
            _sheet = result.Data;
            _data = result.Data.Data;
            _derived = result.Data.DerivedStats;
            _characterName = result.Data.CharacterName;
            _portraitImagePath = result.Data.PortraitImagePath;
            _isDead = result.Data.IsDead;
            _isRetired = result.Data.IsRetired;
        }
        _loading = false;
    }

    private async Task SaveAsync()
    {
        _saving = true;
        _errorMessage = null;
        _successMessage = null;

        var result = await SheetService.UpdateAsync(SheetId, new UpdateCharacterSheetRequest
        {
            CharacterName = _characterName,
            DataJson = System.Text.Json.JsonSerializer.Serialize(_data),
            IsDead = _isDead,
            IsRetired = _isRetired,
            PortraitImagePath = _portraitImagePath
        });

        if (result?.Data is not null)
        {
            _sheet = result.Data;
            _data = result.Data.Data;
            _derived = result.Data.DerivedStats;
            _successMessage = L["Sheet.Saved"];
        }
        else
        {
            _errorMessage = result?.Message ?? L["Common.Error"];
        }

        _saving = false;
    }
}
```

- [ ] **Step 3: Create `CharacterSheetIdentityTab.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@using Ruptura.Shared.Catalog
@inject IStringLocalizer<AppStrings> L
@inject ICatalogClientService CatalogService

<div style="display:flex;flex-direction:column;gap:1rem;max-width:480px">
    <div>
        <label class="form-label">@L["Sheet.Identity.Origin"]</label>
        <select class="form-select" value="@Data.Identity.OriginId" @onchange="e => Data.Identity.OriginId = ParseGuid(e.Value)">
            <option value="">@L["Sheet.Identity.None"]</option>
            @foreach (var entry in _origins)
            {
                <option value="@entry.Id">@entry.Name</option>
            }
        </select>
    </div>
    <div>
        <label class="form-label">@L["Sheet.Identity.Background"]</label>
        <select class="form-select" value="@Data.Identity.BackgroundId" @onchange="e => Data.Identity.BackgroundId = ParseGuid(e.Value)">
            <option value="">@L["Sheet.Identity.None"]</option>
            @foreach (var entry in _backgrounds)
            {
                <option value="@entry.Id">@entry.Name</option>
            }
        </select>
    </div>
    <div>
        <label class="form-label">@L["Sheet.Identity.Lineage"]</label>
        <select class="form-select" value="@Data.Identity.LineageId" @onchange="e => Data.Identity.LineageId = ParseGuid(e.Value)">
            <option value="">@L["Sheet.Identity.None"]</option>
            @foreach (var entry in _lineages)
            {
                <option value="@entry.Id">@entry.Name</option>
            }
        </select>
    </div>
    <div>
        <label class="form-label">@L["Sheet.Identity.Aptitudes"]</label>
        @foreach (var entry in _aptitudes)
        {
            var id = entry.Id;
            <div class="form-check">
                <input class="form-check-input" type="checkbox" checked="@Data.Identity.AptitudeIds.Contains(id)"
                       @onchange="e => ToggleAptitude(id, (bool)(e.Value ?? false))" />
                <label class="form-check-label">@entry.Name</label>
            </div>
        }
    </div>
    <div>
        <label class="form-label">@L["Sheet.Identity.InitialTalent"]</label>
        <select class="form-select" value="@Data.Identity.InitialTalentId" @onchange="e => Data.Identity.InitialTalentId = ParseGuid(e.Value)">
            <option value="">@L["Sheet.Identity.None"]</option>
            @foreach (var entry in _talents)
            {
                <option value="@entry.Id">@entry.Name</option>
            }
        </select>
    </div>
    <div>
        <label class="form-label">@L["Sheet.Identity.Patron"]</label>
        <input class="form-control" @bind="Data.Identity.PatronDisplayName" @bind:event="oninput" />
    </div>
</div>

@code {
    [Parameter] public CharacterSheetData Data { get; set; } = new();
    [Parameter] public Guid CampaignId { get; set; }

    private List<CatalogEntryResponse> _origins = [];
    private List<CatalogEntryResponse> _backgrounds = [];
    private List<CatalogEntryResponse> _lineages = [];
    private List<CatalogEntryResponse> _aptitudes = [];
    private List<CatalogEntryResponse> _talents = [];

    protected override async Task OnInitializedAsync()
    {
        _origins = (await CatalogService.GetByTypeAsync("Origin", CampaignId))?.Data?.ToList() ?? [];
        _backgrounds = (await CatalogService.GetByTypeAsync("Background", CampaignId))?.Data?.ToList() ?? [];
        _lineages = (await CatalogService.GetByTypeAsync("Lineage", CampaignId))?.Data?.ToList() ?? [];
        _aptitudes = (await CatalogService.GetByTypeAsync("Aptitude", CampaignId))?.Data?.ToList() ?? [];
        _talents = (await CatalogService.GetByTypeAsync("Talent", CampaignId))?.Data?.ToList() ?? [];
    }

    private static Guid? ParseGuid(object? value) =>
        Guid.TryParse(value?.ToString(), out var id) ? id : null;

    private void ToggleAptitude(Guid id, bool isChecked)
    {
        if (isChecked && !Data.Identity.AptitudeIds.Contains(id) && Data.Identity.AptitudeIds.Count < 2)
            Data.Identity.AptitudeIds.Add(id);
        else if (!isChecked)
            Data.Identity.AptitudeIds.Remove(id);
    }
}
```

- [ ] **Step 4: Create `CharacterSheetAttributesTab.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@inject IStringLocalizer<AppStrings> L

<div class="ledger-table-wrap">
    <table class="ledger-table">
        <thead>
            <tr>
                <th></th>
                <th>@L["Sheet.NameLabel"]</th>
                <th>@L["Sheet.Attributes.Modifier"]</th>
                <th>@L["Sheet.Attributes.Grade"]</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var attr in Attrs)
            {
                <tr>
                    <td style="width:80px">
                        <input class="form-control form-control-sm" type="number" min="1" max="6"
                               value="@attr.Get(Data.Attributes)" @onchange="e => attr.Set(Data.Attributes, ParseInt(e.Value))" />
                    </td>
                    <td>@L[attr.LabelKey]</td>
                    <td>@FormatModifier(Derived?.AttributeModifiers.GetValueOrDefault(attr.Key) ?? 0)</td>
                    <td>@FormatModifier(Derived?.AttributeGradeBonuses.GetValueOrDefault(attr.Key) ?? 0)</td>
                </tr>
            }
        </tbody>
    </table>
</div>

@code {
    [Parameter] public CharacterSheetData Data { get; set; } = new();
    [Parameter] public CharacterDerivedStats? Derived { get; set; }

    private record AttrRow(string Key, string LabelKey, Func<CharacterAttributes, int> Get, Action<CharacterAttributes, int> Set);

    private static readonly List<AttrRow> Attrs =
    [
        new("Corpo", "Sheet.Attributes.Corpo", a => a.Corpo, (a, v) => a.Corpo = v),
        new("Controle", "Sheet.Attributes.Controle", a => a.Controle, (a, v) => a.Controle = v),
        new("Vigor", "Sheet.Attributes.Vigor", a => a.Vigor, (a, v) => a.Vigor = v),
        new("Presenca", "Sheet.Attributes.Presenca", a => a.Presenca, (a, v) => a.Presenca = v),
        new("Intelecto", "Sheet.Attributes.Intelecto", a => a.Intelecto, (a, v) => a.Intelecto = v),
        new("Percepcao", "Sheet.Attributes.Percepcao", a => a.Percepcao, (a, v) => a.Percepcao = v),
        new("Vontade", "Sheet.Attributes.Vontade", a => a.Vontade, (a, v) => a.Vontade = v),
        new("Afinidade", "Sheet.Attributes.Afinidade", a => a.Afinidade, (a, v) => a.Afinidade = v)
    ];

    private static int ParseInt(object? value) => int.TryParse(value?.ToString(), out var v) ? v : 1;

    private static string FormatModifier(int value) => value switch
    {
        > 0 => $"+{value}",
        _ => value.ToString()
    };
}
```

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: no errors. (There is no automated test coverage for Blazor component markup in this repo's existing conventions — Task 18's end-to-end integration test is what exercises this UI's underlying API calls; the component itself is verified by running the app, per Step 6.)

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Web/Pages/CharacterSheetEditor.razor src/Ruptura.Web/Pages/CharacterSheetIdentityTab.razor \
  src/Ruptura.Web/Pages/CharacterSheetAttributesTab.razor \
  src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add CharacterSheetEditor shell with Identity and Attributes tabs"
```

## Task 15: Combat tab + Skills tab

**Files:**
- Create: `src/Ruptura.Web/Pages/CharacterSheetCombatTab.razor`
- Create: `src/Ruptura.Web/Pages/CharacterSheetSkillsTab.razor`
- Modify: `src/Ruptura.Web/Pages/CharacterSheetEditor.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx` / `.pt-BR.resx`

**Interfaces:**
- Consumes: `CharacterSheetData`, `CharacterDerivedStats`, `WeaponCombatRow` (Task 3-4), `ICatalogClientService` (existing).
- Produces: two more tab entries wired into `CharacterSheetEditor`'s `Tabs` dictionary and render `@if` chain.

- [ ] **Step 1: Add localization keys**

`AppStrings.resx`:

```xml
  <data name="Sheet.Combat.CurrentHp"><value>Current HP</value></data>
  <data name="Sheet.Combat.MaxHp"><value>Max HP</value></data>
  <data name="Sheet.Combat.Movement"><value>Movement</value></data>
  <data name="Sheet.Combat.Initiative"><value>Initiative</value></data>
  <data name="Sheet.Combat.PassiveDefense"><value>Passive Defense</value></data>
  <data name="Sheet.Combat.DamageReduction"><value>Damage Reduction</value></data>
  <data name="Sheet.Combat.Conditions"><value>Active Conditions</value></data>
  <data name="Sheet.Combat.AddCondition"><value>Add</value></data>
  <data name="Sheet.Combat.Weapons"><value>Weapons (equipped, see Equipment tab)</value></data>
  <data name="Sheet.Combat.Weapon.Name"><value>Weapon</value></data>
  <data name="Sheet.Combat.Weapon.Attack"><value>Attack</value></data>
  <data name="Sheet.Combat.Weapon.Damage"><value>Damage</value></data>
  <data name="Sheet.Skills.Add"><value>Add Skill</value></data>
  <data name="Sheet.Skills.Points"><value>Points</value></data>
  <data name="Sheet.Skills.Grade"><value>Grade Bonus</value></data>
  <data name="Sheet.Skills.Remove"><value>Remove</value></data>
```

`AppStrings.pt-BR.resx`:

```xml
  <data name="Sheet.Combat.CurrentHp"><value>PV Atual</value></data>
  <data name="Sheet.Combat.MaxHp"><value>PV Máximo</value></data>
  <data name="Sheet.Combat.Movement"><value>Deslocamento</value></data>
  <data name="Sheet.Combat.Initiative"><value>Iniciativa</value></data>
  <data name="Sheet.Combat.PassiveDefense"><value>Defesa Passiva</value></data>
  <data name="Sheet.Combat.DamageReduction"><value>Redução de Dano</value></data>
  <data name="Sheet.Combat.Conditions"><value>Condições Ativas</value></data>
  <data name="Sheet.Combat.AddCondition"><value>Adicionar</value></data>
  <data name="Sheet.Combat.Weapons"><value>Armas (equipadas, ver aba Equipamento)</value></data>
  <data name="Sheet.Combat.Weapon.Name"><value>Arma</value></data>
  <data name="Sheet.Combat.Weapon.Attack"><value>Ataque</value></data>
  <data name="Sheet.Combat.Weapon.Damage"><value>Dano</value></data>
  <data name="Sheet.Skills.Add"><value>Adicionar Perícia</value></data>
  <data name="Sheet.Skills.Points"><value>Pontos</value></data>
  <data name="Sheet.Skills.Grade"><value>Bônus de Grau</value></data>
  <data name="Sheet.Skills.Remove"><value>Remover</value></data>
```

- [ ] **Step 2: Create `CharacterSheetCombatTab.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@inject IStringLocalizer<AppStrings> L

<div style="display:flex;flex-wrap:wrap;gap:1.5rem;margin-bottom:1.5rem">
    <div>
        <label class="form-label">@L["Sheet.Combat.CurrentHp"]</label>
        <input class="form-control" type="number" @bind="Data.Combat.CurrentHp" @bind:event="oninput" style="width:100px" />
    </div>
    <div><span class="section-title" style="display:block">@L["Sheet.Combat.MaxHp"]</span><span>@Derived?.MaxHp</span></div>
    <div><span class="section-title" style="display:block">@L["Sheet.Combat.Movement"]</span><span>@Derived?.Movement</span></div>
    <div><span class="section-title" style="display:block">@L["Sheet.Combat.Initiative"]</span><span>@Derived?.Initiative</span></div>
    <div><span class="section-title" style="display:block">@L["Sheet.Combat.PassiveDefense"]</span><span>@Derived?.PassiveDefense</span></div>
    <div><span class="section-title" style="display:block">@L["Sheet.Combat.DamageReduction"]</span><span>@Derived?.DamageReduction</span></div>
</div>

<div class="section-header">
    <span class="section-title">@L["Sheet.Combat.Conditions"]</span>
</div>
<div style="display:flex;gap:.5rem;margin-bottom:1rem">
    <input class="form-control" style="max-width:240px" @bind="_newCondition" @bind:event="oninput" />
    <button class="btn btn-outline-secondary btn-sm" @onclick="AddCondition">@L["Sheet.Combat.AddCondition"]</button>
</div>
<div style="display:flex;gap:.5rem;flex-wrap:wrap;margin-bottom:1.5rem">
    @foreach (var condition in Data.Combat.ActiveConditions.ToList())
    {
        <span class="btn btn-outline-secondary btn-sm" @onclick="() => Data.Combat.ActiveConditions.Remove(condition)">
            @condition ✕
        </span>
    }
</div>

<div class="section-header"><span class="section-title">@L["Sheet.Combat.Weapons"]</span></div>
<div class="ledger-table-wrap">
    <table class="ledger-table">
        <thead>
            <tr><th>@L["Sheet.Combat.Weapon.Name"]</th><th>@L["Sheet.Combat.Weapon.Attack"]</th><th>@L["Sheet.Combat.Weapon.Damage"]</th></tr>
        </thead>
        <tbody>
            @foreach (var weapon in Derived?.Weapons ?? [])
            {
                <tr>
                    <td>@weapon.Name</td>
                    <td>@(weapon.AttackBonus >= 0 ? $"+{weapon.AttackBonus}" : weapon.AttackBonus.ToString())</td>
                    <td>@weapon.DamageFormula</td>
                </tr>
            }
        </tbody>
    </table>
</div>

@code {
    [Parameter] public CharacterSheetData Data { get; set; } = new();
    [Parameter] public CharacterDerivedStats? Derived { get; set; }

    private string _newCondition = string.Empty;

    private void AddCondition()
    {
        if (string.IsNullOrWhiteSpace(_newCondition)) return;
        Data.Combat.ActiveConditions.Add(_newCondition.Trim());
        _newCondition = string.Empty;
    }
}
```

- [ ] **Step 3: Create `CharacterSheetSkillsTab.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@using Ruptura.Shared.Catalog
@inject IStringLocalizer<AppStrings> L
@inject ICatalogClientService CatalogService

<div class="ledger-table-wrap">
    <table class="ledger-table">
        <thead>
            <tr>
                <th>@L["Gm.CampaignDetail.Col.Name"]</th>
                <th>@L["Sheet.Skills.Points"]</th>
                <th>@L["Sheet.Skills.Grade"]</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            @foreach (var skill in Data.Skills)
            {
                <tr>
                    <td>@NameOf(skill.CatalogEntryId)</td>
                    <td style="width:100px">
                        <input class="form-control form-control-sm" type="number" min="0"
                               value="@skill.Points" @onchange="e => skill.Points = ParseInt(e.Value)" />
                    </td>
                    <td>@Derived?.SkillGradeBonuses.GetValueOrDefault(skill.CatalogEntryId)</td>
                    <td><button class="btn btn-outline-secondary btn-sm" @onclick="() => Data.Skills.Remove(skill)">@L["Sheet.Skills.Remove"]</button></td>
                </tr>
            }
        </tbody>
    </table>
</div>

<div style="display:flex;gap:.5rem;margin-top:1rem">
    <select class="form-select" style="max-width:320px" @bind="_selectedSkillId">
        @foreach (var entry in _available)
        {
            <option value="@entry.Id">@entry.Name</option>
        }
    </select>
    <button class="btn btn-primary btn-sm" @onclick="AddSkill" disabled="@(_selectedSkillId == Guid.Empty)">@L["Sheet.Skills.Add"]</button>
</div>

@code {
    [Parameter] public CharacterSheetData Data { get; set; } = new();
    [Parameter] public CharacterDerivedStats? Derived { get; set; }
    [Parameter] public Guid CampaignId { get; set; }

    private List<CatalogEntryResponse> _all = [];
    private Guid _selectedSkillId;

    private IEnumerable<CatalogEntryResponse> _available =>
        _all.Where(e => Data.Skills.All(s => s.CatalogEntryId != e.Id));

    protected override async Task OnInitializedAsync()
    {
        _all = (await CatalogService.GetByTypeAsync("Skill", CampaignId))?.Data?.ToList() ?? [];
    }

    private string NameOf(Guid id) => _all.FirstOrDefault(e => e.Id == id)?.Name ?? id.ToString();

    private static int ParseInt(object? value) => int.TryParse(value?.ToString(), out var v) ? v : 0;

    private void AddSkill()
    {
        if (_selectedSkillId == Guid.Empty) return;
        Data.Skills.Add(new CharacterSkillEntry { CatalogEntryId = _selectedSkillId, Points = 0 });
        _selectedSkillId = Guid.Empty;
    }
}
```

- [ ] **Step 4: Wire both tabs into `CharacterSheetEditor.razor`**

In the `Tabs` dictionary, add after `["attributes"] = "Sheet.Tab.Attributes"`:

```csharp
        ["combat"] = "Sheet.Tab.Combat",
        ["skills"] = "Sheet.Tab.Skills",
```

In the tab-body `@if`/`else if` chain, add:

```razor
        else if (_activeTab == "combat")
        {
            <CharacterSheetCombatTab Data="_data" Derived="_derived" />
        }
        else if (_activeTab == "skills")
        {
            <CharacterSheetSkillsTab Data="_data" Derived="_derived" CampaignId="CampaignId" />
        }
```

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: no errors.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Web/Pages/CharacterSheetCombatTab.razor src/Ruptura.Web/Pages/CharacterSheetSkillsTab.razor \
  src/Ruptura.Web/Pages/CharacterSheetEditor.razor \
  src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add Combat and Skills character sheet tabs"
```

## Task 16: Shared reference-list tab (Talents/Spells/Techniques) + Equipment tab

Talents, Spells, and Techniques are all "pick from catalog, remove from list" with no extra per-entry fields — one shared component parameterized by `CatalogEntryType`, used three times. Equipment needs its own component (Quantity/Durability/IsEquipped/LinkedSkillEntryId per row).

**Files:**
- Create: `src/Ruptura.Web/Pages/CharacterSheetCatalogRefListTab.razor`
- Create: `src/Ruptura.Web/Pages/CharacterSheetEquipmentTab.razor`
- Modify: `src/Ruptura.Web/Pages/CharacterSheetEditor.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx` / `.pt-BR.resx`

**Interfaces:**
- Consumes: `CharacterCatalogRefEntry`, `CharacterEquipmentEntry` (Task 3), `ICatalogClientService` (existing).
- Produces: 4 more tab entries in `CharacterSheetEditor`.

- [ ] **Step 1: Add localization keys**

`AppStrings.resx`:

```xml
  <data name="Sheet.RefList.Add"><value>Add</value></data>
  <data name="Sheet.RefList.Remove"><value>Remove</value></data>
  <data name="Sheet.Equipment.Quantity"><value>Qty.</value></data>
  <data name="Sheet.Equipment.Durability"><value>Durability</value></data>
  <data name="Sheet.Equipment.Equipped"><value>Equipped</value></data>
  <data name="Sheet.Equipment.LinkedSkill"><value>Linked Skill (weapons only)</value></data>
  <data name="Sheet.Equipment.NoneSkill"><value>— none —</value></data>
```

`AppStrings.pt-BR.resx`:

```xml
  <data name="Sheet.RefList.Add"><value>Adicionar</value></data>
  <data name="Sheet.RefList.Remove"><value>Remover</value></data>
  <data name="Sheet.Equipment.Quantity"><value>Qtd.</value></data>
  <data name="Sheet.Equipment.Durability"><value>Durabilidade</value></data>
  <data name="Sheet.Equipment.Equipped"><value>Equipado</value></data>
  <data name="Sheet.Equipment.LinkedSkill"><value>Perícia Vinculada (só armas)</value></data>
  <data name="Sheet.Equipment.NoneSkill"><value>— nenhuma —</value></data>
```

- [ ] **Step 2: Create `CharacterSheetCatalogRefListTab.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@using Ruptura.Shared.Catalog
@inject IStringLocalizer<AppStrings> L
@inject ICatalogClientService CatalogService

<div class="ledger-table-wrap">
    <table class="ledger-table">
        <thead>
            <tr><th>@L["Gm.CampaignDetail.Col.Name"]</th><th></th></tr>
        </thead>
        <tbody>
            @foreach (var entry in Entries)
            {
                <tr>
                    <td>@NameOf(entry.CatalogEntryId)</td>
                    <td><button class="btn btn-outline-secondary btn-sm" @onclick="() => Entries.Remove(entry)">@L["Sheet.RefList.Remove"]</button></td>
                </tr>
            }
        </tbody>
    </table>
</div>

<div style="display:flex;gap:.5rem;margin-top:1rem">
    <select class="form-select" style="max-width:320px" @bind="_selectedId">
        @foreach (var entry in Available)
        {
            <option value="@entry.Id">@entry.Name</option>
        }
    </select>
    <button class="btn btn-primary btn-sm" @onclick="AddEntry" disabled="@(_selectedId == Guid.Empty)">@L["Sheet.RefList.Add"]</button>
</div>

@code {
    [Parameter] public List<CharacterCatalogRefEntry> Entries { get; set; } = [];
    [Parameter] public Guid CampaignId { get; set; }
    [Parameter] public string CatalogType { get; set; } = string.Empty; // "Talent" | "Spell" | "Technique"

    private List<CatalogEntryResponse> _all = [];
    private Guid _selectedId;

    private IEnumerable<CatalogEntryResponse> Available =>
        _all.Where(e => Entries.All(x => x.CatalogEntryId != e.Id));

    protected override async Task OnInitializedAsync()
    {
        _all = (await CatalogService.GetByTypeAsync(CatalogType, CampaignId))?.Data?.ToList() ?? [];
    }

    private string NameOf(Guid id) => _all.FirstOrDefault(e => e.Id == id)?.Name ?? id.ToString();

    private void AddEntry()
    {
        if (_selectedId == Guid.Empty) return;
        Entries.Add(new CharacterCatalogRefEntry { CatalogEntryId = _selectedId });
        _selectedId = Guid.Empty;
    }
}
```

Note: `[Parameter] public string CatalogType { get; set; }` (not re-fetched reactively) means if `CampaignId`/`CatalogType` ever changed after first render this component wouldn't reload — acceptable here since both are fixed for the lifetime of a single sheet-editing session (this mirrors `CharacterSheetIdentityTab`'s and `CharacterSheetSkillsTab`'s same `OnInitializedAsync`-only loading, consistent within this task's own scope).

- [ ] **Step 3: Create `CharacterSheetEquipmentTab.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@using Ruptura.Shared.Catalog
@inject IStringLocalizer<AppStrings> L
@inject ICatalogClientService CatalogService

<div class="ledger-table-wrap">
    <table class="ledger-table">
        <thead>
            <tr>
                <th>@L["Gm.CampaignDetail.Col.Name"]</th>
                <th>@L["Sheet.Equipment.Quantity"]</th>
                <th>@L["Sheet.Equipment.Durability"]</th>
                <th>@L["Sheet.Equipment.Equipped"]</th>
                <th>@L["Sheet.Equipment.LinkedSkill"]</th>
                <th></th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in Data.Equipment)
            {
                <tr>
                    <td>@NameOf(item.CatalogEntryId)</td>
                    <td style="width:80px">
                        <input class="form-control form-control-sm" type="number" min="1"
                               value="@item.Quantity" @onchange="e => item.Quantity = ParseInt(e.Value, 1)" />
                    </td>
                    <td style="width:100px">
                        <input class="form-control form-control-sm" type="number" min="0"
                               value="@item.DurabilityRemaining" @onchange="e => item.DurabilityRemaining = ParseInt(e.Value, 0)" />
                    </td>
                    <td><input type="checkbox" checked="@item.IsEquipped" @onchange="e => item.IsEquipped = (bool)(e.Value ?? false)" /></td>
                    <td>
                        <select class="form-select form-select-sm" value="@item.LinkedSkillEntryId"
                                @onchange="e => item.LinkedSkillEntryId = ParseGuid(e.Value)">
                            <option value="">@L["Sheet.Equipment.NoneSkill"]</option>
                            @foreach (var skill in _skills)
                            {
                                <option value="@skill.CatalogEntryId">@SkillNameOf(skill.CatalogEntryId)</option>
                            }
                        </select>
                    </td>
                    <td><button class="btn btn-outline-secondary btn-sm" @onclick="() => Data.Equipment.Remove(item)">@L["Sheet.RefList.Remove"]</button></td>
                </tr>
            }
        </tbody>
    </table>
</div>

<div style="display:flex;gap:.5rem;margin-top:1rem">
    <select class="form-select" style="max-width:320px" @bind="_selectedId">
        @foreach (var entry in _allItems)
        {
            <option value="@entry.Id">@entry.Name</option>
        }
    </select>
    <button class="btn btn-primary btn-sm" @onclick="AddItem" disabled="@(_selectedId == Guid.Empty)">@L["Sheet.RefList.Add"]</button>
</div>

@code {
    [Parameter] public CharacterSheetData Data { get; set; } = new();
    [Parameter] public Guid CampaignId { get; set; }

    private List<CatalogEntryResponse> _allItems = [];
    private List<CharacterSkillEntry> _skills => Data.Skills;
    private List<CatalogEntryResponse> _skillCatalog = [];
    private Guid _selectedId;

    protected override async Task OnInitializedAsync()
    {
        _allItems = (await CatalogService.GetByTypeAsync("EquipmentItem", CampaignId))?.Data?.ToList() ?? [];
        _skillCatalog = (await CatalogService.GetByTypeAsync("Skill", CampaignId))?.Data?.ToList() ?? [];
    }

    private string NameOf(Guid id) => _allItems.FirstOrDefault(e => e.Id == id)?.Name ?? id.ToString();
    private string SkillNameOf(Guid id) => _skillCatalog.FirstOrDefault(e => e.Id == id)?.Name ?? id.ToString();

    private static int ParseInt(object? value, int fallback) => int.TryParse(value?.ToString(), out var v) ? v : fallback;
    private static Guid? ParseGuid(object? value) => Guid.TryParse(value?.ToString(), out var id) ? id : null;

    private void AddItem()
    {
        if (_selectedId == Guid.Empty) return;
        Data.Equipment.Add(new CharacterEquipmentEntry { CatalogEntryId = _selectedId, Quantity = 1 });
        _selectedId = Guid.Empty;
    }
}
```

`_skills` intentionally lists every invested `Skills[]` entry (not just combat-related ones) as candidates for `LinkedSkillEntryId` — the GDD doesn't restrict which Skill can govern a weapon, and filtering by `Area == "Combate — Armas"` etc. would silently hide legitimate homebrew skills with different area names. The dropdown shows every skill the character has invested points in.

- [ ] **Step 4: Wire all 4 tabs into `CharacterSheetEditor.razor`**

In `Tabs`, add after `["skills"] = "Sheet.Tab.Skills"`:

```csharp
        ["talents"] = "Sheet.Tab.Talents",
        ["spells"] = "Sheet.Tab.Spells",
        ["techniques"] = "Sheet.Tab.Techniques",
        ["equipment"] = "Sheet.Tab.Equipment",
```

In the render chain, add:

```razor
        else if (_activeTab == "talents")
        {
            <CharacterSheetCatalogRefListTab Entries="_data.Talents" CampaignId="CampaignId" CatalogType="Talent" />
        }
        else if (_activeTab == "spells")
        {
            <CharacterSheetCatalogRefListTab Entries="_data.Spells" CampaignId="CampaignId" CatalogType="Spell" />
        }
        else if (_activeTab == "techniques")
        {
            <CharacterSheetCatalogRefListTab Entries="_data.Techniques" CampaignId="CampaignId" CatalogType="Technique" />
        }
        else if (_activeTab == "equipment")
        {
            <CharacterSheetEquipmentTab Data="_data" CampaignId="CampaignId" />
        }
```

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: no errors.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Web/Pages/CharacterSheetCatalogRefListTab.razor src/Ruptura.Web/Pages/CharacterSheetEquipmentTab.razor \
  src/Ruptura.Web/Pages/CharacterSheetEditor.razor \
  src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add Talents/Spells/Techniques and Equipment character sheet tabs"
```

## Task 17: Attribute Trial tab + Guild Registry tab + player pages (`/campaigns`, `/campaigns/{id}/character`)

**Files:**
- Create: `src/Ruptura.Web/Pages/CharacterSheetTrialTab.razor`
- Create: `src/Ruptura.Web/Pages/CharacterSheetGuildRegistryTab.razor`
- Create: `src/Ruptura.Web/Pages/PlayerCampaigns.razor`
- Create: `src/Ruptura.Web/Pages/PlayerCharacter.razor`
- Modify: `src/Ruptura.Web/Pages/CharacterSheetEditor.razor`
- Modify: `src/Ruptura.Web/Layout/NavMenu.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx` / `.pt-BR.resx`

**Interfaces:**
- Consumes: `ICampaignClientService.GetMineAsync` (Task 13), `ICharacterSheetClientService.GetMineAsync` (Task 13), `CharacterSheetEditor` (Task 14).
- Produces: the last 2 tabs; the two player-facing routed pages the design spec §8 calls for.

- [ ] **Step 1: Add localization keys**

`AppStrings.resx`:

```xml
  <data name="Sheet.Trial.None"><value>No attribute trial in progress.</value></data>
  <data name="Sheet.Trial.Start"><value>Start Trial</value></data>
  <data name="Sheet.Trial.Attribute"><value>Attribute</value></data>
  <data name="Sheet.Trial.TargetGrade"><value>Target Grade</value></data>
  <data name="Sheet.Trial.DaysRemaining"><value>Days Remaining</value></data>
  <data name="Sheet.Trial.Clear"><value>Clear</value></data>
  <data name="Sheet.Guild.JoinedDate"><value>Joined</value></data>
  <data name="Sheet.Guild.State"><value>State</value></data>
  <data name="Sheet.Guild.Expeditions"><value>Expeditions</value></data>
  <data name="Sheet.Guild.FloorsCleared"><value>Floors Cleared</value></data>
  <data name="Campaigns.Title"><value>My Campaigns</value></data>
  <data name="Campaigns.Empty"><value>You're not part of any campaign yet.</value></data>
  <data name="Campaigns.OpenCharacter"><value>Open Character</value></data>
  <data name="Character.AwaitingGrant"><value>Your Game Master hasn't granted you a character yet.</value></data>
  <data name="Nav.Campaigns.Player"><value>My Campaigns</value></data>
```

`AppStrings.pt-BR.resx`:

```xml
  <data name="Sheet.Trial.None"><value>Nenhuma Provação de Atributo em andamento.</value></data>
  <data name="Sheet.Trial.Start"><value>Iniciar Provação</value></data>
  <data name="Sheet.Trial.Attribute"><value>Atributo</value></data>
  <data name="Sheet.Trial.TargetGrade"><value>Grau Alvo</value></data>
  <data name="Sheet.Trial.DaysRemaining"><value>Dias Restantes</value></data>
  <data name="Sheet.Trial.Clear"><value>Limpar</value></data>
  <data name="Sheet.Guild.JoinedDate"><value>Ingresso</value></data>
  <data name="Sheet.Guild.State"><value>Estado</value></data>
  <data name="Sheet.Guild.Expeditions"><value>Expedições</value></data>
  <data name="Sheet.Guild.FloorsCleared"><value>Andares Limpos</value></data>
  <data name="Campaigns.Title"><value>Minhas Campanhas</value></data>
  <data name="Campaigns.Empty"><value>Você ainda não participa de nenhuma campanha.</value></data>
  <data name="Campaigns.OpenCharacter"><value>Abrir Personagem</value></data>
  <data name="Character.AwaitingGrant"><value>Seu Mestre ainda não concedeu um personagem a você.</value></data>
  <data name="Nav.Campaigns.Player"><value>Minhas Campanhas</value></data>
```

- [ ] **Step 2: Create `CharacterSheetTrialTab.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@inject IStringLocalizer<AppStrings> L

@if (Data.AttributeTrial is null)
{
    <p style="color:var(--text-muted)">@L["Sheet.Trial.None"]</p>
    <button class="btn btn-outline-secondary btn-sm" @onclick="Start">@L["Sheet.Trial.Start"]</button>
}
else
{
    <div style="display:flex;flex-direction:column;gap:1rem;max-width:320px">
        <div>
            <label class="form-label">@L["Sheet.Trial.Attribute"]</label>
            <input class="form-control" @bind="Data.AttributeTrial.AttributeName" @bind:event="oninput" />
        </div>
        <div>
            <label class="form-label">@L["Sheet.Trial.TargetGrade"]</label>
            <input class="form-control" @bind="Data.AttributeTrial.TargetGrade" @bind:event="oninput" />
        </div>
        <div>
            <label class="form-label">@L["Sheet.Trial.DaysRemaining"]</label>
            <input class="form-control" type="number" min="0" @bind="Data.AttributeTrial.DaysRemaining" @bind:event="oninput" />
        </div>
        <button class="btn btn-outline-secondary btn-sm" @onclick="Clear">@L["Sheet.Trial.Clear"]</button>
    </div>
}

@code {
    [Parameter] public CharacterSheetData Data { get; set; } = new();

    private void Start() => Data.AttributeTrial = new CharacterAttributeTrial();
    private void Clear() => Data.AttributeTrial = null;
}
```

- [ ] **Step 3: Create `CharacterSheetGuildRegistryTab.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@inject IStringLocalizer<AppStrings> L

<div style="display:flex;flex-direction:column;gap:1rem;max-width:320px">
    <div>
        <label class="form-label">@L["Sheet.RankLabel"]</label>
        <select class="form-select" @bind="Data.GuildRegistry.Ranking">
            @foreach (var rank in Rankings)
            {
                <option value="@rank">@rank</option>
            }
        </select>
    </div>
    <div>
        <label class="form-label">@L["Sheet.Guild.JoinedDate"]</label>
        <input class="form-control" type="date"
               value="@Data.GuildRegistry.JoinedDate?.ToString("yyyy-MM-dd")"
               @onchange="e => Data.GuildRegistry.JoinedDate = DateTime.TryParse(e.Value?.ToString(), out var d) ? d : null" />
    </div>
    <div>
        <label class="form-label">@L["Sheet.Guild.State"]</label>
        <input class="form-control" @bind="Data.GuildRegistry.State" @bind:event="oninput" />
    </div>
    <div>
        <label class="form-label">@L["Sheet.Guild.Expeditions"]</label>
        <input class="form-control" type="number" min="0" @bind="Data.GuildRegistry.Expeditions" @bind:event="oninput" />
    </div>
    <div>
        <label class="form-label">@L["Sheet.Guild.FloorsCleared"]</label>
        <input class="form-control" type="number" min="0" @bind="Data.GuildRegistry.FloorsCleared" @bind:event="oninput" />
    </div>
</div>

@code {
    [Parameter] public CharacterSheetData Data { get; set; } = new();

    private static readonly string[] Rankings =
        ["Bronze", "Ferro", "Aço", "Prata", "Ouro", "Mithril", "Adamante", "Lendário"];
}
```

(`IsDead`/`IsRetired` — also conceptually part of "Registro da Guilda: morto/aposentado" per the design spec's module list — are edited in `CharacterSheetEditor`'s header, not here, because they're real `CharacterSheet` columns with GM-only write permission, not `DataJson` fields; duplicating their controls here would just be two write paths to the same state.)

- [ ] **Step 4: Wire both tabs into `CharacterSheetEditor.razor`**

In `Tabs`, add after `["equipment"] = "Sheet.Tab.Equipment"`:

```csharp
        ["trial"] = "Sheet.Tab.Trial",
        ["guildRegistry"] = "Sheet.Tab.GuildRegistry"
```

In the render chain, add:

```razor
        else if (_activeTab == "trial")
        {
            <CharacterSheetTrialTab Data="_data" />
        }
        else if (_activeTab == "guildRegistry")
        {
            <CharacterSheetGuildRegistryTab Data="_data" />
        }
```

- [ ] **Step 5: Create `PlayerCampaigns.razor`**

```razor
@page "/campaigns"
@attribute [Authorize(Roles = "Player")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Campaigns
@inject IStringLocalizer<AppStrings> L
@inject ICampaignClientService CampaignService

<PageTitle>@L["Campaigns.Title"] — RUPTURA</PageTitle>

<div class="page-content">
    <div class="page-heading"><h1>@L["Campaigns.Title"]</h1></div>

    @if (_loading)
    {
        <div class="ledger-empty"><span class="spinner-border spinner-border-sm me-2"></span>@L["Common.Loading"]</div>
    }
    else if (_campaigns.Count == 0)
    {
        <div class="ledger-empty"><p>@L["Campaigns.Empty"]</p></div>
    }
    else
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table">
                <thead><tr><th>@L["Gm.CampaignDetail.Col.Name"]</th><th></th></tr></thead>
                <tbody>
                    @foreach (var campaign in _campaigns)
                    {
                        <tr>
                            <td>@campaign.Name</td>
                            <td><a class="btn btn-outline-secondary btn-sm" href="/campaigns/@campaign.Id/character">@L["Campaigns.OpenCharacter"]</a></td>
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

    protected override async Task OnInitializedAsync()
    {
        var result = await CampaignService.GetMineAsync();
        _campaigns = result?.Data?.ToList() ?? [];
        _loading = false;
    }
}
```

- [ ] **Step 6: Create `PlayerCharacter.razor`**

```razor
@page "/campaigns/{CampaignId:guid}/character"
@attribute [Authorize(Roles = "Player")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@inject IStringLocalizer<AppStrings> L
@inject ICharacterSheetClientService SheetService

<PageTitle>@L["Sheet.Tab.Identity"] — RUPTURA</PageTitle>

<div class="page-content">
    @if (_loading)
    {
        <div class="ledger-empty"><span class="spinner-border spinner-border-sm me-2"></span>@L["Common.Loading"]</div>
    }
    else if (_sheetId is null)
    {
        <div class="ledger-empty"><p>@L["Character.AwaitingGrant"]</p></div>
    }
    else
    {
        <CharacterSheetEditor SheetId="_sheetId.Value" CampaignId="CampaignId" CanEditStatus="false" />
    }
</div>

@code {
    [Parameter] public Guid CampaignId { get; set; }

    private bool _loading = true;
    private Guid? _sheetId;

    protected override async Task OnInitializedAsync()
    {
        var result = await SheetService.GetMineAsync(CampaignId);
        _sheetId = result?.Data?.Id;
        _loading = false;
    }
}
```

- [ ] **Step 7: Add the player nav link**

In `src/Ruptura.Web/Layout/NavMenu.razor`, inside the `<Authorized>` block, alongside the existing "Dashboard" link and before the `AuthorizeView Roles="GameMaster"` block:

```razor
            <AuthorizeView Roles="Player">
                <Authorized Context="playerCtx">
                    <NavLink class="nav-link" href="/campaigns">
                        @L["Nav.Campaigns.Player"]
                    </NavLink>
                </Authorized>
            </AuthorizeView>
```

- [ ] **Step 8: Build**

Run: `dotnet build`
Expected: no errors.

- [ ] **Step 9: Commit**

```bash
git add src/Ruptura.Web/Pages/CharacterSheetTrialTab.razor src/Ruptura.Web/Pages/CharacterSheetGuildRegistryTab.razor \
  src/Ruptura.Web/Pages/PlayerCampaigns.razor src/Ruptura.Web/Pages/PlayerCharacter.razor \
  src/Ruptura.Web/Pages/CharacterSheetEditor.razor src/Ruptura.Web/Layout/NavMenu.razor \
  src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: add Trial/GuildRegistry tabs and player-facing campaign/character pages"
```

## Task 18: GM character sheet page + `GmCampaignDetail` grant/list UI + end-to-end flow test

**Files:**
- Create: `src/Ruptura.Web/Pages/GmCharacterSheet.razor`
- Modify: `src/Ruptura.Web/Pages/GmCampaignDetail.razor`
- Modify: `src/Ruptura.Web/Pages/GmCatalog.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx` / `.pt-BR.resx`
- Create: `tests/Ruptura.IntegrationTests/Controllers/CharacterSheetFlowTests.cs`

**Interfaces:**
- Consumes: `ICharacterSheetClientService` (Task 13), `CharacterSheetEditor` (Task 14), `ICatalogClientService.GetByTypeAsync(..., includeArchived:)` (Task 13).
- Produces: nothing new for later tasks — this is the last task in the plan.

- [ ] **Step 1: Add localization keys**

`AppStrings.resx`:

```xml
  <data name="Gm.CampaignDetail.Characters"><value>Characters</value></data>
  <data name="Gm.CampaignDetail.GrantCharacter"><value>Grant Character</value></data>
  <data name="Gm.CampaignDetail.CharacterNamePlaceholder"><value>Character name</value></data>
  <data name="Gm.CampaignDetail.Col.Owner"><value>Player</value></data>
  <data name="Gm.CampaignDetail.Col.Status"><value>Status</value></data>
  <data name="Gm.CampaignDetail.Status.Alive"><value>Alive</value></data>
  <data name="Gm.CampaignDetail.Status.Dead"><value>Dead</value></data>
  <data name="Gm.CampaignDetail.Status.Retired"><value>Retired</value></data>
  <data name="Gm.CampaignDetail.OpenSheet"><value>Open</value></data>
  <data name="Gm.Catalog.Archived"><value>Archived</value></data>
```

`AppStrings.pt-BR.resx`:

```xml
  <data name="Gm.CampaignDetail.Characters"><value>Personagens</value></data>
  <data name="Gm.CampaignDetail.GrantCharacter"><value>Conceder Personagem</value></data>
  <data name="Gm.CampaignDetail.CharacterNamePlaceholder"><value>Nome do personagem</value></data>
  <data name="Gm.CampaignDetail.Col.Owner"><value>Jogador</value></data>
  <data name="Gm.CampaignDetail.Col.Status"><value>Estado</value></data>
  <data name="Gm.CampaignDetail.Status.Alive"><value>Vivo</value></data>
  <data name="Gm.CampaignDetail.Status.Dead"><value>Morto</value></data>
  <data name="Gm.CampaignDetail.Status.Retired"><value>Aposentado</value></data>
  <data name="Gm.CampaignDetail.OpenSheet"><value>Abrir</value></data>
  <data name="Gm.Catalog.Archived"><value>Arquivado</value></data>
```

- [ ] **Step 2: Create `GmCharacterSheet.razor`**

```razor
@page "/gm/campaigns/{CampaignId:guid}/character-sheets/{SheetId:guid}"
@attribute [Authorize(Roles = "GameMaster")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@inject IStringLocalizer<AppStrings> L

<PageTitle>@L["Sheet.Tab.Identity"] — RUPTURA</PageTitle>

<div class="page-content">
    <CharacterSheetEditor SheetId="SheetId" CampaignId="CampaignId" CanEditStatus="true" />
</div>

@code {
    [Parameter] public Guid CampaignId { get; set; }
    [Parameter] public Guid SheetId { get; set; }
}
```

- [ ] **Step 3: Add a "Characters" section to `GmCampaignDetail.razor`**

Add to the `@code` block's fields:

```csharp
    private List<CharacterSheetResponse> _sheets = [];
    private string _newCharacterName = string.Empty;
    private bool _granting;
```

Add `@using Ruptura.Shared.CharacterSheets` and `@inject ICharacterSheetClientService SheetService` near the top, alongside the existing `@inject ICampaignClientService CampaignService`.

Extend `LoadAsync` to also fetch sheets:

```csharp
        var sheetsResult = await SheetService.GetByCampaignAsync(Id);
        _sheets = sheetsResult?.Data?.ToList() ?? [];
```

Add a `GrantAsync` method:

```csharp
    private async Task GrantAsync()
    {
        if (_selectedPlayerId == Guid.Empty || string.IsNullOrWhiteSpace(_newCharacterName)) return;

        _granting = true;
        _errorMessage = null;

        var result = await SheetService.GrantAsync(Id, new GrantCharacterSheetRequest
        {
            PlayerId = _selectedPlayerId, CharacterName = _newCharacterName
        });

        if (result?.Data is not null)
        {
            _newCharacterName = string.Empty;
            await LoadAsync();
        }
        else
        {
            _errorMessage = result?.Message ?? L["Common.Error"];
        }

        _granting = false;
    }
```

Add markup for the Characters section, right after the existing members `<table>`'s closing `</div>` (the `ledger-table-wrap` block) and before the final `@if (!_loading && _members.Count > 0 ...)` paragraph:

```razor
    <div class="section-header" style="margin-top:2rem">
        <span class="section-title">@L["Gm.CampaignDetail.Characters"]</span>
    </div>

    @if (_sheets.Count > 0)
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table">
                <thead>
                    <tr>
                        <th>@L["Gm.CampaignDetail.Col.Name"]</th>
                        <th>@L["Gm.CampaignDetail.Col.Owner"]</th>
                        <th>@L["Gm.CampaignDetail.Col.Status"]</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var sheet in _sheets)
                    {
                        <tr>
                            <td>@sheet.CharacterName</td>
                            <td>@(_members.FirstOrDefault(m => m.PlayerId == sheet.OwnerId)?.DisplayName ?? sheet.OwnerId.ToString())</td>
                            <td>@(sheet.IsDead ? L["Gm.CampaignDetail.Status.Dead"] : sheet.IsRetired ? L["Gm.CampaignDetail.Status.Retired"] : L["Gm.CampaignDetail.Status.Alive"])</td>
                            <td>
                                <a class="btn btn-outline-secondary btn-sm" href="/gm/campaigns/@Id/character-sheets/@sheet.Id">
                                    @L["Gm.CampaignDetail.OpenSheet"]
                                </a>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }

    @if (_members.Count > 0)
    {
        <div style="display:flex;gap:.5rem;margin-top:1rem;max-width:480px">
            <select class="form-select" @bind="_selectedPlayerId">
                <option value="@Guid.Empty">—</option>
                @foreach (var member in _members)
                {
                    <option value="@member.PlayerId">@member.DisplayName</option>
                }
            </select>
            <input class="form-control" placeholder="@L["Gm.CampaignDetail.CharacterNamePlaceholder"]"
                   @bind="_newCharacterName" @bind:event="oninput" />
            <button class="btn btn-primary btn-sm" @onclick="GrantAsync"
                    disabled="@(_granting || _selectedPlayerId == Guid.Empty || string.IsNullOrWhiteSpace(_newCharacterName))">
                @if (_granting) { <span class="spinner-border spinner-border-sm me-1"></span> }
                @L["Gm.CampaignDetail.GrantCharacter"]
            </button>
        </div>
    }
```

`_selectedPlayerId` already exists on this page (reused from the member-assignment autocomplete above) — this reuses the same field for a second purpose (picking who to grant a character to), which is fine since they're never both in use in the same interaction (a GM either assigns a member or grants a character, not both in the same click). If that reuse reads as confusing when actually looking at the diff, rename this task's new dropdown to a fresh `_selectedGrantPlayerId` field instead — either is acceptable, but pick one and don't leave both a reused and a duplicate field half-wired.

- [ ] **Step 4: Update `GmCatalog.razor` to show archived entries and request them**

Change the `LoadAsync` call:

```csharp
        var result = await CatalogService.GetByTypeAsync(_selectedType, CampaignId, includeArchived: true);
```

Change the row rendering so archived entries show a badge instead of Edit/Delete buttons:

```razor
                            <td>
                                @if (entry.IsArchived)
                                {
                                    <span style="color:var(--text-muted)">@L["Gm.Catalog.Archived"]</span>
                                }
                                else if (!entry.IsGlobal)
                                {
                                    <button class="btn btn-outline-secondary btn-sm" @onclick="() => StartEdit(entry)">@L["Gm.Catalog.Edit"]</button>
                                    <button class="btn btn-outline-secondary btn-sm" @onclick="() => DeleteAsync(entry.Id)">@L["Gm.Catalog.Delete"]</button>
                                }
                            </td>
```

(This replaces the existing `@if (!entry.IsGlobal) { ... }` block — same two buttons, now nested inside the new archived/not-archived check.)

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: no errors.

- [ ] **Step 6: Write the end-to-end flow integration test**

```csharp
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class CharacterSheetFlowTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    [Fact]
    public async Task FullFlow_GrantEditModulesMarkDeadGrantReplacement_Succeeds()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "E2E Campaign" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        // 1. Register a player via invite and assign to the campaign.
        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        var playerId = player.User.Id;
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = playerId });

        // 2. Grant a character.
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        grantResponse.EnsureSuccessStatusCode();
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        sheet.DerivedStats.Np.Should().Be(0); // no attributes above 1, no skills/talents/equipment yet

        // 3. Read a real Skill from the official catalog and invest points in it.
        var skillsResponse = await client.GetAsync($"api/catalog?type=Skill&campaignId={campaign.Id}");
        var skill = (await skillsResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<Ruptura.Shared.Catalog.CatalogEntryResponse>>>())!
            .Data!.First(s => s.Name == "Espadas");

        sheet.Data.Attributes.Controle = 3;
        sheet.Data.Skills.Add(new CharacterSkillEntry { CatalogEntryId = skill.Id, Points = 25 });

        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName,
            DataJson = System.Text.Json.JsonSerializer.Serialize(sheet.Data)
        });
        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        updated.DerivedStats.Np.Should().BeGreaterThan(0);
        updated.DerivedStats.SkillGradeBonuses[skill.Id].Should().Be(1); // 25 points → Adepto → +1

        // 4. GM marks the character dead.
        var killResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = updated.CharacterName,
            DataJson = System.Text.Json.JsonSerializer.Serialize(updated.Data),
            IsDead = true
        });
        killResponse.EnsureSuccessStatusCode();

        // 5. GM grants a replacement character for the same player — succeeds now that
        //    the first one is dead (the unique-alive index no longer blocks it).
        var replacementResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Dame Lysbet" });
        replacementResponse.EnsureSuccessStatusCode();

        // 6. GM's campaign detail sheet list shows both.
        var listResponse = await client.GetAsync($"api/campaigns/{campaign.Id}/character-sheets");
        var list = (await listResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CharacterSheetResponse>>>())!.Data!.ToList();
        list.Should().HaveCount(2);
        list.Should().Contain(s => s.CharacterName == "Sir Aldric" && s.IsDead);
        list.Should().Contain(s => s.CharacterName == "Dame Lysbet" && !s.IsDead);
    }
}
```

- [ ] **Step 7: Run the flow test**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullFlow_GrantEditModulesMarkDeadGrantReplacement_Succeeds`
Expected: PASS. Re-run once if it looks like the documented Serilog flake.

- [ ] **Step 8: Run the entire test suite one final time**

```bash
dotnet build
dotnet test tests/Ruptura.UnitTests
dotnet test tests/Ruptura.IntegrationTests
```

Expected: `dotnet build` clean; unit tests all PASS; integration tests all PASS (re-run once if 1-2 unrelated failures match the documented pre-existing Serilog flake — if the same test fails twice in a row, treat it as real).

- [ ] **Step 9: Commit**

```bash
git add src/Ruptura.Web/Pages/GmCharacterSheet.razor src/Ruptura.Web/Pages/GmCampaignDetail.razor \
  src/Ruptura.Web/Pages/GmCatalog.razor \
  src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx \
  tests/Ruptura.IntegrationTests/Controllers/CharacterSheetFlowTests.cs
git commit -m "feat: add GM character sheet page, grant UI, and end-to-end flow test"
```
