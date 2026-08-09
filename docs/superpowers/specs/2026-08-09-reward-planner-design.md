# Reward Planner (GM-3) — Design Spec

**Date:** 2026-08-09
**Status:** Approved (design)
**Feature:** A GM tool to compose and organize reusable reward packages (Resources, Strategic Assets with VE 1-5, Knowledge, items) per campaign — planning only.
**GDD sources:** §9.4 (Recompensas: Conhecimento/Recursos/Progresso; Informação é recurso), §9.10 / Manual §7 (Ativos Estratégicos — categories + VE 1-5), §10.6 (Prata / Moedas de Pacto).
**Depends on:** GM-2 (optional link to an Encounter); reads nothing else.

---

## 1. Goal & Scope

The GM builds reward packages and reuses them, optionally tagging one to an encounter or floor. No randomness (the user rules out dice), no formula-based magnitude suggestion, and no automatic crediting to the guild/characters — it is a structured planner/checklist.

**Out of scope:** applying/crediting rewards to the guild `Resources` or a character (planning-only); random loot tables/rolls; difficulty-scaled magnitude suggestions.

## 2. Key Decisions (settled 2026-08-09)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Nature | Manual reward planner/builder (no randomness, no auto-scaling) |
| 2 | Linkage | Reusable `Reward` packages per campaign; optional `EncounterId`/`Floor` tag |
| 3 | Application | Planning only (an `IsGranted` checklist flag); no system crediting |

## 3. Data Model

- **Entity `Reward`** (Domain): `Id`, `CampaignId (Guid)`, `Name (string)`, `DataJson (string = "{}")`, `CreatedAt`, `UpdatedAt`. New `Rewards` table + migration; index on `CampaignId`.
- **`RewardData`** (`Ruptura.Shared.Rewards`, string-only, zero project refs):
  - Resources: `Silver` (int), `PactCoins` (int), `Fragments` (int), `Cristais` (int), `Materials` (`List<RewardMaterial{ Name, Quantity }>`).
  - `StrategicAssets` (`List<RewardAsset{ Name (string), Category (string: Infraestrutura|Conhecimento|Diplomacia|Artefatos|ControleTerritorial), Ve (int 1-5), Notes (string) }>`).
  - `Knowledge` (`List<string>`), `Items` (`List<string>`).
  - `Notes` (string); `EncounterId` (`Guid?`), `Floor` (`int?`); `IsGranted` (bool).
  - `RewardReference` (static, Shared): the AE `Categories` list — single source for the picker and validation.
- **`RewardResponse`** = `Id`, `Name`, `RewardData Data`, plus a resolved `EncounterName` (string?, when `EncounterId` set and resolvable).
- Requests: `CreateRewardRequest { Name, RewardData }`, `UpdateRewardRequest { Name, RewardData }`.

## 4. Service / API

- `IRewardService` (Application) + impl (Infrastructure), `[Authorize(Roles = "GameMaster")]`, GM-of-campaign only (else `Reward.NotFound`, hide existence — mirror `CampaignDashboardService`):
  - `GetForCampaignAsync(gmId, campaignId)`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`, and a small `SetGrantedAsync(gmId, campaignId, id, bool)` (or just via update).
  - Validation: structural blob (valid JSON, no null list elements); `Name` required; each `RewardAsset.Ve` clamped to `[1,5]`; resource ints and material quantities clamped ≥0; `Category` validated against `RewardReference.Categories` (400 on unknown); if `EncounterId` is set, verify it belongs to the same campaign (else 400 `Reward.EncounterInvalid`).
- `RewardController` (routes `api/campaigns/{campaignId}/rewards...`), DI, `ErrorCodes.Reward.*` (`NotFound`, `NameRequired`, `CategoryInvalid`, `EncounterInvalid`) + API resx (both cultures, reflection localization guard).

## 5. UI

`/gm/campaigns/{id}/rewards` (GM-only), linked from `GmCampaignDetail`/dashboard. Master-detail (reuse the UI-B `.master-detail`/`.detail-panel` pattern): a searchable list of reward packages + a structured editor:
- Resource fields (Silver / Pact Coins / Fragments / Cristais), a dynamic Materials list ({Name, Quantity}).
- A dynamic Strategic Assets list ({Name, Category picker, VE 1-5, Notes}).
- Dynamic Knowledge and Items lists (strings).
- Optional encounter picker (over the campaign's encounters, GM-2) and Floor number.
- An `IsGranted` toggle (session checklist).
Toolkit (loading/toast/confirm-delete); all strings i18n (both resx).

## 6. Testing

- **Integration:** CRUD; GM-of-campaign auth (a different GM → 404); `Name` required (400); `Ve` clamped to [1,5]; unknown `Category` → 400; an `EncounterId` from another campaign → 400 `Reward.EncounterInvalid`; `IsGranted` round-trips. Testcontainers.
- No unit calculator (nothing computed). No bUnit (UI via build + manual).

## 7. Data-Model Impact

| Change | Kind |
|--------|------|
| `Reward` entity + EF config + migration (`Rewards`) | New |
| `Ruptura.Shared.Rewards`: `RewardData`(+nested), `RewardResponse`, requests, `RewardReference` | New |
| `IRewardService` + impl + repository + `RewardController` + DI + `ErrorCodes.Reward.*` | New |
| Web: `/gm/campaigns/{id}/rewards` page + client + resx + entry link | New |

No changes to existing entities.
