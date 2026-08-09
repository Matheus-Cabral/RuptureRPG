# Bestiary + NPC Library (GM-1) — Design Spec

**Date:** 2026-08-09
**Status:** Approved (design)
**Feature:** A GM-owned, cross-campaign library of combat **Creatures** (full stat blocks with a computed NP) and non-combat **NPCs**, authored freely by the GM. Foundational for the encounter generator (GM-2) and loot (GM-3).
**GDD sources:** §9.5 (Criaturas — FECHADO: types, function, behavior→encounter multiplier, characteristic weights, NP formula, categories, simplified sheet, base bestiary §9.5.10), §6.8 (NP formula: Poder Base + Especialização + Equipamento).

---

## 1. Goal & Scope

The GM builds and reuses an unlimited number of Creatures and NPCs. What the GDD lists (the 8 types, the base bestiary) are **examples/seeds**, not limits.

- **Creatures:** full combat stat block per §9.5.7 with a **server-computed NP** (§9.5.5, grade-bonus interpretation) and a Category-range advisory (§9.5.6 "Regra do Teto").
- **NPCs:** simple non-combat records (role, faction, disposition, location, notes).
- **Scope:** a **global GM library** keyed by the owning Game Master — shared across all that GM's campaigns (NOT per-campaign). Official examples (the §9.5.10 base bestiary) are system-owned (no owner), visible read-only to every GM.

**Out of scope:** creature/NPC portraits (media — future), encounter generation (GM-2), loot tables (GM-3), linking NPCs to guild Influence factions.

## 2. Key Decisions (settled in brainstorming, 2026-08-09)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Creatures vs NPCs | Two separate models |
| 2 | Architecture | Dedicated `Creature` + `Npc` entities, typed `DataJson` blob, structured editors |
| 3 | Scope/visibility | Global GM library (keyed by `GameMasterId`); official examples have no owner |
| 4 | NP | Full calculator (§9.5.5), **grade-bonus** interpretation for "Atributos + Perícias" |
| 5 | Seed | The 10 base creatures (§9.5.10) as official examples; NPCs start empty |

## 3. Data Model

### 3.1 Entities (Domain)
```
Creature: Id (Guid), GameMasterId (Guid?  — null = official example),
          Name (string), DataJson (string, CreatureData), CreatedAt, UpdatedAt
Npc:      Id (Guid), GameMasterId (Guid?  — null = official example),
          Name (string), DataJson (string, NpcData), CreatedAt, UpdatedAt
```
EF: new `Creatures` and `Npcs` tables + one migration. Index on `GameMasterId`.

### 3.2 `CreatureData` (`Ruptura.Shared.Bestiary`, string-only, zero project refs)
- **Classification:** `Type` (string, free — homebrew allowed, §9.5.9), `Function` (string — the 5 official: Predador/Guardião/Soldado/Parasita/Evento Vivo, custom allowed), `Behavior` (string — one of `Instintiva` | `Inteligente` | `Estrategica`; drives the GM-2 intelligence multiplier), `Category` (string — one of the 8: `Fraca`|`Comum`|`Veterana`|`Elite`|`Campea`|`ChefeMenor`|`ChefeDeArco`|`EntidadeSuperior`).
- **NP inputs:**
  - `Attributes` (the 8 GDD attributes as int scores: Corpo, Controle, Vigor, Presenca, Intelecto, Percepcao, Vontade, Afinidade).
  - `NaturalSkills`: `List<{ Name (string), Points (int) }>`.
  - `Characteristics`: `List<{ Name (string), Weight (string: Menor|Media|Maior|Suprema → 1/3/5/10) }>`.
  - `Abilities`: `List<{ Name (string), Tier (string: Comum|Avancada|Suprema → 5/10/20) }>`.
  - `Equipment`: `List<{ Name (string), Rarity (string: Comum|Incomum|Raro|Epico|Lendario|Divino → 1/3/7/15/30/50) }>`.
- **Authored combat sheet:** `Pv` (int), `DefesaPassiva` (int), `Deslocamento` (int), `AtaquePrincipal` (string, e.g. "2d10+5 vs Defesa"), `Dano` (string, e.g. "2d6+3").
- **Balance & rewards:** `Fraqueza` (string, required — Regra da Fraqueza), `Recompensas` (`List<string>` — feeds GM-3), `Notes` (string).

### 3.3 `NpcData` (`Ruptura.Shared.Bestiary`)
- `Role` (string — Patrono/Contato/LiderDeFaccao/Comerciante/QuestGiver, custom allowed), `Faction` (string), `Disposition` (string — Aliado|Neutro|Hostil), `Location` (string), `Notes` (string).

## 4. NP Calculator (`CreatureStatsCalculator`, pure, Application)

