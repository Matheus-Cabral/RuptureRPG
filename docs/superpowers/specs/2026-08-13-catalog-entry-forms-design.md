# Catalog Entry Structured Forms (Catalog UX) — Design Spec

**Date:** 2026-08-13
**Status:** Approved (design)
**Feature:** Replace the raw-JSON textarea in the GM catalog editor with schema-driven, per-type structured forms (labeled fields), keeping an "Advanced (JSON)" escape hatch with usage instructions.
**GDD sources:** §catalog types (Origin/Background/Lineage/Aptitude/Talent/Skill/Spell/Technique/EquipmentItem/Installation/Doctrine). Field conventions derived from `CatalogSeedData.*` (the 171 official rows).

---

## 1. Goal & Scope

Today a GM adds/edits any catalog entry by hand-typing raw JSON into a `<textarea>` (`GmCatalog.razor:112-113`), needing to know each type's exact JSON schema. This is the entire pain. Replace it with a **structured form per `CatalogEntryType`**: labeled inputs the GM fills in, from which the system builds the same `DataJson` the consumers already read. Preserve a raw-JSON **Advanced** mode (with instructions) for homebrew fields beyond the standard set.

**In scope:** the GM catalog editor UX (`GmCatalog.razor`) + a Shared `CatalogSchema` reference + field-label/help i18n + a pure form↔JSON round-trip helper with unit tests.

**Out of scope (YAGNI):** server-side schema enforcement (DataJson stays intentionally freeform — homebrew + escape hatch); migrating/normalizing existing entries; rich widgets (skill dropdowns, etc. — plain inputs suffice); any change to catalog consumers (calculators/character-sheet/guild-sheet) or the catalog API.

## 2. Key Decisions (settled 2026-08-13)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Direction | Schema-driven structured forms per type (data-driven, one generic form component) |
| 2 | Escape hatch | Standard fields + an "Advanced (JSON)" toggle exposing raw JSON, WITH usage instructions |
| 3 | Source of truth | The editor holds one working JSON object; the form owns known keys, unknown keys are preserved (round-trip); no data loss |
| 4 | Server | No schema enforcement, no consumer changes, no migration — the form only composes the same `DataJson` |

## 3. Architecture

### 3.1 `CatalogSchema` (Ruptura.Shared, static, zero project refs)
Mirrors the existing `*Reference` pattern (EncounterReference/ContentReference/…). Maps each `CatalogEntryType` (as its string name) → an ordered `IReadOnlyList<CatalogField>`:

```
CatalogField { string Key; string LabelKey; CatalogFieldKind Kind; }
enum CatalogFieldKind { Text, TextArea, Number, Bool }
```

- `Key` = the EXACT JSON property name the seeds/consumers use (PascalCase, e.g. `MainBenefit`). This is the contract — the form writes these keys verbatim so consumers keep working.
- `LabelKey` = a Web resx key for the field's label (e.g. `Gm.Catalog.Field.MainBenefit`).
- `Kind` drives the input control. No `Required` flag (DataJson is freeform; the form never blocks on field content — only Name is required, unchanged).

A helper exposes `FieldsFor(string type)` → the list (empty for an unknown/legacy type, which then falls back to Advanced-only).

### 3.2 Schema-driven form (Web, in the `GmCatalog.razor` editor)
Replaces the raw `<textarea>` as the DEFAULT editor. For the selected type, renders one control per `CatalogField`:
- `Text` → `<input type="text">`; `TextArea` → `<textarea>`; `Number` → `<input type="number">`; `Bool` → checkbox.
- Each control is bound to the corresponding key in the working data model (a `Dictionary<string, JsonElement>`-backed model or an equivalent typed wrapper — see 3.4).

### 3.3 Advanced (JSON) toggle
A toggle ("Avançado (JSON)") reveals a `<textarea>` bound to the FULL working JSON (pretty-printed), preceded by a localized instructions block explaining:
- what it's for (fields beyond the standard form above / homebrew);
- it must be valid JSON (an object);
- standard fields still come from the form; switching back re-reads them;
- invalid JSON blocks Save with a clear message.
Live-validate on input: invalid JSON → inline error + Save disabled while in Advanced mode with a parse error.

