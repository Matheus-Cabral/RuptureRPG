# Player Pages Rollout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Apply the design-system rework's usability toolkit (already merged, demonstrated on `GmCatalog` and the rest of the GM page group) to the Player role's UI surface — `PlayerDashboard`, `PlayerCampaigns`.

**Architecture:** No new components or services. The Player role currently has almost no standalone UI: `PlayerDashboard` is 12 lines of static placeholder text (no data fetch, no table), and `PlayerCampaigns` is a single small read-only table. Most of what a player actually interacts with lives inside `CharacterSheetEditor`, which is its own future 11-tab rollout phase. This plan is proportionally small because the real surface area is small — it is not a truncated version of a bigger plan.

**Tech Stack:** ASP.NET Core Blazor WebAssembly 8, plain CSS (existing tokens/classes, no new ones needed), `IStringLocalizer<AppStrings>`, the existing `LoadingIndicator` component.

## Global Constraints

- No font-size anywhere below `--text-2xs` (11px).
- Reuse existing toolkit components and CSS classes/tokens — no new `app.css` rules and no new resx keys are needed for this plan (neither page gains a feature that would introduce new user-facing text; both already have every string they display).
- No search, no toasts, no confirmation dialog: `PlayerDashboard` has no data to search or act on, and `PlayerCampaigns` is a personal membership list (typically 1-3 rows) with no destructive action — `TableSearchBox`/`ToastService`/`ConfirmDialog` have no legitimate wiring site on either page. This mirrors the read-only treatment `GmPlayers` got *before* search was added — the difference is `GmPlayers` shows the GM's entire recruited roster (unbounded, grows over a campaign's life), while a player's own campaign list does not.

**Full design spec:** `docs/superpowers/specs/2026-08-06-design-system-rework-design.md`
**Foundation plan (already merged):** `docs/superpowers/plans/2026-08-06-design-system-rework.md`
**Reference implementation to follow:** `src/Ruptura.Web/Pages/GmPlayers.razor` (the same "read-only list, no actions" shape, from the GM pages rollout)

## Out of Scope

- `PlayerFields.razor` — despite the name, this is the Player-role registration fields partial used by `Register.razor`, not a Player-role page. Already reviewed and confirmed clean as part of the Auth pages rollout.
- `PlayerCharacter.razor` — a thin wrapper (like `GmCharacterSheet.razor`, deferred in the GM pages rollout for the same reason) that shows either an "awaiting grant" empty state or `CharacterSheetEditor`. No tables, no hardcoded styles, no untranslated strings today. The 11-tab character sheet editor is its own future rollout phase where this wrapper's context (which campaign, which character) belongs.
- `Breadcrumbs` on any page in this plan — `PlayerCampaigns` is a top-level list (no parent context, same as `GmCampaigns`), and `PlayerCharacter` (where a breadcrumb would make sense: Campaigns → Character) is out of scope per the point above.
- No backend changes.

---

## File Structure

```
src/Ruptura.Web/Pages/
├── PlayerDashboard.razor   Modify — token cleanup only (two hardcoded inline font-sizes)
└── PlayerCampaigns.razor   Modify — LoadingIndicator, mobile stack-table
```

---

## Task 1: PlayerDashboard (token cleanup)

**Files:**
- Modify: `src/Ruptura.Web/Pages/PlayerDashboard.razor`

**Interfaces:** none — this page has no data fetch and no `@code` block; the only change is two inline `font-size` values.

`PlayerDashboard` is a static placeholder (no service injection beyond localization, no `@code` block at all) shown regardless of whether the player has an active character — that behavior is unchanged, pre-existing, and out of scope for a design-system pass. The two hardcoded `font-size:.85rem`/`.8rem` values are replaced with type-scale tokens, which also slightly increases the visual hierarchy between the title and subtitle line (14px vs 12px, versus the original 13.6px vs 12.8px).

- [ ] **Step 1: Replace `PlayerDashboard.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@inject IStringLocalizer<AppStrings> L

<div class="ledger-empty" style="text-align:left;padding:2rem 0;border-top:1px solid var(--border)">
    <p style="margin-bottom:.5rem;font-family:var(--font-display);font-size:var(--text-sm);letter-spacing:.04em;color:var(--text)">
        @L["Dashboard.Player.Title"]
    </p>
    <p style="color:var(--text-muted);font-size:var(--text-xs);margin-bottom:0">
        @L["Dashboard.Player.Subtitle"]
    </p>
</div>
```

