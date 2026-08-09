# Encounter Generator (GM-2) — Design Spec

**Date:** 2026-08-09
**Status:** Approved (design)
**Feature:** Build and persist balanced encounters per campaign, computing the GDD threat math (PG / PE / R / OA + FCE) from the campaign party and the GM's bestiary.
**GDD sources:** §9.8 (Sistema de Encontros — PG, PE, R), §9.9 (Orçamento de Ameaça — OA, FCE), §9.2 (Pressão multiplier feeds PE), §9.5.3 (Behavior→intelligence multiplier), §6.8 (NP by Ranking).
**Depends on:** GM-1 (bestiary — creatures with `DerivedNp`); the campaign party (character NP) and campaign Pressão (dashboard) already exist.

---

## 1. Goal & Scope

A GM tool to assemble encounters from the bestiary and see, deterministically, how hard they are for a campaign's party — and to plan a floor's threat budget.

- **Persisted** `Encounter` records per campaign (name, floor, chosen creatures×quantity, terrain/objective/intelligence, difficulty/duration, pressure toggle).
- **Server-authoritative computation:** PG (from the campaign's alive party × synergy, or a manual override), PE (from the selected creatures' NP × the §9.8 multipliers, optionally × the campaign's current Pressão multiplier), `R = PE/PG` → difficulty label, `OA = PG × desired-difficulty × duration`, and an FCE advisory (`RealStatMultiplier = 1 + (R−1) × FCE`).

**Out of scope:** auto-distributing the OA across creatures/traps/events (GDD gives suggested proportions — shown as reference text only); running the combat (GM-4); loot (GM-3).

## 2. Key Decisions (settled 2026-08-09)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Persistence | Persisted `Encounter` entity per campaign/floor |
| 2 | PG source | Auto from the campaign's alive party × synergy, with a manual override |
| 3 | Advanced factors | Include the campaign Pressão multiplier (toggle) + FCE advisory (display only) |
| 4 | Encounter Intelligence | GM picker (Instinto/Tatico/Militar/Genial), defaulted from the creatures' Behavior (ambiguity in §9.5.3 Inteligente→Tatico/Militar) |
| 5 | Computation trust | Always server-side; the client's PE/R are never trusted |

## 3. The Math (`EncounterCalculator`, pure, Application)

Given: party NPs (or override PG inputs), a list of `(creatureNp, quantity)`, `Intelligence`, `Terrain`, `Objective`, an optional Pressão multiplier, and desired `Difficulty` + `Duration`, and the group's Ranking band:

```
PG = Σ NP(party) × Synergy(partySize)
    Synergy: 1→1.0 | 2→1.1 | 3→1.2 | 4→1.3 | 5→1.4 | 6+→1.5

baseNp    = Σ (creatureNp × quantity)
totalCount = Σ quantity
PE = baseNp
   × QuantityMult(totalCount)   // 1→1 | 2-3→1.25 | 4-8→1.5 | 9-20→2 | 21+→3
   × IntelligenceMult           // Instinto 1 | Tatico 1.2 | Militar 1.5 | Genial 2
   × TerrainMult                // Neutro 1 | LevementeFavoravel 1.1 | Favoravel 1.25 | Extremo 1.5
   × ObjectiveMult              // Eliminar 1 | Sobreviver 1.25 | Defender 1.5 | Resgatar 1.5 | MissaoCritica 2
   × PressureMult               // 1.0 unless ApplyPressure, then the campaign's DungeonPressure PE multiplier

R = PE / PG   (guard PG==0 → R null/∞ handling: show "no party")
    R label: ≤0.5 Muito facil | ~0.75 Facil | ~1 Equilibrado | ~1.25 Dificil | ~1.5 Muito dificil | ~2 Extremo | ≥3 Possivel morte
             (use band thresholds, not exact matches)

OA = PG × DifficultyFactor × DurationFactor
    Difficulty: Seguro 0.75 | Normal 1.0 | Perigoso 1.25 | Mortal 1.5 | Infernal 2.0 | Apocaliptico 3.0
    Duration:   Curto 1 | Normal 2 | Longo 3 | Extenso 4

FCE band by group Ranking: Bronze-Ferro 0.40 | Aco-Prata 0.25 | Ouro-Mithril 0.15 | Adamante-Lendario 0.10
RealStatMultiplier = 1 + (R − 1) × FCE   (advisory)
```
- All multiplier tables live in a single `EncounterReference` (Shared) — used by the calculator, validators, and UI pickers.
- Overflow/precision: compute in `decimal`/`long` as appropriate; guard divide-by-zero (empty party); unknown enum strings → the neutral multiplier (1) or reject on write (fixed sets).
- The group Ranking band for FCE is derived from the party's characters' `Ranking` (the dominant/most-common, or by average NP mapped to the §6.8 band); document the exact rule in the plan.

## 4. Data Model

- **Entity `Encounter`** (Domain): `Id`, `CampaignId (Guid)`, `Name (string)`, `DataJson (string)`, `CreatedAt`, `UpdatedAt`. New `Encounters` table + migration; index on `CampaignId`.
- **`EncounterData`** (`Ruptura.Shared.Encounters`): `List<EncounterCreature{ CreatureId (Guid), Quantity (int) }>`; `Intelligence`, `Terrain`, `Objective` (strings from `EncounterReference`); `bool ApplyPressure`; `int? Floor`; `int? PartyNpOverride`, `int? PartySizeOverride`; `DesiredDifficulty`, `Duration` (strings).
- **`EncounterResponse`**: the inputs + resolved `List<{CreatureId, CreatureName, Np, Quantity}>` + computed `Pg`, `Pe`, `decimal R`, `RLabel`, `Oa`, `decimal Fce`, `decimal RealStatMultiplier`, `bool PressureApplied`, `int PressureValue`, and a `PartyResolved` flag (false when no alive party and no override).
- Requests: `CreateEncounterRequest { Name, EncounterData }`, `UpdateEncounterRequest { Name, EncounterData }`.

## 5. Service / API

- `IEncounterService` (Application) + impl (Infrastructure), `[Authorize(Roles = "GameMaster")]`, GM-of-campaign only (else NotFound, hide existence — mirror `CampaignDashboardService`):
  - `GetForCampaignAsync(gmId, campaignId)` → the campaign's encounters, each mapped with computed values.
  - `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`.
  - Mapping resolves: creature NPs via the bestiary (`ICreatureService`/repo — own+official; a referenced creature that no longer exists contributes 0 and is flagged), party NP via `ICharacterSheetService.GetByCampaignAsync` (alive, non-retired) unless overridden, the campaign Pressão via the `Campaign` entity + `DungeonPressure`, then calls `EncounterCalculator`.
  - Validation: structural blob; quantities ≥1 clamped; fixed sets (Intelligence/Terrain/Objective/Difficulty/Duration) validated (400 on unknown); `Name` required.
- `EncounterController` (routes `api/campaigns/{campaignId}/encounters...`), DI, `ErrorCodes.Encounter.*` + API resx (both cultures, guard test).

## 6. UI

`/gm/campaigns/{id}/encounters` (GM-only), linked from `GmCampaignDetail`/dashboard. Master-detail: a list of saved encounters + an editor:
- Add creatures from the bestiary (a picker over the GM's creatures — own + official — with quantity per row; remove).
- Pickers: Intelligence (default suggested from the added creatures' Behaviors), Terrain, Objective; toggle "apply current Pressão"; Desired Difficulty + Duration.
- **Live readout:** PG (from the campaign party, with the override control), PE, `R` + a color-coded difficulty label (reuse the dashboard pressure color scale), OA budget, and the FCE advisory (`RealStatMultiplier`). A "no alive party" hint when PG can't be computed.
- All strings i18n (both resx); toolkit for loading/toast/confirm-delete.

## 7. Testing

- **Unit (`EncounterCalculator`):** synergy tiers; quantity/intelligence/terrain/objective multiplier tables; `R` label bands at boundaries; OA (difficulty × duration); FCE bands by ranking + RealStatMultiplier; Pressão applied vs not; empty-party guard (no divide-by-zero); the §9.8 validation vignette (≈5 weak goblins → "muito facil").
- **Integration:** CRUD + GM-of-campaign auth (a different GM → 404); server resolves party NP + creature NP and computes `R` (assert against a seeded party + seeded creatures); a client-sent `Pe`/`R` is ignored; unknown Terrain → 400. Testcontainers.
- No bUnit (UI verified by build + manual).

## 8. Data-Model Impact

| Change | Kind |
|--------|------|
| `Encounter` entity + EF config + migration (`Encounters`) | New |
| `Ruptura.Shared.Encounters`: `EncounterData`(+nested), `EncounterResponse`, requests, `EncounterReference` | New |
| `EncounterCalculator` (pure, Application) + `ErrorCodes.Encounter.*` | New |
| `IEncounterService` + impl + repository + `EncounterController` + DI | New |
| Web: `/gm/campaigns/{id}/encounters` page + client + resx + entry link | New |

No changes to existing entities (reads Campaign Pressão, party NP, and bestiary creatures through existing services).
