# Session Prep & Content Organization (GM-5) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Per-campaign Arcs → Floors content structure + a session log, with floor→encounter/reward links and an optional dashboard `CurrentFloorId`.

**Architecture:** Three entities (`Arc`, `Floor`, `SessionLog`) per campaign with `DataJson` blobs, GM-only CRUD services (validation only), Blazor editors reusing UI-B master-detail, plus a small additive `Campaign.CurrentFloorId` surfaced on the dashboard. **Depends on GM-2 (encounter links), GM-3 (reward links), and the dashboard — execute after those.**

**Tech Stack:** .NET 8, EF Core 8 (+ migration), Blazor WASM 8, xUnit + Testcontainers.

**Spec:** `docs/superpowers/specs/2026-08-09-prep-content-org-design.md`. GDD §3/§9.1/§4.2.

## Global Constraints

- **GM-of-campaign auth** — every endpoint `[Authorize(Roles = "GameMaster")]`; caller owns the campaign else 404 (mirror `CampaignDashboardService`).
- **Validation** — structural blob (valid JSON, no null list elements); `Floor.ArcId` same-campaign; `ObjectiveType` ∈ `ContentReference.ObjectiveTypes` (400 `Content.ObjectiveTypeInvalid`); every `LinkedEncounterId`/`LinkedRewardId` same-campaign (400 `Content.LinkInvalid`); `SessionLog.Title` required; dashboard `CurrentFloorId` must be a floor of the campaign (else reject/ignore→null — document).
- **`Ruptura.Shared` zero project refs**; enum-like values strings via `ContentReference`.
- **Additive dashboard change only** — add `Campaign.CurrentFloorId (Guid?)`; extend `DungeonStateDto`/`UpdateDungeonStateRequest`; do NOT alter existing dungeon behavior (free-text `FloorName` still works).
- **Reuse:** encounters via GM-2 service/repo, rewards via GM-3 service/repo (own-campaign), for link resolution + validation.
- **i18n** both resx (Web + API); `ErrorCodes.Content.*`/`ErrorCodes.Session.*` guarded by the reflection localization test.
- **Commit per task**; Testcontainers Serilog flake → re-run once.

## File Structure

**Create:** `Domain/Entities/Arc.cs`, `Floor.cs`, `SessionLog.cs`; `Shared/Content/*` (ArcData/FloorData/SessionLogData + responses + requests + `ContentReference` + `LinkRef`); `Interfaces/ICampaignContentService.cs`, `ISessionLogService.cs`, repositories; `Infrastructure/Services/CampaignContentService.cs`, `SessionLogService.cs`, repositories, EF configs; `API/Controllers/CampaignContentController.cs`, `SessionLogController.cs`; `Web/Pages/GmCampaignContent.razor`, `GmSessions.razor` + client services; unit-free (integration only) tests.
**Modify:** `AppDbContext`, `Campaign.cs` (+CurrentFloorId), `Shared/Campaigns/CampaignDashboardResponse.cs` (`DungeonStateDto`) + `UpdateDungeonStateRequest.cs`, `CampaignDashboardService.cs`, `ErrorCodes.cs`, DI, resx, `GmCampaignDetail.razor` + `GmCampaignDashboard.razor`.

---

### Task 1: Entities + DTOs + `ContentReference` + `Campaign.CurrentFloorId` + EF + migration

- [ ] **Step 1:** `Arc` (`Id`,`CampaignId`,`Name`,`Order`,`DataJson`,ts), `Floor` (`Id`,`CampaignId`,`ArcId`,`Number`,`Name`,`DataJson`,ts), `SessionLog` (`Id`,`CampaignId`,`Date`,`Title`,`DataJson`,ts) entities; add `Campaign.CurrentFloorId (Guid?)`.
- [ ] **Step 2:** `Shared/Content/`: `ArcData{Theme,History,Conflict,FinalObjective,Ecosystem,Resources,Mechanic,Notes}`; `FloorData{ObjectiveType,Identity,MainObjective,List<string> SecondaryObjectives,FailureCondition,Notes,List<Guid> LinkedEncounterIds,List<Guid> LinkedRewardIds}`; `SessionLogData{Recap,Agenda,Notes}`; responses (`ArcResponse`,`FloorResponse` with `List<LinkRef> Encounters/Rewards`, `SessionLogResponse`, `LinkRef{Id,Name}`); create/update requests; `ContentReference.ObjectiveTypes = ["Exploracao","Reconhecimento","Defesa","Ataque","Caca","Escolta","Sobrevivencia","Puzzle","Eliminacao"]`.
- [ ] **Step 3:** Extend `Ruptura.Shared/Campaigns/CampaignDashboardResponse.cs` `DungeonStateDto` with `Guid? CurrentFloorId`, `string? CurrentFloorName`, `string? CurrentFloorObjective`; and `UpdateDungeonStateRequest` with `Guid? CurrentFloorId`.
- [ ] **Step 4:** EF configs (indices `CampaignId`, `Floors.ArcId`) + `DbSet`s; `dotnet ef migrations add AddCampaignContent` — confirm it creates `Arcs`/`Floors`/`SessionLogs` and adds `Campaign.CurrentFloorId` (nullable) and nothing else. Build.
- [ ] **Step 5:** Commit (`feat: add Arc/Floor/SessionLog entities + Campaign.CurrentFloorId + migration`).

---

### Task 2: Content service (Arcs + Floors) + API (+ integration tests)

