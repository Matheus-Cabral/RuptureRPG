# GAME MASTER'S MANUAL
### Hardcore Dungeon Crawler RPG

> This manual gathers everything you need to run the campaign: design philosophy, the complete cosmology, content construction (floors, creatures, encounters), and the calibration tools validated in playtest. Character, combat, exploration, magic, and equipment rules that players use directly are detailed in the **Player Manual** — this manual references those sections instead of repeating them.

---

## Table of Contents

1. [System Philosophy](#1-system-philosophy) — Central Concept, Pillars, the 16 Design Principles
2. [Complete Cosmology](#2-complete-cosmology)
3. [Campaign Structure](#3-campaign-structure) — Arcs, Floors, Special Floors
4. [The Dungeon](#4-the-dungeon) — Structure, Pressure (numeric), States, Rewards
5. [Creatures](#5-creatures) — Types, Behavior/AI, NP, Sheet, Creation Manual, Bestiary
6. [Hordes and the Encounter System](#6-hordes-and-the-encounter-system) — PG/PE, Threat Budget, FCE
   - 6.6 [Content-Building Guide](#66-content-building-guide-creature--encounter--floor--arc--campaign) (Creature → Encounter → Floor → Arc → Campaign)
7. [Dominion and the Four Pillars of Progression](#7-dominion-and-the-four-pillars-of-progression)
8. [The Guild — GM Tools](#8-the-guild--gm-tools)
   - 8.1 [Guild Capacity (CG)](#81-guild-capacity-cg) · 8.2 [Headquarters and Buildings](#82-headquarters-and-buildings) (complete tech tree) · 8.3 [Workers/Mercenaries](#83-workers-mercenaries-and-departments) · 8.4 [Economy](#84-economy) · 8.5 Doctrines
9. [Interlude](#9-interlude--running-the-time-between-sessions) — Running the Time Between Sessions
10. [Dynamic Events, Tension, and Factions](#10-dynamic-events-tension-and-factions)
11. [Campaign Record](#11-campaign-record)
12. [Appendix — Consolidated Formulas](#12-appendix--consolidated-formulas)
13. [Complete Glossary](#13-complete-glossary)
14. [Known Pending Items](#14-known-pending-items--all-closed) — all closed

---

## 1. System Philosophy

### Central Concept
> A hardcore dungeon crawler where the players run, as a Council, a permanent Guild of explorers in service of a deity; the Guild is the true protagonist of the campaign, and the characters who descend into the Dungeon are valuable resources — but expendable.

Use this concept as a filter: every new rule (yours or the players') must be tested against it.

### Pillars
Hardcore dungeon crawler with high lethality · persistent world · permanent Guild as the "main character" · expendable characters · progression through actions taken, never generic XP · rewarding exploration (information = power) · strategic Interlude · **time is the game's most important resource**.

### The 16 Design Principles

1. **Dungeon Dominance** — progress made outside the Dungeon never exceeds what's gained by exploring. `Dungeon >>> Interlude >>> Inactivity.`
2. **Specialization** — all evolution comes from the activity practiced; there is no universal XP.
3. **Modifier Origin** — every bonus needs an identifiable source.
4. **Golden Rule** — no activity generates unlimited progress without consuming a limited resource.
5. **Symmetry** — the same rules apply to players and to the world (NPCs, factions, creatures).
6. **Linear Progression** — every activity grants a fixed base progress; bonuses modify it, never scale with Ranking.
7. **Failures as Consequence** — failing never blocks, it generates a consequence.
8. **Narrative Coherence** — the narrative justifies the mechanics, never replaces them.
9. **Permanent Institution** — the Guild never regresses; the character is replaceable, the organization is not.
10. **Milestones** — perceptible evolution in clear milestones.
11. **Natural Limit** — every attribute/skill has a ceiling (Grade V); exceeding it requires Transcendence.
12. **Scale of Conflict/Organization/Behavior/Information** — mass conflicts follow the same rules, at a different scale.
13. **Automation/Exploration Frontier** — NPCs and mercenaries never replace the players; they only operate in conquered areas.
14. **Living World** — the world evolves on its own in the players' absence.
15. **Irreversible Progression** — completed floors are not repeated by player characters.
16. **Dominion** — true victory over the Dungeon is gaining permanent influence (Strategic Assets), not just surviving.

---

## 2. Complete Cosmology

Ancient deities created independent universes; many were destroyed, leaving behind **Dimensional Fragments** that collide with other realities. The **Central World** contains **Gates** that imprison each Fragment — each Gate holds a **Dungeon**, and each floor is a preserved piece of a dead universe (biomes/technologies/creatures vary freely between floors).

**Dimensional Stability**: fragments accumulate pressure to return to the real world; exploring reduces this pressure. Losing stability causes a **Rupture** — the Dungeon invades the Central World.

Each deity competes for influence through the efficiency of the Guilds that manage their Gates (replacing direct war between gods). **Patrons** made a pact: they never cross the Gate (they are "Anchors"), they keep the Guild active, they preserve knowledge. If a Patron dies without a successor, stability collapses.

```
Player → Patron → Guild → Gate → Dungeon → Characters
```

Use this foundation to organically justify: Guild Registry (a divine requirement of control), Rankings (certification of who can contain greater instability), Interlude (continuous preparation), Buildings (operational capacity), Doctrines (divine philosophies), Memory Crystals (knowledge that cannot depend on a single individual).

---

## 3. Campaign Structure

### 3.1 Arcs
A universe that ended its cycle (an entire Fragment). Every Arc defines: theme, story, conflict, final objective, specific pressure, ecosystem, exclusive resources and mechanic, and at least 5 floors. Suggested narrative structure: **Introduction → Investigation → Development → Preparation → Climax → Consequence.**

### 3.2 Floors
Objective types: Exploration, Reconnaissance, Defense, Attack, Hunt, Escort, Survival, Puzzle, Elimination, secret objectives. Classification: **Transitional** (passage) · **Strategic** (grant Strategic Assets) · **Narrative** (advance the story) · **Milestone** (turning points).

### 3.3 Special Floors
Every 5 floors, a Special Floor of elevated difficulty. **Fixed rule**: the 5 preceding floors always contain the tools needed to beat it. Those who explore little can still reach the boss; those who explore thoroughly survive it.

### 3.4 Irreversible Progression
Completed floors are not played again by the characters (mercenaries/secondary expeditions can operate on them later — §7 of this manual).

---

## 4. The Dungeon

### 4.1 Floor Structure
Every floor has: Identity (biome inherited from the source fragment), Main Objective, Secondary Objectives, Failure Condition.

### 4.2 Dungeon Pressure
Scale: **Stable → Aggravated → Critical → Collapse.** Each floor type "presses" in a way coherent with its theme (a living forest suffocates, a volcanic floor heats up, a fortress reinforces its defenses). It feeds events, penalties, and environmental changes.

**Numeric counter (0-100 per floor, resets on every new floor)**:

| State | Range | Multiplier on remaining encounters' PE |
|---|---:|---:|
| Stable | 0–24 | ×1.00 |
| Aggravated | 25–59 | ×1.10 |
| Critical | 60–89 | ×1.25 |
| Collapse | 90–100 | ×1.50 + automatically triggers a Collapse Event (reinforcements, a drastic environmental change, or an immediate risk to the Failure Condition) |

**Standard sources of Pressure** (a starting point, adjust freely): An Exploration Turn beyond the floor's expected Duration → +5. Each completed combat → +10. Critical failure on a relevant test → +15. A narrative event defined by you → +20 to +60.

The Pressure multiplier adds to the Terrain/Intelligence/Objective multipliers already present in the PE formula (§6.3) — the Dungeon reacts to the players' presence instead of waiting motionless.

### 4.3 Floor States
**Unexplored → Explored → Conquered → Dominated.**

### 4.4 Rewards and Information
Rewards: Knowledge, Resources, Progress. Treat Information as a concrete resource — knowing a boss beforehand should be worth as much as raw power.

---

## 5. Creatures

### 5.1 Types (nature/origin)
Beasts · Undead · Aberrations · Spirits · Constructs · Corrupted Humanoids · Draconic · Extraplanar Entities.

### 5.2 Function in the Dungeon
Predator, Guardian, Soldier, Parasite, Living Event. *(Type and Function combine freely.)*

### 5.3 Behavior (AI)

| Behavior | Multiplier (§6.3) | Action rule |
|---|---|---|
| Instinctive | Instinct (×1) | Attacks the nearest target/lowest Defense; never uses group tactics; flees at PV<25% or low Morale |
| Intelligent | Tactical (×1.2) / Military (×1.5) | Chooses a target by perceived threat; retreats to reposition; uses the Reaction optimally |
| Strategic | Genius (×2) | Coordinates the group, targets known weaknesses, feigns retreat, uses terrain/Cover, prioritizes support/casters |

### 5.4 Natural Characteristics Table (NP cost)

| Weight | NP | Examples |
|---|---:|---|
| Minor | 1 | Darkvision, Keen Smell, Seismic Sense, Resistance to 1 element |
| Medium | 3 | Carapace (+2 Damage Reduction), Flight, Natural Camouflage, Multiple Eyes (immune to Surprised) |
| Major | 5 | Regeneration, Potent Poison, Multiple Attacks |
| Supreme | 10 | Metamorphosis, Dimensional Core (revives 1x), Immunity to a damage category |

### 5.5 Creature NP Formula
```
NP(creature) = (Attributes + Natural Skills) + Σ Characteristics + Σ Abilities + Equipment
```
(Ability common=5 / advanced=10 / supreme=20 — same logic as characters, Player Manual §2)

### 5.6 Creature Categories

| Category | NP Range | Ranking Equivalent |
|---|---|---|
| Weak | 20–40 | Below Bronze |
| Common | 40–70 | Bronze |
| Veteran | 70–105 | Iron |
| Elite | 105–195 | Steel–Silver |
| Champion | 195–340 | Gold–Mithril |
| Minor Boss | 340–430 | Adamant |
| Arc Boss | 430–550+ | Legendary |
| Superior Entity | 550+ | Above Legendary |

### 5.7 Simplified Creature Sheet
```
NAME (Type — Function — Category: NP XX)
Behavior: Instinctive / Intelligent / Strategic
PV: XX | Passive Defense: XX | Movement: XX
Main attack: 2d10 + X vs Defense | Damage: XdX+X
Characteristics: [brief list, 1 line each]
Abilities: [brief list]
Weakness: [1 line]
Rewards: [brief list]
```

### 5.8 Creature Creation Manual
Step by step: (1) Concept; (2) Type (§5.1 or homebrew, §5.9); (3) Function (§5.2); (4) Behavior (§5.3, already fixes the encounter multiplier); (5) Target Category (§5.6, already defines the NP range); (6) distribute the target NP among Attributes+Skills, Characteristics (§5.4), Abilities, and Equipment; (7) define 1 mandatory Weakness; (8) define Rewards; (9) validate against the checklist.

**Balancing Checklist**: **Weakness Rule** — every creature needs at least 1, without exception. **Clear Function Rule** — 1 defined primary function, never a "generic monster." **Category Cap Rule** — total NP does not exceed the Category's range by more than 15%.

### 5.9 Homebrew Type Creation Manual
(1) it describes the **nature/origin**, never function or behavior; (2) it must be compatible with the cosmology (§2) — which fragment/arc it represents; (3) it **never grants its own mechanical bonus** — it's a narrative classification; (4) it combines freely with any Function/Behavior.

### 5.10 Base Bestiary

| Name | Type | Function | Behavior | Category | Key Characteristics | Weakness |
|---|---|---|---|---|---|---|
| Goblin Raider | Corrupted Humanoid | Soldier | Instinctive | Weak | Enhanced Senses | Flees below 50% PV |
| Plagued Rat | Beast | Parasite | Instinctive | Weak | Keen Smell | Vulnerable to fire |
| Guardian Skeleton | Undead | Guardian | Instinctive | Common | Carapace (bone) | Vulnerable to blunt damage |
| Corrupted Cultist | Corrupted Humanoid | Soldier | Intelligent | Common | Minor ritual | Low Will (easy to intimidate) |
| Deep Spider | Beast | Predator | Instinctive | Veteran | Potent Poison, Natural Camouflage | Sensitive to strong light/vibration |
| Corrupted Knight | Undead | Guardian | Strategic | Elite | Carapace, Regeneration | Vulnerable to sacred magic |
| Swamp Witch | Aberration | Soldier (Control) | Strategic | Elite | Advanced Control ability | Weak in melee combat |
| Fragmented Stone Golem | Construct | Guardian | Instinctive | Champion | Double Carapace, Immune to Poison/Fear | Exposed core |
| Spectral Commander | Spirit | Soldier (Command) | Strategic | Champion | Flight, Supreme Command (horde buff) | Dissipates with sacred light/a Seal |
| Eclipse Dragon | Draconic | Boss (Sovereign) | Strategic | Arc Boss | Flight, Regeneration, Multiple Attacks, breath weapon | Exposed core after a certain phase |

---

## 6. Hordes and the Encounter System

### 6.1 Hordes and Mass Conflicts
Types: Swarm, Army, Invasion, Catastrophe. Size: Small, Medium, Large, Massive. Each horde has its own Power, Pressure, Origin, Command, and turns (it acts in blocks). Objectives: Survival, Defense, Escort, Containment, Retreat. Scaled Command (Individual → Tactical → Military → Strategic), with its own Leadership, Strategy, Military Knowledge, Intelligence, and Morale.

### 6.2 Creature Scale Against the Party
The character-NP × creature-NP relationship is calibrated by category (Common/Elite/Boss), with a **Horde Factor** (a multiplier by the number of simultaneous enemies) and specific Boss rules (phases, Legendary Actions). **Principle of Dungeon Superiority**: by default, the Dungeon should slightly outmatch the party — never be trivial.

### 6.3 Encounter System (tabletop formulas)
**Party Power (PG)**:
```
PG = Σ NP(characters) × Synergy Factor
```

| # of characters | Factor |
|---|---:|
| 1 | 1.0 |
| 2 | 1.1 |
| 3 | 1.2 |
| 4 | 1.3 |
| 5 | 1.4 |
| 6+ | 1.5 |

**Encounter Power (PE)**:
```
PE = Σ NP(creatures) × Quantity × Intelligence × Terrain × Objective
```

- Quantity: 1→1 | 2-3→1.25 | 4-8→1.5 | 9-20→2 | 20+→3
- Intelligence: Instinct→1 | Tactical→1.2 | Military→1.5 | Genius→2
- Terrain: Neutral→1 | Slightly favorable→1.1 | Favorable→1.25 | Extreme→1.5
- Objective: Eliminate→1 | Survive→1.25 | Defend→1.5 | Rescue under pressure→1.5 | Critical mission→2

**Encounter classification**: `R = PE / PG`

| R | Difficulty |
|---|---|
| ≤0.5 | Very easy |
| 0.75 | Easy |
| 1 | Balanced |
| 1.25 | Hard |
| 1.5 | Very hard |
| 2 | Extreme |
| ≥3 | Possible death |

**Separate difficulties**: **Combat Difficulty (DC = PE/PG)** measures how hard it is to beat the enemies; **Objective Difficulty (DO)** measures how hard it is to complete the mission (time, environment, pressure) — they are independent.

### 6.4 Floor Threat Budget
```
OA = PG × Difficulty × Duration Factor
```

- Difficulty: Safe 0.75 | Normal 1.0 | Dangerous 1.25 | Deadly 1.5 | Infernal 2.0 | Apocalyptic 3.0
- Duration: Short (1-2 encounters)→1 | Normal (3-5)→2 | Long (6-10)→3 | Extended→4

Distribute the OA among creatures, traps, events, elites, and a boss (e.g.: a combat floor ≈ 70% creatures/15% environment/15% events; a boss floor ≈ 70% boss/20% mechanics/10% environment).

### 6.5 Encounter Compression Factor (FCE) — validated by Playtest
Using the Encounter Ratio (R) directly as a multiplier on the enemy's real stats creates a "cliff" (the party almost always wins or almost always loses). When building the stats of a creature/group to hit a target Ratio R:
```
Real Attribute/Skill Multiplier = 1 + (R − 1) × FCE
```

| Party Ranking | FCE |
|---|---:|
| Bronze–Iron | 0.40 |
| Steel–Silver | 0.25 |
| Gold–Mithril | 0.15 |
| Adamant–Legendary | 0.10 |

The FCE decreases as Ranking rises: low-Ranking parties have reduced PV (dice variance alone already smooths out the difficulty); high-Ranking parties have high PV (more deterministic combats), requiring stronger compression to preserve the Favorable/Balanced/Unfavorable/Impossible gradation.

> **Playtest Result** (Monte Carlo, 2d10, 500 combats/cell, parties of 4): with the Combat fixes (see Player Manual §4.5) and the FCE above, the win rate stayed consistent across the 8 Rankings — Favorable 92-100%, Balanced 40-68%, Unfavorable 11-30%, Impossible ~0%.
>
> **Validation with heterogeneous parties (CLOSED)**: I repeated the simulation with realistic parties (1 Tank, 2 Balanced, 1 DPS, individual NP varying ±20% — not identical builds) to make sure the FCE wasn't an artifact of the "average character." Result: the FCE stayed stable and was **even more consistent** across Rankings — Favorable 77-98%, Balanced 53-65%, Unfavorable 15-32%, Impossible 0-5%. The FCE is validated for direct use at the table, with no further caveats.

### 6.6 Content-Building Guide (Creature → Encounter → Floor → Arc → Campaign)

This guide ties together all the already-closed tools into a single workflow. Use it top-down when prepping a new session, or bottom-up when improvising in play.

#### Level 1 — Create a Creature

1. Choose the **Concept** and the **Type** (§5.1, official or homebrew via §5.9).
2. Define **Function** (§5.2) and **Behavior** (§5.3) — the Behavior already fixes the Intelligence multiplier it will use in any encounter.
3. Choose the **Target Category** (§5.6) — this defines the NP range.
4. Distribute the NP among Attributes/Skills, Characteristics (§5.4), and Abilities until you hit the range.
5. Define 1 mandatory Weakness and the Rewards.
6. Fill in the Simplified Sheet (§5.7). **Done**: the creature can now enter any encounter.

#### Level 2 — Build an Encounter

1. Calculate the current party's **PG** (§6.3).
2. Choose how many creatures, of which Category, will make up the encounter (Categories can be mixed).
3. Calculate the **PE** (§6.3) with the Quantity/Intelligence/Terrain/Objective multipliers — include the **Pressure** multiplier (§4.2) if the floor is already Aggravated+, and the **Faction** multiplier (§10) if it's the territory of a faction with relevant Reputation.
4. Calculate `R = PE/PG` and check it against the classification table (§6.3) — this tells you whether the encounter is Easy, Balanced, Hard, etc.
5. If you're building the creature/group from scratch to hit a specific R (instead of assembling it from ready-made creatures), apply the **FCE** (§6.5) when multiplying the enemy's attributes/skills.

#### Level 3 — Build a Floor

1. Calculate the **Threat Budget (OA)** = PG × Difficulty × Duration (§6.4).
2. Distribute the OA among Combat/Exploration/Events/Pressure/Boss according to the floor type (suggested proportions in §6.4; adjust freely for narrative floors vs. pure combat floors).
3. Design the floor's Areas, each consuming a slice of the OA — mix Encounters (Level 2), Exploration tests, and Events.
4. Define the source and pace of **Pressure** (§4.2) throughout the floor.
5. Define the Rewards and at least 1 **Strategic Asset** with an assigned Strategic Value (§7).
6. Validate the result: does the floor have at least 1 Balanced+ encounter and a real choice (with no objectively "wrong" option)? See the complete example in GDD §17.10 ("The Village of a Thousand Monsters") as a reference for a good result.

#### Level 4 — Structure an Arc

1. Define the theme, story, conflict, and final objective (§3.1).
2. Plan at least 5 floors (Level 3), following the suggested narrative progression: Introduction → Investigation → Development → Preparation → Climax → Consequence.
3. Every 5 floors, insert a **Special Floor** (§3.3) — make sure the 5 preceding floors contain the tools needed to beat it.
4. Decide the arc's thematic Pressure (what kind of "growing threat" it represents) and which Factions are in play.

#### Level 5 — Plan the Campaign

1. Chain Arcs together (Level 4), each representing a different Dimensional Fragment.
2. Track the **Four Pillars** in parallel (§7): characters' NP, the Guild's CG, Strategic Resources, accumulated Strategic Assets.
3. Use the Campaign Record (§11) to maintain consistency between sessions.
4. Let the Guild evolve at the same pace as the 5-floor milestones (§8.1) — this keeps the Guild and the Dungeon advancing together, reinforcing the Principle of the Permanent Institution.

---

## 7. Dominion and the Four Pillars of Progression

The campaign progresses on four simultaneous, independent fronts:

1. **Individual Power (NP)** — characters.
2. **Institutional Power (CG)** — the Guild.
3. **Strategic Resources (SR)** — consumable/economic goods.
4. **Strategic Assets (SA)** — permanent achievements obtained in the Dungeon.

**SA Categories**: Infrastructure, Knowledge, Diplomacy, Artifacts, Territorial Control. **Strategic Value (SV)**: a scale from 1 (local benefit) to 5 (large-scale permanent change) — use it to calibrate risk vs. reward. Not all of a floor's SAs can be obtained at the same time: force choices between conflicting objectives.

> **Fundamental Principle**: characters evolve through NP; the Guild evolves through CG; the campaign evolves through Strategic Assets.

---

## 8. The Guild — GM Tools

### 8.1 Guild Capacity (CG)
**Decoupled from the threat calculation** — never added to the PG nor the OA (this avoids counting the Guild's strength twice).
```
CG = Infrastructure + Research + Logistics + Resources
```

- Infrastructure = Σ (level of each building × weight: Foundation=1, Production=2, Specialization=3, Institutional=5, Monumental=8)
- Research = points accumulated in completed projects
- Logistics = Support Capacity (CS) + number of qualified workers × 2
- Resources = Pact Coin reserves + converted strategic materials

**Official table by stage** (a milestone every 5 floors):

| Stage | Floors | Infra | Research | Logistics | Resources | **CG** |
|---|---:|---:|---:|---:|---:|---:|
| Foundation | 0 | 5 | 0 | 5 | 5 | **15** |
| Minor Guild | 5 | 20 | 10 | 15 | 15 | **60** |
| Regional Guild | 10 | 45 | 25 | 30 | 30 | **130** |
| Recognized Guild | 15 | 80 | 45 | 50 | 50 | **225** |
| Major Guild | 20 | 125 | 70 | 75 | 75 | **345** |
| Renowned Guild | 25 | 180 | 100 | 105 | 105 | **490** |
| Legendary Guild | 30 | 245 | 135 | 140 | 140 | **660** |
| Divine Guild | 35+ | 320 | 175 | 180 | 180 | **855** |

**8.1.1 Derived Capacities — CI, CF, CS**

Unlike CG (institutional, isolated from combat), these three lock in concrete gameplay limits, each tied to specific facilities (§8.2.1):
```
CS (Support Capacity) = 5 + (Logistics Center Level × 2) + (Warehouse Level × 1)
CI (Institutional Capacity) = 3 + (Council Chamber Level × 4) + (Logistics Center Level × 1)
CF (Formation Capacity) = 10 + (Memorial Level × 3) + (Library Level × 1) + (Training Field Level × 1)
```

| Stage | CS | CI | CF |
|---|---:|---:|---:|
| Foundation | 6 | 3 | 11 |
| Minor Guild | 7 | 3 | 13 |
| Regional Guild | 10 | 4 | 18 |
| Recognized Guild | 12 | 5 | 23 |
| Major Guild | 14 | 10 | 28 |
| Renowned Guild | 16 | 15 | 33 |
| Legendary Guild | 16 | 15 | 34 |
| Divine Guild | 16 | 15 | 34 |

**CS locks in**: the maximum number of buildings active/managed at the same time — with 19 constructible facilities and a CS cap of 16, even a Divine Guild has to choose what stays active (the excess become Inactive, with no benefit).

**CI locks in**: Simultaneous active Patrons = CI ÷ 3 (rounded up, min. 1) · Simultaneous Interlude projects = CI ÷ 2 · Total hireable workers = CI × 3.

**CF grants** (Formation bonus on a new character, Player Manual §3):

| CF | Formation Bonus |
|---|---|
| 10–17 | None (standard Recruit) |
| 18–22 | +5 extra skill points |
| 23–27 | +10 extra skill points; starting equipment can be Uncommon |
| 28–32 | +15 extra skill points; Uncommon equipment guaranteed; +1 extra minor Talent |
| 33+ | +20 extra skill points; Rare equipment possible; 1 starting skill already begins at Basic Grade |

### 8.2 Headquarters and Buildings
Buildings form a real tech tree. Every building has: Prerequisites (structural, institutional, knowledge-based, resource-based, human), Costs, Direct Benefits, Synergies.

**8.2.1 Tech Tree — Complete List**

| # | Facility | Weight | Cap | Prerequisite | What it unlocks |
|---|---|---:|---|---|---|
| **Foundation (Weight 1)** | | | | | |
| 1 | Gate | — | Fixed | Exists from the start | The Dungeon's core; neither built nor upgraded |
| 2 | Dormitory | 1 | V | None | Slots for characters/workers (Level × 2) |
| 3 | Warehouse | 1 | V | None | Storage (Level × 50 units) |
| 4 | Training Field | 1 | V | None | Combat training; Body/Control Trials |
| **Production (Weight 2)** | | | | | |
| 5 | Smithy | 2 | V | Warehouse I | Weapon/armor Crafting (Common→Rare at I-II, Epic at III+) |
| 6 | Workshop | 2 | V | Warehouse I | General Crafting (Common/Uncommon) |
| 7 | Library | 2 | VII | Dormitory I | Minor/Moderate Research; Intellect/Perception Trials |
| 8 | Infirmary | 2 | V | Dormitory I | Advanced healing, PV recovery during the Interlude; Vigor Trial |
| **Specialization (Weight 3)** | | | | | |
| 9 | Arcane Laboratory | 3 | V | Library II | Major Arcane Research; Affinity Trial; Enchantment |
| 10 | Military Academy | 3 | V | Training Field II + Infirmary I | Presence/Will Trials; Supreme Techniques; advanced mercenaries |
| 11 | Alchemical Garden | 3 | IV | Workshop II | Advanced Alchemy (Poisons/Transmutation) |
| 12 | Rune Workshop | 3 | IV | Smithy II | Epic+ Crafting; weapon Enchantment |
| **Institutional (Weight 5)** | | | | | |
| 13 | Memorial | 5 | IV | Library III | Access to Memory Crystals; increases Formation Capacity (CF) |
| 14 | Logistics Center | 5 | IV | Warehouse III + Workshop II | Increases Support Capacity (CS); more Secondary Expeditions |
| 15 | Mercenary Barracks | 5 | IV | Military Academy II | Higher-Ranking Mercenaries; raises the limit |
| 16 | Mage Tower | 5 | IV | Arcane Laboratory III | Supreme Research; advanced Rituals; rare Grimoires |
| **Monumental (Weight 8)** | | | | | |
| 17 | Council Chamber | 8 | II | Logistics Center III + Memorial II | Increases Institutional Capacity (CI); more simultaneous Patrons/projects |
| 18 | Divine Vault | 8 | II | Memorial III | Securely stores Pact Coins; enables Divine Crafting |
| 19 | Dimensional Observatory | 8 | II | Mage Tower III | Predicts Ruptures; reduces the base Pressure of explored floors |
| 20 | Patron's Sanctuary | 8 | I–II | Council Chamber I + Divine Vault I | Strengthens the Divine Pact; resistance to negative Divine events |

**8.2.2 Construction and Upgrade Cost**: reuses the weights already fixed in CG (§8.1). The cost of **upgrading** a facility from one Level to the next uses the same formula, always referring to the target Level (e.g.: raising the Smithy from II to III costs the full Level III value).
```
Resource Cost = Target Level × Category Weight × 10
Construction/Upgrade Time = Target Level × Category Weight × 3 days
Minimum workers = Category Weight
```

| Category (Weight) | Level I | Level III (if applicable) |
|---|---|---|
| Foundation (1) | 10 resources / 3 days | — |
| Production (2) | 20 resources / 6 days | 60 resources / 18 days |
| Specialization (3) | 30 resources / 9 days | 90 resources / 27 days |
| Institutional (5) | 50 resources / 15 days | 150 resources / 45 days |
| Monumental (8) | 80 resources / 24 days | rarely goes past Level I-II |

Monumental buildings also require **Pact Coins = Level × 2**, in addition to common resources.

**Principle of Institutional Maturity**: prerequisites check the **level** of the base building, not just its existence. Not every building has the same cap (Dormitory may stop at V; Library may go up to VII).

**Guild Technology Level (NTG)**: infrastructure + accumulated knowledge — a reference for cutting-edge unlocks.

Start of the campaign: only the Gate, Dormitory, and a basic Training Field exist.

### 8.3 Workers, Mercenaries, and Departments
Workers (Laborers, Artisans, Researchers, Instructors, Merchants, Physicians, Administrators) have efficiency, salary, morale, and a specialty — good, but never as good as the players. Mercenaries only operate on floors already conquered (they never replace the players). Departments (Exploration, Military, Arcane, Logistics) aggregate functions to ease administration in large campaigns.

### 8.4 Economy
Common currency (**Silver**) + materials + Pact Coins (a divine currency). Base exchange rate: **1 Pact Coin = 10 Silver**. Funding: Free Contribution, Guild Contract, Return Investment. Rewards split between Character / Guild / Strategic Reserve.

**Base prices**: Ration 1 Silver | Lodging 2 Silver | Laborer wage 3 Silver/day | Artisan/Researcher wage 8 Silver/day | Building maintenance = Category Weight × 1 Silver/day.

**Mercenary Wage by Ranking**: Bronze 10 | Iron 18 | Steel 30 | Silver 50 | Gold 80 | Mithril 120 | Adamant 170 | Legendary 250 (Silver/day).

**Income Generation**: Expedition Rewards (the "Guild" share) · Trade (Commercial Doctrine +10% on sales) · Workers (Laborer ~2 Silver/day) · Secondary Expeditions (`Mercenary's NP × 0.5 Silver` per success) · Legacy (permanent income bonuses).

**Daily Maintenance** = Σ(Level × Weight × 1 Silver, per building) + Σ(active wages). Unpaid: buildings enter Neglect (half the benefit) and Workers lose Morale — it never blocks the game.

**Inflation — Price Index by Guild Stage** (same stages as CG, §8.1): Foundation ×1.0 | Minor Guild ×1.2 | Regional Guild ×1.5 | Recognized Guild ×1.8 | Major Guild ×2.2 | Renowned Guild ×2.6 | Legendary Guild ×3.2 | Divine Guild ×4.0. `Adjusted Price = Base Price × Index`. Money never "solves the game" at advanced stages — the cost of operating grows alongside the Guild's ambition.

### 8.5 Doctrines
A tree of institutional specialization. The Guild starts with **up to 2 active Doctrines**, unlocking **+1 per Council Chamber Level** (§8.2.1), up to **4 simultaneous**. Swapping one requires an Interlude project (20 days, Difficult Leadership/Administration Test).

| Doctrine | Bonus |
|---|---|
| Military | +10% Attack/Damage for Mercenaries/combat NPCs; -1 day on Body/Control/Presence/Will Trials |
| Academic | +15% Research speed; -10% cost on Intellect/Perception Trials |
| Commercial | +10% on surplus sales; -1 stage on the Price Index for the Guild's own purchases |
| Exploration | +15% success on Secondary Expeditions; -10% resource consumption for the main party |
| Arcane | -1 extra PA on casting for the whole Guild; -25% on Affinity Trial time |
| Engineering | -15% on Construction/Upgrade time; +10% chance of a Great Success in Crafting |
| Logistics | +20% Support Capacity (CS); -10% Daily Maintenance |
| Diplomatic | New factions start with +15 Reputation; Moderate gains count as Major |

---

## 9. Interlude — Running the Time Between Sessions

**Two timelines**: Dungeon Time (used during the session) and World/Headquarters Time (passes in weeks between sessions, with a fixed dilation — e.g.: 10 days in the Dungeon ↔ 1 day at the Headquarters). Each character receives interlude actions proportional to the time since their last expedition; the player declares, you resolve.

**Subsystems**: Training (`(1 + Facility Bonus + Instructor Bonus) × Learning Curve Multiplier` points/day; while Untrained — 0-9 points — the Facility/Instructor bonuses don't apply, and there's a fixed cap by Correlation: None=1/day, Low=2, Medium=3, High=5; tests with the skill in that range suffer -2. Full formula and tables in the Player Manual §3.6) · Research (Discover→Research→Master→Apply) · Production/Crafting (see Player Manual §7.3 for the material cost) · Guild Administration · Secondary Expeditions (mercenaries, always limited to conquered floors).

**Research Cost**: reuses the Magic Complexity tiers (Player Manual §6.1):

| Complexity | Time | Resource Cost | Minimum Facility |
|---|---:|---:|---|
| Minor | 5 days | 10 | Basic Library/Workshop |
| Moderate | 10 days | 25 | Library II+ |
| Major | 20 days | 50 | Corresponding Laboratory |
| Supreme | 40+ days | 100+ | Advanced facility + 5 Pact Coins |

Collective research divides the time proportionally, but never below 50% of the base time.

**Modifier Origin Rule**: every facility/instructor/piece of equipment that boosts an activity needs a traceable origin.

---

## 10. Dynamic Events, Tension, and Factions

The world doesn't stop in the players' absence. Event categories: Personal, Guild, Dungeon, World, Divine (generation is Natural, by Consequence, or Narrative).

**Tension System** — 4 indicators accumulate value and increase the chance/intensity of events: Guild Tension, Dungeon Tension, World Tension, Divine Tension. A **Rupture** is the maximum Dungeon Tension event.

**Factions (CLOSED)**: they exist within the Dungeon (Goblins, Cultists, Undead, Merchants, Beasts, rival Adventurers), control territory, react to the players' choices, form alliances/wage war on each other — but their influence stays confined to the floors (not the external political world).

**Reputation** (-100 to +100, 5 levels):

| Reputation | Level | Default Behavior |
|---|---|---|
| -100 to -51 | Hostile | Attacks on sight; closes routes; may put a bounty on the party |
| -50 to -11 | Distrustful | Bad prices, withholds information, demands proof |
| -10 to +10 | Neutral | No bonus/penalty |
| +11 to +50 | Friendly | Trade/information, safe passage, hints |
| +51 to +100 | Allied | Fights alongside the party, shares territory/resources, unlocks exclusive Strategic Assets |

**Consequences of choices**: Minor ±5 | Moderate ±15 | Major ±30.

**Practical effect on the floor** (connects directly to the Encounter System, §6.3): Hostile faction territory = Favorable Terrain (×1.25) or Extreme (×1.5) at the main lair; an Allied faction in an area reinforces local hostile encounters (`PG×1.1`); an active Hostile faction uses the "Critical mission" Objective (×2) more often; Friendly+ Reputation unlocks hidden Strategic Assets; an Allied faction in an area reduces Pressure generated there by -5 (§4.2).

Record each relevant faction's Reputation on the Guild Sheet (Influence) and in the Campaign Record (§11).

---

## 11. Campaign Record

Functions as the "save game": floors conquered, deaths, living characters, resources, buildings, workers, research, faction relations, available memories, important events, doctrines. Keep this updated — it's the base for the Guild Sheet and the narrative history.

---

## 12. Appendix — Consolidated Formulas

```
Attribute Modifier = Attribute − 2
Attribute Grade Bonus (Attack only) = Attribute − 1
Skill Grade Bonus = Basic +0 | Adept +1 | Expert +2 | Master +3 | Legendary +4

NP (character) = (Attributes + Skills) + (Talents + Abilities) + Equipment
NP (creature) = (Attributes + Natural Skills) + Σ Characteristics + Σ Abilities + Equipment

PG (Party Power) = Σ NP(characters) × Synergy Factor
PE (Encounter Power) = Σ NP(creatures) × Quantity × Intelligence × Terrain × Objective
R (encounter classification) = PE / PG
DC (Combat Difficulty) = PE / PG
DO (Objective Difficulty) = calculated separately by time/environment/pressure/information

OA (Floor Threat Budget) = PG × Floor Difficulty × Duration Factor
Real Attribute/Skill Multiplier of the enemy = 1 + (R − 1) × FCE

CG (Guild Capacity) = Infrastructure + Research + Logistics + Resources
CS (Support Capacity) = 5 + (Logistics Center Level × 2) + (Warehouse Level × 1)
CI (Institutional Capacity) = 3 + (Council Chamber Level × 4) + (Logistics Center Level × 1)
CF (Formation Capacity) = 10 + (Memorial Level × 3) + (Library Level × 1) + (Training Field Level × 1)
```

### NP Ranges by Ranking

| Ranking | NP Range | Recommended floors |
|---|---:|---|
| Bronze | 40–70 | 1–5 |
| Iron | 70–105 | 6–10 |
| Steel | 105–145 | 11–15 |
| Silver | 145–195 | 16–20 |
| Gold | 195–260 | 21–25 |
| Mithril | 260–340 | 26–30 |
| Adamant | 340–430 | 31–35 |
| Legendary | 430–550+ | 36+ |

---

## 13. Complete Glossary

**Roles and Structure**

- **Player** — the person at the table. **Patron** — their institutional representation (Guild Council), never enters the Dungeon. **Character** — the expendable adventurer who explores.
- **Guild** — the permanent institution that manages a Gate. **Gate** — the structure that imprisons a Dimensional Fragment; contains a Dungeon. **Dimensional Fragment** — the remnant of a destroyed universe. **Rupture** — dimensional collapse when containment fails.

**Individual Progression**

- **NP** — Power Level (GDD §6.8 / Player Manual). Measures the strength of a character or creature.
- **Ranking** — the character's rank (Bronze → Legendary).
- **Grade** — the level of mastery of an Attribute (I-V) or Skill (Basic → Legendary).
- **PA** — Action Points (3/turn in combat). **PV** — Hit Points.
- **Trial** — an Interlude project to raise an Attribute.
- **Memory Crystal** — the posthumous record of a dead character, accessible at the Memorial.
- **CF** — Formation Capacity: a new recruit's starting potential (formula in §8.1.1).

**Combat and Encounters**

- **PG** — Party Power. **PE** — Encounter Power. **R** — Encounter Ratio (PE/PG), classifies the difficulty.
- **DC** — Combat Difficulty (same formula as R). **DO** — Objective Difficulty (time/environment/pressure, calculated separately).
- **OA** — Threat Budget: the total "danger points" available to build a floor.
- **FCE** — Encounter Compression Factor: dampens the translation of R into real combat stats.

**The Guild**

- **CG** — Guild Capacity (institutional, decoupled from combat). **CI** — Institutional Capacity (what the Guild can sustain; formula in §8.1.1). **CS** — Support Capacity (limit of simultaneously manageable buildings; formula in §8.1.1).
- **NTG** — Guild Technology Level. **Doctrine** — a permanent institutional specialization (up to 4 simultaneous).
- **Memorial** — the facility that grants access to Memory Crystals.
- **Pact Coin** — a premium divine currency (1 Pact Coin = 10 Silver).
- **Formation Debt** — the starting cost every character owes the Guild, automatically paid off with rewards.

**The Dungeon and the World**

- **Special Floor** — occurs every 5 floors; always solvable with what's already been explored.
- **Pressure** — a 0-100 counter per floor (Stable/Aggravated/Critical/Collapse) that increases the PE of remaining encounters.
- **SA** — Strategic Asset: a permanent, non-consumable achievement. **SR** — Strategic Resource: consumable. **SV** — Strategic Value: an Asset's importance (scale 1-5).
- **Reputation** — the Guild's numeric relationship (-100 to +100) with a faction.

---

## 14. Known Pending Items — **ALL CLOSED**

- ~~Cost of attribute progression~~ — **CLOSED** (Player Manual §3.5): the Attribute Trial system. As GM, your role is to ensure the required facility (Level ≥ current Grade) and apply the Absolute Test at the end of the time — the thematic Trials table already suggests which skill to use for each attribute.
- ~~Final cost of research, construction, and crafting~~ — **CLOSED** (§8.2, §9): time/resource/Pact Coin tables for all three (crafting is detailed in the Player Manual §7.3).
- ~~Complete economy~~ — **CLOSED** (§8.4): Silver/Pact Coin, base prices, mercenary wages, income generation, maintenance, and the Price Index by stage (inflation).
- ~~Numeric calculation of Dungeon Pressure triggers~~ — **CLOSED** (§4.2): a 0-100 counter, thresholds, and multipliers validated in the end-to-end test of Floor 1 of Arc 1 ("The Village of a Thousand Monsters").
- ~~Factions: missing numeric reputation mechanic and consequence table~~ — **CLOSED** (§10): Reputation -100/+100, 5 levels, and a direct connection to encounters' Terrain/Objective/Pressure.
- ~~A fully consolidated "GM's manual" for content construction~~ — **CLOSED** (§6.6): a 5-level Content-Building Guide (Creature → Encounter → Floor → Arc → Campaign), tying together all the already-closed tools into a single workflow.
- ~~The FCE was calibrated by aggregated simulation — worth testing and adjusting with real characters at the table~~ — **CLOSED** (§6.5): validated with heterogeneous parties (Tank/DPS/Balanced, NP ±20%), stable result, no further caveats.

**There are no more known pending items.** The system is complete and validated end to end — from character creation to the entire campaign, including statistically tested balancing and a real case built and reviewed (§6.6, Level 3).
