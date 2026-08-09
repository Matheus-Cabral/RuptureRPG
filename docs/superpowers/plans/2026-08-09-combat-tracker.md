# In-Session Combat Tracker (GM-4) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A persisted per-campaign combat tracker — initiative order, turn/round, combatant HP + conditions — buildable from a GM-2 encounter + party, with the campaign Pressão embedded (reusing the dashboard endpoint).

**Architecture:** A `CombatSession` entity (per campaign, `DataJson` = `CombatState` blob) persisted; a pure `CombatOrder` helper for ordering + turn advance; `ICombatService` for CRUD + `StartFromEncounter` (server expands creatures + party) + a whole-state PUT. Blazor tracker view. **Depends on GM-1 (creatures), GM-2 (encounters), and the dashboard dungeon endpoint — execute after those.**

**Tech Stack:** .NET 8, EF Core 8 (+ migration), Blazor WASM 8, xUnit + Testcontainers.

**Spec:** `docs/superpowers/specs/2026-08-09-combat-tracker-design.md`. GDD §7.2/§7.7/§7.8/§9.2.

## Global Constraints

- **GM-of-campaign auth** — every endpoint `[Authorize(Roles = "GameMaster")]`; caller owns the campaign else 404 (mirror `CampaignDashboardService`).
- **Whole-state PUT** for live mutations (single GM, last-write, no concurrency token) — mirrors the dashboard dungeon PUT.
- **Server normalizes** — clamp `CurrentPv` to `[0, MaxPv]`, `MaxPv ≥ 0`, `Round ≥ 1`, `CurrentIndex` into range; recompute `IsDefeated` (`CurrentPv <= 0` or has `Morto`); validate every `Condition` against `CombatReference.Conditions` (400 `Combat.ConditionInvalid`).
- **Initiative is GM-entered** (no dice). Ordering: `Initiative` desc, tie-break `Percepcao` desc, stable by name.
- **`Ruptura.Shared` zero project refs**; enum-like values strings via `CombatReference`.
- **Reuse, don't duplicate:** creatures via the bestiary service (GM-1); party via `ICharacterSheetService.GetByCampaignAsync`; the encounter via the encounter service (GM-2); Pressão via the existing `PUT /api/campaigns/{id}/dashboard/dungeon` (no new Pressão backend).
- **i18n** both resx (Web + API); `ErrorCodes.Combat.*` guarded by the reflection localization test.
- **Commit per task**; Testcontainers Serilog flake → re-run once.

## File Structure

**Create:** `Domain/Entities/CombatSession.cs`; `Shared/Combat/*` (CombatState, Combatant, CombatReference, CombatSessionResponse, requests); `Application/Services/CombatOrder.cs`; `Interfaces/ICombatService.cs`, `ICombatSessionRepository.cs`; `Infrastructure/Services/CombatService.cs`, `Repositories/CombatSessionRepository.cs`, `Data/Configurations/CombatSessionConfiguration.cs`; `API/Controllers/CombatController.cs`; `Web/Pages/GmCombat.razor` + `Web/Services/ICombatClientService.cs`(+impl); unit + integration tests.
**Modify:** `AppDbContext`, `ErrorCodes.cs`, DI, resx, `GmCampaignDetail.razor` (link).

---

### Task 1: Entity + DTOs + `CombatReference` + `CombatOrder` (+ unit tests) + EF + migration

- [ ] **Step 1:** `CombatSession` entity (`Id`, `CampaignId`, `Name`, `DataJson="{}"`, `IsActive`, `CreatedAt`, `UpdatedAt`).
- [ ] **Step 2:** `Shared/Combat/`: `CombatState { int Round=1, int CurrentIndex, List<Combatant> Combatants }`; `Combatant { Guid Id, string Name, string Kind, Guid? SourceId, int Initiative, int Percepcao, int MaxPv, int CurrentPv, List<string> Conditions, string Notes, bool IsDefeated }`; `CombatReference.Conditions = ["FeridoLeve","FeridoGrave","Sangrando","Atordoado","Enfraquecido","Amedrontado","Imobilizado","Agonizante","Morto"]`; `CombatSessionResponse { Id, Name, bool IsActive, CombatState State, int Pressure, string PressureStateKey }`; `CreateCombatSessionRequest{Name}`, `UpdateCombatStateRequest{Name,IsActive,CombatState State}`, `StartFromEncounterRequest{Name,Guid EncounterId}`.
- [ ] **Step 3:** Failing unit tests (`tests/Ruptura.UnitTests/Combat/CombatOrderTests.cs`): `Order` sorts by Initiative desc, tie-break Percepcao desc; `AdvanceTurn` from last index wraps to 0 and Round+1; `PreviousTurn` from index 0 goes to previous round's last index and floors Round at 1. Run → fail.
- [ ] **Step 4:** Implement `CombatOrder` (pure). Run → pass.
- [ ] **Step 5:** EF config (index `CampaignId`) + `DbSet<CombatSession>`; `dotnet ef migrations add AddCombatSessions` (confirm only that table). Build; full unit sweep.
- [ ] **Step 6:** Commit (`feat: add CombatSession entity, combat DTOs, CombatOrder, migration`).

