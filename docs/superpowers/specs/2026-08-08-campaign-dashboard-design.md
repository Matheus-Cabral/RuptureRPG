# Campaign Dashboard — Design Spec

**Date:** 2026-08-08
**Status:** Approved (design)
**Feature:** GM campaign dashboard — a per-campaign "command center"
**GDD sources:** Manual do Mestre §4.2 (Pressão da Dungeon — numeric 0-100 counter, 4 states), §4.3 (floor states Inexplorado→Explorado→Conquistado→Dominado).

---

## 1. Goal & Scope

A **GM-only** dashboard page at `/gm/campaigns/{id}/dashboard` that gives the Game Master a single command view of a campaign: the in-progress dungeon floor + its Pressão counter (the only new persisted state), plus read-only aggregations of the active party, the guild snapshot, and pending rank-promotion notifications.

**Out of scope:** player-facing view (GM-only); auto-generating Colapso events (the dashboard *displays* the Colapso state + warning; the GM narrates the event); a floor history log; any change to combat/encounter math (the Pressão multiplier is displayed, not applied anywhere automatically).

## 2. Key Decisions (settled in brainstorming, 2026-08-08)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Audience | GM-only (`[Authorize(Roles = "GameMaster")]`; the caller must be the campaign's GM) |
| 2 | New state | Yes — in-progress floor + Pressão are new, GM-editable campaign state |
| 3 | State location | **Fields on `Campaign`** (4 scalars, intrinsic to the campaign — not a separate aggregate like the guild) |
| 4 | Panels | All four: in-progress floor + Pressão, active party, guild snapshot, pending notifications |
| 5 | Placement | New page `/gm/campaigns/{id}/dashboard`, linked from `GmCampaignDetail` (which stays the roster page) |
| 6 | Concurrency | None (single GM per campaign; last-write) |
| 7 | Dungeon mutation | Whole-state `PUT` (client computes new values from quick buttons / advance-floor and PUTs) |

## 3. Data Model

New fields on `Campaign` (Domain entity + migration):
```
Campaign: + CurrentFloor  int    (default 1)
          + FloorName     string (default "")
          + FloorState    string ("Inexplorado" | "Explorado" | "Conquistado" | "Dominado"; default "Inexplorado")
          + Pressure      int    (0..100; default 0)
```
- `FloorState` is a `string` on the wire and column (values are unaccented, so a Domain enum is optional; keep it a validated `string` to avoid a `Shared→Domain` coupling in DTOs — the DTOs live in `Ruptura.Shared`, which must keep zero project references, consistent with the guild feature).
- **Pressão → derived state** (NOT stored): a pure helper `DungeonPressure.StateFor(int pressure) → (string StateKey, decimal PeMultiplier)` per §4.2:

| Range | StateKey | PE multiplier |
|---|---|---|
| 0–24 | `Estavel` | 1.00 |
| 25–59 | `Agravado` | 1.10 |
| 60–89 | `Critico` | 1.25 |
| 90–100 | `Colapso` | 1.50 |

  State keys are unaccented (resx-key suffixes `Dashboard.Pressure.<StateKey>`); the UI localizes them (Estável/Agravado/Crítico/Colapso). Colapso additionally surfaces a UI warning ("dispares an Evento de Colapso" — display only).

## 4. API

- **`GET /api/campaigns/{id}/dashboard`** — `[Authorize(Roles = "GameMaster")]`; caller must be the campaign's GM (else 404, hiding existence). Returns `CampaignDashboardResponse`:
  ```
  CampaignDashboardResponse {
    CampaignName, CampaignId,
    Dungeon: { CurrentFloor, FloorName, FloorState, Pressure, PressureStateKey, PeMultiplier },
    Party: [ { CharacterName, Ranking, Np, CurrentHp, MaxHp } ],      // alive, non-retired sheets
    Guild:  { Stage, Cg, FloorsConquered, Silver, PactCoins } | null, // null if no guild yet
    PendingNotifications: [ { Id, CharacterName, Message } ]           // unread rank-promotion
  }
  ```
  The `CampaignDashboardService` assembles this server-side by reusing the existing services/repos: `CharacterSheetService` (party — alive only, with `DerivedStats.Np`/`MaxHp` + `Combat.CurrentHp`), `GuildSheetService` (guild snapshot), `NotificationService`/repo (unread notifications for the campaign). It does NOT duplicate their logic.
