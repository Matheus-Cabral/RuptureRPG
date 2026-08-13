# Catalog Entry Structured Forms Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the raw-JSON textarea in the GM catalog editor with schema-driven per-type structured forms, keeping an "Advanced (JSON)" escape hatch with usage instructions.

**Architecture:** A static `CatalogSchema` (Ruptura.Shared) maps each `CatalogEntryType` → an ordered field list; a pure `CatalogEntryData` helper (JsonObject-backed) round-trips form values ↔ `DataJson` while preserving unknown keys; `GmCatalog.razor` renders a control per field and a raw-JSON advanced toggle. No backend/consumer/migration changes.

**Tech Stack:** .NET 8, Blazor WASM 8, `System.Text.Json` / `System.Text.Json.Nodes`, xUnit + FluentAssertions.

**Spec:** `docs/superpowers/specs/2026-08-13-catalog-entry-forms-design.md`.

## Global Constraints

- **`Ruptura.Shared` zero project refs** — `CatalogSchema`/`CatalogField`/`CatalogFieldKind` and the `CatalogEntryData` helper use only framework types (`System.Text.Json[.Nodes]`).
- **No backend/consumer/migration changes** — the form only composes the same `DataJson`; catalog entity, API, calculators, character/guild sheets are untouched. No server-side schema enforcement.
- **Consumer key contract** — field `Key`s are the EXACT PascalCase JSON property names from `CatalogSeedData.*`; the form writes them verbatim so consumers keep reading them.
- **No data loss** — the working JSON preserves every key not in the schema (round-trip); only Name remains required (unchanged), the form never blocks on field content.
- **i18n** — every field label (`Gm.Catalog.Field.<Key>`) + advanced-mode help/error strings in BOTH `AppStrings.resx` and `AppStrings.pt-BR.resx`, identical key sets, pt-BR accented.
- **Per-type field schema (exact keys + kinds):**
  - Origin: MainBenefit:TextArea, PrimarySkill:Text, SecondarySkill:Text, StartingEquipment:Text, NarrativeHook:TextArea
  - Background: TriggeringEvent:TextArea, Benefit:TextArea, Complication:TextArea
  - Lineage: RacialAdjustment:Text, RacialTrait:TextArea
  - Aptitude: CoveredAreas:TextArea
  - Talent: Category:Text, Effect:TextArea, PowerTier:Text
  - Skill: Area:Text, RelatedAttribute:Text
  - Spell: School:Text, ComplexityPaCost:Text, Range:Text, Area:Text, Duration:Text, Test:Text, Effect:TextArea
  - Technique: Style:Text, Category:Text, PaCost:Text, Effect:TextArea
  - Installation: Category:Text, Weight:Number, LevelCap:Number, Prerequisites:TextArea, Unlocks:TextArea, NonConstructible:Bool
  - Doctrine: Bonus:TextArea
  - EquipmentItem: Category:Text, Damage:Text, Defense:Text, Weight:Number, Properties:TextArea, Description:TextArea
- **Commit per task.**

---

### Task 1: `CatalogSchema` + `CatalogEntryData` helper (Shared) + unit tests

**Files:**
- Create: `src/Ruptura.Shared/Catalog/CatalogSchema.cs` (schema: `CatalogFieldKind` enum, `CatalogField` record, `CatalogSchema` static with `FieldsFor`)
- Create: `src/Ruptura.Shared/Catalog/CatalogEntryData.cs` (pure JsonObject-backed round-trip helper)
- Test: `tests/Ruptura.UnitTests/Catalog/CatalogEntryDataTests.cs`

