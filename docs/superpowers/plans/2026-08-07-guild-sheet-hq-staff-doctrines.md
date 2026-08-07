# Guild Sheet — Quartel-General, Pessoal & Doutrinas (Sub-plan #4) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let the guild manage its **buildings** (the construction tree — add/upgrade/activate installations, with server validation of the installation reference, level cap, one-per-type, and non-Portão), its **personnel** (worker/mercenary roster with GDD default salaries), and its **active doctrines** (selection enforced against the derived doctrine limit), surfaced through three new editor tabs.

**Architecture:** Buildings and staff are child entities with their own CRUD endpoints (mirror the #3 expeditions pattern), returned inside `GuildSheetResponse`. Doctrine selection edits `GuildSheetData.ActiveDoctrineIds` (blob) through the existing version-checked `PUT guild`, which gains server-side doctrine validation (ids are real Doctrines; count ≤ derived limit). Building writes validate against the seeded Installation catalog. CS overflow stays **advisory** (the calculator's `ActiveBuildingOverflow` flag + a UI warning — no hard block); prerequisites are **display-only** (shown from the installation's `DataJson`, never enforced) — both per explicit user decision.

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-08-07-guild-sheet-design.md` §3.2 (child entities), §4 (doctrine limit), §6, §7 (tabs 4, 5, 7), §12.4. Carry-in (memory `project_campaign_architecture.md` sub-plan #2 item 7a, sub-plan #3 item 7): **installation-reference validation is owed here**; `GuildStaffTypes` default salaries were deferred to this sub-plan.

## Global Constraints

- **Shared write, no field-level gating:** any campaign GM or member may add/edit/delete buildings, staff, and doctrines. Non-member → 404 (`Guild.NotFound`, hides existence). Reuse `GuildSheetService.AuthorizeAsync(callerId, campaignId, ct)` (from #3, returns the guild).
- **`Ruptura.Shared` must NOT reference `Ruptura.Domain`** — building/staff DTOs carry enum-like values as `string` (`GuildStaff.Kind` → `"Worker"`/`"Mercenary"`); the service maps string↔`GuildStaffKind` (`Enum.TryParse`, reject unknown with a validation error, never throw).
- **Building write validation (record-keeping posture):** on add/upgrade, validate — (a) the `CatalogEntryId` resolves to a `CatalogEntry` of `Type == Installation` **visible to the campaign** (`CampaignId == null` official OR `== campaignId` homebrew) and **not archived**, else `Guild.InstallationInvalid`; (b) the installation is not `NonConstructible` (Portão), else `Guild.BuildingNotConstructible`; (c) `Level` is `1..LevelCap` (from the installation's `InstallationCatalogData`), else `Guild.BuildingLevelInvalid`; (d) one-per-type — a duplicate `(GuildSheetId, CatalogEntryId)` hits the `ux_guild_buildings_sheet_installation` index → catch `DbUpdateException` → `Guild.BuildingExists`. **Prerequisites are NOT validated** (display-only in the UI).
- **CS overflow is advisory, never blocking:** activating a building past CS succeeds; the calculator's `ActiveBuildingOverflow` + the UI warning communicate it (GDD §10.9 "excedentes ficam Inativas sem benefício" — the app doesn't hard-block).
- **Doctrine validation (blob `PUT guild`):** every id in `ActiveDoctrineIds` must resolve to a `Doctrine` catalog entry visible to the campaign, else `Guild.DoctrineInvalid`; `ActiveDoctrineIds.Count` must be ≤ the derived doctrine limit (`min(4, 2 + Câmara do Conselho level)`, computed from the guild's buildings), else `Guild.DoctrineLimitExceeded`; `Identity.MainDoctrineId` (if set) must be a Doctrine, else `Guild.DoctrineInvalid`. This is service-level (needs the DB buildings), added to `UpdateAsync`.
- **Cross-guild safety:** update/delete of a building or staff row verifies its `GuildSheetId == the campaign's guild.Id`, else `Guild.NotFound`.
- **Default salaries are GDD-fixed** (§10.6.1): workers Operário 3, Artesão 8, Pesquisador 8, and (not GDD-specified → skilled default) Instrutor/Mercador/Médico/Administrador 8; mercenaries Bronze 10, Ferro 18, Aço 30, Prata 50, Ouro 80, Mithril 120, Adamante 170, Lendário 250. The client pre-fills from these but the value is overridable; the server stores whatever it's given (`DailySalary >= 0`).
- **Every visible string via `IStringLocalizer`**, in BOTH Web resx (en + pt-BR); API error strings in BOTH API resx.
- **Integration tests** use `IntegrationTestFactory`, `IClassFixture<>`, `parallelizeTestCollections: false`; lone Serilog flake = known race, re-run once.
- **Commit after each task** on `main`; end messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:**
- `src/Ruptura.Shared/Guilds/GuildBuildingResponse.cs`, `CreateBuildingRequest.cs`, `UpdateBuildingRequest.cs`, `GuildStaffResponse.cs`, `CreateStaffRequest.cs`, `UpdateStaffRequest.cs`, `GuildStaffReference.cs`
- `src/Ruptura.Web/Pages/GuildBuildingsTab.razor`, `GuildStaffTab.razor`, `GuildDoctrinesTab.razor`
- `tests/Ruptura.IntegrationTests/Guilds/GuildBuildingTests.cs`, `GuildStaffTests.cs`, `GuildDoctrineTests.cs`

**Modify:**
- `src/Ruptura.Shared/Guilds/GuildSheetResponse.cs` (+ `Buildings`, `Staff` lists)
- `src/Ruptura.Application/Common/ErrorCodes.cs` (+ building/doctrine codes)
- `src/Ruptura.Application/Interfaces/IGuildSheetService.cs` (+ building/staff CRUD)
- `src/Ruptura.Infrastructure/Services/GuildSheetService.cs` (building/staff CRUD, doctrine validation in `UpdateAsync`, include buildings/staff in response)
- `src/Ruptura.API/Controllers/GuildController.cs` (+ building/staff endpoints)
- `src/Ruptura.API/Resources/*.resx`, Web resx pair (strings)
- `src/Ruptura.Web/Pages/GuildSheet.razor` (mount the three new tabs)
- the Web guild client service (building/staff CRUD methods)

> Note: `IGuildBuildingRepository`/`IGuildStaffRepository` already exist (from #2) with `GetByGuildAsync`; add/update/delete come from `BaseRepository<T>` (`AddAsync`/`Update`/`Remove`/`SaveChangesAsync`) — no repository changes needed unless a helper is missing (check `BaseRepository`).

---

### Task 1: DTOs, staff reference data, response additions, error codes

**Files:** the six DTOs + `GuildStaffReference`; modify `GuildSheetResponse`, `ErrorCodes`.

**Interfaces:**
- Produces: `GuildBuildingResponse`, `CreateBuildingRequest`, `UpdateBuildingRequest`, `GuildStaffResponse`, `CreateStaffRequest`, `UpdateStaffRequest`, `GuildStaffReference`; `GuildSheetResponse.Buildings`/`.Staff`; the new `ErrorCodes.Guild.*`.

- [ ] **Step 1: Building DTOs**

`src/Ruptura.Shared/Guilds/GuildBuildingResponse.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public class GuildBuildingResponse
{
    public Guid Id { get; set; }
    public Guid CatalogEntryId { get; set; }
    public string InstallationName { get; set; } = string.Empty; // resolved from the catalog for display
    public int Level { get; set; }
    public bool IsActive { get; set; }
}
```
`src/Ruptura.Shared/Guilds/CreateBuildingRequest.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class CreateBuildingRequest
{
    [Required]
    public Guid CatalogEntryId { get; set; }   // an Installation catalog entry
    public int Level { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
```
`src/Ruptura.Shared/Guilds/UpdateBuildingRequest.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public class UpdateBuildingRequest
{
    public int Level { get; set; }
    public bool IsActive { get; set; }
}
```

- [ ] **Step 2: Staff DTOs**

`src/Ruptura.Shared/Guilds/GuildStaffResponse.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public class GuildStaffResponse
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;          // "Worker" | "Mercenary"
    public string TypeOrRanking { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DailySalary { get; set; }
    public bool IsActive { get; set; }
    public int? Efficiency { get; set; }
    public int? Morale { get; set; }
}
```
`src/Ruptura.Shared/Guilds/CreateStaffRequest.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class CreateStaffRequest
{
    [Required]
    public string Kind { get; set; } = string.Empty;          // "Worker" | "Mercenary"
    [Required, MaxLength(80)]
    public string TypeOrRanking { get; set; } = string.Empty;
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;
    [Range(0, int.MaxValue)]
    public int DailySalary { get; set; }
    public bool IsActive { get; set; } = true;
    public int? Efficiency { get; set; }
    public int? Morale { get; set; }
}
```
`src/Ruptura.Shared/Guilds/UpdateStaffRequest.cs` — same fields as `CreateStaffRequest` (Kind, TypeOrRanking, Name, DailySalary, IsActive, Efficiency, Morale) with the same annotations.

- [ ] **Step 3: Staff reference data**

`src/Ruptura.Shared/Guilds/GuildStaffReference.cs` — the fixed worker types (reuse `GuildStaffTypes`), the 8 mercenary rankings, and GDD default salaries so the client picker can pre-fill:
```csharp
namespace Ruptura.Shared.Guilds;

public static class GuildStaffReference
{
    public static readonly IReadOnlyList<string> WorkerTypes =
    [
        GuildStaffTypes.Operario, GuildStaffTypes.Artesao, GuildStaffTypes.Pesquisador,
        GuildStaffTypes.Instrutor, GuildStaffTypes.Mercador, GuildStaffTypes.Medico,
        GuildStaffTypes.Administrador
    ];

    // GDD §10.6.1 mercenary daily salaries by ranking (accented values are valid const/string).
    public static readonly IReadOnlyList<string> MercenaryRankings =
        ["Bronze", "Ferro", "Aço", "Prata", "Ouro", "Mithril", "Adamante", "Lendário"];

    // Default daily salary by type/ranking (Prata/dia). Workers: Operário 3, others skilled 8
    // (GDD fixes only Operário=3, Artesão/Pesquisador=8; the rest default to the skilled rate).
    public static readonly IReadOnlyDictionary<string, int> DefaultSalary = new Dictionary<string, int>
    {
        [GuildStaffTypes.Operario] = 3,
        [GuildStaffTypes.Artesao] = 8,
        [GuildStaffTypes.Pesquisador] = 8,
        [GuildStaffTypes.Instrutor] = 8,
        [GuildStaffTypes.Mercador] = 8,
        [GuildStaffTypes.Medico] = 8,
        [GuildStaffTypes.Administrador] = 8,
        ["Bronze"] = 10, ["Ferro"] = 18, ["Aço"] = 30, ["Prata"] = 50,
        ["Ouro"] = 80, ["Mithril"] = 120, ["Adamante"] = 170, ["Lendário"] = 250,
    };
}
```

- [ ] **Step 4: Response additions**

In `src/Ruptura.Shared/Guilds/GuildSheetResponse.cs`, add:
```csharp
    public List<GuildBuildingResponse> Buildings { get; set; } = [];
    public List<GuildStaffResponse> Staff { get; set; } = [];
```

- [ ] **Step 5: Error codes**

In `ErrorCodes.Guild`, add:
```csharp
        public const string InstallationInvalid = "Guild.InstallationInvalid";
        public const string BuildingNotConstructible = "Guild.BuildingNotConstructible";
        public const string BuildingLevelInvalid = "Guild.BuildingLevelInvalid";
        public const string BuildingExists = "Guild.BuildingExists";
        public const string BuildingNotFound = "Guild.BuildingNotFound";
        public const string StaffNotFound = "Guild.StaffNotFound";
        public const string StaffKindInvalid = "Guild.StaffKindInvalid";
        public const string DoctrineInvalid = "Guild.DoctrineInvalid";
        public const string DoctrineLimitExceeded = "Guild.DoctrineLimitExceeded";
```

- [ ] **Step 6: Build**

Run: `dotnet build` — PASS.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Shared/Guilds src/Ruptura.Application/Common/ErrorCodes.cs
git commit -m "feat: add guild building/staff DTOs, staff reference data, error codes

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: Building CRUD with installation validation

**Files:** modify `IGuildSheetService.cs`, `GuildSheetService.cs`, `GuildController.cs`, API resx; test `GuildBuildingTests.cs`.

**Interfaces:**
- Produces: `IGuildSheetService.AddBuildingAsync(callerId, campaignId, CreateBuildingRequest, ct)` → `Task<Result<GuildBuildingResponse>>`; `UpdateBuildingAsync(callerId, campaignId, buildingId, UpdateBuildingRequest, ct)`; `DeleteBuildingAsync(callerId, campaignId, buildingId, ct)` → `Task<Result>`.

- [ ] **Step 1: Write the failing tests**

`tests/Ruptura.IntegrationTests/Guilds/GuildBuildingTests.cs` — mirror `GuildExpeditionTests`. Cases:
```
- Member adds a valid installation (e.g. seeded Armazém id, level 2) -> 201; GET guild shows it in Buildings with InstallationName "Armazém"; DerivedStats.StorageCapacity == 100.
- Add a NON-installation catalog id (e.g. a seeded Skill) -> 400 Guild.InstallationInvalid.
- Add Portão (NonConstructible) -> 400 Guild.BuildingNotConstructible.
- Add with level above the installation's LevelCap (e.g. Câmara do Conselho level 5, cap 2) -> 400 Guild.BuildingLevelInvalid.
- Add the same installation twice -> second returns 400 Guild.BuildingExists.
- Update a building's level/active -> 200; reflected in GET + DerivedStats.
- Update level above cap -> 400.
- Delete a building -> 200/gone.
- Non-member add/update/delete -> 404. Cross-guild update/delete -> 404.
- Activate a building beyond CS -> 200 (NOT blocked), and DerivedStats.ActiveBuildingOverflow == true.
```

- [ ] **Step 2: Run → fail.**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildBuildingTests` → FAIL.

- [ ] **Step 3: Service interface**

Add to `IGuildSheetService.cs`:
```csharp
    Task<Result<GuildBuildingResponse>> AddBuildingAsync(Guid callerId, Guid campaignId, CreateBuildingRequest request, CancellationToken ct = default);
    Task<Result<GuildBuildingResponse>> UpdateBuildingAsync(Guid callerId, Guid campaignId, Guid buildingId, UpdateBuildingRequest request, CancellationToken ct = default);
    Task<Result> DeleteBuildingAsync(Guid callerId, Guid campaignId, Guid buildingId, CancellationToken ct = default);
```

- [ ] **Step 4: Implement in `GuildSheetService.cs`**

Inject `IGuildBuildingRepository buildingRepo` (already injected for reads in #2 — reuse). Add a private installation-validation helper and the three methods:
```csharp
    // Returns the InstallationCatalogData if the entry is a valid, visible, non-archived,
    // constructible Installation for this campaign; otherwise a failure with the right code.
    private async Task<Result<InstallationCatalogData>> ValidateInstallationAsync(
        Guid catalogEntryId, Guid campaignId, int level, CancellationToken ct)
    {
        var entry = await catalogRepo.GetByIdAsync(catalogEntryId, ct);
        if (entry is null || entry.Type != CatalogEntryType.Installation
            || entry.IsArchived
            || (entry.CampaignId is not null && entry.CampaignId != campaignId))
            return Result.Failure<InstallationCatalogData>(ErrorCodes.Guild.InstallationInvalid);

        InstallationCatalogData? data;
        try { data = JsonSerializer.Deserialize<InstallationCatalogData>(entry.DataJson); }
        catch (JsonException) { data = null; }
        if (data is null)
            return Result.Failure<InstallationCatalogData>(ErrorCodes.Guild.InstallationInvalid);
        if (data.NonConstructible)
            return Result.Failure<InstallationCatalogData>(ErrorCodes.Guild.BuildingNotConstructible);
        if (level < 1 || level > data.LevelCap)
            return Result.Failure<InstallationCatalogData>(ErrorCodes.Guild.BuildingLevelInvalid);
        return Result.Success(data);
    }

    public async Task<Result<GuildBuildingResponse>> AddBuildingAsync(
        Guid callerId, Guid campaignId, CreateBuildingRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure) return Result.Failure<GuildBuildingResponse>(auth.Error!);
        var guild = auth.Value!;

        var validation = await ValidateInstallationAsync(request.CatalogEntryId, campaignId, request.Level, ct);
        if (validation.IsFailure) return Result.Failure<GuildBuildingResponse>(validation.Error!);

        var building = new GuildBuilding
        {
            Id = Guid.NewGuid(), GuildSheetId = guild.Id,
            CatalogEntryId = request.CatalogEntryId, Level = request.Level, IsActive = request.IsActive
        };
        try
        {
            await buildingRepo.AddAsync(building, ct);
            await buildingRepo.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // ux_guild_buildings_sheet_installation — one building per installation type.
            return Result.Failure<GuildBuildingResponse>(ErrorCodes.Guild.BuildingExists);
        }
        return Result.Success(await MapBuildingAsync(building, ct));
    }

    public async Task<Result<GuildBuildingResponse>> UpdateBuildingAsync(
        Guid callerId, Guid campaignId, Guid buildingId, UpdateBuildingRequest request, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure) return Result.Failure<GuildBuildingResponse>(auth.Error!);
        var guild = auth.Value!;

        var building = await buildingRepo.GetByIdAsync(buildingId, ct);
        if (building is null || building.GuildSheetId != guild.Id)
            return Result.Failure<GuildBuildingResponse>(ErrorCodes.Guild.BuildingNotFound);

        var validation = await ValidateInstallationAsync(building.CatalogEntryId, campaignId, request.Level, ct);
        if (validation.IsFailure) return Result.Failure<GuildBuildingResponse>(validation.Error!);

        building.Level = request.Level;
        building.IsActive = request.IsActive;
        buildingRepo.Update(building);
        await buildingRepo.SaveChangesAsync(ct);
        return Result.Success(await MapBuildingAsync(building, ct));
    }

    public async Task<Result> DeleteBuildingAsync(
        Guid callerId, Guid campaignId, Guid buildingId, CancellationToken ct = default)
    {
        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure) return Result.Failure(auth.Error!);
        var guild = auth.Value!;

        var building = await buildingRepo.GetByIdAsync(buildingId, ct);
        if (building is null || building.GuildSheetId != guild.Id)
            return Result.Failure(ErrorCodes.Guild.BuildingNotFound);

        buildingRepo.Remove(building);
        await buildingRepo.SaveChangesAsync(ct);
        return Result.Success();
    }

    private async Task<GuildBuildingResponse> MapBuildingAsync(GuildBuilding b, CancellationToken ct)
    {
        var entry = await catalogRepo.GetByIdAsync(b.CatalogEntryId, ct);
        return new GuildBuildingResponse
        {
            Id = b.Id, CatalogEntryId = b.CatalogEntryId,
            InstallationName = entry?.Name ?? string.Empty,
            Level = b.Level, IsActive = b.IsActive
        };
    }
