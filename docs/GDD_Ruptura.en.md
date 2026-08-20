# MASTER GDD — Hardcore Dungeon Crawler RPG
### Consolidated version (drawn from all material developed so far)

> **Note:** this document gathers and organizes everything decided throughout development, eliminating repetition and presenting the **final state** of each decision (when a concept was revised more than once, this is the most recent version). At the end is the system's closure history (§17) — all pending items have been resolved.

---

## Table of Contents

0. [Concept in One Sentence](#0-concept-in-one-sentence)
1. [System Pillars](#1-system-pillars) — Design Principles (16)
2. [Cosmology and Structural Lore](#2-cosmology-and-structural-lore-the-architecture-of-the-world)
3. [The Three Roles](#3-the-three-roles-fundamental-naming-distinction)
4. [Campaign Structure](#4-campaign-structure) — Arcs, Floors, Special Floors
5. [Resolution Core](#5-resolution-core-global-rules--universal-system) — Dice, Tests, Difficulty, Rankings
6. [Character System](#6-character-system) — Full creation, Attributes, Skills, Talents, Magic/Techniques, Equipment, NP
7. [Combat](#7-combat-closed)
8. [Exploration](#8-exploration-closed)
9. [The Dungeon](#9-the-dungeon) — Pressure, Creatures, Encounters, Threat Budget, FCE, Strategic Assets
10. [The Guild](#10-the-guild) — Sheet, Headquarters (tech tree), Economy, CG
11. [Interlude](#11-interlude-the-systems-second-heart)
12. [Dynamic Events and Tension](#12-dynamic-events-and-tension)
13. [Factions](#13-factions-closed)
14. [Campaign Record](#14-campaign-record)
15. [Appendix — Consolidated Formulas](#15-appendix--consolidated-formulas)
16. [Quick Glossary](#16-quick-glossary)
17. [System Closure History](#17-system-closure-history)

---

## 0. Concept in One Sentence

> **A hardcore dungeon crawler where the players run, as a Council, a permanent Guild of explorers in service of a deity; the Guild is the true protagonist of the campaign, and the characters who descend into the Dungeon are valuable resources — but expendable.**

This concept works as a filter: every new rule must be tested against it. If it doesn't strengthen this identity, it probably doesn't belong in the system.

---

## 1. System Pillars

- Hardcore dungeon crawler, with high lethality.
- Persistent world (time passes and the world changes even without a session).
- Permanent Guild — the true "main character" of the campaign.
- Expendable characters (death is permanent and part of the game).
- Progression based on actions taken, never on generic XP.
- Rewarding exploration; information is worth as much as power.
- Strategic Interlude (the period between sessions is as important as the expedition).
- **Time is the game's most important resource.**

### Design Principles (consolidated list)

1. **Principle of Dungeon Dominance** — every form of progress obtained outside the Dungeon must increase the efficiency of the next expedition, but never exceed what would be gained by exploring. `Dungeon >>> Interlude >>> Inactivity.`
2. **Principle of Specialization** — all evolution is a direct consequence of the activity practiced. There is no universal XP.
3. **Principle of Modifier Origin** — every bonus/penalty needs an identifiable source (equipment, talent, facility, doctrine, status, magic, event).
4. **Golden Rule** — no activity generates unlimited progress without consuming a limited resource (time, money, materials, workers, space, prestige, knowledge).
5. **Principle of Symmetry** — the same rules that apply to players apply to the world (NPCs, factions, buildings, and organizations follow the same fundamental laws).
6. **Principle of Linear Progression** — every activity grants a fixed base amount of progress; bonuses modify this value, but the base never scales with Ranking.
7. **Principle of Failures as Consequence** — failing on a floor doesn't block the campaign, it generates consequences (loss of an artifact, a stronger faction, an evolved boss, reduced resources).
8. **Principle of Narrative Coherence** — the narrative exists to justify the mechanics, never to replace them (nor the other way around).
9. **Principle of the Permanent Institution / Continuity** — the Guild never fully regresses; the character is replaceable, the organization is not.
10. **Principle of Milestones** — evolution must be perceptible in clear milestones, not just in invisible numeric increments.
11. **Principle of the Natural Limit** — every attribute/skill has a natural ceiling (Grade V); exceeding it requires Transcendence (see §6.3).
12. **Principle of the Scale of Conflict / Organization / Behavior / Information** (from the creature and horde system) — mass conflicts follow the same fundamental rules, just at a different scale; the intelligence and organization of an enemy group alters its threat as much as its raw power does; information about the enemy is, itself, a resource.
13. **Principle of Automation / the Exploration Frontier** — NPCs and mercenaries never replace the players; they only operate in areas already conquered.
14. **Principle of the Living World / Dynamic Indicators** — the world evolves on its own during the players' absence, and important states (Pressure, Tension, Guild Capacity, Power Level) have their own mechanical values.
15. **Principle of Irreversible Progression** — completed floors are not repeated by the characters (see Secondary Expeditions/Mercenaries for later exploration).
16. **Principle of Dominion** — true victory over the Dungeon is not just survival, it's gaining permanent influence over the universe it represents (Strategic Assets).

---

## 2. Cosmology and Structural Lore (the "Architecture of the World")

The lore exists only to justify the mechanics — never to create exceptions to them.

- In the past, various deities created independent universes. Many were destroyed by wars, cataclysms, or the natural end of their cycle.
- A destroyed universe never disappears completely: it leaves behind a **Dimensional Fragment**, which tends to collide with other realities.
- To contain this, the deities built a **Central World** with **Gates** — structures that imprison each Fragment. Each Gate contains a **Dungeon**.
- Each floor of a Dungeon is a preserved piece of a dead universe — which is why floors can have completely different biomes, technologies, creatures, and physical laws from one another.
- **Dimensional Stability**: fragments accumulate constant pressure to return to the real world. Exploring the Dungeon reduces this pressure. If stability is lost, a **Rupture** occurs — part of the Dungeon invades the Central World, creatures escape, regions are corrupted or replaced.
- Each deity is responsible for certain Gates and competes for influence through the efficiency of the Guilds that manage their Gates (replacing direct wars between gods).
- **Guilds**: permanent institutions responsible for maintaining the stability of a Gate — they organize expeditions, preserve knowledge, develop infrastructure, and train adventurers.
- **Patrons**: each player, in the administrative role, is a Patron. They made a direct pact with a deity that grants them authority over the Guild, in exchange for permanent responsibility for the Gate's stability.
- **Divine Pact**: the Patron can never cross the Gate (they are an "Anchor" — their stable presence outside the Dungeon is what keeps the Gate contained); they must keep the Guild active and expeditions ongoing; they must preserve the accumulated knowledge.
- If a Patron crosses the Gate, the pact breaks. If they die without a legitimate successor, the Guild loses authority over the Gate, stability collapses, and a Rupture occurs.

**Campaign hierarchy:**
```
Player → Patron → Guild → Gate → Dungeon → Characters
```

This narrative foundation organically explains nearly the entire mechanical system: Guild Registry (divine requirement of control), Rankings (certification of who can contain greater instability), Interlude (continuous preparation for containment), Buildings (operational capacity), Doctrines (philosophies taught by the deity), Memory Crystals (knowledge that cannot depend on a single individual), Metaprogression (the Guild's capacity to fulfill its cosmic duty).

---

## 3. The Three Roles (fundamental naming distinction)

- **Player** — the person sitting at the table.
- **Patron** — the permanent representation of the player within the Guild Council; manages the Guild during the interlude; never enters the Dungeon.
- **Character** — the adventurer recruited by the Patron to explore the Dungeon; expendable from the institution's point of view.

The player never "is" the character — they are a Patron who sends successive characters to fulfill the divine pact.

---

## 4. Campaign Structure

### 4.1 Arcs
Each **Arc** represents a universe that ended its cycle of existence (an entire Dimensional Fragment). An arc has: theme, story, conflict, final objective, specific pressure, its own ecosystem, its own resources, an exclusive mechanic, and at least five floors.

Suggested narrative structure for the floors within an arc: Introduction → Investigation → Development → Preparation → Climax → Consequence.

### 4.2 Floors
Each floor is an exploration stage within an arc, with a fixed theme and objective. Planned objective types: Exploration, Reconnaissance, Defense, Attack, Hunt, Escort, Survival, Puzzle, Elimination, secret objectives.

Complementary classification of floors:

- **Transitional Floors** — passage floors, of lesser strategic relevance.
- **Strategic Floors** — grant important Strategic Assets.
- **Narrative Floors** — advance the arc's story.
- **Milestone Floors** — campaign turning points.

### 4.3 Special Floors
Every five floors there is a **Special Floor**, with very high difficulty. Fixed rule: the five preceding floors always contain the tools needed to beat it (information, shortcuts, items, allies, equipment, knowledge). Those who explore little can still reach the boss; those who explore thoroughly can survive it.

### 4.4 Irreversible Progression
Completed floors cannot be repeated by player characters (mercenaries and secondary expeditions can operate on floors already conquered — see §9).

---

## 5. Resolution Core (Global Rules / Universal System)

### 5.1 Dice
**Final decision: 2d10** (not d20). Rationale: it produces a normal curve (average results are more frequent, extremes are rare), reduces the influence of chance over hundreds of sessions, and scales better than a plain d20 as bonuses grow.

### 5.2 Types of Tests

- **Opposed Tests**: when there is direct opposition (combat, stealth vs. perception, grappling, intimidation, a race). Whoever rolls the higher result wins.
- **Absolute Tests**: against a fixed difficulty (perception, translation, research, crafting, climbing, medicine, survival). Success when the result ≥ difficulty.

### 5.3 Difficulty
`Difficulty = Task Category + Challenge Scale (expected for the Ranking)`. Preliminary values:

| Difficulty | Value |
|---|---:|
| Trivial | 8 |
| Easy | 12 |
| Moderate | 16 |
| Difficult | 20 |
| Very Difficult | 24 |
| Heroic | 28 |
| Legendary | 32 |

### 5.4 Degrees of Result

| Result | Effect |
|---|---|
| Far below | Critical failure |
| Below | Failure |
| Equal or higher | Success |
| Well above | Great success |
| Far above | Extraordinary success |

The **Success Margin** (the difference between the result and the difficulty) is actively used to determine the quality of the effect, not just success/failure.

### 5.5 Criticals
Occur on a maximum/minimum natural result or on an extreme difference in the test. Positive criticals generate exceptional feats; negative criticals generate serious consequences.

### 5.6 Rule Hierarchy (for resolving conflicts)
```

1. Global Rules
2. Dungeon
3. Floor
4. Event
5. Character
6. Equipment
7. Temporary Effects
```

### 5.7 Power Level (NP)
A value automatically calculated for balancing, unlocks, content recommendations, and difficulty calculation. The player can look it up, but **never** uses it directly at the table.

### 5.8 Rankings
The character's rank (e.g.: Bronze → Iron → Steel → Silver → Gold → Mithril → Adamant → Legendary). Each Ranking defines: attribute cap, skill cap, allowed equipment, accessible technologies, usable facilities, recommended content. It advances through **achievements** (e.g., reaching a certain floor), never through simple XP accumulation.

### 5.9 Interlude (official definition)
> **The Interlude is the period between two consecutive expeditions of a character, during which they carry out activities at the Headquarters.** Every activity consumes time, has requirements, and produces specific progress.

---

## 6. Character System

### 6.1 Creation Flow (official — final version)
```

1. Origin              (§6.1.2)  → +25 skill pts (15+10), benefit, equipment, hook
2. Background          (§6.1.4)  → benefit + complication (no skill/attribute)
3. Lineage             (§6.1.7)  → cap adjustment on 2 attributes + 1 racial trait
4. Aptitudes (2)       (§6.1.5)  → ease of learning + natural instinct
5. Attributes          (§6.3)    → 20 pts, free purchase, min 1 / max 5 (or 6/4 if adjusted by Lineage)
6. Starting Skills                → those from Origin already apply; distribute any extra points
7. Initial Talent (1)  (§6.1.6)
8. Equipment                      → those from Origin + whatever the Guild provides
9. Power Level (§6.8)             → must fall in the Bronze range (40–70)
10. Guild Registry                → name, registration number, Ranking (Bronze), Formation Debt, date of joining
```

- **Origin**: social/professional past (Soldier, Hunter, Artisan, Peasant, etc.). Grants 1 mechanical benefit, starting skills and/or equipment, and a narrative justification. Rule: origin creates *different* characters, never *superior* ones.
- **Background**: a defining event that shaped the character (event + consequence + benefit + possible complication). See the full manual and closed list in §6.1.3/§6.1.4.
- **Lineage**: the character's species/ancestry. See the full manual and closed list in §6.1.7.
- **Starting Aptitudes**: inclinations that reduce difficulties and improve initial learning — they never block future paths. See the full manual and closed list in §6.1.5.
- **Guild Registry**: every character receives an official record (name, registration number, Ranking, date of joining, Power Level, status — active/wounded/absent/retired/missing/dead —, expeditions completed, floors conquered, specializations).
- Everyone starts as a **Recruit** of the Guild (a common zero point), with the **Formation Debt** already closed in §6.2.

### 6.1.1 Origin Creation Manual

Every Origin — official or created by the GM/player — needs exactly these 4 components:

1. **Main Mechanical Benefit** — a single *light* passive effect. Never a direct bonus to damage, PA, or an attribute. Allowed types: difficulty reduction in a specific test category; access to something exclusive (a contact, a rare skill, a location); a one-off reusable resource (e.g., 1x per expedition).
2. **Starting Skills (fixed rule)** — always **1 primary skill (15 points) + 1 secondary skill (10 points)** = **25 total points across all Origins**, without exception. This ensures no Origin is objectively "better" in quantity — only in direction.
3. **Starting Equipment** — 0 to 2 simple items, never above Uncommon rarity.
4. **Narrative Hook** — 1-2 sentences that give the GM a thread to pull on later.

**Balancing checklist** (mandatory validation for any new Origin):

- **Non-Superiority Rule**: the Origin must make the character *different*, never objectively better overall.
- **Equivalent Cost Rule**: always exactly 15+10 skill points.
- **Trade-off Rule** (recommended): Origins that are very advantageous in a specific niche gain a small corresponding narrative/mechanical fragility.

**Step by step**: (1) define the social concept of the origin; (2) choose the Main Mechanical Benefit; (3) choose the 2 Starting Skills (15+10); (4) define 0-2 Starting Equipment items; (5) write the Narrative Hook; (6) validate against the checklist.

### 6.1.2 Official List of 20 Origins (CLOSED)

| # | Origin | Main Mechanical Benefit | Primary Skill (15) | Secondary Skill (10) | Starting Equipment | Narrative Hook |
|---|---|---|---|---|---|---|
| 1 | Soldier | -1 difficulty on Discipline/organized-combat-training tests | Swords | Armor | Short sword, light armor | Deserted or was discharged from a local military force |
| 2 | Hunter | -1 difficulty on Tracking in the wild | Tracking | Bows | Simple bow, cloak | Has lived off the wilderness for years |
| 3 | Artisan | Can identify material quality without a test | Smithing | Appraisal | Artisan's tools | Learned a trade from a demanding master |
| 4 | Peasant | +1 extra recovery on a long rest | Survival | Animal Lore | Sickle, plain clothes | Grew up working the land |
| 5 | Scholar | 1x per interlude, resolves a factual question without spending research time | History (or Arcane Theory) | Languages | Personal book | Spent their youth among scrolls |
| 6 | Merchant | Prices with the traveling merchant are 10% better | Trade | Appraisal | Extra coin purse | Grew up among counters and negotiations |
| 7 | Fallen Noble | Has 1 actionable contact of influence (limited use) | Leadership | Diplomacy | Family ring (no commercial value) | Lost a title or inheritance |
| 8 | Criminal | -1 difficulty on Stealth in urban environments | Stealth | Manipulation | Lockpicking tools | Has a past the Guild doesn't know about |
| 9 | Priest | 1x per expedition, performs a small ritual blessing (minor effect) | Religion | Rituals | Sacred symbol | Served a temple before joining the Guild |
| 10 | Sailor | -1 difficulty on Balance/unstable terrain | Swimming | Thrown Weapons | Rope, knife | Spent years on ships |
| 11 | Nomad | Never gets narratively "lost" (always knows the general direction) | Navigation | Survival | Sturdy canteen | Never had a fixed home |
| 12 | Miner | -1 difficulty identifying instabilities in caves and tunnels | Construction | Perception | Pickaxe | Worked in mines before becoming an adventurer |
| 13 | Healer | 1x per expedition, stabilizes a gravely wounded ally without a facility | Medicine | Potions | Basic medical kit | Cared for the sick in a village or troop |
| 14 | Minstrel | -1 difficulty on social tests to get information from strangers | Diplomacy | Manipulation | Simple instrument | Traveled from village to village telling stories |
| 15 | Street Orphan | -1 difficulty on Perception to notice traps/ambushes in enclosed spaces | Perception | Stealth | Small hidden knife | Survived alone on the streets |
| 16 | Exile | Knows 1 rare language/symbol exclusive to their group | Languages | Tracking | None (lost everything) | Was cast out from their homeland for a reason only they know |
| 17 | Former Cultist | Automatically recognizes cult symbols/rituals, without a test | Rituals | Religion | Ceremonial dagger | Left a cult before it was too late |
| 18 | Guild Ward | Receives 5 extra skill points to invest in Dungeonology | Dungeonology | Strategy | Outdated Guild map | Grew up inside the Guild itself, child of a veteran |
| 19 | Bounty Hunter | -1 difficulty on Tracking a specific defined target | Tracking | Intimidation | Shackles, light bow | Made a living capturing fugitives and escaped creatures |
| 20 | Arcane Student | -1 difficulty on the first test of any newly learned spell | Magical Control | Arcane Theory | Incomplete grimoire | Studied magic formally, but never graduated |

### 6.1.3 Background Creation Manual

**Fundamental difference between Background and Origin**: Origin represents ordinary life and grants skill points (15+10). Background represents a **specific event** that changed the character and **never grants skill or attribute points** — avoiding role overlap between the two layers. Background grants situational effects, contacts, specific knowledge, or fragilities.

Mandatory structure of every Background:

1. **Defining Event** — what happened in the past, in a few sentences.
2. **Consequence** — how it changed the character's daily life.
3. **Mechanical Benefit** — a light effect, of the same types allowed for Origin (difficulty reduction in a specific niche; exclusive access to something; a one-off reusable resource).
4. **Complication (mandatory, unlike Origin)** — a narrative and/or mechanical fragility of weight equivalent to the benefit.

Balancing rules:

- **Balance Rule** — Benefit and Complication must have equivalent weight.
- **Non-Duplication Rule** — Background never grants skill/attribute points (that is the exclusive role of Origin, Aptitudes, and natural progression).
- **Living Hook Rule** — every Complication must be something the GM can bring back during the campaign; if it doesn't work as a future narrative hook, it isn't a valid Complication.

**Step by step**: (1) define the defining event; (2) define the consequence in daily life; (3) choose the light Mechanical Benefit; (4) create the Complication of equivalent weight; (5) validate against the Balance Rule and the Non-Duplication Rule.

### 6.1.4 Official List of 20 Backgrounds (CLOSED)

| # | Background | Defining Event | Benefit | Complication |
|---|---|---|---|---|
| 1 | Ruin Survivor | Explored an ancient structure and escaped | -1 difficulty to identify structural risks/collapses | Something from that ruin is still after them |
| 2 | Survived an Ambush | Their previous group was wiped out | 1x per expedition, ignores the Surprised condition | Suffers intense reactions to situations resembling the ambush |
| 3 | Was Imprisoned | Spent time confined, justly or not | Advantage escaping physical restraints (ropes, shackles) | Has a criminal record recognizable by authorities |
| 4 | Served in the Army | Their unit was wiped out in combat | Greater resistance to fear in organized combat | A surviving superior blames them for the defeat |
| 5 | Studied Under a Master | Had a renowned mentor who vanished | Can invoke the master's name to open doors in a specific circle | The master's disappearance hides something dangerous |
| 6 | Lived on the Streets | A period of extreme poverty | Can endure longer without food before suffering penalties | Owes favors to an underworld network |
| 7 | Inherited a Tool | Received a family object with history | The inherited item carries a small extra property | Someone else also wants that object back |
| 8 | Discovered a Manuscript | Found a document they shouldn't have found | Knows a rare fragment of information (a name, symbol, place) | Others know they have the manuscript and are after them |
| 9 | Betrayed by an Ally | Was betrayed by someone they trusted | -1 difficulty to perceive betrayal/lies from close allies | Penalty on social tests to form quick bonds |
| 10 | Saved a Village | A heroic feat publicly recognized | Positive reputation and access to minor favors in the region | The village keeps asking for ongoing help; refusing costs reputation |
| 11 | Lost Someone in the Dungeon | A family member vanished or died on an expedition | -1 difficulty on tests linked to tracking that specific type of danger | Obsession that can lead them into unnecessary risks |
| 12 | Made a Minor Pact | Sealed a small agreement with an entity | Small supernatural benefit (defined with the GM) | The entity will collect something in return, at some point |
| 13 | Survived a Grave Illness | Nearly died of a plague | Increased resistance to disease and poison | Carries a light, permanent physical after-effect |
| 14 | Wrongly Accused | Had their reputation tarnished by a crime they didn't commit | Bonus on Diplomacy when defending against accusations | Still viewed poorly or wanted in a certain place |
| 15 | Keeper of a Secret | Knows something dangerous they shouldn't know | Holds valuable, negotiable information | Others know that they know — which makes them a target |
| 16 | Marked by a Ritual | Went through an incomplete ritual | Slight sensitivity to nearby magical presences | The ritual's mark is noticeable or reacts badly to certain stimuli |
| 17 | Rescued by Strangers | Owes their life to someone they never identified | Has a mysterious contact who can help 1x | Doesn't know who it was — the debt can be called in at any time |
| 18 | Lost Everything in a Disaster | A fire or collapse destroyed their former life | Will bonus against despair and loss | Has no old possessions, contacts, or financial support |
| 19 | Witnessed a Rupture | Saw the world's most feared phenomenon up close | Resistance to panic in the face of dimensional phenomena | Hypervigilance: penalty in environments resembling the event |
| 20 | Raised by the Guild | Grew up inside the institution itself | Bonus on the Guild's internal administrative/bureaucratic tests | Never had a "normal" life: light penalty in social situations outside the Guild |

### 6.1.5 Starting Aptitudes (CLOSED)

Unlike Origin and Background (one-off narrative effects), an Aptitude is purely structural: it eases learning within an entire domain of skills.

**Mechanical effect of each chosen Aptitude**, within its domain:

1. **Ease of Learning** — skills in the domain rise 1 correlation category on the Learning Curve (Low→Medium, Medium→High) when learned from scratch, reducing the initial resistance from §6.4.
2. **Natural Instinct** — **-1 degree of difficulty** on Absolute Tests with skills in the domain while they are still "Untrained" (0 points).

An Aptitude never blocks anything: a character without an Aptitude in Magic can still become a mage, they'll just have a harder initial path (without the two bonuses above).

**Choice rule**: every character chooses exactly **2 Aptitudes** at creation, from the 6 below.

| Aptitude | Skill Areas Covered |
|---|---|
| Combat | Combat — Weapons, Combat — Defense, Unarmed Combat, Ranged Combat |
| Exploration | Exploration |
| Knowledge | Knowledge, Healing |
| Craft | Crafting, Alchemy |
| Magic | Magic |
| Leadership | Social |

Together, the 6 Aptitudes cover exactly the 11 Skill Areas closed in §6.4 — no skill is left without a domain.

**Homebrew Aptitude Manual**: (1) the new domain must be a clear subset of one or more existing Skill Areas, never a new area invented just for the Aptitude; (2) the two effects (Ease of Learning + Natural Instinct) are fixed and cannot be swapped for another effect; (3) if the homebrew domain is narrower than the official ones, it still counts as 1 of the character's 2 choices — it never gains an extra bonus for being smaller.

### 6.1.6 Initial Talent (CLOSED)

"Real" Talents (§6.5) are rare and require Ranking/attribute/skill requirements — they don't make sense as a free menu for a Recruit who doesn't yet have any of that. That's why there is a dedicated sub-list of **Initial Talents (Ranking 0)**: simpler and more generic, meant only for the moment of creation.

**Choice rule**: the player chooses **1 Initial Talent**, with no Ranking/attribute/skill prerequisite. An Initial Talent is never as strong as a Talent earned in play — it always equates to a "minor Talent" on the NP scale (§6.8, value 1).

**Official List of 20 Initial Talents:**

| # | Talent | Category | Effect |
|---|---|---|---|
| 1 | Sure Strike | Combat | 1x per combat, rerolls an attack die they consider bad |
| 2 | Combat Reflexes | Combat | +1 on the first Dodge of each combat |
| 3 | Contained Fury | Combat | 1x per combat, ignores the first light-wound penalty |
| 4 | Nose for Danger | Exploration | -1 difficulty on the first Perception test of each floor |
| 5 | Light Foot | Exploration | Suffers no difficult-terrain penalty when moving alone |
| 6 | Survival Instinct | Exploration | 1x per expedition, avoids running out of a ration/torch for a day |
| 7 | Skilled Hands | Production | Reduces the time of the first crafting project of each interlude by 1 day |
| 8 | Discerning Eye | Production | Automatically identifies an item's Quality upon examining it |
| 9 | Artisan's Precision | Production | 1x per interlude, treats a crafting "Success" result as a "Great Success" |
| 10 | Salvager | Production | Recovers half the materials on a failed crafting attempt |
| 11 | Arcane Glimpse | Arcane | Senses the presence of active magic within a short radius, without spending an action |
| 12 | Ritual Reserve | Arcane | +1 PA available specifically for casting spells, 1x per expedition |
| 13 | Elemental Touch | Arcane | Generates a cosmetic/minimal elemental effect (light, mild heat, breeze) without spending PA |
| 14 | Arcane Memory | Arcane | 1x per research project, reduces the required time by 1 day |
| 15 | Steady Presence | Social | +1 on Intimidation/Leadership tests when outnumbered |
| 16 | Trusted Voice | Social | 1x per interlude, obtains a piece of information from an NPC without needing a test |
| 17 | Natural Diplomat | Social | -1 difficulty on the first Diplomacy test with an unknown faction |
| 18 | Recruit's Luck | Extraordinary | 1x per expedition, turns a (non-critical) Failure into a plain Success |
| 19 | Strange Mark | Extraordinary | A small, unexplained supernatural trait (defined with the GM) — narratively rich, mechanically neutral until investigated in play |
| 20 | Protected Fate | Extraordinary | 1x per entire campaign, survives a blow that would have killed them, becoming Incapacitated instead of dead (effect consumed after use) |

**Homebrew Initial Talent Creation Manual**: (1) it must have a unique, one-off effect (1x per combat/expedition/interlude) or a small fixed bonus (+1) — never a strong continuous effect; (2) it never grants permanent extra PA, an attribute increase, or replaces an entire test without a resource cost; (3) it must fit one of the 6 existing Talent categories (§6.5); (4) weight equivalent to "minor Talent" (NP = 1).

### 6.1.7 Lineages (Races/Species) — CLOSED

Structure of every **Lineage**: (1) **Racial Adjustment** — shifts the **cap** of two attributes (never the 20 points spent at creation): +1 to the maximum allowed for one attribute (from 5 to 6) and −1 to the maximum of another (from 5 to 4); never grants a skill. (2) **1 Racial Trait** — an innate effect, weight equivalent to a minor Talent (NP=1). (3) Narrative data (build, life expectancy) — pure flavor, no mechanical effect.

**Official List of 10 Lineages:**

| Lineage | Racial Adjustment | Racial Trait |
|---|---|---|
| Human | None (all attributes at the standard cap of 5) | Adaptable: can swap 1 Aptitude chosen at creation, 1x during the campaign |
| Dwarf | +1 max. Vigor / −1 max. Control | Resistance to poisons and disease |
| Elf | +1 max. Perception / −1 max. Body | Low-light vision |
| Half-Orc | +1 max. Body / −1 max. Intellect | 1x per expedition, ignores a light-wound penalty |
| Halfling | +1 max. Control / −1 max. Presence | -1 difficulty on Stealth tests |
| Gnome | +1 max. Intellect / −1 max. Vigor | -1 difficulty on the first test of any newly learned Crafting skill |
| Half-Elf | Player freely chooses which attribute gets +1 and which gets −1 | The extra Aptitude can be swapped 1x (versatility) |
| Dragonborn | +1 max. Presence / −1 max. Control | Resistance to one elemental type (chosen at creation) |
| Shadow Descendant | +1 max. Will / −1 max. Presence | Resistance to supernatural fear |
| Fragmented *(rare, requires GM approval)* | +1 max. Affinity / −1 max. Vigor | Senses the proximity of Ruptures and dimensional instability — ties directly into the cosmology (§2) |

**Homebrew Lineage Manual**: (1) the net adjustment is always +1/−1 on a pair of attributes (or 0, like Human); (2) exactly 1 Racial Trait, weight = minor Talent (NP=1); (3) never grants a skill; (4) build/life expectancy are flavor only.

### 6.2 Exploration Contract (Guild–Character relationship)
The Guild provides structure (basic equipment, training, lodging); the adventurer, in exchange, completes expeditions and returns part of the earnings. There is a **Guild Estate** (institutional) separate from the character's **Personal Estate**. Retirement is a possible exit for characters (different from death).

**Formation Debt (CLOSED)**: every new character starts with a fixed debt, equivalent to the cost of the basic equipment + training + lodging provided by the Guild. This debt is automatically deducted from the "Character" share of the Reward Distribution (§10.6) of each expedition, until it's paid off — it never blocks progression, it only temporarily reduces personal earnings in Pact Coins/resources. It closes automatically once paid, with no complex manual tracking.

### 6.3 Attributes
Philosophy: **Attributes = capability ("is he capable?"). Skills = experience ("does he know how?").** Attributes never automatically grant a skill, they only modify efficiency. There is no universal "primary attribute" — all of them must serve some purpose.

Eight attributes, four physical and four mental:

**Physical**

- **Body** — strength, power, carrying capacity, physical impact.
- **Control** — coordination, precision, reflexes, balance.
- **Vigor** — endurance, recovery, stamina, tolerance to exertion.
- **Presence** — posture, imposingness, courage, command of space.

**Mental**

- **Intellect** — logic, learning, memory, analytical reasoning.
- **Perception** — observation, attention, reading the environment.
- **Will** — discipline, self-control, mental resistance.
- **Affinity** — connection to the supernatural, understanding of magic, sensitivity to artifacts and dimensional phenomena (it is not "mana").

Fundamental rules:

1. Attributes never represent training (that is a skill).
2. Attributes modify efficiency, never replace a skill.
3. No attribute grants knowledge automatically.
4. Every skill relates primarily to one attribute, but this relationship can vary by context (e.g., Swords normally uses Control, but can use Body for a brute-force blow).

**Attribute progression is rare** — they only evolve through real physical/mental change (months of conditioning, deep research, extreme trials), never through continuous use in combat. This makes them one of the main mechanisms for controlling power escalation in long campaigns.

**Cost of Progression — Attribute Trial (CLOSED)**: unlike Skill Training (guaranteed daily progress), raising an Attribute requires a **Trial** — a dedicated, thematic Interlude project tied to the specific attribute. Only **1 active Trial at a time** per character (Principle of Imperfect Specialization).

```
Trial Time = Current Grade × 10 days
Resource Cost = Current Grade × 5 (Pact Coins or materials of equivalent value)
```

| From Grade → To Grade | Time | Cost |
|---|---:|---:|
| I → II | 10 days | 5 |
| II → III | 20 days | 10 |
| III → IV | 30 days | 15 |
| IV → V | 40 days | 20 |

Requires a facility of a minimum level corresponding to the attribute, with Level ≥ current Grade. At the end of the time, an Absolute Test (thematic skill) vs. Difficulty **Difficult + (Current Grade × 2)**. Success advances the Grade; Failure consumes the time and half the resources, but doesn't block progress — they can try again (Principle of Failures as Consequence).

**Thematic Trials by Attribute**:

| Attribute | Trial | Test Skill | Minimum Facility |
|---|---|---|---|
| Body | Extreme Endurance (sustained brutal physical labor) | Body (raw) | Training Field |
| Control | Absolute Precision (grueling coordination training) | Primary weapon/style skill | Training Field |
| Vigor | Stamina Trial (supervised exhaustion, controlled fasting) | Survival | Infirmary |
| Presence | Trial of Dominance (facing real fear, commanding under pressure) | Leadership/Intimidation | Military Academy |
| Intellect | Intellectual Trial (solving a real theoretical problem) | Arcane Theory/History | Library |
| Perception | Sensory Trial (extreme meditation, perceptual training) | Perception | Library/Training Field |
| Will | Discipline Trial (fasting, psychological ordeal) | Will (self) | Military Academy |
| Affinity | Arcane Trial (controlled contact with the supernatural) | Magical Control/Rituals | Arcane Laboratory |

Beyond Grade V, the normal Trial process never exceeds the natural cap — it requires Transcendence (blessings, rituals, divine events). Lineages that adjust the maximum cap use the same formula, just with the new cap as the limit.

**Scale**: 0–10. **Modifier = Attribute − 2.** A starting character receives **20 attribute points** (final decision, reduced from an earlier proposal of 28). **Distribution method CLOSED: Free Purchase** — the player distributes the 20 points freely among the 8 attributes, respecting a minimum of **1** and a maximum of **5** per attribute (no mandatory pre-built array).

**Attribute Grades**: there is a Natural Maximum Grade (Grade V); beyond it, only via **Transcendence** (see Global Rule of the Natural Limit) — an extraordinary change that breaks the common human cap (blessings, rituals, divine events).

**Principle of Potential**: attributes define a character's natural limit; skills define how far they've gotten within that limit. (Complementary idea under discussion, not closed: attributes as an "effective ceiling" that limits how much skill training converts into real performance.)

### 6.4 Skills
Knowledge structure in three layers: **Skill Area → Skill → Specialization.**

There is an **official list of Core Skills** (closed, for creation/balancing) plus **Custom Skills** (open, subject to GM validation) — a hybrid system.

Base list by area (with Specializations — the third layer, chosen upon reaching the **Adept, 25 points** milestone; the more specific the specialization, the greater its efficiency within it and the smaller its applicability outside it):

- **Combat — Weapons** *(Control; Body for brute-force blows)*: Swords *(Longsword, Short Sword, Bastard Sword, Rapier)*; Axes *(Battle Axe, Hand Axe, Double Axe)*; Hammers *(War Hammer, Mace, Sledgehammer)*; Spears *(Spear, Halberd, Trident)*; Improvised Weapons *(Environmental Objects, Tools as Weapons)*; Exotic Weapons *(Whips/Chains, Dual Weapons, Articulated Weapons)*.
- **Combat — Defense** *(Control/Vigor)*: Shields *(Small, Large, Tower)*; Armor *(Light, Medium, Heavy)*; Dodge *(Reactive, Acrobatic)*; Block *(With Weapon, Body)*.
- **Unarmed Combat** *(Body/Control)*: Martial Arts *(Fist Style, Kick Style, Mixed Style)*; Unarmed Fighting *(Blunt Strikes, Vital Points)*; Grappling *(Immobilization, Throw/Toss)*.
- **Ranged Combat** *(Control)*: Bows *(Short, Long, Shot on the Move)*; Crossbows *(Light, Heavy)*; Thrown Weapons *(Knives, Hand Axes, Short Spears)*.
- **Exploration** *(Perception/Vigor/Control)*: Perception *(Visual Observation, Hearing, Trap Detection)*; Tracking *(Ground Tracks, Water/Snow Tracks)*; Survival *(Foraging, Wilderness Orientation, Shelter)*; Navigation *(Overland, Underground, Celestial/Maritime)*; Stealth *(Silent Movement, Camouflage)*; Traps *(Detection, Disarming, Rigging)*; Dungeon Exploration *(Structural Reading, Environmental Hazard)*; Climbing *(Rocky Surfaces, Artificial Structures)*; Swimming *(Calm Water, Currents)*.
- **Knowledge** *(Intellect)*: History *(Ancient, Guild, Divine)*; Geography *(Cartography, Wild Regions)*; Creatures *(Beasts, Undead, Extraplanar Entities)*; Religion *(Theology, Religious Rituals)*; Languages *(Common Languages, Ancient Languages, Codes and Ciphers)*; Strategy *(Combat Tactics, Military Logistics)*; Dungeonology *(Floor Structure, Fragment Patterns)*; Animal Lore *(Animal Behavior, Domestication)*; Occultism *(Arcane Symbols, Cults and Sects)*; Appraisal *(of Items, of Materials)*.
- **Healing** *(Intellect/Perception)*: Medicine *(First Aid, Diagnosis, Disease Treatment)*; Surgery *(Invasive Procedures, Corruption Removal)*; Pharmacology *(Remedy Preparation, Dosage)*.
- **Crafting** *(Control/Intellect)*: Smithing *(Weapons, Armor, Tools)*; Carpentry *(Structures, Furniture, Wooden Components)*; Tailoring *(Garments, Light Armor, Accessories)*; Engineering *(Mechanisms, Complex Structures, Mechanical Traps)*; Construction *(Fortifications, Structural Repairs)*; Equipment Making *(Specialized Tools, Utility Items)*; Cooking *(Meal Preparation, Food Preservation)*.
- **Alchemy** *(Intellect)*: Potions *(Healing, Buffs, Utility)*; Poisons *(Contact, Ingestion, Inhalation)*; Materials *(Identification, Extraction, Purification)*; Transmutation *(Metals, Organics, Elements)*.
- **Magic** *(Affinity)*: Magical Control *(Casting Precision, Flow Stability)*; Arcane Theory *(Formula Comprehension, Theoretical Research)*; Rituals *(Binding, Summoning)*; Elemental Affinity *(Fire, Water, Earth, Air)*; Enchantments *(of Weapons, of Items)*.
- **Social** *(Presence/Intellect)*: Diplomacy *(Negotiation, Conflict Mediation)*; Leadership *(Group Command, Motivation)*; Trade *(Price Assessment, Commercial Negotiation)*; Intimidation *(Physical Threat, Psychological Threat)*; Manipulation *(Deceptive Persuasion, Social Disguise)*.

**Starting skills** represent the character's history (tied to Origin), they are not random free points.

**Learning Curve**: learning something new is easier the greater the correlation with knowledge already mastered.

- High Correlation (e.g., Short Sword → Rapier): a large difficulty reduction.
- Medium Correlation (e.g., Sword → Spear): a moderate reduction.
- Low Correlation (e.g., Sword → Magic): little to no reduction.
- Milestone: 0–50 points = "Initial Learning Phase" (greater resistance); after 50, normal progression.

**Skill Milestones** (conceptual scale):

| Points | Grade |
|---|---|
| 0 | Untrained |
| 10 | Basic |
| 25 | Adept |
| 50 | Expert |
| 75 | Master |
| 100 | Legendary |

**Principle of Guaranteed Training**: every day of training generates a **fixed base** amount of progress in the trained skill, regardless of the character's Ranking; facilities, instructors, and the Learning Curve modify this value (never eliminate it).

**Training Points per Day (CLOSED)** — since each real day between sessions equals 1 day of Interlude, the base value needs to be small enough not to trivialize the Skill Milestones:
```
Training Points/day = (1 + Facility Bonus + Instructor Bonus) × Learning Curve Multiplier
```

- **Base**: 1 point/day.
- **Facility Bonus**: `Relevant facility Level × 0.5` ("advanced" facilities dedicated to the domain, such as a Military Academy for Combat, double this bonus: `Level × 1`).
- **Instructor Bonus**: +1 point/day if an Instructor Worker (§10.4) is dedicated to that character/skill.

**Relevant facility by Skill Area** (same mapping as Aptitudes, §6.1.5):

| Skill Area | Facility (normal bonus) | Advanced facility (doubled bonus) |
|---|---|---|
| Combat — Weapons/Defense/Unarmed/Ranged | Training Field | Military Academy |
| Exploration | Training Field (half bonus) | — |
| Knowledge | Library | Archive/Mage Tower (depending on the theme) |
| Healing | Infirmary | — |
| Crafting | Workshop / Smithy (depending on the skill) | Rune Workshop |
| Alchemy | Alchemical Garden (or Workshop, if not yet built) | — |
| Magic | Arcane Laboratory | Mage Tower |
| Social | None (Base + Instructor only) | Military Academy (Leadership only) |

**Learning Curve Multiplier** (returning to §6.4):

| Situation | Multiplier |
|---|---:|
| High Correlation (skill very similar to one already mastered) | ×1.5 |
| Medium Correlation (standard) | ×1.0 |
| Low Correlation (loosely related) | ×0.5 |
| No correlation at all, still in the Initial Learning Phase (0-50 points) | ×0.25 |

High or Medium Correlation skips the extra resistance of the Initial Learning Phase; only skills with no correlation at all suffer the ×0.25 until 50 points.

*Example*: a Recruit training a Medium Correlation skill, at a Level II Training Field (bonus +1), with no instructor: `(1+1+0) × 1.0 = 2 points/day`. A mature Guild, with a Level V Training Field (+2.5) and a dedicated Instructor, training something with High Correlation: `(1+2.5+1) × 1.5 ≈ 6.75 points/day` — institutional investment genuinely speeds up the game, without making training trivial from the start.

**Learning Curve Penalty Table (CLOSED)** — unlike the multiplier above (which governs training *speed*), this is the penalty suffered on **tests** while the skill hasn't yet reached Basic. It extends the same Grade Bonus table already used in Attack/Damage (§7.5):

| Points | Grade | Grade Bonus |
|---|---|---:|
| **0–9** | **Untrained** | **-2** |
| 10–24 | Basic | +0 |
| 25–49 | Adept | +1 |
| 50–74 | Expert | +2 |
| 75–99 | Master | +3 |
| 100+ | Legendary | +4 |

The **-2** applies to any test that uses "Skill Grade Bonus" (Attack, Damage, related Absolute/Opposed Tests). The domain's Aptitude (§6.1.5) already reduces the test's Difficulty by 1 degree while Untrained — the two effects stack, but never fully eliminate the risk of trying something completely new.

**Untrained Training Table (CLOSED)**: while the skill is between 0-9 points, the Facility/Instructor Bonuses from the §6.4 formula are **ignored** — no infrastructure speeds up the raw learning phase. Instead, a fixed cap by Correlation applies:
```
Training Points/day (Untrained) = MIN(§6.4 normal formula, Correlation Cap)
```

| Correlation | Points/day Cap | Days to Basic (10 points) |
|---|---:|---:|
| None | 1 | 10 days |
| Low | 2 | 5 days |
| Medium | 3 | ~4 days |
| High | 5 | 2 days |

As soon as the skill reaches Basic (10+), the cap disappears and the full §6.4 formula (with Facility/Instructor Bonuses) takes effect — institutional investment speeds up the game from that point on, never before.

**Learning Capacity / Mastery** — there is no limit on *knowing* skills, but there is a limit on *maintaining excellence* in many at once. Two sub-limits, calculated from the attributes:

- **Technical Capacity** (tied to physical attributes) — how many physical areas the character can master well.
- **Intellectual Capacity** (tied to mental attributes) — how many mental areas they can master well.

**Principle of Imperfect Specialization**: all knowledge can be acquired, but excellence requires dedication — someone who tries to do everything is unlikely to be the best at anything.

### 6.5 Talents
They don't have levels (they're binary: you have it or you don't). Categories: Combat, Arcane, Exploration, Production, Social, Extraordinary. They have a mandatory origin (they don't appear "for no reason"), requirements (Ranking, minimum attribute, minimum skill, narrative event), and can generate synergies with each other. **Principle of Singularity**: talents must be rare and meaningful, never a generic list every character accumulates the same way.

### 6.6 Magic and Martial Techniques (CLOSED)

#### 6.6.1 Schools of Magic (official list — 8 schools)

| School | Focus |
|---|---|
| Evocation | Direct damage, energy, elements |
| Abjuration | Protection, shields, resistances |
| Control | Debuffs, immobilization, area control |
| Conjuration | Summoning creatures/objects |
| Transmutation | Altering form/matter (arcane version, distinct from alchemical Transmutation) |
| Illusion | Deceiving the senses, disguises |
| Necromancy | Manipulating life/death, drain, corruption |
| Divination | Information, detection, precognition |

#### 6.6.2 Structure of an Individual Spell
Every spell is defined by: Name, School, Cost (PA), Range (reuses the combat Zones, §7.1), Area (Single Target / Small Area / Large Area / Line), Duration (Instantaneous / Turns / Scene / Persistent), Test (Opposed vs. the target's Will/Affinity, or Absolute against a fixed difficulty if there's no active resistance), Effect.

#### 6.6.3 Cost and Reduction by Domain

| Complexity | PA | Note |
|---|---:|---|
| Minor | 1 | light effect (small damage/healing, utility) |
| Moderate | 2 | standard effect |
| Major | 3 | strong effect |
| Supreme | Extended Casting (multiple turns) | effects that change the course of an encounter/floor |

**Reduction by Magical Control Grade**: Basic +0 | Adept +0 | Expert −1 PA (min. 1) | Master −1 PA and −1 Extended Casting Turn | Legendary −2 PA (min. 1) and −1 Turn.

**Interruption**: during Extended Casting, taking damage or failing a Will Test (Absolute, difficulty = damage taken) interrupts the spell — PA already spent is lost.

#### 6.6.4 Creating New Spells
Via Arcane Research (§11.2): a project with time by complexity (Minor 5 days | Moderate 10 | Major 20 | Supreme 40+, requiring a Divine Forge/Laboratory), finalized by an Absolute Test (Arcane Theory) that fixes the spell's definitive structure. **Grimoires** physically store known spells, but losing the grimoire doesn't erase the knowledge already learned — a learned spell is permanent (Principle of Knowledge Persistence, §11.3).

**Homebrew Spell Creation Manual** — step by step: (1) choose the School (§6.6.1), which sets the "flavor" of the effect; (2) choose the Complexity (Minor/Moderate/Major/Supreme, §6.6.3), which already fixes the PA Cost and the power ceiling; (3) define the Range (Zone); (4) define the Area (Single Target/Small Area/Large Area/Line); (5) define the Duration (Instantaneous/Turns/Scene/Persistent); (6) define the Test (Opposed or Absolute); (7) define the Single Effect, written in terms of already-existing mechanics (damage equivalent to a weapon category, an applied Condition, a bonus/penalty to Passive Defense or a test); (8) validate against the checklist below.

**Balancing Checklist**: **Scaling Rule** — increasing Area, Duration, or Range beyond that Complexity's standard costs +1 extra PA or forces a Complexity increase; **Single Effect Rule** — a spell does one well-defined thing, combining multiple strong effects requires Major/Supreme Complexity or must become two spells; **Origin of Knowledge Rule** — no spell arises without Arcane Research, a Grimoire, a master, or a documented ritual; **Symmetry Rule** — creatures that use magic follow the same parameters, any exception must come from a Unique Trait on the creature's sheet (§9.5), never "because it's a monster."

#### 6.6.5 Enchanting Items and Rituals
**Enchantment**: when crafting/modifying an item (§6.7.4/§6.7.5), adding a magical Property requires an additional Absolute Test (Enchantments), at the minimum facility of a Mage Tower or Arcane Laboratory.

**Rituals**: unlike combat spells, these use Exploration Turns (§8.1, not PA), allow effects too large for combat (bigger summonings, seals, contact with entities). They require a Test (Rituals), time in Turns, materials/catalysts, and can involve multiple participants contributing Will or Affinity. Failing a Ritual is more dangerous than failing an ordinary spell (risk of a reversed/backfire effect — Principle of Arcane Complexity).

#### 6.6.6 Example Spells (starting point — 1 per School, with a Minor → Moderate → Major progression)

| School | Minor (1 PA) | Moderate (2 PA) | Major (3 PA) |
|---|---|---|---|
| Evocation | **Fire Lance** — 1 target, Contact/Short, instant fire damage | **Flaming Blast** — line, Medium, greater damage + light ignition | **Flame Storm** — small area, continuous damage over 2 turns |
| Abjuration | **Arcane Shield** — +2 Passive Defense, 1 turn | **Protective Barrier** — +4 Passive Defense, Scene, self only | **Absolute Wall** — +4 Passive Defense to a small area (allies), Scene |
| Control | **Bonds of Will** — Immobilizes 1 target, 1 turn | **Arcane Shackles** — Immobilizes + Weakened, 2 turns | **Prison of Will** — Immobilizes a small area, Scene |
| Conjuration | **Spectral Blade** — summons a temporary weapon (1 turn) | **Battle Familiar** — summons a small creature, Scene | **Summoned Avatar** — summons a powerful ally, Scene, Extended Casting |
| Transmutation | **Warping Touch** — alters a small surface/object | **Partial Metamorphosis** — alters part of one's own body, utility gain, Scene | **Complete Transfiguration** — fully alters one's form, Scene |
| Illusion | **Deceptive Mist** — camouflages 1 target, +Stealth | **Illusory Duplicate** — false image, foils 1 attack | **Veil of Lies** — deceives an entire group/area, Scene |
| Necromancy | **Enfeebling Touch** — small drain of HP/Vigor | **Shadow Breath** — drain in a small area | **Call of the Grave** — summons minor temporary undead, Extended Casting |
| Divination | **Glimpse** — reveals 1 simple piece of information about a target/environment | **Reading the Thread of Fate** — predicts a target's next action, grants Advantage | **All-Seeing Eye** — reveals the map/secrets of an entire area, Scene |

#### 6.6.7 Technique Tree by Style
Each major combat group has its own tree, in the 4 categories already established:

- **Stances** — passive, activated at the start of the turn for 1 PA, maintained afterward at no cost.
- **Techniques** — active, cost 1-2 PA (can evolve from Technique I to Technique II with more PA/effect).
- **Reactions** — use the turn's Reaction (§7.3), defensive/counter-attack effects.
- **Supreme Techniques** — cost the turn's full 3 PA, limited use (1x per combat or expedition).

**Formal requirements by category:**

| Category | Minimum weapon/style skill | Minimum Ranking |
|---|---|---|
| Stance | Adept (25) | — |
| Technique | Expert (50) | — |
| Reaction | Expert (50) | — |
| Supreme Technique | Master (75) | Silver+ |

**Creating New Techniques**: via a "Technique Project" during the Interlude — time by category (Stance 5 days | Technique 10 | Reaction 10 | Supreme 25), finalized by an Absolute Test in the corresponding weapon/style skill. **Variations**: a base technique can gain situational variations (same main effect, different context) unlocked via high Specialization correlation (§6.4).

**Homebrew Technique Creation Manual** — step by step: (1) choose the base Style/Weapon, which must correspond to a skill already existing in §6.4; (2) choose the Category (Stance/Technique/Reaction/Supreme), which already fixes the PA cost and the minimum required skill (table above); (3) define the Effect, always referencing existing mechanics (Passive Defense or damage bonus/penalty, an applied Condition, affected range/area, use of the Reaction); (4) validate against the checklist below.

**Balancing Checklist**: **Single Effect Rule** — same rule as Magic, a technique does one well-defined thing; **Progression Rule** — if the technique has stages (Technique I → II), stage II always requires Master skill and costs +1 more PA than stage I; **Rare Supremacy Rule** — Supreme Techniques are always limited to 1x per combat or expedition, never free use; **Weapon Compatibility Rule** — the technique only works with the corresponding weapon/style category, it's never generic across different styles.

#### 6.6.8 Example Techniques (starting point — 3 styles)

**Swords**

| Category | Technique | Effect |
|---|---|---|
| Stance | Offensive Stance | 1 PA, +1 damage / −1 Passive Defense while maintained |
| Technique I → II | Spinning Strike | I (1 PA): hits 2 targets in Contact → II (2 PA, Master): hits everyone in Contact |
| Reaction | Parry | Reaction, +Passive Defense against 1 attack; if it succeeds, allows a counter-attack with reduced damage |
| Supreme | The Veil-Splitting Cut | 3 PA, 1x/combat: ignores half the armor's Damage Reduction and applies Bleeding |

**Unarmed Combat**

| Category | Technique | Effect |
|---|---|---|
| Stance | Closed Guard | 1 PA, +2 Passive Defense / −1 damage while maintained |
| Technique I → II | Joint Strike | I (1 PA): attack with a chance of light Stun → II (2 PA, Master): greater chance/effect |
| Reaction | Counterstrike | Reaction, if the Active Defense succeeds, deals immediate damage to the attacker |
| Supreme | Rupture of Vital Points | 3 PA, 1x/combat: fully ignores the armor's Damage Reduction, applies Gravely Wounded |

**Bows (Ranged)**

| Category | Technique | Effect |
|---|---|---|
| Stance | Calculated Aim | 1 PA, +1 accuracy against a marked target, maintained until switching targets |
| Technique I → II | Chained Shot | I (2 PA): hits 2 targets in the same line → II (3 PA, Master): hits up to 4 targets |
| Reaction | Interception Shot | Reaction, attacks an enemy entering the Short Zone |
| Supreme | The Veil-Piercing Arrow | 3 PA, 1x/combat: ignores Cover (Partial/Total) and the armor's Damage Reduction |



#### 6.6.9 Starting Spells and Techniques (CLOSED)

Under the normal rules, learning a Stance/Technique requires Adept Skill (25 points) and spells require an Arcane Research project — but Origin only grants 15 points in the primary skill. Without a specific rule, no character would start with any usable spell or technique.

**Rule of Inherited Knowledge**: character creation grants a fixed, small package of spells/techniques, representing incomplete training brought over from before the Guild — this knowledge **ignores the normal minimum-skill requirement** (it's prior baggage, not field experience). Using them still costs PA normally — the rule only frees up the *knowledge*, never the *cost of use* (the Golden Rule remains intact).

- **Aptitude in Magic** (§6.1.5) → knows **2 Minor Complexity Spells** (from the §6.6.6 list or approved homebrew, pre-approved before the campaign). If the Origin is also arcane (e.g., Arcane Student, §6.1.2) → +1 extra (3 total).
- **Aptitude in Combat** (§6.1.5) → knows **1 Stance + 1 Technique (stage I)**, from a style compatible with the Origin's primary Skill.
- With neither of these Aptitudes, but still wanting 1 one-off spell/technique → **swap the Initial Talent (§6.1.6)** for 1 Minor Spell or 1 basic Technique/Stance.

#### 6.6.10 Intuitive Magic (Free-Form Magic) — CLOSED

A character with at least 1 point in Magical Control can attempt to produce, on the spot, a magical effect they don't know as a formal spell — as long as it logically fits a School in which they have practiced Affinity.

- **Cost**: always **+1 extra PA** over the equivalent Complexity estimated by the GM (lack of structure of an improvised spell).
- **Double Test**: besides the normal effect test (if there's a target/resistance), the player makes an **additional Absolute Test of Magical Control**, difficulty set by the estimated Complexity — representing "assembling" the spell on the spot.
- **Failure** = PA consumed, no effect. **Critical Failure** = Reverse Interruption — the character suffers a light Condition, or damage equal to the estimated Complexity, or generates a spike in Arcane/Divine Tension (§12), at the GM's discretion.
- **Limit**: never reproduces a Supreme Complexity effect; never creates a permanent physical item, only scene/instantaneous effects.
- **Positive consequence**: if used successfully, the GM can formalize it as a **Discovered Spell** — it becomes officially known at no extra Research cost, and can later be refined via Arcane Research (§6.6.4) to bring its cost down to the standard. This reinforces the "learning by doing" that already runs through the system.

### 6.7 Equipment and Crafting (CLOSED)

Philosophy: equipment should expand possibilities, not just numbers (**Principle of Equipment Identity**). Structure in four pillars: Quality, Material, Construction, Modifications (**Principle of Modularity**). They carry knowledge/history (**Principle of Material Legacy** — an item can "teach" something to whoever studies it).

**6.7.1 Rarity** (already used in NP, §6.8, now with full mechanical effect):

| Rarity | Max Properties | Base Bonus (Attack/Damage/Defense) | NP |
|---|---|---:|---:|
| Common | 0 | +0 | 1 |
| Uncommon | 1 | +1 | 3 |
| Rare | 2 | +2 | 7 |
| Epic | 3 | +3 | 15 |
| Legendary | 4 | +4 | 30 |
| Divine | 5+ | +5 or unique effect | 50+ |

**6.7.2 Categories**: Weapons, Armor, Shields, Tools, Consumables, Artifacts, Relics.

**6.7.3 Properties and Enchantments** (closed list of 20 — each occupies 1 Property slot, up to the Rarity's cap):

Sharp (+1 damage die) · Precise (-1 difficulty on the attack) · Sturdy (+2 Wear Hits, §6.7.6) · Light (reduces weight) · Flaming / Frost / Corrosive (extra elemental damage) · Piercing (ignores part of the armor's Damage Reduction) · Vampiric (heals a fraction of the damage dealt) · Resonant (casting bonus, arcane items only) · Camouflaged (Stealth bonus) · Warded (resistance to 1 specific Condition) · Unstable (strong effect, but with a chance of failure/backfire) · Regenerative (armor recovers 1 extra HP per Short Rest) · Silent (Stealth bonus while moving with the item equipped) · Anchored (the weapon cannot be disarmed) · Adaptive (switches between 2 damage categories at no action cost) · Amplifying (+1 zone to the casting range of arcane items) · Shattering (light damage in a small area around the target) · Sealing (reduces the chance of generating the Bleeding Condition) · Cursed (strong effect with a fixed, always-active penalty, defined at creation).

**Homebrew Property Manual**: (1) **Single Slot Rule** — every property occupies exactly 1 slot, regardless of how "good" it seems; (2) **Calibrated Weight Rule** — the effect must be equivalent to one of these, never more: +1 damage die, -1 degree of difficulty in a specific niche, a one-off reusable resource (1x/expedition or interlude), or resistance/immunity to 1 Condition; (3) **Compatibility Rule** — it needs to make physical sense with the item's category; (4) **Trade-off Rule** — very strong properties (equivalent to Unstable/Cursed) require an always-active penalty or a real chance of a reversed effect.

**6.7.4 Crafting**:
```
Absolute Test (Crafting Skill) vs. Recipe Difficulty
```

| Target Rarity | Difficulty | Time | Material Cost (CLOSED) | Minimum Facility |
|---|---|---|---:|---|
| Common | Trivial/Easy | 1 day | 5 | Basic Workshop |
| Uncommon | Easy/Moderate | 3 days | 15 | Basic Workshop |
| Rare | Moderate/Difficult | 7 days | 35 | Smithy |
| Epic | Difficult/Very Difficult | 14 days | 75 | Advanced Smithy |
| Legendary | Very Difficult/Heroic | 30 days | 150 | Rune Forge |
| Divine | Legendary (requires a Pact Coin) | Requires a prior Research project | 250 + 10 Pact Coins | Divine Forge |

Result by Success Margin (§5.4): Success = standard item of the target rarity | Great Success = +1 extra Property (without exceeding the rarity cap) | Extraordinary Success = time cut in half or a reusable bonus material | Failure = half the materials lost | Critical Failure = materials completely lost (risk of damage to the facility/tool).

**6.7.5 Upgrading, Modification, and Reconstruction**: **Upgrading** strengthens the Base Bonus within the same rarity (up to its cap); **Modification** swaps 1 existing Property for another of equivalent cost; **Reconstruction** raises the item to the next rarity — same material cost as crafting from scratch, but with half the time.

**6.7.6 Durability — Wear Hits**: instead of "item HP," an item loses 1 Wear Hit only on a **Critical Failure** of an attack/defense using it, or on a specific narrative event (trap, corrosion, extreme environment).

| Rarity | Wear Hits until maintenance is needed |
|---|---:|
| Common | 3 |
| Uncommon | 4 |
| Rare | 5 |
| Epic | 6 |
| Legendary | 8 |
| Divine | 10 |

Once exhausted, the item becomes **Damaged** (-1 to the Base Bonus) until repaired during the Interlude (corresponding Crafting Skill, short time). The Sturdy Property grants +2 extra Wear Hits.

**6.7.7 Complete Item Creation Guide**

General step by step (applies to any item origin): (1) Category; (2) Target Rarity (§6.7.1, already sets the Property cap and Base Bonus); (3) Properties up to the cap, official or validated homebrew; (4) Wear Hits (§6.7.6, +2 if Sturdy); (5) if weapon/armor, damage/protection category (Light/Medium/Heavy/Two-Handed, §7.5); (6) Narrative Hook (recommended for Rare+, tied to the Principle of Material Legacy); (7) final validation — does the item expand possibilities, or is it just a bigger number?

- **Path A — GM creating an item for the Dungeon (loot/reward)**: the item is born ready-made, without going through the Crafting Test. If it's a Strategic Asset (§9.10), assign its Strategic Value (1-5). Unique Legendary/Divine items must have a **Material Complication** (equivalent to a Background's Complication, §6.1.3) — every extraordinary item carries a weight or consequence.
- **Path B — Player crafting via Crafting (Interlude)**: follows §6.7.4, at the required minimum facility. Requires having the Known Recipe or a Discovered Project (§11.2) — it's not possible to craft a rarity without first having the corresponding recipe.

### 6.8 Power Level — final approved formula
After testing a multiplicative formula (which over-inflated specialization), the final approved version uses a **weighted sum**:

```
NP = Base Power + Specialization Power + Equipment

Base Power = Attributes + Skills

Specialization Power = Talents + Abilities
  Minor talent = 1 | medium = 3 | major = 5
  Common ability = 5 | advanced = 10 | supreme = 20

Equipment:
  Common = 1 | Uncommon = 3 | Rare = 7 | Epic = 15 | Legendary = 30 | Divine = 50+
```

**Official NP ranges by Ranking (CLOSED)** — simulated across the 8 Rankings, with ±15% acceptable individual variation:

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

The increment between neighboring Rankings varies between +25 and +75 points — never doubling from one rank to the next. This growth is deliberately **smooth and predictable** (never exponential): even at the top, most of the NP comes from Skills and Equipment (which have a natural cap via Grade V / max rarity), not from an unchecked multiplier, so the die (2d10) stays relevant at any Ranking. This table connects directly to the Threat Budget (§9.9): the GM uses the party's average Ranking as an immediate reference for which floor range it's calibrated for.

### 6.9 Death, Legacy, and Memory Crystals

- Death is permanent and has no narrative protection. The player (Patron) creates another character.
- Upon dying, the character **drops a Memory Crystal** — a "black box" that can only be accessed at the **Memorial**. Accessing memories costs time and doesn't automatically transmit attributes or skills — only concrete knowledge lived by the character (maps, languages, known traps, puzzle solutions).
- Three levels of memory recovery: **Record** (simple facts) → **Technique** (procedures/methods) → **Full Memory** (most complete, most expensive in time).
- **Recruitment Level / Formation Capacity (CF)**: a new character never starts from absolute zero — the Guild has already evolved, so they receive training compatible with current infrastructure (starting attributes, starting skills, available talents, provided equipment, basic techniques). They never reach the average level of the veterans, but they also never fall too far behind — avoiding both trivializing death and over-punishing whoever lost a character.

---

## 7. Combat (CLOSED)

### 7.1 Movement — Hybrid System
The Encounter Scale (§9.7) defines the movement mode:

- **Small Scale** (Individual/Tactical Command) → **Grid/Hex**, measured in **squares** (square grid or hex grid are interchangeable, mechanically identical).
- **Large Scale** (Hordes, Military/Strategic Command) → **Zones** (Contact/Short/Medium/Long), 1 PA per adjacent zone.

**Movement (Grid mode)**: `Movement = 4 + Mod(Control)` squares per PA spent Moving.

**Range conversion table** (unifies the two modes):

| Zone | Grid/Hex (squares) | Range penalty |
|---|---|---|
| Contact | 0–1 | Ranged weapons suffer a large penalty |
| Short | 2–6 | Ideal range for most bows/crossbows |
| Medium | 7–12 | -1 additional degree of difficulty |
| Long | 13+ | -2 additional degrees of difficulty |

Cover (valid in both modes): **Light** (+2 Passive Defense) | **Partial** (+4 Passive Defense, half damage if hit) | **Total** (impossible to hit with a direct attack).

### 7.2 Initiative
`Initiative = 2d10 + Mod(Control)`. Descending order; ties resolved by higher Perception.

### 7.3 Actions and Action Points
**3 PA per turn** (base value) + **1 Reaction**. PA only increases through rare talents, equipment, special skills, or divine powers — attributes never increase PA directly. Actions: Move (1 PA/zone or up to the Movement value in squares), Attack (1-2 PA depending on weapon category), Defend (1 PA, triggers Dodge or Block — see §7.4.1), Use Item (1 PA), Ready Action (holds PA to react to a trigger).

**Opportunity Attacks**: don't exist as their own mechanic — they're covered by the existing Reaction ("Interception": a character can use their Reaction when an enemy leaves their Contact Zone/square without disengaging carefully).

### 7.4 Hybrid Defense
Combat is an Opposed Test by definition (§5.2), but by default this would make every attack slow. That's why:

- **Passive Defense** (default, costs no PA) — the attacker must beat it, functioning in practice like an Absolute Test:
```
Passive Defense = 10 + Mod(Control) + Equipment Base Bonus (armor, §6.7.1) + Equipment Base Bonus (shield, if equipped)
```

- **Active Defense** (optional) — the defender spends 1 PA (the Defend action) or their Reaction, and the attack becomes a true Opposed Test. Upon activating it, the defender chooses between **Dodge** or **Block** (§7.4.1) — the two skills that power this mechanism, with different attributes and effects.

This resolves the tension between "fast combat" and "combat is an Opposed Test": fast by default, tactical when the player invests a resource.

#### 7.4.1 Dodge (Control) vs. Block (Vigor) — CLOSED

Unlike what the name suggests at first glance, Dodge and Block aren't the same thing with different labels — they're two mechanically distinct ways to execute Active Defense, each tied to a different attribute:

**Dodge** — reflex, agility:
```
Roll = 2d10 + Attribute Grade Bonus (Control) + Dodge Grade Bonus
```
- Success → avoids the attack **entirely** (zero damage). Failure → takes full damage, no reduction.
- Requires no equipment (uses only the body).
- Doesn't work against Large Area attacks, or if the defender is Immobilized.
- Characters with high Control tend to be better at Dodge.

**Block** — resistance, physical:
```
Roll = 2d10 + Attribute Grade Bonus (Vigor) + Block Grade Bonus
```
- Success → reduces damage by `Grade Bonus × 2` (adds to the armor's Damage Reduction, §7.5).
- Great Success (margin ≥5) → damage reduced to zero.
- Extraordinary Success (margin ≥10) → zero damage **+** unlocks an immediate counter-attack, if the defender still has their Reaction available.
- Requires a weapon or shield equipped.
- Works even against Large Area attacks (the defender doesn't need to move, just react in place).
- Characters with high Vigor tend to be better at Block.

Neither is objectively superior — Dodge bets everything (avoids completely or doesn't avoid at all); Block is more predictable (guaranteed partial mitigation on a simple success, with the upside of zeroing damage and counter-attacking at high margins). The choice depends on the rest of the character's build.

The **Parry** Technique (§6.6.8, Swords) still exists as a formal upgrade over Block — uses the same logic, but requires the Expert Skill and comes with an additional effect.

### 7.5 Attack and Damage (CORRECTED by Playtest, §17.10)
```
Attack = 2d10 + Attribute Grade Bonus + Skill Grade Bonus  →  vs. Passive Defense (or Opposed Test if actively defended)
  Attribute Grade Bonus = Attribute (score) − 1   [Grade I=+0 | II=+1 | III=+2 | IV=+3 | V=+4; beyond V, only via Transcendence, §6.3]
  Skill Grade Bonus  = Basic +0 | Adept +1 | Expert +2 | Master +3 | Legendary +4

Damage = Weapon category die + Mod(Attribute) + Skill Grade Bonus + Equipment Base Bonus (weapon, §6.7.1)
  Light Weapons: 1d6 | Medium: 1d8 | Heavy: 1d10 | Two-Handed: 2d6

Armor Damage Reduction: Light -1 | Medium -2 | Heavy -3 (a minimum of 1 damage always gets through)
```
**Balancing errata (Playtest, §17.10)**: the original version of this formula added the weapon's Skill as a raw value to Attack — since Skill grows up to 100-200+ points, this made the 2d10 irrelevant from Steel/Silver onward and broke the scale between Rankings. The corrected version uses the **Grade Bonus** (of both the Attribute and the Skill) on both sides of the formula, keeping the die always relevant. The absence of the **Equipment Base Bonus** (§6.7.1) in Damage and Passive Defense was also corrected — previously, better equipment had no effect on combat's actual math. By design decision, **Equipment never factors into Attack** (it shouldn't influence hit rate, only Damage and Defense).

The Success Margin (§5.4) modifies the result: Success = normal damage | Great Success = +1 extra damage die | Extraordinary Success = +2 extra damage dice.

### 7.6 Hit Points and Recovery
```
PV = 10 + (Vigor × 2) + Ranking Bonus
Ranking Bonus: Bronze +0 | Iron +5 | Steel +10 | Silver +15 | Gold +20 | Mithril +25 | Adamant +30 | Legendary +35
```
Natural recovery only occurs during the Interlude (via §11/Healing); inside the Dungeon, a short rest recovers only a small fraction (at the GM's discretion based on the floor's Pressure) — this reinforces the lethality.

### 7.7 Conditions (closed list)
Lightly Wounded, Gravely Wounded, Bleeding, Stunned, Weakened, Frightened, Immobilized, Dying, Dead.

### 7.8 Death
Upon reaching 0 PV, the character becomes **Dying** (unconscious; Medicine tests can stabilize them, 1 PA). Any additional damage taken while Dying causes **instant death**. This preserves "no mercy, no narrative protection" (§1) while still giving a real window for allies to act. Upon death, the Memory Crystal is activated (§6.9).

### 7.9 Threat Index
Relates the Power Level of the Dungeon/enemies to that of the party — see §9.8/§9.9 for the final, expanded version as the Encounter System and Threat Budget.

---

## 8. Exploration (CLOSED)

### 8.1 Exploration Turn
Outside of combat, the Dungeon advances in **Exploration Turns = 10 minutes** each. In 1 turn the party can: move between points of interest, search for traps/secrets, rest briefly, or perform a longer skill action. Resource consumption (torches, food/water pace) is counted per Exploration Turn — PA remains exclusive to combat/quick actions.

### 8.2 Vision and Lighting

| Condition | Effect |
|---|---|
| Lit | No penalty |
| Dim | -1 degree of difficulty on visual tests and ranged attacks |
| Total Darkness | Visual tests impossible without a special sense; movement reduced by half |

Light sources: **Torch** (Short radius, lasts 6 Exploration Turns = 1 hour) | **Magic Light** (Medium radius, duration tied to PA/spell cost, §6.6) | natural ambient light (varies by Arc/floor).

### 8.3 Navigation and Maps
The Dungeon Exploration/Navigation skill maintains the route and avoids getting lost. Critical failure = the group gets lost (spends 1 extra Turn, risk of an event/encounter). Physical maps and maps obtained as a Strategic Asset (§9.10) reduce navigation difficulty on floors already partially mapped — reinforcing that Information is a Resource (§9.4).

### 8.4 Traps

- **Detection**: Absolute Test (Perception/Traps) vs. difficulty tied to the floor's Ranking (§5.3).
- **Disarming**: Absolute Test (Traps); failure can trigger the trap.
- **Damage**: follows the same logic as §7.5 (Light/Medium/Heavy), possibly including additional effects (poison, a Condition).
- Failure never blocks exploration — it generates a consequence (Principle of Failures as Consequence, §1).

### 8.5 Group Exploration
Suggested roles (a guide, not mandatory): Scout (perception/stealth up front), Bodyguard (rearguard), Navigator (keeps the route), Specialist (traps/puzzles). The group can split into subgroups to act in parallel during the same Turn — but each separated subgroup locally reduces the Party Power (§9.8) if an encounter occurs.

### 8.6 Camping and Rest

- **Short Rest** (1 dedicated Turn): recovers a small fraction of PV (same rule as §7.6), allows reorganizing equipment.
- **Full Camp** (a larger block of time, consuming food/water): recovers more PV, but requires a location with no active Pressure (§9.2) and always carries a risk of an event depending on the floor's Pressure.
- Resting always costs time — it's never "free" (Golden Rule, §1).

### 8.7 Resource Consumption

| Resource | Consumption |
|---|---|
| Food | 1 ration/character per day of Dungeon Time |
| Water | 1 canteen/character per day (doubles in arid environments) |
| Torch | 1 unit per 6 Exploration Turns |
| Rope | Consumed per specific use (climbing, pits) |
| Ammunition | 1 unit per ranged attack made |
| Carrying Capacity | `Body × 5` (weight); exceeding it generates a Movement penalty and physical test penalties |

Lack of food/water for consecutive days generates the **Hungry/Dehydrated** Conditions (increasing penalties, never direct death — a consequence, not a block).

---

## 9. The Dungeon

### 9.1 Floor Structure
Every floor has: Identity (biome/theme inherited from the source fragment-universe), Main Objective, Secondary Objectives, Failure Condition. Types: Exploration, Defense, Attack, Hunt (among others already listed in §4.2).

### 9.2 Dungeon Pressure (CLOSED by End-to-End Test, §17.10)
State scale: **Stable → Aggravated → Critical → Collapse.** Represents a floor's growing urgency/deterioration/corruption; it feeds events, penalties, and environmental changes (**Principle of Thematic Pressure** — each floor type "presses" in a way coherent with its theme: a living forest grows and suffocates, a volcanic floor increases the heat, a fortress reinforces its defenses, etc.).

**Numeric counter**: each floor has a Pressure counter from **0 to 100**, which **resets on every new floor** (narrative consequences of having reached Critical/Collapse can echo into the next floor, at the GM's discretion, but the counter itself doesn't carry over between floors).

| State | Range | Multiplier on remaining encounters' PE |
|---|---:|---:|
| Stable | 0–24 | ×1.00 |
| Aggravated | 25–59 | ×1.10 |
| Critical | 60–89 | ×1.25 |
| Collapse | 90–100 | ×1.50 + automatically triggers a Collapse Event (defined by the GM: enemy reinforcements, drastic environmental change, or an immediate risk to the floor's Failure Condition) |

**Standard sources of Pressure** (the GM adds these up as the narrative demands; the list is a starting point, not a rigid table):

- Each Exploration Turn beyond what the floor's Duration allows for (§9.9): **+5**.
- Each completed combat (noise, tracks, the Dungeon's attention): **+10**.
- Critical failure on a relevant test (a trap triggered, an alarm, a serious mistake): **+15**.
- A specific narrative event defined by the GM (e.g., the horde notices the players): **+20 to +60**, depending on impact.

The Pressure multiplier adds to the Terrain/Intelligence/Objective multipliers already present in the Encounter Power formula (§9.8), mechanically reinforcing that the Dungeon reacts to the players' presence instead of waiting motionless.

### 9.3 Floor States
Unexplored → Explored → Conquered → Dominated.

### 9.4 Rewards and Information
Rewards: Knowledge, Resources, Progress. **Information is treated as a concrete resource** — knowing a boss beforehand should give a real advantage, equivalent to raw power.

### 9.5 Creatures (CLOSED)

**9.5.1 Types** (nature/origin — closed list, 8 types): Beasts · Undead · Aberrations · Spirits · Constructs · Corrupted Humanoids · Draconic · Extraplanar Entities. *(Type describes nature; Function, below, describes the role in the Dungeon — the two combine freely: a Guardian can be a Construct or Undead, for example.)*

**9.5.2 Function in the Dungeon**: Predator, Guardian, Soldier, Parasite, Living Event.

**9.5.3 Behavior (AI)** — links the narrative category to the Encounter System's Intelligence multiplier (§9.8):

| Behavior | Equivalent Multiplier | GM Action Rule |
|---|---|---|
| Instinctive | Instinct (×1) | Always attacks the nearest target or the one with the lowest Passive Defense; never uses group tactics; flees when PV < 25% or Morale drops |
| Intelligent | Tactical (×1.2) or Military (×1.5) | Chooses a target based on perceived threat; can retreat 1 zone to reposition; uses the Reaction optimally |
| Strategic | Genius (×2) | Coordinates with other creatures in the group, avoiding overlapping targets and aiming at known weaknesses; can feign retreat; actively uses terrain/Cover; prioritizes eliminating support/casters first |

**9.5.4 Natural Characteristics Table** (NP cost, same weighting logic as Talents/Abilities, §6.8):

| Weight | NP | Examples |
|---|---:|---|
| Minor | 1 | Darkvision, Keen Smell, Seismic Sense, Resistance to 1 element |
| Medium | 3 | Carapace (+2 Damage Reduction), Flight, Natural Camouflage, Multiple Eyes (immune to Surprised) |
| Major | 5 | Regeneration (recovers PV/turn), Potent Poison (automatic Condition on hit), Multiple Attacks (+1 attack/turn at no extra PA cost) |
| Supreme | 10 | Metamorphosis, Dimensional Core (revives 1x when destroyed), Immunity to an entire damage category |

**9.5.5 Creature NP Formula**:
```
NP(creature) = (Attributes + Natural Skills) + Σ Characteristics + Σ Abilities + Equipment
```
(same logic as §6.8; Ability common=5/advanced=10/supreme=20)

**9.5.6 Creature Categories** (mapped to the Ranking ranges, §6.8, Principle of Symmetry):

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

**9.5.7 Simplified Creature Sheet** (table format, quick to use):
```
NAME (Type — Function — Category: NP XX)
Behavior: Instinctive / Intelligent / Strategic
PV: XX | Passive Defense: XX | Movement: XX
Main attack: 2d10 + X vs. Defense | Damage: XdX+X
Characteristics: [brief list, 1 line each]
Abilities: [brief list]
Weakness: [1 line]
Rewards: [brief list — Materials/Knowledge/Techniques/Crystals, §9.4]
```

**9.5.8 Creature Creation Manual** — step by step: (1) Concept (name, theme); (2) Type (§9.5.1, official or homebrew — see §9.5.9); (3) Function (§9.5.2); (4) Behavior (§9.5.3, already fixes the encounter multiplier); (5) Target Category (§9.5.6, already defines the desired NP range); (6) distribute the target NP among Attributes+Natural Skills, Characteristics (§9.5.4), Abilities, and Equipment, until reaching the chosen category's NP; (7) define 1 mandatory Weakness; (8) define Rewards; (9) validate against the checklist below.

**Balancing Checklist**: **Weakness Rule** — every creature needs at least 1 clear Weakness, without exception; **Clear Function Rule** — the creature has 1 defined primary function, never a "generic monster that attacks"; **Category Cap Rule** — total NP cannot exceed the chosen Category's range by more than 15%.

**9.5.9 Homebrew Type Creation Manual**: (1) a Homebrew Type describes the creature's **nature/origin**, never its function or behavior (those are separate layers, §9.5.2/§9.5.3); (2) it must be compatible with the cosmology (§2) — since each floor is a fragment of a dead universe, a new Type must justify which fragment/arc it represents; (3) it **never grants its own mechanical bonus** — Type is a purely narrative/organizational classification (unlike Characteristics, which have an NP cost); (4) it must allow free combination with any existing Function and Behavior.

**9.5.10 Base Bestiary (10 ready-to-play creatures)**

| Name | Type | Function | Behavior | Category | Key Characteristics | Weakness |
|---|---|---|---|---|---|---|
| Goblin Raider | Corrupted Humanoid | Soldier | Instinctive | Weak | Enhanced Senses | Flees below 50% PV |
| Plagued Rat | Beast | Parasite | Instinctive | Weak | Keen Smell | Vulnerable to fire |
| Guardian Skeleton | Undead | Guardian | Instinctive | Common | Carapace (bone) | Vulnerable to blunt damage |
| Corrupted Cultist | Corrupted Humanoid | Soldier | Intelligent | Common | Minor Ritual (common ability) | Low Will (easy to intimidate) |
| Deep Spider | Beast | Predator | Instinctive | Veteran | Potent Poison, Natural Camouflage | Sensitive to strong light/vibration |
| Corrupted Knight | Undead | Guardian | Strategic | Elite | Carapace, Regeneration | Vulnerable to sacred magic |
| Swamp Witch | Aberration | Soldier (Control) | Strategic | Elite | Advanced Control ability | Weak in melee combat |
| Fragmented Stone Golem | Construct | Guardian | Instinctive | Champion | Double Carapace, Immune to Poison/Fear | Exposed core (weak point) |
| Spectral Commander | Spirit | Soldier (Command, §9.7) | Strategic | Champion | Flight, Supreme Command (horde buff) | Dissipates with sacred light/a Seal |
| Eclipse Dragon | Draconic | Boss (Sovereign) | Strategic | Arc Boss | Flight, Regeneration, Multiple Attacks, breath weapon (supreme ability) | Exposed core after a certain phase |

### 9.6 Creature Scale Against the Party
The character-NP × creature-NP relationship is calibrated by category (Common, Elite, Boss), with a **Horde Factor** (a multiplier by the number of simultaneous enemies) and specific rules for Bosses (phases, Legendary Actions). **Principle of Dungeon Superiority**: the Dungeon should, by default, slightly outmatch the party — never be trivial.


### 9.7 Hordes, Sieges, and Mass Conflicts
Horde types: Swarm, Army, Invasion, Catastrophe. Size: Small, Medium, Large, Massive. Has its own Power, Pressure, Origin, Command, and Turns (the horde acts in blocks, not creature by creature). Possible objectives: Survival, Defense, Escort, Containment, Retreat. Scaled Command System (Individual → Tactical → Military → Strategic), with its own attributes (Leadership, Strategy, Military Knowledge, Intelligence) and Morale.

### 9.8 Encounter System (tabletop formulas)
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

Validated through practical tests (5 weak goblins → "very easy"; 10 corrupted soldiers on favorable terrain → "nearly impossible"; a lone dragon → plausible as "arc boss").

### 9.9 Floor Threat Budget (the GM's central tool)
```
OA = PG × Difficulty × Duration Factor
```

- GM's desired Difficulty: Safe 0.75 | Normal 1.0 | Dangerous 1.25 | Deadly 1.5 | Infernal 2.0 | Apocalyptic 3.0
- Duration: Short (1-2 encounters)→1 | Normal (3-5)→2 | Long (6-10)→3 | Extended→4

The GM distributes the OA among creatures, traps, events, elites, and a boss, following proportions suggested by floor type (e.g., a combat floor ≈ 70% creatures/15% environment/15% events; a boss floor ≈ 70% boss/20% mechanics/10% environment).

**Encounter Compression Factor (FCE) — CLOSED by Playtest (§17.10)**: simulations showed that using the Encounter Ratio (R = PE/PG, §9.8) directly as a multiplier on the enemy's real stats creates a "cliff" (the party almost always wins or almost always loses, with no real gradation). When building the combat stats of a creature/group to hit a target Ratio R, the GM should apply:
```
Real Attribute/Skill Multiplier = 1 + (R − 1) × FCE
```

| Party Ranking | FCE |
|---|---:|
| Bronze–Iron | 0.40 |
| Steel–Silver | 0.25 |
| Gold–Mithril | 0.15 |
| Adamant–Legendary | 0.10 |

The FCE decreases as Ranking rises: low-Ranking parties have reduced PV, so dice variance already smooths out the difficulty on its own; high-Ranking parties have high PV, making combats more deterministic, requiring stronger compression to preserve the gradation between Favorable/Balanced/Unfavorable/Impossible.

**Validation with heterogeneous parties (CLOSED)**: the FCE was initially recalibrated with identical "average" builds — a retest done with realistic parties (1 Tank, 2 Balanced, 1 DPS, individual NP varying ±20%) confirmed the table remains stable, and was even more consistent across Rankings than with uniform builds (Favorable 77-98%, Balanced 53-65%, Unfavorable 15-32%, Impossible 0-5%). The FCE is validated for direct use at the table.

**Separate difficulties**: **Combat Difficulty (DC = PE/PG)** measures how hard it is to beat the enemies; **Objective Difficulty (DO)** measures how hard it is to complete the mission itself (time, environment, pressure) — the two are independent (an easy fight can happen within a very difficult mission).

### 9.10 Dominion, Strategic Assets, and the Four Pillars of Progression
The campaign progresses on four simultaneous, independent fronts:

1. **Individual Power (NP)** — characters.
2. **Institutional Power (CG — Guild Capacity)** — the organization.
3. **Strategic Resources (SR)** — consumable/economic goods.
4. **Strategic Assets (SA)** — permanent, non-consumable achievements obtained in the Dungeon.

Strategic Asset categories: Infrastructure (mines, workshops, towers, laboratories found), Knowledge (diaries, maps, weaknesses, rituals), Diplomacy (alliances, rescued survivors), Artifacts (dimensional keys, relics), Territorial Control (bridges, forts, stabilized portals).

**Strategic Value (SV)** — a scale from 1 (local benefit) to 5 (large-scale permanent change), used by the GM to calibrate risk vs. reward.

Not all Strategic Assets on a floor can be obtained at the same time — players frequently choose between conflicting objectives, and these choices permanently alter the course of the Guild and the campaign.

> **Fundamental Principle of Progression**: characters evolve through Power Level; the Guild evolves through Guild Capacity; the campaign evolves through Strategic Assets.

---

## 10. The Guild

### 10.1 Institutional Structure
Council (Patrons) → Characters (field agents) → Its own Hierarchy → Prestige, Influence, Legacy, Institutional Resources, Specializations, Institutional Capacities.

### 10.2 Guild Sheet

1. **Identity** — name, coat of arms, patron deity, main doctrine, founding date, Guild ranking.
2. **Prestige** — recognition (affects recruitment, contracts, influence, events).
3. **Influence** — political relations, separated by city/faction/other Guild/deity.
4. **Resources** — Pact Coins, materials, Dimensional Fragments, artifacts, stockpiles.
5. **Headquarters** — list of facilities and their levels.
6. **Staff** — artisans, researchers, workers, mercenaries, administrators.
7. **Knowledge** — maps, recipes, research, catalogued enemies, defeated bosses, weaknesses, historical records (the campaign's permanent memory).
8. **Doctrines** — active doctrines and their effects.
9. **Logistics** — storage capacity, maximum number of workers, mercenary limit, simultaneous expeditions, exploration reach.
10. **Expeditions** — a record of every incursion (date, participants, objective, result, losses, resources obtained) — functions as the campaign's "diary."
11. **Legacy** — greatest historical feats (first floor conquered, first Rupture averted, etc.), which can grant permanent benefits.
12. **Institutional Capacity (CI)** — measures how much the organization can *sustain* (number of active Patrons, workers, facilities, simultaneous projects, mercenaries, warehouse size). Formula closed in §10.9.
13. **Formation Capacity (CF)** — determines the initial potential of a newly recruited character (see §6.9). Formula closed in §10.9.
14. **Support Capacity (CS)** — structural limit on the number of buildings the Guild can manage simultaneously (expanded by administrative/logistics facilities). Formula closed in §10.9.

### 10.3 Headquarters and Buildings
Philosophy: buildings form a **real tech tree**, not an independent shopping list.

Every building has: **Prerequisites** (structural, institutional, knowledge-based, resource-based, human), **Costs**, **Direct Benefits**, **Synergies** with other buildings.

**Hierarchy by category:**

| Level | Category | Examples |
|---|---|---|
| I | Foundation | Gate, Dormitory, Warehouse, Training Field |
| II | Production | Smithy, Workshop, Library, Infirmary |
| III | Specialization | Arcane Laboratory, Military Academy, Alchemical Garden, Rune Workshop |
| IV | Institutional | Memorial, Logistics Center, Mercenary Barracks, Mage Tower |
| V | Monumental | Council Chamber, Divine Vault, Dimensional Observatory, Patron's Sanctuary |

**Principle of Institutional Maturity**: the prerequisites check not just the *existence* of a base building, but its **level** (e.g., an Arcane University requires Library III + Arcane Laboratory I, not just "having a library"). Not every building has the same level cap (Dormitory may stop at V; Library may go up to VII; Dimensional Gate may only reach II, but be extremely expensive).

**10.3.1 Official List of Facilities and Tech Tree (CLOSED)**

| # | Facility | Weight | Level Cap | Prerequisite | What it unlocks |
|---|---|---:|---|---|---|
| **Foundation** | | **1** | | | |
| 1 | Gate | — | Fixed (I) | None — exists from the start | The Dungeon's core; neither built nor upgraded |
| 2 | Dormitory | 1 | V | None | Capacity for resident characters/workers (Level × 2 slots) |
| 3 | Warehouse | 1 | V | None | Storage capacity (Level × 50 resource units) |
| 4 | Training Field | 1 | V | None | Basic combat skill training; Body and Control Trials (§6.3) |
| **Production** | | **2** | | | |
| 5 | Smithy | 2 | V | Warehouse I | Weapon/armor Crafting (Common up to Rare at Level I-II; Epic at III+) |
| 6 | Workshop | 2 | V | Warehouse I | General Crafting — tools, utility items (Common/Uncommon) |
| 7 | Library | 2 | VII | Dormitory I | Minor/Moderate Research (§11.2); Intellect and Perception Trials |
| 8 | Infirmary | 2 | V | Dormitory I | Advanced healing, improves PV recovery during the Interlude; Vigor Trial |
| **Specialization** | | **3** | | | |
| 9 | Arcane Laboratory | 3 | V | Library II | Major Arcane Research; Affinity Trial; Enchantment (together with the Mage Tower) |
| 10 | Military Academy | 3 | V | Training Field II + Infirmary I | Presence and Will Trials; Supreme Techniques (§6.6.7); advanced mercenary training |
| 11 | Alchemical Garden | 3 | IV | Workshop II | Advanced Alchemy — Poisons and Transmutation at a competitive level |
| 12 | Rune Workshop | 3 | IV | Smithy II | Epic+ Crafting; weapon Enchantment (together with the Arcane Laboratory) |
| **Institutional** | | **5** | | | |
| 13 | Memorial | 5 | IV | Library III | Access to Memory Crystals (§6.9); increases Formation Capacity (CF) |
| 14 | Logistics Center | 5 | IV | Warehouse III + Workshop II | Increases Support Capacity (CS); more simultaneous Secondary Expeditions |
| 15 | Mercenary Barracks | 5 | IV | Military Academy II | Hiring higher-Ranking Mercenaries; increases the mercenary limit |
| 16 | Mage Tower | 5 | IV | Arcane Laboratory III | Supreme Research; advanced Rituals; rare Grimoires |
| **Monumental** | | **8** | | | |
| 17 | Council Chamber | 8 | II | Logistics Center III + Memorial II | Increases Institutional Capacity (CI); more active Patrons/simultaneous projects |
| 18 | Divine Vault | 8 | II | Memorial III | Secure storage of Pact Coins; enables Divine Crafting (§6.7.4) |
| 19 | Dimensional Observatory | 8 | II | Mage Tower III | Detects/predicts Ruptures in advance; reduces the base Pressure of explored floors |
| 20 | Patron's Sanctuary | 8 | I–II | Council Chamber I + Divine Vault I | Strengthens the Divine Pact; grants resistance to negative Divine events (§12) |

**Construction Cost (CLOSED)** — reuses the weights already fixed in the CG formula (§10.8: Foundation=1, Production=2, Specialization=3, Institutional=5, Monumental=8):
```
Resource Cost = Building Level × Category Weight × 10
Construction Time = Building Level × Category Weight × 3 days
Minimum workers involved = Category Weight
```

| Category (Weight) | Level I | Level III (if applicable) |
|---|---|---|
| Foundation (1) | 10 resources / 3 days | — |
| Production (2) | 20 resources / 6 days | 60 resources / 18 days |
| Specialization (3) | 30 resources / 9 days | 90 resources / 27 days |
| Institutional (5) | 50 resources / 15 days | 150 resources / 45 days |
| Monumental (8) | 80 resources / 24 days | — (rarely goes past Level I-II) |

**Monumental** buildings also require **Pact Coins = Level × 2**, in addition to common resources — reinforcing that they are campaign achievements, not just money.

**Guild Technology Level (NTG)**: an indicator derived from accumulated infrastructure + knowledge, used as a reference for cutting-edge unlocks (monumental buildings, legendary equipment, very complex spells, research with Dimensional Fragments).

Start of the campaign: only the Gate, Dormitory, and a basic Training Field exist — everything else is built by the players.

### 10.4 Workers and Mercenaries

- **Workers**: Laborers, Artisans, Researchers, Instructors, Merchants, Physicians, Administrators. Each has efficiency, salary, morale, and a specialty — they do tasks reasonably well, never as well as the players.
- **Mercenaries**: NPCs hired to patrol, collect, mine, transport, and explore **only floors already conquered**. Fixed rule: mercenaries never enter unknown floors — they never "play the game for the players" (Principle of the Exploration Frontier).

### 10.5 Guild Departments
Exploration, Military, Arcane, Logistics — each aggregates related functions and workers, easing administration in large campaigns.

### 10.6 Guild Economy (CLOSED)
Common currency (**Silver**) + material resources + **Pact Coins** (a special divine currency, obtained in the Dungeon, with commercial value, material value for Divine Crafting, and divine/symbolic value). Base exchange rate: **1 Pact Coin = 10 Silver**. Funding can come from characters' Free Contributions, a Guild Contract, or Return Investment. Reward distribution splits between Character / Guild / Strategic Reserve. Institutional maintenance continuously consumes resources (the Golden Rule applied to the Guild).

**10.6.1 Base prices**:

| Item/Service | Base price |
|---|---:|
| Food ration (1 day) | 1 Silver |
| Simple lodging (1 night) | 2 Silver |
| Laborer daily wage | 3 Silver |
| Artisan/Researcher daily wage | 8 Silver |
| Building maintenance | Category Weight × 1 Silver/day |

**Mercenary daily wage** (by Ranking):

| Ranking | Wage/day |
|---|---:|
| Bronze | 10 |
| Iron | 18 |
| Steel | 30 |
| Silver | 50 |
| Gold | 80 |
| Mithril | 120 |
| Adamant | 170 |
| Legendary | 250 |

**10.6.2 Income Generation**: (1) Expedition Rewards — the "Guild" share already closed in the distribution; (2) Trade — the Commercial Doctrine grants +10% on selling surplus materials; (3) Worker Production — Laborers generate ~2 Silver/day of value, Artisans/Researchers generate items/research instead of direct Silver; (4) Secondary Expeditions: `Yield = Mercenary's NP × 0.5 Silver per successful secondary expedition`; (5) Legacy — historical feats can grant permanent income bonuses (e.g., +5% in a specific source).

**10.6.3 Maintenance**:
```
Daily Maintenance = Σ (Level × Category Weight × 1 Silver, per building) + Σ (daily wages of active Workers/Mercenaries)
```
If unpaid: buildings enter **Neglect** (half the benefit until settled) and Workers lose Morale (reduced efficiency) — it never blocks the game, it's a consequence (Principle of Failures as Consequence).

**10.6.4 Inflation — Price Index by Guild Stage** (reuses the stages already fixed in CG, §10.8):
```
Adjusted Price = Base Price × Current Stage's Price Index
```

| Guild Stage | Price Index |
|---|---:|
| Foundation | ×1.0 |
| Minor Guild | ×1.2 |
| Regional Guild | ×1.5 |
| Recognized Guild | ×1.8 |
| Major Guild | ×2.2 |
| Renowned Guild | ×2.6 |
| Legendary Guild | ×3.2 |
| Divine Guild | ×4.0 |

This ensures money never "solves the game" at advanced stages — the cost of operating grows alongside the Guild's ambition, keeping the Golden Rule active throughout the campaign.

### 10.7 Guild Doctrines (CLOSED)
A permanent specialization of the organization's operational philosophy (functions like an institutional specialization tree). Each campaign develops its own combination, giving each Guild a unique identity even using the same base system.

**Choice rule**: the Guild starts with **up to 2 active Doctrines**. It unlocks **+1 extra Doctrine per Council Chamber Level** (§10.3.1), up to a maximum of **4 simultaneous Doctrines**. Swapping an active Doctrine for another requires an Interlude project (time = 20 days, Difficult difficulty on a Leadership/Administration Test) — it's not a trivial choice.

| Doctrine | Bonus |
|---|---|
| **Military** | +10% on Attack/Damage for the Guild's Mercenaries and combat NPCs; -1 day on the time of Body/Control/Presence/Will Trials |
| **Academic** | +15% speed on Research projects (reduces time); -10% Resource cost for Intellect/Perception Trials |
| **Commercial** | +10% on all sales of surplus materials; reduces the Inflation Price Index by 1 stage for the Guild's own purchases |
| **Exploration** | +15% success chance on Secondary Expeditions; -10% Food/Water/Torch consumption for the main party |
| **Arcane** | -1 additional PA for casting for all Guild characters (stacks with Magical Control Grade); -25% on Affinity Trial time |
| **Engineering** | -15% on facility Construction/Upgrade Time; +10% chance of a Great Success in Crafting |
| **Logistics** | +20% Support Capacity (CS); -10% Daily Maintenance |
| **Diplomatic** | Newly discovered factions start with +15 Reputation; Reputation gains of Moderate weight count as Major (losses remain normal) |

### 10.8 Guild Capacity (CG) — CLOSED formula

**Architectural decision**: CG is **decoupled** from the combat threat calculation. It is never added to Party Power (PG) nor does it enter the Threat Budget (OA) — this would avoid counting the Guild's strength twice (once via equipment/training already baked into the characters' NP, and again in the danger calculation). CG is a **purely institutional** value, which measures what the Guild can *sustain* (workers, mercenaries, simultaneous buildings, Formation Capacity for new recruits, which advanced Strategic Assets it can maintain). This preserves the **Four Pillars of Progression** (§9.10) as truly independent tracks: NP (character), CG (Guild), SR (resources), SA (strategic assets).

```
CG = Infrastructure + Research + Logistics + Resources
```
where:

- **Infrastructure** = Σ (level of each building × category weight: Foundation=1, Production=2, Specialization=3, Institutional=5, Monumental=8)
- **Research** = points accumulated in completed projects
- **Logistics** = Support Capacity (CS) + number of qualified workers × 2
- **Resources** = Pact Coin reserves + strategic materials (converted value)

**Official CG table by Guild Stage** (a milestone every 5 floors conquered, tracking the Special Floors):

| Guild Stage | Floors conquered | Infrastructure | Research | Logistics | Resources | **CG** |
|---|---:|---:|---:|---:|---:|---:|
| Foundation | 0 | 5 | 0 | 5 | 5 | **15** |
| Minor Guild | 5 | 20 | 10 | 15 | 15 | **60** |
| Regional Guild | 10 | 45 | 25 | 30 | 30 | **130** |
| Recognized Guild | 15 | 80 | 45 | 50 | 50 | **225** |
| Major Guild | 20 | 125 | 70 | 75 | 75 | **345** |
| Renowned Guild | 25 | 180 | 100 | 105 | 105 | **490** |
| Legendary Guild | 30 | 245 | 135 | 140 | 140 | **660** |
| Divine Guild | 35+ | 320 | 175 | 180 | 180 | **855** |

The curve advances at the same pace as the 5-floor milestones, mechanically reinforcing that the Guild and the Dungeon progress together.

### 10.9 Derived Capacities — CI, CF, CS (CLOSED)

Unlike CG (institutional, isolated from combat), these three capacities **lock in concrete gameplay limits**, each tied to specific facilities in the tech tree (§10.3.1):

```
CS (Support Capacity) = 5 + (Logistics Center Level × 2) + (Warehouse Level × 1)

CI (Institutional Capacity) = 3 + (Council Chamber Level × 4) + (Logistics Center Level × 1)

CF (Formation Capacity) = 10 + (Memorial Level × 3) + (Library Level × 1) + (Training Field Level × 1)
```

**Progression by Guild Stage** (the same 8 stages from §10.8):

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

**CS — what it locks in**: the maximum number of buildings the Guild can keep **active/managed** at the same time. Since the tech tree has 19 constructible facilities and the CS cap is 16, even a Divine Guild has to choose which stay active — the excess become **Inactive** (no benefit) until the player deactivates another or raises CS. This reinforces the Golden Rule even at the top of the game.

**CI — what it locks in**:
- Simultaneous active Patrons = CI ÷ 3 (rounded up, minimum 1)
- Simultaneous Interlude projects (research/construction/etc. in parallel) = CI ÷ 2
- Total hireable workers = CI × 3

**CF — what it grants** (Formation bonus when creating a new character, §6.9):

| CF | Formation Bonus |
|---|---|
| 10–17 | None (standard Recruit) |
| 18–22 | +5 extra skill points |
| 23–27 | +10 extra skill points; starting equipment can be Uncommon |
| 28–32 | +15 extra skill points; Uncommon equipment guaranteed; +1 extra minor Talent |
| 33+ | +20 extra skill points; Rare equipment possible; 1 starting skill already begins at Basic Grade |

---

## 11. Interlude (the system's "second heart")

### 11.1 Two timelines

- **Dungeon Time**: used during the session (e.g., an expedition can last 10 "internal" days).
- **World/Headquarters Time**: passes in weeks between sessions, with a **fixed time dilation** to simplify accounting (e.g., 10 days in the Dungeon ↔ just 1 day at the Headquarters), eliminating the classic problem of "my character got stuck in the Dungeon because I missed a session."
- Each character receives a number of **interlude actions** proportional to the time available since their last expedition. Actions are declared by the player before the next session and resolved by the GM.

### 11.2 Interlude Subsystems

1. **Training** (§6.4) — guaranteed progress, fixed per day, modified by the Guild's facilities/instructors/equipment/knowledge; the learning curve applies normally.
2. **Research** — types: Arcane, Biological, Technological, Dimensional, Historical, Military. Flow: **Discover → Research → Master → Apply**. Research projects have their own progress, can be collective (multiple researchers), can generate partial discoveries, and can be interrupted. Relevant facilities: Library, Arcane Laboratory, Workshop, Memorial.

**Research Cost (CLOSED)** — reuses the Complexity tiers already fixed for Magic (§6.6.3), extended to any type of research:

| Complexity | Time | Resource Cost | Minimum Facility |
|---|---:|---:|---|
| Minor | 5 days | 10 | Basic Library/Workshop |
| Moderate | 10 days | 25 | Library II+ |
| Major | 20 days | 50 | Corresponding Laboratory |
| Supreme | 40+ days | 100+ | Advanced facility + 5 Pact Coins |

Collective research (multiple researchers) divides the time proportionally, but never below 50% of the base time.

3. **Production and Creation (Crafting)** — categories: Forge, Alchemy, Enchantment, Engineering, Artifacts. Artisans have a specialty, a mastery grade, and efficiency. Recipes can be Known, Discovered Projects, or Unique Recipes. Item Quality: Common, Superior, Rare, Epic, Legendary, Divine.
4. **Guild Administration** — institutional economy, reward distribution, worker/mercenary management, departments, maintenance, administrative events, Prestige.
5. **Secondary Expeditions** (mercenaries) — collection, recovery, patrol, transport, support, field research; always limited to floors already conquered (Principle of the Frontier); they generate reports, risk of casualties, and variable yield depending on the team's specialization.

### 11.3 Applying the Modifier Origin Rule
Every facility, instructor, piece of equipment, and institutional knowledge that boosts an interlude activity must have a clearly traceable origin (no "+2 just because").

---

## 12. Dynamic Events and Tension

The world doesn't stand still during the players' absence. Event categories: Personal, Guild, Dungeon, World, Divine. Generation can be Natural, a Consequence of past actions, or Narrative (a GM decision).

**Tension System** — four indicators accumulate value over time and increase the chance/intensity of events:

- Guild Tension
- Dungeon Tension
- World Tension
- Divine Tension

Events can be Positive, Negative, or Mixed, can chain together, and a **Calamity** (official name: **Rupture**) is the Dungeon's maximum-tension event — when a Fragment breaks containment and invades the Central World. There is a permanent historical record of important events.

---

## 13. Factions (CLOSED)

Concept: factions exist within the Dungeon (Goblins, Cultists, Undead, Merchants, Beasts, rival Adventurers, etc.), control territory, have objectives, react to the players' choices, form alliances, and go to war with each other — but their influence stays confined to the Dungeon's floors (not the external political world), so as not to over-expand the system's scope.

**13.1 Faction Reputation**: a scale from **-100 to +100**, divided into 5 levels:

| Reputation | Level | Default Behavior |
|---|---|---|
| -100 to -51 | Hostile | Attacks the party on sight; closes routes; may put a bounty on the party's heads |
| -50 to -11 | Distrustful | Bad prices, withholds information, demands proof before helping |
| -10 to +10 | Neutral | Default behavior, no bonus/penalty |
| +11 to +50 | Friendly | Access to trade/information, grants safe passage, offers hints |
| +51 to +100 | Allied | Fights alongside the party, shares territory/resources, unlocks exclusive Strategic Assets |

**13.2 Table of Consequences for Choices**:

| Weight of Choice | Variation |
|---|---:|
| Minor (a small favor, a gesture of goodwill or a light offense) | ±5 |
| Moderate (honoring/breaking an agreement, helping/attacking a member) | ±15 |
| Major (deciding a faction's fate in a conflict, betrayal, rescuing a leader) | ±30 |

**13.3 How a Faction Changes a Floor in Practice** — connects Factions directly to the Encounter System (§9.8):

- **Territory under faction control**: applies the Terrain multiplier from the PE formula — a Hostile faction defending its own territory = Favorable (×1.25) or Extreme (×1.5) if it's the main lair; Neutral Reputation = Neutral Terrain (×1).
- **Allied Faction in an area**: hostile encounters in that area get reinforced — treat it as `PG × 1.1` just for that encounter.
- **Active Hostile Faction**: its encounters more often use the "Critical mission" Objective (×2) — it's fighting to stop the party, not just to survive.
- **Access to Strategic Assets**: some only become available at Friendly+ Reputation (the faction shows a hidden path) or require direct conflict if Hostile.
- **Effect on Pressure**: keeping an Allied faction in an area reduces Pressure generated by events in that area by **-5** (§9.2), stackable with other sources.

**13.4 Record**: each relevant faction's Reputation enters the Guild Sheet (§10.2, item 3 "Influence") and the Campaign Record (§14) — a history of decisions that moved the number, for narrative consistency between sessions.

---

## 14. Campaign Record

Functions as the campaign's "save game" — automatically records floors conquered, deaths, living characters, resources, buildings, workers, research, faction relations, available memories, important events, doctrines. The base for the history and for the Guild Sheet (§10.2, item 10).

---

## 15. Appendix — Consolidated Formulas

```
Attribute Modifier = Attribute − 2

NP (character) = (Attributes + Skills) + (Talents + Abilities) + Equipment

PG (Party Power) = Σ NP(characters) × Synergy Factor

PE (Encounter Power) = Σ NP(creatures) × Quantity × Intelligence × Terrain × Objective

R (encounter classification) = PE / PG

DC (Combat Difficulty) = PE / PG      [same formula as R, applied to combat alone]
DO (Objective Difficulty) = calculated separately by time/environment/pressure/information

OA (Floor Threat Budget) = PG × Floor Difficulty × Duration Factor

CG (Guild Capacity) [CLOSED — institutional, decoupled from combat] = Infrastructure + Research + Logistics + Resources
CS (Support Capacity) = 5 + (Logistics Center Level × 2) + (Warehouse Level × 1)
CI (Institutional Capacity) = 3 + (Council Chamber Level × 4) + (Logistics Center Level × 1)
CF (Formation Capacity) = 10 + (Memorial Level × 3) + (Library Level × 1) + (Training Field Level × 1)
```

---

## 16. Quick Glossary

- **Patron** — the player in the administrative role (Guild Council).
- **Character** — the expendable adventurer who explores the Dungeon.
- **Rupture** — the dimensional-collapse event when a floor escapes containment.
- **Memory Crystal** — the posthumous record of a dead character's memories.
- **NP** — Power Level (individual).
- **CG** — Guild Capacity (institutional).
- **CI** — Institutional Capacity (what the Guild can sustain).
- **CF** — Formation Capacity (a new recruit's starting level).
- **CS** — Support Capacity (limit of manageable buildings).
- **NTG** — Guild Technology Level.
- **SA** — Strategic Asset (a permanent, non-consumable achievement).
- **SR** — Strategic Resource (consumable).
- **SV** — Strategic Value (an Asset's importance, scale 1–5).
- **OA** — Threat Budget (the GM's floor-building tool).
- **PG / PE** — Party Power / Encounter Power.

---

## 17. SYSTEM CLOSURE HISTORY

> **Current status: 100% closed.** Every pending item identified throughout development (formulas, character creation, combat, exploration, equipment, magic/techniques, creatures, balancing/playtest, cost of attributes/research/construction/crafting, economy, pressure, factions, and content-building tools) has been resolved and validated. This section now functions as a **historical record** of how each system reached its final state — useful for understanding the reasoning behind each number, should anything need revisiting in the future.

### 17.1 Formulas — **FULLY CLOSED**

- ~~Definitive Power Level formula~~ — **CLOSED** (§6.8): official NP ranges by Ranking, validated by simulation across the 8 Rankings.
- ~~Definitive Guild Capacity (CG) formula~~ — **CLOSED** (§10.8): CG decoupled from the combat calculation, with an official table by Guild stage.
- ~~Attribute progression cost~~ — **CLOSED** (§6.3): the Attribute Trial system, with time/cost scaling by Grade and thematic Trials per attribute.
- ~~Cost of research, construction, and crafting~~ — **CLOSED** (§10.3, §11.2, §6.7.4): time/resource/Pact Coin tables for all three, reusing the scales already fixed for CG, Magic Complexity, and Rarity.
- ~~Complete economy~~ — **CLOSED** (§10.6): Silver/Pact Coin with a base exchange rate, base prices, mercenary wages by Ranking, income generation, daily maintenance, and a Price Index by Guild stage (inflation).
- ~~Final Dungeon Pressure calculation~~ — **CLOSED** (§9.2): a 0-100 counter per floor, with thresholds and PE multipliers, validated in an end-to-end test (§17.10).

### 17.2 Character Creation — **FULLY CLOSED**
All items below have been resolved: attribute distribution (§6.3, Free Purchase), Origins (§6.1.1/§6.1.2), Backgrounds (§6.1.3/§6.1.4), Aptitudes (§6.1.5), Initial Talent (§6.1.6), Lineages/Races (§6.1.7), Formation Debt (§6.2), and the final step-by-step procedure (§6.1). No pending items remain in this system.

### 17.3 Combat — **FULLY CLOSED**
All items below have been resolved in §7: Hybrid Movement (Grid/Hex for small scale, Zones for large scale), Initiative, Range/Cover, Opportunity Attacks (covered by the Reaction), final Attack/Damage/Armor formulas, Hybrid Defense (Passive/Active), Hit Points and recovery, Conditions (closed list), and the death procedure (Dying → instant death on new damage). No pending items remain in this system.

### 17.4 Exploration — **FULLY CLOSED**
All items below have been resolved in §8: Exploration Turn (10 min), Vision/Lighting, Navigation and Maps, Traps (detection/disarming/damage), Group Exploration (roles and subgroups), Camping/Rest, and Resource Consumption (food, water, torches, rope, ammunition, carrying capacity). No pending items remain in this system.

### 17.5 Equipment and Crafting — **FULLY CLOSED**
All items below have been resolved in §6.7: Rarity (table of max properties/base bonus/NP), Categories, 20 Properties and Enchantments + homebrew manual, the Crafting process, Upgrading/Modification/Reconstruction, Durability (Wear Hits), and the Complete Item Creation Guide (GM Path / Player Path). No pending items remain in this system.

### 17.6 Magic — **FULLY CLOSED**
All items below have been resolved in §6.6: the 8 official Schools of Magic, the mechanical structure of a spell (cost/range/area/duration/test/effect), cost and reduction by Magical Control Grade, Interruption, creating new spells via Arcane Research, Enchanting Items, Rituals, and a list of 24 example spells (1 per School, evolving Minor→Moderate→Major). No pending items remain in this system.

### 17.7 Martial Techniques — **FULLY CLOSED**
All items below have been resolved in §6.6.7/§6.6.8: the technique tree by style (Stance/Technique/Reaction/Supreme), formal requirements by category, the process for creating new techniques via the Interlude, and a list of example techniques for 3 styles (Swords, Unarmed Combat, Bows), with Technique I→II progression. No pending items remain in this system.

### 17.8 Creatures — **FULLY CLOSED**
All items below have been resolved in §9.5: 8 official Types, Function in the Dungeon, Behavior/AI with concrete tabletop rules, a Natural Characteristics table with NP cost, the creature NP formula, Categories mapped by NP/Ranking range, the Simplified Creature Sheet, the Creature Creation Manual + balancing checklist, the Homebrew Type Manual, and a Base Bestiary of 10 ready-to-play creatures. No pending items remain in this system.

### 17.9 Factions — **FULLY CLOSED**
System closed in §13: numeric Reputation (-100 to +100, 5 levels), the Table of Consequences for Choices, and a direct connection to the Encounter System (Terrain/Objective/PG reinforcement) and to Dungeon Pressure. No pending items remain in this system.

### 17.10 General Balancing and Playtest — Simulation Result

A Monte Carlo simulation was run (2d10, parties of 4 characters, 300-500 combats per cell) crossing the 8 Rankings × 4 requested conditions (Favorable/Balanced/Unfavorable/Impossible). The process uncovered and fixed two critical balancing bugs (see errata in §7.5 and §7.4) and calibrated the Encounter Compression Factor (§9.9). Final validated table (with the fixes + FCE by Ranking applied):

| Ranking | Favorable | Balanced | Unfavorable | Impossible |
|---|---:|---:|---:|---:|
| Bronze | 93% | 50% | 15% | 0% |
| Iron | 96% | 52% | 15% | 0% |
| Steel | 99% | 56% | 16% | 0% |
| Silver | 93% | 53% | 15% | 0% |
| Gold | 94% | 53% | 11% | 0% |
| Mithril | 94% | 50% | 19% | 0% |
| Adamant | 98% | 54% | 25% | 0% |
| Legendary | 92% | 40% | 30% | 0% |

Reading: Favorable and Impossible behave consistently across all 8 Rankings (almost always a win / almost never a win). Balanced remained stable between 40-56% ("succeeds if not too many mistakes are made"). Unfavorable landed between 11-30% ("needs excellent results") — still with some variation between Rankings that could benefit from another round of FCE fine-tuning in a real tabletop session, but within an acceptable range for immediate use.

**Remaining balancing items** (fine-tuning, do not block use of the system):

- A consolidated Balancing and Content-Building Guide (creating creatures, floors, arcs, and entire campaigns) — partially covered by the Threat Budget (§9.9) and the FCE (§9.9), but still without a single unified "GM's manual."
- Testing the complete system on a real floor of the first arc ("The Village of a Thousand Monsters") end to end, including pressure, rewards, and strategic assets.
- Validating the FCE in real play (the simulation uses aggregated/average builds; real characters with specialized builds may behave differently).

### 17.11 Content Exploration / GM Tools — **FULLY CLOSED**

- ~~Balancing and Content-Building Guide~~ — **CLOSED** (GM's Manual §6.6): a 5-level Content-Building Guide (Creature → Encounter → Floor → Arc → Campaign), tying together the Threat Budget, FCE, Pressure, and Factions into a single workflow.
- ~~Test the complete system on a real floor of the first arc~~ — **DONE**: Arc 1 "The Village of a Thousand Monsters," Floor 1 "The Silence Before the Horde," built end to end (PG=315, OA=630, two encounters classified via §9.8, a Strategic Asset with an assigned SV, numeric Pressure applied in real time). Result: the system produced a balanced fight at the start and a hard one at the climax, exploration with real mechanical-value information, a permanent choice with no "wrong" option, and cross-validated against the Playtest numbers (§17.10). The test revealed the gap in numeric Pressure, which was closed in §9.2 as a direct consequence.
- ~~Validate the FCE with real/heterogeneous builds~~ — **DONE**: a retest with Tank/DPS/Balanced parties and NP ±20% confirmed the FCE remains stable (and even more consistent) outside the scenario of identical "average" builds.

### 17.12 Suggested Order of Work (picking back up the plan already underway)

1. ~~Close the fundamental formulas (final NP and CG) with a simulation of several hypothetical characters/Guilds.~~ — **DONE**.
2. ~~Close character creation as a playable procedure.~~ — **DONE**.
3. ~~Close Combat.~~ — **DONE**.
4. ~~Close Exploration.~~ — **DONE**.
5. ~~Close Equipment and Crafting.~~ — **DONE**.
6. ~~Close Magic and Martial Techniques.~~ — **DONE**.
7. ~~Close Creatures (base bestiary).~~ — **DONE**.
8. ~~General balancing and playtest.~~ — **DONE** (§17.10).

---

*End of document. This GDD reflects the consolidated state of all decisions made to date; any future changes must be recorded here to prevent contradictions between modules.*
