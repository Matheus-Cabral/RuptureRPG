# Structured Detail Panels (UI-B) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace raw `DataJson` display and info-heavy rows with a readable master-detail panel — a reusable `JsonDetailView` renderer applied to the GM catalog, the character-sheet reference tables, and (with a small additive backend change) the guild Buildings tab.

**Architecture:** A pure, recursive `JsonDetailView` (`Shared/`) renders any `DataJson` string as a readable key/value view. A master-detail CSS pattern (selectable table + side panel, stacks on mobile) is composed per page with the page's own selection state. The guild Buildings case adds computed installation-detail fields to `GuildBuildingResponse` (read-time, from the existing catalog entry — no DB migration).

**Tech Stack:** Blazor WebAssembly 8, `System.Text.Json.Nodes`, `IStringLocalizer` (resx en + pt-BR), design-system CSS tokens; xUnit + Testcontainers for the one backend test.

**Spec:** `docs/superpowers/specs/2026-08-09-detail-panels-design.md`.

## Global Constraints

- **Generic, type-agnostic rendering** — `JsonDetailView` must handle any JSON shape (9 catalog types, homebrew arbitrary keys); never per-type code, never throw on malformed JSON (raw fallback).
- **Master-detail** — selectable table + side panel; panel stacks below the table on narrow viewports; empty selection shows a localized hint.
- **No DB migration** — the new `GuildBuildingResponse` fields are computed at read time from the existing `CatalogEntry` (`InstallationCatalogData`), at the same spot that already resolves `InstallationName`.
- **`Ruptura.Shared` keeps zero project references** — the new `GuildBuildingResponse` fields are plain scalars.
- **Every fixed visible string via `IStringLocalizer<AppStrings>`** in BOTH resx; identical key sets; pt-BR carries accents. Prettified JSON keys are derived at runtime and are NOT resx-localized.
- **Design-system tokens only** in CSS; theme-aware; responsive.
- **No bUnit** — frontend verified by clean `dotnet build` + manual checks; the guild backend field-population gets an integration test.
- **Commit after each task** on `main`; end messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:** `src/Ruptura.Web/Shared/JsonDetailView.razor`, `src/Ruptura.Web/Shared/JsonValueView.razor`.
**Modify:** `GmCatalog.razor`; `CharacterSheetSkillsTab.razor`, `CharacterSheetCatalogRefListTab.razor`, `CharacterSheetEquipmentTab.razor`; `GuildBuildingResponse.cs`, `GuildSheetService.cs` (MapBuilding), `GuildBuildingsTab.razor`; `app.css`; `AppStrings.resx` + `AppStrings.pt-BR.resx`; a guild integration test.

---

### Task 1: `JsonDetailView` renderer + master-detail CSS + GM Catalog

**Files:** create `JsonDetailView.razor`, `JsonValueView.razor`; modify `GmCatalog.razor`, `app.css`, both resx.

**Interfaces:**
- Produces: `<JsonDetailView Json="@someDataJson" />` (consumed by Tasks 2 & 3); the `.master-detail`/`.detail-panel` CSS classes.

- [ ] **Step 1: `JsonValueView` (recursive)** — `src/Ruptura.Web/Shared/JsonValueView.razor`:
```razor
@using System.Text.Json.Nodes
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@inject IStringLocalizer<AppStrings> L

@if (Node is JsonObject obj)
{
    <dl class="detail-list">
        @foreach (var kv in obj)
        {
            <div class="detail-row">
                <dt class="detail-key">@Prettify(kv.Key)</dt>
                <dd class="detail-value"><JsonValueView Node="kv.Value" /></dd>
            </div>
        }
    </dl>
}
else if (Node is JsonArray arr)
{
    @if (arr.Count == 0)
    {
        <span class="detail-value">—</span>
    }
    else
    {
        <ul class="detail-array">
            @foreach (var item in arr)
            {
                <li><JsonValueView Node="item" /></li>
            }
        </ul>
    }
}
else
{
    <span>@Scalar(Node)</span>
}

@code {
    [Parameter] public JsonNode? Node { get; set; }

    // camelCase / PascalCase / snake_case -> spaced Title Case.
    private static string Prettify(string key)
    {
        if (string.IsNullOrEmpty(key)) return key;
        var spaced = System.Text.RegularExpressions.Regex.Replace(key.Replace('_', ' '), "(?<=[a-z0-9])(?=[A-Z])", " ");
        return char.ToUpperInvariant(spaced[0]) + spaced[1..];
    }

    private string Scalar(JsonNode? node)
    {
        if (node is null) return "—";
        if (node is JsonValue v)
        {
            if (v.TryGetValue<bool>(out var b)) return b ? L["Common.Yes"] : L["Common.No"];
            var s = v.ToString();
            return string.IsNullOrWhiteSpace(s) ? "—" : s;
        }
        return node.ToString();
    }
}
```

