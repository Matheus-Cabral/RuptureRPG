# In-Session Combat Tracker (GM-4) — Design Spec

**Date:** 2026-08-09
**Status:** Approved (design)
**Feature:** A persisted, live combat tracker per campaign — initiative order, turn/round advance, combatant HP and conditions, with the campaign Pressão embedded — for running an encounter at the table.
**GDD sources:** §7.2 (Iniciativa = 2d10 + Mod(Controle), descending, tie by Percepção), §7.7 (closed condition list), §7.8 (death), §9.2 (Pressão +5/extra turn).
**Depends on:** GM-1 (creatures for combatant PV), GM-2 (start from an encounter), the campaign party (character PV) and the campaign dashboard Pressão endpoint (reused).

---

## 1. Goal & Scope

The GM runs a fight from the app: build a combatant list (from an encounter + the party + ad-hoc), track initiative order and turns/rounds, mutate HP and conditions live, and adjust the campaign's Pressão in the same view. State is persisted so a combat survives a refresh or a break.

**Out of scope:** automatic damage/attack resolution (the GM enters HP changes — combat resolves at the table); any dice roller (initiative is GM-entered); a player-facing live view (future); per-turn PA/Reação tracking (core depth chosen).

## 2. Key Decisions (settled 2026-08-09)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Persistence | Persisted `CombatSession` per campaign |
| 2 | Combatant sources | Start from a GM-2 encounter (expand creatures×qty) + import the alive party + ad-hoc adds |
| 3 | Pressão | Embedded, reusing the existing dashboard dungeon PUT (no new Pressão backend) |
| 4 | Depth | Core: initiative/turn/round + HP + conditions (no PA/Reação) |
| 5 | Initiative | GM-entered value (no dice roller); tracker sorts descending, tie-break Percepção |
| 6 | Live mutations | Whole-state PUT (single GM, last-write; mirrors the dashboard) |

## 3. Data Model

- **Entity `CombatSession`** (Domain): `Id`, `CampaignId (Guid)`, `Name (string)`, `DataJson (string = "{}")` (a `CombatState`), `IsActive (bool)`, `CreatedAt`, `UpdatedAt`. New `CombatSessions` table + migration; index on `CampaignId`.
- **`CombatState`** (`Ruptura.Shared.Combat`, string-only, zero project refs): `int Round` (default 1), `int CurrentIndex` (default 0), `List<Combatant> Combatants`.
- **`Combatant`**: `Guid Id`, `string Name`, `string Kind` (`Character`|`Creature`|`Adhoc`), `Guid? SourceId` (character-sheet or creature id), `int Initiative`, `int Percepcao`, `int MaxPv`, `int CurrentPv`, `List<string> Conditions` (from the closed §7.7 set), `string Notes`, `bool IsDefeated`.
- **`CombatReference`** (static, Shared): `Conditions` = `["FeridoLeve","FeridoGrave","Sangrando","Atordoado","Enfraquecido","Amedrontado","Imobilizado","Agonizante","Morto"]` (§7.7) — single source for the toggles and validation.
- **`CombatSessionResponse`** = `Id`, `Name`, `IsActive`, `CombatState State` (with `Combatants` already ordered by initiative), plus the campaign's current `Pressure` + derived `PressureStateKey` (from `DungeonPressure`) for the embedded panel.
- Requests: `CreateCombatSessionRequest { Name }`, `UpdateCombatStateRequest { Name, IsActive, CombatState State }`, `StartFromEncounterRequest { Name, Guid EncounterId }`.

## 4. Ordering (`CombatOrder`, pure, Application)

A small pure helper (unit-tested):
- `Order(IEnumerable<Combatant>)` → combatants sorted by `Initiative` desc, tie-break `Percepcao` desc, then stable by name.
- `AdvanceTurn(round, currentIndex, count)` → next index; wrapping past the last combatant increments `round` and resets index to 0. `PreviousTurn` symmetric (index 0 → previous round, last index; floor round at 1).
- Skipping defeated combatants is a UI concern (the GM may still act on them); the helper does not skip.

## 5. Service / API

