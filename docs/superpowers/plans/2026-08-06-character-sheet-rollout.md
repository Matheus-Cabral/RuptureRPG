# Character Sheet Rollout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Apply the design-system rework's usability toolkit to the last and largest remaining surface — the 11-tab Character Sheet editor (`CharacterSheetEditor` + its tab components) and the two thin wrapper pages around it (`GmCharacterSheet`, `PlayerCharacter`) that were deliberately deferred from the GM and Player rollouts specifically for this plan.

**Architecture:** Most of the 11 tabs need no changes at all — they already inherit the design system fully through shared classes (`.form-label`, `.form-control`, `.ledger-table`, `.section-title`, `.btn`). The real work concentrates in three places: (1) the tab bar itself, which today renders as **unstyled raw Bootstrap** (`.nav.nav-tabs`) — the one piece of this entire rollout that never got the "Arcane Ledger" treatment, and which would overflow badly across 11 tabs on a phone without a deliberate responsive design; (2) the Journal tab, which — unlike every other tab — calls its API immediately per action (create/update/delete an entry) rather than batching into the sheet's single top-level Save, making it this plan's only tab with a genuine `ConfirmDialog`-worthy destructive action; (3) `Breadcrumbs`, finally getting wired into the two wrapper pages that were held back for exactly this moment (Campaigns → Campaign → Character).

**Tech Stack:** ASP.NET Core Blazor WebAssembly 8, plain CSS (one new themed component — the tab bar — plus reuse of existing tokens/classes elsewhere), `IStringLocalizer<AppStrings>`, the existing `LoadingIndicator`/`ToastService`/`ConfirmService`/`ConfirmDialog`/`Breadcrumbs` toolkit.

## Global Constraints