- [ ] **Step 2: `JsonDetailView`** — `src/Ruptura.Web/Shared/JsonDetailView.razor`:
```razor
@using System.Text.Json.Nodes
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@inject IStringLocalizer<AppStrings> L

@if (_error)
{
    <pre class="detail-raw">@Json</pre>
}
else if (_root is null || (_root is JsonObject o && o.Count == 0))
{
    <p class="detail-empty">@L["Detail.None"]</p>
}
else
{
    <JsonValueView Node="_root" />
}

@code {
    [Parameter] public string? Json { get; set; }

    private JsonNode? _root;
    private bool _error;

    protected override void OnParametersSet()
    {
        _error = false;
        _root = null;
        if (string.IsNullOrWhiteSpace(Json)) return;
        try { _root = JsonNode.Parse(Json); }
        catch (System.Text.Json.JsonException) { _error = true; }
    }
}
```

- [ ] **Step 3: CSS** — add to `app.css` (tokens/theme-aware):
```css
.master-detail { display: flex; gap: 1.5rem; align-items: flex-start; }
.master-detail-list { flex: 1 1 60%; min-width: 0; }
.detail-panel {
    flex: 1 1 40%;
    min-width: 0;
    border: 1px solid var(--border-strong);
    padding: 1rem 1.25rem;
    background: var(--bg-surface);
    align-self: stretch;
}
.detail-panel-empty { color: var(--text-muted); font-size: var(--text-sm); }
.detail-list { margin: 0; }
.detail-row { display: flex; gap: 0.75rem; padding: 0.25rem 0; border-bottom: 1px solid var(--border-subtle, rgba(127,127,127,0.15)); }
.detail-key { flex: 0 0 40%; color: var(--text-muted); font-size: var(--text-2xs); text-transform: uppercase; letter-spacing: 0.05em; margin: 0; }
.detail-value { flex: 1 1 60%; margin: 0; }
.detail-array { margin: 0; padding-left: 1.1rem; }
.detail-raw { white-space: pre-wrap; font-size: var(--text-2xs); color: var(--text-muted); }
.detail-empty { color: var(--text-muted); font-size: var(--text-sm); }
.ledger-table tr.is-selected > td { background: var(--surface-2, rgba(127,127,127,0.10)); }
@media (max-width: 1023px) { .master-detail { flex-direction: column; } }
```

- [ ] **Step 4: i18n** — add to BOTH resx (grep first; `Common.Yes`/`Common.No` may already exist — only add missing keys):
```xml
<!-- en -->  <data name="Common.Yes"><value>Yes</value></data>
             <data name="Common.No"><value>No</value></data>
             <data name="Detail.None"><value>No details</value></data>
             <data name="Detail.SelectItem"><value>Select an item to see its details.</value></data>
<!-- pt-BR --><data name="Common.Yes"><value>Sim</value></data>
             <data name="Common.No"><value>Não</value></data>
             <data name="Detail.None"><value>Sem detalhes</value></data>
             <data name="Detail.SelectItem"><value>Selecione um item para ver os detalhes.</value></data>
```