- **`PUT /api/campaigns/{id}/dashboard/dungeon`** — `[Authorize(Roles = "GameMaster")]`; `UpdateDungeonStateRequest { CurrentFloor, FloorName, FloorState, Pressure }`. Server clamps `Pressure` to `[0,100]`, `CurrentFloor` to `>= 1`, validates `FloorState` is one of the four (else 400 `Campaign.FloorStateInvalid`); returns the refreshed `CampaignDashboardResponse` (or the Dungeon sub-object).
  - Quick Pressão buttons (+5 Turno, +10 Combate, +15 Falha Crítica, +N Evento) and "Advance floor" (`CurrentFloor+1`, `Pressure=0`, optionally reset `FloorState`) are computed client-side and sent as a whole-state PUT (GM-only → no concurrency concern).

## 5. UI — `/gm/campaigns/{id}/dashboard`

GM page, four panels, reusing the design-system toolkit (`Breadcrumbs`, `LoadingIndicator`, `ToastService`, `.ledger-table.stack-mobile`, tokens):
1. **Andar & Pressão** — CurrentFloor + FloorName + FloorState `<select>`; a Pressão meter (0-100) showing the derived state (localized) + PE multiplier; quick-adjust buttons + a custom `+N` (Evento) input; "Avançar andar" (resets Pressão); a prominent warning when in Colapso. Each control computes the new dungeon state and PUTs it, then refreshes.
2. **Party ativa** — table of alive/non-retired characters: name, ranking, NP, HP (current/max).
3. **Guilda** — Stage, CG, floors conquered, Silver/PactCoins; link to the guild sheet. ("No guild yet" empty state.)
4. **Notificações** — pending rank promotions with a link to `/gm/notifications` to resolve. (Empty state when none.)

Entry point: a "Dashboard" button on `GmCampaignDetail`. Every visible string via `IStringLocalizer` (pt-BR + en resx). Distinct empty states (`*.Empty`) per panel.

## 6. Reused Patterns / Conventions

- `Ruptura.Shared` DTOs stay Domain-free (`FloorState`/state keys as strings), as with the guild feature.
- The dashboard service is an orchestrator that composes existing services at the service/controller layer (the "controller/service orchestrates, services stay one-directional" convention).
- GM-of-campaign authorization + hide-existence-as-404 mirrors the existing campaign/guild authorization.
- The Pressão-state helper is pure + unit-tested (like `GuildStatsCalculator`/`DungeonPressure`).

## 7. Testing (TDD)

- **Unit**: `DungeonPressure.StateFor` at every boundary (24/25, 59/60, 89/90) → correct state key + multiplier; clamp behavior.
- **Integration** (Testcontainers): `GET dashboard` returns the aggregate for the campaign's GM (party = alive only; guild snapshot present/absent; unread notifications); a non-GM or a different GM → 404. `PUT dungeon` clamps `Pressure` to `[0,100]` and `CurrentFloor` to `>= 1`, rejects an invalid `FloorState` (400), and "advance floor" (CurrentFloor+1, Pressure 0) round-trips.

## 8. Data-Model Impact

| Change | Kind |
|--------|------|
| `Campaign`: + `CurrentFloor`, `FloorName`, `FloorState`, `Pressure` | Modify + migration |
| `Ruptura.Shared.Campaigns`: `CampaignDashboardResponse` (+ sub-DTOs), `UpdateDungeonStateRequest`, `DungeonStateResponse` | New |
| `DungeonPressure` (pure helper) + `ErrorCodes.Campaign.FloorStateInvalid` | New |
| `ICampaignDashboardService` + impl + `CampaignDashboardController` (or extend `CampaignController`) | New |
