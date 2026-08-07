# Guild Sheet — Record-Keeping Modules (Sub-plan #3) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the guild sheet's first WRITE path — editing the blob record-keeping modules (Identidade, Recursos, Prestígio, Influência, Conhecimento, Legado), the Expedições log (child entity CRUD), and emblem upload — with real optimistic-concurrency enforcement (Postgres `xmin`) so concurrent shared-write edits can't silently clobber each other.

**Architecture:** A whole-blob `PUT /api/campaigns/{id}/guild` (mirrors `CharacterSheetService.UpdateAsync`) replaces `GuildSheet.DataJson` and `GuildName`, enforcing the client-supplied `Version` (xmin) as the concurrency token — a stale write returns 409 instead of a lost update. `EmblemImagePath` is set only via `POST /api/media` (never the general update payload), following the `PortraitImagePath` precedent. Expeditions are a dedicated child entity with their own CRUD endpoints, returned inside `GuildSheetResponse`. The Blazor guild page becomes a tabbed editor (the Capacidades panel from #2 stays as one read-only tab).

**Tech Stack:** .NET 8, EF Core 8 + Npgsql (xmin concurrency token), Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-08-07-guild-sheet-design.md` §3.2/§3.4 (data), §5 n/a, §6 (permissions + concurrency), §7 (tabs 1–3, 6, 8, 9), §11.4 (emblem), §12.3. Carry-in notes: `project_campaign_architecture.md` "sub-plan #2" section items 2 (deepen blob guard), 3 (enforce xmin Version + cross-request test), 7a (validate installation refs — NOT here, that's #4), 7b (Recursos inflation — see §Global Constraints).

## Global Constraints

- **Shared write, no field-level gating:** any campaign GM or member may edit every blob module and every expedition (spec §2 decision 8). There is NO GM-only field in the guild blob (unlike CharacterSheet's Ranking) — so the update does a straight whole-blob replace with two server-authoritative exceptions below.
- **`EmblemImagePath` is server-authoritative:** the general update MUST preserve the stored `Identity.EmblemImagePath` and ignore any value in the incoming payload (exactly like `CharacterSheet.PortraitImagePath`). Emblem changes happen only through `POST /api/media`.
- **Optimistic concurrency via `xmin` `Version`:** the update requires the client's `Version` and enforces it as the concurrency token (`db.Entry(guild).Property(g => g.Version).OriginalValue = request.Version`); a mismatch → `DbUpdateConcurrencyException` → `ErrorCodes.Guild.Conflict` → HTTP 409. This is the load-bearing improvement over the character sheet's known lost-update gap — the integration test MUST exercise the **cross-request** stale-write path (load v1 → other write bumps to v2 → save with v1 → 409).
- **Deepen the blob non-null guard:** sub-plan #2's `GuildSheetService.Deserialize` guards only one level. This plan binds `Knowledge.*` and `Influence[]` in the UI, so extend the guard to guarantee every `GuildKnowledge` list (`Maps`, `Recipes`, `CataloguedEnemies`, `DefeatedBosses`, `HistoricalRecords`) is non-null. (`Influence` is a `List<InfluenceRelation>` already guarded at the top level; its elements are non-null by List semantics.)
- **`Expedition.Date` is `timestamptz`:** normalize any incoming `DateTime` to UTC at the service boundary (`DateTime.SpecifyKind(value, DateTimeKind.Utc)` if `Kind != Utc`) — Npgsql throws on a non-UTC `Kind`.
- **`Ruptura.Shared` must NOT reference `Ruptura.Domain`** (clean-architecture boundary; established convention — existing DTOs expose enums as `string`, e.g. `CatalogEntryResponse.Type`). Expedition DTOs carry `Kind` as `string` ("Principal"/"Secundaria"); the service maps string↔`ExpeditionKind`. Do not add a `Shared→Domain` ProjectReference.
- **`DataJson` is sent as a string** in the request (client serializes `GuildSheetData`), matching `UpdateCharacterSheetRequest`. Blob deserialization uses `JsonSerializerDefaults.Web`; catalog `DataJson` (unchanged here) uses default options — do not unify.
- **Every visible string via `IStringLocalizer`**, added to BOTH Web resx files (English default + pt-BR). API error strings in BOTH API resx files.
- **Integration tests** use `WebApplicationFactory<Program>` + Testcontainers (`IntegrationTestFactory`, `IClassFixture<>`, `parallelizeTestCollections: false`). Lone Serilog "logger already frozen" flake = known pre-existing race; re-run once.
- **Commit after each task** on `main`; end commit messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.
- **Recursos inflation (spec §11 item 3) stays deferred:** this task ships the Recursos editor, which lets a user type arbitrary material quantities that inflate CG. Do NOT add a per-material valuation rule here — it remains out of scope (CG Recursos = PactCoins + fragments + Σ quantities as locked in #2). Just note it in the task's report so it isn't mistaken for an oversight.

## File Structure

**Create:**
- `src/Ruptura.Shared/Guilds/UpdateGuildSheetRequest.cs`, `ExpeditionResponse.cs`, `CreateExpeditionRequest.cs`, `UpdateExpeditionRequest.cs`
- `src/Ruptura.Application/Interfaces/IExpeditionRepository.cs`
- `src/Ruptura.Infrastructure/Repositories/ExpeditionRepository.cs`
- `src/Ruptura.Infrastructure/Validators/UpdateGuildSheetRequestValidator.cs` (follow the existing validator folder/namespace)
- `src/Ruptura.Web/Pages/GuildIdentityTab.razor`, `GuildResourcesTab.razor`, `GuildInfluenceTab.razor`, `GuildKnowledgeTab.razor`, `GuildLegacyTab.razor`, `GuildExpeditionsTab.razor`
- `tests/Ruptura.IntegrationTests/Guilds/GuildUpdateTests.cs`, `GuildExpeditionTests.cs`, `GuildEmblemTests.cs`

**Modify:**
- `src/Ruptura.Shared/Guilds/GuildSheetResponse.cs` (add `List<ExpeditionResponse> Expeditions`)
- `src/Ruptura.Application/Common/ErrorCodes.cs` (add `Guild.Conflict`)
- `src/Ruptura.Application/Interfaces/IGuildSheetService.cs` (add update, expedition CRUD, emblem-auth, set-emblem methods)
- `src/Ruptura.Application/Interfaces/IGuildSheetRepository.cs` (add `SetExpectedVersion`)
- `src/Ruptura.Infrastructure/Repositories/GuildSheetRepository.cs` (impl `SetExpectedVersion`)
- `src/Ruptura.Infrastructure/Services/GuildSheetService.cs` (update, expeditions, emblem, deeper deserialize, expeditions in response)
- `src/Ruptura.API/Controllers/GuildController.cs` (PUT + expedition endpoints)
- `src/Ruptura.API/Controllers/MediaController.cs` (guild emblem upload + download branches)
- `src/Ruptura.Domain/Enums/MediaEntityType.cs` (add `GuildEmblem`)
- `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` (register `IExpeditionRepository`, validator)
- `src/Ruptura.API/Resources/*.resx` + `src/Ruptura.Web/**/*.resx` (strings)
- `src/Ruptura.Web/Pages/GuildSheet.razor` (turn into a tabbed editor)
- the Web guild client service (add update + expedition + emblem-upload methods)

---

### Task 1: Request/response DTOs, error code, validator

Scaffolding folded into one build-verified task.

**Files:** Create the four DTOs + the validator; modify `GuildSheetResponse`, `ErrorCodes`, register the validator in DI.

**Interfaces:**
- Produces: `UpdateGuildSheetRequest { GuildName, DataJson, Version }`; `ExpeditionResponse`; `CreateExpeditionRequest`; `UpdateExpeditionRequest`; `GuildSheetResponse.Expeditions`; `ErrorCodes.Guild.Conflict`.

- [ ] **Step 1: `UpdateGuildSheetRequest`**

`src/Ruptura.Shared/Guilds/UpdateGuildSheetRequest.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class UpdateGuildSheetRequest
{
    [Required, MinLength(1), MaxLength(120)]
    public string GuildName { get; set; } = string.Empty;

    [Required]
    public string DataJson { get; set; } = "{}";

    // xmin concurrency token the client last read (GuildSheetResponse.Version). Enforced on save.
    public uint Version { get; set; }
}
```

- [ ] **Step 2: Expedition DTOs**

`src/Ruptura.Shared/Guilds/ExpeditionResponse.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public class ExpeditionResponse
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty; // "Principal" | "Secundaria" — string, NOT the Domain enum (Shared must not reference Domain)
    public DateTime Date { get; set; }
    public string Participants { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Losses { get; set; } = string.Empty;
    public string ResourcesGained { get; set; } = string.Empty;
}
```
`src/Ruptura.Shared/Guilds/CreateExpeditionRequest.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public class CreateExpeditionRequest
{
    public string Kind { get; set; } = string.Empty; // "Principal" | "Secundaria" — string, NOT the Domain enum (Shared must not reference Domain)
    public DateTime Date { get; set; }
    public string Participants { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Losses { get; set; } = string.Empty;
    public string ResourcesGained { get; set; } = string.Empty;
}
```
`src/Ruptura.Shared/Guilds/UpdateExpeditionRequest.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public class UpdateExpeditionRequest
{
    public string Kind { get; set; } = string.Empty; // "Principal" | "Secundaria" — string, NOT the Domain enum (Shared must not reference Domain)
    public DateTime Date { get; set; }
    public string Participants { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Losses { get; set; } = string.Empty;
    public string ResourcesGained { get; set; } = string.Empty;
}
```

- [ ] **Step 3: Add `Expeditions` to the response**

In `src/Ruptura.Shared/Guilds/GuildSheetResponse.cs`, add:
```csharp
    public List<ExpeditionResponse> Expeditions { get; set; } = [];
```

- [ ] **Step 4: Add the error code**

In `src/Ruptura.Application/Common/ErrorCodes.cs`, inside `Guild`:
```csharp
        public const string Conflict = "Guild.Conflict";
```

- [ ] **Step 5: Validator**

`src/Ruptura.Infrastructure/Validators/UpdateGuildSheetRequestValidator.cs` — follow the existing FluentValidation validators (e.g. `CreateCatalogEntryRequestValidator`) for namespace/style:
```csharp
using FluentValidation;
using Ruptura.Shared.Guilds;

namespace Ruptura.Infrastructure.Validators;

public class UpdateGuildSheetRequestValidator : AbstractValidator<UpdateGuildSheetRequest>
{
    public UpdateGuildSheetRequestValidator()
    {
        RuleFor(x => x.GuildName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DataJson).NotEmpty();
    }
}
```

- [ ] **Step 6: Register the validator**

In `InfrastructureExtensions.cs`, alongside the other `IValidator<>` registrations:
```csharp
        services.AddScoped<IValidator<UpdateGuildSheetRequest>, UpdateGuildSheetRequestValidator>();
```

- [ ] **Step 7: Build**

Run: `dotnet build` — PASS.

- [ ] **Step 8: Commit**

```bash
git add src/Ruptura.Shared/Guilds src/Ruptura.Application/Common/ErrorCodes.cs src/Ruptura.Infrastructure/Validators src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs
git commit -m "feat: add guild update/expedition DTOs, conflict error, validator

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: Blob update with xmin concurrency enforcement

The delicate heart. TDD, with the cross-request conflict test as the centerpiece.

**Files:**
- Modify: `IGuildSheetRepository.cs` (+ `SetExpectedVersion`), `GuildSheetRepository.cs`, `IGuildSheetService.cs` (+ `UpdateAsync`), `GuildSheetService.cs`, `GuildController.cs`
- Test: `tests/Ruptura.IntegrationTests/Guilds/GuildUpdateTests.cs`

**Interfaces:**
- Consumes: `UpdateGuildSheetRequest`, `GuildSheetResponse`, `ErrorCodes.Guild.Conflict`.
- Produces: `IGuildSheetService.UpdateAsync(Guid callerId, Guid campaignId, UpdateGuildSheetRequest request, CancellationToken) → Task<Result<GuildSheetResponse>>`; `IGuildSheetRepository.SetExpectedVersion(GuildSheet guild, uint expectedVersion)`.

- [ ] **Step 1: Write the failing integration tests**

`tests/Ruptura.IntegrationTests/Guilds/GuildUpdateTests.cs` — mirror `GuildControllerTests` fixture/token setup. Cases:
```
1. GM updates GuildName + a blob field (e.g. Prestige.Value, PatronDeity), sends the Version from a prior GET -> 200, response reflects changes, Version advanced.
2. Member updates -> 200 (shared write).
3. Non-member -> 404.
4. CROSS-REQUEST CONFLICT (the load-bearing test): GET guild as GM -> Version v1. Perform a second successful update (bumps to v2). Then PUT with Version v1 -> 409 (Guild.Conflict); confirm the v1 payload's changes did NOT persist (GET shows the v2 state).
5. Emblem preserved: directly set Identity.EmblemImagePath via a media upload OR seed it, then PUT a payload whose DataJson has EmblemImagePath="" -> 200, and a subsequent GET shows the ORIGINAL emblem path (general update ignored the client's emblem value).
6. Deep-guard: PUT DataJson with `{"knowledge":{"maps":null}}` -> 200 (no 500), GET returns Maps as [].
```
Write concrete arrange/act/assert (serialize a `GuildSheetData`, send as `DataJson`; read `Version` from the GET response).

- [ ] **Step 2: Run → fail (no UpdateAsync/PUT)**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildUpdateTests`
Expected: FAIL to compile / 404.

- [ ] **Step 3: Add `SetExpectedVersion` to the repository**

In `IGuildSheetRepository.cs`:
```csharp
    void SetExpectedVersion(GuildSheet guild, uint expectedVersion);
```
In `GuildSheetRepository.cs` (the primary-constructor `db` is in scope, as used by the `Detach` method from sub-plan #2):
```csharp
    public void SetExpectedVersion(GuildSheet guild, uint expectedVersion) =>
        db.Entry(guild).Property(g => g.Version).OriginalValue = expectedVersion;
```
> This makes EF emit `UPDATE ... WHERE "Id" = @id AND xmin = @expectedVersion`. If a concurrent write advanced xmin, 0 rows match → `DbUpdateConcurrencyException`.

- [ ] **Step 4: Add `UpdateAsync` to the service interface**

In `IGuildSheetService.cs`:
```csharp
    Task<Result<GuildSheetResponse>> UpdateAsync(
        Guid callerId, Guid campaignId, UpdateGuildSheetRequest request, CancellationToken ct = default);
```

- [ ] **Step 5: Implement `UpdateAsync` + deepen the deserialize guard**

In `GuildSheetService.cs`, add (mirrors `CharacterSheetService.UpdateAsync`, adapted for xmin + emblem-preserve):
```csharp
    public async Task<Result<GuildSheetResponse>> UpdateAsync(
        Guid callerId, Guid campaignId, UpdateGuildSheetRequest request, CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null)
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.NotFound);

        var isGm = campaign.GameMasterId == callerId;
        var isMember = isGm || await membershipRepo.ExistsAsync(campaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.NotFound);

        var guild = await guildRepo.GetByCampaignAsync(campaignId, ct);
        if (guild is null)
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.NotFound);

        // EmblemImagePath is server-authoritative — preserve the stored value, ignore the
        // client's (emblem changes only via POST /api/media). Mirrors PortraitImagePath.
        var stored = Deserialize(guild.DataJson);
        var incoming = Deserialize(request.DataJson);
        incoming.Identity.EmblemImagePath = stored.Identity.EmblemImagePath;

        guild.GuildName = request.GuildName;
        guild.DataJson = JsonSerializer.Serialize(incoming, JsonOpts);
        guild.UpdatedAt = DateTime.UtcNow;

        guildRepo.SetExpectedVersion(guild, request.Version);
        guildRepo.Update(guild);
        try
        {
            await guildRepo.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.Conflict);
        }

        return Result.Success(await MapToResponseAsync(guild, ct));
    }
