# Guild Sheet — Concurrency Hardening & Polish (Sub-plan #7, FINAL) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Close the guild sheet feature: fix the client-side optimistic-concurrency erosion (a child-entity refresh silently adopting the server's new `xmin` while the blob holds unsaved edits, letting the next Save clobber a concurrent user), sweep the accumulated UI/i18n debt (responsive tab strip, in-flight-flag safety, label/breadcrumb fixes, server-side reputation clamp), and add the informational inflation-adjusted price reference — the last open item.

**Architecture:** The concurrency fix is client-only (`GuildSheet.razor`): track the last server-synced blob snapshot; a child-entity refresh adopts the refreshed `Version` **only when the blob is clean**, so a concurrent blob save is caught by the (already server-enforced, already tested) 409 instead of silently bypassed. The rest are localized polish: a CSS pass on the 12-tab strip, a `try/finally` sweep on tab handlers, resx/label tweaks, a reputation clamp in the blob-update service, and a read-only price-reference panel (base prices × `InflationIndex`).

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-08-07-guild-sheet-design.md` §6 (concurrency), §7 (tabs), §10.6.4 (inflation). Carry-ins (memory `project_campaign_architecture.md`): sub-plan #6 item 6 (the 6-site concurrency erosion — site 6/interlude already fixed via `ReplaceGuild` adopting `Resources`; the 5 child-refresh sites remain), item 7 (`finally` sweep, tab overflow); sub-plan #5 (Recursos inflation still open); sub-plan #4 (DoctrineLimit label; reputation not clamped; breadcrumb label-only/GM-string-for-players).

## Global Constraints

- **Concurrency-fix invariant:** a **child-row** mutation (building/staff/expedition/research/crafting) never changes the guild's blob `xmin` (child rows are different DB rows). So a child-refresh adopting the refreshed guild `Version` is only meaningful when a **concurrent blob save** moved it — and in that case, if the local blob has unsaved edits, adopting the new Version would mask the conflict. Rule: **adopt `_guild.Version = refreshed.Data.Version` in a child-refresh ONLY when the blob is not dirty**; when dirty, keep the current `_guild.Version` so the next blob Save legitimately 409s. "Dirty" = the serialized `_data` blob and `_guildName` differ from the last server-synced snapshot.
- **The interlude `ReplaceGuild` (site 6) is already correct** (it adopts `Resources` + Version because the interlude apply genuinely rewrote the server blob). This task must not regress it, and should update the server-synced snapshot baseline consistently so `ReplaceGuild` interacts correctly with the dirty check.
- **Testing reality:** the concurrency fix is Blazor-component logic and the project has no bUnit harness — verify it by review + the existing server-side cross-request 409 test (`GuildUpdateTests`, from #3) which this fix ensures is *reached* rather than bypassed. Extract the dirty-compare into a small pure helper in `Ruptura.Shared` so at least that unit is test-covered.
- **`Ruptura.Shared` stays ZERO project references** (the price-reference + dirty helper are pure).
- **Every visible string via `IStringLocalizer` in BOTH Web resx (en + pt-BR)**; API error strings in both API resx (the `GuildErrorCodeLocalizationTests` guard covers new codes — none expected here).
- **No behavioral regression:** all 437 existing tests stay green; new work is additive + the concurrency-adoption change.
- **Integration tests** use `IntegrationTestFactory`, `IClassFixture<>`, `parallelizeTestCollections: false`; lone Serilog flake known — re-run once.
- **Commit after each task** on `main`; end messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:**
- `src/Ruptura.Shared/Guilds/GuildBlobDirtyState.cs` (pure dirty-compare helper)
- `src/Ruptura.Shared/Guilds/GuildPriceReference.cs` (GDD §10.6.1 base prices)
- `tests/Ruptura.UnitTests/Guilds/GuildBlobDirtyStateTests.cs`

**Modify:**
- `src/Ruptura.Web/Pages/GuildSheet.razor` (dirty-aware version adoption; baseline tracking)
- `src/Ruptura.Web/Pages/Guild*Tab.razor` (in-flight `finally` sweep where missing; DoctrineLimit label; price-reference panel host)
- `src/Ruptura.Web/wwwroot/css/app.css` (responsive tab strip)
- `src/Ruptura.Infrastructure/Services/GuildSheetService.cs` (`UpdateAsync` reputation clamp)
- `src/Ruptura.Web/Pages/GuildCapacitiesPanel.razor` or a small Economia section (price reference display)
- Web resx pair (label fixes + price-reference strings)
- `tests/Ruptura.IntegrationTests/Guilds/GuildUpdateTests.cs` (reputation-clamp test)

---

### Task 1: Concurrency-erosion fix (client dirty-aware version adoption)

**Files:** create `GuildBlobDirtyState.cs` + `GuildBlobDirtyStateTests.cs`; modify `GuildSheet.razor`.

**Interfaces:**
- Produces: `GuildBlobDirtyState.IsDirty(string currentBlobJson, string baselineBlobJson, string currentName, string baselineName) → bool`.

- [ ] **Step 1: Write the failing unit test for the pure helper**

`tests/Ruptura.UnitTests/Guilds/GuildBlobDirtyStateTests.cs`:
```csharp
using FluentAssertions;
using Ruptura.Shared.Guilds;
using Xunit;

