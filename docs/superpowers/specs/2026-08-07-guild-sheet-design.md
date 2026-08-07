# Guild Sheet — Design Spec

**Date:** 2026-08-07
**Status:** Approved (design), pending implementation plan
**Feature:** Full Guild Sheet (Ficha da Guilda) for RuptureRPG
**GDD sources:** §10 (A Guilda), §10.2 (Ficha da Guilda, 14 sections), §10.3 (Quartel-General / árvore tecnológica), §10.4 (Trabalhadores e Mercenários), §10.6 (Economia), §10.7 (Doutrinas), §10.8 (CG), §10.9 (CI/CF/CS — FECHADO). Mirror sections in `docs/manuais/Manual_do_Mestre.md` §8, §8.1.1, §8.2, §8.4, §8.5.

---

## 1. Goal & Scope

Deliver the **complete** guild sheet in a single spec (user's explicit choice over a phased decomposition), covering all 14 §10.2 sections plus the interlocking FECHADO subsystems: construction tech-tree, worker/mercenary roster, economy (maintenance/income/inflation), doctrines, the CG formula, and the CI/CF/CS derived capacities. It also delivers an **Interlude Calculator** — a preview-and-apply engine that projects N days of guild-level interlude forward and lets the user apply each computed indicator individually.

The spec is written in clearly separated modules so it can be **implemented incrementally** even though it ships as one design (see §12 implementation ordering).

**Out of scope (this project):**
- Character-level interlude (Treinamento §6.4, Provações) — stays on the character sheet; the Interlude Calculator does **not** reach into character sheets. Future integration.
- A campaign calendar / automatic time simulation. Time advances only when a user runs the Interlude Calculator for N days and applies results.
- Faction mechanics beyond recording reputation on the sheet (the §13 encounter effects live in GM adjudication, not here).

---

## 2. Key Decisions (settled during brainstorming)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Scope depth | Full guild sheet in one spec |
| 2 | Guild ↔ Campaign | **1:1**. `CampaignId` FK + unique index. Membership = campaign roster (`CampaignMembership`). |
| 3 | `GuildMembership` | **Removed** (entity + table). Superseded by roster-derived membership. |
| 4 | `GuildName` | Stays a column on `GuildSheet`; rest of Identity in the blob. |
| 5 | Reference data (installations, doctrines) | New `CatalogEntry` types `Installation` (20 seeded) + `Doctrine` (8 seeded). Reuse catalog picker/archive/homebrew/name-collision infra. |
| 6 | Worker/mercenary types | Static reference list (fixed types + GDD default salaries), overridable per roster row. Not catalog rows. |
| 7 | Derived values | Calculated on read via a pure `GuildStatsCalculator`. Never persisted. |
| 8 | Permissions | **Shared write** — GM of the campaign OR any campaign member has full read/write. |
| 9 | Economy / time | Interlude Calculator: input N days → projected indicators, each with an individual **Apply** that persists immediately. No auto-tick. Scope = guild subsystems only. |
| 10 | Research/crafting | **Full workflow** (Descobrir → Pesquisar → Dominar → Aplicar) with in-progress projects, progress, researchers, crafting queue. |
| 11 | Apply model | Each indicator's **Apply** persists that mutation immediately; server recomputes the delta (never trusts a client-supplied number). |
| 12 | Persistence | **Hybrid**: `DataJson` blob for stable modules + dedicated child entities for high-churn, addressable lists. |

---

## 3. Persistence & Data Model

### 3.1 `GuildSheet` (modified entity)

```
GuildSheet
  Id: Guid
  CampaignId: Guid            // NEW — real FK to Campaign, ON DELETE CASCADE, UNIQUE index (enforces 1:1)
  GuildName: string
  CreatedByGameMasterId: Guid
  CreatedAt, UpdatedAt: DateTime
  DataJson: string            // stable-modules blob (GuildSheetData)
  // Optimistic concurrency for the blob under shared write via Postgres xmin
  // (UseXminAsConcurrencyToken — no CLR property/column; bytea RowVersion is inert on PG).
```

- **Remove** the `Memberships` navigation and the `GuildMembership` entity + table (migration drops it).
- `CampaignId` unique index enforces exactly one guild per campaign. First access to a campaign's guild **creates it on demand** (`GuildName` seeded from the campaign name, editable after).

> **Note on the FK:** unlike `CharacterSheet.CampaignId` (bare Guid, soft reference), `GuildSheet.CampaignId` gets a **real FK + unique index** because the 1:1 invariant must be enforced at the DB level. This is consistent with `Notification`'s precedent of real FKs where cascade/uniqueness actually matters, while `RecipientUserId`-style soft references stay soft.

### 3.2 Child entities (high-churn, addressable)

Each has `Id`, `GuildSheetId` (FK, `ON DELETE CASCADE`), and is edited row-by-row (concurrency-safe; each Interlude "Apply" = one row update).

```
GuildBuilding
  CatalogEntryId: Guid   // Installation catalog ref
  Level: int
  IsActive: bool         // CS caps active buildings; inactive ones give no benefit (§10.9)

GuildStaff
  Kind: enum { Worker, Mercenary }
  TypeOrRanking: string  // worker type (Operário…) or merc ranking (Bronze…)
  Name: string
  DailySalary: int       // pre-filled from GDD default, overridable
  IsActive: bool
  Efficiency: int?       // workers only, optional
  Morale: int?           // workers only, optional

ResearchProject
  Name: string
  ResearchType: string   // Arcana | Biológica | Tecnológica | Dimensional | Histórica | Militar
  Complexity: enum { Menor, Moderada, Maior, Suprema }
  Stage: enum { Descobrir, Pesquisar, Dominar, Aplicar }
  ProgressDays: int
  RequiredDays: int      // from Complexity tier (§11.2): 5/10/20/40
  Researchers: int       // count of assigned researchers (splits time, floor 50%)
  Points: int            // awarded to CG's Pesquisa term on completion
  IsComplete: bool

CraftingOrder
  Category: enum { Forja, Alquimia, Encantamento, Engenharia, Artefatos }
  ItemName: string
  Quality: string   // "Comum|Superior|Raro|Épico|Lendário|Divino" — plain string (accented values aren't valid C# enum identifiers)
  ProgressDays: int
  RequiredDays: int
  Status: enum { EmAndamento, Concluido, Cancelado }   // unaccented identifier; UI localizes display

Expedition
  Kind: enum { Principal, Secundaria }   // unaccented identifier; UI localizes display
  Date: DateTime
  Participants: string
  Objective: string
  Result: string
  Losses: string
  ResourcesGained: string
```

### 3.3 `CatalogEntry` reference data (new types)

Extend `CatalogEntryType` with `Installation` and `Doctrine`. Seed via a new `CatalogSeedData.Installations.cs` / `CatalogSeedData.Doctrines.cs` partial, matching the existing `Entry(id, type, name, new { … })` pattern.

**Installations (20, §10.3.1)** — `DataJson`:
```
{ Category: "Fundação|Produção|Especialização|Institucional|Monumental",
  Weight: 1|2|3|5|8,
  LevelCap: int,            // e.g. Biblioteca VII, Dormitório V, Câmara do Conselho II
  Prerequisites: string,    // e.g. "Armazém I", "Biblioteca III"
  Unlocks: string }
```
(Portão is the fixed core — modeled as a level-fixed, non-constructible installation, or omitted from the constructible list. See §11 open items.)

**Doctrines (8, §10.7)** — `DataJson`:
```
{ Bonus: string }   // free-text effect description; mechanical modifiers the calculator
                    // actually applies are keyed by doctrine identity, not parsed from text
```

> Doctrine mechanical effects the calculator applies (Logística +20% CS / −10% maintenance; Comercial −1 inflation stage on guild purchases; etc.) are implemented as **coded rules keyed to the seeded doctrine ids**, not parsed from the `Bonus` text. The text is display flavor; the seeded ids are the contract.

### 3.4 `GuildSheetData` blob (Ruptura.Shared)

Stable, low-churn modules only. Deserialization guarantees every member non-null at the boundary (same defensive pattern as `CharacterSheetService.DeserializeSheetData`).

```
GuildSheetData
  Identity {
    EmblemImagePath: string      // uploaded via existing IFileStorageService media flow
    PatronDeity: string
    MainDoctrineId: Guid?        // Doctrine catalog ref
    FoundingDate: DateTime?
    GuildRanking: string         // 8 GDD rank names (same value set as character Ranking)
  }
  Prestige { Value: int; Notes: string }
  Influence: List<InfluenceRelation {
    Name: string; Kind: "Cidade|Facção|Guilda|Divindade"; Reputation: int (-100..100); Notes: string
  }>
  Resources {
    Silver: int; PactCoins: int;
    Materials: List<{ Name: string; Quantity: int }>;
    DimensionalFragments: int;
    Artifacts: List<string>;
    StrategicReserveNotes: string
  }
  ActiveDoctrineIds: List<Guid>   // ≤ doctrine limit (validated: 2 + Câmara do Conselho level, max 4)
  Knowledge {
    Maps: List<string>; Recipes: List<string>; CataloguedEnemies: List<string>;
    DefeatedBosses: List<string>; HistoricalRecords: List<string>
  }
  Legado: List<LegacyFeat { Title: string; Description: string; PermanentBenefit: string }>
  FloorsConquered: int            // drives Guild Stage (§10.8 milestones every 5 floors)
```

---

## 4. GuildStatsCalculator (pure, calculated on read)

Application layer, pure & stateless, mirrors `CharacterStatsCalculator`. Never persists. Input: `GuildSheet` blob + buildings + active staff + completed research + resources + active doctrines. Output `GuildDerivedStats`:

| Value | Formula / source |
|-------|------------------|
| **Guild Stage** | From `FloorsConquered`: 0→Fundação, 5→Menor, 10→Regional, 15→Reconhecida, 20→Maior, 25→Renomada, 30→Lendária, 35+→Divina |
| **CG** | `Infra + Pesquisa + Logística + Recursos` (§10.8). Infra = Σ(building level × category weight); Pesquisa = Σ completed-project Points; Logística = CS + qualified-workers×2; Recursos = PactCoins + converted strategic materials |
| **CS** | `5 + (Centro Logístico × 2) + (Armazém × 1)`; ×1.20 if Logística doctrine active |
| **CI** | `3 + (Câmara do Conselho × 4) + (Centro Logístico × 1)` |
| **CF** | `10 + (Memorial × 3) + (Biblioteca × 1) + (Campo de Treinamento × 1)` |
| **Inflation Index** | By stage: ×1.0/1.2/1.5/1.8/2.2/2.6/3.2/4.0 (§10.6.4); Comercial doctrine −1 stage for guild purchases |
| **Daily Maintenance** | `Σ(building level × weight × 1 Prata) + Σ(active staff salaries)`; Logística doctrine −10% |
| **Income rates** | Worker production (~2 Prata/day per Operário), secondary-expedition yield (`merc NP × 0.5` per success), commerce (Comercial +10% sales) |
| **Caps** | Storage = Armazém level × 50; worker/merc/simultaneous-expedition limits from relevant installations |
| **Doctrine limit** | `2 + Câmara do Conselho level`, max 4 |
| **Active-building overflow** | Count(active buildings) vs CS — surface a violation flag when over cap |

The official per-stage tables (§10.8 CG table; §10.9 CS/CI/CF table) are the **exact test oracles** — the calculator must reproduce them for a canonical build at each stage.

---

## 5. Interlude Calculator

### 5.1 Projection (read-only)

`IInterludeCalculator` (Application, pure). Input: guild snapshot + `days: int` → `InterludeProjection`:

```
InterludeProjection { Days: int; Indicators: List<InterludeIndicator> }
InterludeIndicator {
  Kind: enum { Maintenance, Income, ResearchProgress, CraftingProgress, SecondaryExpedition }
  Label: string
  Description: string          // human-readable ("Manutenção de 30 dias: -450 Prata")
  ProposedDelta: <typed>       // e.g. { Silver: -450 } or { ProjectId, DaysAdded, Completed, PointsAwarded }
  TargetId: Guid?              // the child row this indicator applies to (project/order/expedition)
}
```

Endpoint: `GET /api/guilds/{id}/interlude/preview?days=N` — computes and returns the projection. **Mutates nothing.**

### 5.2 Apply (per indicator, server-recomputed)

Endpoint: `POST /api/guilds/{id}/interlude/apply` with descriptor `{ kind, targetId?, days }`.
The server **recomputes** the delta for that specific indicator from current stored state and applies it to the addressable row (or blob field), then returns the refreshed projection.

> **Security invariant** (per the media-vulnerability lesson in project memory): the client sends a *selector* (which indicator, which target, how many days), never an authoritative delta. The server is the sole source of the number applied. This prevents a client from posting an arbitrary treasury/point delta.

Apply semantics by kind:
- **Maintenance** → subtract recomputed maintenance×days from `Resources.Silver` (floor at 0; unpaid → GM handles Negligência narratively, no hard block per §8.4).
- **Income** → add recomputed income×days to `Resources.Silver`.
- **ResearchProgress** (per project) → add days (÷ researcher split, floor 50% of base) to `ProgressDays`; on reaching `RequiredDays`, advance `Stage` / set `IsComplete` + award `Points` (which then feed CG).
- **CraftingProgress** (per order) → advance `ProgressDays`; complete when done.
- **SecondaryExpedition** → append an `Expedition` (Secundária) with computed yield/report.

---

## 6. API & Permissions

New `GuildController` (`/api/guilds` + `/api/campaigns/{campaignId}/guild`):
- `GET /api/campaigns/{campaignId}/guild` — get (create-on-first-access) the campaign's guild sheet with derived stats.
- CRUD for the blob modules and each child collection (buildings, staff, research, crafting, expeditions, influence relations, etc.).
- `GET .../interlude/preview?days=N` and `POST .../interlude/apply`.

**Authorization — shared write.** New `GuildSheetService.AuthorizeGuildAccessAsync(guildId, userId)` returns success if the user is the campaign's GM **or** a member of the campaign (`CampaignMembership`). Follows `CharacterSheetService.AuthorizeAccessAsync` shape. Read and write share the same check (shared write, no field-level gating).

Requires adding `ICampaignMembershipRepository.GetByPlayerAsync(playerId)` (additive; flagged missing in project memory).

**Concurrency:** child-entity writes are per-row (inherently safe). Blob writes use Postgres's `xmin` system column as the concurrency token, mapped to a **round-trippable CLR property** `GuildSheet.Version` (`uint`, `HasColumnName("xmin")`, `ValueGeneratedOnAddOrUpdate().IsConcurrencyToken()`) — **not** the shadow-only `UseXminAsConcurrencyToken()`, whose token can't leave the server and so only guards the in-request load-modify-save window. To actually protect the shared-write case the spec cares about (user A loads, user B saves, user A saves stale), sub-plan #3 **must** surface `Version` in the read DTO and require it on write, and the §8 conflict test must exercise that **cross-request** stale-write path, not just an in-request race. *(A `bytea RowVersion` via `.IsRowVersion()` is inert on PostgreSQL; the shadow xmin token is real but too narrow — both were rejected during sub-plan #1. Decided Task 2 + final review.)*

---

## 7. UI (Blazor WASM)

Guild sheet page under the campaign, tabbed like the character sheet, reachable from `GmCampaignDetail` (GM) and the player's campaign view. Tabs:

1. **Identidade** — name, emblem upload, patron deity, main doctrine, founding date, ranking.
2. **Prestígio & Influência** — prestige value/notes; influence relations table (reputation −100..100).
3. **Recursos** — Prata, Moedas de Pacto, materials, fragments, artifacts, reserve notes.
4. **Quartel-General** — buildings table (catalog picker for Installation, level, active toggle) with **live CS active-cap validation**; shows build/upgrade cost (`level × weight × 10` resources / `×3` days).
5. **Pessoal** — workers + mercenaries roster (type/ranking picker, default salary pre-fill, active toggle).
6. **Conhecimento** — maps/recipes/enemies/bosses/records lists.
7. **Doutrinas** — active doctrine selection (catalog picker), enforcing the derived doctrine limit.
8. **Expedições** — expeditions log table.
9. **Legado** — historical feats.
10. **Capacidades** — read-only derived panel: Stage, CG, CI, CF, CS, inflation, daily maintenance, income, caps.
11. **Interlúdio** — the calculator: a day-count input, projected indicators list, each with an **Aplicar** button (calls the apply endpoint, toasts result, refreshes).

Reuse the design-system toolkit: `ToastService`, `ConfirmService`, `LoadingIndicator`, `TableSearchBox`/`TableFilter` (with distinct `*.NoResults` vs `*.Empty` strings), `Breadcrumbs`, `.ledger-table.stack-mobile` with `data-label`, and the catalog-picker archived-entry pattern (fetch `includeArchived: true`, filter archived out of "add new", keep full list to resolve referenced ids). All strings via `IStringLocalizer` (pt-BR + en resx) — none hard-coded.

---

## 8. Testing (TDD)

**Unit (`Ruptura.UnitTests`):**
- `GuildStatsCalculator` reproduces the official §10.9 CS/CI/CF table for a canonical active build at each of the 8 stages (exact-value oracles). **Note (sub-plan #2):** the §10.8 **CG** table (Infra/Pesquisa/Logística/Recursos totals per stage) is a *narrative design-target* progression, NOT reproducible from the §10.8 formula — e.g. at Fundação the table's Logística=5 is impossible since `Logística = CS + workers×2 ≥ CS = 6`. Only the §10.9 CS/CI/CF table is used as an exact oracle.
- Doctrine modifiers (Logística +20% CS / −10% maintenance; Comercial inflation shift).
- Inflation index, daily maintenance, income, caps, doctrine limit.
- CS active-building overflow flag.
- `InterludeCalculator` projections for each Kind, including researcher-split floor (50%) and stage transitions / point award on completion.

**Integration (`Ruptura.IntegrationTests`, Testcontainers/Postgres):**
- Guild create-on-first-access; 1:1 uniqueness enforced.
- Shared-write permission matrix: GM, member, non-member; read == write access.
- CRUD on child collections.
- Interlude apply recomputes server-side (a client-supplied bogus delta is ignored; only the server number lands).
- Blob xmin concurrency-token conflict path.
- Catalog seed present: 20 installations + 8 doctrines with correct `DataJson` shapes.

---

## 9. Reused Patterns / Project Conventions

- **DataJson defensive deserialization** — guarantee every blob member non-null at the boundary (character-sheet §5 lesson).
- **Catalog picker archived handling** — copy the four-tab pattern.
- **Controller orchestrates, services stay one-directional** — if any cross-service reaction is needed (e.g. research completion nudging CG display), wire at the controller, not via service back-references.
- **Client input as selector, not authoritative value** — the Interlude apply endpoint and any "keep these" path lists.
- **Real FK only where cascade/uniqueness matters** (`GuildSheet.CampaignId`), soft references elsewhere, matching the schema style.

---

## 10. Data-Model Impact Summary

| Change | Kind |
|--------|------|
| `GuildSheet`: + `CampaignId` (FK+unique), + xmin concurrency token; − `Memberships` | Modify + migration |
| Drop `GuildMembership` entity + table | Migration |
| New entities: `GuildBuilding`, `GuildStaff`, `ResearchProject`, `CraftingOrder`, `Expedition` | Migration |
| `CatalogEntryType`: + `Installation`, + `Doctrine` | Enum |
| Seed 20 installations + 8 doctrines | Seed migration |
| `Ruptura.Shared`: `GuildSheetData` + submodules, `GuildDerivedStats`, `InterludeProjection`/`InterludeIndicator`, request/response DTOs | New |

---

## 11. Open Items / Decisions to Confirm in the Plan

1. **Portão modeling — RESOLVED (sub-plan #1).** Seeded as a level-fixed installation flagged `NonConstructible: true` (weight 1). **Sub-plan #2's calculator MUST exclude `NonConstructible: true` entries from CG's Infra term AND from the CS active-building cap count** — the GDD gives Portão `Peso —` and counts 19 constructible installations, so it must not add to Infra. Excluded from the "add building" picker in the UI. *(GuildBuilding also carries a unique index on `(GuildSheetId, CatalogEntryId)` — one building of each installation type per guild; decided at final review, sub-plan #1.)*
2. **"Qualified workers" definition** for CG's Logística term — which worker types count as qualified (Artesãos/Pesquisadores/Instrutores/Médicos/Administradores, i.e. non-Operário?). Confirm against GDD intent when writing the plan.
3. **"Converted strategic materials" valuation** for CG's Recursos term — needs a concrete conversion rule (exchange base 1 Moeda de Pacto = 10 Prata is the only fixed rate). Confirm the material→value mapping or make it a summed manual field.
4. **Emblem upload** reuses `IFileStorageService` with a new `guild-sheets/{id}/...` path prefix + a matching `AuthorizeGuildAccessAsync` check in `MediaController` (same path-encoded authorization pattern).
5. **Latent FK/cleanup class** — no `DELETE` endpoint exists for `Campaign`/`GuildSheet` yet; the new cascades (and emblem orphan files) join the existing "unexercised cleanup" set. One pass whenever a real delete endpoint is built.

---

## 12. Suggested Implementation Ordering (single spec, incremental delivery)

1. **Foundation** — entity changes + migration (CampaignId, RowVersion, drop GuildMembership, child tables), `CatalogEntryType` extension, installation/doctrine seed.
2. **GuildStatsCalculator** + Capacidades panel (derived read path).
3. **Record-keeping modules** — Identidade, Recursos, Prestígio/Influência, Conhecimento, Legado, Expedições.
4. **Quartel-General + Pessoal + Doutrinas** — buildings (CS validation), staff roster, doctrine selection.
5. **Pesquisa/Crafting** — project & order entities + workflow.
6. **Interlude Calculator** — preview + per-indicator apply.
7. **UI polish + i18n** — full pt-BR/en resx, mobile tables, toolkit wiring.