- [ ] **Step 5: GM Catalog master-detail.** In `GmCatalog.razor`:
  - Remove the `DataJson` `<th>` (was `<th>DataJson</th>`) and the `DataJson` `<td>` (the truncated-JSON cell).
  - Wrap the table + a new panel in `<div class="master-detail">…<div class="detail-panel">…</div></div>`; give the table container `class="master-detail-list"`.
  - Make each row select: `<tr class="@(_selected?.Id == entry.Id ? "is-selected" : null)" @onclick="() => _selected = entry" style="cursor:pointer">`. Keep the Edit/Delete buttons working (add `@onclick:stopPropagation="true"` on the actions `<td>` so clicking a button doesn't also change selection).
  - Panel body:
    ```razor
    <div class="detail-panel">
        @if (_selected is null)
        {
            <p class="detail-panel-empty">@L["Detail.SelectItem"]</p>
        }
        else
        {
            <h3>@_selected.Name</h3>
            <p class="detail-panel-empty">@(_selected.IsGlobal ? L["Gm.Catalog.Official"] : L["Gm.Catalog.Homebrew"])</p>
            <JsonDetailView Json="@_selected.DataJson" />
        }
    </div>
    ```
  - Add `private CatalogEntryResponse? _selected;` to `@code`. When the type changes or the list reloads (`OnTypeChanged`/reload), clear `_selected = null`. If the selected entry is filtered out by the search, clear it (e.g., in the panel guard, also check it is in `FilteredEntries`, or reset `_selected` when `_searchTerm` changes).

- [ ] **Step 6: Build + commit.** `dotnet build` clean.
```bash
git add src/Ruptura.Web/Shared/JsonDetailView.razor src/Ruptura.Web/Shared/JsonValueView.razor src/Ruptura.Web/Pages/GmCatalog.razor src/Ruptura.Web/wwwroot/css/app.css src/Ruptura.Web/Resources
git commit -m "feat: JsonDetailView + master-detail panel for the GM catalog

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: Character-sheet reference tables — selection + detail panel

**Files:** modify `CharacterSheetSkillsTab.razor`, `CharacterSheetCatalogRefListTab.razor`, `CharacterSheetEquipmentTab.razor`.

**Interfaces:** consumes `<JsonDetailView>` (Task 1).

Each of these tabs already loads the full catalog list with `DataJson` (`_all` in Skills/CatalogRefList, `_allItems` in Equipment) and has a `NameOf(Guid)` resolver. For each tab:

- [ ] **Step 1: Wrap the existing table in the master-detail layout** and add a `_selectedRefId` (`Guid?`) selection field. Make each existing row clickable to set `_selectedRefId = <the row's CatalogEntryId>` with the `is-selected` highlight, keeping the existing per-row inputs/buttons working (`@onclick:stopPropagation="true"` on interactive cells so editing a Points input or clicking Remove doesn't change selection).

- [ ] **Step 2: Add the detail panel** beside the table:
```razor
<div class="detail-panel">
    @{
        var sel = _all.FirstOrDefault(e => e.Id == _selectedRefId); // Equipment: _allItems
    }
    @if (sel is null)
    {
        <p class="detail-panel-empty">@L["Detail.SelectItem"]</p>
    }
    else
    {
        <h3>@sel.Name</h3>
        <JsonDetailView Json="@sel.DataJson" />
    }
</div>
```
(Use `_allItems` in the Equipment tab; `_all` in Skills and CatalogRefList. Gate the whole thing under the existing `_loading` check added in the earlier loading-states work.)

- [ ] **Step 3: Build + commit.** `dotnet build` clean.
```bash
git add src/Ruptura.Web/Pages/CharacterSheetSkillsTab.razor src/Ruptura.Web/Pages/CharacterSheetCatalogRefListTab.razor src/Ruptura.Web/Pages/CharacterSheetEquipmentTab.razor
git commit -m "feat: detail panel for character-sheet reference tables

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Guild Buildings — installation detail (backend + UI)

**Files:** modify `GuildBuildingResponse.cs`, `GuildSheetService.cs`, `GuildBuildingsTab.razor`; add a guild integration test.

**Interfaces:**
- Produces: `GuildBuildingResponse` with `Category`/`Weight`/`LevelCap`/`Prerequisites`/`Unlocks`.

- [ ] **Step 1: Write the failing integration test.** In `tests/Ruptura.IntegrationTests/Guilds/GuildBuildingTests.cs`, add a test: add a building for a known seeded Installation, `GET guild`, and assert the returned building carries the installation's `Category`/`Weight`/`LevelCap` (values from the seeded installation catalog entry — read one seeded installation's known values from the existing seed data/tests). Run → expect FAIL (fields absent/default).

- [ ] **Step 2: Extend the DTO.** In `src/Ruptura.Shared/Guilds/GuildBuildingResponse.cs` add:
```csharp
    public string Category { get; set; } = string.Empty;
    public int Weight { get; set; }
    public int LevelCap { get; set; }
    public string Prerequisites { get; set; } = string.Empty;
    public string Unlocks { get; set; } = string.Empty;
```

- [ ] **Step 3: Populate from the catalog.** In `GuildSheetService.cs`, change `MapBuilding` to take the `CatalogEntry?` and deserialize its `InstallationCatalogData` (mirror the existing `ValidateInstallationAsync` deserialization; use the same `JsonSerializer` options it uses for catalog blobs):
```csharp
    private static GuildBuildingResponse MapBuilding(GuildBuilding b, CatalogEntry? entry)
    {
        InstallationCatalogData? data = null;
        if (entry is not null)
        {
            try { data = JsonSerializer.Deserialize<InstallationCatalogData>(entry.DataJson); }
            catch (JsonException) { /* leave defaults */ }
        }
        return new GuildBuildingResponse
        {
            Id = b.Id,
            CatalogEntryId = b.CatalogEntryId,
            InstallationName = entry?.Name ?? string.Empty,
            Level = b.Level,
            IsActive = b.IsActive,
            Category = data?.Category ?? string.Empty,
            Weight = data?.Weight ?? 0,
            LevelCap = data?.LevelCap ?? 0,
            Prerequisites = data?.Prerequisites ?? string.Empty,
            Unlocks = data?.Unlocks ?? string.Empty
        };
    }
