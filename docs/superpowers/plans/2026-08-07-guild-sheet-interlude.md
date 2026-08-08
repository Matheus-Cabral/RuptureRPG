# Guild Sheet — Interlude Calculator (Sub-plan #6) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A **preview-and-apply interlude engine** — the user enters a number of days, sees every projected indicator (daily maintenance accrued, worker income, per-project research progress, per-order crafting progress) computed server-side, and applies each indicator **individually** with a button, each Apply recomputing its delta on the server and persisting only that mutation.

**Architecture:** `IInterludeCalculator` (Application, pure) projects N days into a list of `InterludeIndicator`s from the guild's derived stats + research/crafting rows. `GuildSheetService` exposes `PreviewInterludeAsync` (read-only) and `ApplyInterludeAsync` — the Apply **re-runs the same projection server-side**, selects the indicator by `{Kind, TargetId}`, and applies *that server-computed delta* (the client's request carries only a selector `{Kind, TargetId?, Days}` — never a number), guaranteeing preview and apply agree and that a client can't post an arbitrary treasury/points delta. Blob deltas (Silver) use a version-safe save; child-row deltas (research/crafting progress) are per-row. Day-count is capped and all economic math is overflow-safe (`long` + clamp), reusing the #5 lesson.

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-08-07-guild-sheet-design.md` §5 (Interlude Calculator), §6, §7 (Interlúdio tab), §12.6. GDD §11.2 (interlude subsystems), §8.4/§10.6 (maintenance/income). Carry-in (memory `project_campaign_architecture.md` sub-plan #5): `ProgressDays`/`Stage`/`IsComplete` are manual until this sub-plan automates advancement; `ResearchProject.Researchers` is ready for the §11.2 split-time rule; reuse the `long`+clamp overflow-safe pattern.

## Global Constraints

- **Shared write:** GM or member may preview/apply. Non-member → 404 (`Guild.NotFound`). Reuse `GuildSheetService.AuthorizeAsync`.
- **Security invariant (server is the sole source of every applied number):** `ApplyInterludeRequest` carries ONLY `{ Kind, TargetId?, Days }` — no Silver/points/days-added values. The service **re-runs the projection** and applies the matching indicator's server-computed delta. A client cannot influence the magnitude, only the selector. (Same lesson as the media path-selector vulnerability.)
- **Preview == Apply:** Apply reuses the same `IInterludeCalculator.Project(...)` call as Preview to derive the delta, so what the user saw is exactly what's applied (modulo intervening state changes, which Apply correctly reflects because it recomputes from fresh state).
- **Four indicator kinds only** (user decision — SecondaryExpedition deferred; secondary expeditions stay manual via the #3 Expedições tab): `Maintenance`, `Income`, `ResearchProgress`, `CraftingProgress`.
- **Research advancement (user decision, §11.2 50%-floor):** progress-per-day = `min(Researchers, 2)` (1 researcher → full time; 2+ → half time, never faster). `ProgressDays += min(Researchers, 2) × days`, capped at `RequiredDays`; on reaching `RequiredDays` → `IsComplete = true` (its `Points` then feed CG on the next read). Only `!IsComplete` projects produce a ResearchProgress indicator.
- **Crafting advancement:** `ProgressDays += days`, capped at `RequiredDays`; on reaching it → `Status = Concluido`. Only `EmAndamento` orders produce a CraftingProgress indicator.
- **Maintenance/Income:** Maintenance delta = `-(DailyMaintenance × days)` applied to `Resources.Silver`, **floored at 0** (unpaid → GM handles Negligência narratively; §8.4 "nunca trava o jogo"). Income delta = `+(WorkerIncomePerDay × days)`. Both come from the `GuildStatsCalculator` outputs already available.
- **Day cap + overflow safety:** `Days` must be `1..3650` (else `Guild.InterludeDaysInvalid`). All `rate × days` and progress math use `long` then clamp to `int` (a `ClampToInt` helper), so no `OverflowException` is reachable — reuse the #5 pattern.
- **Concurrency:** blob (Silver) mutations in Apply use a version-safe save (`SetExpectedVersion` with the version just read; on `DbUpdateConcurrencyException` re-read + recompute + save once, then `Guild.Conflict`). Child-row (research/crafting) mutations are per-row (inherently safe). *(This does NOT fix the separate, tracked client-side `_guild.Version`-refresh erosion debt — that's a #7/dedicated-pass item.)*
- **Recursos inflation stays deferred:** the interlude moves maintenance (upkeep) and worker income, neither of which the GDD inflation-adjusts (inflation is for purchases/sales, an unbuilt feature). `InflationIndex` remains computed-but-unused; this sub-plan does not force it.
- **`Ruptura.Shared` stays ZERO project references** — indicator `Kind` is a `string`. Every visible string via `IStringLocalizer` in both Web resx; API error strings in both API resx (the `GuildErrorCodeLocalizationTests` guard from #4 covers new `ErrorCodes.Guild` codes).
- **Integration tests** use `IntegrationTestFactory`, `IClassFixture<>`, `parallelizeTestCollections: false`; lone Serilog flake known — re-run once.
- **Commit after each task** on `main`; end messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:**
- `src/Ruptura.Shared/Guilds/InterludeProjection.cs` (holds `InterludeProjection` + `InterludeIndicator`), `ApplyInterludeRequest.cs`
- `src/Ruptura.Application/Interfaces/IInterludeCalculator.cs`
- `src/Ruptura.Application/Services/InterludeCalculator.cs`
- `src/Ruptura.Web/Pages/GuildInterludeTab.razor`
- `tests/Ruptura.UnitTests/Guilds/InterludeCalculatorTests.cs`
- `tests/Ruptura.IntegrationTests/Guilds/GuildInterludeTests.cs`

**Modify:**
- `src/Ruptura.Application/Common/ErrorCodes.cs` (+ `InterludeDaysInvalid`, `InterludeKindInvalid`)
- `src/Ruptura.Application/Interfaces/IGuildSheetService.cs` (+ preview/apply)
- `src/Ruptura.Infrastructure/Services/GuildSheetService.cs` (preview/apply)
- `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` (register `IInterludeCalculator` singleton)
- `src/Ruptura.API/Controllers/GuildController.cs` (+ preview/apply endpoints)
- `src/Ruptura.API/Resources/*.resx`, Web resx pair (strings)
- `src/Ruptura.Web/Pages/GuildSheet.razor` (mount the Interlúdio tab)
- the Web guild client service (preview/apply methods)

---

### Task 1: Interlude DTOs + error codes

**Files:** create `InterludeProjection.cs`, `ApplyInterludeRequest.cs`; modify `ErrorCodes.cs`.

**Interfaces:**
- Produces: `InterludeProjection`, `InterludeIndicator`, `ApplyInterludeRequest`; `ErrorCodes.Guild.InterludeDaysInvalid`/`.InterludeKindInvalid`.

- [ ] **Step 1: Projection DTOs**

`src/Ruptura.Shared/Guilds/InterludeProjection.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public class InterludeProjection
{
    public int Days { get; set; }
    public List<InterludeIndicator> Indicators { get; set; } = [];
}

// One projected effect of advancing `Days`. The client shows Label + Description and an
// Apply button carrying {Kind, TargetId, Days}; it never sends the numeric deltas below
// (they are display-only — the server recomputes on Apply).
public class InterludeIndicator
{
    public string Kind { get; set; } = string.Empty; // Maintenance|Income|ResearchProgress|CraftingProgress
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; // human-readable, e.g. "Manutenção de 30 dias: -450 Prata"
    public Guid? TargetId { get; set; }                // research/crafting row id; null for Maintenance/Income

    // Display-only projected deltas (nullable per kind):
    public int? SilverDelta { get; set; }             // Maintenance (negative) / Income (positive)
    public int? DaysAdded { get; set; }               // ResearchProgress / CraftingProgress
    public bool? WillComplete { get; set; }           // ResearchProgress / CraftingProgress
    public int? PointsAwarded { get; set; }           // ResearchProgress (only if WillComplete)
}
```

`src/Ruptura.Shared/Guilds/ApplyInterludeRequest.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

// Selector ONLY — no numeric deltas. The server recomputes the effect for {Kind, TargetId, Days}.
public class ApplyInterludeRequest
{
    public string Kind { get; set; } = string.Empty; // Maintenance|Income|ResearchProgress|CraftingProgress
    public Guid? TargetId { get; set; }              // required for ResearchProgress/CraftingProgress
    public int Days { get; set; }
}
```

- [ ] **Step 2: Error codes**

In `ErrorCodes.Guild`:
```csharp
        public const string InterludeDaysInvalid = "Guild.InterludeDaysInvalid";
        public const string InterludeKindInvalid = "Guild.InterludeKindInvalid";
```
(Add en + pt-BR resx strings in Task 3 — the `GuildErrorCodeLocalizationTests` guard requires them.)

- [ ] **Step 3: Build** — `dotnet build` PASS.

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Shared/Guilds src/Ruptura.Application/Common/ErrorCodes.cs
git commit -m "feat: add interlude projection DTOs and error codes

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: `InterludeCalculator` (pure) + unit tests

**Files:** create `IInterludeCalculator.cs`, `InterludeCalculator.cs`; test `InterludeCalculatorTests.cs`.

**Interfaces:**
- Consumes: `GuildDerivedStats`, `ResearchProject`, `CraftingOrder`, `ResearchStage`/`CraftingStatus`.
- Produces: `IInterludeCalculator.Project(GuildDerivedStats derived, IReadOnlyList<ResearchProject> research, IReadOnlyList<CraftingOrder> crafting, int days) → InterludeProjection`.

- [ ] **Step 1: Write the failing unit tests**

`tests/Ruptura.UnitTests/Guilds/InterludeCalculatorTests.cs`:
```csharp
using FluentAssertions;
using Ruptura.Application.Services;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;
using Xunit;

namespace Ruptura.UnitTests.Guilds;

public class InterludeCalculatorTests
{
    private readonly InterludeCalculator _calc = new();

    private static ResearchProject Research(int required, int progress, int researchers, int points, bool complete = false) =>
        new() { Id = Guid.NewGuid(), Name = "R", Complexity = ResearchComplexity.Maior,
                RequiredDays = required, ProgressDays = progress, Researchers = researchers, Points = points, IsComplete = complete };

    private static CraftingOrder Crafting(int required, int progress, CraftingStatus status = CraftingStatus.EmAndamento) =>
        new() { Id = Guid.NewGuid(), Category = CraftingCategory.Forja, ItemName = "Sword",
                RequiredDays = required, ProgressDays = progress, Status = status };

    [Fact]
    public void Maintenance_And_Income_Scale_With_Days()
    {
        var derived = new GuildDerivedStats { DailyMaintenance = 15, WorkerIncomePerDay = 4 };
        var p = _calc.Project(derived, [], [], days: 30);
        p.Days.Should().Be(30);
        p.Indicators.Should().Contain(i => i.Kind == "Maintenance" && i.SilverDelta == -450);
        p.Indicators.Should().Contain(i => i.Kind == "Income" && i.SilverDelta == 120);
    }

    [Fact]
    public void Research_OneResearcher_AdvancesOnePerDay_CapsAtRequired()
    {
        var r = Research(required: 20, progress: 0, researchers: 1, points: 3);
        var p = _calc.Project(new GuildDerivedStats(), [r], [], days: 10);
        var ind = p.Indicators.Single(i => i.Kind == "ResearchProgress" && i.TargetId == r.Id);
        ind.DaysAdded.Should().Be(10);       // min(1,2)*10 = 10
        ind.WillComplete.Should().BeFalse();
        ind.PointsAwarded.Should().Be(0);
    }

    [Fact]
    public void Research_TwoResearchers_HalfTime_Completes_AwardsPoints()
    {
        var r = Research(required: 20, progress: 0, researchers: 2, points: 3);
        var p = _calc.Project(new GuildDerivedStats(), [r], [], days: 10);
        var ind = p.Indicators.Single(i => i.Kind == "ResearchProgress");
        ind.DaysAdded.Should().Be(20);       // min(2,2)*10 = 20 == required
        ind.WillComplete.Should().BeTrue();
        ind.PointsAwarded.Should().Be(3);
    }

    [Fact]
    public void Research_ThreeResearchers_StillCappedAtTwoPerDay()
    {
        var r = Research(required: 20, progress: 0, researchers: 5, points: 3);
        var p = _calc.Project(new GuildDerivedStats(), [r], [], days: 3);
        p.Indicators.Single(i => i.Kind == "ResearchProgress").DaysAdded.Should().Be(6); // min(5,2)*3
    }

    [Fact]
    public void CompletedResearch_ProducesNoIndicator()
    {
        var r = Research(required: 20, progress: 20, researchers: 1, points: 3, complete: true);
        var p = _calc.Project(new GuildDerivedStats(), [r], [], days: 5);
        p.Indicators.Should().NotContain(i => i.Kind == "ResearchProgress");
    }

    [Fact]
    public void Crafting_AdvancesOnePerDay_Completes()
    {
        var c = Crafting(required: 6, progress: 4);
        var p = _calc.Project(new GuildDerivedStats(), [], [c], days: 5);
        var ind = p.Indicators.Single(i => i.Kind == "CraftingProgress" && i.TargetId == c.Id);
        ind.DaysAdded.Should().Be(2);        // capped at required-progress
        ind.WillComplete.Should().BeTrue();
    }

    [Fact]
    public void FinishedOrCancelledCrafting_ProducesNoIndicator()
    {
        var done = Crafting(6, 6, CraftingStatus.Concluido);
        var cancelled = Crafting(6, 0, CraftingStatus.Cancelado);
        var p = _calc.Project(new GuildDerivedStats(), [], [done, cancelled], days: 5);
        p.Indicators.Should().NotContain(i => i.Kind == "CraftingProgress");
    }

    [Fact]
    public void HugeMaintenanceTimesDays_DoesNotOverflow()
    {
        var derived = new GuildDerivedStats { DailyMaintenance = int.MaxValue };
        var act = () => _calc.Project(derived, [], [], days: 3650);
        act.Should().NotThrow();
        _calc.Project(derived, [], [], 3650).Indicators.Single(i => i.Kind == "Maintenance")
            .SilverDelta.Should().Be(int.MinValue); // saturated negative
    }
}
```

- [ ] **Step 2: Run → fail** (`InterludeCalculator` doesn't exist).

- [ ] **Step 3: Interface**

`src/Ruptura.Application/Interfaces/IInterludeCalculator.cs`:
```csharp
using Ruptura.Domain.Entities;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Interfaces;

public interface IInterludeCalculator
{
    InterludeProjection Project(
        GuildDerivedStats derived,
        IReadOnlyList<ResearchProject> research,
        IReadOnlyList<CraftingOrder> crafting,
        int days);
}
```

- [ ] **Step 4: Implement**

`src/Ruptura.Application/Services/InterludeCalculator.cs`:
```csharp
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Services;

public class InterludeCalculator : IInterludeCalculator
{
    public InterludeProjection Project(
        GuildDerivedStats derived,
        IReadOnlyList<ResearchProject> research,
        IReadOnlyList<CraftingOrder> crafting,
        int days)
    {
        var indicators = new List<InterludeIndicator>
        {
            new()
            {
                Kind = "Maintenance",
                Label = "Maintenance",
                SilverDelta = ClampToInt(-(long)derived.DailyMaintenance * days),
                Description = $"{days}d × {derived.DailyMaintenance}/d",
            },
            new()
            {
                Kind = "Income",
                Label = "Income",
                SilverDelta = ClampToInt((long)derived.WorkerIncomePerDay * days),
                Description = $"{days}d × {derived.WorkerIncomePerDay}/d",
            },
        };

        foreach (var r in research.Where(r => !r.IsComplete))
        {
            var perDay = Math.Min(Math.Max(1, r.Researchers), 2);          // min(R,2), floor 1
            var remaining = Math.Max(0, r.RequiredDays - r.ProgressDays);
            var added = (int)Math.Min(remaining, (long)perDay * days);
            var willComplete = r.ProgressDays + added >= r.RequiredDays;
            indicators.Add(new InterludeIndicator
            {
                Kind = "ResearchProgress", Label = r.Name, TargetId = r.Id,
                DaysAdded = added, WillComplete = willComplete,
                PointsAwarded = willComplete ? r.Points : 0,
                Description = $"+{added}d ({r.ProgressDays + added}/{r.RequiredDays})",
            });
        }

        foreach (var c in crafting.Where(c => c.Status == CraftingStatus.EmAndamento))
        {
            var remaining = Math.Max(0, c.RequiredDays - c.ProgressDays);
            var added = (int)Math.Min(remaining, (long)days);
            indicators.Add(new InterludeIndicator
            {
                Kind = "CraftingProgress", Label = c.ItemName, TargetId = c.Id,
                DaysAdded = added, WillComplete = c.ProgressDays + added >= c.RequiredDays,
                Description = $"+{added}d ({c.ProgressDays + added}/{c.RequiredDays})",
            });
        }

        return new InterludeProjection { Days = days, Indicators = indicators };
    }

    private static int ClampToInt(long v) => (int)Math.Clamp(v, int.MinValue, int.MaxValue);
}
```
> `Label`/`Description` here are compact/technical; the UI localizes and formats the user-facing text from the typed fields (`SilverDelta`, `DaysAdded`, etc.), so these server strings are fallbacks, not the primary display. (If you prefer, leave `Description` empty and build it entirely client-side — the typed fields carry all the data.)

- [ ] **Step 5: Run → pass.** `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~InterludeCalculatorTests`

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Application tests/Ruptura.UnitTests/Guilds/InterludeCalculatorTests.cs
git commit -m "feat: add pure InterludeCalculator projecting N days into indicators

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Service preview/apply + endpoints

**Files:** modify `IGuildSheetService.cs`, `GuildSheetService.cs`, `GuildController.cs`, `InfrastructureExtensions.cs`, API resx; test `GuildInterludeTests.cs`.

**Interfaces:**
- Produces: `IGuildSheetService.PreviewInterludeAsync(callerId, campaignId, days)` → `Task<Result<InterludeProjection>>`; `ApplyInterludeAsync(callerId, campaignId, ApplyInterludeRequest)` → `Task<Result<GuildSheetResponse>>`.

- [ ] **Step 1: Write the failing integration tests**

`tests/Ruptura.IntegrationTests/Guilds/GuildInterludeTests.cs`:
```
- Preview: seed Silver, a couple active workers (income), a building (maintenance), an in-progress research + crafting; GET .../interlude/preview?days=10 -> 200 with Maintenance/Income indicators (correct signs/magnitudes) + one ResearchProgress + one CraftingProgress.
- Apply Maintenance: POST apply {Kind:"Maintenance", Days:10} -> 200; GET guild shows Silver reduced by DailyMaintenance*10 (floored at 0 if it would go negative — test the floor with a low Silver).
- Apply Income: -> Silver increased by WorkerIncomePerDay*10.
- Apply ResearchProgress {Kind, TargetId, Days} -> ProgressDays advanced by min(Researchers,2)*Days; if it reaches RequiredDays, IsComplete true AND DerivedStats.CgPesquisa rises by its Points.
- Apply CraftingProgress -> ProgressDays advanced; Status->Concluido when done.
- Security: the request DTO has no delta fields, so there's nothing to spoof — assert applying with a huge Days is capped (days>3650 -> 400 Guild.InterludeDaysInvalid; days<1 -> 400) and that the applied Silver delta equals the SERVER's rate*days, independent of anything else the client sends.
- Bad Kind -> 400 Guild.InterludeKindInvalid. ResearchProgress/CraftingProgress with a missing/foreign TargetId -> 404 (ResearchNotFound/CraftingNotFound). Applying ResearchProgress to an already-complete project -> no-op/404 (pick: it produces no indicator, so 404 ResearchNotFound is fine).
- Non-member preview/apply -> 404.
```

- [ ] **Step 2: Run → fail.**

- [ ] **Step 3: Service interface**

Add to `IGuildSheetService.cs`:
```csharp
    Task<Result<InterludeProjection>> PreviewInterludeAsync(Guid callerId, Guid campaignId, int days, CancellationToken ct = default);
    Task<Result<GuildSheetResponse>> ApplyInterludeAsync(Guid callerId, Guid campaignId, ApplyInterludeRequest request, CancellationToken ct = default);
```

- [ ] **Step 4: Implement**

Inject `IInterludeCalculator interludeCalculator` (already have `researchRepo`/`craftingRepo`/`buildingRepo`/`staffRepo`/`calculator`). Add a private helper that builds the projection from fresh state (reused by preview and apply):
```csharp
    private const int MaxInterludeDays = 3650;

    private async Task<Result<(GuildSheet Guild, InterludeProjection Projection, GuildSheetData Data)>>
        BuildInterludeProjectionAsync(Guid callerId, Guid campaignId, int days, CancellationToken ct)
    {
        if (days < 1 || days > MaxInterludeDays)
            return Result.Failure<(GuildSheet, InterludeProjection, GuildSheetData)>(ErrorCodes.Guild.InterludeDaysInvalid);

        var auth = await AuthorizeAsync(callerId, campaignId, ct);
        if (auth.IsFailure) return Result.Failure<(GuildSheet, InterludeProjection, GuildSheetData)>(auth.Error!);
        var guild = auth.Value!;

        var data = Deserialize(guild.DataJson);
        var buildings = (await buildingRepo.GetByGuildAsync(guild.Id, ct)).ToList();
        var staff = (await staffRepo.GetByGuildAsync(guild.Id, ct)).ToList();
        var research = (await researchRepo.GetByGuildAsync(guild.Id, ct)).ToList();
        var crafting = (await craftingRepo.GetByGuildAsync(guild.Id, ct)).ToList();

        var installationIds = buildings.Select(b => b.CatalogEntryId).Distinct().ToList();
        var installationCatalog = installationIds.Count == 0
            ? new Dictionary<Guid, CatalogEntry>()
            : (await catalogRepo.GetByIdsAsync(installationIds, ct)).ToDictionary(e => e.Id);
        var researchPoints = research.Where(r => r.IsComplete).Sum(r => r.Points);
        var derived = calculator.Calculate(data, buildings, staff, researchPoints, installationCatalog);

        var projection = interludeCalculator.Project(derived, research, crafting, days);
        return Result.Success((guild, projection, data));
    }

    public async Task<Result<InterludeProjection>> PreviewInterludeAsync(
        Guid callerId, Guid campaignId, int days, CancellationToken ct = default)
    {
        var built = await BuildInterludeProjectionAsync(callerId, campaignId, days, ct);
        return built.IsFailure
            ? Result.Failure<InterludeProjection>(built.Error!)
            : Result.Success(built.Value.Projection);
    }

    public async Task<Result<GuildSheetResponse>> ApplyInterludeAsync(
        Guid callerId, Guid campaignId, ApplyInterludeRequest request, CancellationToken ct = default)
    {
        var validKinds = new[] { "Maintenance", "Income", "ResearchProgress", "CraftingProgress" };
        if (!validKinds.Contains(request.Kind))
            return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.InterludeKindInvalid);

        var built = await BuildInterludeProjectionAsync(callerId, campaignId, request.Days, ct);
        if (built.IsFailure) return Result.Failure<GuildSheetResponse>(built.Error!);
        var (guild, projection, data) = built.Value;

        // Select the SERVER-computed indicator matching the client's selector. No client number is trusted.
        var indicator = projection.Indicators.FirstOrDefault(i =>
            i.Kind == request.Kind && i.TargetId == request.TargetId);
        if (indicator is null)
            return Result.Failure<GuildSheetResponse>(request.Kind is "ResearchProgress"
                ? ErrorCodes.Guild.ResearchNotFound
                : request.Kind is "CraftingProgress"
                    ? ErrorCodes.Guild.CraftingNotFound
                    : ErrorCodes.Guild.InterludeKindInvalid);

        switch (request.Kind)
        {
            case "Maintenance":
            case "Income":
                data.Resources.Silver = Math.Max(0, data.Resources.Silver + (indicator.SilverDelta ?? 0));
                guild.DataJson = JsonSerializer.Serialize(data, JsonOpts);
                guild.UpdatedAt = DateTime.UtcNow;
                guildRepo.SetExpectedVersion(guild, guild.Version);
                guildRepo.Update(guild);
                try { await guildRepo.SaveChangesAsync(ct); }
                catch (DbUpdateConcurrencyException) { return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.Conflict); }
                break;

            case "ResearchProgress":
            {
                var r = (await researchRepo.GetByGuildAsync(guild.Id, ct)).FirstOrDefault(x => x.Id == request.TargetId);
                if (r is null || r.IsComplete) return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.ResearchNotFound);
                r.ProgressDays += indicator.DaysAdded ?? 0;
                if (r.ProgressDays >= r.RequiredDays) { r.ProgressDays = r.RequiredDays; r.IsComplete = true; }
                researchRepo.Update(r);
                await researchRepo.SaveChangesAsync(ct);
                break;
            }

            case "CraftingProgress":
            {
                var c = (await craftingRepo.GetByGuildAsync(guild.Id, ct)).FirstOrDefault(x => x.Id == request.TargetId);
                if (c is null || c.Status != CraftingStatus.EmAndamento) return Result.Failure<GuildSheetResponse>(ErrorCodes.Guild.CraftingNotFound);
                c.ProgressDays += indicator.DaysAdded ?? 0;
                if (c.ProgressDays >= c.RequiredDays) { c.ProgressDays = c.RequiredDays; c.Status = CraftingStatus.Concluido; }
                craftingRepo.Update(c);
                await craftingRepo.SaveChangesAsync(ct);
                break;
            }
        }

        // Re-map from fresh state (need a fresh guild fetch for maintenance/income Version bump).
        var refreshed = await guildRepo.GetByCampaignAsync(campaignId, ct);
        return Result.Success(await MapToResponseAsync(refreshed!, ct));
    }
```
> `Deserialize`/`JsonOpts`/`SetExpectedVersion`/`MapToResponseAsync`/`AuthorizeAsync` all exist from prior sub-plans. Confirm the `Result.Failure<(...)>` tuple usage compiles (if the `Result<T>` helper dislikes tuples, return a small private record instead). The research/crafting apply re-fetches the row (rather than reusing the projection's snapshot) so the mutation is on a tracked, current entity.

- [ ] **Step 5: Register the calculator + controller endpoints + resx**

`InfrastructureExtensions.cs`: `services.AddSingleton<IInterludeCalculator, InterludeCalculator>();` (pure, like the others).
`GuildController.cs`:
```csharp
    [HttpGet("campaigns/{campaignId:guid}/guild/interlude/preview")]
    public async Task<IActionResult> InterludePreview(Guid campaignId, [FromQuery] int days, CancellationToken ct) { /* callerId; PreviewInterludeAsync; 200 | 400 InterludeDaysInvalid | 404 */ }

    [HttpPost("campaigns/{campaignId:guid}/guild/interlude/apply")]
    public async Task<IActionResult> InterludeApply(Guid campaignId, [FromBody] ApplyInterludeRequest request, CancellationToken ct) { /* callerId; ApplyInterludeAsync; 200 | 400 (Days/Kind invalid) | 409 Conflict | 404 (not-found/target) */ }
