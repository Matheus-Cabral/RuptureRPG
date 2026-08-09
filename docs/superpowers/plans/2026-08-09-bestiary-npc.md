# Bestiary + NPC Library (GM-1) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A GM-owned, cross-campaign library of combat Creatures (structured stat block + server-computed NP) and non-combat NPCs, with the 10 GDD base creatures seeded as official examples.

**Architecture:** Two dedicated entities (`Creature`, `Npc`) keyed by owning `GameMasterId` (null = official example), each storing a typed `DataJson` blob (mirrors the guild/character blob pattern). A pure `CreatureStatsCalculator` (Application) computes NP exactly like `CharacterStatsCalculator` (grade-bonus). GM-only CRUD services (own-writable, official read-only). Structured Blazor editors reusing the UI-B master-detail pattern. No changes to existing entities.

**Tech Stack:** .NET 8, EF Core 8 (+ migration), Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.

**Spec:** `docs/superpowers/specs/2026-08-09-bestiary-npc-design.md`. GDD §9.5, §6.8.

## Global Constraints

- **Global GM library** — `Creature`/`Npc` carry `GameMasterId (Guid?)`; null = official example (visible read-only to all GMs). Every endpoint `[Authorize(Roles = "GameMaster")]`. A GM reads own + official; writes/deletes own only (official → Forbidden/NotFound). Never expose another GM's homebrew.
- **NP is server-authoritative** — always recomputed via `CreatureStatsCalculator`; a client-sent NP is ignored.
- **NP formula (grade-bonus, confirmed):** `NP = Σ(attr−1) + Σ skillGradeBonus(points) + Σ characteristicWeight[Menor1/Media3/Maior5/Suprema10] + Σ abilityWeight[Comum5/Avancada10/Suprema20] + Σ equipmentRarity[Comum1/Incomum3/Raro7/Epico15/Lendario30/Divino50]`. `skillGradeBonus`: `>=100→4, >=75→3, >=50→2, >=25→1, >=10→0, _→-2` (same as `CharacterStatsCalculator.SkillGradeBonus`). Unknown weight/tier/rarity → 0. Sum in `long`, clamp to `int`.
- **`Ruptura.Shared` keeps zero project references** — all `CreatureData`/`NpcData` enum-like values are `string`.
- **Every visible/error string via `IStringLocalizer`** (Web: both `AppStrings` resx; API: both `SharedResources` resx); identical key sets; pt-BR accented. `ErrorCodes.Bestiary.*` guarded by a reflection localization test like `GuildErrorCodeLocalizationTests`.
- **Validation clamps, doesn't corrupt** — structural blob validation (valid JSON, no null list elements), `Fraqueza` required on write, non-negative attribute/point bounds; fixed sets (`Behavior`, `Category`) rejected with a 400 if unknown.
- **Commit after each task** on `main`; `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`. Testcontainers Serilog flake — re-run once.

## File Structure

**Create:** `Ruptura.Domain/Entities/Creature.cs`, `Npc.cs`; `Ruptura.Shared/Bestiary/*` (CreatureData, NpcData, CreatureResponse, NpcResponse, requests); `Ruptura.Application/Services/CreatureStatsCalculator.cs` + `Interfaces/ICreatureStatsCalculator.cs`; `Interfaces/ICreatureService.cs`, `INpcService.cs`, `ICreatureRepository.cs`, `INpcRepository.cs`; `Ruptura.Infrastructure/Services/CreatureService.cs`, `NpcService.cs`; `Repositories/CreatureRepository.cs`, `NpcRepository.cs`; `Data/Configurations/CreatureConfiguration.cs`, `NpcConfiguration.cs`; `Data/Seed/BestiarySeedData.cs`; `Ruptura.API/Controllers/BestiaryController.cs`, `NpcController.cs`; `Ruptura.Web/Pages/GmBestiary.razor`, `GmNpcs.razor`; `Web/Services/IBestiaryClientService.cs`(+impl), `INpcClientService.cs`(+impl); unit + integration test files.
**Modify:** `AppDbContext` (DbSets), `ErrorCodes.cs`, `InfrastructureExtensions.cs` + `Program.cs` (DI), API/Web resx, GM nav (`NavMenu`/GM dashboard).

---

### Task 1: Entities + Shared DTOs + EF config + migration