**Interfaces:**
- Produces (consumed by Task 2):
  - `enum CatalogFieldKind { Text, TextArea, Number, Bool }`
  - `record CatalogField(string Key, string LabelKey, CatalogFieldKind Kind)`
  - `static IReadOnlyList<CatalogField> CatalogSchema.FieldsFor(string type)` — the ordered fields for a `CatalogEntryType` name; empty list for unknown/legacy types.
  - `sealed class CatalogEntryData` with:
    - `static CatalogEntryData Parse(string? dataJson)` — parse object, or empty on null/blank/invalid/non-object
    - `static bool TryParse(string? raw, out CatalogEntryData data)` — for advanced-mode validation (false on invalid/non-object JSON)
    - `string GetString(string key)` (`""` if missing/null), `double? GetNumber(string key)` (null if missing/not-number), `bool GetBool(string key)` (false if missing)
    - `void SetString(string key, string? value)` (trims; removes the key when null/whitespace), `void SetNumber(string key, double? value)` (removes when null), `void SetBool(string key, bool value)` (always stores)
    - `string ToJson(bool indented = false)`

- [ ] **Step 1: Write `CatalogSchema.cs`**

```csharp
namespace Ruptura.Shared.Catalog;

public enum CatalogFieldKind { Text, TextArea, Number, Bool }

public record CatalogField(string Key, string LabelKey, CatalogFieldKind Kind);

// Per-type field schema — Keys are the exact JSON property names from CatalogSeedData.*
// (the consumer contract). LabelKey resolves to a Web resx string.
public static class CatalogSchema
{
    private static CatalogField F(string key, CatalogFieldKind kind) =>
        new(key, $"Gm.Catalog.Field.{key}", kind);

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<CatalogField>> ByType =
        new Dictionary<string, IReadOnlyList<CatalogField>>
        {
            ["Origin"] = [F("MainBenefit", CatalogFieldKind.TextArea), F("PrimarySkill", CatalogFieldKind.Text), F("SecondarySkill", CatalogFieldKind.Text), F("StartingEquipment", CatalogFieldKind.Text), F("NarrativeHook", CatalogFieldKind.TextArea)],
            ["Background"] = [F("TriggeringEvent", CatalogFieldKind.TextArea), F("Benefit", CatalogFieldKind.TextArea), F("Complication", CatalogFieldKind.TextArea)],
            ["Lineage"] = [F("RacialAdjustment", CatalogFieldKind.Text), F("RacialTrait", CatalogFieldKind.TextArea)],
            ["Aptitude"] = [F("CoveredAreas", CatalogFieldKind.TextArea)],
            ["Talent"] = [F("Category", CatalogFieldKind.Text), F("Effect", CatalogFieldKind.TextArea), F("PowerTier", CatalogFieldKind.Text)],
            ["Skill"] = [F("Area", CatalogFieldKind.Text), F("RelatedAttribute", CatalogFieldKind.Text)],
            ["Spell"] = [F("School", CatalogFieldKind.Text), F("ComplexityPaCost", CatalogFieldKind.Text), F("Range", CatalogFieldKind.Text), F("Area", CatalogFieldKind.Text), F("Duration", CatalogFieldKind.Text), F("Test", CatalogFieldKind.Text), F("Effect", CatalogFieldKind.TextArea)],
            ["Technique"] = [F("Style", CatalogFieldKind.Text), F("Category", CatalogFieldKind.Text), F("PaCost", CatalogFieldKind.Text), F("Effect", CatalogFieldKind.TextArea)],
            ["Installation"] = [F("Category", CatalogFieldKind.Text), F("Weight", CatalogFieldKind.Number), F("LevelCap", CatalogFieldKind.Number), F("Prerequisites", CatalogFieldKind.TextArea), F("Unlocks", CatalogFieldKind.TextArea), F("NonConstructible", CatalogFieldKind.Bool)],
            ["Doctrine"] = [F("Bonus", CatalogFieldKind.TextArea)],
            ["EquipmentItem"] = [F("Category", CatalogFieldKind.Text), F("Damage", CatalogFieldKind.Text), F("Defense", CatalogFieldKind.Text), F("Weight", CatalogFieldKind.Number), F("Properties", CatalogFieldKind.TextArea), F("Description", CatalogFieldKind.TextArea)],
        };

    public static IReadOnlyList<CatalogField> FieldsFor(string type) =>
        ByType.TryGetValue(type ?? string.Empty, out var fields) ? fields : [];
}
```

