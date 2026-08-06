# GM Pages Rollout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Apply the design-system rework's usability toolkit (already merged, demonstrated on `GmCatalog`) to the rest of the Game Master page group — `GmDashboard`, `GmPlayers`, `GmCampaigns`, `GmCampaignDetail`, `GmNotifications` — replacing ad-hoc loading/feedback patterns with the shared components, adding client-side search to the three browsing lists, wiring `Breadcrumbs` into its first live usage, and making every table on this route mobile-friendly.

**Architecture:** No new components or services — everything these five pages need (`ToastService`, `LoadingIndicator`, `TableSearchBox`/`TableFilter`, `Breadcrumbs`) already exists from the design-system rework and is proven on `GmCatalog`. This plan is pure application of that toolkit plus small, targeted cleanups (hardcoded `.78rem`/`.8rem` styles, an ad-hoc copy-feedback flag that duplicates what `ToastService` already does, inline `alert-danger` banners that duplicate what a toast now covers).

**Tech Stack:** ASP.NET Core Blazor WebAssembly 8, plain CSS (existing tokens/classes, no new ones needed), `IStringLocalizer<AppStrings>`, the existing `ToastService`/`LoadingIndicator`/`TableSearchBox`/`TableFilter`/`Breadcrumbs` toolkit.

## Global Constraints

- No font-size anywhere below `--text-2xs` (11px).
- Every new/changed user-facing string goes through `IStringLocalizer<AppStrings>` with a key in **both** `AppStrings.resx` (English) and `AppStrings.pt-BR.resx` (Portuguese).
- Reuse existing toolkit components and CSS classes/tokens — no new `app.css` rules are needed for this plan (`.ledger-table.stack-mobile`, `.ledger-empty`, `.loading-inline`, `.toast-*`, `.breadcrumbs` all already exist).
- Toasts replace inline `alert-danger` banners for transient action outcomes (create/assign/grant/promote/dismiss/copy) — don't show both for the same event. Keep an inline banner only where a page needs a *persistent* error state across renders (none of the pages in this plan need that: every existing `_errorMessage` here is set exclusively by transient actions, never by the initial `LoadAsync`, so every one of them converts cleanly to a toast with no inline banner left behind).
- No destructive/delete actions exist on any of these five pages, so `ConfirmDialog` has no wiring site here — that's expected, not a gap.

**Full design spec:** `docs/superpowers/specs/2026-08-06-design-system-rework-design.md`
**Foundation plan (already merged):** `docs/superpowers/plans/2026-08-06-design-system-rework.md`
**Reference implementation to follow:** `src/Ruptura.Web/Pages/GmCatalog.razor` (already wired with this exact toolkit)

## Out of Scope

