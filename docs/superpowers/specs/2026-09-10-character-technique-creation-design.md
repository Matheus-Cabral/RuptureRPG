# Character Interlude — Technique Creation — Design Spec

**Date:** 2026-09-10
**Status:** Approved (design), pending implementation plan
**Feature:** Sub-project #3 of 5 in the "Character Interlude" decomposition ([[project-character-interlude]] memory). #1 (Skill Training) and #2 (Equipment Repair) are complete. #4 (Crafting Pessoal), #5 (Pesquisa de Magia) remain unplanned.
**GDD sources:** §6.6.7 (Árvore de Técnicas por Estilo — requisitos formais, criação de Técnicas Novas), §10.3.1 (Academia Militar unlocks Técnicas Supremas).

---

## 1. Goal & Scope

A character can start a **Projeto de Técnica** during Interlúdio — a time-based project (5/10/10/25 days by category) tied to an already-invested weapon/style Perícia — that, once the time passes, awaits the GM's manual Teste Absoluto resolution. Success creates a new homebrew Technique in the catalog and adds it to the character's known Techniques; Failure resets the invested days so the player can try again.

Unlike sub-project #1 (needs a server preview because of Guild bonuses) or #2 (needs no server call at all), this sub-project needs **exactly one** server round-trip: validating a project can legally START (Perícia mínima, Ranking mínimo, and — for Técnica Suprema only — Academia Militar built in the campaign's Guild). Day-by-day progress and the actual project record are then client-side, riding the existing autosave, matching #1/#2's established "no dedicated persisting endpoint" posture.

**Out of scope (deliberate):**
- **Variações** (GDD: "uma técnica-base pode ganhar variações situacionais... desbloqueadas via alta correlação de Especialização") — too vague to mechanize (no computable trigger, mirrors how skill Especializações were already deferred per [[project-campaign-architecture]] item 4a). Not modeled at all in this sub-project.
- **Automatic PA (Pontos de Ação) tracking** — PA isn't modeled anywhere in this app's Combat module (a pre-existing spec gap, [[project-campaign-architecture]] Character Sheet core item 4); a Technique's `PaCost` stays informational free text on the catalog entry, same as it is today.
- **Dice/Teste Absoluto automation** — the app has no dice roller anywhere; the GM resolves the test's outcome manually and reports it via a Sucesso/Falha action (Key Decision #3).
- **A concurrency limit on projects** — GDD doesn't state one for Técnicas (unlike Provação's explicit "apenas 1 por vez"); a character may have multiple in-progress projects (Key Decision #2).

---

## 2. Key Decisions (settled during brainstorming)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Prerequisite validation | **Enforced**, at project start only — Perícia mínima (from GDD's requisitos-formais table) + Ranking mínimo (Suprema only, Prata+) + Academia Militar built (Suprema only). Closes a pre-existing gap (today NOTHING validates Talent/Spell/Technique prerequisites). |
| 2 | Concurrent projects | **Unlimited** — consistent with Skill Training (no cap on parallel skills), not with Provação's explicit 1-at-a-time rule (which doesn't apply here per the GDD's silence). |
| 3 | Test resolution | **GM marks Sucesso/Falha manually** once `DaysInvested >= RequiredDays` — closes the loop instead of leaving the final step entirely outside the system. |
| 4 | Failure penalty | **`DaysInvested` resets to 0**, project stays open for a retry. The GDD only specifies a Falha penalty (time + half materials) for Provação, not for Técnica creation — inventing a materials-loss rule here would be design overreach; resetting days (not deleting the project) mirrors Provação's "não bloqueia" spirit without borrowing a penalty the GDD didn't state for this system. |
| 5 | Data storage | New blob module `CharacterSheetData.TechniqueProjects` (`List<CharacterTechniqueProject>`) — no migration, same posture as #1/#2. |
| 6 | Day-progress mechanism | **100% client-side** — the GDD states no installation/instructor bonus for Técnica creation speed (unlike Skill Training), so there's no server-computed rate to preview; `DaysInvested = min(RequiredDays, DaysInvested + days)` is a pure client mutation. |
| 7 | Start-validation mechanism | **One read-only server endpoint** (`GET .../technique-projects/validate-start`) — the only part of this sub-project needing data the client doesn't have loaded (the campaign's Guild building state, for Suprema). On success, the CLIENT appends the new project to `Data.TechniqueProjects` and autosave persists it — the endpoint itself never persists anything, matching #1's Preview/Apply split. |
| 8 | Homebrew Technique creation on Success | Reuses the existing GM-only `POST /api/catalog` endpoint (`[Authorize(Roles="GameMaster")]`) — same pattern as #1's inline "Nova Perícia". Only the GM can resolve a test and create the resulting catalog entry. |
| 9 | UI placement | **New section inside the existing "Interlúdio" tab** (alongside sub-project #1's Training section) — unlike #2, which deliberately stayed out of that tab (it had a more natural home on the Equipment tab). Técnica Projects has no equally natural home elsewhere, and it's explicitly an Interlúdio activity like Training — this is the point where the Interlúdio tab reasonably grows a second section, one Razor component per concern (`CharacterSheetTechniqueProjectsTab.razor`, new, mounted next to `CharacterSheetTrainingTab.razor` under the same tab key). |
| 10 | Wire values | Category strings unaccented on the wire (`"Postura"`, `"Tecnica"`, `"Reacao"`, `"Suprema"`), matching the Correlação convention from #1 (`"Media"` not `"Média"`). |

---

## 3. Tables (GDD §6.6.7, FECHADO — copy verbatim into `TechniqueReference`)

| Categoria (wire value) | Dias do Projeto | Perícia mínima na arma/estilo | Ranking mínimo |
|---|---:|---:|---|
| Postura | 5 | 25 (Adepto) | — |
| Tecnica | 10 | 50 (Especialista) | — |
| Reacao | 10 | 50 (Especialista) | — |
| Suprema | 25 | 75 (Mestre) | Prata+ (also requires Academia Militar built, §10.3.1) |

Ranking comparison reuses the existing `Ruptura.Shared.RankProgression.Ordered` (8-entry ascending list) — `IndexOf(character's Ranking) >= IndexOf("Prata")`. Skill-points comparison reads the character's current `Points` for the chosen Skill from `Data.Skills` (0 if not invested at all — this naturally fails every category's minimum, no special-casing needed).

