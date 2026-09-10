# Character Interlude — Equipment Repair — Design Spec

**Date:** 2026-09-10
**Status:** Approved (design), pending implementation plan
**Feature:** Sub-project #2 of 5 in the "Character Interlude" decomposition ([[project-character-interlude]] memory). #1 (Skill Training) is complete; #3 (Criação de Técnicas), #4 (Crafting Pessoal), #5 (Pesquisa de Magia) remain unplanned.
**GDD sources:** §6.7.1 (Raridade — Bônus Base/NP table), §6.7.4 (Criação — days-by-rarity, reused here), §6.7.6 (Durabilidade — Golpes de Desgaste, FECHADO table + Danificado rule).

---

## 1. Goal & Scope

`CharacterEquipmentEntry.DurabilityRemaining` already exists but is purely a free-typed number with no ceiling, no "Danificado" state, and no combat-math effect. This sub-project closes that gap: an item whose `DurabilityRemaining` reaches 0 is **Danificado** (GDD §6.7.6) — its weapon Damage / armor-shield Defense contribution drops by 1 — until repaired during a character's Interlúdio. Repair is a small, self-contained, no-server-round-trip action: a day-cost derived from the item's Raridade, applied client-side (mirrors sub-project #1's Apply pattern).

**Out of scope (deliberate, confirmed during brainstorming):**
- The "Resistente" property's +2 Golpes de Desgaste bonus — `EquipmentItemCatalogData` doesn't model Propriedades/Encantamentos at all today; adding that is a catalog-schema change bigger than this sub-project, not requested.
- Automatic depletion of `DurabilityRemaining` (on a Falha Crítica, etc.) — combat isn't automated in this app; `DurabilityRemaining` stays exactly as manually-adjusted as it is today. This sub-project only adds what happens once it reaches 0 and how to undo that.
- Any Guild/installation dependency for repair, and any Teste Absoluto (pass/fail) for repair — the GDD states neither for repair specifically (unlike Criação, which has both). Repair is guaranteed once its day cost has passed, exactly like Skill Training's guaranteed-progress design.
- A shared/tracked "interlude days remaining" budget across features — consistent with sub-project #1 and the Guild's own Interlude Calculator, day costs shown here are informational for the table, not enforced against any budget.

---

## 2. Key Decisions (settled during brainstorming)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Max Golpes de Desgaste source | **Raridade only**, GDD §6.7.6 table verbatim. The "Resistente" +2 is not modeled (Properties aren't tracked on `EquipmentItemCatalogData` at all). |
| 2 | Repair time | **Scaled by Raridade**, derived from the already-FECHADO Criação time table (§6.7.4): `max(1, ceil(CreationDays[Raridade] / 4))`. Não é um número do GDD (que só diz "tempo curto") — é uma extrapolação consciente, documentada aqui, não uma regra fechada. |
| 3 | Repair test | **Garantido, sem Teste Absoluto** — matches Skill Training's "Princípio do Treinamento Garantido" posture; the GDD states a Teste Absoluto only for Criação, not for repair. |
| 4 | "Danificado" storage | **Computed live**, never persisted — `IsDamaged := DurabilityRemaining <= 0` (only when the item's Raridade resolves to a known max > 0). No new column, no migration. Mirrors `CharacterDerivedStats`'s "always calculated on read, never stored" convention. |
| 5 | Max Durability storage | **Computed live** from `EquipmentReference.MaxDurabilityByRarity[eqData.Rarity]` — not stored per-entry. |
| 6 | "-1 no Bônus Base" scope | Applies only to `DamageBonus` (weapons) and `DefenseBonus` (armor/shield) — the two fields `CharacterStatsCalculator` actually reads for combat math. `EquipmentItemCatalogData.AttackBonus` is confirmed dead code in the calculator (equipment never affects Attack, per CLAUDE.md's documented design decision) and is left untouched. |
| 7 | Apply pattern | **100% client-side**, no server endpoint at all (not even a read-only preview) — unlike Skill Training, repair has no Guild-dependent input to fetch, so there's nothing a server round-trip would compute that the client doesn't already have (the item's Rarity is already loaded for the Equipment tab). |
| 8 | UI placement | **Embedded directly in the existing `CharacterSheetEquipmentTab`**, next to each equipped item — not a separate tab section. Keeps the repair action co-located with the item it affects; avoids growing the Interlúdio tab into a multi-section page prematurely. |
| 9 | Permissions | Same as existing equipment editing: owner or GM (whoever can already edit the Equipment tab). No new permission concept. |

---

## 3. Tables (this sub-project's authority — copy verbatim into `EquipmentReference`)

**Golpes de Desgaste até precisar manutenção (GDD §6.7.6, FECHADO):**

| Raridade | Golpes de Desgaste (= Max Durability) |
|---|---:|
| Comum | 3 |
| Incomum | 4 |
| Raro | 5 |
| Épico | 6 |
| Lendário | 8 |
| Divino | 10 |

**Dias de reparo (derivado, ver Decisão #2 — NÃO uma tabela do GDD):**

| Raridade | Dias de Criação (§6.7.4, referência) | Dias de Reparo = max(1, ceil(Criação/4)) |
|---|---:|---:|
| Comum | 1 | 1 |
| Incomum | 3 | 1 |
| Raro | 7 | 2 |
| Épico | 14 | 4 |
| Lendário | 30 | 8 |
| Divino | — (exige Pesquisa prévia, sem dias fixos) | 10 (fallback, ancorado no próprio teto de Golpes de Desgaste do Divino — maior que Lendário, mantendo a ordem monotônica) |

An unrecognized/homebrew `Rarity` string (not one of the 6 above) resolves to `MaxDurability = 0` — such an item is never "Danificado" (there's no meaningful ceiling to compare against) and shows no repair action. Consistent with the rest of the codebase's "unknown catalog value → no effect, never an exception" convention (`CharacterStatsCalculator.EquipmentNpWeight.GetValueOrDefault(..., 0)`, etc.).

---

## 4. `EquipmentReference` (new, `Ruptura.Shared.CharacterSheets` or `Ruptura.Shared.Catalog` — plan decides based on existing file organization)

```csharp
public static class EquipmentReference
{
    public static readonly IReadOnlyDictionary<string, int> MaxDurabilityByRarity = new Dictionary<string, int>
    {
        ["Comum"] = 3, ["Incomum"] = 4, ["Raro"] = 5, ["Épico"] = 6, ["Lendário"] = 8, ["Divino"] = 10
    };

    public static readonly IReadOnlyDictionary<string, int> RepairDaysByRarity = new Dictionary<string, int>
    {
        ["Comum"] = 1, ["Incomum"] = 1, ["Raro"] = 2, ["Épico"] = 4, ["Lendário"] = 8, ["Divino"] = 10
    };
}
```

Rarity string matching: exact today (`EquipmentItemCatalogData.Rarity` values are GM-typed free text, same caveat CLAUDE.md already documents for `Category`) — the plan should decide whether to apply the same case/whitespace-insensitive `GetValueOrDefault`-via-normalized-key convention used elsewhere (`CharacterStatsCalculator.CategoryIs`), consistent with the project's standing guidance that "any new enum-like free-text catalog field being read by exact `==` elsewhere should do the same."

---

## 5. `CharacterStatsCalculator` changes

For each equipped item (`CharacterEquipmentEntry` where `IsEquipped`):
```
maxDurability := EquipmentReference.MaxDurabilityByRarity.GetValueOrDefault(eqData.Rarity, 0)
isDamaged := maxDurability > 0 && entry.DurabilityRemaining <= 0
```
- Weapon row (`BuildWeaponRow`): `damage = attributeModifier + skillGrade + eqData.DamageBonus - (isDamaged ? 1 : 0)`.
- Armor/shield defense (`armorAndShieldDefense` sum in `Calculate`): each damaged armor/shield item contributes `DefenseBonus - 1` instead of `DefenseBonus`.
- No floor — a Danificado Comum item (Bônus Base already +0) can go negative, matching the GDD giving no floor either.
- `ArmorDamageReduction` is untouched (Decision #6 — "Bônus Base" per §6.7.1 is Ataque/Dano/Defesa only, Redução de Dano is a separate stat).

`CharacterDerivedStats` does **not** need a new field — the UI computes the same `isDamaged`/`maxDurability` client-side from data it already has (the entry's `DurabilityRemaining` + the already-fetched catalog entry's `Rarity`), reusing the same `EquipmentReference` table (Shared, so both Application and Web read it). No new Shared DTO needed for this sub-project.

---

## 6. UI (`CharacterSheetEquipmentTab.razor`)

Per equipped-item row, when `isDamaged`:
- A "Danificado" badge/label.
- A "Reparar" button showing the item's `RepairDaysByRarity[eqData.Rarity]` day cost (e.g. "Reparar (2 dias)"). On click: confirm (reuse `ConfirmService`, per CLAUDE.md's app-wide feedback pattern), then set `entry.DurabilityRemaining = MaxDurabilityByRarity[eqData.Rarity]` directly on the shared `Data.Equipment` list — no HTTP call — and toast success. Persisted by the existing `AutosaveWatcher`, exactly like Skill Training's Apply.

All visible strings via `IStringLocalizer`, both Web resx (en + pt-BR), per CLAUDE.md.

---

## 7. Testing (TDD)

- **Unit (`Ruptura.UnitTests`):** `CharacterStatsCalculatorTests` additions — a damaged weapon's damage formula drops by 1; a damaged armor/shield's Defesa Passiva drops by 1; an undamaged item (positive `DurabilityRemaining`) is unaffected; an item with an unrecognized `Rarity` is never treated as damaged regardless of `DurabilityRemaining`; multiple damaged items stack their penalties independently (one damaged weapon + one damaged armor both apply).
- No integration tests needed for this sub-project — no new endpoint exists (Decision #7). The existing `PUT character-sheets/{id}` write path is unchanged; `DurabilityRemaining` was already a writable field there before this sub-project.
- No automated UI test (project convention: build + manual, per CLAUDE.md and sub-project #1's precedent).

---

## 8. Reused Patterns / Project Conventions

- "Computed live, never persisted" — same posture as `CharacterDerivedStats` and sub-project #1's `TrainingProjection`.
- Client-side Apply riding the existing `AutosaveWatcher` — directly reused from sub-project #1's corrected Apply model (design spec §5.3 of `2026-09-10-character-training-interlude-design.md`), here taken even further since not even a Preview server call is needed.
- `ConfirmService`/`ToastService` for the repair confirmation — CLAUDE.md's app-wide feedback pattern, not a bespoke dialog.
- Unrecognized catalog values degrade to "no effect," never an exception — matches `CharacterStatsCalculator`'s existing `GetValueOrDefault(..., 0)` convention throughout.
- `IStringLocalizer` for every visible string, both resx cultures.

---

## 9. Open Items for the Plan

- Exact namespace/file for `EquipmentReference` (`Ruptura.Shared.CharacterSheets` alongside `TrainingReference`, or `Ruptura.Shared.Catalog` alongside `EquipmentItemCatalogData` — plan should follow whichever the codebase's existing organization favors once it reads the current file layout).
- Whether Rarity matching should be case/whitespace-insensitive (recommended, consistent with `CategoryIs`) — plan should implement this rather than leave it as exact-match, since `EquipmentItemCatalogData.Rarity` is GM-typed free text with the same casing-risk CLAUDE.md already flags for `Category`.
