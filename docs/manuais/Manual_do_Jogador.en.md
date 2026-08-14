# PLAYER MANUAL
### Hardcore Dungeon Crawler RPG

> This manual gathers everything you need to create a character, play an expedition, and take part in your Guild's life. Whenever a rule allows for **homebrew** content (created by you or the GM), it's marked as such — **talk to your GM before using anything homebrew in play**, since they're the one who approves what enters the campaign.

---

## Table of Contents

1. [The World in a Few Words](#1-the-world-in-a-few-words)
2. [How the System Works](#2-how-the-system-works)
3. [Character Creation](#3-character-creation)
   - 3.1 [Origin](#31-origin) · 3.2 [Background](#32-background) · 3.3 [Lineage](#33-lineage-racespecies) · 3.4 [Starting Aptitudes](#34-starting-aptitudes)
   - 3.5 [Attributes](#35-attributes) (+ Trial) · 3.6 [Skills](#36-skills) · 3.7 [Initial Talent](#37-initial-talent) · 3.8 [Starting Spells and Techniques](#38-starting-spells-and-techniques)
4. [Combat](#4-combat) — Movement, Initiative, PA, Defense, Attack/Damage, PV, Conditions, Death
5. [Exploration](#5-exploration) — Turns, Vision, Navigation, Traps, Rest, Resources
6. [Magic and Martial Techniques](#6-magic-and-martial-techniques) — Schools, Examples, Free-Form Magic, Techniques
7. [Equipment and Crafting](#7-equipment-and-crafting) — Rarity, Properties, Crafting, Durability
8. [The Guild (the Patron's view)](#8-the-guild-the-patrons-view) — Facilities, Staff, Economy
9. [Interlude — The Time Between Expeditions](#9-interlude--the-time-between-expeditions)
10. [Glossary](#10-glossary)

---

## 1. The World in a Few Words

In the past, various deities created independent universes. Many were destroyed by wars, cataclysms, or the natural end of their cycle — but a destroyed universe never disappears completely: it leaves behind a **Dimensional Fragment**, which tends to collide with other realities.

To contain this, the deities built a **Central World** with **Gates** — structures that imprison each Fragment. Each Gate contains a **Dungeon**, and each of its floors is a preserved piece of a dead universe (which is why floors can have completely different biomes, technologies, and creatures from one another).

Fragments accumulate constant pressure to return to the real world. Exploring the Dungeon reduces this pressure. If stability is lost, a **Rupture** occurs — part of the Dungeon invades the Central World.

**Guilds** are permanent institutions responsible for maintaining the stability of a Gate. Each player, in the administrative role, is a **Patron** — they made a direct pact with a deity that grants them authority over the Guild, in exchange for permanent responsibility for the Gate's stability. The Patron can never cross the Gate; if they die without a legitimate successor, the Guild loses authority and stability collapses.

### The Three Roles

- **Player** — you, sitting at the table.
- **Patron** — your permanent representation on the Guild Council; manages the Guild during the Interlude; never enters the Dungeon.
- **Character** — the adventurer you recruit to explore the Dungeon. They're expendable from the institution's point of view — you don't "are" the character, you're a Patron who sends successive characters to fulfill the pact.

```
Player → Patron → Guild → Gate → Dungeon → Characters
```

---

## 2. How the System Works

- **Dice**: the system uses **2d10** (two ten-sided dice, summed) for all tests.
- **Opposed Tests**: used when there's direct opposition (combat, stealth vs. perception). Whoever rolls the higher result wins.
- **Absolute Tests**: against a fixed difficulty (perception, research, crafting, climbing). Success when the result ≥ difficulty.

| Difficulty | Value |
|---|---:|
| Trivial | 8 |
| Easy | 12 |
| Moderate | 16 |
| Difficult | 20 |
| Very Difficult | 24 |
| Heroic | 28 |
| Legendary | 32 |

The **Success Margin** (the difference between your result and the difficulty) matters: the greater the margin, the better the effect (Success → Great Success → Extraordinary Success). The opposite is true for failures (Failure → Critical Failure).

**Rankings**: your rank evolves in steps — Bronze → Iron → Steel → Silver → Gold → Mithril → Adamant → Legendary. You advance through **achievements** (reaching a certain floor, important feats), never through simple accumulation of experience points.

**Power Level (NP)**: a number calculated behind the scenes to balance the game. You can look it up, but you never use it directly at the table.

---

## 3. Character Creation

### Step by Step
```

1. Origin              → +25 skill pts (15+10), benefit, equipment, narrative hook
2. Background          → benefit + complication (no skill/attribute)
3. Lineage             → cap adjustment on 2 attributes + 1 racial trait
4. Aptitudes (2)       → ease of learning + natural instinct
5. Attributes          → 20 points, free purchase, min 1 / max 5 (or 6/4 if adjusted by Lineage)
6. Starting Skills     → those from Origin already apply; distribute any extra points
7. Initial Talent (1)
8. Equipment           → those from Origin + whatever the Guild provides
9. Power Level         → must fall in the Bronze range (40–70)
10. Guild Registry     → name, registration number, Ranking (Bronze), Formation Debt, date of joining
```

Every character starts as a **Recruit** of the Guild and with a **Formation Debt** — a fixed value equal to the cost of the basic equipment, training, and lodging the Guild provided you. This debt is automatically deducted from your share of the rewards on every expedition, until it's paid off — it never blocks your progression.

### 3.1 Origin
Your social/professional past. Every Origin grants: 1 light mechanical benefit, **1 primary skill (15 points) + 1 secondary skill (10 points)**, 0-2 starting equipment items (never above Uncommon rarity), and a narrative hook.

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

> 🛠️ **Homebrew**: want an Origin that isn't on the list? **Consult your GM.** Every new Origin needs: 1 light benefit, exactly 15+10 skill points, 0-2 simple equipment items, and a narrative hook — your GM will check whether it makes your character *different*, not *better*, than the 20 official ones.

### 3.2 Background
A specific event that marked your character — it **never grants a skill or attribute**, only a situational benefit and a complication of equivalent weight.

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

> 🛠️ **Homebrew**: want to create your own Background? **Consult your GM.** They'll check whether the benefit and the complication have equivalent weight, and whether the complication is something that can come back during the campaign (otherwise it's not a valid complication — it's just decoration).

### 3.3 Lineage (Race/Species)
Your ancestry adjusts the **cap** of two attributes (never the points spent) and grants 1 innate trait.

| Lineage | Racial Adjustment | Racial Trait |
|---|---|---|
| Human | None (all attributes at the standard cap of 5) | Adaptable: can swap 1 Aptitude chosen at creation, 1x during the campaign |
| Dwarf | +1 max. Vigor / −1 max. Control | Resistance to poisons and disease |
| Elf | +1 max. Perception / −1 max. Body | Low-light vision |
| Half-Orc | +1 max. Body / −1 max. Intellect | 1x per expedition, ignores a light-wound penalty |
| Halfling | +1 max. Control / −1 max. Presence | -1 difficulty on Stealth tests |
| Gnome | +1 max. Intellect / −1 max. Vigor | -1 difficulty on the first test of any newly learned Crafting skill |
| Half-Elf | You freely choose which attribute gets +1 and which gets −1 | The extra Aptitude can be swapped 1x (versatility) |
| Dragonborn | +1 max. Presence / −1 max. Control | Resistance to one elemental type (chosen at creation) |
| Shadow Descendant | +1 max. Will / −1 max. Presence | Resistance to supernatural fear |
| Fragmented *(rare, requires GM approval)* | +1 max. Affinity / −1 max. Vigor | Senses the proximity of Ruptures and dimensional instability |

> 🛠️ **Homebrew**: new Lineages **always require GM approval** — they need a net adjustment of +1/−1 on a pair of attributes, exactly 1 racial trait, and never grant a skill.

### 3.4 Starting Aptitudes
Choose **2 Aptitudes**, from the 6 below. Each one eases learning within its domain: skills in that domain rise one category on the Learning Curve when learned from scratch, and you gain **-1 degree of difficulty** on Absolute Tests with skills in the domain that are still "Untrained."

| Aptitude | Skill Areas Covered |
|---|---|
| Combat | Combat — Weapons, Combat — Defense, Unarmed Combat, Ranged Combat |
| Exploration | Exploration |
| Knowledge | Knowledge, Healing |
| Craft | Crafting, Alchemy |
| Magic | Magic |
| Leadership | Social |

An Aptitude never blocks anything: without an Aptitude in Magic you can still become a mage, you'll just have a harder start.

> 🛠️ **Homebrew**: for a narrower Aptitude (e.g., splitting "Magic" into two lines), **consult your GM** — the new domain needs to be a clear subset of already-existing Skill Areas.

### 3.5 Attributes
Eight attributes — four physical, four mental. **Modifier = Attribute − 2.**

**Physical**: Body (strength, carrying capacity, impact) · Control (coordination, precision, reflexes) · Vigor (endurance, stamina, recovery) · Presence (posture, courage, command of space).
**Mental**: Intellect (logic, learning, memory) · Perception (observation, reading the environment) · Will (discipline, self-control) · Affinity (connection to the supernatural, understanding of magic).

Distribute **20 points** freely among the 8 attributes, minimum 1 and maximum 5 each (6/4 if your Lineage adjusts it). Attributes evolve rarely — only through real physical/mental change (months of training, extreme trials), never through simple use in combat.

### How to Raise an Attribute — Trial
Unlike training a skill (guaranteed day-to-day progress), raising an Attribute requires a **Trial**: a dedicated Interlude project, tied to the specific attribute. You can only have **1 active Trial at a time**.

```
Trial Time = Current Grade × 10 days
Resource Cost = Current Grade × 5 (Pact Coins or equivalent materials)
```

| From Grade → To Grade | Time | Cost |
|---|---:|---:|
| I → II | 10 days | 5 |
| II → III | 20 days | 10 |
| III → IV | 30 days | 15 |
| IV → V | 40 days | 20 |

You need a Guild facility with Level ≥ your current Grade in the attribute (see the table below). At the end of the time, an Absolute Test against Difficulty **Difficult + (Current Grade × 2)** decides the result — failing doesn't block you, it only costs half the resources, and you can try again.

| Attribute | Trial | Test Skill | Minimum Facility |
|---|---|---|---|
| Body | Extreme Endurance | Body (raw) | Training Field |
| Control | Absolute Precision | Primary weapon/style skill | Training Field |
| Vigor | Stamina Trial | Survival | Infirmary |
| Presence | Trial of Dominance | Leadership/Intimidation | Military Academy |
| Intellect | Intellectual Trial | Arcane Theory/History | Library |
| Perception | Sensory Trial | Perception | Library/Training Field |
| Will | Discipline Trial | Will (self) | Military Academy |
| Affinity | Arcane Trial | Magical Control/Rituals | Arcane Laboratory |

Beyond Grade V, you need **Transcendence** (blessings, rituals, divine events) — a normal Trial never exceeds your natural cap.

### 3.6 Skills
Structured in three layers: **Skill Area → Skill → Specialization** (chosen upon reaching 25 points/Adept).

- **Combat — Weapons** *(Control; Body for brute-force blows)*: Swords, Axes, Hammers, Spears, Improvised Weapons, Exotic Weapons.
- **Combat — Defense** *(Control/Vigor)*: Shields, Armor, Dodge, Block.
- **Unarmed Combat** *(Body/Control)*: Martial Arts, Unarmed Fighting, Grappling.
- **Ranged Combat** *(Control)*: Bows, Crossbows, Thrown Weapons.
- **Exploration** *(Perception/Vigor/Control)*: Perception, Tracking, Survival, Navigation, Stealth, Traps, Dungeon Exploration, Climbing, Swimming.
- **Knowledge** *(Intellect)*: History, Geography, Creatures, Religion, Languages, Strategy, Dungeonology, Animal Lore, Occultism, Appraisal.
- **Healing** *(Intellect/Perception)*: Medicine, Surgery, Pharmacology.
- **Crafting** *(Control/Intellect)*: Smithing, Carpentry, Tailoring, Engineering, Construction, Equipment Making, Cooking.
- **Alchemy** *(Intellect)*: Potions, Poisons, Materials, Transmutation.
- **Magic** *(Affinity)*: Magical Control, Arcane Theory, Rituals, Elemental Affinity, Enchantments.
- **Social** *(Presence/Intellect)*: Diplomacy, Leadership, Trade, Intimidation, Manipulation.

**Learning Curve**: learning something new is easier the greater the correlation with what you already master (e.g., Short Sword → Rapier is easy; Sword → Magic barely helps at all).

| Points | Grade |
|---|---|
| 0 | Untrained |
| 10 | Basic |
| 25 | Adept |
| 50 | Expert |
| 75 | Master |
| 100 | Legendary |

**Every day of training during the Interlude generates progress in the trained skill.** Since each real day between sessions equals 1 day of Interlude, the pace is deliberately slow at the start — and speeds up as your Guild invests in infrastructure:
```
Training Points/day = (1 + Facility Bonus + Instructor Bonus) × Learning Curve Multiplier
```

- Base: **1 point/day**.
- Relevant Facility Bonus: `Level × 0.5` (Level × 1 if it's an advanced facility dedicated to it, such as a Military Academy for Combat).
- Dedicated Instructor Bonus: **+1**.
- Learning Curve Multiplier: High ×1.5 | Medium ×1.0 | Low ×0.5 | No correlation at all (still in the Initial Learning Phase, 0-50 points) ×0.25.

*Example*: training something of Medium Correlation at a Level II Training Field, with no instructor: `(1+1) × 1.0 = 2 points/day`.

**Penalty while "Untrained"**: until you reach Basic (10 points), your skill doesn't give +0 — it gives **-2**. This is the natural extension of the same Grade table used in Attack (§4.5):

| Points | Grade | Grade Bonus |
|---|---|---:|
| 0–9 | Untrained | **-2** |
| 10–24 | Basic | +0 |
| 25–49 | Adept | +1 |
| 50–74 | Expert | +2 |
| 75–99 | Master | +3 |
| 100+ | Legendary | +4 |

That -2 applies to any test that uses that skill (attack, damage, related tests). If you have the domain's Aptitude, the test's Difficulty is already 1 degree easier while Untrained — it helps a lot, but doesn't eliminate the risk of trying something from scratch.

**Training while Untrained**: during this phase (0-9 points), Facility/Instructor bonuses **don't apply** — nothing speeds up the absolute beginning of learning. Instead, you have a fixed cap by Correlation:

| Correlation | Points/day Cap | Days to Basic |
|---|---:|---:|
| None | 1 | 10 days |
| Low | 2 | 5 days |
| Medium | 3 | ~4 days |
| High | 5 | 2 days |

As soon as you reach Basic (10+), the cap disappears and the full formula (with Facility/Instructor) takes effect.

There's no limit to *knowing* skills, but there is a limit to *maintaining excellence* in many at once (Technical/Intellectual Capacity, tied to your attributes).

> 🛠️ **Homebrew**: need a skill outside the list? The official list is closed for balancing, but **Custom Skills** exist — **consult your GM** to validate.

### 3.7 Initial Talent
Choose **1 Initial Talent**, with no prerequisites. It's always more modest than a Talent earned later in play.

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
| 13 | Elemental Touch | Arcane | Generates a cosmetic/minimal elemental effect, without spending PA |
| 14 | Arcane Memory | Arcane | 1x per research project, reduces the required time by 1 day |
| 15 | Steady Presence | Social | +1 on Intimidation/Leadership tests when outnumbered |
| 16 | Trusted Voice | Social | 1x per interlude, obtains a piece of information from an NPC without needing a test |
| 17 | Natural Diplomat | Social | -1 difficulty on the first Diplomacy test with an unknown faction |
| 18 | Recruit's Luck | Extraordinary | 1x per expedition, turns a (non-critical) Failure into a plain Success |
| 19 | Strange Mark | Extraordinary | A small, unexplained supernatural trait (defined with the GM) |
| 20 | Protected Fate | Extraordinary | 1x per entire campaign, survives a blow that would have killed them, becoming Incapacitated instead of dead |

> 🛠️ **Homebrew**: **consult your GM** before creating your own Initial Talent — it needs a unique, one-off effect, and can never grant permanent extra PA or raise an attribute.

### 3.8 Starting Spells and Techniques
Without a special rule, nobody would start with a usable spell/technique (they require Adept skill or higher). That's why:

- **Aptitude in Magic** → you know **2 Minor Complexity Spells** (+1 extra if your Origin is also arcane).
- **Aptitude in Combat** → you know **1 Stance + 1 Technique (stage I)**, from a style compatible with your primary skill.
- Without these Aptitudes, but still want 1 spell/technique anyway → **swap your Initial Talent** for 1 Minor Spell or 1 basic Technique/Stance.

Using these spells/techniques still costs PA normally — the rule only frees up the knowledge.

---

## 4. Combat

### 4.1 Movement

- **Small combats** (few combatants per side) → **Grid/Hex**, measured in squares. Your Movement is `4 + Mod(Vigor)` squares per PA spent Moving.
- **Large-scale combats** (hordes, battles) → **Zones** (Contact/Short/Medium/Long), 1 PA per adjacent zone.

| Zone | Grid/Hex (squares) | Range penalty |
|---|---|---|
| Contact | 0–1 | Ranged weapons suffer a large penalty |
| Short | 2–6 | Ideal range for most bows/crossbows |
| Medium | 7–12 | -1 additional degree of difficulty |
| Long | 13+ | -2 additional degrees of difficulty |

Cover: **Light** (+2 Passive Defense) | **Partial** (+4 Passive Defense, half damage if hit) | **Total** (impossible to hit).

### 4.2 Initiative
`Initiative = 2d10 + Mod(Control)`. Descending order; ties resolved by higher Perception.

### 4.3 Actions and Action Points (PA)
You have **3 PA per turn** + **1 Reaction**. Actions: Move (1 PA/zone), Attack (1-2 PA depending on the weapon), Defend (1 PA, triggers Active Defense), Use Item (1 PA), Ready Action.

**Opportunity Attacks** don't exist as their own mechanic — use your Reaction to "Intercept" an enemy that leaves your Contact Zone carelessly.

### 4.4 Defense
By default, your **Passive Defense** already protects you at no PA cost:
```
Passive Defense = 10 + Mod(Control) + Equipment Bonus (armor) + Equipment Bonus (shield)
```
If you want to defend actively, spend 1 PA (the Defend action) or your Reaction — the attack becomes a real Opposed Test, where you roll against the attacker.

### 4.5 Attack and Damage
```
Attack = 2d10 + Attribute Grade Bonus + Skill Grade Bonus
  Attribute Grade Bonus = Attribute (score) − 1   [Grade I=+0 | II=+1 | III=+2 | IV=+3 | V=+4]
  Skill Grade Bonus  = Basic +0 | Adept +1 | Expert +2 | Master +3 | Legendary +4

Damage = Weapon die + Mod(Attribute) + Skill Grade Bonus + Equipment Bonus (weapon)
  Light Weapons: 1d6 | Medium: 1d8 | Heavy: 1d10 | Two-Handed: 2d6

Armor Damage Reduction: Light -1 | Medium -2 | Heavy -3 (a minimum of 1 damage always gets through)
```
Your equipment never improves your hit rate — only your Damage and your Defense.

The Success Margin modifies damage: Success = normal | Great Success = +1 extra die | Extraordinary Success = +2 extra dice.

### 4.6 Hit Points
```
PV = 10 + (Vigor × 2) + Ranking Bonus
Ranking Bonus: Bronze +0 | Iron +5 | Steel +10 | Silver +15 | Gold +20 | Mithril +25 | Adamant +30 | Legendary +35
```
Natural recovery only really happens during the Interlude; inside the Dungeon, a short rest recovers only a small fraction.

### 4.7 Conditions
Lightly Wounded, Gravely Wounded, Bleeding, Stunned, Weakened, Frightened, Immobilized, Dying, Dead.

### 4.8 Death
Upon reaching 0 PV, you become **Dying** (unconscious; a Medicine test can stabilize you). Any additional damage taken while Dying causes **instant death** — there is no narrative protection. Upon death, your character drops a Memory Crystal (§9).

---

## 5. Exploration

- **Exploration Turn = 10 minutes.** Outside of combat, time passes in this unit.
- **Vision**: Lit (no penalty) | Dim (-1 degree on visual tests/ranged attacks) | Total Darkness (visual tests impossible, movement halved). A torch lasts 6 Turns (1 hour).
- **Navigation**: your skill keeps the route; a Critical Failure leaves you lost (costs 1 extra Turn and risks an encounter).
- **Traps**: Detection and Disarming are Absolute Tests; a failure never blocks exploration, it only generates a consequence.
- **Group exploration**: suggested roles — Scout, Bodyguard, Navigator, Specialist. Splitting the group into subgroups reduces your local Party Power if an encounter happens.
- **Rest**: a Short Rest (1 Turn) recovers a small fraction of PV; a Full Camp recovers more, but requires a location with no active Pressure and consumes food/water. Resting always costs time.
- **Dungeon Pressure**: your GM tracks a growing tension counter on each floor (Stable → Aggravated → Critical → Collapse). You don't see the exact number, but you'll feel the effect: the more time your group spends or the more noise it makes, the more dangerous the Dungeon becomes.

**Resource Consumption**:

| Resource | Consumption |
|---|---|
| Food | 1 ration/character per day |
| Water | 1 canteen/character per day (doubles in arid environments) |
| Torch | 1 unit per 6 Exploration Turns |
| Rope | Per specific use (climbing, pits) |
| Ammunition | 1 unit per ranged attack |
| Carrying Capacity | `Body × 5` (weight); exceeding it penalizes movement and physical tests |

Running out of food/water generates the Hungry/Dehydrated Conditions — never kills you directly, but debilitates you seriously.

---

## 6. Magic and Martial Techniques

### 6.1 Schools of Magic

| School | Focus |
|---|---|
| Evocation | Direct damage, energy, elements |
| Abjuration | Protection, shields, resistances |
| Control | Debuffs, immobilization, area control |
| Conjuration | Summoning creatures/objects |
| Transmutation | Altering form/matter |
| Illusion | Deceiving the senses, disguises |
| Necromancy | Manipulating life/death, drain, corruption |
| Divination | Information, detection, precognition |

Every spell has: School, PA Cost, Range (Zone), Area, Duration (Instantaneous/Turns/Scene/Persistent) and Test (Opposed or Absolute).

| Complexity | PA |
|---|---:|
| Minor | 1 |
| Moderate | 2 |
| Major | 3 |
| Supreme | Extended Casting (multiple turns) |

Your Grade in Magical Control reduces the cost: Expert -1 PA | Master -1 PA and -1 Turn | Legendary -2 PA and -1 Turn. During Extended Casting, taking damage or failing a Will Test interrupts the spell (the PA spent is lost).

### 6.2 Example Spells

| School | Minor (1 PA) | Moderate (2 PA) | Major (3 PA) |
|---|---|---|---|
| Evocation | Fire Lance | Flaming Blast | Flame Storm |
| Abjuration | Arcane Shield | Protective Barrier | Absolute Wall |
| Control | Bonds of Will | Arcane Shackles | Prison of Will |
| Conjuration | Spectral Blade | Battle Familiar | Summoned Avatar |
| Transmutation | Warping Touch | Partial Metamorphosis | Complete Transfiguration |
| Illusion | Deceptive Mist | Illusory Duplicate | Veil of Lies |
| Necromancy | Enfeebling Touch | Shadow Breath | Call of the Grave |
| Divination | Glimpse | Reading the Thread of Fate | All-Seeing Eye |

> 🛠️ **Homebrew (New Spells)**: **consult your GM** before creating a new spell. The process: (1) choose a School; (2) choose the Complexity (already fixes cost and power ceiling); (3) define Range, Area, Duration, and Test; (4) define a Single Effect, written in terms of already-existing mechanics. Your GM will check that you're not stacking too many effects onto a low Complexity.

### 6.3 Intuitive Magic (Free-Form Magic)
With at least 1 point in Magical Control, you can try to produce, on the spot, a magical effect you don't know formally — if it fits a School you practice.

- **Cost**: +1 PA over the Complexity estimated by the GM.
- You make an **extra Absolute Test of Magical Control** to "assemble" the spell on the spot.
- Failure = PA lost, no effect. Critical Failure = a consequence (a light Condition, damage, or a Tension spike).
- Never reproduces a Supreme effect; never creates a permanent physical item.
- If it works, your GM can formalize it as a **Discovered Spell** — it becomes officially known, at no extra research cost.

### 6.4 Martial Techniques
Each combat style has its own tree: **Stances** (passive, 1 PA to activate, free afterward) · **Techniques** (active, 1-2 PA, can evolve from I to II) · **Reactions** (use your Reaction) · **Supreme Techniques** (3 PA, limited use).

| Category | Minimum Skill | Minimum Ranking |
|---|---|---|
| Stance | Adept (25) | — |
| Technique | Expert (50) | — |
| Reaction | Expert (50) | — |
| Supreme Technique | Master (75) | Silver+ |

**Examples (Swords)**: Offensive Stance (+1 damage/-1 Defense) · Spinning Strike I/II (hits multiple targets in Contact) · Parry (Reaction) · The Veil-Splitting Cut (Supreme).
**Examples (Unarmed Combat)**: Closed Guard · Joint Strike I/II · Counterstrike (Reaction) · Rupture of Vital Points (Supreme).
**Examples (Bows)**: Calculated Aim · Chained Shot I/II · Interception Shot (Reaction) · The Veil-Piercing Arrow (Supreme).

> 🛠️ **Homebrew (New Techniques)**: **consult your GM**. Step by step: (1) choose the base Style/Weapon; (2) choose the Category (already fixes PA and minimum skill); (3) define the Effect in terms of existing mechanics. If the technique has a stage II, it always requires Master skill and +1 more PA than stage I.

---

## 7. Equipment and Crafting

### 7.1 Rarity

| Rarity | Max Properties | Base Bonus (Damage/Defense) | NP |
|---|---|---|---:|
| Common | 0 | +0 | 1 |
| Uncommon | 1 | +1 | 3 |
| Rare | 2 | +2 | 7 |
| Epic | 3 | +3 | 15 |
| Legendary | 4 | +4 | 30 |
| Divine | 5+ | +5 or unique effect | 50+ |

**Categories**: Weapons, Armor, Shields, Tools, Consumables, Artifacts, Relics.

### 7.2 Properties (official list of 20)
Sharp · Precise · Sturdy · Light · Flaming / Frost / Corrosive · Piercing · Vampiric · Resonant · Camouflaged · Warded · Unstable · Regenerative · Silent · Anchored · Adaptive · Amplifying · Shattering · Sealing · Cursed.

> 🛠️ **Homebrew (New Properties)**: **consult your GM.** Every property occupies exactly 1 slot, and must be equivalent to +1 damage die OR -1 degree of difficulty in a specific niche OR a one-off reusable resource OR resistance to 1 Condition — never more than that. Very strong properties require an always-active penalty.

### 7.3 Crafting
```
Absolute Test (Crafting Skill) vs. Recipe Difficulty
```

| Target Rarity | Time | Material Cost | Minimum Facility |
|---|---|---:|---|
| Common | 1 day | 5 | Basic Workshop |
| Uncommon | 3 days | 15 | Basic Workshop |
| Rare | 7 days | 35 | Smithy |
| Epic | 14 days | 75 | Advanced Smithy |
| Legendary | 30 days | 150 | Rune Forge |
| Divine | Requires a prior Research project | 250 + 10 Pact Coins | Divine Forge |

You need the **Known Recipe** or a **Discovered Project** — you can't craft a rarity without the corresponding recipe.

### 7.4 Upgrading, Modification, and Reconstruction
**Upgrading** strengthens the Base Bonus within the same rarity. **Modification** swaps 1 Property for another of equivalent cost. **Reconstruction** raises the item to the next rarity, with half the time of crafting it from scratch.

### 7.5 Durability — Wear Hits
Your item loses 1 Wear Hit only on a Critical Failure of an attack/defense, or on a narrative event (trap, corrosion).

| Rarity | Wear Hits |
|---|---:|
| Common | 3 |
| Uncommon | 4 |
| Rare | 5 |
| Epic | 6 |
| Legendary | 8 |
| Divine | 10 |

Once exhausted, the item becomes Damaged (-1 to the Base Bonus) until repaired during the Interlude.

---

## 8. The Guild (the Patron's view)

Your Guild has its own sheet with: Identity (name, coat of arms, patron deity), Prestige, Influence, Resources (Pact Coins, materials), Headquarters (facilities), Staff, accumulated Knowledge, active Doctrines, Logistics, an Expedition record, and historical Legacy.

**Headquarters**: the buildings form a real tech tree — each one has prerequisites, costs, benefits, and synergies with others. At the start of the campaign only the Gate, Dormitory, and a basic Training Field exist; everything else is built by the Council of Patrons.

| Facility | What it unlocks for you |
|---|---|
| Dormitory | Slots for characters and workers |
| Warehouse | Storage capacity for resources |
| Training Field | Combat training; Body/Control Trial |
| Smithy | Crafting weapons/armor |
| Workshop | Crafting general items and tools |
| Library | Research; Intellect/Perception Trial |
| Infirmary | Better healing; Vigor Trial |
| Arcane Laboratory | Advanced arcane research; Affinity Trial; Enchantments |
| Military Academy | Presence/Will Trial; Supreme Techniques |
| Alchemical Garden | Advanced Alchemy (poisons, transmutation) |
| Rune Workshop | Epic+ items; weapon enchantment |
| Memorial | Access to Memory Crystals of dead characters |
| Mage Tower | Research and rituals at the highest level |

Ask your GM for the complete tree (prerequisites and costs) if you want to plan construction in advance.

**Staff and Mercenaries**: the Guild employs Laborers, Artisans, Researchers, Instructors, Merchants, Physicians, and Administrators. Mercenaries can patrol, gather, and explore — but **only floors already conquered**; they never enter unknown territory in your place.

**Economy**: common currency + materials + **Pact Coins** (a special divine currency, obtained in the Dungeon). Every expedition reward is split between Character / Guild / Strategic Reserve.

**Doctrines**: permanent specializations of the Guild's philosophy (Military, Academic, Commercial, Exploration, Arcane, Engineering, Logistics, Diplomatic) — they grant global bonuses and give your organization a unique identity.

---

## 9. Interlude — The Time Between Expeditions

The **Interlude** is the period between two of your character's expeditions, when they stay at the Headquarters. Every activity consumes time and produces specific progress:

1. **Training** — guaranteed, fixed progress per day, improved by facilities/instructors.
2. **Research** — Discover → Research → Master → Apply.
3. **Production and Crafting** — item crafting (§7.3).
4. **Guild Administration** — institutional management (if your character/Patron takes part in it).
5. **Secondary Expeditions** (mercenaries) — always limited to floors already conquered.

**Death and Legacy**: if your character dies, they drop a **Memory Crystal** — accessible at the **Memorial**, without automatically transmitting attributes/skills, only concrete knowledge lived (maps, languages, puzzle solutions). A new character never starts from absolute zero: they receive training compatible with how much your Guild has already evolved.

---

## 10. Glossary

- **Patron** — you, in the administrative role.
- **Character** — the adventurer you recruit.
- **Rupture** — dimensional collapse when a floor escapes containment.
- **Memory Crystal** — the posthumous record of a dead character.
- **NP** — Power Level (individual).
- **PA** — Action Points.
- **Grade** — level of mastery of an attribute or skill (Basic → Legendary).
- **Ranking** — your rank within the Guild (Bronze → Legendary).
