# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Ruptura** is a tabletop RPG system (dungeon crawler hardcore), fully documented in Brazilian Portuguese. This is a pure documentation repository — there is no code, build system, or test suite. All files are Markdown documents and PDF character sheets.

> Core concept: *Um dungeon crawler hardcore onde os jogadores administram, como Conselho, uma Guilda permanente de exploradores a serviço de uma divindade; a Guilda é a verdadeira protagonista da campanha, e os personagens que descem à Dungeon são recursos valiosos, porém descartáveis.*

## Document Structure

```
docs/
  GDD_Ruptura.md          ← Master reference (authoritative; 1,500+ lines)
  manuais/
    Manual_do_Jogador.md  ← Player-facing rules (references GDD, never repeats it)
    Manual_do_Mestre.md   ← GM-facing tools (references GDD, never repeats it)
  fichas/
    Ficha_de_Personagem.pdf
    Ficha_de_NPC.pdf
    Ficha_de_Criatura.pdf
    Ficha_da_Guilda.pdf
```

**Source of truth hierarchy**: GDD > Manuais. When the manuals conflict with the GDD, the GDD wins. When making changes, update the GDD first and propagate to the manuals.

## Design Filter

Every rule addition or modification must pass this filter: *does it strengthen the identity of a hardcore dungeon crawler where the Guild — not individual characters — is the true protagonist?* If not, it probably doesn't belong.

## The 16 Design Principles (must not be violated)

1. **Dominância da Dungeon** — progress outside the Dungeon never exceeds what exploring gives. `Dungeon >>> Interlúdio >>> Inatividade.`
2. **Especialização** — all evolution comes from the activity practiced; no universal XP.
3. **Origem dos Modificadores** — every bonus/penalty needs an identifiable source.
4. **Regra de Ouro** — no activity generates unlimited progress without consuming a limited resource.
5. **Simetria** — the same rules apply to players and the world (NPCs, factions, creatures).
6. **Progressão Linear** — base progress is fixed; bonuses modify, never scale with Ranking.
7. **Fracassos como Consequência** — failure never blocks the campaign, it generates consequences.
8. **Coerência Narrativa** — narrative justifies mechanics, never replaces them.
9. **Instituição Permanente** — the Guild never fully retreats; the character is replaceable, the organization is not.
10. **Marcos** — evolution is perceptible at clear milestones.
11. **Limite Natural** — every attribute/skill has a natural cap (Grau V); exceeding it requires Transcendência.
12. **Escala de Conflito** — mass conflicts follow the same fundamental rules, at different scale.
13. **Automatização/Fronteira da Exploração** — NPCs and mercenaries never replace players; they only act in already-conquered areas.
14. **Mundo Vivo** — the world evolves on its own during player absence.
15. **Progressão Irreversível** — completed floors are not replayed by player characters.
16. **Domínio** — true victory is permanent influence (Ativos Estratégicos), not just survival.

## Closed Lists (treat as final unless explicitly reopening)

Several lists are explicitly marked **FECHADA** (closed) in the GDD and should not be expanded without deliberate design discussion:

- **Origens** (20 official) — GDD §6.1.2
- **Históricos** (20 official) — GDD §6.1.4
- **Aptidões** (6 official) — GDD §6.1.5
- **Talentos Iniciais** (20 official) — GDD §6.1.6
- **Linhagens** (10 official) — GDD §6.1.7
- **Escolas de Magia** (8 official) — GDD §6.6.1
- **Perícias Fundamentais** — GDD §6.4 (personalized skills exist but require GM validation)
- **Facções** — GDD §13

## Key Mechanical Relationships

- **Atributo** = capacity ("can they?") — never grants skills automatically.
- **Perícia** = experience ("do they know how?") — trained separately, drives test bonuses.
- **Talento** = binary (have or don't have); rare and meaningful, never a generic accumulation.
- **NP (Nível de Poder)** = behind-the-scenes balance number; players consult but never use directly.
- **Ranking** = character's guild rank (Bronze→Lendário); advances by achievements, never by XP accumulation.
- **CG (Capacidade da Guilda)** = institutional power; deliberately decoupled from combat calculations (never added to PG or OA).

## Homebrew Validation Checklists

Each content type has a mandatory checklist in the GDD. When drafting new content, always apply the relevant one:

- **Nova Origem** — §6.1.1: exactly 15+10 skill points, 1 light mechanical benefit, Regra do Não-Superior.
- **Novo Histórico** — §6.1.3: benefit and complication must be equivalent weight; complication must be a viable narrative hook.
- **Nova Linhagem** — §6.1.7: net +1/−1 on exactly one pair of attributes; exactly 1 racial trait (weight = Talento menor, NP=1); never grants skills.
- **Nova Magia** — §6.6.4: one well-defined effect per spell (Regra do Efeito Único); scaling Area/Duration/Range beyond the Complexity standard costs +1 PA or forces a higher Complexity tier.
- **Nova Técnica** — §6.6.7: one defined effect; Supremas always limited to 1×/combat or expedition; only compatible with the corresponding weapon category.
- **Nova Criatura** — §5.8: 1 mandatory weakness; 1 primary function; NP total must not exceed the category ceiling by more than 15%.

## Encounter Balancing Quick Reference

```
PG = Σ NP(personagens) × Fator de Sinergia
PE = Σ NP(criaturas) × Quantidade × Inteligência × Terreno × Objetivo
R  = PE / PG   →  ≤0.5 trivial | 1.0 balanced | 1.5 very hard | ≥3 probable death

FCE (Fator de Compressão): Bronze–Ferro 0.40 | Aço–Prata 0.25 | Ouro–Mithril 0.15 | Adamante–Lendário 0.10
Multiplicador Real = 1 + (R − 1) × FCE
```

The FCE is validated by Monte Carlo simulation (500 combats/cell, 8 Rankings, heterogeneous groups). Do not adjust it without re-running the simulation.

## Language

All game content and documentation is written in **Brazilian Portuguese**. Maintain this when editing or adding to any document in `docs/`.
