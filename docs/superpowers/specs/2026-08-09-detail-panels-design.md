# Structured Detail Panels (UI-B) — Design Spec

**Date:** 2026-08-09
**Status:** Approved (design)
**Feature:** Replace raw `DataJson` display (and consolidate info-heavy rows) with a readable, structured master-detail panel — in the GM catalog, the character-sheet reference tables, and the guild Buildings tab.
**Scope note:** Second sub-project of the "UI adjustments + GM tools" roadmap. Sibling of UI-A (image UX). GM-tools subsystems are separate specs.

---

## 1. Goal & Scope

- **GM Catalog:** the table currently shows a raw `DataJson` column (truncated JSON). Remove it; selecting a row shows a right-hand detail panel rendering that entry's `DataJson` in a readable, structured form.
- **Character-sheet reference tables** (Skills, Talents/Spells/Techniques, Equipment): selecting a referenced row shows the referenced catalog entry's detail (its `DataJson`, already loaded client-side) in a detail panel.
- **Guild Buildings tab:** selecting a building shows the referenced Installation's catalog detail (category, weight, level cap, prerequisites, unlocks) — requires a small additive backend change to carry those fields on `GuildBuildingResponse`.

**Out of scope:** a per-type structured *editor* (the catalog edit form keeps its raw `DataJson` `<textarea>`); the guild Research/Crafting/Staff tabs (structured inline editors already showing all fields); localizing arbitrary homebrew JSON keys.

## 2. Key Decisions (settled in brainstorming, 2026-08-09)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Rendering approach | Generic JSON→readable renderer (type-agnostic; 9 catalog types, only 3 typed, homebrew arbitrary) |
| 2 | Layout | Master-detail: selectable table + side panel; panel stacks below on mobile |
| 3 | Catalog | Remove `DataJson` column; panel shows `JsonDetailView(DataJson)` |
| 4 | Character reference tables | Panel shows the referenced catalog entry's `DataJson` (already in the tab's loaded list) |
| 5 | Guild tables | Only Buildings; add installation detail fields to `GuildBuildingResponse` (small backend change); Research/Crafting/Staff untouched |

## 3. Core Components

**3.1 `JsonDetailView` (`Shared/`)** — the reusable readable renderer.
- Input: `Json` (`string`, a `DataJson` blob).
- Behavior: parse with `System.Text.Json`; render:
  - **object** → a definition list: each key as a prettified label (camelCase/PascalCase → spaced Title Case, e.g. `levelCap` → "Level Cap"), value rendered recursively; nested objects indented.
  - **array** → a bulleted/comma list of rendered elements; empty array → the empty marker.
  - **scalar** → string as-is; number as-is; boolean → localized Yes/No; null/empty string → an em-dash "—".
  - **empty object `{}` / whitespace** → localized "No details".
  - **malformed JSON** → fall back to showing the raw string in a monospace block (never throw).
- Pure display component; no service dependency.

**3.2 Master-detail layout** — a consistent markup + CSS pattern (`.master-detail`, `.master-detail-list`, `.detail-panel`), tokens-based and theme-aware, that each consuming page composes with its own selection state (`_selectedId`/`_selected`). Selecting a row highlights it and fills the panel. On viewports below the responsive breakpoint the panel drops below the table (flex-column). An empty selection shows a localized "Select an item" hint in the panel.

## 4. Application

**4.1 GM Catalog (`GmCatalog.razor`)**
- Remove the `DataJson` `<th>`/`<td>`.
- Make each row clickable → sets `_selectedEntry`; the selected row gets a highlight class.
- Right panel: entry Name (heading) + Official/Homebrew badge + `<JsonDetailView Json="@_selectedEntry.DataJson" />`. Keep Edit/Delete reachable (in the row actions as today, or in the panel header — implementer's call, keep both behaviors working).
- The `TableSearchBox` (search) and archived toggle stay; selection is independent of filtering (if the selected row is filtered out, the panel clears or keeps last — clear it).

**4.2 Character-sheet reference tables** (`CharacterSheetSkillsTab`, `CharacterSheetCatalogRefListTab`, `CharacterSheetEquipmentTab`)
- Each already loads the full catalog list with `DataJson` (`_all` / `_allItems`). Add row selection → resolve the referenced entry from that list by `CatalogEntryId` → panel shows its Name + `<JsonDetailView Json="@entry.DataJson" />`.
- Read-only detail; existing add/remove/edit (and the UI-A/loading changes) unchanged. If the referenced entry isn't found (archived+missing), the panel shows the empty/"No details" state.

**4.3 Guild Buildings (`GuildBuildingsTab` / `GuildSheet`)**
- **Backend (additive):** `GuildBuildingResponse` gains `Category` (string), `Weight` (int), `LevelCap` (int), `Prerequisites` (string), `Unlocks` (string). Populate them where the read path already resolves `InstallationName` from the catalog (deserialize the `InstallationCatalogData` from the same `CatalogEntry` — no new query). When the installation can't be resolved/deserialized, leave defaults (empty/0).
- **UI:** the Buildings table becomes master-detail; selecting a building shows a panel with the resolved installation detail (Category, Weight, Level Cap, Prerequisites, Unlocks) plus the building's own Level/Active. Inline add/edit/delete unchanged.
- Research/Crafting/Staff tabs unchanged.

## 5. i18n / Styling / Testing

- **i18n:** every fixed visible string via `IStringLocalizer<AppStrings>` in BOTH resx — panel "select an item" hint, "No details", Yes/No, and the Buildings panel field labels (Category/Weight/Level Cap/Prerequisites/Unlocks). Prettified JSON keys are *derived at runtime* (homebrew keys are arbitrary) and are NOT resx-localized. Keys unaccented ASCII; identical key sets across cultures.
- **Styling:** design-system tokens only; new classes `.master-detail`, `.detail-panel`, `.detail-row`, `.detail-key`, `.detail-value`, selected-row highlight; theme-aware; responsive (panel stacks on mobile).
- **Testing:**
  - Backend: an integration test that `GET guild` returns a building with its installation fields populated (Category/Weight/LevelCap) from the catalog.
  - Frontend: no bUnit — verify via clean `dotnet build` + manual checks (catalog row select fills the panel with readable fields; malformed/empty DataJson shows the fallback/empty state; character reference row shows the referenced entry; building shows installation detail). Note manual verification in the report.

## 6. Component/Data Impact

| Change | Kind |
|--------|------|
| `JsonDetailView` (`Shared/`) + master-detail CSS pattern | New |
| `GmCatalog.razor` — drop DataJson column, master-detail panel | Modify |
| `CharacterSheetSkillsTab` / `CharacterSheetCatalogRefListTab` / `CharacterSheetEquipmentTab` — selection + panel | Modify |
| `GuildBuildingResponse` — + Category/Weight/LevelCap/Prerequisites/Unlocks | Modify (Shared) |
| Guild read path — populate the new building fields from the catalog | Modify (Infrastructure) |
| `GuildBuildingsTab`/`GuildSheet` — Buildings master-detail panel | Modify |
| `app.css` (`.master-detail*`, `.detail-*`) + `AppStrings` resx pair | Modify |

No DB migration (the new `GuildBuildingResponse` fields are computed at read time from the existing catalog entry).
