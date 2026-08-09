# CgRecursos VE-Based Valuation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the guild CG's **Recursos** term value-based (`PactCoins + Σ material StrategicValue`, VE 0-5) instead of raw material quantity, closing the inflation vector in the guild-sheet spec §11.3.

**Architecture:** Add a `StrategicValue` field to the blob DTO `MaterialStock`; change the pure `GuildStatsCalculator` Recursos term to sum clamped VE (dropping raw `Quantity` and `DimensionalFragments` from CG); clamp VE server-side on the guild blob write path; add a VE input to the Recursos tab. No DB migration (blob-only field).

**Tech Stack:** .NET 8, EF Core 8 (blob JSON, no migration), Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-08-09-cgrecursos-valuation-design.md`. GDD §10.8 (CG formula), §9.10 / Manual §7 (Valor Estratégico 1-5).

## Global Constraints

- **Formula (exact):** `CgRecursos = PactCoins + Σ clamp(material.StrategicValue, 0, 5)`. `Silver` and `DimensionalFragments` are NOT part of CgRecursos. `PactCoins` is face value.
- **VE range:** `StrategicValue ∈ [0,5]`, clamped both in the calculator (defensive) and on the server write path (authoritative). Never 400 on an out-of-range VE — clamp it (guild convention: server normalizes authoritatively).
- **`Ruptura.Shared` keeps ZERO project references** — `MaterialStock` gains a plain `int`, nothing more.
- **Blob-only:** `StrategicValue` is serialized inside `GuildSheetData.DataJson`. No EF migration, no DB column. Legacy blobs deserialize with `StrategicValue = 0`.
- **Overflow-safety:** keep the existing `long`-sum + `ClampToInt` discipline in the calculator.
- **Every visible string via `IStringLocalizer`** in BOTH `AppStrings.resx` (en) and `AppStrings.pt-BR.resx`; resx KEYS unaccented ASCII, pt-BR VALUES carry accents.
- **Commit after each task** on `main`; end messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Modify:**
- `src/Ruptura.Shared/Guilds/GuildSheetData.cs` — `MaterialStock` += `StrategicValue`
- `src/Ruptura.Application/Services/GuildStatsCalculator.cs` — Recursos term
- `tests/Ruptura.UnitTests/Guilds/GuildStatsCalculatorTests.cs` — new VE tests + update 2 existing
- `src/Ruptura.Infrastructure/Services/GuildSheetService.cs` — clamp VE in `UpdateAsync`
- `tests/Ruptura.IntegrationTests/Guilds/` — a VE-clamp integration test (new method in the existing guild PUT test class)
- `src/Ruptura.Web/Pages/GuildResourcesTab.razor` — VE input per material row
- `src/Ruptura.Web/Resources/AppStrings.resx` + `AppStrings.pt-BR.resx` — VE label(s)

---

### Task 1: Data model + calculator formula (+ unit tests)

**Files:** modify `GuildSheetData.cs`, `GuildStatsCalculator.cs`, `GuildStatsCalculatorTests.cs`.

**Interfaces:**
- Produces: `MaterialStock.StrategicValue` (int, blob field); the new `CgRecursos = PactCoins + Σ clamp(StrategicValue,0,5)` behavior consumed by Tasks 2-3.

- [ ] **Step 1: Add the blob field**

In `src/Ruptura.Shared/Guilds/GuildSheetData.cs`, extend `MaterialStock`:
```csharp
public class MaterialStock
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }             // inventory only — no longer feeds CG
    public int StrategicValue { get; set; }       // VE 0..5 — the CG Recursos contribution
}
```

- [ ] **Step 2: Write/adjust the failing unit tests**

In `tests/Ruptura.UnitTests/Guilds/GuildStatsCalculatorTests.cs`:

(a) **Update** the existing CG-terms test (currently asserting `CgRecursos.Should().Be(17)`). Replace its `Resources` block and the two Recursos assertions:
```csharp
            Resources = new GuildResources
            {
                PactCoins = 5, DimensionalFragments = 2, Silver = 999,
                Materials =
                [
                    new MaterialStock { Name = "Ferro", Quantity = 10, StrategicValue = 3 },
                    new MaterialStock { Name = "Cristal", Quantity = 1, StrategicValue = 4 },
                ]
            }
```
```csharp
        r.CgRecursos.Should().Be(12);        // 5 (PactCoins) + 3 + 4 (VE); Silver/Fragments/Quantity excluded
        r.Cg.Should().Be(11 + 7 + 14 + 12);  // 44
```

(b) **Update** `HugeMaterialsQuantity_DoesNotOverflow_And_ClampsCgToIntMax` to reflect the new formula (Fragments/Quantity no longer feed CG; VE does):
```csharp
        var data = new GuildSheetData
        {
            Resources = new GuildResources
            {
                PactCoins = int.MaxValue,
                DimensionalFragments = int.MaxValue,   // excluded from CG now
                Materials =
                [
                    new MaterialStock { Name = "A", Quantity = int.MaxValue, StrategicValue = 5 },
                    new MaterialStock { Name = "B", Quantity = int.MaxValue, StrategicValue = 5 },
                ]
            }
        };
        var act = () => _calc.Calculate(data, [], [], int.MaxValue, new Dictionary<Guid, CatalogEntry>());
        act.Should().NotThrow();
        var r = act();
        r.CgRecursos.Should().Be(int.MaxValue); // PactCoins(int.MaxValue) + 10 saturates, not overflows
        r.Cg.Should().Be(int.MaxValue);