- No font-size anywhere below `--text-2xs` (11px).
- Every new/changed user-facing string goes through `IStringLocalizer<AppStrings>` with a key in **both** `AppStrings.resx` (English) and `AppStrings.pt-BR.resx` (Portuguese).
- Toasts replace inline `alert-danger`/fading-text feedback for transient action outcomes — don't show both for the same event. The one exception: `CharacterSheetEditor`'s "sheet failed to load at all" state (`_sheet is null`) stays a blocking `alert-danger`, because there's no sheet to interact with and no action to retry via toast — this is a different case from a save/upload failure on an already-loaded sheet.
- `ConfirmDialog` is reserved for actions that call the API immediately and cannot be undone. Inside the sheet's 11 tabs, "Remove" buttons on Skills/Talents/Spells/Techniques/Equipment/Combat-conditions only mutate the in-memory `_data` object — nothing is persisted until the sheet's own top-level Save button is clicked, so removing a row there is exactly as reversible as any other unsaved form edit (just don't click Save, or re-add the row). None of those get a confirm gate. `CharacterSheetJournalTab`'s Delete is different: it calls `JournalService.DeleteAsync` immediately, with no undo endpoint — it gets the gate, mirroring `GmCatalog.DeleteAsync` and `GmNotifications.DismissAsync`.
- Touch targets ≥44×44px at ≤1023px width — the new tab bar buttons need this explicitly, since they're the one interactive element on this page not already covered by an existing selector list.
- No backend changes.

**Full design spec:** `docs/superpowers/specs/2026-08-06-design-system-rework-design.md`
**Foundation plan (already merged):** `docs/superpowers/plans/2026-08-06-design-system-rework.md`
**Reference implementations to follow:** `src/Ruptura.Web/Pages/GmCatalog.razor` (Confirm/Toast pattern), `src/Ruptura.Web/Pages/GmCampaignDetail.razor` (Breadcrumbs pattern, `GetMineAsync()`-based name resolution)

## Out of Scope

- The 8 tabs that need no changes at all (already fully on shared classes/tokens, no tables needing mobile treatment, no hardcoded styles, no destructive actions): `CharacterSheetIdentityTab`, `CharacterSheetTrialTab`, `CharacterSheetGuildRegistryTab`. (The other 5 non-Journal tabs *do* need the single mechanical change covered in Task 3 — see below.)
- Adding `LoadingIndicator` to the 4 tabs that fetch their own catalog option lists (`CharacterSheetIdentityTab`, `CharacterSheetSkillsTab`, `CharacterSheetCatalogRefListTab`, `CharacterSheetEquipmentTab`) — today they render immediately with empty `<select>` lists until the fetch resolves, a brief flash rather than a broken state. Fixing it means restructuring each tab's render logic around a new loading flag, which is a data-loading UX improvement, not a visual/design-system pass. Worth a dedicated follow-up, not bundled here.
- `TableSearchBox` on any in-sheet table — each list (skills, equipment, talents/spells/techniques) is scoped to one character sheet, the same "bounded, personal-scale" reasoning already applied to `PlayerCampaigns`.
- Restructuring `CharacterSheetEditor`'s top toolbar (name/portrait/rank/NP/status-checkboxes/Save row) — it already wraps via `flex-wrap:wrap` and needs no further responsive work per the Task 6 visual pass; only reviewed here, not touched unless that pass finds a real problem.
- No backend changes, no new API endpoints — `Breadcrumbs`' campaign/character-name resolution reuses `ICampaignClientService.GetMineAsync()` and `ICharacterSheetClientService.GetAsync()`/`GetMineAsync()`, all pre-existing.

---

## File Structure

```
src/Ruptura.Web/
├── wwwroot/css/app.css                          Modify — new themed, responsive tab-bar CSS (first new app.css addition since the foundation)
├── Pages/
│   ├── CharacterSheetEditor.razor                Modify — LoadingIndicator, Toast (save, portrait upload)
│   ├── CharacterSheetAttributesTab.razor          Modify — stack-mobile + data-label
│   ├── CharacterSheetCombatTab.razor              Modify — stack-mobile + data-label (weapons table only)
│   ├── CharacterSheetSkillsTab.razor              Modify — stack-mobile + data-label
│   ├── CharacterSheetCatalogRefListTab.razor      Modify — stack-mobile + data-label
│   ├── CharacterSheetEquipmentTab.razor           Modify — stack-mobile + data-label
│   ├── CharacterSheetJournalTab.razor             Modify — LoadingIndicator, Toast, ConfirmDialog on delete, token cleanup
│   ├── GmCharacterSheet.razor                     Modify — Breadcrumbs (Campaigns → Campaign → Character)
│   └── PlayerCharacter.razor                      Modify — Breadcrumbs, LoadingIndicator
└── Resources/
    ├── AppStrings.resx                            Modify — 7 new keys
    └── AppStrings.pt-BR.resx                      Modify — matching 7 keys
```

---

## Task 1: Tab bar CSS

**Files:**
- Modify: `src/Ruptura.Web/wwwroot/css/app.css`

**Interfaces:**
- Produces: `.nav-tabs`, `.nav-tabs .nav-item`, `.nav-tabs .nav-link`, `.nav-tabs .nav-link:hover`, `.nav-tabs .nav-link.active` — scoped selectors styling the exact Bootstrap classes already present in `CharacterSheetEditor.razor`'s markup (`nav`, `nav-tabs`, `nav-item`, `nav-link`, `active`), confirmed used nowhere else in the app. No markup changes needed anywhere — this task is CSS-only, and Task 2 doesn't touch the tab bar markup either.

Today the 11-tab bar renders as raw, unstyled Bootstrap — a box-bordered tab strip that matches nothing else in the app, and would wrap or overflow unpredictably across 11 tabs on a narrow screen with no horizontal-scroll affordance. This task replaces it with a themed strip (bottom-border active indicator, uppercase small-caps labels matching `.section-title`) that scrolls horizontally on narrow viewports instead of wrapping.

This is CSS-only — no unit test to write first. Verification is `dotnet build` plus the group-wide visual pass in Task 6.

- [ ] **Step 1: Add the tab-bar styles to `app.css`**

Find this line (added by the design-system foundation plan, in the `/* ── Responsive ─────... */` section):

```css
/* Tablet: handled by Task 2 (sidebar rail — depends on NavMenu markup) */
```

Leave that line exactly as-is (it was already replaced by the foundation plan's own Task 2 and is now just a stale comment header for that section — do not touch it). Instead, find the following block, near the top of the file, right after the `/* ── Bootstrap overrides ─...─ */` section's `.alert-danger` rule and before the `/* ── App Shell ─...─ */` comment:

```css
.alert-danger {
    background: var(--danger-bg);
    border: 1px solid var(--danger-border);
    color: var(--danger);
    border-radius: 0;
    font-size: var(--text-sm);
    padding: 0.75rem 1rem;
}
```

Add this new block directly after it (before the `/* ── App Shell ─...─ */` comment):

```css

/* ── Character Sheet Tab Bar ───────────────────────────────────────────────── */
.nav-tabs {
    display: flex;
    flex-wrap: nowrap;
    overflow-x: auto;
    -webkit-overflow-scrolling: touch;
    scrollbar-width: thin;
    gap: 0;
    border-bottom: 1px solid var(--border);
    margin-bottom: 0;
}
.nav-tabs .nav-item { flex: 0 0 auto; }
.nav-tabs .nav-link {
    display: block;
    background: none;
    border: none;
    padding: 0.75rem 1.1rem;
    font-family: var(--font-body);
    font-size: var(--text-xs);
    font-weight: 500;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: var(--text-muted);
    white-space: nowrap;
    border-bottom: 2px solid transparent;
    cursor: pointer;
    transition: color 0.15s, border-color 0.15s;
}
.nav-tabs .nav-link:hover { color: var(--text); }
.nav-tabs .nav-link.active {
    color: var(--text);
    border-bottom-color: var(--primary);
}
```

- [ ] **Step 2: Add the tab buttons to the existing touch-target selector list**

Find this exact block (in the `@media (max-width: 1023px)` responsive section):

```css
@media (max-width: 1023px) {
    .btn, .hamburger, .copy-btn, .theme-btn, .lang-btn,
    .sidebar-nav a.nav-link, .nav-btn-link, .toast-close {
        min-height: 44px;
        min-width: 44px;
    }
    .toast-close {
        align-self: center;
    }
```

Replace it with (only the selector list's first line changes, adding `.nav-tabs .nav-link`):

```css
@media (max-width: 1023px) {
    .btn, .hamburger, .copy-btn, .theme-btn, .lang-btn,
    .sidebar-nav a.nav-link, .nav-btn-link, .toast-close, .nav-tabs .nav-link {
        min-height: 44px;
        min-width: 44px;
    }
    .toast-close {
        align-self: center;
    }
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/wwwroot/css/app.css
git commit -m "feat: add themed, horizontally-scrollable tab bar for the character sheet editor"
```

---

## Task 2: CharacterSheetEditor (loading, toast)

**Files:**
- Modify: `src/Ruptura.Web/Pages/CharacterSheetEditor.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `LoadingIndicator`, `ToastService` (`Success`/`Error`) — pre-existing, globally available. `.nav-tabs`/`.nav-item`/`.nav-link` CSS from Task 1 (no markup change needed here — the tab bar's existing markup is untouched, it just becomes styled once Task 1 lands).

Two distinct feedback situations exist in this file and must stay distinct: (1) the sheet **fails to load at all** (`_sheet is null`) — there's nothing to interact with, so this stays a blocking `alert-danger`, unchanged; (2) **Save or portrait-upload fails on an already-loaded sheet** — these are transient action outcomes and become toasts, replacing the old inline `_errorMessage`/`_successMessage` block that only ever rendered inside the loaded-sheet branch. Because that inline block is now unreachable (nothing sets `_errorMessage` from inside the loaded branch anymore — only `LoadAsync` sets it, and that's read by the *other* branch), it's removed entirely rather than left dead.

- [ ] **Step 1: Add the `Sheet.PortraitUploaded` resx key**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the `Sheet.PortraitLabel` line and add directly after it:

```xml
  <data name="Sheet.PortraitUploaded"><value>Portrait updated.</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the matching `Sheet.PortraitLabel` line and add directly after it:

```xml
  <data name="Sheet.PortraitUploaded"><value>Retrato atualizado.</value></data>
```

- [ ] **Step 2: Replace `CharacterSheetEditor.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@inject IStringLocalizer<AppStrings> L
@inject ICharacterSheetClientService SheetService
@inject IMediaClientService MediaService
@inject ToastService Toast

@if (_loading)
{
    <LoadingIndicator Text="@L["Sheet.Loading"]" />
}
else if (_sheet is null)
{
    <div class="alert-danger">@(_errorMessage ?? L["Sheet.NotFound"])</div>
}
else
{
    <div style="display:flex;flex-wrap:wrap;gap:1rem;align-items:flex-end;margin-bottom:1.5rem">
        <div>
            <label class="form-label">@L["Sheet.NameLabel"]</label>
            <input class="form-control" @bind="_characterName" @bind:event="oninput" />
        </div>
        <div>
            <label class="form-label">@L["Sheet.PortraitLabel"]</label>
            <div style="display:flex;align-items:center;gap:.5rem">
                @if (_portraitDataUri is not null)
                {
                    <img src="@_portraitDataUri" style="width:48px;height:48px;object-fit:cover;border-radius:4px" />
                }
                <InputFile OnChange="UploadPortraitAsync" accept="image/*" disabled="@_uploadingPortrait" />
                @if (_uploadingPortrait) { <span class="spinner-border spinner-border-sm"></span> }
            </div>
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
        else if (_activeTab == "combat")
        {
            <CharacterSheetCombatTab Data="_data" Derived="_derived" />
        }
        else if (_activeTab == "skills")
        {
            <CharacterSheetSkillsTab Data="_data" Derived="_derived" CampaignId="CampaignId" />
        }
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
            <CharacterSheetEquipmentTab Data="_data" Derived="_derived" CampaignId="CampaignId" />
        }
        else if (_activeTab == "trial")
        {
            <CharacterSheetTrialTab Data="_data" />
        }
        else if (_activeTab == "guildRegistry")
        {
            <CharacterSheetGuildRegistryTab Data="_data" CanEdit="CanEditStatus" />
        }
        else if (_activeTab == "journal")
        {
            <CharacterSheetJournalTab CharacterSheetId="SheetId" IsOwner="IsOwner" />
        }
    </div>
}

@code {
    [Parameter] public Guid SheetId { get; set; }
    [Parameter] public Guid CampaignId { get; set; }
    [Parameter] public bool CanEditStatus { get; set; }
    [Parameter] public bool IsOwner { get; set; }

    private static readonly Dictionary<string, string> Tabs = new()
    {
        ["identity"] = "Sheet.Tab.Identity",
        ["attributes"] = "Sheet.Tab.Attributes",
        ["combat"] = "Sheet.Tab.Combat",
        ["skills"] = "Sheet.Tab.Skills",
        ["talents"] = "Sheet.Tab.Talents",
        ["spells"] = "Sheet.Tab.Spells",
        ["techniques"] = "Sheet.Tab.Techniques",
        ["equipment"] = "Sheet.Tab.Equipment",
        ["trial"] = "Sheet.Tab.Trial",
        ["guildRegistry"] = "Sheet.Tab.GuildRegistry",
        ["journal"] = "Sheet.Tab.Journal"
    };

    private bool _loading = true;
    private bool _saving;
    private string? _errorMessage;
    private CharacterSheetResponse? _sheet;
    private CharacterSheetData _data = new();
    private CharacterDerivedStats? _derived;
    private string _characterName = string.Empty;
    private string? _portraitImagePath;
    private string? _portraitDataUri;
    private bool _uploadingPortrait;
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
            _portraitDataUri = await MediaService.GetDataUriAsync(_portraitImagePath);
            _isDead = result.Data.IsDead;
            _isRetired = result.Data.IsRetired;
        }
        _loading = false;
    }

    private async Task SaveAsync()
    {
        _saving = true;

        var result = await SheetService.UpdateAsync(SheetId, new UpdateCharacterSheetRequest
        {
            CharacterName = _characterName,
            DataJson = System.Text.Json.JsonSerializer.Serialize(_data),
            IsDead = _isDead,
            IsRetired = _isRetired
        });

        if (result?.Data is not null)
        {
            _sheet = result.Data;
            _data = result.Data.Data;
            _derived = result.Data.DerivedStats;
            Toast.Success(L["Sheet.Saved"]);
        }
        else
        {
            Toast.Error(result?.Message ?? L["Common.Error"]);
        }

        _saving = false;
    }

    private async Task UploadPortraitAsync(InputFileChangeEventArgs e)
    {
        _uploadingPortrait = true;

        try
        {
            await using var stream = e.File.OpenReadStream(maxAllowedSize: Ruptura.Shared.Media.MediaLimits.ClientMaxUploadBytes);
            var result = await MediaService.UploadAsync(stream, e.File.Name, "CharacterSheetPortrait", SheetId);

            if (result?.Data is not null)
            {
                _portraitImagePath = result.Data.Path;
                _portraitDataUri = await MediaService.GetDataUriAsync(_portraitImagePath);
                Toast.Success(L["Sheet.PortraitUploaded"]);
            }
            else
            {
                Toast.Error(result?.Message ?? L["Common.Error"]);
            }
        }
        catch (Exception)
        {
            Toast.Error(L["Common.Error"]);
        }
        finally
        {
            _uploadingPortrait = false;
        }
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Pages/CharacterSheetEditor.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: wire loading indicator and toasts into CharacterSheetEditor"
```

---

## Task 3: In-sheet tables — mobile stacking

**Files:**
- Modify: `src/Ruptura.Web/Pages/CharacterSheetAttributesTab.razor`
- Modify: `src/Ruptura.Web/Pages/CharacterSheetCombatTab.razor`
- Modify: `src/Ruptura.Web/Pages/CharacterSheetSkillsTab.razor`
- Modify: `src/Ruptura.Web/Pages/CharacterSheetCatalogRefListTab.razor`
- Modify: `src/Ruptura.Web/Pages/CharacterSheetEquipmentTab.razor`

**Interfaces:** none — this task only adds `stack-mobile` (existing CSS class, from the foundation plan) and `data-label` attributes to five already-working tables. No logic changes anywhere.

This is the same mechanical change repeated five times — bundled into one task because a reviewer would naturally evaluate "did every in-sheet table get the mobile treatment" as one unit, not five independent decisions. Every `<td>` gets a `data-label` matching its column's `<th>` text (or an empty string, for a column whose `<th>` is itself empty).

- [ ] **Step 1: `CharacterSheetAttributesTab.razor`**

Replace:

```razor
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
```

with:

```razor
<div class="ledger-table-wrap">
    <table class="ledger-table stack-mobile">
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
                    <td data-label="" style="width:80px">
                        <input class="form-control form-control-sm" type="number" min="1" max="6"
                               value="@attr.Get(Data.Attributes)" @onchange="e => attr.Set(Data.Attributes, ParseInt(e.Value))" />
                    </td>
                    <td data-label="@L["Sheet.NameLabel"]">@L[attr.LabelKey]</td>
                    <td data-label="@L["Sheet.Attributes.Modifier"]">@FormatModifier(Derived?.AttributeModifiers.GetValueOrDefault(attr.Key) ?? 0)</td>
                    <td data-label="@L["Sheet.Attributes.Grade"]">@FormatModifier(Derived?.AttributeGradeBonuses.GetValueOrDefault(attr.Key) ?? 0)</td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

- [ ] **Step 2: `CharacterSheetCombatTab.razor`**

Replace (only the weapons table at the bottom of the file — the conditions chips above it are not a table and are untouched):

```razor
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
```

with:

```razor
<div class="section-header"><span class="section-title">@L["Sheet.Combat.Weapons"]</span></div>
<div class="ledger-table-wrap">
    <table class="ledger-table stack-mobile">
        <thead>
            <tr><th>@L["Sheet.Combat.Weapon.Name"]</th><th>@L["Sheet.Combat.Weapon.Attack"]</th><th>@L["Sheet.Combat.Weapon.Damage"]</th></tr>
        </thead>
        <tbody>
            @foreach (var weapon in Derived?.Weapons ?? [])
            {
                <tr>
                    <td data-label="@L["Sheet.Combat.Weapon.Name"]">@weapon.Name</td>
                    <td data-label="@L["Sheet.Combat.Weapon.Attack"]">@(weapon.AttackBonus >= 0 ? $"+{weapon.AttackBonus}" : weapon.AttackBonus.ToString())</td>
                    <td data-label="@L["Sheet.Combat.Weapon.Damage"]">@weapon.DamageFormula</td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

- [ ] **Step 3: `CharacterSheetSkillsTab.razor`**

Replace:

```razor
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
                    <td><button class="btn btn-outline-secondary btn-sm" @onclick="() => RemoveSkill(skill)">@L["Sheet.Skills.Remove"]</button></td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

with:

```razor
<div class="ledger-table-wrap">
    <table class="ledger-table stack-mobile">
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
                    <td data-label="@L["Gm.CampaignDetail.Col.Name"]">@NameOf(skill.CatalogEntryId)</td>
                    <td data-label="@L["Sheet.Skills.Points"]" style="width:100px">
                        <input class="form-control form-control-sm" type="number" min="0"
                               value="@skill.Points" @onchange="e => skill.Points = ParseInt(e.Value)" />
                    </td>
                    <td data-label="@L["Sheet.Skills.Grade"]">@Derived?.SkillGradeBonuses.GetValueOrDefault(skill.CatalogEntryId)</td>
                    <td data-label=""><button class="btn btn-outline-secondary btn-sm" @onclick="() => RemoveSkill(skill)">@L["Sheet.Skills.Remove"]</button></td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

- [ ] **Step 4: `CharacterSheetCatalogRefListTab.razor`** (shared by the Talents/Spells/Techniques tabs)

Replace:

```razor
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
```

with:

```razor
<div class="ledger-table-wrap">
    <table class="ledger-table stack-mobile">
        <thead>
            <tr><th>@L["Gm.CampaignDetail.Col.Name"]</th><th></th></tr>
        </thead>
        <tbody>
            @foreach (var entry in Entries)
            {
                <tr>
                    <td data-label="@L["Gm.CampaignDetail.Col.Name"]">@NameOf(entry.CatalogEntryId)</td>
                    <td data-label=""><button class="btn btn-outline-secondary btn-sm" @onclick="() => Entries.Remove(entry)">@L["Sheet.RefList.Remove"]</button></td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

- [ ] **Step 5: `CharacterSheetEquipmentTab.razor`**

Replace:

```razor
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
```

with:

```razor
<div class="ledger-table-wrap">
    <table class="ledger-table stack-mobile">
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
                    <td data-label="@L["Gm.CampaignDetail.Col.Name"]">@NameOf(item.CatalogEntryId)</td>
                    <td data-label="@L["Sheet.Equipment.Quantity"]" style="width:80px">
                        <input class="form-control form-control-sm" type="number" min="1"
                               value="@item.Quantity" @onchange="e => item.Quantity = ParseInt(e.Value, 1)" />
                    </td>
                    <td data-label="@L["Sheet.Equipment.Durability"]" style="width:100px">
                        <input class="form-control form-control-sm" type="number" min="0"
                               value="@item.DurabilityRemaining" @onchange="e => item.DurabilityRemaining = ParseInt(e.Value, 0)" />
                    </td>
                    <td data-label="@L["Sheet.Equipment.Equipped"]"><input type="checkbox" checked="@item.IsEquipped" @onchange="e => item.IsEquipped = (bool)(e.Value ?? false)" /></td>
                    <td data-label="@L["Sheet.Equipment.LinkedSkill"]">
                        <select class="form-select form-select-sm" value="@item.LinkedSkillEntryId"
                                @onchange="e => item.LinkedSkillEntryId = ParseGuid(e.Value)">
                            <option value="">@L["Sheet.Equipment.NoneSkill"]</option>
                            @foreach (var skill in _skills)
                            {
                                <option value="@skill.CatalogEntryId">@SkillNameOf(skill.CatalogEntryId)</option>
                            }
                        </select>
                    </td>
                    <td data-label=""><button class="btn btn-outline-secondary btn-sm" @onclick="() => Data.Equipment.Remove(item)">@L["Sheet.RefList.Remove"]</button></td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

- [ ] **Step 6: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Web/Pages/CharacterSheetAttributesTab.razor src/Ruptura.Web/Pages/CharacterSheetCombatTab.razor \
        src/Ruptura.Web/Pages/CharacterSheetSkillsTab.razor src/Ruptura.Web/Pages/CharacterSheetCatalogRefListTab.razor \
        src/Ruptura.Web/Pages/CharacterSheetEquipmentTab.razor
git commit -m "feat: make in-sheet tables mobile-friendly (stack-mobile + data-label)"
```

---

## Task 4: CharacterSheetJournalTab (loading, toast, confirm)

**Files:**
- Modify: `src/Ruptura.Web/Pages/CharacterSheetJournalTab.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `LoadingIndicator`, `ToastService`, `ConfirmService` — pre-existing, globally available.

Unlike every other tab, Journal entries call the API immediately per action (`CreateAsync`/`UpdateAsync`/`DeleteAsync`, not batched into the sheet's top-level Save) — that's what makes `DeleteAsync` here this plan's one genuine `ConfirmDialog` site inside the sheet, mirroring `GmCatalog.DeleteAsync`. Image-upload success stays silent (it's a sub-step within editing an entry, not a standalone completed action) but its failure now goes to a toast instead of the removed inline banner, consistent with everywhere else.

- [ ] **Step 1: Add 5 new resx keys**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the `Journal.Delete` line and add directly after it:

```xml
  <data name="Journal.DeleteConfirm.Title"><value>Delete journal entry?</value></data>
  <data name="Journal.DeleteConfirm.Message"><value>This will permanently delete this journal entry and any attached images. This cannot be undone.</value></data>
  <data name="Journal.CreateSuccess"><value>Entry added.</value></data>
  <data name="Journal.SaveSuccess"><value>Entry updated.</value></data>
  <data name="Journal.DeleteSuccess"><value>Entry deleted.</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the matching `Journal.Delete` line and add directly after it:

```xml
  <data name="Journal.DeleteConfirm.Title"><value>Apagar entrada do diário?</value></data>
  <data name="Journal.DeleteConfirm.Message"><value>Isso vai apagar esta entrada do diário e quaisquer imagens anexadas, permanentemente. Essa ação não pode ser desfeita.</value></data>
  <data name="Journal.CreateSuccess"><value>Entrada adicionada.</value></data>
  <data name="Journal.SaveSuccess"><value>Entrada atualizada.</value></data>
  <data name="Journal.DeleteSuccess"><value>Entrada apagada.</value></data>
```

- [ ] **Step 2: Replace `CharacterSheetJournalTab.razor`**

```razor
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.Journal
@inject IStringLocalizer<AppStrings> L
@inject IJournalEntryClientService JournalService
@inject IMediaClientService MediaService
@inject ToastService Toast
@inject ConfirmService Confirm

@if (_loading)
{
    <LoadingIndicator Text="@L["Common.Loading"]" />
}
else
{
    @if (IsOwner)
    {
        <div style="display:flex;flex-direction:column;gap:.5rem;max-width:600px;margin-bottom:1.5rem">
            <textarea class="form-control" rows="3" placeholder="@L["Journal.NewEntryPlaceholder"]"
                      @bind="_newText" @bind:event="oninput"></textarea>
            <button class="btn btn-primary btn-sm" style="align-self:flex-start"
                    @onclick="CreateAsync" disabled="@(_creating || string.IsNullOrWhiteSpace(_newText))">
                @if (_creating) { <span class="spinner-border spinner-border-sm me-1"></span> }
                @L["Journal.Add"]
            </button>
        </div>
    }

    @if (_entries.Count == 0)
    {
        <div class="ledger-empty"><p>@L["Journal.Empty"]</p></div>
    }
    else
    {
        <div style="display:flex;flex-direction:column;gap:1.5rem">
            @foreach (var entry in _entries)
            {
                <div style="border-top:1px solid var(--border);padding-top:1rem">
                    <div style="color:var(--text-muted);font-size:var(--text-xs);margin-bottom:.5rem">
                        @entry.CreatedAt.ToLocalTime().ToString("dd/MM/yy HH:mm")
                    </div>

                    @if (_editingId == entry.Id)
                    {
                        <textarea class="form-control" rows="3" @bind="_editText" @bind:event="oninput"></textarea>
                        <div style="display:flex;gap:.5rem;flex-wrap:wrap;margin:.75rem 0">
                            @foreach (var path in _editImagePaths.ToList())
                            {
                                <div style="position:relative">
                                    <img src="@GetThumb(path)" style="width:80px;height:80px;object-fit:cover;border-radius:4px" />
                                    <span class="btn btn-outline-secondary btn-sm" style="position:absolute;top:-8px;right:-8px;padding:0 6px"
                                          @onclick="() => _editImagePaths.Remove(path)">✕</span>
                                </div>
                            }
                        </div>
                        <InputFile OnChange="e => UploadImageAsync(entry.Id, e)" accept="image/*" disabled="@_uploading" />
                        @if (_uploading) { <span class="spinner-border spinner-border-sm ms-2"></span> @L["Journal.Uploading"] }
                        <div style="display:flex;gap:.5rem;margin-top:.75rem">
                            <button class="btn btn-primary btn-sm" @onclick="() => SaveEditAsync(entry.Id)">@L["Journal.Save"]</button>
                            <button class="btn btn-outline-secondary btn-sm" @onclick="CancelEdit">@L["Journal.Cancel"]</button>
                        </div>
                    }
                    else
                    {
                        <p style="white-space:pre-wrap">@entry.Text</p>
                        @if (entry.ImagePaths.Count > 0)
                        {
                            <div style="display:flex;gap:.5rem;flex-wrap:wrap;margin-bottom:.5rem">
                                @foreach (var path in entry.ImagePaths)
                                {
                                    <img src="@GetThumb(path)" style="width:80px;height:80px;object-fit:cover;border-radius:4px" />
                                }
                            </div>
                        }
                        @if (IsOwner)
                        {
                            <div style="display:flex;gap:.5rem">
                                <button class="btn btn-outline-secondary btn-sm" @onclick="() => StartEdit(entry)">@L["Journal.Edit"]</button>
                                <button class="btn btn-outline-secondary btn-sm" @onclick="() => DeleteAsync(entry.Id)">@L["Journal.Delete"]</button>
                            </div>
                        }
                    }
                </div>
            }
        </div>
    }
}

@code {
    [Parameter] public Guid CharacterSheetId { get; set; }
    [Parameter] public bool IsOwner { get; set; }

    private List<JournalEntryResponse> _entries = [];
    private readonly Dictionary<string, string?> _thumbCache = new();
    private bool _loading = true;
    private bool _creating;
    private bool _uploading;
    private string _newText = string.Empty;
    private Guid? _editingId;
    private string _editText = string.Empty;
    private List<string> _editImagePaths = [];

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        var result = await JournalService.GetByCharacterSheetAsync(CharacterSheetId);
        _entries = result?.Data?.ToList() ?? [];
        foreach (var path in _entries.SelectMany(e => e.ImagePaths))
            await EnsureThumbAsync(path);
        _loading = false;
    }

    private async Task EnsureThumbAsync(string path)
    {
        if (_thumbCache.ContainsKey(path)) return;
        _thumbCache[path] = await MediaService.GetDataUriAsync(path);
    }

    private string? GetThumb(string path) => _thumbCache.GetValueOrDefault(path);

    private async Task CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(_newText)) return;

        _creating = true;
        var result = await JournalService.CreateAsync(CharacterSheetId, new CreateJournalEntryRequest { Text = _newText });

        if (result?.Data is not null)
        {
            Toast.Success(L["Journal.CreateSuccess"]);
            _newText = string.Empty;
            await LoadAsync();
            StartEdit(result.Data);
        }
        else
        {
            Toast.Error(result?.Message ?? L["Common.Error"]);
        }

        _creating = false;
    }

    private void StartEdit(JournalEntryResponse entry)
    {
        _editingId = entry.Id;
        _editText = entry.Text;
        _editImagePaths = entry.ImagePaths.ToList();
    }

    private void CancelEdit()
    {
        _editingId = null;
        _editText = string.Empty;
        _editImagePaths = [];
    }

    private async Task SaveEditAsync(Guid entryId)
    {
        var result = await JournalService.UpdateAsync(CharacterSheetId, entryId,
            new UpdateJournalEntryRequest { Text = _editText, ImagePaths = _editImagePaths });

        if (result?.Data is not null)
        {
            Toast.Success(L["Journal.SaveSuccess"]);
            CancelEdit();
            await LoadAsync();
        }
        else
        {
            Toast.Error(result?.Message ?? L["Common.Error"]);
        }
    }

    private async Task DeleteAsync(Guid entryId)
    {
        var confirmed = await Confirm.AskAsync(
            L["Journal.DeleteConfirm.Title"],
            L["Journal.DeleteConfirm.Message"],
            L["Journal.Delete"],
            L["Journal.Cancel"]);
        if (!confirmed) return;

        var result = await JournalService.DeleteAsync(CharacterSheetId, entryId);

        if (result?.Success == true)
        {
            Toast.Success(L["Journal.DeleteSuccess"]);
            await LoadAsync();
        }
        else
        {
            Toast.Error(result?.Message ?? L["Common.Error"]);
        }
    }

    private async Task UploadImageAsync(Guid entryId, InputFileChangeEventArgs e)
    {
        _uploading = true;

        try
        {
            await using var stream = e.File.OpenReadStream(maxAllowedSize: Ruptura.Shared.Media.MediaLimits.ClientMaxUploadBytes);
            var result = await MediaService.UploadAsync(stream, e.File.Name, "JournalEntryImage", entryId);

            if (result?.Data is not null)
            {
                var refreshed = await JournalService.GetByCharacterSheetAsync(CharacterSheetId);
                _entries = refreshed?.Data?.ToList() ?? _entries;
                var updatedEntry = _entries.FirstOrDefault(x => x.Id == entryId);
                if (updatedEntry is not null)
                {
                    _editImagePaths = updatedEntry.ImagePaths.ToList();
                    foreach (var path in updatedEntry.ImagePaths)
                        await EnsureThumbAsync(path);
                }
            }
            else
            {
                Toast.Error(result?.Message ?? L["Common.Error"]);
            }
        }
        catch (Exception)
        {
            Toast.Error(L["Common.Error"]);
        }
        finally
        {
            _uploading = false;
        }
    }
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Web/Pages/CharacterSheetJournalTab.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: wire loading, toasts, and delete confirmation into CharacterSheetJournalTab"
```

---

## Task 5: Breadcrumbs on the wrapper pages

**Files:**
- Modify: `src/Ruptura.Web/Pages/GmCharacterSheet.razor`
- Modify: `src/Ruptura.Web/Pages/PlayerCharacter.razor`
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx`
- Modify: `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`

**Interfaces:**
- Consumes: `Breadcrumbs` (`Items` param) + `BreadcrumbItem(string Text, string? Href)` — pre-existing, first used in `GmCampaignDetail.razor`. `ICampaignClientService.GetMineAsync()` resolves the campaign name (same pattern as `GmCampaignDetail`); `ICharacterSheetClientService.GetAsync(SheetId)` (GM) / the sheet data `PlayerCharacter.razor` already fetches via `GetMineAsync(CampaignId)` (Player — no extra call needed, `CharacterName` comes along with the existing `_sheetId` lookup) resolve the character name.

Both routes were purpose-built for this: they're the "Campaign → Character" leaf the spec named as the breadcrumb target back when `Breadcrumbs` was first built. GM's trail is 3 crumbs (Campaigns → Campaign → Character), since a GM always has a concrete, already-granted sheet to view. Player's trail conditionally has 2 or 3 (Campaigns → Campaign, with the Character crumb only added once a sheet is confirmed to exist) — a player who hasn't been granted a character yet has no third thing to name.

- [ ] **Step 1: Add the `Sheet.UnknownCharacter` resx key**

In `src/Ruptura.Web/Resources/AppStrings.resx`, find the `Sheet.NotFound` line and add directly after it:

```xml
  <data name="Sheet.UnknownCharacter"><value>Character</value></data>
```

In `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, find the matching `Sheet.NotFound` line and add directly after it:

```xml
  <data name="Sheet.UnknownCharacter"><value>Personagem</value></data>
```

- [ ] **Step 2: Replace `GmCharacterSheet.razor`**

```razor
@page "/gm/campaigns/{CampaignId:guid}/character-sheets/{SheetId:guid}"
@attribute [Authorize(Roles = "GameMaster")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@inject IStringLocalizer<AppStrings> L
@inject ICampaignClientService CampaignService
@inject ICharacterSheetClientService SheetService

<PageTitle>@L["Sheet.Tab.Identity"] — RUPTURA</PageTitle>

<div class="page-content">
    <Breadcrumbs Items="_breadcrumbs" />
    <CharacterSheetEditor SheetId="SheetId" CampaignId="CampaignId" CanEditStatus="true" IsOwner="false" />
</div>

@code {
    [Parameter] public Guid CampaignId { get; set; }
    [Parameter] public Guid SheetId { get; set; }

    private List<BreadcrumbItem> _breadcrumbs = [];

    protected override async Task OnInitializedAsync()
    {
        var campaignsResult = await CampaignService.GetMineAsync();
        var campaignName = campaignsResult?.Data?.FirstOrDefault(c => c.Id == CampaignId)?.Name;

        var sheetResult = await SheetService.GetAsync(SheetId);
        var characterName = sheetResult?.Data?.CharacterName;

        _breadcrumbs =
        [
            new BreadcrumbItem(L["Nav.Campaigns"], "/gm/campaigns"),
            new BreadcrumbItem(campaignName ?? L["Sheet.UnknownCharacter"], $"/gm/campaigns/{CampaignId}"),
            new BreadcrumbItem(characterName ?? L["Sheet.UnknownCharacter"], null)
        ];
    }
}
```

- [ ] **Step 3: Replace `PlayerCharacter.razor`**

```razor
@page "/campaigns/{CampaignId:guid}/character"
@attribute [Authorize(Roles = "Player")]
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@inject IStringLocalizer<AppStrings> L
@inject ICampaignClientService CampaignService
@inject ICharacterSheetClientService SheetService

<PageTitle>@L["Sheet.Tab.Identity"] — RUPTURA</PageTitle>

<div class="page-content">
    <Breadcrumbs Items="_breadcrumbs" />

    @if (_loading)
    {
        <LoadingIndicator Text="@L["Common.Loading"]" />
    }
    else if (_sheetId is null)
    {
        <div class="ledger-empty"><p>@L["Character.AwaitingGrant"]</p></div>
    }
    else
    {
        <CharacterSheetEditor SheetId="_sheetId.Value" CampaignId="CampaignId" CanEditStatus="false" IsOwner="true" />
    }
</div>

@code {
    [Parameter] public Guid CampaignId { get; set; }

    private bool _loading = true;
    private Guid? _sheetId;
    private List<BreadcrumbItem> _breadcrumbs = [];

    protected override async Task OnInitializedAsync()
    {
        var campaignsResult = await CampaignService.GetMineAsync();
        var campaignName = campaignsResult?.Data?.FirstOrDefault(c => c.Id == CampaignId)?.Name;

        var result = await SheetService.GetMineAsync(CampaignId);
        _sheetId = result?.Data?.Id;

        _breadcrumbs =
        [
            new BreadcrumbItem(L["Nav.Campaigns.Player"], "/campaigns"),
            new BreadcrumbItem(campaignName ?? L["Sheet.UnknownCharacter"], null)
        ];
        if (result?.Data is { } sheet)
        {
            _breadcrumbs.Add(new BreadcrumbItem(sheet.CharacterName, null));
        }

        _loading = false;
    }
}
```

- [ ] **Step 4: Build to verify**

Run: `dotnet build src/Ruptura.Web/Ruptura.Web.csproj`
Expected: `Build succeeded.`

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Web/Pages/GmCharacterSheet.razor src/Ruptura.Web/Pages/PlayerCharacter.razor \
        src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: wire breadcrumbs into the character sheet wrapper pages"
```

---

## Task 6: Final verification — full build, full test run, visual pass

**Files:** none prescribed — this task may touch whatever file a genuine regression it finds lives in (same allowance as prior plans' final verification tasks).

**Interfaces:** none — this task confirms every earlier task's deliverable together, and is the closing task of the entire design-system rollout series that began with the foundation plan.

- [ ] **Step 1: Build and test**

Run: `dotnet build` (whole solution), then `dotnet test tests/Ruptura.UnitTests`
Expected: 0 build errors; all tests passing (this plan adds no new tests, so the count should match whatever `main` currently has).

- [ ] **Step 2: Launch the app and capture screenshots**

Use the `run` skill to build and serve `Ruptura.Web`. As a GM with a campaign, a granted character sheet with some data in a few tabs (attributes, a skill, an equipped item, a journal entry), capture screenshots of:

- `/gm/campaigns/{id}/character-sheets/{sheetId}` (`GmCharacterSheet`) — light and dark, desktop (≥1024px), tablet (600–1023px), and mobile (<600px). Confirm the breadcrumb trail ("Campaigns / {campaign} / {character}") renders and links back correctly, and specifically confirm the **11-tab bar**: styled consistently with the rest of the app on desktop, and on mobile/tablet scrolls horizontally rather than wrapping or overflowing off-screen.
- The same page's **Attributes**, **Skills**, **Equipment**, and **Journal** tabs specifically at <600px — confirm each table renders as stacked mobile cards, and the Journal tab's image thumbnails/edit controls remain usable at that width.
- Trigger a **Journal delete** — confirm the `ConfirmDialog` appears with the entry-specific message, and a toast confirms the outcome.
- Trigger a **sheet Save** — confirm a toast confirms success (not the old fading inline text).
- As a Player with a granted character in the same campaign, `/campaigns/{id}/character` (`PlayerCharacter`) — light and dark, desktop and mobile; confirm its 3-crumb trail, and separately register/use a Player account with **no** granted character yet to confirm the 2-crumb trail (no character name) renders sensibly alongside the "awaiting grant" message.

Confirm throughout: no text below 11px, no low-contrast text, no `alert-danger` banner appearing alongside a toast for the same event.

- [ ] **Step 3: Fix anything the visual pass surfaces**

If a screenshot shows a real regression (not pre-existing/out-of-scope — only things this plan's files actually have wrong), fix it directly, keeping the fix small and targeted, then re-verify. If a finding needs a design decision rather than a bug fix, stop and report `DONE_WITH_CONCERNS`/`BLOCKED` describing it instead of improvising.

- [ ] **Step 4: Commit any fixes made**

If Step 3 produced changes:

```bash
git add -A
git commit -m "fix: address character-sheet visual regressions found in verification pass"
```

If Step 3 found nothing to fix, skip this step.

- [ ] **Step 5: Report**

Summarize what was verified. This closes the design-system rollout that began with `docs/superpowers/plans/2026-08-06-design-system-rework.md` — every page group named in that plan's spec (§8: Casca, Auth, GM, Player, Character Sheet) is now aligned with the design system. Note the deferred items still on record from earlier plans in this series for future follow-up (not part of this plan): the dormant nullable-reference warning in `GmNotifications.razor` (design-system-rework-adjacent, GM pages plan), the dead `Ruptura.Web.styles.css` 404 (Player pages plan), and the `TableSearchBox`-on-`GmNotifications` deferral (GM pages plan).