```
Update the two call sites: `MapBuildingAsync` (pass the fetched `entry` instead of `entry?.Name`), and the read path (line ~784) where buildings are mapped from the `GetByIdsAsync` dictionary — pass `dict.GetValueOrDefault(b.CatalogEntryId)` as the entry. (Confirm the exact variable names for the dictionary and the per-building select at that site.)

- [ ] **Step 4: Run the test → pass; full guild sweep.** `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildBuildingTests` then `dotnet test`. Known Serilog flake — re-run once if a lone unrelated test trips it.

- [ ] **Step 5: Buildings tab master-detail.** In `GuildBuildingsTab.razor`, wrap the buildings table in the master-detail layout, add a `_selectedBuilding` selection (highlight + `@onclick`, with `stopPropagation` on the inline edit/level/active/delete controls), and a panel showing the selected building's installation detail:
```razor
<div class="detail-panel">
    @if (_selectedBuilding is null)
    {
        <p class="detail-panel-empty">@L["Detail.SelectItem"]</p>
    }
    else
    {
        <h3>@_selectedBuilding.InstallationName</h3>
        <dl class="detail-list">
            <div class="detail-row"><dt class="detail-key">@L["Guild.Buildings.Category"]</dt><dd class="detail-value">@_selectedBuilding.Category</dd></div>
            <div class="detail-row"><dt class="detail-key">@L["Guild.Buildings.Weight"]</dt><dd class="detail-value">@_selectedBuilding.Weight</dd></div>
            <div class="detail-row"><dt class="detail-key">@L["Guild.Buildings.LevelCap"]</dt><dd class="detail-value">@_selectedBuilding.LevelCap</dd></div>
            <div class="detail-row"><dt class="detail-key">@L["Guild.Buildings.Prerequisites"]</dt><dd class="detail-value">@(string.IsNullOrEmpty(_selectedBuilding.Prerequisites) ? "—" : _selectedBuilding.Prerequisites)</dd></div>
            <div class="detail-row"><dt class="detail-key">@L["Guild.Buildings.Unlocks"]</dt><dd class="detail-value">@(string.IsNullOrEmpty(_selectedBuilding.Unlocks) ? "—" : _selectedBuilding.Unlocks)</dd></div>
        </dl>
    }
</div>
```
Add the 5 `Guild.Buildings.*` label keys to BOTH resx (Category/Weight/Level Cap/Prerequisites/Unlocks; pt-BR: Categoria/Peso/Nível Máx./Pré-requisitos/Desbloqueia).

- [ ] **Step 6: Build + commit.** `dotnet build` clean; full sweep green.
```bash
git add src/Ruptura.Shared/Guilds/GuildBuildingResponse.cs src/Ruptura.Infrastructure/Services/GuildSheetService.cs src/Ruptura.Web/Pages/GuildBuildingsTab.razor src/Ruptura.Web/Resources tests/Ruptura.IntegrationTests/Guilds/GuildBuildingTests.cs
git commit -m "feat: guild Buildings installation-detail panel (+ response fields)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage:** `JsonDetailView` generic renderer (§3.1) → Task 1; master-detail (§3.2) → Task 1 CSS + all consumers; catalog (§4.1) → Task 1; character reference tables (§4.2) → Task 2; guild Buildings backend+UI (§4.3) → Task 3; i18n/testing (§5) → across tasks; out-of-scope (structured editor, Research/Crafting/Staff, homebrew-key localization) not present. ✓
**2. Placeholder scan:** `JsonDetailView`/`JsonValueView` carry full code; catalog + guild edits concrete; the character-tabs task is pattern-directive over 3 near-identical tabs with the exact panel snippet and the field-name differences called out; the one "confirm the dict variable name at the read-path building map" note is a scoped lookup against existing code. No "TBD"/"handle appropriately".
**3. Type consistency:** `<JsonDetailView Json="string?">` used identically in catalog, character tabs, (not guild — guild uses explicit labels since its detail is typed, per spec). `GuildBuildingResponse` new fields (`Category`/`Weight`/`LevelCap`/`Prerequisites`/`Unlocks`) match between DTO, `MapBuilding`, the test, and the Buildings panel. resx keys (`Common.Yes`/`Common.No`/`Detail.None`/`Detail.SelectItem`/`Guild.Buildings.*`) used exactly as defined in both cultures. `.master-detail`/`.detail-panel`/`.detail-*`/`is-selected` classes defined in Task 1 and used by all consumers.