namespace Ruptura.UnitTests.Guilds;

public class GuildBlobDirtyStateTests
{
    [Fact] public void Clean_WhenBlobAndNameMatchBaseline() =>
        GuildBlobDirtyState.IsDirty("{\"a\":1}", "{\"a\":1}", "Guild", "Guild").Should().BeFalse();

    [Fact] public void Dirty_WhenBlobDiffers() =>
        GuildBlobDirtyState.IsDirty("{\"a\":2}", "{\"a\":1}", "Guild", "Guild").Should().BeTrue();

    [Fact] public void Dirty_WhenNameDiffers() =>
        GuildBlobDirtyState.IsDirty("{\"a\":1}", "{\"a\":1}", "New Name", "Guild").Should().BeTrue();
}
```

- [ ] **Step 2: Run → fail.**

- [ ] **Step 3: Implement the helper**

`src/Ruptura.Shared/Guilds/GuildBlobDirtyState.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

// Pure helper for the guild editor's optimistic-concurrency safety: the blob is "dirty" when the
// current serialized blob or the guild name differs from the last state the server confirmed.
// Used by GuildSheet.razor to decide whether a child-entity refresh may adopt the server's new
// xmin Version (adopting while dirty would mask a concurrent blob save and cause a lost update).
public static class GuildBlobDirtyState
{
    public static bool IsDirty(string currentBlobJson, string baselineBlobJson, string currentName, string baselineName) =>
        !string.Equals(currentBlobJson, baselineBlobJson, StringComparison.Ordinal)
        || !string.Equals(currentName, baselineName, StringComparison.Ordinal);
}
```

- [ ] **Step 4: Run → pass.**

- [ ] **Step 5: Wire dirty-aware version adoption into `GuildSheet.razor`**

Read `GuildSheet.razor` first (its `LoadGuildAsync`, `SaveAsync`, the five `Refresh*Async`, and `ReplaceGuild`). Add baseline tracking + a dirty check:

1. Add fields:
```csharp
    private string _baselineBlobJson = "{}";
    private string _baselineName = string.Empty;
```
2. In `LoadGuildAsync` (after `_data`/`_guildName` are set): `SyncBaseline();` where
```csharp
    private void SyncBaseline()
    {
        _baselineBlobJson = System.Text.Json.JsonSerializer.Serialize(_data);
        _baselineName = _guildName;
    }
    private bool BlobDirty() =>
        GuildBlobDirtyState.IsDirty(System.Text.Json.JsonSerializer.Serialize(_data), _baselineBlobJson, _guildName, _baselineName);
```
   (Use the same `JsonSerializerOptions` the client already uses to send `DataJson` on Save, so the baseline and the wire format match — reuse whatever `SaveAsync` serializes with.)
3. In each of the FIVE child-refresh methods (`RefreshBuildingsAsync`, `RefreshStaffAsync`, `RefreshExpeditionsAsync`, `RefreshResearchAsync`, `RefreshCraftingAsync`): update the child list + `DerivedStats` as today, but adopt the Version conditionally:
```csharp
        _guild.Buildings = refreshed.Data.Buildings;
        _guild.DerivedStats = refreshed.Data.DerivedStats;
        if (!BlobDirty()) _guild.Version = refreshed.Data.Version; // don't mask a concurrent blob save
```
   (Apply the analogous change in all five; expeditions has no DerivedStats line — keep its existing shape, just gate the Version adoption.)
4. In `SaveAsync` on success (after replacing `_guild`/`_data`/`_guildName` from the response): call `SyncBaseline();` so the blob is clean again.
5. In `ReplaceGuild` (interlude): after `_guild = refreshed; _data.Resources = refreshed.Data.Resources;`, set the baseline to the SERVER's blob so the dirty check reports only genuine unsaved edits (not the server-applied Resources deduction):
```csharp
        _baselineBlobJson = System.Text.Json.JsonSerializer.Serialize(refreshed.Data);
        _baselineName = refreshed.GuildName;