- `GmFields.razor` — despite the name, this is the Game-Master-role registration fields partial used by `Register.razor`, not a GM-role page. It was already reviewed and needs no changes as part of the Auth pages rollout.
- `GmCharacterSheet.razor` — a 16-line wrapper around `CharacterSheetEditor`. It has no tables, no hardcoded styles, and no untranslated strings today; the 11-tab character sheet editor itself is its own future rollout phase where a proper breadcrumb/header treatment (campaign + character context) belongs. Not touched here.
- Adding a `Breadcrumbs` trail to `GmCharacterSheet.razor` — deferred for the same reason: doing it well needs an extra sheet-name fetch that's better designed alongside that phase's own work, not bolted on here.
- Adding `TableSearchBox` to `GmCampaignDetail`'s members/character-sheets tables — these are scoped to one campaign (naturally small), unlike the global Players/Campaigns/Invites lists. Can be added later if a campaign's roster grows large enough to need it.
- No backend changes. `ICampaignClientService` already exposes `GetMineAsync()` (a GM's own campaigns), which Task 4 uses to resolve a campaign's display name — no new API endpoint is added.

---

## File Structure

```
src/Ruptura.Web/
├── Pages/
│   ├── GmDashboard.razor         Modify — TableSearchBox, ToastService (replaces ad-hoc copy-feedback flag), LoadingIndicator, stack-mobile, token cleanup
│   ├── GmPlayers.razor           Modify — TableSearchBox, LoadingIndicator, stack-mobile, token cleanup
│   ├── GmCampaigns.razor         Modify — TableSearchBox, ToastService, LoadingIndicator, stack-mobile, token cleanup
│   ├── GmCampaignDetail.razor    Modify — Breadcrumbs (first live usage), ToastService, LoadingIndicator, stack-mobile x2, token cleanup
│   └── GmNotifications.razor     Modify — ToastService, LoadingIndicator, stack-mobile, token cleanup
└── Resources/
    ├── AppStrings.resx           Modify — 8 new keys across the 5 pages
    └── AppStrings.pt-BR.resx     Modify — matching 8 keys
```

---

## Task 1: GmDashboard (invite codes)

**Files:**
- Modify: `src/Ruptura.Web/Pages/GmDashboard.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ToastService` (`Show`/`Success`/`Error`, `ToastLevel`), `LoadingIndicator` (`Text` param), `TableSearchBox` (`@bind-Value`, `Placeholder`), `TableFilter.Matches(term, params fields)` — all pre-existing from the design-system rework, all globally available via `_Imports.razor`.

`CopyAsync` currently hand-rolls its own "Copied!" feedback with a `_copied` bool, a `StateHasChanged()` call, and a `Task.Delay(2000)` — this is exactly what `ToastService` already does, so this task deletes that flag entirely in favor of a real toast. `_errorMessage`/`alert-danger` is used only for the two transient failures (generate, copy) — both become toasts, and the inline banner is removed.

- [ ] **Step 1: Add the `Gm.Invites.SearchPlaceholder` resx key**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the `Gm.Invites.Status.Expired` line and add directly after it:

```xml
  <data name="Gm.Invites.SearchPlaceholder"><value>Search invite codes…</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the matching `Gm.Invites.Status.Expired` line and add directly after it:

```xml
  <data name="Gm.Invites.SearchPlaceholder"><value>Buscar códigos de convite…</value></data>
```

- [ ] **Step 2: Replace `GmDashboard.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Shared.Invites
@using Ruptura.Web.Resources
@inject IStringLocalizer<AppStrings> L
@inject IInviteClientService InviteService
@inject IJSRuntime JS
@inject ToastService Toast

<!-- Stats -->
<div class="ledger-stats">
    <div class="stat">
        <div class="stat-num">@_codes.Count</div>
        <div class="stat-label">@L["Gm.Invites.Total"]</div>
    </div>
    <div class="stat s-active">
        <div class="stat-num">@ActiveCount</div>
        <div class="stat-label">@L["Gm.Invites.Active"]</div>
    </div>
    <div class="stat s-used">
        <div class="stat-num">@UsedCount</div>
        <div class="stat-label">@L["Gm.Invites.Used"]</div>
    </div>
    <div class="stat s-expired">
        <div class="stat-num">@ExpiredCount</div>
        <div class="stat-label">@L["Gm.Invites.Expired"]</div>
    </div>
</div>

<!-- New code banner -->
@if (_newCode is not null)
{
    <div class="new-code-banner">
        <div>
            <div class="banner-label">@L["Gm.Invites.NewCode"]</div>
            <div class="banner-code">@_newCode.Code</div>
        </div>
        <button class="btn btn-outline-secondary btn-sm" @onclick="() => CopyAsync(_newCode.Code)">
            @L["Gm.Invites.Copy"]
        </button>
        <span class="banner-note">@L["Gm.Invites.ExpiresIn", "48h"]</span>
    </div>
}

<!-- Section header -->
<div class="section-header">
    <span class="section-title">@L["Gm.Invites.Title"]</span>
    <button class="btn btn-primary btn-sm" @onclick="GenerateAsync" disabled="@_generating">
        @if (_generating) { <span class="spinner-border spinner-border-sm me-1"></span> }
        @L["Gm.Invites.Generate"]
    </button>
</div>

<!-- Table -->
@if (_loading)
{
    <LoadingIndicator Text="@L["Common.Loading"]" />
}
else if (_codes.Count == 0)
{
    <div class="ledger-empty">
        <p>@L["Gm.Invites.Empty"]</p>
        <button class="btn btn-primary btn-sm" @onclick="GenerateAsync">
            @L["Gm.Invites.GenerateFirst"]
        </button>
    </div>
}
else
{
    <TableSearchBox @bind-Value="_searchTerm" Placeholder="@L["Gm.Invites.SearchPlaceholder"]" />

    @if (FilteredCodes.Count == 0)
    {
        <div class="ledger-empty">
            <p>@L["Gm.Invites.Empty"]</p>
        </div>
    }
    else
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table stack-mobile">
                <thead>
                    <tr>
                        <th>@L["Gm.Invites.Col.Code"]</th>
                        <th>@L["Gm.Invites.Col.Status"]</th>
                        <th>@L["Gm.Invites.Col.Created"]</th>
                        <th>@L["Gm.Invites.Col.Expires"]</th>
                        <th>@L["Gm.Invites.Col.RedeemedAt"]</th>
                        <th>@L["Gm.Invites.Col.RedeemedBy"]</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in FilteredCodes)
                    {
                        var itemCode = item.Code;
                        <tr>
                            <td data-label="@L["Gm.Invites.Col.Code"]">
                                <span class="code-chip" @onclick="() => CopyAsync(itemCode)"
                                      title="@L["Gm.Invites.Copy"]">
                                    @item.Code
                                </span>
                            </td>
                            <td data-label="@L["Gm.Invites.Col.Status"]"><StatusBadge Code="item" /></td>
                            <td data-label="@L["Gm.Invites.Col.Created"]" style="color:var(--text-muted);font-size:var(--text-xs);white-space:nowrap">
                                @item.CreatedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")
                            </td>
                            <td data-label="@L["Gm.Invites.Col.Expires"]" style="color:var(--text-muted);font-size:var(--text-xs);white-space:nowrap">
                                @(item.IsUsed ? "—" : item.ExpiresAt.ToLocalTime().ToString("dd/MM/yy HH:mm"))
                            </td>
                            <td data-label="@L["Gm.Invites.Col.RedeemedAt"]" style="color:var(--text-muted);font-size:var(--text-xs);white-space:nowrap">
                                @(item.UsedAt is { } usedAt ? usedAt.ToLocalTime().ToString("dd/MM/yy HH:mm") : "—")
                            </td>
                            <td data-label="@L["Gm.Invites.Col.RedeemedBy"]" style="color:var(--text-muted);font-size:var(--text-xs);white-space:nowrap">
                                @(item.IsUsed ? $"{item.RedeemedByDisplayName} ({item.RedeemedByEmail})" : "—")
                            </td>
                            <td data-label="">
                                @if (!item.IsUsed && item.ExpiresAt > DateTime.UtcNow)
                                {
                                    <button class="copy-btn" @onclick="() => CopyAsync(itemCode)"
                                            title="@L["Gm.Invites.Copy"]">⎘</button>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    }
}

@code {
    private List<InviteCodeResponse> _codes = [];
    private InviteCodeResponse? _newCode;
    private bool _loading = true;
    private bool _generating;
    private string _searchTerm = string.Empty;

    private int ActiveCount  => _codes.Count(i => !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);
    private int UsedCount    => _codes.Count(i => i.IsUsed);
    private int ExpiredCount => _codes.Count(i => !i.IsUsed && i.ExpiresAt <= DateTime.UtcNow);

    private List<InviteCodeResponse> FilteredCodes =>
        _codes.Where(i => TableFilter.Matches(_searchTerm, i.Code, i.RedeemedByDisplayName, i.RedeemedByEmail)).ToList();

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        var result = await InviteService.GetAllAsync();
        _codes = result?.Data?.ToList() ?? [];
        _loading = false;
    }

    private async Task GenerateAsync()
    {
        _generating = true;
        _newCode = null;

        var result = await InviteService.GenerateAsync();
        if (result?.Data is not null)
        {
            _newCode = result.Data;
            _codes.Insert(0, result.Data);
        }
        else
        {
            Toast.Error(L["Common.Error"]);
        }
        _generating = false;
    }

    private async Task CopyAsync(string text)
    {
        var success = await JS.InvokeAsync<bool>("ruptura.copyToClipboard", text);
        Toast.Show(success ? L["Gm.Invites.Copied"] : L["Gm.Invites.CopyFailed"],
            success ? ToastLevel.Success : ToastLevel.Error);
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Pages/GmDashboard.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: wire usability toolkit into GmDashboard invite-codes table"
```

---

## Task 2: GmPlayers (roster)

**Files:**
- Modify: `src/Ruptura.Web/Pages/GmPlayers.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `LoadingIndicator`, `TableSearchBox`, `TableFilter.Matches`.

This is a read-only page (no create/delete/edit) — only search, loading, and the mobile table need wiring.

- [ ] **Step 1: Add the `Gm.Players.SearchPlaceholder` resx key**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the `Gm.Players.Col.RecruitedAt` line and add directly after it:

```xml
  <data name="Gm.Players.SearchPlaceholder"><value>Search players…</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the matching `Gm.Players.Col.RecruitedAt` line and add directly after it:

```xml
  <data name="Gm.Players.SearchPlaceholder"><value>Buscar jogadores…</value></data>
```

- [ ] **Step 2: Replace `GmPlayers.razor`**

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
        <LoadingIndicator Text="@L["Common.Loading"]" />
    }
    else if (_players.Count == 0)
    {
        <div class="ledger-empty">
            <p>@L["Gm.Players.Empty"]</p>
        </div>
    }
    else
    {
        <TableSearchBox @bind-Value="_searchTerm" Placeholder="@L["Gm.Players.SearchPlaceholder"]" />

        @if (FilteredPlayers.Count == 0)
        {
            <div class="ledger-empty">
                <p>@L["Gm.Players.Empty"]</p>
            </div>
        }
        else
        {
            <div class="ledger-table-wrap">
                <table class="ledger-table stack-mobile">
                    <thead>
                        <tr>
                            <th>@L["Gm.Players.Col.Name"]</th>
                            <th>@L["Gm.Players.Col.Email"]</th>
                            <th>@L["Gm.Players.Col.RecruitedAt"]</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var player in FilteredPlayers)
                        {
                            <tr>
                                <td data-label="@L["Gm.Players.Col.Name"]">@player.DisplayName</td>
                                <td data-label="@L["Gm.Players.Col.Email"]">@player.Email</td>
                                <td data-label="@L["Gm.Players.Col.RecruitedAt"]" style="color:var(--text-muted);font-size:var(--text-xs);white-space:nowrap">
                                    @player.RecruitedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        }
    }
</div>

@code {
    private List<PlayerRosterResponse> _players = [];
    private bool _loading = true;
    private string _searchTerm = string.Empty;

    private List<PlayerRosterResponse> FilteredPlayers =>
        _players.Where(p => TableFilter.Matches(_searchTerm, p.DisplayName, p.Email)).ToList();

    protected override async Task OnInitializedAsync()
    {
        var result = await CampaignService.GetRosterAsync();
        _players = result?.Data?.ToList() ?? [];
        _loading = false;
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Pages/GmPlayers.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: wire search and mobile table into GmPlayers roster"
```

---

## Task 3: GmCampaigns (list + create)

**Files:**
- Modify: `src/Ruptura.Web/Pages/GmCampaigns.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ToastService`, `LoadingIndicator`, `TableSearchBox`, `TableFilter.Matches`.

`CreateAsync` currently gives no success feedback at all (the new row just appears) and shows failures only via an inline banner. Both become toasts.

- [ ] **Step 1: Add the `Gm.Campaigns.SearchPlaceholder` and `Gm.Campaigns.CreateSuccess` resx keys**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the `Gm.Campaigns.View` line and add directly after it:

```xml
  <data name="Gm.Campaigns.SearchPlaceholder"><value>Search campaigns…</value></data>
  <data name="Gm.Campaigns.CreateSuccess"><value>Campaign created.</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the matching `Gm.Campaigns.View` line and add directly after it:

```xml
  <data name="Gm.Campaigns.SearchPlaceholder"><value>Buscar campanhas…</value></data>
  <data name="Gm.Campaigns.CreateSuccess"><value>Campanha criada.</value></data>
```

- [ ] **Step 2: Replace `GmCampaigns.razor`**

```razor
@page "/gm/campaigns"
@attribute [Authorize(Roles = "GameMaster")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Campaigns
@inject IStringLocalizer<AppStrings> L
@inject ICampaignClientService CampaignService
@inject ToastService Toast
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

    @if (_loading)
    {
        <LoadingIndicator Text="@L["Common.Loading"]" />
    }
    else if (_campaigns.Count == 0)
    {
        <div class="ledger-empty">
            <p>@L["Gm.Campaigns.Empty"]</p>
        </div>
    }
    else
    {
        <TableSearchBox @bind-Value="_searchTerm" Placeholder="@L["Gm.Campaigns.SearchPlaceholder"]" />

        @if (FilteredCampaigns.Count == 0)
        {
            <div class="ledger-empty">
                <p>@L["Gm.Campaigns.Empty"]</p>
            </div>
        }
        else
        {
            <div class="ledger-table-wrap">
                <table class="ledger-table stack-mobile">
                    <thead>
                        <tr>
                            <th>@L["Gm.Campaigns.Col.Name"]</th>
                            <th>@L["Gm.Campaigns.Col.CreatedAt"]</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var campaign in FilteredCampaigns)
                        {
                            <tr>
                                <td data-label="@L["Gm.Campaigns.Col.Name"]">@campaign.Name</td>
                                <td data-label="@L["Gm.Campaigns.Col.CreatedAt"]" style="color:var(--text-muted);font-size:var(--text-xs);white-space:nowrap">
                                    @campaign.CreatedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")
                                </td>
                                <td data-label="">
                                    <button class="btn btn-outline-secondary btn-sm"
                                            @onclick="() => NavigateToCampaign(campaign.Id)">
                                        @L["Gm.Campaigns.View"]
                                    </button>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        }
    }
</div>

@code {
    private List<CampaignResponse> _campaigns = [];
    private bool _loading = true;
    private bool _creating;
    private string _newName = string.Empty;
    private string _searchTerm = string.Empty;

    private List<CampaignResponse> FilteredCampaigns =>
        _campaigns.Where(c => TableFilter.Matches(_searchTerm, c.Name)).ToList();

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

        var result = await CampaignService.CreateAsync(new CreateCampaignRequest { Name = _newName });
        if (result?.Data is not null)
        {
            Toast.Success(L["Gm.Campaigns.CreateSuccess"]);
            _campaigns.Insert(0, result.Data);
            _newName = string.Empty;
        }
        else
        {
            Toast.Error(L["Common.Error"]);
        }

        _creating = false;
    }

    private void NavigateToCampaign(Guid id) => Nav.NavigateTo($"/gm/campaigns/{id}");
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Pages/GmCampaigns.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: wire usability toolkit into GmCampaigns list"
```

---

## Task 4: GmCampaignDetail (members, characters, breadcrumbs)

**Files:**
- Modify: `src/Ruptura.Web/Pages/GmCampaignDetail.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ToastService`, `LoadingIndicator`, `Breadcrumbs` (`Items` param) + `BreadcrumbItem(string Text, string? Href)` — this is `Breadcrumbs`' first live usage anywhere in the app. `Nav.Campaigns` (existing resx key, "Campaigns"/"Campanhas") is reused as the first crumb's label, linking to `/gm/campaigns`. `ICampaignClientService.GetMineAsync()` (existing method, already used elsewhere) resolves the current campaign's display name for the second (current-page, unlinked) crumb — no new API surface.

This is the largest task in the group: two tables (members, character sheets) both need `stack-mobile` + `data-label`, two write actions (assign, grant) both get toasts, and the page gains its first breadcrumb trail. The existing player-search autocomplete (a different, already-working pattern for finding a *new* player to assign) is untouched — it is not `TableSearchBox`, which filters an already-rendered table.

- [ ] **Step 1: Add the `Gm.CampaignDetail.AssignSuccess` and `Gm.CampaignDetail.GrantSuccess` resx keys**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the `Gm.CampaignDetail.OpenSheet` line and add directly after it:

```xml
  <data name="Gm.CampaignDetail.AssignSuccess"><value>Player assigned to the campaign.</value></data>
  <data name="Gm.CampaignDetail.GrantSuccess"><value>Character granted.</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the matching `Gm.CampaignDetail.OpenSheet` line and add directly after it:

```xml
  <data name="Gm.CampaignDetail.AssignSuccess"><value>Jogador adicionado à campanha.</value></data>
  <data name="Gm.CampaignDetail.GrantSuccess"><value>Personagem concedido.</value></data>
```

- [ ] **Step 2: Replace `GmCampaignDetail.razor`**

```razor
@page "/gm/campaigns/{Id:guid}"
@attribute [Authorize(Roles = "GameMaster")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Campaigns
@using Ruptura.Shared.CharacterSheets
@inject IStringLocalizer<AppStrings> L
@inject ICampaignClientService CampaignService
@inject ICharacterSheetClientService SheetService
@inject ToastService Toast

<PageTitle>@L["Gm.CampaignDetail.Members"] — RUPTURA</PageTitle>

<div class="page-content">
    <Breadcrumbs Items="_breadcrumbs" />

    <div class="page-heading">
        <h1>@L["Gm.CampaignDetail.Members"]</h1>
        <a href="/gm/campaigns/@Id/catalog" class="btn btn-outline-secondary btn-sm" style="margin-top:.75rem">
            @L["Gm.CampaignDetail.ViewCatalog"]
        </a>
    </div>

    <div class="section-header">
        <span class="section-title">@L["Gm.CampaignDetail.Members"]</span>
        @if (_availablePlayers.Count > 0)
        {
            <div style="display:flex;gap:.5rem">
                <div class="autocomplete" style="position:relative;width:260px">
                    <input class="form-control" placeholder="@L["Gm.CampaignDetail.SearchPlaceholder"]"
                           value="@_playerSearch"
                           @oninput="OnSearchInput" @onfocus="() => _showSuggestions = true"
                           @onfocusout="() => _showSuggestions = false" />
                    @if (_showSuggestions && FilteredAvailablePlayers.Any())
                    {
                        <div class="autocomplete-list">
                            @foreach (var player in FilteredAvailablePlayers)
                            {
                                <div class="autocomplete-item" @onmousedown="() => SelectPlayer(player)" @onmousedown:preventDefault>
                                    @player.DisplayName (@player.Email)
                                </div>
                            }
                        </div>
                    }
                </div>
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
        <LoadingIndicator Text="@L["Common.Loading"]" />
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
            <table class="ledger-table stack-mobile">
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
                            <td data-label="@L["Gm.CampaignDetail.Col.Name"]">@member.DisplayName</td>
                            <td data-label="@L["Gm.CampaignDetail.Col.Email"]">@member.Email</td>
                            <td data-label="@L["Gm.CampaignDetail.Col.AssignedAt"]" style="color:var(--text-muted);font-size:var(--text-xs);white-space:nowrap">
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
        <p style="color:var(--text-muted);font-size:var(--text-sm);margin-top:1rem">
            @L["Gm.CampaignDetail.NoAvailablePlayers"]
        </p>
    }

    <div class="section-header" style="margin-top:2rem">
        <span class="section-title">@L["Gm.CampaignDetail.Characters"]</span>
    </div>

    @if (_sheets.Count > 0)
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table stack-mobile">
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
                            <td data-label="@L["Gm.CampaignDetail.Col.Name"]">@sheet.CharacterName</td>
                            <td data-label="@L["Gm.CampaignDetail.Col.Owner"]">@(_members.FirstOrDefault(m => m.PlayerId == sheet.OwnerId)?.DisplayName ?? sheet.OwnerId.ToString())</td>
                            <td data-label="@L["Gm.CampaignDetail.Col.Status"]">@(sheet.IsDead ? L["Gm.CampaignDetail.Status.Dead"] : sheet.IsRetired ? L["Gm.CampaignDetail.Status.Retired"] : L["Gm.CampaignDetail.Status.Alive"])</td>
                            <td data-label="">
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
            <select class="form-select" @bind="_selectedGrantPlayerId">
                <option value="@Guid.Empty">—</option>
                @foreach (var member in _members)
                {
                    <option value="@member.PlayerId">@member.DisplayName</option>
                }
            </select>
            <input class="form-control" placeholder="@L["Gm.CampaignDetail.CharacterNamePlaceholder"]"
                   @bind="_newCharacterName" @bind:event="oninput" />
            <button class="btn btn-primary btn-sm" @onclick="GrantAsync"
                    disabled="@(_granting || _selectedGrantPlayerId == Guid.Empty || string.IsNullOrWhiteSpace(_newCharacterName))">
                @if (_granting) { <span class="spinner-border spinner-border-sm me-1"></span> }
                @L["Gm.CampaignDetail.GrantCharacter"]
            </button>
        </div>
    }
</div>

@code {
    [Parameter] public Guid Id { get; set; }

    private List<CampaignMemberResponse> _members = [];
    private List<PlayerRosterResponse> _availablePlayers = [];
    private List<CharacterSheetResponse> _sheets = [];
    private List<BreadcrumbItem> _breadcrumbs = [];
    private bool _loading = true;
    private bool _assigning;
    private bool _showSuggestions;
    private Guid _selectedPlayerId;
    private Guid _selectedGrantPlayerId;
    private string _playerSearch = string.Empty;
    private string _newCharacterName = string.Empty;
    private bool _granting;

    private IEnumerable<PlayerRosterResponse> FilteredAvailablePlayers =>
        string.IsNullOrWhiteSpace(_playerSearch)
            ? _availablePlayers
            : _availablePlayers.Where(p =>
                p.DisplayName.Contains(_playerSearch, StringComparison.OrdinalIgnoreCase) ||
                p.Email.Contains(_playerSearch, StringComparison.OrdinalIgnoreCase));

    private void OnSearchInput(ChangeEventArgs e)
    {
        _playerSearch = e.Value?.ToString() ?? string.Empty;
        _selectedPlayerId = Guid.Empty;
        _showSuggestions = true;
    }

    private void SelectPlayer(PlayerRosterResponse player)
    {
        _selectedPlayerId = player.Id;
        _playerSearch = $"{player.DisplayName} ({player.Email})";
        _showSuggestions = false;
    }

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;

        var campaignsResult = await CampaignService.GetMineAsync();
        var campaignName = campaignsResult?.Data?.FirstOrDefault(c => c.Id == Id)?.Name;
        _breadcrumbs =
        [
            new BreadcrumbItem(L["Nav.Campaigns"], "/gm/campaigns"),
            new BreadcrumbItem(campaignName ?? L["Gm.CampaignDetail.Members"], null)
        ];

        var membersResult = await CampaignService.GetMembersAsync(Id);
        _members = membersResult?.Data?.ToList() ?? [];

        var rosterResult = await CampaignService.GetRosterAsync();
        var roster = rosterResult?.Data?.ToList() ?? [];
        var memberIds = _members.Select(m => m.PlayerId).ToHashSet();
        _availablePlayers = roster.Where(p => !memberIds.Contains(p.Id)).ToList();

        var sheetsResult = await SheetService.GetByCampaignAsync(Id);
        _sheets = sheetsResult?.Data?.ToList() ?? [];

        _loading = false;
    }

    private async Task AssignAsync()
    {
        if (_selectedPlayerId == Guid.Empty) return;

        _assigning = true;

        var result = await CampaignService.AssignMemberAsync(
            Id, new AssignMemberRequest { PlayerId = _selectedPlayerId });

        if (result?.Data is not null)
        {
            Toast.Success(L["Gm.CampaignDetail.AssignSuccess"]);
            _selectedPlayerId = Guid.Empty;
            _playerSearch = string.Empty;
            _showSuggestions = false;
            await LoadAsync();
        }
        else
        {
            Toast.Error(result?.Message ?? L["Common.Error"]);
        }

        _assigning = false;
    }

    private async Task GrantAsync()
    {
        if (_selectedGrantPlayerId == Guid.Empty || string.IsNullOrWhiteSpace(_newCharacterName)) return;

        _granting = true;

        var result = await SheetService.GrantAsync(Id, new GrantCharacterSheetRequest
        {
            PlayerId = _selectedGrantPlayerId, CharacterName = _newCharacterName
        });

        if (result?.Data is not null)
        {
            Toast.Success(L["Gm.CampaignDetail.GrantSuccess"]);
            _selectedGrantPlayerId = Guid.Empty;
            _newCharacterName = string.Empty;
            await LoadAsync();
        }
        else
        {
            Toast.Error(result?.Message ?? L["Common.Error"]);
        }

        _granting = false;
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Pages/GmCampaignDetail.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: wire breadcrumbs and usability toolkit into GmCampaignDetail"
```

---

## Task 5: GmNotifications (promote/dismiss)

**Files:**
- Modify: `src/Ruptura.Web/Pages/GmNotifications.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `ToastService`, `LoadingIndicator`.

Notifications are grouped per campaign, each group rendering its own `<table>` — every one of those tables needs `stack-mobile` + `data-label`, not just a single top-level table.

- [ ] **Step 1: Add the `Gm.Notifications.PromoteSuccess` and `Gm.Notifications.DismissSuccess` resx keys**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the `Gm.Notifications.Dismiss` line and add directly after it:

```xml
  <data name="Gm.Notifications.PromoteSuccess"><value>Character promoted.</value></data>
  <data name="Gm.Notifications.DismissSuccess"><value>Notification dismissed.</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the matching `Gm.Notifications.Dismiss` line and add directly after it:

```xml
  <data name="Gm.Notifications.PromoteSuccess"><value>Personagem promovido.</value></data>
  <data name="Gm.Notifications.DismissSuccess"><value>Notificação descartada.</value></data>
```

- [ ] **Step 2: Replace `GmNotifications.razor`**

```razor
@page "/gm/notifications"
@attribute [Authorize(Roles = "GameMaster")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Notifications
@inject IStringLocalizer<AppStrings> L
@inject INotificationClientService NotificationService
@inject ToastService Toast

<PageTitle>@L["Gm.Notifications.Title"] — RUPTURA</PageTitle>

<div class="page-content">
    <div class="page-heading">
        <h1>@L["Gm.Notifications.Title"]</h1>
    </div>

    @if (_loading)
    {
        <LoadingIndicator Text="@L["Common.Loading"]" />
    }
    else if (_groups.Count == 0)
    {
        <div class="ledger-empty">
            <p>@L["Gm.Notifications.Empty"]</p>
        </div>
    }
    else
    {
        @foreach (var group in _groups)
        {
            <div style="margin-bottom:1.5rem">
                <h2 class="section-title">@group.CampaignName</h2>
                <div class="ledger-table-wrap">
                    <table class="ledger-table stack-mobile">
                        <thead>
                            <tr>
                                <th>@L["Gm.Notifications.Col.Character"]</th>
                                <th>@L["Gm.Notifications.Col.CreatedAt"]</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            @foreach (var notification in group.Notifications)
                            {
                                <tr>
                                    <td data-label="@L["Gm.Notifications.Col.Character"]">@notification.CharacterName</td>
                                    <td data-label="@L["Gm.Notifications.Col.CreatedAt"]" style="color:var(--text-muted);font-size:var(--text-xs);white-space:nowrap">
                                        @notification.CreatedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")
                                    </td>
                                    <td data-label="" style="display:flex;gap:.5rem">
                                        <button class="btn btn-primary btn-sm" @onclick="() => PromoteAsync(notification.Id)" disabled="@_busy">
                                            @L["Gm.Notifications.Promote"]
                                        </button>
                                        <button class="btn btn-outline-secondary btn-sm" @onclick="() => DismissAsync(notification.Id)" disabled="@_busy">
                                            @L["Gm.Notifications.Dismiss"]
                                        </button>
                                    </td>
                                </tr>
                            }
                        </tbody>
                    </table>
                </div>
            </div>
        }
    }
</div>

@code {
    private List<NotificationGroupResponse> _groups = [];
    private bool _loading = true;
    private bool _busy;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        var result = await NotificationService.GetMineAsync();
        _groups = result?.Data?.ToList() ?? [];
        _loading = false;
    }

    private async Task PromoteAsync(Guid id)
    {
        _busy = true;
        try
        {
            var result = await NotificationService.PromoteAsync(id);
            if (result is not null && result.Success)
                Toast.Success(L["Gm.Notifications.PromoteSuccess"]);
            else
                Toast.Error(result?.Message ?? L["Common.Error"]);
        }
        finally
        {
            _busy = false;
        }
        await LoadAsync();
    }

    private async Task DismissAsync(Guid id)
    {
        _busy = true;
        try
        {
            var result = await NotificationService.DismissAsync(id);
            if (result is not null && result.Success)
                Toast.Success(L["Gm.Notifications.DismissSuccess"]);
            else
                Toast.Error(result?.Message ?? L["Common.Error"]);
        }
        finally
        {
            _busy = false;
        }
        await LoadAsync();
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Pages/GmNotifications.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: wire usability toolkit into GmNotifications"
```

---

## Task 6: Final verification — full build, full test run, visual pass

**Files:** none prescribed — this task may touch whatever file a genuine regression it finds lives in (same allowance as prior plans' final verification tasks).

**Interfaces:** none — this task confirms every earlier task's deliverable together.

- [ ] **Step 1: Build and test**

Run: `dotnet build` (whole solution), then `dotnet test tests/Ruptura.UnitTests`
Expected: 0 build errors; all tests passing (this plan adds no new tests, so the count should match whatever `main` currently has).

- [ ] **Step 2: Launch the app and capture screenshots**

Use the `run` skill to build and serve `Ruptura.Web`. As a GM with at least one campaign, a couple of roster players, an invite code, and (if practical) a pending rank-promotion notification, capture screenshots of:

- `/dashboard` (GM view, shows `GmDashboard`'s invite table) — light and dark, desktop (≥1024px) and mobile (<600px).
- `/gm/players` — light and dark, desktop and mobile.
- `/gm/campaigns` — light and dark, desktop and mobile.
- `/gm/campaigns/{id}` (`GmCampaignDetail`) — light and dark, desktop and mobile; confirm the breadcrumb trail ("Campaigns / {campaign name}") renders and the first crumb links back to `/gm/campaigns`.
- `/gm/notifications` — light and dark, desktop and mobile (if no real pending notification can be created within reasonable effort, screenshot the empty state and verify the table styling by inspecting the CSS rather than skipping the page).

Confirm for each: no text below 11px, no low-contrast text, `TableSearchBox` filters correctly on `/gm/players`, `/gm/campaigns`, and the dashboard's invite table, tables render as stacked mobile cards (not squeezed) at <600px, and a toast appears for at least one action per page that has one (generate/copy on the dashboard, create on Campaigns, assign/grant on Campaign Detail, promote/dismiss on Notifications).

- [ ] **Step 3: Fix anything the visual pass surfaces**

If a screenshot shows a real regression (not pre-existing/out-of-scope — only things these five pages actually have wrong), fix it directly, keeping the fix small and targeted, then re-verify. If a finding needs a design decision rather than a bug fix, stop and report `DONE_WITH_CONCERNS`/`BLOCKED` describing it instead of improvising.

- [ ] **Step 4: Commit any fixes made**

If Step 3 produced changes:

```bash
git add -A
git commit -m "fix: address GM-pages visual regressions found in verification pass"
```

If Step 3 found nothing to fix, skip this step.

- [ ] **Step 5: Report**

Summarize what was verified and confirm the GM page group (minus `GmCatalog`, already done, and `GmFields`/`GmCharacterSheet`, out of scope per this plan) is now aligned with the design system. Note that the next groups per the spec's §8 rollout order are Player (`PlayerDashboard`, `PlayerCampaigns`, `PlayerCharacter`, `PlayerFields`) and then the 11-tab Character Sheet editor — each needs its own plan.