- [ ] **Step 2: Write the failing tests** (`tests/Ruptura.UnitTests/Catalog/CatalogEntryDataTests.cs`)

```csharp
using FluentAssertions;
using Ruptura.Shared.Catalog;
using Xunit;

namespace Ruptura.UnitTests.Catalog;

public class CatalogEntryDataTests
{
    [Fact]
    public void Parse_NullOrBlankOrInvalid_YieldsEmptyObject()
    {
        CatalogEntryData.Parse(null).ToJson().Should().Be("{}");
        CatalogEntryData.Parse("   ").ToJson().Should().Be("{}");
        CatalogEntryData.Parse("not json").ToJson().Should().Be("{}");
        CatalogEntryData.Parse("[1,2,3]").ToJson().Should().Be("{}"); // non-object
    }

    [Fact]
    public void SetString_TrimsAndStores_RemovesWhenBlank()
    {
        var d = CatalogEntryData.Parse("{}");
        d.SetString("PrimarySkill", "  Espadas  ");
        d.GetString("PrimarySkill").Should().Be("Espadas");
        d.SetString("PrimarySkill", "   ");
        d.GetString("PrimarySkill").Should().Be("");        // removed
        d.ToJson().Should().Be("{}");
    }

    [Fact]
    public void SetNumber_And_SetBool_SerializeAsJsonPrimitives()
    {
        var d = CatalogEntryData.Parse("{}");
        d.SetNumber("Weight", 3);
        d.SetBool("NonConstructible", true);
        d.GetNumber("Weight").Should().Be(3);
        d.GetBool("NonConstructible").Should().BeTrue();
        d.ToJson().Should().Contain("\"Weight\":3").And.Contain("\"NonConstructible\":true"); // not "3"/"true" strings
    }

    [Fact]
    public void SchemaEdits_PreserveUnknownHomebrewKeys()
    {
        var d = CatalogEntryData.Parse("{\"MainBenefit\":\"old\",\"HomebrewExtra\":\"keep me\"}");
        d.SetString("MainBenefit", "new");
        var json = d.ToJson();
        json.Should().Contain("\"MainBenefit\":\"new\"");
        json.Should().Contain("\"HomebrewExtra\":\"keep me\"");   // untouched
    }

    [Fact]
    public void TryParse_RejectsInvalidOrNonObject()
    {
        CatalogEntryData.TryParse("{ bad", out _).Should().BeFalse();
        CatalogEntryData.TryParse("42", out _).Should().BeFalse();
        CatalogEntryData.TryParse("{\"a\":1}", out var ok).Should().BeTrue();
        ok.GetNumber("a").Should().Be(1);
    }

    [Fact]
    public void FieldsFor_KnownType_ReturnsExactKeys_UnknownType_Empty()
    {
        CatalogSchema.FieldsFor("Origin").Select(f => f.Key)
            .Should().Equal("MainBenefit", "PrimarySkill", "SecondarySkill", "StartingEquipment", "NarrativeHook");
        CatalogSchema.FieldsFor("Nonexistent").Should().BeEmpty();
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

Run: `dotnet test tests/Ruptura.UnitTests --filter "FullyQualifiedName~Catalog.CatalogEntryDataTests"`
Expected: FAIL (CatalogEntryData not defined).

- [ ] **Step 4: Write `CatalogEntryData.cs`**

```csharp
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ruptura.Shared.Catalog;

// Pure, DI-free working model for a catalog entry's DataJson. The form edits known schema
// keys; any other keys are preserved verbatim (round-trip, no data loss).
public sealed class CatalogEntryData
{
    private readonly JsonObject _root;
    private CatalogEntryData(JsonObject root) => _root = root;