```
> Confirm `BaseRepository` exposes `AddAsync`/`Update`/`Remove`/`GetByIdAsync`/`SaveChangesAsync` (used across the codebase) — if `Remove` has a different name, match it. Add `using Ruptura.Domain.Enums;` (for `CatalogEntryType`) if not present. Include buildings in `MapToResponseAsync`: `response.Buildings = (await buildingRepo.GetByGuildAsync(guild.Id, ct)).Select(...).ToList()` — but that needs the catalog names; resolve them in one batch via `catalogRepo.GetByIdsAsync` rather than N `GetByIdAsync` calls (the installation dict is already loaded there for the calculator — reuse it to fill `InstallationName`).

- [ ] **Step 5: Controller endpoints**

In `GuildController.cs`, add `POST/PUT/DELETE campaigns/{campaignId:guid}/guild/buildings[/{buildingId:guid}]` (mirror the expedition endpoints; POST→201, PUT→200, DELETE→200, failures→404 except validation codes → 400). Map the building/doctrine `Guild.*` validation codes (`InstallationInvalid`, `BuildingNotConstructible`, `BuildingLevelInvalid`, `BuildingExists`) to **400**, and `BuildingNotFound`/`NotFound` to **404**. Add the resx strings for each new code (both API resx files).

- [ ] **Step 6: Run tests → pass; commit**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildBuildingTests` → PASS.
```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Guilds/GuildBuildingTests.cs
git commit -m "feat: add guild building CRUD with installation validation

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Staff CRUD

**Files:** modify `IGuildSheetService.cs`, `GuildSheetService.cs`, `GuildController.cs`, API resx; test `GuildStaffTests.cs`.

**Interfaces:**
- Produces: `AddStaffAsync`/`UpdateStaffAsync`/`DeleteStaffAsync(callerId, campaignId, [staffId,] <request>, ct)`.

- [ ] **Step 1: Write the failing tests**

`tests/Ruptura.IntegrationTests/Guilds/GuildStaffTests.cs`:
```
- Member adds a worker (Kind "Worker", TypeOrRanking "Operário", salary 3) -> 201; GET shows it in Staff; DerivedStats.WorkerIncomePerDay reflects it (Operário active -> +2), DailyMaintenance includes salary.
- Add a mercenary (Kind "Mercenary", "Bronze", salary 10) -> 201; maintenance includes it; NOT counted as a worker.
- Add with Kind "Nonsense" -> 400 Guild.StaffKindInvalid.
- Update salary/active -> 200; maintenance reflects change (inactive staff excluded).
- Delete -> gone.
- Non-member -> 404; cross-guild update/delete -> 404.
```

- [ ] **Step 2: Run → fail.**

- [ ] **Step 3: Service methods**

Add to `IGuildSheetService.cs`: `AddStaffAsync`/`UpdateStaffAsync`/`DeleteStaffAsync` (signatures parallel to buildings, with `CreateStaffRequest`/`UpdateStaffRequest`, returning `Result<GuildStaffResponse>`/`Result`). Implement in `GuildSheetService.cs` (inject `IGuildStaffRepository staffRepo` — already injected for reads in #2). Parse `Kind` string → `GuildStaffKind` with `Enum.TryParse` (reject unknown → `Guild.StaffKindInvalid`, never throw). Map via a private `MapStaff(GuildStaff)` (`Kind = s.Kind.ToString()`). Cross-guild check on update/delete. Include staff in `MapToResponseAsync` (`response.Staff = (await staffRepo.GetByGuildAsync(guild.Id, ct)).Select(MapStaff).ToList()`).

- [ ] **Step 4: Controller endpoints**

`POST/PUT/DELETE campaigns/{campaignId:guid}/guild/staff[/{staffId:guid}]`, mirroring buildings. `StaffKindInvalid` → 400; `StaffNotFound`/`NotFound` → 404. Add resx strings.

- [ ] **Step 5: Run tests → pass; commit**

```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Guilds/GuildStaffTests.cs
git commit -m "feat: add guild staff (workers/mercenaries) CRUD

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 4: Doctrine validation on the blob update

