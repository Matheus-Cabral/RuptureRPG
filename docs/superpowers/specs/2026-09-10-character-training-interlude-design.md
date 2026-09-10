# Character Interlude — Skill Training Calculator — Design Spec

**Date:** 2026-09-10
**Status:** Approved (design), pending implementation plan
**Feature:** Sub-project #1 of 5 in the "Character Interlude" decomposition (Treinamento → Reparo de Equipamento → Criação de Técnicas → Crafting Pessoal → Pesquisa de Magia). Only Treinamento is scoped here.
**GDD sources:** §6.4 (Perícias — Pontos de Treinamento/dia, FECHADO), §6.5 (Marcos de Perícia / Bônus de Grau), §10.3.1 (Instalações), §10.4 (Trabalhadores/Instrutores).

---

## 1. Goal & Scope

Deliver a **preview-and-apply Skill Training calculator** for `CharacterSheet`, mirroring the UX and security pattern of the Guild Sheet's existing `InterludeCalculator` (`docs/superpowers/specs/2026-08-07-guild-sheet-design.md` §5) but computing the character-level Treinamento de Perícia formula (GDD §6.4), which that guild calculator explicitly excluded ("Character-level interlude... Future integration").

The owner or the GM picks a skill (already known, or a brand-new homebrew one created inline), a day count, and a Curva de Aprendizado correlation level; previews the computed Pontos de Treinamento; and applies it, which increments that skill's `Points` on the character sheet.