- [ ] **Step 1:** `Creature` and `Npc` entities (Domain) — `Id`, `GameMasterId (Guid?)`, `Name`, `DataJson (string = "{}")`, `CreatedAt`, `UpdatedAt`.
- [ ] **Step 2:** `Ruptura.Shared/Bestiary/`:
  - `CreatureData` with: `Type`, `Function`, `Behavior`, `Category` (strings); `CreatureAttributes` (8 ints: Corpo/Controle/Vigor/Presenca/Intelecto/Percepcao/Vontade/Afinidade, default 1); `List<CreatureNaturalSkill{Name,Points}>`, `List<CreatureCharacteristic{Name,Weight}>`, `List<CreatureAbility{Name,Tier}>`, `List<CreatureEquipment{Name,Rarity}>`; `Pv`, `DefesaPassiva`, `Deslocamento` (ints); `AtaquePrincipal`, `Dano`, `Fraqueza`, `Notes` (strings); `List<string> Recompensas`.
  - `NpcData`: `Role`, `Faction`, `Disposition`, `Location`, `Notes` (strings).
  - `CreatureResponse { Id, Name, bool IsOfficial, CreatureData Data, int DerivedNp, int CategoryNpMin, int CategoryNpMax, bool CategoryOverflow }`; `NpcResponse { Id, Name, bool IsOfficial, NpcData Data }`.
  - `CreateCreatureRequest { Name, CreatureData Data }`, `UpdateCreatureRequest { Name, CreatureData Data }`; same for NPC.
  - `BestiaryReference` (static): the fixed picker lists — `Functions`, `Behaviors`, `Categories`, characteristic `Weights`, ability `Tiers`, equipment `Rarities`, npc `Roles`, `Dispositions` — and the weight/tier/rarity → int maps + the Category → (NpMin, NpMax) map (from §9.5.6). Single source of truth used by the calculator, validators, and UI pickers.
- [ ] **Step 3:** EF: `CreatureConfiguration`/`NpcConfiguration` (index on `GameMasterId`); add `DbSet<Creature>`/`DbSet<Npc>` to `AppDbContext`.
- [ ] **Step 4:** `dotnet ef migrations add AddBestiary` → confirm it creates only `Creatures` + `Npcs` (no drift). Build.
- [ ] **Step 5:** Commit (`feat: add Creature/Npc entities, bestiary DTOs, migration`).

---

### Task 2: `CreatureStatsCalculator` + unit tests

- [ ] **Step 1:** Write failing unit tests (`tests/Ruptura.UnitTests/Bestiary/CreatureStatsCalculatorTests.cs`): NP across all 5 terms; grade-bonus attributes (`Corpo 5` → +4) and natural skills (points 50 → +2); characteristic/ability/equipment weight sums; unknown weight/tier/rarity → 0; Category range + `CategoryOverflow` at boundaries (e.g. Comum 40–70: NP 80 → overflow since > 70×1.15=80.5? pick concrete values); huge/malformed inputs → no throw. Run → fail.
- [ ] **Step 2:** Implement `CreatureStatsCalculator.Calculate(CreatureData) → (int Np, int NpMin, int NpMax, bool CategoryOverflow)` (or a small result record) using the Global-Constraints formula + `BestiaryReference` maps; `ErrorCodes.Bestiary` need not be touched here. `long`-sum + clamp. Reuse the exact `SkillGradeBonus` tiers.
- [ ] **Step 3:** Run → pass; full unit sweep. Commit (`feat: add CreatureStatsCalculator (NP + category advisory)`).

---

### Task 3: Creature service + repository + controller + API (+ integration tests)

- [ ] **Step 1:** Add `ErrorCodes.Bestiary` (`NotFound`, `Forbidden`, `FraquezaRequired`, `BehaviorInvalid`, `CategoryInvalid`) + API resx (both cultures) + extend/ create the reflection localization test to cover `ErrorCodes.Bestiary`.
- [ ] **Step 2:** Write failing integration tests (`tests/Ruptura.IntegrationTests/Bestiary/CreatureTests.cs`): GM creates a creature → 201 with server `DerivedNp` computed (ignores any client NP — there is none on the DTO, so assert `DerivedNp` matches the calculator for the sent data); GET returns own + official (seeded) but NOT another GM's; official creature is not editable/deletable (403/404); create with empty `Fraqueza` → 400 `Bestiary.FraquezaRequired`; unknown `Behavior` → 400. Run → fail.
- [ ] **Step 3:** `ICreatureRepository` (own+official query, by-id) + impl; `ICreatureService` (`GetForGameMasterAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`) + impl (auth: own-writable, official read-only; structural blob + Fraqueza + fixed-set validation; map with `CreatureStatsCalculator`); `BestiaryController` (`[Authorize(Roles="GameMaster")]`, routes `api/bestiary/creatures...`), DI registration.
- [ ] **Step 4:** Run → pass; full sweep. Commit (`feat: add creature CRUD service + API`).

