# Session Prep & Content Organization (GM-5) — Design Spec

**Date:** 2026-08-09
**Status:** Approved (design)
**Feature:** A per-campaign content structure (Arcs → Floors) plus a session log, with light links from floors to encounters/rewards and an optional dashboard "current floor" pointer.
**GDD sources:** §3 (Arco: tema/história/conflito/objetivo final/ecossistema/recursos/mecânica/≥5 andares), §9.1/§4.2 (Andar: identidade/objetivo principal/secundários/condição de fracasso; objective types).
**Depends on:** GM-2 (encounter links), GM-3 (reward links), and the campaign dashboard (optional `CurrentFloorId`).

---

## 1. Goal & Scope

Give the GM a place to prep and organize a campaign's content: narrative Arcs each containing Floors (with their GDD fields), a session log, and light cross-links (a floor can point at encounters/rewards; the dashboard's current floor can optionally point at a defined Floor).

**Out of scope:** a "Room" sub-level (rooms live as notes inside a floor); rewriting the dashboard (only the additive `CurrentFloorId`); a player-facing view; NPCs (they live in GM-1).

## 2. Key Decisions (settled 2026-08-09)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Structure depth | Arc → Floor (2 levels); rooms are floor notes |
| 2 | Session log | Included (date/title/recap/agenda/notes) |
| 3 | Linking | Light: floor → encounters/rewards; optional dashboard `CurrentFloorId` |

## 3. Data Model (per campaign)

- **`Arc`** (Domain): `Id`, `CampaignId (Guid)`, `Name (string)`, `Order (int)`, `DataJson (string = "{}")`, `CreatedAt`, `UpdatedAt`.
  - `ArcData` (`Ruptura.Shared.Content`): `Theme`, `History`, `Conflict`, `FinalObjective`, `Ecosystem`, `Resources`, `Mechanic`, `Notes` (all strings).
- **`Floor`** (Domain): `Id`, `CampaignId (Guid)`, `ArcId (Guid)`, `Number (int)`, `Name (string)`, `DataJson (string = "{}")`, `CreatedAt`, `UpdatedAt`.
  - `FloorData` (`Shared.Content`): `ObjectiveType (string)`, `Identity (string, biome/theme)`, `MainObjective (string)`, `SecondaryObjectives (List<string>)`, `FailureCondition (string)`, `Notes (string)`, `LinkedEncounterIds (List<Guid>)`, `LinkedRewardIds (List<Guid>)`.
- **`SessionLog`** (Domain): `Id`, `CampaignId (Guid)`, `Date (DateTime)`, `Title (string)`, `DataJson (string = "{}")`, `CreatedAt`, `UpdatedAt`.
  - `SessionLogData` (`Shared.Content`): `Recap (string)`, `Agenda (string)`, `Notes (string)`.
- **`ContentReference`** (static, Shared): `ObjectiveTypes` = `["Exploracao","Reconhecimento","Defesa","Ataque","Caca","Escolta","Sobrevivencia","Puzzle","Eliminacao"]` — single source for the picker + validation.
- **Responses:** `ArcResponse { Id, Name, Order, ArcData Data }`; `FloorResponse { Id, ArcId, Number, Name, FloorData Data, List<LinkRef> Encounters, List<LinkRef> Rewards }` (`LinkRef { Id, Name }` resolved names); `SessionLogResponse { Id, Date, Title, SessionLogData Data }`.
- **Requests:** create/update for each (Arc: Name/Order/ArcData; Floor: ArcId/Number/Name/FloorData; SessionLog: Date/Title/SessionLogData).
- New tables `Arcs`, `Floors`, `SessionLogs` + one migration; indices on `CampaignId` (+ `Floors.ArcId`).
- **Dashboard integration:** add `CurrentFloorId (Guid?)` to `Campaign` (additive; part of this migration). The existing dashboard `UpdateDungeonStateRequest`/`DungeonStateDto` gain an optional `CurrentFloorId`; when set and resolvable, the dashboard shows that floor's Name/ObjectiveType/MainObjective. The free-text `FloorName` still works unchanged.

