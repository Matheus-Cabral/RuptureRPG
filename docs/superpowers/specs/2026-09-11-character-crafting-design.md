# Character Interlude — Personal Crafting (Caminho B) — Design Spec

**Date:** 2026-09-11
**Status:** Approved (design), pending implementation plan
**Feature:** Sub-project #4 of the "Character Interlude" decomposition ([[project-character-interlude]] memory). #1 (Skill Training), #2 (Equipment Repair), #3 (Technique Creation) are complete. #5 (Pesquisa de Magia) is **dropped** (user decision, 2026-09-11 — redundant with what this sub-project and the Guild's existing `ResearchProject` already cover).
**GDD sources:** §6.7.1 (Raridade), §6.7.4 (Criação — Dificuldade/Tempo/Materiais/Instalação por Raridade, FECHADO), §6.7.7 (Guia Completo — Caminho A vs Caminho B), §10.3.1 (installations, for the Raridade→Instalação reconciliation below), §11.2 item 3 (Receitas Conhecidas/Projetos Descobertos/Receitas Únicas).

---

## 1. Goal & Scope

A character can craft an item they already have a "Receita" for (GDD's Caminho B) during Interlúdio: pick a known recipe, validate the Guild has the minimum installation for its Raridade, run a time-based project, and have the GM resolve the Teste Absoluto — Sucesso adds the crafted item to the character's Equipment, Falha resets the invested days.

This closes the app's largest remaining GDD-modeled gap in this decomposition: personal crafting has zero representation in the codebase today (only the Guild's institutional `CraftingOrder` — NPC artisans working for the Guild, a different system — exists).