```
   (Keep adopting `refreshed.Version` here — the interlude apply read fresh server state, so its Version is current; a later concurrent save is still caught because the baseline now reflects the server and any further local edit re-dirties.)

> **CORRECTION (final review):** the reasoning below was WRONG and the implementation was changed during the final-review fix wave. The flawed claim was "clean-blob adoption is safe (nothing local to lose)" — but "clean" only means the user hasn't edited since their last sync, NOT that `_data` matches the server. Since child mutations don't bump the guild's `xmin`, a *changed* `refreshed.Data.Version` on a child-refresh ALWAYS means a concurrent blob write happened — so adopting it (even when clean) leaves `_data` stale relative to that write, and the next Save silently clobbers it. **Correct invariant: a child-refresh must NEVER adopt the server `Version`.** The fix removed the version adoption from all five refresh methods (and deleted the now-dead `GuildBlobDirtyState`/baseline machinery). No spurious 409s result, because the load-time version stays valid through child mutations unless a concurrent blob save moved it — which SHOULD 409. See `project_campaign_architecture.md` for the final state.
>
> ~~child mutations don't bump the guild's `xmin`, so when the blob is clean, `refreshed.Data.Version` equals the current guild Version anyway ... clean-blob adoption is safe (nothing local to lose)~~ (superseded — see correction above).

- [ ] **Step 6: Build + commit**

Run: `dotnet build && dotnet test` (all green; the new unit tests pass).
```bash
git add src/Ruptura.Shared/Guilds/GuildBlobDirtyState.cs tests/Ruptura.UnitTests/Guilds/GuildBlobDirtyStateTests.cs src/Ruptura.Web/Pages/GuildSheet.razor
git commit -m "fix: guild editor adopts server version on child-refresh only when blob is clean

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: Reputation clamp + i18n/label polish

**Files:** modify `GuildSheetService.cs` (`UpdateAsync`); Web resx + the relevant tabs; test `GuildUpdateTests.cs`.

- [ ] **Step 1: Write the failing reputation-clamp test**

In `GuildUpdateTests.cs`: PUT a guild whose `DataJson` has an `Influence` relation with `Reputation = 999` (and one with `-999`) → 200; a follow-up GET shows the values clamped to `100` / `-100`.

- [ ] **Step 2: Run → fail.**

- [ ] **Step 3: Clamp reputation in `UpdateAsync`**

In `GuildSheetService.UpdateAsync`, after deserializing `incoming` (and alongside the existing emblem-preserve / doctrine validation), clamp every influence relation's reputation to `[-100, 100]`:
```csharp
        foreach (var rel in incoming.Influence)
            rel.Reputation = Math.Clamp(rel.Reputation, -100, 100);
```
(This runs before serializing `incoming` back to `guild.DataJson`.)

- [ ] **Step 4: Run → pass.**

- [ ] **Step 5: i18n/label polish (UI)**