    public static CatalogEntryData Parse(string? dataJson)
    {
        if (!string.IsNullOrWhiteSpace(dataJson))
        {
            try
            {
                if (JsonNode.Parse(dataJson) is JsonObject obj)
                    return new CatalogEntryData(obj);
            }
            catch (JsonException) { /* fall through to empty */ }
        }
        return new CatalogEntryData(new JsonObject());
    }

    public static bool TryParse(string? raw, out CatalogEntryData data)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(raw) && JsonNode.Parse(raw) is JsonObject obj)
            {
                data = new CatalogEntryData(obj);
                return true;
            }
        }
        catch (JsonException) { /* invalid */ }
        data = new CatalogEntryData(new JsonObject());
        return false;
    }

    public string GetString(string key) =>
        _root.TryGetPropertyValue(key, out var n) && n is not null ? n.ToString() : string.Empty;

    public double? GetNumber(string key) =>
        _root.TryGetPropertyValue(key, out var n) && n is JsonValue v && v.TryGetValue<double>(out var d) ? d : null;

    public bool GetBool(string key) =>
        _root.TryGetPropertyValue(key, out var n) && n is JsonValue v && v.TryGetValue<bool>(out var b) && b;

    public void SetString(string key, string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) _root.Remove(key);
        else _root[key] = trimmed;
    }

    public void SetNumber(string key, double? value)
    {
        if (value is null) _root.Remove(key);
        else _root[key] = value.Value;
    }

    public void SetBool(string key, bool value) => _root[key] = value;

    public string ToJson(bool indented = false) =>
        _root.ToJsonString(new JsonSerializerOptions { WriteIndented = indented });
}
```

- [ ] **Step 5: Run tests to verify they pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter "FullyQualifiedName~Catalog.CatalogEntryDataTests"`
Expected: PASS (6 tests). Then full unit sweep `dotnet test tests/Ruptura.UnitTests` stays green.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Shared/Catalog/CatalogSchema.cs src/Ruptura.Shared/Catalog/CatalogEntryData.cs tests/Ruptura.UnitTests/Catalog/CatalogEntryDataTests.cs
git commit -m "feat: add CatalogSchema + CatalogEntryData round-trip helper"
```

---

### Task 2: Schema-driven catalog form + Advanced (JSON) toggle in `GmCatalog.razor`

**Files:**
- Modify: `src/Ruptura.Web/Pages/GmCatalog.razor` (replace the raw `<textarea>` at ~lines 112-113 with the schema form + advanced toggle; adjust `_form*` state, load, and save)
- Modify: `src/Ruptura.Web/Resources/AppStrings.resx` and `src/Ruptura.Web/Resources/AppStrings.pt-BR.resx` (field labels + advanced help/error strings)

**Interfaces:**
- Consumes: `CatalogSchema.FieldsFor(type)`, `CatalogField`, `CatalogFieldKind`, `CatalogEntryData` (Task 1). The page already has `_selectedType` (string), `_formName`, and previously `_formDataJson` (string) sent in `CreateCatalogEntryRequest`/`UpdateCatalogEntryRequest.DataJson`.
- Produces: nothing consumed downstream (final UI task).

- [ ] **Step 1: Rework editor state.** In `@code`, replace the single `_formDataJson` string with a working model plus advanced-mode state:

```csharp
private CatalogEntryData _formData = CatalogEntryData.Parse("{}");
private bool _advancedMode;
private string _advancedJson = "{}";
private bool _advancedInvalid;
```

Add a helper to read the type whose schema drives the form. When editing (`SelectForEdit`) set `_formData = CatalogEntryData.Parse(entry.DataJson);` and `_advancedJson = _formData.ToJson(indented: true);`. In the "new"/reset path set `_formData = CatalogEntryData.Parse("{}")` and `_advancedJson = "{}"`, `_advancedMode = false`, `_advancedInvalid = false`. Read the exact existing method names in the file first and adapt.

- [ ] **Step 2: Render the schema form (default mode).** Replace the raw `<textarea>` (lines ~112-113 — keep the Name `<input>` above it) with, inside the existing `bestiary-form`/editor container:

```razor
@if (!_advancedMode)
{
    @foreach (var field in CatalogSchema.FieldsFor(FormType))
    {
        <div class="field-row">
            <label class="field-label">@L[field.LabelKey]</label>
            @switch (field.Kind)
            {
                case CatalogFieldKind.TextArea:
                    <textarea class="form-control" rows="2"
                              value="@_formData.GetString(field.Key)"
                              @onchange="e => _formData.SetString(field.Key, e.Value?.ToString())"></textarea>
                    break;
                case CatalogFieldKind.Number:
                    <input class="form-control" type="number"
                           value="@(_formData.GetNumber(field.Key)?.ToString(System.Globalization.CultureInfo.InvariantCulture))"
                           @onchange="e => _formData.SetNumber(field.Key, ParseNum(e.Value?.ToString()))" />
                    break;
                case CatalogFieldKind.Bool:
                    <input class="form-check-input" type="checkbox"
                           checked="@_formData.GetBool(field.Key)"
                           @onchange="e => _formData.SetBool(field.Key, (bool)(e.Value ?? false))" />
                    break;
                default:
                    <input class="form-control" type="text"
                           value="@_formData.GetString(field.Key)"
                           @onchange="e => _formData.SetString(field.Key, e.Value?.ToString())" />
                    break;
            }
        </div>
    }
    @if (CatalogSchema.FieldsFor(FormType).Count == 0)
    {
        <p class="dash-help">@L["Gm.Catalog.NoSchemaFields"]</p>
    }
}
```

Where `FormType` is the string type the editor is bound to (the selected/edited type), and:

```csharp
private static double? ParseNum(string? s) =>
    double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : null;