---

## 4. `TechniqueReference` (new, `Ruptura.Shared.CharacterSheets`, mirrors `TrainingReference`/`EquipmentReference`)

```csharp
public static class TechniqueReference
{
    public static readonly IReadOnlyList<string> Categories = ["Postura", "Tecnica", "Reacao", "Suprema"];

    public static readonly IReadOnlyDictionary<string, int> RequiredDaysByCategory = new Dictionary<string, int>
    {
        ["Postura"] = 5, ["Tecnica"] = 10, ["Reacao"] = 10, ["Suprema"] = 25
    };

    public static readonly IReadOnlyDictionary<string, int> MinSkillPointsByCategory = new Dictionary<string, int>
    {
        ["Postura"] = 25, ["Tecnica"] = 50, ["Reacao"] = 50, ["Suprema"] = 75
    };

    // Only Suprema has a minimum Ranking; null means "no requirement."
    public static readonly IReadOnlyDictionary<string, string?> MinRankingByCategory = new Dictionary<string, string?>
    {
        ["Postura"] = null, ["Tecnica"] = null, ["Reacao"] = null, ["Suprema"] = "Prata"
    };
}
```
An unrecognized `Category` string is a request-shape error (`400`), not a silently-ignored value — unlike Área/Rarity (free catalog text), Category here is a closed, UI-driven dropdown value, so there's no legitimate "unknown" case to degrade gracefully.

---

## 5. Data Model

### 5.1 `CharacterSheetData` — new module, no migration