### 3.4 Data flow (single source of truth, no loss)
The editor keeps a **working JSON object** (the entry's `DataJson` parsed once on load):
- **Form mode:** each field reads/writes ONLY its own key in the working object. Keys not in the schema are left untouched → editing an existing entry (or one with extra homebrew keys) never drops them.
- **Advanced mode:** the textarea is bound to the serialized working object; on edit it re-parses into the working object (known keys then repopulate the form when toggled back; unknown keys persist).
- **Save:** serialize the working object → `DataJson`. Same keys as before → consumers unaffected.
- **Load (edit existing):** parse `DataJson`; the form shows schema keys (missing ones render blank), extras survive via Advanced.

### 3.5 Pure round-trip helper (testable)
Extract the form↔JSON mapping into a small pure helper in **`Ruptura.Shared`** (no DI, `System.Text.Json` only) so `Ruptura.UnitTests` (which references Shared) can test it independent of Blazor:
- `Merge(workingJson, key, value)` / read a key's value as string/number/bool;
- serialize/deserialize with the same `JsonSerializerOptions` the app uses;
- guarantee: load `DataJson` → set only schema keys → serialize preserves every non-schema key and value.

## 4. Per-type field schema (derived from `CatalogSeedData.*`)

Keys are verbatim from the seeds (the consumer contract). Kinds chosen by field length/type.

| Type | Fields (Key : Kind) |
|---|---|
| **Origin** | MainBenefit:TextArea, PrimarySkill:Text, SecondarySkill:Text, StartingEquipment:Text, NarrativeHook:TextArea |
| **Background** | TriggeringEvent:TextArea, Benefit:TextArea, Complication:TextArea |
| **Lineage** | RacialAdjustment:Text, RacialTrait:TextArea |
| **Aptitude** | CoveredAreas:TextArea |
| **Talent** | Category:Text, Effect:TextArea, PowerTier:Text |
| **Skill** | Area:Text, RelatedAttribute:Text |
| **Spell** | School:Text, ComplexityPaCost:Text, Range:Text, Area:Text, Duration:Text, Test:Text, Effect:TextArea |
| **Technique** | Style:Text, Category:Text, PaCost:Text, Effect:TextArea |
| **Installation** | Category:Text, Weight:Number, LevelCap:Number, Prerequisites:TextArea, Unlocks:TextArea, NonConstructible:Bool |
| **Doctrine** | Bonus:TextArea |
| **EquipmentItem** | Category:Text, Damage:Text, Defense:Text, Weight:Number, Properties:TextArea, Description:TextArea |

Notes: `ComplexityPaCost`/`PaCost` kept as Text (seed values can be non-numeric/formulae). Installation `Weight`/`LevelCap` are ints in the seeds → Number; `NonConstructible` is bool → Bool. **EquipmentItem** has no seed and no consumer reading its keys today, so its field set is a NEW starting convention (descriptive only); anything else goes through Advanced.

## 5. i18n

- Every field label (`Gm.Catalog.Field.<Key>`) and the Advanced instructions/error strings added to BOTH `AppStrings.resx` + `AppStrings.pt-BR.resx`, identical key sets (pt-BR accented, en/keys unaccented ASCII).
- Field labels are human phrases, not the raw PascalCase key (e.g. `MainBenefit` → "Benefício principal" / "Main benefit").

## 6. Testing

- **Unit (round-trip helper):** load a `DataJson` with schema keys + an extra homebrew key → set schema keys via the helper → serialize → assert schema keys updated AND the extra key/value preserved; empty/`{}` and malformed input degrade to an empty object without throwing; number/bool fields serialize as JSON number/bool (not quoted strings).
- **No bUnit** — the Blazor form is verified by build + manual. Manual smoke: create an Origin via the form (no JSON typed) → saved entry's `DataJson` has the 5 keys; toggle Advanced → shows that JSON; add an extra key in Advanced → toggle back → form intact + extra survives Save.

## 7. Data-Model Impact

| Change | Kind |
|--------|------|
| `Ruptura.Shared.Catalog`: `CatalogSchema` + `CatalogField` + `CatalogFieldKind` | New (Shared, string-only) |
| Pure form↔JSON round-trip helper + unit tests | New |
| `GmCatalog.razor`: schema-driven form replaces raw textarea; Advanced (JSON) toggle + instructions | Modify (Web) |
| `AppStrings.resx` + `.pt-BR.resx`: field labels + Advanced help/error strings | Modify (Web) |
| Catalog entity / API / consumers / migration | **None** |