- `ICombatService` (Application) + impl (Infrastructure), `[Authorize(Roles = "GameMaster")]`, GM-of-campaign only (else `Combat.NotFound`, hide existence):
  - `GetForCampaignAsync(gmId, campaignId)`, `GetByIdAsync`.
  - `CreateAsync(gmId, campaignId, CreateCombatSessionRequest)` (empty state).
  - `StartFromEncounterAsync(gmId, campaignId, StartFromEncounterRequest)` — server builds the initial `CombatState`: expand the encounter's creatures (each `quantity` → that many `Combatant`s, `Kind=Creature`, `Name = "<creature> #n"`, `MaxPv=CurrentPv=<creature Pv>`, `SourceId=creatureId`); import the campaign's alive/non-retired characters (`Kind=Character`, PV from the sheet's `DerivedStats.MaxHp`/`Data.Combat.CurrentHp`, `SourceId=sheetId`); `Initiative` 0 (GM fills in). Resolves creatures via the bestiary service, party via `ICharacterSheetService.GetByCampaignAsync`, the encounter via the encounter service.
  - `UpdateStateAsync(gmId, campaignId, sessionId, UpdateCombatStateRequest)` — whole-state PUT: structural validation (valid JSON, no null combatants), clamp `CurrentPv` to `[0, MaxPv]` (and `MaxPv ≥ 0`), validate every `Condition` against `CombatReference.Conditions` (unknown → 400 `Combat.ConditionInvalid`), clamp `Round ≥ 1` and `CurrentIndex` into range; recompute `IsDefeated` server-side (`CurrentPv <= 0` or has `Morto`). Returns the ordered response.
  - `DeleteAsync`.
  - The embedded Pressão control is NOT a new endpoint — the UI calls the existing `PUT /api/campaigns/{id}/dashboard/dungeon` (dashboard) to change Pressure, then re-reads it.
- `CombatController` (routes `api/campaigns/{campaignId}/combat...`), DI, `ErrorCodes.Combat.*` (`NotFound`, `NameRequired`, `ConditionInvalid`, `EncounterInvalid`) + API resx (both cultures, reflection guard).

## 6. UI

`/gm/campaigns/{id}/combat` (GM-only), linked from `GmCampaignDetail`/dashboard: a sessions list + the tracker:
- **Combatant rows** in initiative order, the current turn highlighted: name, initiative (editable), PV with quick damage/heal inputs, Condition toggles (`CombatReference`), a defeated marker; add/remove combatant; import party; "start from encounter" picker.
- **Turn controls:** Next / Previous turn, Round counter.
- **Pressão panel:** current Pressão + state (color-coded like the dashboard) + quick +N buttons (calling the dashboard dungeon endpoint) + the "+5 per extra turn" reminder.
- Live edits persist via the whole-state PUT (debounced/onchange); toolkit for loading/toast/confirm; all strings i18n both resx.

## 7. Testing

- **Unit (`CombatOrder`):** initiative-desc ordering; Percepção tie-break; `AdvanceTurn` wraps and increments round; `PreviousTurn` floors at round 1.
- **Integration:** CRUD + GM-of-campaign auth (different GM → 404); `StartFromEncounterAsync` expands creatures×quantity + imports the alive party with correct PV; whole-state PUT clamps `CurrentPv` to `[0,MaxPv]` and recomputes `IsDefeated`; unknown Condition → 400; a bad `EncounterId` (other campaign) → 400. Testcontainers.
- No bUnit (UI via build + manual).

## 8. Data-Model Impact

| Change | Kind |
|--------|------|
| `CombatSession` entity + EF config + migration (`CombatSessions`) | New |
| `Ruptura.Shared.Combat`: `CombatState`, `Combatant`, `CombatReference`, `CombatSessionResponse`, requests | New |
| `CombatOrder` (pure, Application) + `ErrorCodes.Combat.*` | New |
| `ICombatService` + impl + repository + `CombatController` + DI | New |
| Web: `/gm/campaigns/{id}/combat` page + client + resx + entry link | New |

No changes to existing entities (reads party PV, creatures, encounters; reuses the dashboard dungeon endpoint for Pressão).