**Out of scope (deferred to later sub-projects in this decomposition, NOT this spec):**
- Provação de Atributo automation (excluded by explicit user decision — stays fully manual).
- Reparo de Equipamento (needs a "Danificado" state that doesn't exist yet).
- Criação de Técnicas (project-based, ends in a manual Teste Absoluto).
- Crafting pessoal do jogador / Caminho B (needs Receita Conhecida / Projeto Descoberto modeling).
- Pesquisa de Magia at character level (no closed GDD rule yet — needs its own brainstorm).
- A campaign calendar / automatic time simulation — same non-goal as the guild calculator; time only advances when a user runs this calculator for N days and applies.

---

## 2. Key Decisions (settled during brainstorming)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Bonus source | Installation/Instructor bonuses are read **automatically** from the campaign's `GuildSheet` (1:1 with Campaign, already established). |
| 2 | UX pattern | **Preview-and-apply**, same UX as the guild's Interlude Calculator — never auto-apply. *(Plumbing, corrected during plan-writing: Preview is a server call; Apply is a client-side mutation of `Data.Skills` through the editor's existing autosave, not a dedicated persisting endpoint — see §5.3.)* |
| 3 | Curva de Aprendizado (correlação) | **Manual** per preview — the player/GM picks Alta/Média/Baixa/Nenhuma; correlation itself is a narrative judgment call, not computable from catalog data. |
| 4 | Bônus de Instrutor | Add 2 new nullable fields to `GuildStaff` (`DedicatedCharacterSheetId`, `DedicatedSkillArea`) so a GM can dedicate an Instrutor to a specific character+área. Bonus applies only when a matching active Instrutor exists. |
| 5 | Área→Instalação mapping ambiguity | Two GDD rows say "conforme o tema"/"conforme a perícia" (Conhecimento's advanced installation, Artesanato's Oficina-vs-Ferraria split) — **not** mechanically resolvable from existing catalog data. Simplified: Conhecimento gets no advanced-installation variant; Artesanato always maps to Oficina (never Ferraria). Documented as a conscious simplification, not a bug. |
| 6 | Cross-Básico-threshold windows | The per-day rate is computed **once**, from the skill's Points at the start of the preview, and applied linearly across all N days. If the window crosses the 10-point (Básico) threshold, the tail days are undercounted (never overcounted) rather than simulated day-by-day. A user can re-preview at any time to get a fresh, accurate rate. |
| 7 | New (not-yet-cataloged) skills | The training UI can create a **new homebrew `CatalogEntry`** (Type=Skill) inline, reusing the existing `POST /api/catalog` endpoint. That endpoint is **GM-only** already (`[Authorize(Roles="GameMaster")]`) — the inline "Nova Perícia" option is therefore visible only when the current editor is the GM, same rule as every other catalog-creation path in the app. |
| 8 | Concurrency | `CharacterSheet.DataJson` has no concurrency token today (documented, accepted gap — CLAUDE.md). Apply goes through the editor's existing Save, unchanged by this feature — no new token introduced. |
| 9 | Rounding | Points added = `floor(rate × days)` — skill `Points` is an `int`; never round up past what the formula guarantees. |
| 10 | Permissions | Same as existing skill editing: owner or GM (whoever can already edit `CharacterSheetEditor`'s Skills tab). |

---

## 3. Formula (GDD §6.4/§6.5 — FECHADO)

```
Pontos de Treinamento/dia = (1 + BônusInstalação + BônusInstrutor) × MultCorrelação
```

- **Base:** 1 point/day.
- **BônusInstalação** = `Nível da instalação relevante × 0.5` (`× 1` if the mapped installation is the "avançada" one for that Área — see §4).
- **BônusInstrutor** = `+1` if an active `GuildStaff` row has `Kind == Instrutor`, `DedicatedCharacterSheetId == this character`, and `DedicatedSkillArea == this skill's Area` (case/whitespace-insensitive compare, per the project's established `CategoryIs`-style convention for free-text catalog fields).
- **MultCorrelação:**
  | Correlação | Multiplicador |
  |---|---:|
  | Alta | ×1.5 |
  | Média (default) | ×1.0 |
  | Baixa | ×0.5 |
  | Nenhuma, enquanto `Points < 50` | ×0.25 |
  | Nenhuma, `Points ≥ 50` | ×1.0 (progressão normal) |

**Faixa "Sem Treinamento" (`Points` 0–9):** Instalação/Instrutor bonuses are **ignored entirely**; rate = `MIN(1 × MultCorrelação, Teto[correlação])`:

| Correlação | Teto (pts/dia) |
|---|---:|
| Nenhuma | 1 |
| Baixa | 2 |
| Média | 3 |
| Alta | 5 |

(In practice `1 × MultCorrelação` never exceeds these ceilings, so the `MIN` never binds — implemented literally anyway, per the project convention of reproducing FECHADO tables exactly rather than only their observable effect.)

**Grade** (Marcos de Perícia, for display only — already computed elsewhere in `CharacterStatsCalculator`, reused not reimplemented): 0=Sem Treinamento, 10=Básico, 25=Adepto, 50=Especialista, 75=Mestre, 100=Lendário.

---

## 4. `TrainingReference` (new, `Ruptura.Shared`) — Área → Instalação mapping

Mirrors `GuildCatalogIds`'s pattern of identifying formula-relevant rows by fixed GUID, never by name. Extends `GuildCatalogIds` with the additional installation IDs this feature needs (`AcademiaMilitar`, `Enfermaria`, `Oficina`, `JardimAlquimico`, `LaboratorioArcano`, `TorreDosMagos` — `CampoDeTreinamento` and `Biblioteca` already exist).

| Área de Perícia (matches `SkillCatalogData.Area`) | Instalação normal | Instalação avançada (×1 instead of ×0.5) |
|---|---|---|
| Combate — Armas/Defesa/Corporal/Distância | Campo de Treinamento | Academia Militar |
| Exploração | Campo de Treinamento (bonus **halved again**, i.e. `Nível × 0.25`) | — |
| Conhecimento | Biblioteca | — *(simplification — see Key Decision #5)* |
| Cura | Enfermaria | — |
| Artesanato | Oficina | — *(simplification — see Key Decision #5)* |
| Alquimia | Jardim Alquímico (fallback: Oficina, if Jardim Alquímico not built) | — |
| Magia | Laboratório Arcano | Torre dos Magos |
| Social | — (Base + Instrutor only) | Academia Militar (**only** when the skill's Name is "Liderança") |

Area comparison is case/whitespace-insensitive (same convention as `CharacterStatsCalculator.CategoryIs`). An Área not in this table (a homebrew skill with an unrecognized Area string) gets no installation bonus (Base + Instrutor only) — never an exception.

---

## 5. Data Model Changes

### 5.1 `GuildStaff` (modified entity) — Instrutor dedication

```csharp
public class GuildStaff
{
    // ...existing fields unchanged...
    public Guid? DedicatedCharacterSheetId { get; set; }  // NEW, nullable — Instrutor-only in practice, not enforced by Kind
    public string? DedicatedSkillArea { get; set; }       // NEW, nullable — matches SkillCatalogData.Area values
}
```

Migration adds both columns (nullable, no backfill needed — existing rows default to un-dedicated). No FK to `CharacterSheet` (bare `Guid`, consistent with the codebase's established soft-reference convention for cross-aggregate references — see `CharacterSheet.CampaignId`, `Campaign.GameMasterId`, etc.). The Guild Staff tab (`GuildStaffTab.razor` or equivalent) gets two new optional inputs, shown only for `Kind == Instrutor`: a party-member picker (reuse whatever roster-listing endpoint the campaign already exposes) and a free-choice Área dropdown (the 8 GDD Área values).

### 5.2 `CharacterSheetData` — no schema change

Training reads/writes the existing `Skills: List<CharacterSkillEntry> { CatalogEntryId, Points }`. Training a brand-new skill (not yet in `Skills`) appends a new entry with `Points = 0` before applying the computed delta. No new fields needed on `CharacterSheetData` itself.

### 5.3 New Shared DTO (`Ruptura.Shared.CharacterSheets`)

```csharp
public class TrainingProjection
{
    public Guid SkillCatalogEntryId { get; set; }
    public string SkillName { get; set; } = string.Empty;      // resolved server-side, display only
    public int CurrentPoints { get; set; }
    public double PointsPerDay { get; set; }                   // display-only
    public int Days { get; set; }
    public string Correlation { get; set; } = string.Empty;    // echoes the request, display only
    public int PointsToAdd { get; set; }                       // floor(rate*days)
    public int ProjectedTotalPoints { get; set; }
}
```

**Apply model — corrected during plan-writing from the guild-mirrored design originally sketched here.** Unlike Guild Sheet children (`ResearchProject`/`CraftingOrder` are real DB rows with their own persisting endpoints, and Silver is guarded by a server-recomputed-delta invariant because a malicious client could otherwise grant itself currency), `CharacterSkillEntry` lives inside the `CharacterSheetData` blob and is **already** 100% client-editable — the existing Skills tab lets the owner/GM type any `Points` value directly into a number input, with zero server-side validation of the total (see `CharacterSheetSkillsTab.razor`). There is therefore no security invariant a dedicated persisting "Apply" endpoint would add. **Apply is a client-side mutation**: clicking "Aplicar" adds `TrainingProjection.PointsToAdd` to the matching `CharacterSheetData.Skills[]` entry (creating it with `Points = 0` first if the skill wasn't yet known) in the same in-memory `_data` the rest of `CharacterSheetEditor` already manages, exactly like every other tab's edits — persisted through the **existing** `AutosaveWatcher`/manual Save path, no new endpoint. This keeps the feature consistent with "follow established patterns" rather than introducing a persistence mechanism that exists nowhere else on `CharacterSheet`. The user-facing UX is unchanged (still preview-then-apply); only the plumbing simplified.

---

## 6. `ITrainingCalculator` (pure, Application)

```csharp
public interface ITrainingCalculator
{
    TrainingProjection Project(
        Guid skillCatalogEntryId, string skillName, string skillArea,
        int currentPoints, IReadOnlyList<GuildBuilding> buildings,
        IReadOnlyList<GuildStaff> staff, Guid characterSheetId,
        int days, string correlation);
}
```

Pure function: no I/O, no persistence. `CharacterSheetService` resolves `skillArea` from the target `CatalogEntry`'s `SkillCatalogData` (default JSON options, per the established catalog-deserialization convention) before calling it, and resolves `buildings`/`staff` from `IGuildSheetRepository.GetByCampaignAsync` (read-only — if the campaign has no `GuildSheet` yet, treat `buildings`/`staff` as empty lists rather than get-or-creating one as a side effect of a read-only preview).

`Correlation` is validated against `{Alta, Media, Baixa, Nenhuma}` (unaccented identifiers on the wire, like the guild's enum-as-string convention) — invalid value → `400 CharacterSheet.CorrelationInvalid`. `Days` bounded `1..3650` (same ceiling as the guild calculator) → `400 CharacterSheet.TrainingDaysInvalid`.

---

## 7. API & Permissions

- `GET /api/character-sheets/{id:guid}/training/preview?skillCatalogEntryId={id}&days={n}&correlation={c}` — read-only preview, matching the existing (non-campaign-scoped) `character-sheets/{id}` route convention (`CharacterSheetController.Get`/`Update`). `skillCatalogEntryId` may reference a skill not yet in `Skills[]` (points start at 0). No apply endpoint — see §5.3.
- Reuses the existing owner-or-GM authorization (`ICharacterSheetService.AuthorizeAccessAsync`). No new permission concept.
- New homebrew skill creation stays on the existing `POST /api/catalog` (`[Authorize(Roles="GameMaster")]`) — the training UI just calls it before previewing, no controller change needed there.

---

## 8. UI (Blazor WASM)

New **"Interlúdio"** tab on `CharacterSheetEditor` (owner and GM both see it, same visibility as the Skills tab):

1. A skill picker: known skills (from `Skills[]`) + "outra perícia do catálogo" (existing catalog-picker pattern, includeArchived:false) + **"Nova Perícia"** (GM-only — visible only when `CanEditStatus`/GM context is true) opening an inline mini-form (Name, Área select from the 8 GDD values, RelatedAttribute) that POSTs to the catalog endpoint and selects the resulting entry.
2. Days input (1–3650) + Correlação dropdown (Alta/Média/Baixa/Nenhuma, default Média).
3. **Preview** button → shows Pontos/dia, Pontos a adicionar, Total projetado.
4. **Aplicar** button → mutates `Data.Skills` in place (see §5.3) and toasts success; the existing `AutosaveWatcher`/Save button persists it like any other edit, no dedicated apply call.

All visible strings via `IStringLocalizer`, both Web resx (en + pt-BR), per CLAUDE.md.

---

## 9. Testing (TDD)

- **Unit (`Ruptura.UnitTests`):** `TrainingCalculatorTests` — canonical GDD examples from §6.4 (Campo de Treinamento II + Média correlação → 2 pts/dia; Campo de Treinamento V + Instrutor + Alta correlação → ≈6.75 pts/dia), the Sem-Treinamento ceiling table (all 4 correlation tiers), the Nenhuma-correlação Points≥50 transition, Exploração's halved-again installation bonus, Social's Instrutor-only + Liderança-only Academia Militar case, unmapped/unknown Área falling back to Base-only, and the `floor()` rounding rule.
- **Integration (`Ruptura.IntegrationTests`):** preview for a known skill, preview for a not-yet-known skill (Points defaults to 0, no Guild yet → Base-only), preview reflects a dedicated Instrutor and built installations from `GuildStaff`/`GuildBuilding`, invalid `Correlation`/`Days` → 400, unknown/non-Skill `skillCatalogEntryId` → 404, non-owner-non-GM → 404 (existing hide-existence convention), `GuildStaff` migration round-trips the two new nullable columns, staff dedication validated (unknown area / cross-campaign character → 400).
- Resx guard test (`CharacterSheetErrorCodeLocalizationTests` if one exists, else create it following the `GuildErrorCodeLocalizationTests` pattern) covers the 2 new error codes in both cultures.

---

## 10. Reused Patterns / Project Conventions

- Preview is server-computed and read-only, mirroring the guild `InterludeCalculator`'s projection step (§5 of the guild spec); Apply is a client-side blob mutation through the existing autosave path (§5.3) rather than the guild's dedicated persisting-apply endpoint — see §5.3 for why that pattern doesn't transfer here.
- `Area`/free-text catalog field comparisons — case/whitespace-insensitive, per CLAUDE.md's `CategoryIs` note.
- Catalog `DataJson` deserialized with **default** `JsonSerializerOptions`, never Web — per the guild calculator's load-bearing note.
- Formula-relevant catalog rows identified by fixed GUID (`GuildCatalogIds`/`TrainingReference`), never by name.
- No FK on the new `GuildStaff` dedication fields — matches the repo-wide soft-reference convention.
- `IStringLocalizer` for every visible string, both resx cultures — no hardcoding.

---

## 11. Open Items — resolved while writing the implementation plan

- **Route/controller placement:** RESOLVED — added as an action on the existing `CharacterSheetController` (§7), not a new controller, matching its `character-sheets/{id:guid}` (non-campaign-scoped) convention.
- **`TrainingReference` location:** RESOLVED — static data in `Ruptura.Shared.CharacterSheets`, mirroring `GuildStaffReference`/`ResearchReference`. No GM-configurability (not requested).
- **Roster-listing endpoint for the Guild Staff "dedicate to character" picker:** RESOLVED — `GuildSheet.razor` already computes GM-vs-player locally (`state.User.FindFirst("role")`); the picker is **GM-only** (a new `IsGameMaster` parameter threads down to `GuildStaffTab`) and sources the roster from the existing GM-only `ICharacterSheetClientService.GetByCampaignAsync` — avoids exposing the party roster to non-GM guild editors, consistent with the standing "roster stays GM-only, member privacy" decision.

---

## 12. Suggested Implementation Ordering

1. `GuildStaff` migration (2 new nullable columns) + Guild Staff tab UI for dedicating an Instrutor (GM-only picker, sourced from the existing GM-only `GetByCampaignAsync` roster endpoint) — small, isolated, testable alone.
2. `TrainingReference` (Área→Installation static table) + `ITrainingCalculator`/`TrainingCalculator` (pure) + full unit test suite (the canonical-example tests above) — no persistence yet.
3. `TrainingProjection` DTO + error codes + `PreviewTrainingAsync` service method + controller GET endpoint + integration tests.
4. Blazor "Interlúdio" tab (skill picker incl. inline homebrew creation, days/correlation inputs, preview + client-side apply) + i18n (both resx).
