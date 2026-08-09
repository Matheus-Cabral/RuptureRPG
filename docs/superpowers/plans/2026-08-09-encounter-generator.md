# Encounter Generator (GM-2) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Persisted per-campaign encounters whose difficulty (PG/PE/R/OA + FCE) is computed server-side from the campaign party and the GM's bestiary, per GDD §9.8/§9.9.

**Architecture:** A pure `EncounterCalculator` (Application) holds the closed math; an `Encounter` entity (per campaign, `DataJson` blob) persists the GM's inputs; `IEncounterService` resolves party NP (character service), creature NP (bestiary), and campaign Pressão, then computes the response. Blazor editor with a live readout. **Depends on GM-1 (bestiary) — this plan is executed after GM-1.**

**Tech Stack:** .NET 8, EF Core 8 (+ migration), Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.

**Spec:** `docs/superpowers/specs/2026-08-09-encounter-generator-design.md`. GDD §9.8/§9.9/§9.2/§6.8.

## Global Constraints

- **Server-authoritative computation** — PG/PE/R/OA/FCE always recomputed via `EncounterCalculator`; client-sent computed values ignored.
- **Math (exact):** PG = Σ NP(party) × Synergy(1→1.0/2→1.1/3→1.2/4→1.3/5→1.4/6+→1.5). PE = Σ(creatureNp×qty) × QuantityMult(1→1/2-3→1.25/4-8→1.5/9-20→2/21+→3) × Intelligence(Instinto1/Tatico1.2/Militar1.5/Genial2) × Terrain(Neutro1/LevementeFavoravel1.1/Favoravel1.25/Extremo1.5) × Objective(Eliminar1/Sobreviver1.25/Defender1.5/Resgatar1.5/MissaoCritica2) × PressureMult(1.0 unless ApplyPressure→campaign DungeonPressure PE multiplier). R = PE/PG (guard PG≤0). OA = PG × Difficulty(Seguro0.75/Normal1.0/Perigoso1.25/Mortal1.5/Infernal2.0/Apocaliptico3.0) × Duration(Curto1/Normal2/Longo3/Extenso4). FCE band by group Ranking (Bronze-Ferro0.40/Aco-Prata0.25/Ouro-Mithril0.15/Adamante-Lendario0.10); RealStatMultiplier = 1+(R−1)×FCE.
- **R→label bands** (not exact matches): ≤0.5 Muito facil; ≤0.85 Facil; ≤1.15 Equilibrado; ≤1.4 Dificil; ≤1.75 Muito dificil; <3 Extremo; ≥3 Possivel morte. (Tune thresholds in the calculator; document them.)
- **GM-of-campaign auth** — every endpoint `[Authorize(Roles = "GameMaster")]`; caller must own the campaign else 404 (mirror `CampaignDashboardService`).
- **`Ruptura.Shared` zero project refs** — enum-like values are strings via `EncounterReference`.
- **i18n** both resx (Web + API); `ErrorCodes.Encounter.*` guarded by the reflection localization test.
- **Reuse, don't duplicate:** party NP via `ICharacterSheetService.GetByCampaignAsync`; creature NP via the bestiary service/repo (GM-1); Pressão via the `Campaign` entity + `Ruptura.Shared.Campaigns.DungeonPressure`.
- **Commit per task**; Testcontainers Serilog flake → re-run once.

## File Structure

**Create:** `Domain/Entities/Encounter.cs`; `Shared/Encounters/*` (EncounterData+nested, EncounterResponse, requests, EncounterReference); `Application/Services/EncounterCalculator.cs` + `Interfaces/IEncounterCalculator.cs`; `Interfaces/IEncounterService.cs`, `IEncounterRepository.cs`; `Infrastructure/Services/EncounterService.cs`, `Repositories/EncounterRepository.cs`, `Data/Configurations/EncounterConfiguration.cs`; `API/Controllers/EncounterController.cs`; `Web/Pages/GmEncounters.razor` + `Web/Services/IEncounterClientService.cs`(+impl); unit + integration tests.
**Modify:** `AppDbContext`, `ErrorCodes.cs`, DI, resx, `GmCampaignDetail.razor` (link).

---

### Task 1: Entity + DTOs + `EncounterReference` + EF + migration

- [ ] **Step 1:** `Encounter` entity (`Id`, `CampaignId`, `Name`, `DataJson="{}"`, `CreatedAt`, `UpdatedAt`).
- [ ] **Step 2:** `Shared/Encounters/`:
  - `EncounterData` (`List<EncounterCreature{CreatureId,Quantity}>`, `Intelligence`, `Terrain`, `Objective` strings, `bool ApplyPressure`, `int? Floor`, `int? PartyNpOverride`, `int? PartySizeOverride`, `DesiredDifficulty`, `Duration`).
  - `EncounterResponse` (inputs + `List<EncounterCreatureResolved{CreatureId,CreatureName,Np,Quantity}>` + `int Pg`, `int Pe`, `decimal R`, `string RLabel`, `int Oa`, `decimal Fce`, `decimal RealStatMultiplier`, `bool PressureApplied`, `int PressureValue`, `bool PartyResolved`).
  - `CreateEncounterRequest`/`UpdateEncounterRequest` (`Name` + `EncounterData`).
  - `EncounterReference` (static): the multiplier maps (Synergy, Quantity, Intelligence, Terrain, Objective, Difficulty, Duration) + the FCE-by-ranking map + the R→label bands + the picker string lists. Single source of truth.
- [ ] **Step 3:** EF config (index `CampaignId`) + `DbSet<Encounter>`; `dotnet ef migrations add AddEncounters` (confirm only the `Encounters` table). Build.
- [ ] **Step 4:** Commit (`feat: add Encounter entity, DTOs, EncounterReference, migration`).