```csharp
public class CharacterSheetData
{
    // ...existing modules unchanged...
    public List<CharacterTechniqueProject> TechniqueProjects { get; set; } = [];
}

public class CharacterTechniqueProject
{
    public Guid Id { get; set; }                  // client-generated (Guid.NewGuid()) at creation
    public string Name { get; set; } = string.Empty;      // draft name for the eventual Technique
    public string Category { get; set; } = string.Empty;  // TechniqueReference.Categories value
    public Guid SkillCatalogEntryId { get; set; }          // the arma/estilo Perícia this ties to
    public int RequiredDays { get; set; }          // copied from TechniqueReference at creation (server-derived, never client-computed)
    public int DaysInvested { get; set; }
}
```
No `Status`/`IsComplete` field needed — "ready for test" is simply `DaysInvested >= RequiredDays`, computed wherever needed (same "derive, don't duplicate" posture as #1/#2). Follows the `NormalizeSheetData` convention ([[project-campaign-architecture]] Character Sheet core item 5) — `TechniqueProjects ??= []` at the deserialization boundary.

### 5.2 New Shared DTO

```csharp
public class TechniqueProjectValidation
{
    public bool CanStart { get; set; }
    public string? BlockedReason { get; set; }   // "InsufficientSkill" | "InsufficientRanking" | "MissingInstallation" | null
    public int RequiredDays { get; set; }         // display-only; also what the client copies onto the new project
}
```

---

## 6. API & Permissions

- `GET character-sheets/{id:guid}/technique-projects/validate-start?category={c}&skillCatalogEntryId={id}` — read-only. Route matches the established (non-campaign-scoped) `character-sheets/{id}` convention (`CharacterSheetController`).
  - `400` if `category` isn't one of `TechniqueReference.Categories`, or `skillCatalogEntryId` doesn't resolve to a Skill-type catalog entry visible to the sheet's campaign (mirrors #1's `SkillNotFound`/validation pattern exactly).
  - `404` via the existing owner-or-GM `AuthorizeAccessAsync` (hide-existence convention).
  - `200` with `TechniqueProjectValidation` otherwise — `CanStart=false` + a `BlockedReason` for a legitimate domain rejection (not enough Points, Ranking too low, Academia Militar missing) is NOT an error status; it's the normal "no" answer to a "can I?" question, same posture as the rest of this app's preview endpoints.