```
Extend the existing `Deserialize` method to guard the `Knowledge` lists:
```csharp
        data.Knowledge ??= new GuildKnowledge();
        data.Knowledge.Maps ??= [];
        data.Knowledge.Recipes ??= [];
        data.Knowledge.CataloguedEnemies ??= [];
        data.Knowledge.DefeatedBosses ??= [];
        data.Knowledge.HistoricalRecords ??= [];
```
> Ensure `using Microsoft.EntityFrameworkCore;` is present for `DbUpdateConcurrencyException` (it is, from the get-or-create catch added in #2). `Update` on `guildRepo` — confirm `BaseRepository` exposes `Update` (used by `CharacterSheetService`); if the guild repo lacks it, it's inherited from `BaseRepository<GuildSheet>`.

- [ ] **Step 6: Add the PUT endpoint**

In `GuildController.cs`, inject `IValidator<UpdateGuildSheetRequest> updateValidator` (add to the primary constructor) and add:
```csharp
    [HttpPut("campaigns/{campaignId:guid}/guild")]
    [ProducesResponseType(typeof(ApiResponse<GuildSheetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid campaignId, [FromBody] UpdateGuildSheetRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.UpdateAsync(callerId, campaignId, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Guild.Conflict
                ? Conflict(ApiResponse.Fail(localizer[result.Error!]))
                : NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<GuildSheetResponse>.Ok(result.Value!, localizer["Guild.Saved"]));
    }
```
Add `Error.ValidationFailed` reuse (already exists), and `Guild.Saved` + `Guild.Conflict` strings to BOTH API resx files.

- [ ] **Step 7: Run the tests → pass**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildUpdateTests`
Expected: PASS (all 6). The cross-request conflict test is the one that proves xmin works end to end.

- [ ] **Step 8: Commit**

```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Guilds/GuildUpdateTests.cs
git commit -m "feat: add guild blob update with xmin optimistic-concurrency enforcement

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Expeditions child CRUD

**Files:**
- Create: `IExpeditionRepository.cs`, `ExpeditionRepository.cs`
- Modify: `IGuildSheetService.cs`, `GuildSheetService.cs` (expedition CRUD + include in response), `GuildController.cs`, `InfrastructureExtensions.cs`
- Test: `tests/Ruptura.IntegrationTests/Guilds/GuildExpeditionTests.cs`

**Interfaces:**
- Produces: `IExpeditionRepository.GetByGuildAsync(Guid guildSheetId, CancellationToken)`; `IGuildSheetService.AddExpeditionAsync/UpdateExpeditionAsync/DeleteExpeditionAsync(Guid callerId, Guid campaignId, [Guid expeditionId,] <request>, CancellationToken) → Task<Result<...>>`.

- [ ] **Step 1: Write the failing tests**

`tests/Ruptura.IntegrationTests/Guilds/GuildExpeditionTests.cs`:
```
- Member adds an expedition -> 201/200; GET guild shows it in Expeditions (ordered by Date desc or Id — pick one and assert it).
- Update an expedition -> 200; changes reflected.
- Delete an expedition -> 200/204; gone from GET.
- Non-member add/update/delete -> 404.
- A DateTime with Kind=Unspecified/Local is accepted and stored (no Npgsql throw): send Date=new DateTime(2026,1,1) (Unspecified) -> 200.
- Deleting an expedition of another guild (wrong campaign) -> 404.
```

- [ ] **Step 2: Run → fail**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildExpeditionTests` → FAIL.

- [ ] **Step 3: Repository**

`src/Ruptura.Application/Interfaces/IExpeditionRepository.cs`:
```csharp
using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IExpeditionRepository : IRepository<Expedition>
{
    Task<IEnumerable<Expedition>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default);
}
```
`src/Ruptura.Infrastructure/Repositories/ExpeditionRepository.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class ExpeditionRepository(AppDbContext db)
    : BaseRepository<Expedition>(db), IExpeditionRepository
{
    public async Task<IEnumerable<Expedition>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default) =>
        await Set.Where(e => e.GuildSheetId == guildSheetId)
                 .OrderByDescending(e => e.Date)
                 .ToListAsync(ct);
}
```

- [ ] **Step 4: Service methods**

Add to `IGuildSheetService.cs`:
```csharp
    Task<Result<ExpeditionResponse>> AddExpeditionAsync(Guid callerId, Guid campaignId, CreateExpeditionRequest request, CancellationToken ct = default);
    Task<Result<ExpeditionResponse>> UpdateExpeditionAsync(Guid callerId, Guid campaignId, Guid expeditionId, UpdateExpeditionRequest request, CancellationToken ct = default);
    Task<Result> DeleteExpeditionAsync(Guid callerId, Guid campaignId, Guid expeditionId, CancellationToken ct = default);
```
Implement in `GuildSheetService.cs` (inject `IExpeditionRepository expeditionRepo` into the primary constructor). Each: resolve campaign + shared-write auth (extract the existing GM-or-member check into a private `AuthorizeAsync(callerId, campaignId, ct) → Task<Result<GuildSheet>>` returning the guild, and reuse it in `GetByCampaignAsync`/`UpdateAsync` too); for update/delete, verify the expedition's `GuildSheetId == guild.Id` (else `Guild.NotFound` — prevents cross-guild edits); normalize `Date` to UTC:
```csharp
    private static DateTime Utc(DateTime d) => d.Kind == DateTimeKind.Utc ? d : DateTime.SpecifyKind(d, DateTimeKind.Utc);
```
**Map string↔enum at the service boundary:** the DTOs carry `Kind` as `string` ("Principal"/"Secundaria") — Shared must not reference the Domain `ExpeditionKind`. On write, `Enum.TryParse<ExpeditionKind>(request.Kind, out var kind)` (default to `ExpeditionKind.Principal` on an unrecognized value, or return a validation failure — implementer's choice, but never throw). On read, `entity.Kind.ToString()`. Map `Expedition` → `ExpeditionResponse` with a private helper, and include the guild's expeditions in `MapToResponseAsync` (`response.Expeditions = (await expeditionRepo.GetByGuildAsync(guild.Id, ct)).Select(MapExpedition).ToList();`).

- [ ] **Step 5: Controller endpoints**

In `GuildController.cs`:
```csharp
    [HttpPost("campaigns/{campaignId:guid}/guild/expeditions")]
    public async Task<IActionResult> AddExpedition(Guid campaignId, [FromBody] CreateExpeditionRequest request, CancellationToken ct) { /* callerId; service; 201 on success, 404 on failure */ }

    [HttpPut("campaigns/{campaignId:guid}/guild/expeditions/{expeditionId:guid}")]
    public async Task<IActionResult> UpdateExpedition(Guid campaignId, Guid expeditionId, [FromBody] UpdateExpeditionRequest request, CancellationToken ct) { /* 200 / 404 */ }

    [HttpDelete("campaigns/{campaignId:guid}/guild/expeditions/{expeditionId:guid}")]
    public async Task<IActionResult> DeleteExpedition(Guid campaignId, Guid expeditionId, CancellationToken ct) { /* 200 / 404 */ }
```
Write them out following the `Update` endpoint's structure (parse `callerId`, call service, map `Result` to `ApiResponse` with the right status). Expeditions have no request validator (free-text); skip validation.

- [ ] **Step 6: Register the repository**

`InfrastructureExtensions.cs`:
```csharp
        services.AddScoped<IExpeditionRepository, ExpeditionRepository>();
```

- [ ] **Step 7: Run tests → pass**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildExpeditionTests` → PASS.

- [ ] **Step 8: Commit**

```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Guilds/GuildExpeditionTests.cs
git commit -m "feat: add guild expeditions log CRUD

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 4: Emblem upload

**Files:**
- Modify: `MediaEntityType.cs` (+ `GuildEmblem`), `IGuildSheetService.cs` (+ emblem auth + set), `GuildSheetService.cs`, `MediaController.cs` (upload + download branches)
- Test: `tests/Ruptura.IntegrationTests/Guilds/GuildEmblemTests.cs`

**Interfaces:**
- Produces: `IGuildSheetService.AuthorizeGuildAccessByIdAsync(Guid callerId, Guid guildSheetId, CancellationToken) → Task<Result<GuildSheet>>`; `IGuildSheetService.SetEmblemPathAsync(Guid guildSheetId, string path, CancellationToken) → Task<Result>`; `MediaEntityType.GuildEmblem`.

- [ ] **Step 1: Add the enum value**

`src/Ruptura.Domain/Enums/MediaEntityType.cs` — append:
```csharp
    GuildEmblem
```

- [ ] **Step 2: Write the failing test**

`tests/Ruptura.IntegrationTests/Guilds/GuildEmblemTests.cs` — follow the media upload test pattern (if one exists for portraits; otherwise construct a multipart form with a tiny valid PNG byte header). Cases:
```
- Member uploads an emblem (entityType="GuildEmblem", entityId=guildSheetId, a valid PNG) -> 200 with a path "guild-sheets/{id}/emblem-*.png"; GET guild -> Identity.EmblemImagePath == that path.
- Non-member upload -> 404.
- Download: GET /api/media/{that path} as a member -> 200; as a non-member -> 404.
```
Reuse the minimal valid-image bytes from the existing portrait media test (find it; PNG magic header `89 50 4E 47`).

- [ ] **Step 3: Service methods**

Add to `IGuildSheetService.cs`:
```csharp
    Task<Result<GuildSheet>> AuthorizeGuildAccessByIdAsync(Guid callerId, Guid guildSheetId, CancellationToken ct = default);
    Task<Result> SetEmblemPathAsync(Guid guildSheetId, string path, CancellationToken ct = default);
```
Implement in `GuildSheetService.cs`:
```csharp
    public async Task<Result<GuildSheet>> AuthorizeGuildAccessByIdAsync(Guid callerId, Guid guildSheetId, CancellationToken ct = default)
    {
        var guild = await guildRepo.GetByIdAsync(guildSheetId, ct);
        if (guild is null)
            return Result.Failure<GuildSheet>(ErrorCodes.Guild.NotFound);

        var campaign = await campaignRepo.GetByIdAsync(guild.CampaignId, ct);
        var isGm = campaign?.GameMasterId == callerId;
        var isMember = isGm || await membershipRepo.ExistsAsync(guild.CampaignId, callerId, ct);
        if (!isMember)
            return Result.Failure<GuildSheet>(ErrorCodes.Guild.NotFound);

        return Result.Success(guild);
    }

    // No auth of its own — MediaController authorizes via AuthorizeGuildAccessByIdAsync first
    // (mirrors CharacterSheetService.SetPortraitPathAsync). Sets Identity.EmblemImagePath inside
    // the blob (there is no dedicated column), preserving all other blob data.
    public async Task<Result> SetEmblemPathAsync(Guid guildSheetId, string path, CancellationToken ct = default)
    {
        var guild = await guildRepo.GetByIdAsync(guildSheetId, ct);
        if (guild is null)
            return Result.Failure(ErrorCodes.Guild.NotFound);

        var data = Deserialize(guild.DataJson);
        data.Identity.EmblemImagePath = path;
        guild.DataJson = JsonSerializer.Serialize(data, JsonOpts);
        guild.UpdatedAt = DateTime.UtcNow;

        guildRepo.Update(guild);
        await guildRepo.SaveChangesAsync(ct);
        return Result.Success();
    }
```
> `SetEmblemPathAsync` does NOT enforce `Version` — it's a targeted server-side mutation like `SetPortraitPathAsync`, not a client blob replace. A concurrent blob update racing an emblem upload is an accepted low-probability edge (same posture as the portrait path).

- [ ] **Step 4: MediaController upload branch**

In `MediaController.cs`, inject `IGuildSheetService guildService` (add to the primary constructor). Add a branch alongside the `CharacterSheetPortrait` branch:
```csharp
        if (parsedType == MediaEntityType.GuildEmblem)
        {
            var authorized = await guildService.AuthorizeGuildAccessByIdAsync(callerId, entityId, ct);
            if (authorized.IsFailure)
                return NotFound(ApiResponse.Fail(localizer[authorized.Error!]));

            var guild = authorized.Value!;
            var existing = System.Text.Json.JsonSerializer
                .Deserialize<Ruptura.Shared.Guilds.GuildSheetData>(guild.DataJson, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))?
                .Identity.EmblemImagePath;
            if (!string.IsNullOrEmpty(existing))
                await fileStorage.DeleteAsync(existing, ct);

            var relativePath = $"guild-sheets/{entityId}/emblem-{Guid.NewGuid()}{extension}";
            await using (var stream = file.OpenReadStream())
                await fileStorage.SaveAsync(stream, relativePath, ct);

            var setResult = await guildService.SetEmblemPathAsync(entityId, relativePath, ct);
            if (setResult.IsFailure)
            {
                await fileStorage.DeleteAsync(relativePath, ct);
                return BadRequest(ApiResponse.Fail(localizer[setResult.Error!]));
            }
            return Ok(ApiResponse<MediaUploadResponse>.Ok(new MediaUploadResponse { Path = relativePath }));
        }
```
> Reading the existing emblem inline here is acceptable but if it feels heavy, add a tiny `GetEmblemPathAsync` to the service instead — implementer's choice; keep the fail-closed delete-on-mutation-failure behavior either way.

- [ ] **Step 5: MediaController download branch**

In the `Download` action's `segments[0] switch`, add:
```csharp
            "guild-sheets" => (await guildService.AuthorizeGuildAccessByIdAsync(callerId, entityId, ct)) as Result,
```
> `segments[1]` is the guild id, which `AuthorizeGuildAccessByIdAsync` expects — the path-encoded authorization pattern, same as character sheets.

- [ ] **Step 6: Run the test → pass**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildEmblemTests` → PASS.

- [ ] **Step 7: Full sweep + commit**

Run: `dotnet build && dotnet test` → PASS (re-run once on a lone Serilog flake).
```bash
git add src/Ruptura.Domain src/Ruptura.Application src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Guilds/GuildEmblemTests.cs
git commit -m "feat: add guild emblem upload via path-encoded media authorization

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 5: Tabbed guild editor UI

Turn `GuildSheet.razor` into a tabbed editor: the read-only **Capacidades** panel from #2 plus six editable tabs. Pattern-directive — match the character-sheet editor.

**Files:**
- Modify: `src/Ruptura.Web/Pages/GuildSheet.razor` (host: name field, emblem upload, Save button, xmin conflict handling, tab strip)
- Create: `GuildIdentityTab.razor`, `GuildResourcesTab.razor`, `GuildInfluenceTab.razor`, `GuildKnowledgeTab.razor`, `GuildLegacyTab.razor`, `GuildExpeditionsTab.razor`
- Modify: the Web guild client service (add `UpdateGuildAsync`, `AddExpeditionAsync`, `UpdateExpeditionAsync`, `DeleteExpeditionAsync`, and emblem upload via the existing media client), Web resx pair.

**Interfaces:** Consumes `GuildSheetResponse`/`GuildSheetData`/`UpdateGuildSheetRequest`/expedition DTOs from `Ruptura.Shared.Guilds`; the design-system toolkit (Toast, Confirm, LoadingIndicator, TableSearchBox, `.ledger-table.stack-mobile`).

- [ ] **Step 1: Client methods**

In the Web guild client service (from #2), add methods for `PUT campaigns/{id}/guild` (body `UpdateGuildSheetRequest`, returns `ApiResponse<GuildSheetResponse>` — a 409 must be distinguishable from success so the page can react), and the three expedition endpoints. Follow the existing client conventions (how `CharacterSheetClientService` surfaces non-200s). Emblem upload reuses the existing `IMediaClientService` with `entityType="GuildEmblem"`, `entityId=_guild.Id`.

- [ ] **Step 2: Host page — editing shell**

Modify `GuildSheet.razor` to mirror `CharacterSheetEditor.razor`: a `GuildName` input, emblem `<InputFile>` (shows current emblem via a data-uri or the media URL), a **Save** button that serializes the edited `GuildSheetData` to `DataJson`, sends it with the last-known `Version`, and:
- on 200: replace `_guild` with the response (picks up the new `Version`), `Toast.Success`.
- on 409 (conflict): `Toast.Error` with a localized "someone else edited this — reloading" message, then refetch the guild (so the user sees current state) — the refetch-and-retry pattern the concurrency design calls for.
Keep the **Capacidades** tab rendering `<GuildCapacitiesPanel Stats="_guild.DerivedStats" />` (read-only).

- [ ] **Step 3: The six tabs**

Each tab is a component taking the relevant slice of `_data` (`GuildSheetData`) by `[Parameter]` and binding inputs to it (two-way), with no Save button of its own (the host's Save persists the whole blob):
- **GuildIdentityTab** — PatronDeity, MainDoctrineId (a `<select>` of Doctrine catalog entries — fetch via the catalog client filtered to `Doctrine` type, reusing the archived-entry picker pattern), FoundingDate, GuildRanking (`<select>` of the 8 ranks).
- **GuildResourcesTab** — Silver, PactCoins, DimensionalFragments, StrategicReserveNotes, and editable lists for Materials (name + qty add/remove rows) and Artifacts (string add/remove).
- **GuildInfluenceTab** — Prestige (Value + Notes) and the Influence relations table (Name, Kind `<select>`, Reputation −100..100, Notes; add/remove rows). Use `.ledger-table.stack-mobile` with `data-label`.
- **GuildKnowledgeTab** — five editable string lists (Maps, Recipes, CataloguedEnemies, DefeatedBosses, HistoricalRecords).
- **GuildLegacyTab** — Legado list (Title, Description, PermanentBenefit; add/remove rows).
- **GuildExpeditionsTab** — this one manages the child entity directly (NOT the blob): lists `_guild.Expeditions`, with add/edit/delete calling the expedition client methods (each mutation refetches the guild), `Confirm.AskAsync` before delete, `Toast` on result. Kind `<select>` (Principal/Secundaria, localized), a date input.

- [ ] **Step 4: i18n**

Add every visible string to BOTH Web resx files: tab titles, all field labels, the two `ExpeditionKind` display names, the doctrine/ranking select labels, the conflict/save toasts, add/remove button labels. English default + pt-BR.

- [ ] **Step 5: Build + verify**

Run: `dotnet build` (clean). If feasible, run the app (`make up` / the `run` skill) and confirm: editing a field + Save persists; a forced conflict (edit the same guild from two tabs) shows the reload toast; emblem upload shows the image and survives a reload. If a live run isn't feasible, confirm a clean build and note it in the report.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Web
git commit -m "feat: add tabbed guild editor with record-keeping modules and emblem

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage (§3.4 modules, §6 concurrency, §7 tabs, §11.4 emblem, §12.3):**
- Identidade / Recursos / Prestígio / Influência / Conhecimento / Legado (blob) → Task 2 (write path) + Task 5 (tabs). ✓
- Expedições (child entity) → Task 3 + Task 5 tab. ✓
- Emblem upload (§11.4) → Task 4 + Task 5 upload control. ✓
- xmin concurrency ENFORCED on write + cross-request conflict test (§6 / carry-in item 3) → Task 2. ✓
- Deepen blob non-null guard (carry-in item 2) → Task 2 Step 5. ✓
- `Expedition.Date` UTC normalization (carry-in item 3 of #2 minors) → Task 3. ✓
- **Deliberately deferred (not gaps):** Recursos per-material valuation (spec §11 item 3, still open — noted in Global Constraints); installation-reference validation (carry-in 7a — belongs to #4's Quartel-General write path); Doctrine limit enforcement on `ActiveDoctrineIds` (the Doutrinas tab is #4, not here — the Identidade tab only sets `MainDoctrineId`, a single display field, so no limit applies yet); the DoctrineLimit label + breadcrumb-href UI polish (carry-in 7c) — fold into Task 5 if trivial, else remains a tracked minor.

**2. Placeholder scan:** Backend Tasks 1–4 carry complete code. The controller expedition endpoints (Task 3 Step 5) and the UI (Task 5) are described structurally with explicit requirements and the exact endpoints/DTOs/patterns to follow — the concrete Razor/client/multipart conventions must be read from the repo at execution (same posture as #2 Task 5). No "TBD"/"handle appropriately".

**3. Type consistency:** `UpdateAsync(callerId, campaignId, request, ct)` identical across interface, impl, controller. `SetExpectedVersion(GuildSheet, uint)` consistent. `Version` (`uint`) matches `GuildSheet.Version` / `GuildSheetResponse.Version`. `AuthorizeGuildAccessByIdAsync`/`SetEmblemPathAsync` signatures consistent between interface, impl, and `MediaController`. `ExpeditionResponse`/`CreateExpeditionRequest`/`UpdateExpeditionRequest` fields match `Expedition`. The extracted private `AuthorizeAsync(callerId, campaignId, ct)` is used by `GetByCampaignAsync`/`UpdateAsync`/expedition methods consistently.