**Files:** modify `GuildSheetService.cs` (`UpdateAsync`); test `GuildDoctrineTests.cs`.

**Interfaces:** no new public methods — extends `UpdateAsync` behavior.

- [ ] **Step 1: Write the failing tests**

`tests/Ruptura.IntegrationTests/Guilds/GuildDoctrineTests.cs`:
```
- Set ActiveDoctrineIds to two valid seeded Doctrine ids (limit is 2 with no Câmara do Conselho) -> PUT 200; GET shows them; DerivedStats reflect doctrine effects if applicable.
- Set ActiveDoctrineIds to three ids with no Câmara do Conselho (limit 2) -> 400 Guild.DoctrineLimitExceeded.
- Build a Câmara do Conselho level 2 (limit -> 4), then set 4 doctrines -> 200.
- Set an ActiveDoctrineId that is NOT a Doctrine (e.g. an Installation id) -> 400 Guild.DoctrineInvalid.
- Set Identity.MainDoctrineId to a non-Doctrine id -> 400 Guild.DoctrineInvalid.
- Empty ActiveDoctrineIds -> 200.
```

- [ ] **Step 2: Run → fail** (currently `UpdateAsync` accepts any doctrine ids).

- [ ] **Step 3: Add doctrine validation to `UpdateAsync`**