---

### Task 4: NPC service + repository + controller + API (+ integration tests)

- [ ] **Step 1:** Failing integration tests (`NpcTests.cs`): CRUD; own+official visibility; official read-only; basic field round-trip. Run → fail.
- [ ] **Step 2:** `INpcRepository`/`INpcService` + impls (same auth model; NpcData needs only structural validation — no NP), `NpcController`, DI.
- [ ] **Step 3:** Run → pass; full sweep. Commit (`feat: add NPC CRUD service + API`).

---

### Task 5: Seed the 10 base creatures (official examples)

- [ ] **Step 1:** `Data/Seed/BestiarySeedData.cs` — the 10 creatures from GDD §9.5.10 (Goblin Saqueador, Rato Pragado, Esqueleto Guardião, Cultista Corrompido, Aranha das Profundezas, Cavaleiro Corrompido, + the remaining 4 in that table) as `Creature` rows with `GameMasterId = null`, each carrying: the §9.5.10 Type/Function/Behavior/Category and Fraqueza, plus a representative set of Attributes/Characteristics/Abilities that lands `DerivedNp` inside the row's Category range (§9.5.6). Deterministic Guids.
- [ ] **Step 2:** A `SeedBestiary` migration inserting them (follow the existing `Seed*` migration + `CatalogSeedData` pattern).
- [ ] **Step 3:** Integration test: a fresh GM's `GET creatures` returns the 10 official examples, each with `DerivedNp` within its Category range. Run → pass. Commit (`feat: seed the 10 base creatures as official examples`).

---

### Task 6: Bestiary UI (`/gm/bestiary`)

- [ ] **Step 1:** `IBestiaryClientService` + impl (GET list, GET by id, POST/PUT/DELETE) following `CampaignClientService` conventions.
- [ ] **Step 2:** `GmBestiary.razor` (`@page "/gm/bestiary"`, `[Authorize(Roles="GameMaster")]`): master-detail (reuse UI-B `.master-detail`/`.detail-panel`) — searchable list (`TableSearchBox`) on the left, a structured editor on the right: Type (free), Function/Behavior/Category pickers (from `BestiaryReference`), an 8-attribute grid, dynamic add/remove lists (NaturalSkills/Characteristics/Abilities/Equipment/Recompensas — weight/tier/rarity via pickers), combat fields, Fraqueza (required), Notes, and a **live NP + Category advisory** (compute client-side from `BestiaryReference` maps for feedback; the saved response's `DerivedNp` is authoritative). Official entries render read-only. Loading/toast/confirm-delete via the toolkit; all strings i18n (both resx).
- [ ] **Step 3:** Add a GM nav entry / GM-dashboard link to `/gm/bestiary`. Build clean; commit (`feat: add GM bestiary page`).

---

### Task 7: NPC UI (`/gm/npcs`)

- [ ] **Step 1:** `INpcClientService` + impl.
- [ ] **Step 2:** `GmNpcs.razor` (`@page "/gm/npcs"`, GM-only): master-detail — searchable list + a simple form (Role/Faction/Disposition pickers, Location, Notes). Official read-only; toolkit + i18n.
- [ ] **Step 3:** GM nav entry to `/gm/npcs`. Build clean; commit (`feat: add GM NPCs page`).

---

## Self-Review

**1. Spec coverage:** entities/global-scope (§3.1) → Task 1; `CreatureData`/`NpcData` (§3.2/3.3) → Task 1; NP calculator (§4) → Task 2; services/API/auth/validation (§5) → Tasks 3-4; seed (§1/§7-of-spec) → Task 5; UI (§6) → Tasks 6-7; testing (§7) → per task. Out-of-scope (portraits, encounters, loot, faction links) absent. ✓
**2. Placeholder scan:** backend tasks carry concrete formula/signatures; seed transcription and the two UI tasks are pattern-directive with the field set fully enumerated and `BestiaryReference` as the single source for pickers/maps. No "TBD"/"handle appropriately". The one judgment step (authoring seed NP inputs to hit each Category) is explicitly scoped with the acceptance check (DerivedNp within range).
**3. Type consistency:** `CreatureData`/nested list types, `BestiaryReference` maps, and the NP formula are referenced identically in the calculator (Task 2), validators/service (Task 3), seed (Task 5), and UI (Task 6). `CreatureResponse.DerivedNp`/`CategoryNpMin`/`CategoryNpMax`/`CategoryOverflow` produced by the calculator and consumed by the UI. `ErrorCodes.Bestiary.*` used in service + resx + guard test. `GameMasterId?` null-as-official is consistent across entity, repo query, service auth, and seed.