- **DoctrineLimit label** (`GuildDoctrinesTab.razor` / its resx key, from #4): change the displayed label from "Doctrines"/"Doutrinas" to "Doctrine Limit"/"Limite de Doutrinas" (or "Active Doctrines"/"Doutrinas Ativas") so the `count / limit` fraction reads correctly. resx-only tweak in both cultures.
- **Breadcrumb** (`GuildSheet.razor`, from #2): make the campaign crumb role-aware — a clickable href to the caller's campaign view and the correct string (players see "My Campaigns"/`Nav.Campaigns.Player`, GM sees "Campaigns"/`Nav.Campaigns`). Follow how other pages resolve the current role (the auth state / an existing role check).
- **Hard-coded-string audit:** grep the guild `.razor` files for any user-facing literal not routed through `IStringLocalizer`; move any found into both resx.

- [ ] **Step 6: Build + full sweep + commit**

Run: `dotnet build && dotnet test`.
```bash
git add src/Ruptura.Infrastructure src/Ruptura.Web tests/Ruptura.IntegrationTests/Guilds/GuildUpdateTests.cs
git commit -m "feat: clamp influence reputation server-side; polish guild labels/breadcrumb

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Responsive tab strip + in-flight-flag `finally` sweep

**Files:** modify `app.css` (tab strip); the guild `*Tab.razor` handlers.

- [ ] **Step 1: Responsive tab strip**

The guild sheet now has 12 tabs; Bootstrap `nav-tabs` overflow on narrow viewports is logged design-system debt. In `app.css`, make the guild tab strip degrade gracefully on small screens — a horizontally scrollable strip (`overflow-x:auto; flex-wrap:nowrap` with momentum scroll) OR a wrapping strip, using the existing design tokens. Scope the rule so it doesn't disturb other `nav-tabs` usages (a guild-specific wrapper class if needed). Verify the strip is usable at a ~375px width (no horizontal page scroll — the strip scrolls inside its own container).

- [ ] **Step 2: In-flight-flag `finally` sweep**

Across the guild `*Tab.razor` components, any handler that sets a `_saving`/`_applying`/`_previewing`/`_uploading`-style flag `true` then `false` around an `await` must reset the flag in a `finally` (a throwing HTTP/JSON call otherwise leaves the button permanently disabled). Wrap each such handler body in `try { ... } finally { _flag = false; StateHasChanged(); }`. Check every guild tab (buildings/staff/expeditions/research/crafting/interlude/emblem-upload on the host).

- [ ] **Step 3: Build + verify + commit**

Run: `dotnet build`. If feasible, run the app and confirm the tab strip scrolls on a narrow viewport and buttons recover after a simulated failure; else confirm clean build and note it.
```bash
git add src/Ruptura.Web
git commit -m "fix: responsive guild tab strip and reset in-flight flags in finally

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 4: Inflation-adjusted price reference

**Files:** create `GuildPriceReference.cs`; modify the Capacidades panel (or add a small Economia section) + Web resx.

- [ ] **Step 1: Price reference data**

`src/Ruptura.Shared/Guilds/GuildPriceReference.cs` — GDD §10.6.1 base prices (Prata):
```csharp
namespace Ruptura.Shared.Guilds;

public record GuildBasePrice(string Key, int BasePrice);

public static class GuildPriceReference
{
    // GDD §10.6.1 base prices in Prata. `Key` is a resx key suffix (Guild.Price.<Key>).
    public static readonly IReadOnlyList<GuildBasePrice> Items =
    [
        new("Ration", 1),        // Ração de comida (1 dia)
        new("Lodging", 2),       // Estadia simples (1 noite)
        new("LaborerWage", 3),   // Salário diário — Operário
        new("ArtisanWage", 8),   // Salário diário — Artesão/Pesquisador
    ];
}
```

- [ ] **Step 2: Display panel**

Add a read-only "Economia / Preços" section to the Capacidades panel (or a small dedicated block on that tab) that lists each `GuildPriceReference.Items` row as: localized label (`Guild.Price.<Key>`), base price, and the **inflation-adjusted** price = `ceil(BasePrice × Stats.InflationIndex)` (round up — prices don't get cheaper via rounding). Show the current `InflationIndex` as the multiplier. It's purely informational (no purchase action). All strings localized in both resx (the 4 labels + "base"/"adjusted"/"inflation" headers).

- [ ] **Step 3: Build + commit**

Run: `dotnet build` (clean).
```bash
git add src/Ruptura.Shared/Guilds/GuildPriceReference.cs src/Ruptura.Web
git commit -m "feat: add inflation-adjusted price reference to the guild capacities panel

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Coverage (the accumulated debt this closes):**
- **Concurrency erosion (6 sites)** → Task 1 fixes the 5 child-refresh sites (dirty-aware Version adoption); site 6 (interlude `ReplaceGuild`) was fixed in #6 and Task 1 keeps it consistent (baseline sync). ✓
- **Reputation not clamped server-side** (#3) → Task 2. ✓
- **DoctrineLimit label + breadcrumb href/string** (#2/#4) → Task 2. ✓
- **12-tab overflow** (design-system debt) → Task 3. ✓
- **In-flight flags not in `finally`** (#5/#6) → Task 3. ✓
- **Recursos inflation** (#5, long-open) → Task 4 (informational price reference; the GDD applies inflation to purchase prices, and this surfaces exactly that without inventing a buy/sell flow). ✓
- **Deliberately NOT done:** a full buy/sell economy (out of scope — inflation is surfaced as reference only); a bUnit test harness (not introduced — the concurrency fix is review-verified atop the existing server 409 test, with the pure dirty-compare unit-tested). Build-cost formula stays in Razor (no server consumer yet).

**2. Placeholder scan:** Task 1 (helper + wiring) and Task 4 (reference + panel) carry concrete code; Tasks 2–3's UI/CSS steps are described with exact targets and the specific fixes, read against the repo at execution (same posture as prior UI tasks). No "TBD"/"handle appropriately".

**3. Type consistency:** `GuildBlobDirtyState.IsDirty(string,string,string,string)` identical in helper, test, and `GuildSheet.razor` usage. Baseline fields (`_baselineBlobJson`/`_baselineName`) set in `LoadGuildAsync`/`SaveAsync`/`ReplaceGuild` and read in `BlobDirty()`. `GuildPriceReference.Items` (`GuildBasePrice` records) consumed by the panel with `Stats.InflationIndex` (existing `GuildDerivedStats` field). Reputation clamp operates on `incoming.Influence[].Reputation` (existing `InfluenceRelation` field) in `UpdateAsync`.

---

## After This Sub-plan

This is the **final** sub-plan of the guild-sheet feature (spec `2026-08-07-guild-sheet-design.md`). When it merges, all seven sub-plans (#1 Foundation → #7 Polish) are complete. Any further guild work is a new feature — start a fresh brainstorm. Remaining *tracked, deliberately-deferred* items (none blocking): a real buy/sell economy applying inflation transactionally; secondary-expedition yield (needs a mercenary NP model); orphan media/FK-cleanup once a Campaign/Guild DELETE endpoint exists.