```

(c) **Add** a focused inflation-fix + exclusions test:
```csharp
    [Fact]
    public void CgRecursos_IsPactCoinsPlusStrategicValue_NotQuantity_AndExcludesSilverAndFragments()
    {
        var data = new GuildSheetData
        {
            Resources = new GuildResources
            {
                PactCoins = 40,
                Silver = 100_000,          // must NOT affect CG
                DimensionalFragments = 50, // must NOT affect CG
                Materials =
                [
                    new MaterialStock { Name = "Ferro", Quantity = 10_000, StrategicValue = 1 }, // huge qty, VE 1
                    new MaterialStock { Name = "Cristal", Quantity = 3, StrategicValue = 4 },
                    new MaterialStock { Name = "Legado", Quantity = 1, StrategicValue = 99 },     // clamped to 5
                ]
            }
        };
        var r = _calc.Calculate(data, [], [], 0, new Dictionary<Guid, CatalogEntry>());
        r.CgRecursos.Should().Be(50); // 40 + 1 + 4 + clamp(99->5)
    }
```

- [ ] **Step 3: Run the tests → confirm they fail**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~GuildStatsCalculator`
Expected: the updated/new Recursos assertions FAIL (calculator still sums Quantity + Fragments).

- [ ] **Step 4: Change the calculator Recursos term**

In `src/Ruptura.Application/Services/GuildStatsCalculator.cs`, replace the current `recursos` computation:
```csharp
        var recursos = ClampToInt(
            (long)resources.PactCoins
            + resources.DimensionalFragments
            + (resources.Materials ?? []).Sum(m => (long)m.Quantity));
```
with:
```csharp
        // §10.8 Recursos = Moedas de Pacto (face value) + materiais estratégicos (VE 0..5).
        // Raw Quantity and DimensionalFragments deliberately excluded (spec 2026-08-09 §11.3):
        // Quantity is inventory only; Fragments are the separate RE pillar. long-sum + clamp keeps
        // a legacy/hand-edited blob from overflowing the guild read.
        var recursos = ClampToInt(
            (long)resources.PactCoins
            + (resources.Materials ?? []).Sum(m => (long)Math.Clamp(m.StrategicValue, 0, 5)));
```

- [ ] **Step 5: Run tests → pass; full unit sweep**

Run: `dotnet test tests/Ruptura.UnitTests` → all green.

- [ ] **Step 6: Commit**

```bash
git add src/Ruptura.Shared/Guilds/GuildSheetData.cs src/Ruptura.Application/Services/GuildStatsCalculator.cs tests/Ruptura.UnitTests/Guilds/GuildStatsCalculatorTests.cs
git commit -m "feat: value CgRecursos by StrategicValue (VE 0-5), not raw quantity

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: Server-side VE clamp on the guild blob write path (+ integration test)

**Files:** modify `src/Ruptura.Infrastructure/Services/GuildSheetService.cs`; add a test method to the existing guild PUT integration test class in `tests/Ruptura.IntegrationTests/Guilds/`.

**Interfaces:**
- Consumes: `MaterialStock.StrategicValue` (Task 1).

- [ ] **Step 1: Write the failing integration test**

Find the existing integration test class that exercises `PUT /api/campaigns/{id}/guild` (the guild record-keeping / update tests under `tests/Ruptura.IntegrationTests/Guilds/`; read one first to copy its fixture, auth helper, and how it builds an `UpdateGuildSheetRequest` with a `Version`). Add a test that:
- GETs the guild (to obtain the current `Version`), sets `Data.Resources.Materials = [ { Name:"X", Quantity:1, StrategicValue: 99 }, { Name:"Y", Quantity:1, StrategicValue: -4 } ]`, PUTs it,
- then GETs again and asserts the persisted materials have `StrategicValue == 5` and `0` respectively (clamped), and that `DerivedStats.CgRecursos` reflects the clamp (e.g. includes +5 and +0 for those stacks).

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~<YourTestName>` → expect FAIL (no clamp yet; 99 persists).

- [ ] **Step 2: Add the clamp in `UpdateAsync`**

