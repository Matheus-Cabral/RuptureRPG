# Character Interlude — Equipment Repair Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** An equipped item whose `DurabilityRemaining` reaches 0 becomes **Danificado** (GDD §6.7.6) — its weapon Damage / armor-shield Defense contribution drops by 1 — until the owner or GM clicks "Reparar" on the Equipment tab, which resets it to the Raridade's max and shows the Interlúdio day cost that repair represents.

**Architecture:** No new endpoint, no migration, no new persisted fields. "Danificado" and "Max Durability" are both computed live from a new `EquipmentReference` (Shared) table keyed by the item's Raridade — `CharacterStatsCalculator` reads it server-side to apply the -1 combat-math penalty, and `CharacterSheetEquipmentTab.razor` reads the same table client-side to show the badge/button and perform the repair (a plain in-memory mutation of `Data.Equipment`, persisted by the existing `AutosaveWatcher` — same pattern as sub-project #1's Apply).

**Tech Stack:** .NET 8, Blazor WASM 8, xUnit + FluentAssertions.

**Spec:** `docs/superpowers/specs/2026-09-10-character-equipment-repair-design.md` (read in full — this plan implements it verbatim).

## Global Constraints

- **No migration, no new endpoint, no new persisted field** — everything is computed live from `EquipmentReference` + the existing `DurabilityRemaining`/`Rarity` (spec Decisions #4, #5, #7).
- **Rarity matching is case/whitespace-insensitive** (`.Trim()` + `StringComparer.OrdinalIgnoreCase`), matching `CharacterStatsCalculator.CategoryIs`'s established reasoning for free-text catalog fields.
- **"-1 no Bônus Base" applies ONLY to `DamageBonus` (weapons) and `DefenseBonus` (armor/shield)** — `EquipmentItemCatalogData.AttackBonus` is confirmed dead code in the calculator (equipment never affects Attack — CLAUDE.md's documented design decision) and must NOT be touched.
- **An unrecognized Raridade resolves to Max Durability 0** — such an item is never "Danificado," regardless of `DurabilityRemaining`.
- **A brand-new equipment entry must start at full durability, not 0** — `CharacterSheetEquipmentTab.AddItem()` currently leaves `DurabilityRemaining` at its `int` default (0), which under this feature's new rule would make every newly-equipped item born "Danificado." This is a pre-existing latent gap this plan must also close (Task 2), not a new design decision — it falls directly out of the already-approved "Danificado = DurabilityRemaining ≤ 0" rule.
- **Existing rows already at `DurabilityRemaining = 0`** (any equipment added before this feature shipped) will start showing as Danificado once this ships — expected, not a bug: no backfill/migration is planned (spec Decision #4/#7), and the one-click Reparar action self-heals it.
- **Every visible string via `IStringLocalizer`**, both Web resx (en + pt-BR) — no hardcoded UI text.
- **`ConfirmService`/`ToastService`** for the repair confirmation and success toast — CLAUDE.md's app-wide feedback pattern (`@inject ConfirmService Confirm`, `@inject ToastService Toast`), not a bespoke dialog.
- **Commit after each task** on `main`; end commit messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:**
- `src/Ruptura.Shared/CharacterSheets/EquipmentReference.cs`

**Modify:**
- `src/Ruptura.Application/Services/CharacterStatsCalculator.cs`
- `tests/Ruptura.UnitTests/Application/CharacterStatsCalculatorTests.cs`
- `src/Ruptura.Web/Pages/CharacterSheetEquipmentTab.razor`
- `src/Ruptura.Web/wwwroot/css/app.css`
- `src/Ruptura.Web/Resources/AppStrings.resx`, `AppStrings.pt-BR.resx`

---

### Task 1: `EquipmentReference` + `CharacterStatsCalculator` Danificado penalty

**Files:** create `EquipmentReference.cs`; modify `CharacterStatsCalculator.cs`, `CharacterStatsCalculatorTests.cs`.

**Interfaces:**
- Produces: `EquipmentReference.MaxDurabilityFor(string rarity) → int`, `EquipmentReference.RepairDaysFor(string rarity) → int` (both consumed by Task 2's Razor component too).

- [ ] **Step 1: `EquipmentReference`**

`src/Ruptura.Shared/CharacterSheets/EquipmentReference.cs`:
```csharp
namespace Ruptura.Shared.CharacterSheets;

// GDD §6.7.6 — Golpes de Desgaste até precisar manutenção, by Raridade (FECHADO). The
// repair-days table is NOT from the GDD (which only says "tempo curto") — it's derived from
// §6.7.4's Criação-days table: max(1, ceil(CreationDays/4)). Divino has no fixed Criação days
// (requires a prior Pesquisa project) so it falls back to its own Golpes de Desgaste ceiling.
// See design spec §3 for the full derivation.
public static class EquipmentReference
{
    public static readonly IReadOnlyDictionary<string, int> MaxDurabilityByRarity =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Comum"] = 3, ["Incomum"] = 4, ["Raro"] = 5, ["Épico"] = 6, ["Lendário"] = 8, ["Divino"] = 10
        };

    public static readonly IReadOnlyDictionary<string, int> RepairDaysByRarity =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Comum"] = 1, ["Incomum"] = 1, ["Raro"] = 2, ["Épico"] = 4, ["Lendário"] = 8, ["Divino"] = 10
        };

    // Raridade is free text (GM-typed via the catalog form) — trims and looks up
    // case-insensitively, matching CharacterStatsCalculator.CategoryIs' established reasoning
    // for free-text catalog fields. An unrecognized value resolves to 0 (never "damaged" —
    // there's no known ceiling to compare DurabilityRemaining against).
    public static int MaxDurabilityFor(string rarity) =>
        MaxDurabilityByRarity.GetValueOrDefault(rarity.Trim(), 0);

    public static int RepairDaysFor(string rarity) =>
        RepairDaysByRarity.GetValueOrDefault(rarity.Trim(), 0);
}
```

- [ ] **Step 2: Write the failing unit tests**

Append to `tests/Ruptura.UnitTests/Application/CharacterStatsCalculatorTests.cs`, right before the file's closing `}` (after the last existing test, `Calculate_MalformedCatalogDataJson_DoesNotThrow_TreatsEntryAsAbsent`):

```csharp
    // ── Danificado (GDD §6.7.6) — Golpes de Desgaste exhausted (DurabilityRemaining <= 0) ──

    [Fact]
    public void Calculate_DamagedEquippedWeapon_DamageDropsBy1()
    {
        var skillId = Guid.NewGuid();
        var weaponId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 4 }, // modifier +2, grade bonus +3
            Skills = [new CharacterSkillEntry { CatalogEntryId = skillId, Points = 30 }], // grade bonus +1
            Equipment =
            [
                new CharacterEquipmentEntry
                {
                    CatalogEntryId = weaponId, IsEquipped = true, LinkedSkillEntryId = skillId,
                    DurabilityRemaining = 0 // "Comum" max is 3 → 0 remaining = Danificado
                }
            ]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [skillId] = Skill(skillId, "Controle"),
            [weaponId] = Equipment(weaponId, "arma", "Comum", damageBonus: 2, diceCategory: "Média")
        };

        var result = _sut.Calculate(data, catalog);

        var row = result.Weapons.Should().ContainSingle().Subject;
        row.DamageFormula.Should().Be("1d8 +4"); // undamaged would be "1d8 +5" (attr +2, skill +1, item +2); Danificado -1
    }

    [Fact]
    public void Calculate_DamagedArmorAndShield_DefenseDropsBy1Each()
    {
        var armorId = Guid.NewGuid();
        var shieldId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 3 },
            Equipment =
            [
                new CharacterEquipmentEntry { CatalogEntryId = armorId, IsEquipped = true, DurabilityRemaining = 0 },
                new CharacterEquipmentEntry { CatalogEntryId = shieldId, IsEquipped = true, DurabilityRemaining = 0 }
            ]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [armorId] = Equipment(armorId, "armadura", "Comum", defenseBonus: 2),
            [shieldId] = Equipment(shieldId, "escudo", "Comum", defenseBonus: 1)
        };

        var result = _sut.Calculate(data, catalog);

        // Undamaged would be 10 + (3-2) + 2 + 1 = 14; each Danificado item loses 1.
        result.PassiveDefense.Should().Be(10 + (3 - 2) + (2 - 1) + (1 - 1));
    }

    [Fact]
    public void Calculate_UndamagedEquipment_PositiveDurability_NoPenalty()
    {
        var armorId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 3 },
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = armorId, IsEquipped = true, DurabilityRemaining = 3 }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [armorId] = Equipment(armorId, "armadura", "Comum", defenseBonus: 2)
        };

        var result = _sut.Calculate(data, catalog);

        result.PassiveDefense.Should().Be(10 + (3 - 2) + 2); // full bonus, not damaged
    }

    [Fact]
    public void Calculate_UnrecognizedRarity_NeverTreatedAsDamaged_EvenAtZeroDurability()
    {
        var armorId = Guid.NewGuid();
        var data = new CharacterSheetData
        {
            Attributes = new CharacterAttributes { Controle = 3 },
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = armorId, IsEquipped = true, DurabilityRemaining = 0 }]
        };
        var catalog = new Dictionary<Guid, CatalogEntry>
        {
            [armorId] = Equipment(armorId, "armadura", "Artesanal Caseira", defenseBonus: 2) // not one of the 6 GDD rarities
        };

        var result = _sut.Calculate(data, catalog);

        result.PassiveDefense.Should().Be(10 + (3 - 2) + 2); // no known ceiling → never "Danificado"
    }
```

- [ ] **Step 3: Run → fail** (compiles — `DurabilityRemaining` already exists — but assertions fail: no penalty applied yet).

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~CharacterStatsCalculatorTests` → the 4 new tests FAIL (no -1 applied); all pre-existing tests still PASS at this point (the calculator hasn't changed yet).

- [ ] **Step 4: Implement the penalty in `CharacterStatsCalculator`**

Add this private helper (anywhere among the other private static helpers, e.g. right above `CategoryIs`):
```csharp
    // GDD §6.7.6 — an item is Danificado once its Golpes de Desgaste (DurabilityRemaining) is
    // exhausted, PROVIDED its Raridade resolves to a known ceiling (an unrecognized Raridade
    // has no ceiling to compare against, so it's never "damaged").
    private static bool IsDamaged(CharacterEquipmentEntry entry, EquipmentItemCatalogData data) =>
        EquipmentReference.MaxDurabilityFor(data.Rarity) > 0 && entry.DurabilityRemaining <= 0;
```

Change the `armorAndShieldDefense` line inside `Calculate`:
```csharp
        var armorAndShieldDefense = equipped
            .Where(x => CategoryIs(x.Data!.Category, "armadura") || CategoryIs(x.Data!.Category, "escudo"))
            .Sum(x => x.Data!.DefenseBonus);
```
to:
```csharp
        var armorAndShieldDefense = equipped
            .Where(x => CategoryIs(x.Data!.Category, "armadura") || CategoryIs(x.Data!.Category, "escudo"))
            .Sum(x => x.Data!.DefenseBonus - (IsDamaged(x.Entry, x.Data!) ? 1 : 0));
```

Change the `damage` line inside `BuildWeaponRow`:
```csharp
        var damage = attributeModifier + skillGrade + eqData.DamageBonus;
```
to:
```csharp
        var damage = attributeModifier + skillGrade + eqData.DamageBonus - (IsDamaged(entry, eqData) ? 1 : 0);
```

`EquipmentReference` lives in `Ruptura.Shared.CharacterSheets`, which this file already imports (`using Ruptura.Shared.CharacterSheets;`) — no new `using` needed.

- [ ] **Step 5: Run the 4 new tests → pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~CharacterStatsCalculatorTests` — the 4 new tests now PASS. **3 pre-existing tests now FAIL** — this is expected (see Step 6), not a regression to chase: those tests' `CharacterEquipmentEntry` fixtures never set `DurabilityRemaining`, which defaults to `0`, and their items' Raridade ("Comum") now resolves to a real ceiling (3) — making them "Danificado" under the just-added rule, so their previously-hardcoded expected totals are now 1 (or 2) too high.

- [ ] **Step 6: Fix the 3 pre-existing tests broken by the new default-0-is-damaged rule**

In `Calculate_PassiveDefenseAndDamageReduction_OnlyCountEquippedArmorAndShield`, change:
```csharp
                new CharacterEquipmentEntry { CatalogEntryId = armorId, IsEquipped = true },
                new CharacterEquipmentEntry { CatalogEntryId = shieldId, IsEquipped = true },
```
to:
```csharp
                new CharacterEquipmentEntry { CatalogEntryId = armorId, IsEquipped = true, DurabilityRemaining = 1 },
                new CharacterEquipmentEntry { CatalogEntryId = shieldId, IsEquipped = true, DurabilityRemaining = 1 },
```
(leave the third, `unequippedArmorId`, entry untouched — it's `IsEquipped = false`, never reaches the damage/defense math either way).

In `Calculate_ArmorCategory_MatchesRegardlessOfCaseOrWhitespace`, change:
```csharp
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = armorId, IsEquipped = true }]
```
to:
```csharp
            Equipment = [new CharacterEquipmentEntry { CatalogEntryId = armorId, IsEquipped = true, DurabilityRemaining = 1 }]
```

In `Calculate_EquippedWeaponWithLinkedSkill_ProducesAttackBonusAndDamageFormula`, change:
```csharp
                new CharacterEquipmentEntry
                {
                    CatalogEntryId = weaponId, IsEquipped = true, LinkedSkillEntryId = skillId
                }
```
to:
```csharp
                new CharacterEquipmentEntry
                {
                    CatalogEntryId = weaponId, IsEquipped = true, LinkedSkillEntryId = skillId,
                    DurabilityRemaining = 1
                }
```

Do NOT touch any other `CharacterEquipmentEntry` construction in this file (`Calculate_CarryCapacity_...`, `Calculate_UnequippedWeapon_...`, `Calculate_Np_...`, the two "Case A/Case B" entries in the untrained-skill-grade test, or the malformed-data test) — none of them assert on `PassiveDefense`/`DamageFormula`, so the new rule doesn't affect their outcomes; verify this yourself by re-reading each before deciding whether to touch it, don't just trust this note.

- [ ] **Step 7: Run the full unit suite → pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~CharacterStatsCalculatorTests` → all tests (old + new) PASS. Then `dotnet build` clean.

- [ ] **Step 8: Commit**

```bash
git add src/Ruptura.Shared/CharacterSheets/EquipmentReference.cs \
  src/Ruptura.Application/Services/CharacterStatsCalculator.cs \
  tests/Ruptura.UnitTests/Application/CharacterStatsCalculatorTests.cs
git commit -m "feat: apply GDD §6.7.6 Danificado -1 penalty to damaged equipment's combat math

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: Equipment tab UI — Danificado badge + Reparar button

**Files:** modify `CharacterSheetEquipmentTab.razor`, `app.css`, `AppStrings.resx`, `AppStrings.pt-BR.resx`.

**Interfaces:**
- Consumes: `EquipmentReference.MaxDurabilityFor`/`RepairDaysFor` (Task 1).

- [ ] **Step 1: Fix `AddItem` to start new equipment at full durability**

In `src/Ruptura.Web/Pages/CharacterSheetEquipmentTab.razor`, change:
```csharp
    private void AddItem()
    {
        if (_selectedId == Guid.Empty) return;
        Data.Equipment.Add(new CharacterEquipmentEntry { CatalogEntryId = _selectedId, Quantity = 1 });
        _selectedId = Guid.Empty;
    }
```
to:
```csharp
    private void AddItem()
    {
        if (_selectedId == Guid.Empty) return;
        // Start at full durability — the int default (0) would otherwise make every
        // newly-added item immediately "Danificado" under the new Golpes de Desgaste rule.
        var maxDurability = EquipmentReference.MaxDurabilityFor(RarityOf(_selectedId));
        Data.Equipment.Add(new CharacterEquipmentEntry
        {
            CatalogEntryId = _selectedId, Quantity = 1, DurabilityRemaining = maxDurability
        });
        _selectedId = Guid.Empty;
    }
```

- [ ] **Step 2: Add `RarityOf`, `IsDamaged`, and `RepairAsync` helpers**

Add `@inject ConfirmService Confirm` and `@inject ToastService Toast` to the top injection block (after the existing `@inject ICatalogClientService CatalogService`).

Add these methods next to the existing `CategoryLabelOf` helper (same file, same pattern — `CatalogEntryData.Parse(...).GetString(...)`):
```csharp
    // Same generic-JSON-key lookup as CategoryLabelOf, just for "Rarity" instead of "Category".
    private string RarityOf(Guid id) =>
        CatalogEntryData.Parse(_allItems.FirstOrDefault(e => e.Id == id)?.DataJson).GetString("Rarity");

    private bool IsDamaged(CharacterEquipmentEntry item) =>
        EquipmentReference.MaxDurabilityFor(RarityOf(item.CatalogEntryId)) > 0 && item.DurabilityRemaining <= 0;

    private async Task RepairAsync(CharacterEquipmentEntry item)
    {
        var rarity = RarityOf(item.CatalogEntryId);
        var days = EquipmentReference.RepairDaysFor(rarity);
        var confirmed = await Confirm.AskAsync(
            L["Sheet.Equipment.RepairConfirm.Title"],
            L["Sheet.Equipment.RepairConfirm.Message", days],
            L["Sheet.Equipment.Repair"],
            L["Sheet.Equipment.Cancel"]);
        if (!confirmed) return;

        item.DurabilityRemaining = EquipmentReference.MaxDurabilityFor(rarity);
        Toast.Success(L["Sheet.Equipment.Repaired"]);
    }
```
`EquipmentReference` is in `Ruptura.Shared.CharacterSheets`, already imported by this file (`@using Ruptura.Shared.CharacterSheets` at the top) — no new `@using` needed.

- [ ] **Step 3: Show the badge + button in the Durability cell**

In the table body, change the Durability `<td>`:
```razor
                            <td data-label="@L["Sheet.Equipment.Durability"]" style="width:100px" @onclick:stopPropagation="true">
                                <input class="form-control form-control-sm" type="number" min="0"
                                       value="@item.DurabilityRemaining" @onchange="e => item.DurabilityRemaining = ParseInt(e.Value, 0)" />
                            </td>
```
to:
```razor
                            <td data-label="@L["Sheet.Equipment.Durability"]" style="width:160px" @onclick:stopPropagation="true">
                                <input class="form-control form-control-sm" type="number" min="0"
                                       value="@item.DurabilityRemaining" @onchange="e => item.DurabilityRemaining = ParseInt(e.Value, 0)" />
                                @if (IsDamaged(item))
                                {
                                    <div style="margin-top:.25rem;display:flex;align-items:center;gap:.4rem">
                                        <span class="badge-status badge-damaged">@L["Sheet.Equipment.Damaged"]</span>
                                        <button class="btn btn-outline-secondary btn-sm" @onclick="() => RepairAsync(item)">@L["Sheet.Equipment.Repair"]</button>
                                    </div>
                                }
                            </td>
```

- [ ] **Step 4: `.badge-damaged` CSS rule**

In `src/Ruptura.Web/wwwroot/css/app.css`, add a new rule right after the existing `.badge-expired` rule (search for `.badge-expired { background: var(--badge-expired-bg); color: var(--badge-expired-text); }`):
```css
.badge-damaged { background: var(--danger-bg); color: var(--danger); border: 1px solid var(--danger-border); }
```
This reuses the already theme-aware `--danger`/`--danger-bg`/`--danger-border` tokens (already defined for both light and dark elsewhere in this file) — no new custom properties needed.

- [ ] **Step 5: i18n — both Web resx**

`src/Ruptura.Web/Resources/AppStrings.resx`, add after the existing `Sheet.Equipment.NoneSkill` entry (search for it):
```xml
  <data name="Sheet.Equipment.Damaged"><value>Damaged</value></data>
  <data name="Sheet.Equipment.Repair"><value>Repair</value></data>
  <data name="Sheet.Equipment.Cancel"><value>Cancel</value></data>
  <data name="Sheet.Equipment.RepairConfirm.Title"><value>Repair item</value></data>
  <data name="Sheet.Equipment.RepairConfirm.Message"><value>This takes {0} day(s) of Interlúdio. Apply?</value></data>
  <data name="Sheet.Equipment.Repaired"><value>Item repaired — remember to save.</value></data>
```
`src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, same position:
```xml
  <data name="Sheet.Equipment.Damaged"><value>Danificado</value></data>
  <data name="Sheet.Equipment.Repair"><value>Reparar</value></data>
  <data name="Sheet.Equipment.Cancel"><value>Cancelar</value></data>
  <data name="Sheet.Equipment.RepairConfirm.Title"><value>Reparar item</value></data>
  <data name="Sheet.Equipment.RepairConfirm.Message"><value>Isso leva {0} dia(s) de Interlúdio. Aplicar?</value></data>
  <data name="Sheet.Equipment.Repaired"><value>Item reparado — lembre-se de salvar.</value></data>
```

- [ ] **Step 6: Build + verify + commit**

Run: `dotnet build` (clean). If feasible, run the app and confirm: an item with `DurabilityRemaining = 0` shows the Danificado badge + Reparar button; clicking Reparar (after confirming) resets it to the Raridade's max and clears the badge; a newly-added item starts at full durability, not 0. Else confirm a clean build and note it.

```bash
git add src/Ruptura.Web
git commit -m "feat: add Danificado badge and client-side Reparar action to the Equipment tab

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage:**
- §3 tables (Max Durability + Repair Days by Raridade, verbatim) → Task 1 `EquipmentReference`. ✓
- §4 `EquipmentReference` shape + Rarity matching insensitivity → Task 1 Step 1/4. ✓
- §5 `CharacterStatsCalculator` -1 penalty scoped to `DamageBonus`/`DefenseBonus` only, `ArmorDamageReduction` untouched → Task 1 Step 4. ✓
- §6 UI (badge + Reparar, client-side mutation, no HTTP call) → Task 2. ✓
- §7 testing (damaged weapon/armor, undamaged unaffected, unrecognized Rarity never damaged) → Task 1 Steps 2-3. ✓
- The `AddItem` zero-durability gap found while writing this plan (not in the spec, but a direct, uncontroversial consequence of spec Decision #4) → Task 2 Step 1. ✓
- **Deliberately out of scope (per spec §1, not gaps):** Resistente's +2 bonus (Properties not modeled), automatic durability depletion, Guild/installation dependency for repair, Teste Absoluto for repair, a shared interlude-day budget.

**2. Placeholder scan:** every step has complete, real code — no "TBD"/"add appropriate handling"/"similar to Task N". The 3 pre-existing-test fixes are each given verbatim before/after code, not a vague "update the durability values" instruction.

**3. Type consistency:** `EquipmentReference.MaxDurabilityFor(string) → int` / `RepairDaysFor(string) → int` identical across Task 1's definition, `CharacterStatsCalculator.IsDamaged`'s call site, and Task 2's `RarityOf`/`IsDamaged`/`RepairAsync`/`AddItem` call sites. `IsDamaged`'s two overloads (`CharacterStatsCalculator`'s private static one taking `(CharacterEquipmentEntry, EquipmentItemCatalogData)`, and the Razor component's instance one taking just `(CharacterEquipmentEntry)` and resolving Rarity itself via `RarityOf`) are deliberately different shapes for different layers — the server has the strongly-typed `EquipmentItemCatalogData` already deserialized, the client only has the raw catalog `DataJson` string via the existing `CatalogEntryData.Parse` convention already used by `CategoryLabelOf` in the same file.