- [ ] **Step 2: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add src/Ruptura.Web/Pages/PlayerDashboard.razor
git commit -m "fix: replace hardcoded font-sizes with type-scale tokens in PlayerDashboard"
```

---

## Task 2: PlayerCampaigns (list)

**Files:**
- Modify: `src/Ruptura.Web/Pages/PlayerCampaigns.razor`

**Interfaces:**
- Consumes: `LoadingIndicator` (`Text` param) — pre-existing, globally available via `_Imports.razor`.

Read-only page, no create/edit/delete, no search (see Global Constraints). Only the loading state and mobile table treatment change.

- [ ] **Step 1: Replace `PlayerCampaigns.razor`**

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
        <LoadingIndicator Text="@L["Common.Loading"]" />
    }
    else if (_campaigns.Count == 0)
    {
        <div class="ledger-empty"><p>@L["Campaigns.Empty"]</p></div>
    }
    else
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table stack-mobile">
                <thead><tr><th>@L["Gm.CampaignDetail.Col.Name"]</th><th></th></tr></thead>
                <tbody>
                    @foreach (var campaign in _campaigns)
                    {
                        <tr>
                            <td data-label="@L["Gm.CampaignDetail.Col.Name"]">@campaign.Name</td>
                            <td data-label=""><a class="btn btn-outline-secondary btn-sm" href="/campaigns/@campaign.Id/character">@L["Campaigns.OpenCharacter"]</a></td>
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

- [ ] **Step 2: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 3: Commit**

```bash
git add src/Ruptura.Web/Pages/PlayerCampaigns.razor
git commit -m "feat: wire loading indicator and mobile table into PlayerCampaigns"
```

---

## Task 3: Final verification — full build, full test run, visual pass

**Files:** none prescribed — this task may touch whatever file a genuine regression it finds lives in (same allowance as prior plans' final verification tasks).

**Interfaces:** none — this task confirms both earlier tasks' deliverables together.

- [ ] **Step 1: Build and test**

Run: `dotnet build` (whole solution), then `dotnet test tests/Ruptura.UnitTests`
Expected: 0 build errors; all tests passing (this plan adds no new tests, so the count should match whatever `main` currently has).

- [ ] **Step 2: Launch the app and capture screenshots**

Use the `run` skill to build and serve `Ruptura.Web`. As a Player account (registered via an invite code, per `CLAUDE.md`) with at least one campaign membership, capture screenshots of:

- `/dashboard` (Player view, renders `PlayerDashboard`) — light and dark, desktop (≥1024px) and mobile (<600px).
- `/campaigns` (`PlayerCampaigns`) — light and dark, desktop and mobile; both with at least one campaign row and, if practical, the empty state (an account with zero campaign memberships).

Confirm for each: no text below 11px, no low-contrast text, `PlayerDashboard`'s title/subtitle read with clear size hierarchy, `PlayerCampaigns`' table renders as a stacked mobile card at <600px (not squeezed), and the "Open Character" link works.

- [ ] **Step 3: Fix anything the visual pass surfaces**

If a screenshot shows a real regression (not pre-existing/out-of-scope — only things these two pages actually have wrong), fix it directly, keeping the fix small and targeted, then re-verify. If a finding needs a design decision rather than a bug fix, stop and report `DONE_WITH_CONCERNS`/`BLOCKED` describing it instead of improvising.

- [ ] **Step 4: Commit any fixes made**

If Step 3 produced changes:

```bash
git add -A
git commit -m "fix: address Player-pages visual regressions found in verification pass"
```

If Step 3 found nothing to fix, skip this step.

- [ ] **Step 5: Report**

Summarize what was verified and confirm the Player page group (minus `PlayerCharacter`/`PlayerFields`, out of scope per this plan) is now aligned with the design system. Note that the last remaining group per the spec's §8 rollout order is the 11-tab Character Sheet editor (`CharacterSheetEditor` + its tab components), which needs its own plan — and that `PlayerCharacter.razor`/`GmCharacterSheet.razor` (both deferred so far) will most naturally be revisited as part of that plan, since they're the wrapper pages around the editor those tabs live in.