## 4. Services / API

- `ICampaignContentService` (Arcs + Floors) + `ISessionLogService` (Application) + impls, `[Authorize(Roles = "GameMaster")]`, GM-of-campaign only (else NotFound, hide existence):
  - Arcs: `GetForCampaignAsync`, `GetByIdAsync`, `Create/Update/Delete`. Deleting an arc with floors → cascade-delete its floors (or block with a clear error — implementer chooses; document it).
  - Floors: `GetForArcAsync`/`GetForCampaignAsync`, `Create/Update/Delete`; on write, validate `ArcId` belongs to the same campaign; `ObjectiveType` ∈ `ContentReference.ObjectiveTypes` (400 `Content.ObjectiveTypeInvalid`); every `LinkedEncounterId`/`LinkedRewardId` belongs to the same campaign (400 `Content.LinkInvalid`); resolve link names in the response.
  - Session logs: `GetForCampaignAsync` (date desc), `GetByIdAsync`, `Create/Update/Delete`; `Title` required.
  - Structural blob validation (valid JSON, no null list elements) throughout.
- Controllers (`api/campaigns/{campaignId}/arcs...`, `.../floors...`, `.../sessions...`), DI, `ErrorCodes.Content.*` + `ErrorCodes.Session.*` + API resx (both cultures, reflection guard).
- **Dashboard service change:** `CampaignDashboardService.UpdateDungeonAsync` persists the optional `CurrentFloorId` (validate it's a floor of this campaign, else 400 or ignore→null — document); `GetAsync`/`BuildAsync` include the resolved current-floor summary in `DungeonStateDto`.

## 5. UI

- `/gm/campaigns/{id}/content` (GM-only): an Arc → Floor tree — arcs list (ordered, add/reorder) each expandable to its floors; selecting a floor opens a structured editor (ObjectiveType picker, Identity/biome, MainObjective, dynamic SecondaryObjectives list, FailureCondition, Notes, and encounter/reward multi-select pickers over the campaign's encounters/rewards). Master-detail (reuse UI-B).
- `/gm/campaigns/{id}/sessions` (GM-only): session log — list (date desc) + editor (Date, Title, Recap, Agenda, Notes).
- Dashboard: the "Andar & Pressão" panel gains an optional "linked floor" picker (over the campaign's floors); when set, it shows the floor's objective. `FloorName` free text remains.
- Linked from `GmCampaignDetail`/dashboard; toolkit (loading/toast/confirm-delete); all strings i18n both resx.

## 6. Testing

- **Integration:** CRUD for arcs/floors/sessions; GM-of-campaign auth (different GM → 404); floor `ArcId` must be same-campaign; `ObjectiveType` validation (400); a `LinkedEncounterId`/`LinkedRewardId` from another campaign → 400; arc delete cascades floors (or blocks — assert the chosen behavior); session `Title` required; dashboard `CurrentFloorId` round-trips and rejects a foreign floor. Testcontainers.
- No unit calculators. No bUnit (UI via build + manual).

## 7. Data-Model Impact

| Change | Kind |
|--------|------|
| `Arc`, `Floor`, `SessionLog` entities + EF configs + migration (`Arcs`,`Floors`,`SessionLogs`) | New |
| `Campaign` + `CurrentFloorId (Guid?)` (same migration) | Modify + migration |
| `Ruptura.Shared.Content`: ArcData/FloorData/SessionLogData + responses + requests + `ContentReference`; `DungeonStateDto`/`UpdateDungeonStateRequest` + `CurrentFloorId` | New / Modify (Shared) |
| `ICampaignContentService` + `ISessionLogService` + impls + repositories + controllers + DI + `ErrorCodes.Content.*`/`.Session.*` | New |
| `CampaignDashboardService` — persist/resolve `CurrentFloorId` | Modify |
| Web: `/gm/campaigns/{id}/content`, `/sessions` pages + clients + resx + entry links; dashboard linked-floor picker | New / Modify |