```

- [ ] **Step 3: Render the Advanced (JSON) toggle + instructions.** Below the form fields, still inside the editor:

```razor
<div class="field-row">
    <label class="field-label">
        <input type="checkbox" checked="@_advancedMode" @onchange="ToggleAdvanced" />
        @L["Gm.Catalog.AdvancedToggle"]
    </label>
</div>
@if (_advancedMode)
{
    <p class="dash-help">@L["Gm.Catalog.AdvancedHelp"]</p>
    <textarea class="form-control" rows="6" @bind="_advancedJson" @bind:event="oninput"></textarea>
    @if (_advancedInvalid)
    {
        <div class="dash-alert" role="alert">@L["Gm.Catalog.AdvancedInvalid"]</div>
    }
}
```

With the toggle + validation handlers:

```csharp
private void ToggleAdvanced(ChangeEventArgs e)
{
    var on = (bool)(e.Value ?? false);
    if (on)
    {
        // entering advanced: show the full working JSON
        _advancedJson = _formData.ToJson(indented: true);
        _advancedInvalid = false;
    }
    else
    {
        // leaving advanced: adopt edited JSON if valid, else stay in advanced
        if (CatalogEntryData.TryParse(_advancedJson, out var parsed))
        {
            _formData = parsed;
            _advancedInvalid = false;
        }
        else { _advancedInvalid = true; return; } // keep advanced open on invalid
    }
    _advancedMode = on;
}
```

- [ ] **Step 4: Wire Save to the working model.** In the existing save handler, before building the request: if `_advancedMode`, first adopt the textarea (`if (!CatalogEntryData.TryParse(_advancedJson, out _formData)) { _advancedInvalid = true; Toast.Error(L["Gm.Catalog.AdvancedInvalid"]); return; }`). Then send `DataJson = _formData.ToJson()` (compact) in both the create and update requests (replacing the old `_formDataJson`). Keep the existing Name-required guard unchanged.

- [ ] **Step 5: Add resx strings (BOTH cultures, identical keys).** Add to `AppStrings.resx` (en) and `AppStrings.pt-BR.resx` (pt-BR accented):
  - One `Gm.Catalog.Field.<Key>` per distinct key across all types: `MainBenefit, PrimarySkill, SecondarySkill, StartingEquipment, NarrativeHook, TriggeringEvent, Benefit, Complication, RacialAdjustment, RacialTrait, CoveredAreas, Category, Effect, PowerTier, Area, RelatedAttribute, School, ComplexityPaCost, Range, Duration, Test, Style, PaCost, Weight, LevelCap, Prerequisites, Unlocks, NonConstructible, Bonus, Damage, Defense, Properties, Description` (dedupe shared keys like `Category`, `Area`, `Effect`, `Weight`). Values are human phrases (e.g. en `MainBenefit` → "Main benefit"; pt-BR → "Benefício principal").
  - `Gm.Catalog.AdvancedToggle` (en "Advanced (JSON)" / pt-BR "Avançado (JSON)").
  - `Gm.Catalog.AdvancedHelp` — instructions, e.g. en: "For homebrew fields beyond the form above. Edit the entry's full JSON object here — it must be valid JSON. Standard fields still come from the form; switching back re-reads them. Invalid JSON blocks saving." / pt-BR accented equivalent.
  - `Gm.Catalog.AdvancedInvalid` (en "Invalid JSON — fix it before saving or switching back to the form." / pt-BR accented).
  - `Gm.Catalog.NoSchemaFields` (en "This type has no standard fields — use Advanced (JSON)." / pt-BR accented).

- [ ] **Step 6: Build + verify parity + manual smoke.**

Run: `dotnet build src/Ruptura.Web` → 0 errors.
Verify resx parity: en `<data>` count == pt-BR count; key sets identical.
Manual smoke (document in the task report): (a) new Origin, fill fields, save → entry `DataJson` has the 5 keys with typed values; (b) edit it, toggle Advanced → shows that JSON; add `"HomebrewX":"y"` in Advanced, toggle back → form intact; save → `HomebrewX` preserved; (c) enter invalid JSON in Advanced → error shown, Save blocked.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Web/Pages/GmCatalog.razor src/Ruptura.Web/Resources/AppStrings.resx src/Ruptura.Web/Resources/AppStrings.pt-BR.resx
git commit -m "feat: schema-driven catalog entry form + advanced JSON mode"
```

