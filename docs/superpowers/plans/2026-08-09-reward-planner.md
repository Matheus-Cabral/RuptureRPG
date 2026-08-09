# Reward Planner (GM-3) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reusable per-campaign reward packages (Resources, Strategic Assets VE 1-5, Knowledge, items), planning-only, optionally tagged to an encounter/floor.

**Architecture:** A `Reward` entity per campaign (`DataJson` blob), a GM-only CRUD service (validation only — no computation), and a Blazor structured editor reusing the UI-B master-detail pattern. **Optional link to GM-2 encounters — execute after GM-2 (or make the encounter picker degrade gracefully if GM-2 isn't present yet).**

**Tech Stack:** .NET 8, EF Core 8 (+ migration), Blazor WASM 8, xUnit + Testcontainers.

**Spec:** `docs/superpowers/specs/2026-08-09-reward-planner-design.md`. GDD §9.4/§9.10/§10.6.

## Global Constraints

- **Planning only** — no crediting to guild/characters; no randomness; no auto-scaling.
- **GM-of-campaign auth** — every endpoint `[Authorize(Roles = "GameMaster")]`; caller owns the campaign else 404 (mirror `CampaignDashboardService`).
- **`Ruptura.Shared` zero project refs** — enum-like values are strings via `RewardReference`.
- **Validation clamps, doesn't corrupt** — structural blob (valid JSON, no null list elements); `Name` required; `RewardAsset.Ve` clamped `[1,5]`; resource ints + material quantities clamped ≥0; `Category` validated against `RewardReference.Categories` (400); `EncounterId` (if set) must belong to the same campaign (400 `Reward.EncounterInvalid`).
- **i18n** both resx (Web + API); `ErrorCodes.Reward.*` guarded by the reflection localization test.
- **Commit per task**; Testcontainers Serilog flake → re-run once.

## File Structure

**Create:** `Domain/Entities/Reward.cs`; `Shared/Rewards/*` (RewardData+nested, RewardResponse, requests, RewardReference); `Interfaces/IRewardService.cs`, `IRewardRepository.cs`; `Infrastructure/Services/RewardService.cs`, `Repositories/RewardRepository.cs`, `Data/Configurations/RewardConfiguration.cs`; `API/Controllers/RewardController.cs`; `Web/Pages/GmRewards.razor` + `Web/Services/IRewardClientService.cs`(+impl); integration tests.
**Modify:** `AppDbContext`, `ErrorCodes.cs`, DI, resx, `GmCampaignDetail.razor` (link).

---

### Task 1: Entity + DTOs + `RewardReference` + EF + migration

- [ ] **Step 1:** `Reward` entity (`Id`, `CampaignId`, `Name`, `DataJson="{}"`, `CreatedAt`, `UpdatedAt`).
- [ ] **Step 2:** `Shared/Rewards/`:
  - `RewardData`: `int Silver, PactCoins, Fragments, Cristais`; `List<RewardMaterial{Name,Quantity}> Materials`; `List<RewardAsset{Name, Category, Ve, Notes}> StrategicAssets`; `List<string> Knowledge`; `List<string> Items`; `string Notes`; `Guid? EncounterId`; `int? Floor`; `bool IsGranted`.
  - `RewardResponse` (`Id`, `Name`, `RewardData Data`, `string? EncounterName`).
  - `CreateRewardRequest`/`UpdateRewardRequest` (`Name` + `RewardData`).
  - `RewardReference` (static): `Categories` = `["Infraestrutura","Conhecimento","Diplomacia","Artefatos","ControleTerritorial"]` (single source for picker + validation).
- [ ] **Step 3:** EF config (index `CampaignId`) + `DbSet<Reward>`; `dotnet ef migrations add AddRewards` (confirm only the `Rewards` table). Build.
- [ ] **Step 4:** Commit (`feat: add Reward entity, DTOs, migration`).

---

### Task 2: Service + repository + controller + API (+ integration tests)

- [ ] **Step 1:** `ErrorCodes.Reward` (`NotFound`, `NameRequired`, `CategoryInvalid`, `EncounterInvalid`) + API resx (both) + extend the reflection localization guard.
- [ ] **Step 2:** Failing integration tests (`tests/Ruptura.IntegrationTests/Rewards/RewardTests.cs`): CRUD; GM-of-campaign auth (different GM → 404); `Name` required → 400; `Ve=9` clamped to 5 (and `Ve=0` → 1) on read-back; unknown `Category` → 400; an `EncounterId` from another campaign → 400 `Reward.EncounterInvalid`; `IsGranted` round-trips. Run → fail.
- [ ] **Step 3:** `IRewardRepository` (by campaign, by id) + impl; `IRewardService` (`GetForCampaignAsync`/`GetByIdAsync`/`CreateAsync`/`UpdateAsync`/`DeleteAsync`) + impl (auth, structural + Name + Ve-clamp + Category + EncounterId-same-campaign validation; resolve `EncounterName` when linked); `RewardController` (`api/campaigns/{campaignId}/rewards...`), DI.
- [ ] **Step 4:** Run → pass; full sweep. Commit (`feat: add reward planner service + API`).

---

### Task 3: Reward UI (`/gm/campaigns/{id}/rewards`)

- [ ] **Step 1:** `IRewardClientService` + impl (list/get/create/update/delete) per `CampaignClientService` conventions.
- [ ] **Step 2:** `GmRewards.razor` (`@page "/gm/campaigns/{Id:guid}/rewards"`, `[Authorize(Roles="GameMaster")]`): master-detail (reuse UI-B) — searchable package list + editor: resource fields (Silver/PactCoins/Fragments/Cristais), dynamic Materials list, dynamic Strategic Assets list ({Name, Category picker from `RewardReference`, VE 1-5, Notes}), dynamic Knowledge + Items lists, optional encounter picker (over the campaign's encounters — if the encounter client/endpoint from GM-2 exists; otherwise omit the picker and keep `Floor`), `Floor` number, `IsGranted` toggle. Breadcrumbs + toolkit (loading/toast/confirm-delete); i18n both resx.
- [ ] **Step 3:** Link from `GmCampaignDetail.razor` → `/gm/campaigns/@Id/rewards`. Build clean; commit (`feat: add GM rewards page`).

---

## Self-Review

**1. Spec coverage:** entity/model (§3) → Task 1; service/auth/validation (§4) → Task 2; UI (§5) → Task 3; testing (§6) → Task 2. Out-of-scope (crediting, randomness, scaling) absent. ✓
**2. Placeholder scan:** DTO shapes + validation rules concrete; the encounter-picker degradation note is explicit (depends on GM-2). No "TBD"/"handle appropriately".
**3. Type consistency:** `RewardData`/nested + `RewardReference.Categories` referenced identically in service validation (Task 2) and UI pickers (Task 3). `RewardResponse.EncounterName` produced by the service, shown in UI. `ErrorCodes.Reward.*` in service + resx + guard. `EncounterId` cross-campaign check consistent between spec, test, and service.