In `GuildSheetService.UpdateAsync`, after deserializing `incoming` and before saving, add:
```csharp
        var doctrineError = await ValidateDoctrinesAsync(incoming, guild.Id, campaignId, ct);
        if (doctrineError is not null)
            return Result.Failure<GuildSheetResponse>(doctrineError);
```
Implement the helper:
```csharp
    private async Task<string?> ValidateDoctrinesAsync(
        GuildSheetData data, Guid guildSheetId, Guid campaignId, CancellationToken ct)
    {
        var ids = new List<Guid>(data.ActiveDoctrineIds ?? []);
        if (data.Identity.MainDoctrineId is { } main) ids.Add(main);
        if (ids.Count == 0 && (data.ActiveDoctrineIds?.Count ?? 0) == 0) return null;

        // Every referenced id must be a Doctrine visible to this campaign.
        var distinct = ids.Distinct().ToList();
        if (distinct.Count > 0)
        {
            var entries = (await catalogRepo.GetByIdsAsync(distinct, ct)).ToDictionary(e => e.Id);
            foreach (var id in distinct)
            {
                if (!entries.TryGetValue(id, out var e) || e.Type != CatalogEntryType.Doctrine
                    || (e.CampaignId is not null && e.CampaignId != campaignId))
                    return ErrorCodes.Guild.DoctrineInvalid;
            }
        }

        // ActiveDoctrineIds count must be within the derived limit (min(4, 2 + Câmara level)).
        var buildings = (await buildingRepo.GetByGuildAsync(guildSheetId, ct)).ToList();
        var camaraLevel = buildings
            .Where(b => b.CatalogEntryId == GuildCatalogIds.CamaraDoConselho && b.IsActive)
            .Select(b => b.Level).FirstOrDefault();
        var limit = Math.Min(4, 2 + camaraLevel);
        if ((data.ActiveDoctrineIds?.Count ?? 0) > limit)
            return ErrorCodes.Guild.DoctrineLimitExceeded;

        return null;
    }
```
> `camaraLevel` uses `IsActive` to match the calculator's `LevelOf` (benefits exclude inactive buildings — sub-plan #2 rule). Map `Guild.DoctrineInvalid`/`DoctrineLimitExceeded` to **400** in the controller's `Update` action (they join the existing conflict/not-found mapping).