**Out of scope (deliberate):**
- **Divino-tier crafting.** The GDD's own text states Divino "Requer projeto de Pesquisa prévio" — a prerequisite system this app doesn't have (and won't: sub-project #5, Pesquisa de Magia, is dropped as redundant). Divino items stay Caminho A only (Mestre-granted).
- **Material resource deduction.** The GDD's Custo em Materiais table (5/15/35/75/150 by Raridade) is shown as information only — nothing is deducted from any resource pool. Modeling a personal "Materials" currency (which doesn't exist on `CharacterCurrency` today) or wiring this into the Guild's shared `Resources.Materials` is real scope this sub-project doesn't take on.
- **The 5-tier margin-of-success outcome scale** (Falha Crítica/Falha/Sucesso/Sucesso Extraordinário/Grande Sucesso). Collapsed to binary Sucesso/Falha, matching sub-project #3's established precedent — the extra tiers' effects (bonus Property, recoverable material, tool/installation damage) aren't modeled.
- **Melhoria, Modificação, Reconstrução (GDD §6.7.5)** — upgrading/modifying an item a character already owns is a distinct mechanic from crafting a new one from a known recipe. Not addressed here.
- **A Perícia mínima gate.** Unlike Técnica creation, the GDD states no minimum skill-points threshold for crafting — it only names "Perícia de Artesanato" generically. No skill-linkage or points check is modeled.
- **"Projeto Descoberto" / "Receita Única" as distinct mechanical states** — collapsed into one "Receita Conhecida" list (Key Decision #1). How a character came to know a recipe (found it, was taught it, it's unique) is narrative flavor the GM tracks outside the system, same posture as how the app already treats e.g. a character's Origin story.

---

## 2. Key Decisions (settled during brainstorming)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Receita Conhecida / Projeto Descoberto / Receita Única | **One list**, `Data.KnownRecipes` (`List<CharacterCatalogRefEntry>`, the SAME reused type as Talents/Spells/Techniques) — each entry references an existing `EquipmentItem` catalog entry. No mechanical distinction between how a recipe was obtained. |
| 2 | Known-recipe list management UI | **Reuse `CharacterSheetCatalogRefListTab`** (the existing generic component already used for Talents/Spells/Techniques) with `CatalogType="EquipmentItem"` and `Entries="Data.KnownRecipes"` — zero new Razor component for this part. No inline homebrew-item creation (unlike Skill Training's "Nova Perícia") — `EquipmentItem`'s field set is large enough that the existing GM Catalog admin page's structured form is the right tool; a GM creates the homebrew item there first, then it's pickable here like any official item. |
| 3 | Divino | **Out of scope** (see §1) — only Comum/Incomum/Raro/Épico/Lendário recipes can be crafted via this feature. |
| 4 | Material cost | **Informational only, no deduction** — shown in the preview/validation response for flavor, never subtracted from anything. |
| 5 | Minimum installation | **Enforced**, at project start (mirrors Técnica Suprema's Academia Militar check) — see §4 for the Raridade→Instalação mapping, reconciled against the ALREADY-SEEDED installation descriptions rather than treating §6.7.4's installation names as new/undefined installations. |
| 6 | Outcome granularity | **Binary Sucesso/Falha**, matching Técnica Creation's precedent. |
| 7 | Test resolution | **GM marks Sucesso/Falha manually**, same UX as Técnica Creation, for cross-feature consistency — even though (unlike Técnica) nothing here structurally REQUIRES GM-only resolution, since Sucesso doesn't touch the GM-only catalog-creation endpoint. Kept GM-gated anyway so "who resolves a Teste Absoluto in this app" has one consistent answer. |
| 8 | Sucesso's effect | Adds a **new `CharacterEquipmentEntry`** referencing the recipe's existing `CatalogEntryId` to `Data.Equipment` (no catalog POST — the item already exists, that's what "knowing the recipe" means) — `DurabilityRemaining` initialized via `EquipmentReference.MaxDurabilityFor` (sub-project #2, reused directly). |
| 9 | Skill linkage | **None** — no Perícia mínima gate exists for crafting per the GDD, so a `CharacterCraftingProject` doesn't reference a Skill at all (unlike `CharacterTechniqueProject`). |
| 10 | UI placement | **Split**: known-recipe list management lives on the **Equipment tab** (recipes are fundamentally about equipment, mirrors sub-project #2's placement reasoning); the crafting PROJECT itself (start/advance/resolve) lives in the **Interlúdio tab**, a third section alongside Training and Técnica Projects (mirrors sub-project #3's placement reasoning — it's the time-based Interlúdio action, the list management isn't). |
| 11 | Endpoint/DTO reuse | The validate-start endpoint returns the **exact same `TechniqueProjectValidation` shape** (`{CanStart, BlockedReason, RequiredDays}`) — no new DTO. `BlockedReason` values differ (`"RecipeNotKnown"` / `"MissingInstallation"`, no `"InsufficientSkill"`/`"InsufficientRanking"` since neither check applies here). |

---

## 3. Tables (GDD §6.7.4, FECHADO for Time/Materials; installation mapping is this spec's reconciliation, not verbatim GDD wording — see §4)

| Raridade | Dias de Criação | Custo em Materiais (display-only) |
|---|---:|---:|
| Comum | 1 | 5 |
| Incomum | 3 | 15 |
| Raro | 7 | 35 |
| Épico | 14 | 75 |
| Lendário | 30 | 150 |
| ~~Divino~~ | — | — *(out of scope, §1/§2 Decision #3)* |

---

## 4. Raridade → Instalação mínima (reconciled against `CatalogSeedData.Installations.cs`, not a new table)

GDD §6.7.4 names the installation column "Oficina Básica / Oficina Básica / Ferraria / Ferraria Avançada / Forja Rúnica / Forja Divina" — none of "Ferraria Avançada"/"Forja Rúnica"/"Forja Divina" are separate seeded installations. Cross-referencing §10.3.1's actual installation descriptions resolves each to an existing installation + level:

| Raridade | Installation | Minimum Level | Evidence |
|---|---|---:|---|
| Comum | Oficina | 1 | "Oficina Básica" = Oficina at any built level |
| Incomum | Oficina | 1 | same |
| Raro | Ferraria | 1 | Ferraria's own §10.3.1 description: "Crafting de armas/armaduras (Comum até Raro em Nível I-II; Épico em III+)" |
| Épico | Ferraria | 3 | same description — "Épico em III+" is the "Ferraria Avançada" GDD refers to |
| Lendário | Oficina de Runas | 1 | §10.3.1: "Crafting Épico+; Encantamento de armas" — the only rune-themed installation in the 20-item list; "Forja Rúnica" is descriptive prose for this seeded entry, added to `GuildCatalogIds` in sub-project #2 as `OficinaDeRunas` |
| ~~Divino~~ | ~~Cofre Divino~~ | — | out of scope; noted for completeness only — §10.3.1's Cofre Divino literally says "habilita Crafting Divino (§6.7.4)", confirming the reconciliation method, but this row is never validated by this sub-project |

---

## 5. Data Model

### 5.1 `CharacterSheetData` — two new modules, no migration

```csharp
public class CharacterSheetData
{
    // ...existing modules unchanged...
    public List<CharacterCatalogRefEntry> KnownRecipes { get; set; } = [];   // reuses the existing type
    public List<CharacterCraftingProject> CraftingProjects { get; set; } = [];
}

public class CharacterCraftingProject
{
    public Guid Id { get; set; }
    public Guid RecipeCatalogEntryId { get; set; }   // must be present in Data.KnownRecipes
    public int RequiredDays { get; set; }             // server-derived at creation from the recipe's Rarity
    public int DaysInvested { get; set; }
}
```
"Ready for test" is `DaysInvested >= RequiredDays` — no separate Status field, same "derive, don't duplicate" posture as #2/#3.

### 5.2 Reused Shared DTO — no new type

`Ruptura.Shared.CharacterSheets.TechniqueProjectValidation` (from sub-project #3) is reused verbatim:
```csharp
public class TechniqueProjectValidation
{
    public bool CanStart { get; set; }
    public string? BlockedReason { get; set; } // this feature's values: "RecipeNotKnown" | "MissingInstallation" | null
    public int RequiredDays { get; set; }
}
```

---

## 6. `CraftingReference` (new, `Ruptura.Shared.CharacterSheets`, mirrors `TrainingReference`/`EquipmentReference`/`TechniqueReference`)

```csharp
public static class CraftingReference
{
    // Divino deliberately absent — out of scope (§1).
    public static readonly IReadOnlyList<string> CraftableRarities = ["Comum", "Incomum", "Raro", "Épico", "Lendário"];

    public static readonly IReadOnlyDictionary<string, int> RequiredDaysByRarity = new Dictionary<string, int>
    {
        ["Comum"] = 1, ["Incomum"] = 3, ["Raro"] = 7, ["Épico"] = 14, ["Lendário"] = 30
    };

    public static readonly IReadOnlyDictionary<string, int> MaterialsCostByRarity = new Dictionary<string, int>
    {
        ["Comum"] = 5, ["Incomum"] = 15, ["Raro"] = 35, ["Épico"] = 75, ["Lendário"] = 150
    };

    // (InstallationId, MinLevel) per §4. Rarity keys match EquipmentItemCatalogData.Rarity's
    // existing values (accented, e.g. "Épico"/"Lendário") — same convention as
    // EquipmentReference.MaxDurabilityByRarity from sub-project #2.
    public static readonly IReadOnlyDictionary<string, (Guid InstallationId, int MinLevel)> InstallationByRarity =
        new Dictionary<string, (Guid, int)>
        {
            ["Comum"] = (GuildCatalogIds.Oficina, 1),
            ["Incomum"] = (GuildCatalogIds.Oficina, 1),
            ["Raro"] = (GuildCatalogIds.Ferraria, 1),
            ["Épico"] = (GuildCatalogIds.Ferraria, 3),
            ["Lendário"] = (GuildCatalogIds.OficinaDeRunas, 1)
        };
}
```
`GuildCatalogIds.Ferraria` doesn't exist yet (sub-project #1/#2 added `Oficina`/`OficinaDeRunas`/others but not `Ferraria`) — the plan adds it (installation #5, `d0000000-...-000005`, per `CatalogSeedData.Installations.cs`).

Rarity matching: case/whitespace-insensitive via `.Trim()` + `StringComparer.OrdinalIgnoreCase` on the dictionary construction, matching `EquipmentReference`'s established convention (the free-text `EquipmentItemCatalogData.Rarity` field carries the same GM-typo risk here as it does for Equipment Repair).

---

## 7. API & Permissions

- `GET character-sheets/{id:guid}/crafting-projects/validate-start?recipeCatalogEntryId={id}` — read-only, returns `TechniqueProjectValidation`.
  - `400` if `recipeCatalogEntryId` doesn't resolve to an `EquipmentItem`-type catalog entry visible to the sheet's campaign, OR resolves to one whose Rarity isn't in `CraftingReference.CraftableRarities` (covers both "not an equipment item" and "Divino/unrecognized rarity" in one check — both are request-shape problems, not domain rejections, since the client should never have offered them as choices).
  - `404` via the existing owner-or-GM `AuthorizeAccessAsync`.
  - `200` with `CanStart=false, BlockedReason="RecipeNotKnown"` if the recipe isn't in the character's `Data.KnownRecipes`.
  - `200` with `CanStart=false, BlockedReason="MissingInstallation"` if the Guild doesn't have the required installation built at the required level (no guild yet → not built, same no-get-or-create posture as #1/#3).
  - `200` with `CanStart=true, RequiredDays=<from table>` otherwise.
- No apply/create/resolve endpoint — the client owns creating the project (after a successful validation), advancing `DaysInvested`, and both Sucesso (append to `Data.Equipment`, remove the project) and Falha (reset `DaysInvested`) — all client-side mutations riding the existing autosave, per #1/#2/#3's established posture. Sucesso needs NO server call at all (no catalog creation — Decision #8), unlike Técnica's Sucesso.

---

## 8. `CharacterSheetService` changes

New method, structurally close to `ValidateTechniqueProjectStartAsync`:
```csharp
Task<Result<TechniqueProjectValidation>> ValidateCraftingProjectStartAsync(
    Guid callerId, Guid sheetId, Guid recipeCatalogEntryId, CancellationToken ct = default);
```
Logic: `AuthorizeAccessAsync` → resolve the recipe catalog entry (Type=EquipmentItem, campaign-visible) → deserialize its `Rarity` → if not in `CraftingReference.CraftableRarities`, request-shape error → is `recipeCatalogEntryId` present in `Data.KnownRecipes`? If not, `RecipeNotKnown` → look up the Guild's buildings for `CraftingReference.InstallationByRarity[rarity]`'s installation at the required level (reusing `IGuildSheetRepository`/`IGuildBuildingRepository`, already injected) → `MissingInstallation` if absent → else `CanStart=true` with `RequiredDays = CraftingReference.RequiredDaysByRarity[rarity]`.

---

## 9. UI

### 9.1 Known Recipes (`CharacterSheetEquipmentTab.razor`, new section)

Mount the existing `CharacterSheetCatalogRefListTab` with `Entries="Data.KnownRecipes" CatalogType="EquipmentItem" CampaignId="CampaignId"` in a clearly labeled sub-section ("Receitas Conhecidas") — no new Razor component.

### 9.2 Crafting Projects (`CharacterSheetCraftingProjectsTab.razor`, new — mounted in the "Interlúdio" tab, third section)

1. **Start a project**: pick a known recipe (from `Data.KnownRecipes`, resolved via a small catalog fetch for display names — mirrors `CharacterSheetTechniqueProjectsTab`'s skill-name resolution). "Validar e Iniciar" calls the validate-start endpoint; `CanStart=true` appends a new `CharacterCraftingProject`; `CanStart=false` toasts the localized `BlockedReason`.
2. **List in-progress projects**: recipe name, `DaysInvested`/`RequiredDays`, a days-to-add input + "Avançar" (client mutation, capped at `RequiredDays`), and a Remove button (per sub-project #3's final-review lesson — every client-side list gets a Remove action from day one, no confirm, matching Skills/Equipment/Talents/Techniques' convention).
3. **Resolve a ready project** (GM-only): "Sucesso" appends a `CharacterEquipmentEntry { CatalogEntryId = project.RecipeCatalogEntryId, Quantity = 1, DurabilityRemaining = EquipmentReference.MaxDurabilityFor(recipeRarity) }` to `Data.Equipment` and removes the project. "Falha" resets `DaysInvested` to 0 with a confirm (`ConfirmService`), matching Técnica's Falha UX.

All visible strings via `IStringLocalizer`, both Web resx (en + pt-BR), per CLAUDE.md.

---

## 10. Testing (TDD)

- **Unit:** a small test pinning `CraftingReference.CraftableRarities`'s entries all have keys in `RequiredDaysByRarity`/`MaterialsCostByRarity`/`InstallationByRarity` (mirrors sub-project #3's final-review-added `TechniqueReferenceTests` — apply proactively this time instead of waiting for a review to ask for it, per [[project-character-interlude]] decision 5).
- **Integration:** validate-start for a known Comum recipe with Oficina built → `CanStart=true, RequiredDays=1`; unknown recipe (not in `KnownRecipes`) → `RecipeNotKnown`; known Raro recipe with no Guild/no Ferraria → `MissingInstallation`; known Épico recipe with Ferraria built at Level 1 (below the Level-3 Épico threshold) → `MissingInstallation` (proves the per-Rarity LEVEL check, not just presence); known Épico recipe with Ferraria at Level 3 → `CanStart=true`; a recipe whose catalog Rarity is "Divino" → `400`; unknown/cross-campaign `recipeCatalogEntryId` → `404`/validation error; non-owner-non-GM → `404`.
- **Full-suite sweep required** before any task is marked done — per [[project-character-interlude]] decision 1, now the standing rule for every sub-project in this decomposition.
- No automated UI test (project convention: build + manual).

---

## 11. Reused Patterns / Project Conventions

- `TechniqueProjectValidation` DTO reused verbatim rather than duplicated (Decision #11) — the first cross-sub-project DTO reuse in this decomposition.
- `EquipmentReference.MaxDurabilityFor` (sub-project #2) reused directly for the crafted item's starting Durability.
- `CharacterSheetCatalogRefListTab` (existing, pre-decomposition component) reused directly for known-recipe list management — the first UI reuse in this decomposition that adds zero new Razor code for a whole piece of the feature.
- "Validate-start is the only server call; everything else is a client-side mutation riding autosave" — directly continues #1/#2/#3's established posture.
- Client-side lists always ship with a Remove action from day one (lesson from #3's final review).
- Rarity/installation reconciliation against ALREADY-SEEDED catalog data (not treating GDD's descriptive installation names as new entities) — same method sub-project #2 used for "Forja Rúnica"/`OficinaDeRunas`.
- `IStringLocalizer` for every visible string, both resx cultures.

---

## 12. Open Items for the Plan

- Exact route/controller placement — follow `CharacterSheetController`'s existing conventions (confirm at execution time, per every prior sub-project's Open Items).
- Whether `CraftingReference` needs its own file or can live alongside `TechniqueReference`/`EquipmentReference` in `Ruptura.Shared.CharacterSheets` (recommended: its own file, matching the one-reference-class-per-file convention already established).
- Confirm `GuildCatalogIds.Ferraria`'s exact GUID against `CatalogSeedData.Installations.cs` at execution time (this spec asserts `d0000000-...-000005` from earlier reads in this decomposition — re-verify, don't assume stale).