---

## Self-Review

**1. Spec coverage:** CatalogSchema + helper (§3.1/3.5) → Task 1; schema-driven form (§3.2) + advanced toggle/instructions (§3.3) + data flow (§3.4) → Task 2; per-type schema (§4) → Task 1 Global Constraints table + `CatalogSchema.cs`; i18n (§5) → Task 2 Step 5; testing (§6) → Task 1 unit tests + Task 2 manual smoke; out-of-scope (no backend/consumer/migration/enforcement) respected — no such tasks. ✓
**2. Placeholder scan:** helper API + tests + form markup are concrete; resx keys enumerated explicitly; no "TBD"/"handle appropriately". The "read the exact existing method names first and adapt" notes point at real code (`SelectForEdit`/reset/save handlers in `GmCatalog.razor`) the implementer must match, not vague placeholders. ✓
**3. Type consistency:** `CatalogEntryData` methods (`Parse`/`TryParse`/`GetString`/`GetNumber`/`GetBool`/`SetString`/`SetNumber`/`SetBool`/`ToJson`), `CatalogField(Key,LabelKey,Kind)`, `CatalogFieldKind` enum, and `CatalogSchema.FieldsFor` are named identically in Task 1 (definition), the tests, and Task 2 (consumption). `LabelKey` = `Gm.Catalog.Field.<Key>` matches the resx keys in Task 2 Step 5. ✓