In `GuildSheetService.UpdateAsync`, immediately after the existing reputation-clamp loop:
```csharp
        foreach (var rel in incoming.Influence)
            rel.Reputation = Math.Clamp(rel.Reputation, -100, 100);
```
add:
```csharp
        // VE (StrategicValue) is the CG Recursos contribution — enforce the GDD 0..5 range
        // server-side (mirrors the reputation/Points clamps); out-of-range is clamped, never trusted.
        foreach (var material in incoming.Resources.Materials)
            material.StrategicValue = Math.Clamp(material.StrategicValue, 0, 5);
```
(`incoming.Resources.Materials` is a non-null `List<MaterialStock>` by the DTO's initializer; the deserialized blob preserves it. If a defensive null-guard is warranted to match surrounding style, use `incoming.Resources?.Materials ?? []` — but confirm `incoming.Resources` is always non-null after `Deserialize`, as the reputation loop already assumes `incoming.Influence` is.)

- [ ] **Step 3: Run the test → pass; full integration sweep**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~<YourTestName>` then `dotnet test` (full). Testcontainers uses Docker; known one-off Serilog "logger already frozen" flake — re-run once if a single test trips it.

- [ ] **Step 4: Commit**

```bash
git add src/Ruptura.Infrastructure/Services/GuildSheetService.cs tests/Ruptura.IntegrationTests/Guilds
git commit -m "feat: clamp material StrategicValue [0,5] on guild blob write

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Recursos tab UI — VE input + i18n

**Files:** modify `src/Ruptura.Web/Pages/GuildResourcesTab.razor`, `AppStrings.resx`, `AppStrings.pt-BR.resx`.

- [ ] **Step 1: Add the VE column to the Materials table**

In `GuildResourcesTab.razor`, the Materials table currently has Name / Qty / (remove) columns. Add a VE column.

Header row — add a `<th>` after the Qty header:
```razor
                        <th>@L["Guild.Resources.MaterialName"]</th>
                        <th>@L["Guild.Resources.MaterialQty"]</th>
                        <th>@L["Guild.Resources.MaterialVE"]</th>
                        <th></th>
```
Body row — add a `<td>` after the Qty cell (with the mobile `data-label`, min/max on the input):
```razor
                            <td data-label="@L["Guild.Resources.MaterialQty"]">
                                <input type="number" class="form-control" @bind="material.Quantity" />
                            </td>
                            <td data-label="@L["Guild.Resources.MaterialVE"]">
                                <input type="number" class="form-control" min="0" max="5" @bind="material.StrategicValue" />
                            </td>
```
Add a one-line hint under the Materials section title that VE (not Quantity) is what counts toward CG — use `@L["Guild.Resources.MaterialVEHint"]` in a `.dash-label`/muted style consistent with the tab.

- [ ] **Step 2: Add the resx keys (BOTH cultures)**

`AppStrings.resx` (en):
```xml
  <data name="Guild.Resources.MaterialVE"><value>VE</value></data>
  <data name="Guild.Resources.MaterialVEHint"><value>Strategic Value (0–5) is what counts toward the guild's CG — not quantity.</value></data>
```
`AppStrings.pt-BR.resx`:
```xml
  <data name="Guild.Resources.MaterialVE"><value>VE</value></data>
  <data name="Guild.Resources.MaterialVEHint"><value>O Valor Estratégico (0–5) é o que conta para a CG da guilda — não a quantidade.</value></data>
```
Place them next to the existing `Guild.Resources.Material*` keys in both files; keep the two files' key sets identical.

- [ ] **Step 3: Build + verify + commit**

Run: `dotnet build` → clean (0 errors). If feasible, run the app and confirm the Materials table shows a VE input (0-5) and editing it round-trips; else confirm the clean build and note it.
```bash
git add src/Ruptura.Web
git commit -m "feat: add Valor Estratégico (VE) input to guild Recursos tab

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage (§3 data, §4 formula, §5 validation, §6 UI, §7 tests):**
- `MaterialStock.StrategicValue` blob field, no migration → Task 1 Step 1. ✓
- `CgRecursos = PactCoins + Σ clamp(VE,0,5)`, Silver/Fragments excluded, PactCoins face value → Task 1 Step 4 + tests. ✓
- Server-side VE clamp on write → Task 2. ✓
- Calculator defensive clamp → Task 1 Step 4 (`Math.Clamp` inside the sum). ✓
- Recursos-tab VE input + i18n → Task 3. ✓
- Unit tests (VE sum, huge-qty→≤5, VE>5 clamp, Silver/Fragments excluded, PactCoins face) → Task 1 Step 2. ✓
- Validator/integration clamp test → Task 2 Step 1. ✓
- Regression (existing guild suites) → Task 1 Step 5 + Task 2 Step 3. ✓
- **Out of scope (not gaps):** inflation index applied to CG/prices; buy/sell economy; secondary-expedition income; re-valuing Fragments into CG.

**2. Placeholder scan:** Every code step carries concrete code. Task 2's test name and exact PUT-test class are left to the implementer to match the real harness (the guild PUT integration tests exist under `tests/Ruptura.IntegrationTests/Guilds/`), with the assertion behavior fully specified — consistent with prior guild plans. No "TBD"/"handle appropriately".

**3. Type consistency:** `StrategicValue` (int) is the single name used in the DTO, calculator, service clamp, tests, and UI binding. `CgRecursos = PactCoins + Σ clamp(material.StrategicValue,0,5)` is identical across the constraint, Task 1 code, and the tests. resx keys `Guild.Resources.MaterialVE`/`MaterialVEHint` match between the UI and both resx files.