---

### Task 2: Service + repository + controller + API (+ integration tests)

- [ ] **Step 1:** `ErrorCodes.Combat` (`NotFound`, `NameRequired`, `ConditionInvalid`, `EncounterInvalid`) + API resx (both) + extend the reflection localization guard.
- [ ] **Step 2:** Failing integration tests (`tests/Ruptura.IntegrationTests/Combat/CombatTests.cs`): create empty session → 201; `StartFromEncounter` expands a seeded encounter's creatures (×quantity → N combatants with the creature PV) and imports the alive party (character combatants with sheet PV); whole-state PUT clamps `CurrentPv` above `MaxPv` down to `MaxPv` and below 0 up to 0, and sets `IsDefeated` when `CurrentPv==0`; unknown Condition → 400; different GM → 404; `StartFromEncounter` with another campaign's encounter → 400 `Combat.EncounterInvalid`. Run → fail.
- [ ] **Step 3:** `ICombatSessionRepository` (by campaign, by id) + impl; `ICombatService` (`GetForCampaignAsync`/`GetByIdAsync`/`CreateAsync`/`StartFromEncounterAsync`/`UpdateStateAsync`/`DeleteAsync`) + impl:
  - Auth (GM owns campaign).
  - `StartFromEncounterAsync`: resolve the encounter (same campaign, else `Combat.EncounterInvalid`), expand creatures (bestiary service; each qty → a `Combatant`, `Name="<creature> #n"`, PV from creature), import alive party (`ICharacterSheetService.GetByCampaignAsync`; PV from `DerivedStats.MaxHp`/`Data.Combat.CurrentHp`), Initiative 0.
  - `UpdateStateAsync`: structural validation + clamps + condition validation + `IsDefeated` recompute; order combatants in the response via `CombatOrder`.
  - Response carries campaign `Pressure` + `DungeonPressure.StateFor(...).StateKey`.
  - `CombatController` (`api/campaigns/{campaignId}/combat...`), DI.
- [ ] **Step 4:** Run → pass; full sweep. Commit (`feat: add combat tracker service + API`).

---

### Task 3: Combat UI (`/gm/campaigns/{id}/combat`)

- [ ] **Step 1:** `ICombatClientService` + impl (list/get/create/startFromEncounter/updateState/delete) per `CampaignClientService` conventions.
- [ ] **Step 2:** `GmCombat.razor` (`@page "/gm/campaigns/{Id:guid}/combat"`, `[Authorize(Roles="GameMaster")]`): sessions list + tracker — combatant rows in initiative order (current highlighted): name, editable Initiative, PV with quick damage/heal, Condition toggles (`CombatReference`), defeated marker, add/remove; "start from encounter" picker (over the campaign's encounters), import-party button; Next/Previous turn + Round counter; a Pressão panel (current + color-coded state + quick +N buttons that call the dashboard dungeon endpoint via the existing campaign client, then refresh). Persist live edits via the whole-state PUT (onchange/debounced). Breadcrumbs + toolkit (loading/toast/confirm-delete); i18n both resx.
- [ ] **Step 3:** Link from `GmCampaignDetail.razor` → `/gm/campaigns/@Id/combat`. Build clean; commit (`feat: add GM combat tracker page`).

---

## Self-Review

**1. Spec coverage:** entity/state model (§3) → Task 1; ordering helper (§4) → Task 1; service/start-from-encounter/PUT/auth (§5) → Task 2; UI (§6) → Task 3; testing (§7) → Tasks 1-2. Out-of-scope (auto damage, dice, player view, PA/Reação) absent. ✓
**2. Placeholder scan:** state model, `CombatOrder` behavior, clamps, and start-from-encounter expansion are concrete; UI is pattern-directive with the full control set. No "TBD"/"handle appropriately".
**3. Type consistency:** `CombatState`/`Combatant`/`CombatReference.Conditions` referenced identically in the helper (Task 1), service validation (Task 2), and UI (Task 3). `CombatSessionResponse` (+ `Pressure`/`PressureStateKey`) produced by the service, consumed by UI. `ErrorCodes.Combat.*` in service + resx + guard. Depends on GM-1 creature PV, GM-2 encounter, `ICharacterSheetService.GetByCampaignAsync`, and the dashboard dungeon endpoint (Pressão) — all reused, not duplicated.