- [ ] **Step 1:** `ErrorCodes.Content` (`NotFound`,`NameRequired`,`ObjectiveTypeInvalid`,`ArcInvalid`,`LinkInvalid`) + API resx (both) + extend the reflection guard.
- [ ] **Step 2:** Failing integration tests (`tests/Ruptura.IntegrationTests/Content/CampaignContentTests.cs`): CRUD arcs + floors; GM-of-campaign auth (different GM → 404); floor `ArcId` from another campaign → 400 `Content.ArcInvalid`; unknown `ObjectiveType` → 400; a `LinkedEncounterId`/`LinkedRewardId` from another campaign → 400 `Content.LinkInvalid`; floor response resolves link names; deleting an arc cascades its floors (assert). Run → fail.
- [ ] **Step 3:** `IArcRepository`/`IFloorRepository` (+impls); `ICampaignContentService` (arcs: GetForCampaign/GetById/Create/Update/Delete[cascade floors]; floors: GetForArc/GetForCampaign/GetById/Create/Update/Delete) + impl (auth, validation, link resolution via GM-2/GM-3 repos); `CampaignContentController` (`api/campaigns/{campaignId}/arcs...` + `.../floors...`), DI.
- [ ] **Step 4:** Run → pass; full sweep. Commit (`feat: add campaign content (arcs/floors) service + API`).

---

### Task 3: Session log service + API (+ integration tests)

- [ ] **Step 1:** `ErrorCodes.Session` (`NotFound`,`TitleRequired`) + API resx (both) + guard.
- [ ] **Step 2:** Failing integration tests (`SessionLogTests.cs`): CRUD (list date-desc); GM-of-campaign auth; `Title` required → 400. Run → fail.
- [ ] **Step 3:** `ISessionLogRepository` + `ISessionLogService` + impls; `SessionLogController` (`api/campaigns/{campaignId}/sessions...`), DI.
- [ ] **Step 4:** Run → pass; full sweep. Commit (`feat: add session log service + API`).

---

### Task 4: Dashboard `CurrentFloorId` integration

- [ ] **Step 1:** Failing integration test (extend `CampaignDashboardTests`): `PUT dungeon` with a `CurrentFloorId` of a campaign floor → GET shows `CurrentFloorName`/`CurrentFloorObjective`; a foreign floor id → 400 (or ignored→null — assert the chosen behavior). Run → fail.
- [ ] **Step 2:** In `CampaignDashboardService`: `UpdateDungeonAsync` validates + persists `campaign.CurrentFloorId`; `BuildAsync` resolves the floor (via the floor repo/service, same campaign) into `DungeonStateDto.CurrentFloorName`/`CurrentFloorObjective`.
- [ ] **Step 3:** Run → pass; full sweep. Commit (`feat: wire optional current-floor pointer into the dashboard`).

---

### Task 5: Content + Sessions UI (+ dashboard picker)

- [ ] **Step 1:** `ICampaignContentClientService` + `ISessionLogClientService` (+impls) per `CampaignClientService` conventions.
- [ ] **Step 2:** `GmCampaignContent.razor` (`@page "/gm/campaigns/{Id:guid}/content"`, GM-only): Arc→Floor tree — arcs list (ordered, add) each expandable to floors; floor editor (ObjectiveType picker from `ContentReference`, Identity, MainObjective, dynamic SecondaryObjectives list, FailureCondition, Notes, encounter/reward multi-select over the campaign's encounters/rewards). Master-detail (reuse UI-B).
- [ ] **Step 3:** `GmSessions.razor` (`@page "/gm/campaigns/{Id:guid}/sessions"`, GM-only): session list (date desc) + editor (Date/Title/Recap/Agenda/Notes).
- [ ] **Step 4:** Dashboard (`GmCampaignDashboard.razor`): add an optional linked-floor picker (over the campaign's floors) that sets `CurrentFloorId` on the dungeon PUT; show the linked floor's objective when set. `FloorName` free text stays.
- [ ] **Step 5:** Link both pages from `GmCampaignDetail.razor`. Build clean; i18n both resx. Commit (`feat: add GM content + sessions pages and dashboard floor picker`).

---

## Self-Review

**1. Spec coverage:** entities/model + CurrentFloorId (§3) → Task 1; content service/validation/links (§4) → Task 2; session log (§4) → Task 3; dashboard integration (§3/§4) → Task 4; UI (§5) → Task 5; testing (§6) → Tasks 2-4. Out-of-scope (rooms, player view, NPCs, dashboard rewrite) absent. ✓
**2. Placeholder scan:** entity/DTO shapes + validation rules concrete; the two "document the chosen behavior" notes (arc-delete cascade, foreign CurrentFloorId reject-vs-ignore) are explicit decisions the implementer records + tests, not vague. UI pattern-directive with the full field set. No "TBD"/"handle appropriately".
**3. Type consistency:** `ArcData`/`FloorData`/`SessionLogData` + `ContentReference.ObjectiveTypes` + `LinkRef` referenced identically across services (Tasks 2-3), dashboard (Task 4), and UI (Task 5). `DungeonStateDto.CurrentFloorId/CurrentFloorName/CurrentFloorObjective` + `UpdateDungeonStateRequest.CurrentFloorId` consistent between Task 1 (definition), Task 4 (service), and Task 5 (UI). `ErrorCodes.Content.*`/`.Session.*` in services + resx + guard. Depends on GM-2/GM-3 repos for link validation/resolution.