---

### Task 2: `EncounterCalculator` + unit tests

- [ ] **Step 1:** Failing unit tests (`tests/Ruptura.UnitTests/Encounters/EncounterCalculatorTests.cs`): the calculator takes explicit inputs — `Calculate(partyNps, partySize, creatures:(np,qty)[], intelligence, terrain, objective, pressureMult, difficulty, duration, fceBand)` → a result record (`Pg, Pe, R, RLabel, Oa, Fce, RealStatMultiplier`). Cover: synergy tiers; quantity/intelligence/terrain/objective tables; R label bands at boundaries; OA (difficulty×duration); FCE bands + RealStatMultiplier; Pressão applied vs not; empty party → `PartyResolved=false`, no divide-by-zero; the §9.8 vignette (5 weak goblins → "Muito facil"). Run → fail.
- [ ] **Step 2:** Implement `EncounterCalculator` from the Global-Constraints math + `EncounterReference` maps; `decimal` for R/multipliers, `long`+clamp for PG/PE/OA; guard PG≤0. Run → pass; full unit sweep.
- [ ] **Step 3:** Commit (`feat: add EncounterCalculator`).

---

### Task 3: Service + repository + controller + API (+ integration tests)

- [ ] **Step 1:** `ErrorCodes.Encounter` (`NotFound`, `Forbidden`, `NameRequired`, `IntelligenceInvalid`, `TerrainInvalid`, `ObjectiveInvalid`, `DifficultyInvalid`, `DurationInvalid`) + API resx (both) + extend the reflection localization guard.
- [ ] **Step 2:** Failing integration tests (`tests/Ruptura.IntegrationTests/Encounters/EncounterTests.cs`): GM creates an encounter referencing a seeded creature → 201; GET computes `Pg` from a seeded alive party and `Pe`/`R` from the creature (assert against a hand-computed expected); a different GM → 404; a client-sent `Pe` in the body is ignored (there is none on the DTO — assert the computed value derives from data only); unknown `Terrain` → 400; `PartyNpOverride` overrides the auto PG. Run → fail.
- [ ] **Step 3:** `IEncounterRepository` (by campaign, by id) + impl; `IEncounterService` (`GetForCampaignAsync`/`GetByIdAsync`/`CreateAsync`/`UpdateAsync`/`DeleteAsync`) + impl:
  - Auth: GM owns the campaign (else `Encounter.NotFound`).
  - Mapping: resolve creature NPs via the bestiary service/repo (own+official; missing creature → Np 0, flagged), party NP via `ICharacterSheetService.GetByCampaignAsync` (alive, non-retired) unless `PartyNpOverride`/`PartySizeOverride` set; Pressão via the `Campaign` (`DungeonPressure.StateFor(campaign.Pressure)`) when `ApplyPressure`; group Ranking band from the party characters' `Ranking` (dominant; document the rule); then `EncounterCalculator`.
  - Validation: structural blob; quantities clamped ≥1; fixed-set validation (400 on unknown); `Name` required.
  - `EncounterController` (`api/campaigns/{campaignId}/encounters...`), DI.
- [ ] **Step 4:** Run → pass; full sweep. Commit (`feat: add encounter service + API`).

---

### Task 4: Encounter UI (`/gm/campaigns/{id}/encounters`)

- [ ] **Step 1:** `IEncounterClientService` + impl (list/get/create/update/delete) per `CampaignClientService` conventions.
- [ ] **Step 2:** `GmEncounters.razor` (`@page "/gm/campaigns/{Id:guid}/encounters"`, `[Authorize(Roles="GameMaster")]`): master-detail — saved encounters list + editor: add-creature picker (over the GM's bestiary: own + official) with quantity rows; Intelligence/Terrain/Objective pickers (Intelligence default suggested from the added creatures' Behaviors); ApplyPressure toggle; Difficulty + Duration; **live readout** of Pg / Pe / R + color-coded `RLabel` (reuse the dashboard pressure color scale) / Oa / Fce + RealStatMultiplier; a "no alive party" hint when `PartyResolved` is false. Breadcrumbs + toolkit (loading/toast/confirm-delete); all strings i18n both resx.
- [ ] **Step 3:** Link from `GmCampaignDetail.razor` (button row) → `/gm/campaigns/@Id/encounters`. Build clean; commit (`feat: add GM encounters page`).

---

## Self-Review

**1. Spec coverage:** math (§3) → Task 2 (+ constants in Task 1 `EncounterReference`); persistence/model (§4) → Task 1; service/auth/resolution (§5) → Task 3; UI (§6) → Task 4; testing (§7) → per task. Out-of-scope (OA auto-distribution, running combat, loot) absent. ✓
**2. Placeholder scan:** the calculator math and DTO shapes are concrete; the two "document the rule" notes (R-label thresholds, group-Ranking derivation) are given concrete starting values and a scoped instruction, not vague. Task 4 is pattern-directive with the full control set enumerated. No "TBD"/"handle appropriately".
**3. Type consistency:** `EncounterReference` maps + the formula referenced identically in the calculator (Task 2), service (Task 3), and UI (Task 4). `EncounterResponse` fields (`Pg/Pe/R/RLabel/Oa/Fce/RealStatMultiplier/PressureApplied/PartyResolved`) produced by the calculator/service and consumed by the UI + tests. `ErrorCodes.Encounter.*` in service + resx + guard. Depends on GM-1's `CreatureResponse.DerivedNp` (resolved via the bestiary service) and the existing `ICharacterSheetService.GetByCampaignAsync` + `DungeonPressure`.
