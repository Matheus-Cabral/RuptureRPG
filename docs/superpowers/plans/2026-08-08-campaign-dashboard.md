# Campaign Dashboard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A GM-only campaign dashboard at `/gm/campaigns/{id}/dashboard` — the in-progress dungeon floor + Pressão counter (new, GM-editable `Campaign` state) plus read-only aggregations of the active party, guild snapshot, and pending rank-promotion notifications.

**Architecture:** Four new scalar fields on `Campaign` hold the dungeon state; a pure `DungeonPressure` helper maps the 0-100 counter to its state + PE multiplier (§4.2). `CampaignDashboardService` (Infrastructure) authorizes the GM, reads the dungeon state, and assembles the read-only panels by reusing the existing `CharacterSheetService`/`GuildSheetService`/notification repo — it orchestrates, it doesn't duplicate their logic. `GET .../dashboard` returns the aggregate; `PUT .../dashboard/dungeon` writes the whole dungeon state (clamped/validated). A Blazor page renders the four panels with GM Pressão controls.

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-08-08-campaign-dashboard-design.md`. GDD: Manual do Mestre §4.2 (Pressão), §4.3 (floor states).

## Global Constraints

- **GM-only:** every endpoint `[Authorize(Roles = "GameMaster")]`; the caller must be the campaign's `GameMasterId`, else 404 (`Campaign.NotFound`, hide existence — matches the existing campaign convention).
- **`Ruptura.Shared` must NOT reference `Ruptura.Domain`** (DTOs use `string` for `FloorState`/state keys; the `DungeonPressure`/`DungeonFloorStates` helpers are pure and live in `Ruptura.Shared.Campaigns`). Verify Shared keeps zero project references.
- **No new concurrency machinery** — single GM per campaign; the dungeon `PUT` is last-write. Do NOT add optimistic concurrency.
- **No side-effect guild creation:** the guild snapshot must be read GET-only (via the guild *repository*, returns null when absent → "no guild yet"); do NOT call `GuildSheetService.GetByCampaignAsync` unconditionally (it get-or-creates). Only map a snapshot when a guild already exists.
- **Party = alive, non-retired** character sheets only (`!IsDead && !IsRetired`), projecting name / Ranking (`Data.GuildRegistry.Ranking`) / Np (`DerivedStats.Np`) / CurrentHp (`Data.Combat.CurrentHp`) / MaxHp (`DerivedStats.MaxHp`) — reuse `CharacterSheetService.GetByCampaignAsync`, which already computes derived stats.
- **Pressão is display-only math:** the PE multiplier and Colapso warning are shown, never applied to any combat/encounter computation automatically.
- **Every visible string via `IStringLocalizer`** in BOTH Web resx (en + pt-BR); API error strings in both API resx.
- **Integration tests** use `IntegrationTestFactory`, `IClassFixture<>`, `parallelizeTestCollections: false`; lone Serilog flake known — re-run once.
- **Commit after each task** on `main`; end messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:**
- `src/Ruptura.Shared/Campaigns/DungeonPressure.cs`, `DungeonFloorStates.cs`, `CampaignDashboardResponse.cs` (+ sub-DTOs), `UpdateDungeonStateRequest.cs`
- `src/Ruptura.Application/Interfaces/ICampaignDashboardService.cs`
- `src/Ruptura.Infrastructure/Services/CampaignDashboardService.cs`
- `src/Ruptura.API/Controllers/CampaignDashboardController.cs`
- `src/Ruptura.Web/Pages/GmCampaignDashboard.razor`
- `tests/Ruptura.UnitTests/Campaigns/DungeonPressureTests.cs`
- `tests/Ruptura.IntegrationTests/Campaigns/CampaignDashboardTests.cs`

**Modify:**
- `src/Ruptura.Domain/Entities/Campaign.cs` (+4 fields)
- a new EF migration (`AddCampaignDungeonState`)
- `src/Ruptura.Application/Common/ErrorCodes.cs` (+ `Campaign.FloorStateInvalid`)
- `src/Ruptura.Application/Interfaces/INotificationRepository.cs` + its impl (add `GetUnreadByCampaignAsync` if no by-campaign query exists)
- `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` (register the dashboard service)
- `src/Ruptura.API/Resources/*.resx`, Web resx pair
- `src/Ruptura.Web/Pages/GmCampaignDetail.razor` (Dashboard link)
- the Web campaign client service (dashboard GET + dungeon PUT)

---

### Task 1: Campaign dungeon fields, DungeonPressure helper, DTOs, migration

**Files:** modify `Campaign.cs`, `ErrorCodes.cs`; create the four Shared files + `DungeonPressureTests.cs`; generate the migration.

**Interfaces:**
- Produces: `Campaign.CurrentFloor/FloorName/FloorState/Pressure`; `DungeonPressure.StateFor(int) → (string StateKey, decimal PeMultiplier)`; `DungeonFloorStates.All`; `CampaignDashboardResponse` (+ `DungeonStateDto`, `PartyMemberDto`, `GuildSnapshotDto`, `PendingNotificationDto`); `UpdateDungeonStateRequest`; `ErrorCodes.Campaign.FloorStateInvalid`.

- [ ] **Step 1: Campaign entity fields**

In `src/Ruptura.Domain/Entities/Campaign.cs`, add:
```csharp
    public int CurrentFloor { get; set; } = 1;
    public string FloorName { get; set; } = string.Empty;
    public string FloorState { get; set; } = "Inexplorado"; // Inexplorado|Explorado|Conquistado|Dominado
    public int Pressure { get; set; }                        // 0..100 (§4.2)
```

- [ ] **Step 2: Write the failing DungeonPressure unit tests**

`tests/Ruptura.UnitTests/Campaigns/DungeonPressureTests.cs`:
```csharp
using FluentAssertions;
using Ruptura.Shared.Campaigns;
using Xunit;

namespace Ruptura.UnitTests.Campaigns;

public class DungeonPressureTests
{
    [Theory]
    [InlineData(0, "Estavel", 1.00)]
    [InlineData(24, "Estavel", 1.00)]
    [InlineData(25, "Agravado", 1.10)]
    [InlineData(59, "Agravado", 1.10)]
    [InlineData(60, "Critico", 1.25)]
    [InlineData(89, "Critico", 1.25)]
    [InlineData(90, "Colapso", 1.50)]
    [InlineData(100, "Colapso", 1.50)]
    public void StateFor_MapsRangeToStateAndMultiplier(int pressure, string key, decimal mult)
    {
        var (stateKey, peMultiplier) = DungeonPressure.StateFor(pressure);
        stateKey.Should().Be(key);
        peMultiplier.Should().Be(mult);
    }
}
```

- [ ] **Step 3: Run → fail.**

- [ ] **Step 4: DungeonPressure + DungeonFloorStates**

`src/Ruptura.Shared/Campaigns/DungeonPressure.cs`:
```csharp
namespace Ruptura.Shared.Campaigns;

// GDD Manual §4.2 — the Pressão counter (0-100) maps to a state + PE multiplier.
// State keys are unaccented resx-key suffixes (Dashboard.Pressure.<StateKey>); the UI localizes.
public static class DungeonPressure
{
    public static (string StateKey, decimal PeMultiplier) StateFor(int pressure) => pressure switch
    {
        >= 90 => ("Colapso", 1.50m),
        >= 60 => ("Critico", 1.25m),
        >= 25 => ("Agravado", 1.10m),
        _ => ("Estavel", 1.00m),
    };
}
```
`src/Ruptura.Shared/Campaigns/DungeonFloorStates.cs`:
```csharp
namespace Ruptura.Shared.Campaigns;

public static class DungeonFloorStates
{
    public static readonly IReadOnlyList<string> All =
        ["Inexplorado", "Explorado", "Conquistado", "Dominado"];
}
```

- [ ] **Step 5: Run → pass.**

- [ ] **Step 6: DTOs**

`src/Ruptura.Shared/Campaigns/CampaignDashboardResponse.cs`:
```csharp
namespace Ruptura.Shared.Campaigns;

public class CampaignDashboardResponse
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public DungeonStateDto Dungeon { get; set; } = new();
    public List<PartyMemberDto> Party { get; set; } = [];
    public GuildSnapshotDto? Guild { get; set; }              // null when no guild exists yet
    public List<PendingNotificationDto> PendingNotifications { get; set; } = [];
}

public class DungeonStateDto
{
    public int CurrentFloor { get; set; }
    public string FloorName { get; set; } = string.Empty;
    public string FloorState { get; set; } = string.Empty;
    public int Pressure { get; set; }
    public string PressureStateKey { get; set; } = string.Empty; // derived
    public decimal PeMultiplier { get; set; }                    // derived
}

public class PartyMemberDto
{
    public Guid Id { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string Ranking { get; set; } = string.Empty;
    public int Np { get; set; }
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
}

public class GuildSnapshotDto
{
    public string Stage { get; set; } = string.Empty;
    public int Cg { get; set; }
    public int FloorsConquered { get; set; }
    public int Silver { get; set; }
    public int PactCoins { get; set; }
}

public class PendingNotificationDto
{
    public Guid Id { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
```
`src/Ruptura.Shared/Campaigns/UpdateDungeonStateRequest.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Campaigns;

public class UpdateDungeonStateRequest
{
    public int CurrentFloor { get; set; }
    [MaxLength(120)]
    public string FloorName { get; set; } = string.Empty;
    [Required]
    public string FloorState { get; set; } = string.Empty; // must be one of DungeonFloorStates.All
    public int Pressure { get; set; }                      // clamped [0,100] server-side
}
```

- [ ] **Step 7: Error code**

In `ErrorCodes.Campaign`: `public const string FloorStateInvalid = "Campaign.FloorStateInvalid";`

- [ ] **Step 8: Migration**

Run: `dotnet ef migrations add AddCampaignDungeonState --project src/Ruptura.Infrastructure --startup-project src/Ruptura.API`. Confirm it adds four columns to `Campaigns` with the entity defaults (CurrentFloor 1, FloorName '', FloorState 'Inexplorado', Pressure 0), no other changes. Do not hand-edit.

- [ ] **Step 9: Build + commit**

Run: `dotnet build && dotnet test` (green; the new unit theory passes).
```bash
git add src/Ruptura.Domain src/Ruptura.Shared/Campaigns src/Ruptura.Application/Common/ErrorCodes.cs src/Ruptura.Infrastructure/Data/Migrations tests/Ruptura.UnitTests/Campaigns/DungeonPressureTests.cs
git commit -m "feat: add campaign dungeon-state fields, DungeonPressure helper, dashboard DTOs

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: Dashboard service + endpoints

**Files:** create `ICampaignDashboardService.cs`, `CampaignDashboardService.cs`, `CampaignDashboardController.cs`; modify `INotificationRepository`(+impl if needed), `InfrastructureExtensions.cs`, API resx; test `CampaignDashboardTests.cs`.

**Interfaces:**
- Produces: `ICampaignDashboardService.GetAsync(Guid gameMasterId, Guid campaignId, ct) → Task<Result<CampaignDashboardResponse>>`; `UpdateDungeonAsync(Guid gameMasterId, Guid campaignId, UpdateDungeonStateRequest, ct) → Task<Result<CampaignDashboardResponse>>`.

- [ ] **Step 1: Write the failing integration tests**

`tests/Ruptura.IntegrationTests/Campaigns/CampaignDashboardTests.cs` — mirror the existing controller-test fixture. Cases:
```
- GM GETs their campaign dashboard -> 200: Dungeon defaults (CurrentFloor 1, FloorState Inexplorado, Pressure 0, PressureStateKey "Estavel"); Party lists alive characters (not a dead/retired one); Guild null when no guild exists; PendingNotifications reflects unread rank-promotion notifications.
- A different GM (not the campaign's) GETs it -> 404. A player -> 403/404 (role-gated).
- PUT dungeon { CurrentFloor 3, FloorName "Cripta", FloorState "Explorado", Pressure 150 } -> 200; GET shows Pressure clamped to 100 (PressureStateKey "Colapso"), CurrentFloor 3, FloorState "Explorado".
- PUT with Pressure -20 -> clamped to 0. PUT CurrentFloor 0 -> clamped to 1.
- PUT FloorState "Nonsense" -> 400 Campaign.FloorStateInvalid.
- "Advance floor" shape: PUT { CurrentFloor 2, Pressure 0, ... } round-trips (Pressure 0 -> Estavel).
- (If a guild exists) Guild snapshot present with Stage/Cg/Silver.
```

- [ ] **Step 2: Run → fail.**

- [ ] **Step 3: Notification-by-campaign query**

Ensure the notification repo can fetch unread notifications for a campaign. If `INotificationRepository` has no by-campaign query, add `Task<IEnumerable<Notification>> GetUnreadByCampaignAsync(Guid campaignId, CancellationToken ct = default)` (impl: `Set.Where(n => n.CampaignId == campaignId && !n.IsRead)`). (Reuse an existing method if one already covers this.)

- [ ] **Step 4: Service interface + impl**

`ICampaignDashboardService.cs` (Application.Interfaces): the two methods above.

`CampaignDashboardService.cs` (Infrastructure.Services) — inject `ICampaignRepository campaignRepo`, `ICharacterSheetService characterSheetService`, `IGuildSheetRepository guildRepo`, `IGuildSheetService guildService`, `INotificationRepository notificationRepo`, `ICharacterSheetRepository sheetRepo` (to resolve notification character names). Sketch:
```csharp
    public async Task<Result<CampaignDashboardResponse>> GetAsync(Guid gameMasterId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CampaignDashboardResponse>(ErrorCodes.Campaign.NotFound);

        return Result.Success(await BuildAsync(campaign, gameMasterId, ct));
    }

    public async Task<Result<CampaignDashboardResponse>> UpdateDungeonAsync(
        Guid gameMasterId, Guid campaignId, UpdateDungeonStateRequest request, CancellationToken ct = default)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        if (campaign is null || campaign.GameMasterId != gameMasterId)
            return Result.Failure<CampaignDashboardResponse>(ErrorCodes.Campaign.NotFound);

        if (!DungeonFloorStates.All.Contains(request.FloorState))
            return Result.Failure<CampaignDashboardResponse>(ErrorCodes.Campaign.FloorStateInvalid);

        campaign.CurrentFloor = Math.Max(1, request.CurrentFloor);
        campaign.FloorName = request.FloorName;
        campaign.FloorState = request.FloorState;
        campaign.Pressure = Math.Clamp(request.Pressure, 0, 100);
        campaign.UpdatedAt = DateTime.UtcNow;
        campaignRepo.Update(campaign);
        await campaignRepo.SaveChangesAsync(ct);

        return Result.Success(await BuildAsync(campaign, gameMasterId, ct));
    }

    private async Task<CampaignDashboardResponse> BuildAsync(Campaign campaign, Guid gameMasterId, CancellationToken ct)
    {
        var (stateKey, mult) = DungeonPressure.StateFor(campaign.Pressure);

        // Party — reuse the character-sheet service (already GM-scoped + derived stats), alive only.
        var sheets = (await characterSheetService.GetByCampaignAsync(gameMasterId, campaign.Id, ct)).Value
                     ?? Enumerable.Empty<CharacterSheetResponse>();
        var party = sheets.Where(s => !s.IsDead && !s.IsRetired)
            .Select(s => new PartyMemberDto {
                Id = s.Id, CharacterName = s.CharacterName,
                Ranking = s.Data.GuildRegistry.Ranking, Np = s.DerivedStats.Np,
                CurrentHp = s.Data.Combat.CurrentHp, MaxHp = s.DerivedStats.MaxHp
            }).ToList();

        // Guild — GET-only (no side-effect create). Map via the service only if it exists.
        GuildSnapshotDto? guild = null;
        if (await guildRepo.GetByCampaignAsync(campaign.Id, ct) is not null)
        {
            var g = (await guildService.GetByCampaignAsync(gameMasterId, campaign.Id, ct)).Value!;
            guild = new GuildSnapshotDto {
                Stage = g.DerivedStats.Stage.ToString(), Cg = g.DerivedStats.Cg,
                FloorsConquered = g.Data.FloorsConquered,
                Silver = g.Data.Resources.Silver, PactCoins = g.Data.Resources.PactCoins
            };
        }

        // Notifications — unread for this campaign; resolve character names.
        var notifs = await notificationRepo.GetUnreadByCampaignAsync(campaign.Id, ct);
        var pending = new List<PendingNotificationDto>();
        foreach (var n in notifs)
        {
            var name = n.RelatedCharacterSheetId is { } sid
                ? (await sheetRepo.GetByIdAsync(sid, ct))?.CharacterName ?? string.Empty
                : string.Empty;
            pending.Add(new PendingNotificationDto { Id = n.Id, CharacterName = name, Message = n.Message });
        }

        return new CampaignDashboardResponse {
            CampaignId = campaign.Id, CampaignName = campaign.Name,
            Dungeon = new DungeonStateDto {
                CurrentFloor = campaign.CurrentFloor, FloorName = campaign.FloorName,
                FloorState = campaign.FloorState, Pressure = campaign.Pressure,
                PressureStateKey = stateKey, PeMultiplier = mult
            },
            Party = party, Guild = guild, PendingNotifications = pending
        };
    }
```
> Confirm exact member names/shapes against the real files (`CharacterSheetResponse.Data.Combat.CurrentHp`, `Data.GuildRegistry.Ranking`, `DerivedStats.Np/MaxHp`; `GuildSheetResponse.DerivedStats.Stage/Cg`, `Data.FloorsConquered`, `Data.Resources.Silver/PactCoins`; `Notification.Message`/`RelatedCharacterSheetId`/`IsRead`/`CampaignId`). Adjust the projections to match. `ErrorCodes.Campaign.NotFound` already exists.

- [ ] **Step 5: Controller + DI + resx**

`CampaignDashboardController.cs` (`[ApiController] [Route("api")] [Authorize(Roles = "GameMaster")]`), parse `gameMasterId` from `JwtRegisteredClaimNames.Sub`:
- `GET campaigns/{campaignId:guid}/dashboard` → `GetAsync`; failure → 404.
- `PUT campaigns/{campaignId:guid}/dashboard/dungeon` (`[FromBody] UpdateDungeonStateRequest`) → `UpdateDungeonAsync`; `Campaign.FloorStateInvalid` → 400, else (NotFound) → 404.
Register `ICampaignDashboardService → CampaignDashboardService` (AddScoped) in `InfrastructureExtensions`. Add `Campaign.FloorStateInvalid` to both API resx.

- [ ] **Step 6: Run tests → pass; full sweep; commit**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~CampaignDashboardTests` then `dotnet build && dotnet test`.
```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Campaigns/CampaignDashboardTests.cs
git commit -m "feat: add campaign dashboard service and endpoints (aggregate GET + dungeon PUT)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Dashboard page (Blazor)

**Files:** create `GmCampaignDashboard.razor`; modify the Web campaign client service, `GmCampaignDetail.razor` (link), Web resx pair.

- [ ] **Step 1: Client methods**

Add to the Web campaign client service: `GetDashboardAsync(Guid campaignId)` (GET → `CampaignDashboardResponse`) and `UpdateDungeonAsync(Guid campaignId, UpdateDungeonStateRequest)` (PUT → `CampaignDashboardResponse`; surface a 400 message for `FloorStateInvalid`). Follow existing client conventions.

- [ ] **Step 2: The page**

`GmCampaignDashboard.razor` — `@page "/gm/campaigns/{Id:guid}/dashboard"`, `@attribute [Authorize(Roles = "GameMaster")]`. On `OnInitializedAsync`: `GetDashboardAsync(Id)`; `LoadingIndicator` while loading; toast on failure; `Breadcrumbs` (campaign name from the response). Four panels:
1. **Andar & Pressão** — CurrentFloor (number), FloorName (text), FloorState `<select>` (`DungeonFloorStates.All`, localized labels); a Pressão meter/bar (0-100) showing `Dungeon.Pressure`, the localized state (`Dashboard.Pressure.<PressureStateKey>`) and `PeMultiplier` (e.g. "×1.25"); quick buttons **+5 Turno / +10 Combate / +15 Falha Crítica** and a custom **+N Evento** input; **Avançar andar** (CurrentFloor+1, Pressure→0, FloorState→"Inexplorado"). Each control computes the new `UpdateDungeonStateRequest` from the current dungeon state and calls `UpdateDungeonAsync`, then replaces the page state from the response + toasts. Show a prominent warning (toolkit alert style) when `PressureStateKey == "Colapso"`. Clamp the buttons client-side too (0-100) for immediate feedback; the server clamps authoritatively.
2. **Party ativa** — `.ledger-table.stack-mobile`: character, ranking, NP, HP (`CurrentHp/MaxHp`). Empty state (`Dashboard.Party.Empty`) when none.
3. **Guilda** — Stage, CG, floors conquered, Silver, PactCoins; link to `/campaigns/{Id}/guild`. Empty state ("no guild yet") when `Guild is null`.
4. **Notificações** — list pending promotions (character + message) with a link to `/gm/notifications`. Empty state when none.

- [ ] **Step 3: Entry point + i18n**

Add a "Dashboard" button/link to `GmCampaignDetail.razor` (alongside the catalog/guild links) → `/gm/campaigns/{Id}/dashboard`. Add every visible string to BOTH Web resx: page/panel titles, floor-state labels (`Dashboard.FloorState.*` or reuse), the four Pressão state labels (`Dashboard.Pressure.Estavel/Agravado/Critico/Colapso`), quick-button labels, "advance floor", the Colapso warning, PE-multiplier/HP/NP labels, the three empty states, and the API error message. English default + pt-BR.

- [ ] **Step 4: Build + verify + commit**

Run: `dotnet build` (clean). If feasible, run the app and confirm: the dashboard loads; a +10 button raises Pressão and flips the state at the thresholds; advancing a floor resets Pressão; the party/guild/notification panels populate. Else confirm clean build and note it.
```bash
git add src/Ruptura.Web
git commit -m "feat: add GM campaign dashboard page (floor/Pressão + party/guild/notifications)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage (§3 data, §4 API, §5 UI, §7 tests):**
- Dungeon fields on `Campaign` + migration → Task 1. ✓
- `DungeonPressure` helper (states + multiplier) + unit tests → Task 1. ✓
- `GET dashboard` aggregate (party alive-only, guild GET-only, unread notifications) + `PUT dungeon` (clamp/validate) → Task 2. ✓
- GM-only auth + hide-existence-404 → Task 2. ✓
- Four-panel page + Pressão controls + entry point → Task 3. ✓
- **Deliberately deferred (not gaps):** player-facing view (GM-only by decision); auto Colapso event (display only); floor history log; applying the PE multiplier to encounter math (display only).

**2. Placeholder scan:** Tasks 1–2 carry complete code / precise service sketches with a "confirm member names against the real files" note (the projections read fields from `CharacterSheetResponse`/`GuildSheetResponse`/`Notification` that exist but whose exact nesting must be read at execution). Task 3 is pattern-directive (Razor/client/i18n conventions read from the repo), consistent with prior UI tasks. No "TBD"/"handle appropriately".

**3. Type consistency:** `DungeonPressure.StateFor(int) → (string, decimal)` identical in helper, tests, and `BuildAsync`. `CampaignDashboardResponse` + sub-DTOs consumed by the service projection and the page. `UpdateDungeonStateRequest { CurrentFloor, FloorName, FloorState, Pressure }` consumed by `UpdateDungeonAsync` + controller + client. `DungeonFloorStates.All` used by the validator and the UI `<select>`. `GetAsync`/`UpdateDungeonAsync(gameMasterId, campaignId, …)` consistent across interface/impl/controller.