- [ ] **Step 4: Run tests → pass; commit**

```bash
git add src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Guilds/GuildDoctrineTests.cs
git commit -m "feat: validate active doctrines against the derived limit on guild update

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 5: Quartel-General, Pessoal & Doutrinas tabs (Blazor)

Add three tabs to the guild editor. Pattern-directive — match the existing tabs + `CharacterSheetJournalTab` (child-entity CRUD) + the catalog-picker pattern.

**Files:**
- Modify: `GuildSheet.razor` (mount the three tabs), the Web guild client service (building/staff CRUD methods), Web resx pair
- Create: `GuildBuildingsTab.razor`, `GuildStaffTab.razor`, `GuildDoctrinesTab.razor`

- [ ] **Step 1: Client methods**

Add building + staff CRUD to the Web guild client service (`AddBuildingAsync`/`UpdateBuildingAsync`/`DeleteBuildingAsync` and the staff trio), following the expedition client methods (surface non-200s so the tab can toast the localized error — validation 400s carry a message). Each mutation's caller refetches only the affected list + `DerivedStats` + `Version` (do NOT clobber `_data`/`_guildName` — same rule as the expeditions refresh from #3).

- [ ] **Step 2: `GuildBuildingsTab`**

Manages the `GuildBuilding` child entity. Requirements:
- Fetch the Installation catalog once (`CatalogClientService.GetByTypeAsync("Installation", campaignId)`) for the "add" picker; exclude `NonConstructible` entries (deserialize `InstallationCatalogData`) and installations already built (one-per-type). Keep the full list to resolve names for existing rows (archived-picker pattern).
- Table of current buildings (`_guild.Buildings`): name, level (editable number input, 1..cap), active toggle, delete (with `Confirm.AskAsync`). Show each installation's **prerequisites** and **build/upgrade cost** (`level × weight × 10` resources / `× 3` days, from `InstallationCatalogData.Weight`) as read-only display text.
- Add-row: installation `<select>` + level + active → `AddBuildingAsync`.
- Show a **CS overflow warning** when `_guild.DerivedStats.ActiveBuildingOverflow` (reuse the panel's warning style): "active buildings (N) exceed Support Capacity (CS)".
- `.ledger-table.stack-mobile` with `data-label`. All strings localized.

- [ ] **Step 3: `GuildStaffTab`**

Manages `GuildStaff`. Two sub-sections (Workers / Mercenaries) or one table with a Kind column. Add-row: Kind `<select>` (Worker/Mercenary, localized), then a TypeOrRanking `<select>` sourced from `GuildStaffReference.WorkerTypes` or `.MercenaryRankings` per Kind, Name, DailySalary (pre-filled from `GuildStaffReference.DefaultSalary` on type change, overridable), active toggle, optional Efficiency/Morale for workers. Edit/delete rows (`Confirm` before delete). Localized.

- [ ] **Step 4: `GuildDoctrinesTab`**

Edits `_data.ActiveDoctrineIds` (blob — saved via the host's top-level Save, like the other blob tabs) plus `_data.Identity.MainDoctrineId`. Fetch the Doctrine catalog (`GetByTypeAsync("Doctrine", campaignId)`). Render the 8 doctrines as checkboxes bound to `ActiveDoctrineIds`; show the **doctrine limit** (`_guild.DerivedStats.ActiveDoctrineCount / DoctrineLimit`) and disable further checks (or warn) once at the limit — but the server is authoritative (a Save over the limit returns 400 with the localized `Guild.DoctrineLimitExceeded` message). MainDoctrine `<select>` of the doctrines. Show each doctrine's `Bonus` text. Localized.

- [ ] **Step 5: Mount tabs + i18n**

In `GuildSheet.razor`, add the three tabs to the strip (Quartel-General, Pessoal, Doutrinas) alongside Capacidades + the #3 record tabs. Add all new strings to BOTH Web resx (tab titles, column labels, Kind/Worker/Mercenary names, cost/prereq labels, overflow warning, doctrine-limit label, add/remove, the mapped API error messages). English default + pt-BR.

- [ ] **Step 6: Build + verify + commit**

Run: `dotnet build` (clean). If feasible, run the app and confirm: add/upgrade a building updates the Capacidades numbers; activating past CS shows the warning; adding staff changes maintenance; selecting doctrines over the limit is rejected with the localized message. If a live run isn't feasible, confirm a clean build and note it.
```bash
git add src/Ruptura.Web
git commit -m "feat: add guild Quartel-General, Pessoal, and Doutrinas editor tabs

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage (§3.2, §4 doctrine limit, §7 tabs 4/5/7, §12.4):**
- Buildings (construction tree, CS validation advisory, cost/prereq display) → Task 2 + Task 5 tab. Installation-reference validation (the owed carry-in) → Task 2 `ValidateInstallationAsync`. ✓
- Staff (workers/mercenaries, default salaries) → Task 1 `GuildStaffReference` + Task 3 + Task 5 tab. ✓
- Doctrines (selection + derived-limit enforcement) → Task 4 + Task 5 tab. ✓
- Buildings/Staff surfaced in the response → Task 1 + Tasks 2/3 mapping. ✓
- **Decisions honored:** CS overflow advisory (no block); prerequisites display-only; validation limited to existence/visibility/level-cap/one-per-type/non-Portão. `IsActive` matches the calculator's benefit-exclusion rule (Câmara level for doctrine limit uses active only).
- **Deliberately deferred (not gaps):** Recursos per-material valuation (spec §11 item 3, still open — untouched here); TypeOrRanking not hard-validated against the fixed list (record-keeping posture — the UI picker supplies canonical values; a free string is harmless, and the calculator already treats a non-"Operário" worker as non-income); worker Efficiency/Morale have no mechanical effect yet (display/record only, per the entity's existing shape).

**2. Placeholder scan:** Backend Tasks 1–4 carry complete code (service methods, validation helpers). Controller endpoints (Tasks 2/3 Step 5) and UI (Task 5) are described structurally with the exact endpoints/DTOs/patterns to mirror (`GuildExpeditionTests`/expedition endpoints from #3; `CharacterSheetJournalTab`) — concrete Razor/client conventions read from the repo at execution, same posture as prior sub-plans' UI tasks. No "TBD"/"handle appropriately".

**3. Type consistency:** `AddBuildingAsync`/`UpdateBuildingAsync`/`DeleteBuildingAsync` and the staff trio have consistent `(callerId, campaignId, [id,] request, ct)` shapes across interface, impl, controller. `GuildBuildingResponse`/`CreateBuildingRequest`/`UpdateBuildingRequest` and staff DTOs match the `GuildBuilding`/`GuildStaff` entities. `Kind` is `string` in DTOs, mapped to `GuildStaffKind` in the service. `GuildCatalogIds.CamaraDoConselho` (from #2) reused for the doctrine-limit computation. `GuildSheetResponse.Buildings`/`.Staff` consumed by the mapping and the UI. `ValidateInstallationAsync` returns `Result<InstallationCatalogData>` used by add + update.