- No apply/create/resolve endpoint — the client mutates `Data.TechniqueProjects` directly (project creation, day progress, Falha's day-reset) and the existing whole-sheet `PUT character-sheets/{id}` persists it, exactly like #1/#2's Apply model.
- Test-resolution **Sucesso** path calls the existing `POST /api/catalog` (GM-only) to create the Technique, then the client adds a `CharacterCatalogRefEntry` to `Data.Techniques` and removes the finished project from `Data.TechniqueProjects` — no new endpoint.
- Test-resolution **Falha** path and the Sucesso/Falha buttons themselves are **GM-only in the UI** (gated by the same `IsGameMaster`/`CanEditStatus` parameter #1 and #2 already thread through `CharacterSheetEditor`) — consistent with Decision #8 (only a GM can create the catalog entry) and with the GDD's Teste Absoluto being something the Mestre adjudicates.

---

## 7. UI (`CharacterSheetTechniqueProjectsTab.razor`, new — mounted in the "Interlúdio" tab)

1. **Start a project** (owner or GM): pick a known Skill (from `Data.Skills`, resolved via the existing catalog-picker pattern), pick a Category (dropdown), type a draft Name. "Validar e Iniciar" calls the validate-start endpoint.
   - `CanStart=true`: append a new `CharacterTechniqueProject` (Id, Name, Category, SkillCatalogEntryId, `RequiredDays` from the response, `DaysInvested=0`) to `Data.TechniqueProjects`. Toast success.
   - `CanStart=false`: toast the localized `BlockedReason`.
2. **List in-progress projects**: Name, Category, `DaysInvested`/`RequiredDays`, a days-to-add input + "Avançar" button (client mutation only, capped at `RequiredDays`).
3. **Resolve a ready project** (`DaysInvested >= RequiredDays`, GM-only section): "Sucesso" opens an inline mini-form (Style/Category pre-filled from the project, Damage/Effect/PowerTier — the remaining `TechniqueCatalogData` fields) that POSTs to `/api/catalog` (Type=Technique), then adds the resulting entry to `Data.Techniques` and removes the project. "Falha" resets `DaysInvested` to 0 with a confirm (`ConfirmService`).

All visible strings via `IStringLocalizer`, both Web resx (en + pt-BR), per CLAUDE.md.

---

## 8. `CharacterSheetService` changes

New method (mirrors #1's `PreviewTrainingAsync` shape):
```csharp
Task<Result<TechniqueProjectValidation>> ValidateTechniqueProjectStartAsync(
    Guid callerId, Guid sheetId, string category, Guid skillCatalogEntryId, CancellationToken ct = default);
```
Logic: validate `category` against `TechniqueReference.Categories` → `AuthorizeAccessAsync` → resolve the skill catalog entry (Type=Skill, campaign-visible, mirrors #1's `SkillNotFound` check) → current Points for that skill from `Data.Skills` (0 if absent) → compare against `MinSkillPointsByCategory[category]` → for Suprema, compare `Data.GuildRegistry.Ranking`'s `RankProgression.Ordered` index against `Prata`'s, AND check the campaign's Guild has an active `GuildBuilding` for `GuildCatalogIds.AcademiaMilitar` at Level ≥ 1 (reusing `IGuildSheetRepository`/`IGuildBuildingRepository`, same as #1 — if no Guild exists yet, treat as "not built," `CanStart=false`, never get-or-create). First failing check sets `BlockedReason`; all pass → `CanStart=true` + `RequiredDays` from `TechniqueReference.RequiredDaysByCategory`.

---

## 9. Testing (TDD)

- **Unit:** none needed for `TechniqueReference` itself (a static table, like #1/#2's reference classes) beyond what the service-level tests below exercise indirectly — no pure calculator exists in this sub-project (the "calculation" is a handful of comparisons, not a formula worth isolating into its own pure class).
- **Integration (`Ruptura.IntegrationTests`):** validate-start for each of the 4 categories — sufficient Points+Ranking+installation → `CanStart=true` with the right `RequiredDays`; insufficient Points → `CanStart=false`/`InsufficientSkill`; Suprema with Ranking below Prata → `InsufficientRanking`; Suprema without Academia Militar built → `MissingInstallation`; unknown `category` → `400`; unknown/cross-campaign `skillCatalogEntryId` → `404`/validation error (mirror #1's exact pattern); non-owner-non-GM → `404`.
- **Full-suite sweep required** (per [[project-character-interlude]] decision 1, learned the hard way in #2's final review): this sub-project adds a new blob module and a new field comparison but touches no existing combat-math formula, so a pre-existing-fixture regression is unlikely — still, run the FULL suite (not a filtered one) at least once before any task is marked done, not just the class the task happens to be about.
- No automated UI test (project convention: build + manual).

---

## 10. Reused Patterns / Project Conventions

- Preview/validate is server-computed and read-only; the client applies the result itself through the existing autosave — directly reused from #1 (Decision #5.3 there) and #2 (Decision #7 here, one step further removed since #2 needed no server call at all).
- `RankProgression.Ordered` (`Ruptura.Shared`) reused for the Ranking comparison — do not re-derive the 8-rank list.
- `GuildCatalogIds.AcademiaMilitar` (added in #1) reused for the installation check — do not re-derive the GUID.
- Homebrew catalog creation reuses the existing GM-only `POST /api/catalog`, exactly like #1's inline "Nova Perícia."
- `ConfirmService`/`ToastService` for the Falha-reset confirmation and both outcomes' toasts.
- `IStringLocalizer` for every visible string, both resx cultures.
- "Derive ready-state from data, don't store a redundant flag" — no `Status`/`IsComplete` field, mirrors #2's "Danificado computed live" posture.

---

## 11. Open Items for the Plan

- Exact route/controller placement for the new validate-start action — follow `CharacterSheetController`'s existing conventions (plan should confirm at execution time, like #1's Open Items did).
- Whether the "Sucesso" mini-form pre-fills `Style` from the project's linked Skill's Name (recommended, avoids re-typing) or leaves it free text — plan's call once it reads `CharacterSheetTrainingTab.razor`'s existing inline-creation code for the established pattern to mirror.