```
Map `InterludeDaysInvalid`/`InterludeKindInvalid` → 400, `Conflict` → 409, `ResearchNotFound`/`CraftingNotFound`/`NotFound` → 404. Add en + pt-BR resx for `Guild.InterludeDaysInvalid` and `Guild.InterludeKindInvalid`.

- [ ] **Step 6: Run tests → pass; full sweep; commit**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildInterludeTests` then `dotnet build && dotnet test`.
```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Guilds/GuildInterludeTests.cs
git commit -m "feat: add interlude preview and per-indicator server-recomputed apply

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 4: Interlúdio tab (Blazor)

**Files:** create `GuildInterludeTab.razor`; modify `GuildSheet.razor` (mount), the Web guild client service (preview/apply), Web resx pair.

- [ ] **Step 1: Client methods** — add `PreviewInterludeAsync(campaignId, days)` (`GET .../interlude/preview?days=N` → `InterludeProjection`) and `ApplyInterludeAsync(campaignId, ApplyInterludeRequest)` (`POST .../interlude/apply` → `GuildSheetResponse`; surface non-200 distinctly for localized toasts) to the Web guild client, following the existing client conventions.

- [ ] **Step 2: `GuildInterludeTab`** — a day-count number input (min 1, max 3650) + a "Preview" button that calls `PreviewInterludeAsync` and lists the returned indicators. For each indicator render a localized Label + a formatted Description built from the typed fields (Maintenance/Income: `SilverDelta` Prata; Research/Crafting: `DaysAdded` + `WillComplete` + `PointsAwarded`) and an **Apply** button that calls `ApplyInterludeAsync({ Kind, TargetId, Days })`; on success toast + replace `_guild` from the response (updates DerivedStats, Version, and the affected child lists — this is a full guild refresh, appropriate here since Apply mutates server state and returns the whole guild) and **re-run Preview** so the remaining indicators reflect the new state; on 409 toast the localized conflict message + re-run Preview. Group indicators by kind for readability. Show a note that secondary expeditions are logged manually on the Expedições tab.

- [ ] **Step 3: Mount + i18n** — add the Interlúdio tab to `GuildSheet.razor`. Every visible string in BOTH Web resx: tab title, "days" label, Preview/Apply buttons, the four kind labels (Maintenance/Income/Research/Crafting), delta formatting templates, the "completed" / "points awarded" bits, the secondary-expeditions note, conflict/success toasts. English default + pt-BR.

- [ ] **Step 4: Build + verify + commit**

Run: `dotnet build` (clean). If feasible, run the app and confirm: preview shows indicators for a guild with maintenance/income/research/crafting; applying Maintenance lowers Silver; applying a 2-researcher research to completion bumps the Capacidades CG. Else confirm clean build and note it.
```bash
git add src/Ruptura.Web
git commit -m "feat: add guild Interlúdio tab (preview + per-indicator apply)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage (§5, §7 Interlúdio tab, §12.6):**
- Projection (read-only preview) → Task 2 calculator + Task 3 `PreviewInterludeAsync`. ✓
- Per-indicator apply, server-recomputed, selector-only request → Task 3 `ApplyInterludeAsync` (reuses `Project` for the authoritative delta). ✓
- Four indicator kinds (Maintenance/Income/ResearchProgress/CraftingProgress) → Task 2. ✓
- Research advancement min(R,2) with 50% floor; crafting advancement; maintenance floor-at-0 → Task 2/3 (user decisions). ✓
- UI tab → Task 4. ✓
- Wires into CG: a completed research (via ResearchProgress apply) raises CgPesquisa on the next read (the #5 wiring) — tested in Task 3. ✓
- **Deliberately deferred (not gaps):** SecondaryExpedition (user decision — no merc NP modeled; manual via Expedições tab); Recursos inflation (interlude doesn't purchase/sell, so it doesn't force the still-open inflation rule); the client-side `_guild.Version`-refresh concurrency erosion (tracked #7/dedicated-pass debt — this sub-plan's server-side apply is version-safe, but does not fix the separate UI erosion).

**2. Placeholder scan:** Backend Tasks 1–3 carry complete code (calculator, service preview/apply). Controller endpoints (Task 3 Step 5) and UI (Task 4) are described structurally with exact routes/DTOs/patterns to mirror — concrete Razor/client conventions read from the repo at execution, same posture as prior sub-plans. No "TBD"/"handle appropriately".

**3. Type consistency:** `IInterludeCalculator.Project(GuildDerivedStats, IReadOnlyList<ResearchProject>, IReadOnlyList<CraftingOrder>, int)` identical in interface, impl, tests, and the service's `BuildInterludeProjectionAsync`. `ApplyInterludeRequest { Kind (string), TargetId (Guid?), Days (int) }` carries no numeric delta (security invariant) and is consumed by `ApplyInterludeAsync` + the controller + client. `InterludeIndicator` typed delta fields (`SilverDelta`/`DaysAdded`/`WillComplete`/`PointsAwarded`) produced by the calculator and read by both the apply switch and the UI. `ClampToInt` overflow-safety mirrors `GuildStatsCalculator`. Reuses existing `AuthorizeAsync`/`Deserialize`/`JsonOpts`/`SetExpectedVersion`/`MapToResponseAsync`/`calculator.Calculate`.