Mirrors `CharacterStatsCalculator` (grade-bonus interpretation, confirmed 2026-08-09):
```
NP = Σ (attributeScore − 1)                       // Poder Base: attributes as grade bonuses
   + Σ skillGradeBonus(naturalSkill.Points)       // Poder Base: natural skills as grade bonuses (reuse the same tiered mapping as CharacterStatsCalculator.SkillGradeBonus)
   + Σ characteristicWeight  (Menor 1 / Media 3 / Maior 5 / Suprema 10)
   + Σ abilityWeight         (Comum 5 / Avancada 10 / Suprema 20)
   + Σ equipmentRarityWeight (Comum 1 / Incomum 3 / Raro 7 / Epico 15 / Lendario 30 / Divino 50)
```
- Returns `Np` plus the selected Category's `[NpMin, NpMax]` (§9.5.6 table) and a boolean `CategoryOverflow` — true when `Np` exceeds `NpMax × 1.15` or falls below `NpMin` (advisory only, never blocks a save).
- Overflow-safe: sum in `long`, clamp to `int` (a hand-edited blob can't 500 a read).
- **Server-authoritative:** the response's NP is always recomputed; the client's NP is never trusted (a `CreatureResponse.DerivedNp` field, like `CharacterDerivedStats.Np`).
- Unknown/invalid weight/tier/rarity strings contribute 0 (defensive), mirroring the enum-string guard convention.

## 5. Services / API

- `ICreatureService` / `INpcService` (Application) + impls (Infrastructure), all `[Authorize(Roles = "GameMaster")]`:
  - `GetForGameMasterAsync(gmId)` → the caller's own entries **plus** official (owner-null) examples, each mapped with `DataJson` and (creatures) `DerivedNp`.
  - `GetByIdAsync(gmId, id)` → own or official; else `NotFound` (hide existence).
  - `CreateAsync` / `UpdateAsync` / `DeleteAsync` → own entries only; official (owner-null) are read-only (attempt → `Forbidden`/`NotFound`). `CreateAsync` stamps `GameMasterId = caller`.
- **Validation** (structural blob validation, like the guild): valid JSON; no null list elements; `Fraqueza` required (non-empty) on create/update; clamp attribute scores and points to sane non-negative bounds; `Behavior`/`Category`/weight/tier/rarity validated against their known sets on the server (unknown → the calculator treats as 0; the write may reject an unknown `Behavior`/`Category` with a 400, since those are fixed sets — implementer's call, but never silently corrupt).
- Repositories + EF configs + migration. `ErrorCodes.Bestiary.*` (NotFound / Forbidden / FraquezaRequired / …) with API resx entries in both cultures (guarded by a reflection localization test like `GuildErrorCodeLocalizationTests`).

## 6. UI

GM-global pages (the library is not campaign-scoped): `/gm/bestiary` and `/gm/npcs`, linked from the GM dashboard/nav. Master-detail layout (reuse the UI-B `.master-detail`/`.detail-panel` pattern):
- **Bestiary:** searchable list (reuse `TableSearchBox`) + a structured editor panel — classification pickers (Type free, Function/Behavior/Category), an 8-attribute grid, dynamic add/remove lists (NaturalSkills, Characteristics, Abilities, Equipment, Recompensas), combat fields (Pv/DefesaPassiva/Deslocamento/AtaquePrincipal/Dano), Fraqueza (required), Notes; a **live NP readout + Category-range advisory** (client mirrors the calculator for immediate feedback; server is authoritative). Official examples render read-only with a "duplicate to edit" affordance (copy into the GM's own library) — optional nicety; at minimum, official entries are non-editable.
- **NPCs:** searchable list + a simple form (Role/Faction/Disposition/Location/Notes).
- All strings via `IStringLocalizer` in both resx; empty/loading/toast/confirm-delete via the existing toolkit.

## 7. Testing

- **Unit (`CreatureStatsCalculator`):** the NP formula across all five terms; grade-bonus attribute/skill contribution; Category range + overflow advisory at boundaries (±15%); unknown weight/tier/rarity → 0; malformed/huge blob → no throw, clamped.
- **Integration:** CRUD for creatures and NPCs; a GM sees official + own but not another GM's homebrew; official entries are not editable/deletable by any GM; `Fraqueza` required (400); server recomputes NP (a client-sent NP is ignored). Testcontainers.
- No bUnit (UI verified by build + manual).

## 8. Data-Model Impact

| Change | Kind |
|--------|------|
| `Creature`, `Npc` entities + EF configs + migration (`Creatures`, `Npcs`) | New |
| `Ruptura.Shared.Bestiary`: `CreatureData` (+ nested), `NpcData`, `CreatureResponse` (+ `DerivedNp`), `NpcResponse`, requests | New |
| `CreatureStatsCalculator` (pure, Application) + `ErrorCodes.Bestiary.*` | New |
| `ICreatureService`/`INpcService` + impls + repositories + controllers + DI | New |
| Seed: 10 base creatures (§9.5.10) as official examples | New |
| Web: `/gm/bestiary`, `/gm/npcs` pages + client services + resx + nav entry | New |

No changes to existing entities.
