# CgRecursos Valuation — Design Spec

**Date:** 2026-08-09
**Status:** Approved (design)
**Feature:** Make the guild CG's **Recursos** term value-based and inflation-proof (closes spec §11.3 open item from the guild-sheet spec).
**GDD sources:** §10.8 (CG formula, FECHADO), §10.6.1 (câmbio 1 Moeda de Pacto = 10 Prata), §9.10 / Manual §7 (Valor Estratégico — VE, scale 1-5).

---

## 1. Problem

The guild CG formula is `CG = Infraestrutura + Pesquisa + Logística + Recursos` (§10.8). The GDD defines **Recursos = reservas de Moedas de Pacto + materiais estratégicos (valor convertido)**, and its official table expects small values (5 at Fundação → 180 at Guilda Divina).

The current implementation computes:
```
CgRecursos = PactCoins + DimensionalFragments + Σ Materials.Quantity
```
Two problems:
1. **Inflation vector:** materials count at raw `Quantity`, so a GM/member typing `Ferro: 10000` on the Recursos tab adds +10000 to CG — money/hoarding "resolves the game", violating the GDD's Regra de Ouro and the §10.8 scale. §11.3 was explicitly left open ("materiais estratégicos — needs a concrete conversion rule").
2. **Off-spec terms:** `DimensionalFragments` is summed raw although the §10.8 term is only "Moedas de Pacto + materiais estratégicos"; `Silver` (common currency) is correctly already excluded.

## 2. Decision (settled in brainstorming, 2026-08-09)

Value materials by the GDD's existing **Valor Estratégico (VE), scale 1-5** — not by raw quantity.

| # | Decision | Choice |
|---|----------|--------|
| 1 | Material valuation | Each material stack carries a `StrategicValue` (VE) in `[0,5]`; it, not `Quantity`, feeds CG |
| 2 | Formula | `CgRecursos = PactCoins + Σ clamp(material.StrategicValue, 0, 5)` |
| 3 | Moedas de Pacto | Face value (×1) — dungeon-earned, not free-typed; fits the §10.8 table scale |
| 4 | Silver | Excluded from CG (common currency; already excluded — keep it so) |
| 5 | DimensionalFragments | **Excluded** from the CG Recursos term (still tracked on the Recursos tab; it's the separate RE pillar) — removes the raw-fragment inflation vector |
| 6 | `Quantity` | Retained as inventory only; no longer contributes to CG |
| 7 | Persistence | `StrategicValue` lives in the `GuildSheetData.DataJson` blob — no DB migration, backward-compatible (absent → 0) |

## 3. Data Model

`Ruptura.Shared.Guilds.MaterialStock` gains one field:
```csharp
public class MaterialStock
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }                 // inventory only (no longer feeds CG)
    public int StrategicValue { get; set; }           // VE 0..5 — the CG Recursos contribution
}
```
- Blob-only (serialized inside `DataJson`); existing guild rows deserialize with `StrategicValue = 0` (they stop inflating CG immediately, which is the intended de-inflation — the GM re-tiers strategic stacks deliberately).
- No new DB column, no EF migration.

## 4. Formula (GuildStatsCalculator)

Replace the current Recursos computation with:
```
CgRecursos = PactCoins + Σ clamp(material.StrategicValue, 0, 5)
```
- `Silver` and `DimensionalFragments` are NOT part of `CgRecursos`.
- Keep the existing overflow-safety discipline: sum in `long`, `ClampToInt` into `int` (a pre-existing row could still carry many stacks). Each per-stack term is already bounded to ≤5, but the long-sum + clamp pattern stays for consistency.
- `PactCoins` contributes at face value (unchanged).
- The unused `InflationIndex` reporting stays as-is (out of scope here; it remains a price reference, not applied to CG).

## 5. Validation

- Server-side, on the guild blob write path (the existing `UpdateAsync` blob validator, alongside the current Points/reputation clamps): clamp every `material.StrategicValue` into `[0,5]` before persisting. Malformed/out-of-range values are clamped, never 400 (consistent with the guild's server-normalizes-authoritatively convention).
- The calculator ALSO clamps defensively (`Math.Clamp(m.StrategicValue, 0, 5)`) so a legacy/hand-edited blob can never push a single stack above 5 into CG.

## 6. UI — Recursos tab (guild sheet)

- Each material row gains a small VE input (number or select, `0`–`5`), labelled **"Valor Estratégico (VE)"** with a hint that VE (not Quantity) is what counts toward CG.
- Every visible string via `IStringLocalizer` in both resx (en + pt-BR): the VE column label + hint.
- No change to how `Quantity` is edited/displayed.

## 7. Testing (TDD)

- **Unit (`GuildStatsCalculator`):**
  - `CgRecursos = PactCoins + Σ StrategicValue` for a mix of stacks.
  - A stack with a huge `Quantity` but `StrategicValue ≤ 5` contributes ≤ 5 (the inflation fix).
  - A stack with `StrategicValue > 5` (legacy/hand-edited) is clamped to 5 by the calculator.
  - `Silver` and `DimensionalFragments` do NOT change `CgRecursos`.
  - `PactCoins` contributes at face value.
- **Unit/integration (validator):** a write carrying `StrategicValue = 99` (or negative) is clamped to `[0,5]` and the persisted/returned value reflects the clamp.
- **Regression:** existing guild unit + integration tests stay green (the CG/economy suite).

## 8. Data-Model Impact

| Change | Kind |
|--------|------|
| `MaterialStock`: + `StrategicValue` (blob field) | Modify (Shared) — no DB migration |
| `GuildStatsCalculator` Recursos term (VE sum; drop Fragments/raw-Quantity) | Modify |
| Guild blob write validator: clamp `StrategicValue [0,5]` | Modify |
| Recursos-tab UI: VE input + i18n | Modify (Web) |

## 9. Out of Scope

- Inflation index applied to CG or to prices (remains a computed-but-referenced value; a separate concern).
- Buy/sell economy and secondary-expedition income (separate feature track).
- Re-valuing `DimensionalFragments` into CG via a fixed weight (deliberately excluded per decision #5).
