# MANUAL DO MESTRE
### RPG Dungeon Crawler Hardcore

> Este manual reúne tudo que você precisa para conduzir a campanha: filosofia de design, cosmologia completa, construção de conteúdo (andares, criaturas, encontros) e as ferramentas de calibração validadas em playtest. Regras de personagem, combate, exploração, magia e equipamentos que os jogadores usam diretamente estão detalhadas no **Manual do Jogador** — este manual referencia essas seções em vez de repeti-las.

---

## Sumário

1. [Filosofia do Sistema](#1-filosofia-do-sistema) — Conceito Central, Pilares, os 16 Princípios de Design
2. [Cosmologia Completa](#2-cosmologia-completa)
3. [Estrutura de Campanha](#3-estrutura-de-campanha) — Arcos, Andares, Andares Especiais
4. [A Dungeon](#4-a-dungeon) — Estrutura, Pressão (numérica), Estados, Recompensas
5. [Criaturas](#5-criaturas) — Tipos, Comportamento/IA, NP, Ficha, Manual de Criação, Bestiário
6. [Hordas e Sistema de Encontros](#6-hordas-e-sistema-de-encontros) — PG/PE, Orçamento de Ameaça, FCE
   - 6.6 [Guia de Construção de Conteúdo](#66-guia-de-construção-de-conteúdo-criatura--encontro--andar--arco--campanha) (Criatura → Encontro → Andar → Arco → Campanha)
7. [Domínio e os Quatro Pilares da Progressão](#7-domínio-e-os-quatro-pilares-da-progressão)
8. [A Guilda — Ferramentas do Mestre](#8-a-guilda--ferramentas-do-mestre)
   - 8.1 [Capacidade da Guilda (CG)](#81-capacidade-da-guilda-cg) · 8.2 [Quartel-General e Construções](#82-quartel-general-e-construções) (árvore tecnológica completa) · 8.3 [Trabalhadores/Mercenários](#83-trabalhadores-mercenários-e-departamentos) · 8.4 [Economia](#84-economia) · 8.5 Doutrinas
9. [Interlúdio](#9-interlúdio--conduzindo-o-tempo-entre-sessões) — Conduzindo o Tempo Entre Sessões
10. [Eventos Dinâmicos, Tensão e Facções](#10-eventos-dinâmicos-tensão-e-facções)
11. [Registro de Campanha](#11-registro-de-campanha)
12. [Apêndice — Fórmulas Consolidadas](#12-apêndice--fórmulas-consolidadas)
13. [Glossário Completo](#13-glossário-completo)
14. [Pendências Conhecidas](#14-pendências-conhecidas--todas-fechadas) — todas fechadas

---

## 1. Filosofia do Sistema

### Conceito Central
> Um dungeon crawler hardcore onde os jogadores administram, como Conselho, uma Guilda permanente de exploradores a serviço de uma divindade; a Guilda é a verdadeira protagonista da campanha, e os personagens que descem à Dungeon são recursos valiosos, porém descartáveis.

Use esse conceito como filtro: toda regra nova (sua ou dos jogadores) deve ser testada contra ele.

### Pilares
Dungeon crawler hardcore de alta letalidade · mundo persistente · Guilda permanente como "personagem principal" · personagens descartáveis · progressão por ações realizadas, nunca XP genérico · exploração recompensadora (informação = poder) · Interlúdio estratégico · **tempo é o recurso mais importante do jogo**.

### Os 16 Princípios de Design

1. **Dominância da Dungeon** — progresso fora da Dungeon nunca supera o que se ganha explorando. `Dungeon >>> Interlúdio >>> Inatividade.`
2. **Especialização** — toda evolução vem da atividade praticada; não existe XP universal.
3. **Origem dos Modificadores** — todo bônus precisa de fonte identificável.
4. **Regra de Ouro** — nenhuma atividade gera progresso ilimitado sem consumir um recurso limitado.
5. **Simetria** — as mesmas regras valem para jogadores e para o mundo (NPCs, facções, criaturas).
6. **Progressão Linear** — toda atividade concede progresso base fixo; bônus modificam, nunca escalam com Ranking.
7. **Fracassos como Consequência** — falhar nunca bloqueia, gera consequência.
8. **Coerência Narrativa** — a narrativa justifica a mecânica, nunca a substitui.
9. **Instituição Permanente** — a Guilda nunca retrocede; o personagem é substituível, a organização não.
10. **Marcos** — evolução perceptível em marcos claros.
11. **Limite Natural** — todo atributo/perícia tem teto (Grau V); superá-lo exige Transcendência.
12. **Escala de Conflito/Organização/Comportamento/Informação** — conflitos em massa seguem as mesmas regras, em escala diferente.
13. **Automatização/Fronteira da Exploração** — NPCs e mercenários nunca substituem os jogadores; só atuam em áreas conquistadas.
14. **Mundo Vivo** — o mundo evolui sozinho na ausência dos jogadores.
15. **Progressão Irreversível** — andares concluídos não são repetidos pelos personagens jogadores.
16. **Domínio** — a vitória real sobre a Dungeon é conquistar influência permanente (Ativos Estratégicos), não só sobreviver.

---

## 2. Cosmologia Completa

Divindades antigas criaram universos independentes; muitos foram destruídos, deixando **Fragmentos Dimensionais** que colidem com outras realidades. O **Mundo Central** contém **Portões** que aprisionam cada Fragmento — cada Portão abriga uma **Dungeon**, e cada andar é um pedaço preservado de um universo morto (biomas/tecnologias/criaturas variam livremente entre andares).

**Estabilidade Dimensional**: fragmentos acumulam pressão para retornar ao mundo real; explorar reduz essa pressão. Perder estabilidade causa uma **Ruptura** — a Dungeon invade o Mundo Central.

Cada divindade compete por influência através da eficiência das Guildas que administram seus Portões (substituindo guerra direta entre deuses). **Patronos** fizeram um pacto: nunca atravessam o Portão (são "Âncoras"), mantêm a Guilda ativa, preservam conhecimento. Se um Patrono morre sem sucessor, a estabilidade colapsa.

```
Jogador → Patrono → Guilda → Portão → Dungeon → Personagens
```

Use essa fundação para justificar organicamente: Registro da Guilda (exigência divina de controle), Rankings (certificação de quem contém instabilidades maiores), Interlúdio (preparo contínuo), Construções (capacidade operacional), Doutrinas (filosofias divinas), Cristais de Memória (conhecimento que não pode depender de um indivíduo só).

---

## 3. Estrutura de Campanha

### 3.1 Arcos
Um universo que encerrou seu ciclo (um Fragmento inteiro). Todo Arco define: tema, história, conflito, objetivo final, pressão específica, ecossistema, recursos e mecânica exclusivos, e ao menos 5 andares. Estrutura narrativa sugerida: **Introdução → Investigação → Desenvolvimento → Preparação → Clímax → Consequência.**

### 3.2 Andares
Tipos de objetivo: Exploração, Reconhecimento, Defesa, Ataque, Caça, Escolta, Sobrevivência, Puzzle, Eliminação, objetivos secretos. Classificação: **Transitórios** (passagem) · **Estratégicos** (concedem Ativos Estratégicos) · **Narrativos** (avançam a história) · **de Marco** (pontos de virada).

### 3.3 Andares Especiais
A cada 5 andares, um Andar Especial de dificuldade elevada. **Regra fixa**: os 5 andares anteriores sempre contêm as ferramentas para vencê-lo. Quem explora pouco ainda chega ao chefe; quem explora muito sobrevive a ele.

### 3.4 Progressão Irreversível
Andares concluídos não voltam a ser jogados pelos personagens (mercenários/expedições secundárias podem operar neles depois — §7 deste manual).

---

## 4. A Dungeon

### 4.1 Estrutura dos Andares
Cada andar tem: Identidade (bioma herdado do fragmento de origem), Objetivo Principal, Objetivos Secundários, Condição de Fracasso.

### 4.2 Pressão da Dungeon
Escala: **Estável → Agravado → Crítico → Colapso.** Cada tipo de andar "pressiona" de um jeito coerente com seu tema (floresta viva sufoca, andar vulcânico esquenta, fortaleza reforça defesas). Alimenta eventos, penalidades e mudanças ambientais.

**Contador numérico (0-100 por andar, reinicia a cada novo andar)**:

| Estado | Faixa | Multiplicador no PE dos encontros restantes |
|---|---:|---:|
| Estável | 0–24 | ×1,00 |
| Agravado | 25–59 | ×1,10 |
| Crítico | 60–89 | ×1,25 |
| Colapso | 90–100 | ×1,50 + dispara automaticamente um Evento de Colapso (reforços, mudança ambiental drástica, ou risco imediato à Condição de Fracasso) |

**Fontes padrão de Pressão** (ponto de partida, ajuste livremente): Turno de Exploração além da Duração prevista do andar → +5. Cada combate concluído → +10. Falha crítica em teste relevante → +15. Evento narrativo definido por você → +20 a +60.

O multiplicador de Pressão se soma aos multiplicadores de Terreno/Inteligência/Objetivo já existentes na fórmula de PE (§6.3) — a Dungeon reage à presença dos jogadores em vez de esperar parada.

### 4.3 Estado dos Andares
**Inexplorado → Explorado → Conquistado → Dominado.**

### 4.4 Recompensas e Informação
Recompensas: Conhecimento, Recursos, Progresso. Trate Informação como recurso concreto — conhecer um chefe com antecedência deve valer tanto quanto poder bruto.

---

## 5. Criaturas

### 5.1 Tipos (natureza/origem)
Bestas · Mortos-vivos · Aberrações · Espíritos · Constructos · Humanoides Corrompidos · Dracônicos · Entidades Extraplanares.

### 5.2 Função na Dungeon
Predador, Guardião, Soldado, Parasita, Evento Vivo. *(Tipo e Função se combinam livremente.)*

### 5.3 Comportamento (IA)

| Comportamento | Multiplicador (§6.3) | Regra de ação |
|---|---|---|
| Instintiva | Instinto (×1) | Ataca o alvo mais próximo/menor Defesa; nunca usa tática de grupo; foge com PV<25% ou Moral baixa |
| Inteligente | Tático (×1,2) / Militar (×1,5) | Escolhe alvo pela ameaça percebida; recua para reposicionar; usa Reação de forma otimizada |
| Estratégica | Genial (×2) | Coordena o grupo, mira fraquezas conhecidas, finge retirada, usa terreno/Cobertura, prioriza suporte/conjuradores |

### 5.4 Tabela de Características Naturais (custo em NP)

| Peso | NP | Exemplos |
|---|---:|---|
| Menor | 1 | Visão no Escuro, Olfato Apurado, Sentido Sísmico, Resistência a 1 elemento |
| Média | 3 | Carapaça (RD+2), Voo, Camuflagem Natural, Múltiplos Olhos (imune a Surpreendido) |
| Maior | 5 | Regeneração, Veneno Potente, Ataques Múltiplos |
| Suprema | 10 | Metamorfose, Núcleo Dimensional (revive 1x), Imunidade a uma categoria de dano |

### 5.5 Fórmula de NP de Criatura
```
NP(criatura) = (Atributos + Perícias Naturais) + Σ Características + Σ Habilidades + Equipamento
```
(Habilidade comum=5 / avançada=10 / suprema=20 — mesma lógica de personagens, Manual do Jogador §2)

### 5.6 Categorias de Criatura

| Categoria | Faixa de NP | Equivalente de Ranking |
|---|---|---|
| Fraca | 20–40 | Abaixo de Bronze |
| Comum | 40–70 | Bronze |
| Veterana | 70–105 | Ferro |
| Elite | 105–195 | Aço–Prata |
| Campeã | 195–340 | Ouro–Mithril |
| Chefe Menor | 340–430 | Adamante |
| Chefe de Arco | 430–550+ | Lendário |
| Entidade Superior | 550+ | Acima de Lendário |

### 5.7 Ficha Simplificada de Criatura
```
NOME (Tipo — Função — Categoria: NP XX)
Comportamento: Instintiva / Inteligente / Estratégica
PV: XX | Defesa Passiva: XX | Deslocamento: XX
Ataque principal: 2d10 + X vs Defesa | Dano: XdX+X
Características: [lista breve, 1 linha cada]
Habilidades: [lista breve]
Fraqueza: [1 linha]
Recompensas: [lista breve]
```

### 5.8 Manual de Criação de Criaturas
Passo a passo: (1) Conceito; (2) Tipo (§5.1 ou homebrew, §5.9); (3) Função (§5.2); (4) Comportamento (§5.3, já fixa multiplicador de encontro); (5) Categoria-alvo (§5.6, já define faixa de NP); (6) distribuir o NP-alvo entre Atributos+Perícias, Características (§5.4), Habilidades e Equipamento; (7) definir 1 Fraqueza obrigatória; (8) definir Recompensas; (9) validar contra a checklist.

**Checklist de Balanceamento**: **Regra da Fraqueza** — toda criatura precisa de ao menos 1, sem exceção. **Regra da Função Clara** — 1 função primária definida, nunca "monstro genérico". **Regra do Teto de Categoria** — NP total não ultrapassa a faixa da Categoria em mais de 15%.

### 5.9 Manual de Criação de Tipos Homebrew
(1) descreve a **natureza/origem**, nunca função ou comportamento; (2) precisa ser compatível com a cosmologia (§2) — que fragmento/arco ele representa; (3) **nunca concede bônus mecânico próprio** — é classificação narrativa; (4) combina livremente com qualquer Função/Comportamento.

### 5.10 Bestiário Base

| Nome | Tipo | Função | Comportamento | Categoria | Características-chave | Fraqueza |
|---|---|---|---|---|---|---|
| Goblin Saqueador | Humanoide Corrompido | Soldado | Instintiva | Fraca | Sentidos Aprimorados | Foge abaixo de 50% PV |
| Rato Pragado | Besta | Parasita | Instintiva | Fraca | Olfato Apurado | Vulnerável a fogo |
| Esqueleto Guardião | Morto-vivo | Guardião | Instintiva | Comum | Carapaça (óssea) | Vulnerável a dano contundente |
| Cultista Corrompido | Humanoide Corrompido | Soldado | Inteligente | Comum | Ritual menor | Vontade baixa (fácil intimidar) |
| Aranha das Profundezas | Besta | Predador | Instintiva | Veterana | Veneno Potente, Camuflagem Natural | Sensível a luz forte/vibração |
| Cavaleiro Corrompido | Morto-vivo | Guardião | Estratégica | Elite | Carapaça, Regeneração | Vulnerável a magia sagrada |
| Bruxa do Pântano | Aberração | Soldado (Controle) | Estratégica | Elite | Habilidade avançada de Controle | Fraca em combate corpo a corpo |
| Golem de Pedra Fragmentado | Constructo | Guardião | Instintiva | Campeã | Carapaça dupla, Imune a Veneno/Medo | Núcleo exposto |
| Comandante Espectral | Espírito | Soldado (Comando) | Estratégica | Campeã | Voo, Comando supremo (buff de horda) | Dissipa-se com luz sagrada/Selo |
| Dragão do Eclipse | Dracônico | Chefe (Soberano) | Estratégica | Chefe de Arco | Voo, Regeneração, Ataques Múltiplos, sopro | Núcleo exposto após certa fase |

---

## 6. Hordas e Sistema de Encontros

### 6.1 Hordas e Conflitos em Massa
Tipos: Enxame, Exército, Invasão, Catástrofe. Tamanho: Pequena, Média, Grande, Massiva. Cada horda tem Poder, Pressão, Origem, Comando e turnos próprios (age em blocos). Objetivos: Sobrevivência, Defesa, Escolta, Contenção, Retirada. Comando em escala (Individual → Tático → Militar → Estratégico), com Liderança, Estratégia, Conhecimento Militar, Informações e Moral próprios.

### 6.2 Escala das Criaturas frente ao Grupo
Relação NP-personagem × NP-criatura calibrada por categoria (Comum/Elite/Chefe), com **Fator de Horda** (multiplicador por quantidade de inimigos simultâneos) e regras específicas de Chefe (fases, Ações Lendárias). **Princípio da Superioridade da Dungeon**: por padrão, a Dungeon deve superar levemente o grupo — nunca ser trivial.

### 6.3 Sistema de Encontros (fórmulas de mesa)
**Poder do Grupo (PG)**:
```
PG = Σ NP(personagens) × Fator de Sinergia
```

| Nº personagens | Fator |
|---|---:|
| 1 | 1,0 |
| 2 | 1,1 |
| 3 | 1,2 |
| 4 | 1,3 |
| 5 | 1,4 |
| 6+ | 1,5 |

**Poder do Encontro (PE)**:
```
PE = Σ NP(criaturas) × Quantidade × Inteligência × Terreno × Objetivo
```

- Quantidade: 1→1 | 2-3→1,25 | 4-8→1,5 | 9-20→2 | 20+→3
- Inteligência: Instinto→1 | Tático→1,2 | Militar→1,5 | Genial→2
- Terreno: Neutro→1 | Levemente favorável→1,1 | Favorável→1,25 | Extremo→1,5
- Objetivo: Eliminar→1 | Sobreviver→1,25 | Defender→1,5 | Resgatar sob pressão→1,5 | Missão crítica→2

**Classificação do encontro**: `R = PE / PG`

| R | Dificuldade |
|---|---|
| ≤0,5 | Muito fácil |
| 0,75 | Fácil |
| 1 | Equilibrado |
| 1,25 | Difícil |
| 1,5 | Muito difícil |
| 2 | Extremo |
| ≥3 | Possível morte |

**Dificuldades separadas**: **Dificuldade de Combate (DC = PE/PG)** mede o quão duro é vencer os inimigos; **Dificuldade de Objetivo (DO)** mede o quão duro é cumprir a missão (tempo, ambiente, pressão) — são independentes.

### 6.4 Orçamento de Ameaça do Andar
```
OA = PG × Dificuldade × Fator de Duração
```

- Dificuldade: Seguro 0,75 | Normal 1,0 | Perigoso 1,25 | Mortal 1,5 | Infernal 2,0 | Apocalíptico 3,0
- Duração: Curto (1-2 encontros)→1 | Normal (3-5)→2 | Longo (6-10)→3 | Extenso→4

Distribua o OA entre criaturas, armadilhas, eventos, elite e chefe (ex.: andar de combate ≈ 70% criaturas/15% ambiente/15% eventos; andar de chefe ≈ 70% chefe/20% mecânicas/10% ambiente).

### 6.5 Fator de Compressão de Encontro (FCE) — validado por Playtest
Usar a Razão de Encontro (R) diretamente como multiplicador das estatísticas reais do inimigo cria um "penhasco" (o grupo vence quase sempre ou perde quase sempre). Ao montar as estatísticas de uma criatura/grupo para atingir uma Razão-alvo R:
```
Multiplicador Real de Atributos/Perícias = 1 + (R − 1) × FCE
```

| Ranking do Grupo | FCE |
|---|---:|
| Bronze–Ferro | 0,40 |
| Aço–Prata | 0,25 |
| Ouro–Mithril | 0,15 |
| Adamante–Lendário | 0,10 |

O FCE diminui conforme o Ranking sobe: grupos de Ranking baixo têm PV reduzido (a variância dos dados já suaviza a dificuldade sozinha); grupos de Ranking alto têm PV alto (combates mais determinísticos), exigindo compressão mais forte para preservar a gradação Favorável/Equilibrado/Desfavorável/Impossível.

> **Resultado do Playtest** (Monte Carlo, 2d10, 500 combates/célula, grupos de 4): com as correções de Combate (ver Manual do Jogador §4.5) e o FCE acima, a taxa de vitória ficou consistente nos 8 Rankings — Favorável 92-100%, Equilibrado 40-68%, Desfavorável 11-30%, Impossível ~0%.
>
> **Validação com grupos heterogêneos (FECHADA)**: repeti a simulação com grupos realistas (1 Tank, 2 Balanced, 1 DPS, NP individual variando ±20% — não builds idênticos) para garantir que o FCE não era um artefato de "personagem médio". Resultado: o FCE se manteve estável e ficou **ainda mais consistente** entre Rankings — Favorável 77-98%, Equilibrado 53-65%, Desfavorável 15-32%, Impossível 0-5%. O FCE está validado para uso direto em mesa, sem ressalvas adicionais.

### 6.6 Guia de Construção de Conteúdo (Criatura → Encontro → Andar → Arco → Campanha)

Este guia amarra todas as ferramentas já fechadas num fluxo de trabalho único. Use-o de cima para baixo ao preparar uma sessão nova, ou de baixo para cima ao improvisar em jogo.

#### Nível 1 — Criar uma Criatura

1. Escolha o **Conceito** e o **Tipo** (§5.1, oficial ou homebrew via §5.9).
2. Defina **Função** (§5.2) e **Comportamento** (§5.3) — o Comportamento já fixa o multiplicador de Inteligência que ela vai usar em qualquer encontro.
3. Escolha a **Categoria-alvo** (§5.6) — isso define a faixa de NP.
4. Distribua o NP entre Atributos/Perícias, Características (§5.4) e Habilidades até bater a faixa.
5. Defina 1 Fraqueza obrigatória e as Recompensas.
6. Preencha a Ficha Simplificada (§5.7). **Pronto**: a criatura já pode entrar em qualquer encontro.

#### Nível 2 — Montar um Encontro

1. Calcule o **PG** do grupo atual (§6.3).
2. Escolha quantas criaturas, de qual Categoria, vão compor o encontro (pode misturar Categorias).
3. Calcule o **PE** (§6.3) com os multiplicadores de Quantidade/Inteligência/Terreno/Objetivo — inclua o multiplicador de **Pressão** (§4.2) se o andar já estiver Agravado+, e o de **Facção** (§10) se for território de uma facção com Reputação relevante.
4. Calcule `R = PE/PG` e confira contra a tabela de classificação (§6.3) — isso te diz se o encontro está Fácil, Equilibrado, Difícil, etc.
5. Se estiver construindo a criatura/grupo do zero para atingir um R específico (em vez de montar com criaturas já prontas), aplique o **FCE** (§6.5) ao multiplicar os atributos/perícias do inimigo.

#### Nível 3 — Construir um Andar

1. Calcule o **Orçamento de Ameaça (OA)** = PG × Dificuldade × Duração (§6.4).
2. Distribua o OA entre Combates/Exploração/Eventos/Pressão/Chefe conforme o tipo de andar (proporções sugeridas em §6.4; ajuste livremente para andares narrativos vs. andares de combate puro).
3. Desenhe as Áreas do andar, cada uma consumindo uma fatia do OA — misture Encontros (Nível 2), testes de Exploração e Eventos.
4. Defina a fonte e o ritmo de **Pressão** (§4.2) ao longo do andar.
5. Defina as Recompensas e pelo menos 1 **Ativo Estratégico** com Valor Estratégico atribuído (§7).
6. Valide o resultado: o andar tem pelo menos 1 encontro Equilibrado+ e uma escolha real (sem opção objetivamente "errada")? Veja o exemplo completo em §17.10 do GDD ("A Vila dos Mil Monstros") como referência de bom resultado.

#### Nível 4 — Estruturar um Arco

1. Defina tema, história, conflito e objetivo final (§3.1).
2. Planeje ao menos 5 andares (Nível 3), seguindo a progressão narrativa sugerida: Introdução → Investigação → Desenvolvimento → Preparação → Clímax → Consequência.
3. A cada 5 andares, insira um **Andar Especial** (§3.3) — garanta que os 5 andares anteriores contêm as ferramentas para vencê-lo.
4. Decida a Pressão temática do arco (que tipo de "ameaça crescente" ele representa) e quais Facções estão em jogo.

#### Nível 5 — Planejar a Campanha

1. Encadeie Arcos (Nível 4), cada um representando um Fragmento Dimensional diferente.
2. Acompanhe os **Quatro Pilares** em paralelo (§7): NP dos personagens, CG da Guilda, Recursos Estratégicos, Ativos Estratégicos acumulados.
3. Use o Registro de Campanha (§11) para manter consistência entre sessões.
4. Deixe a Guilda evoluir no mesmo ritmo dos marcos de 5 andares (§8.1) — isso mantém Guilda e Dungeon avançando juntas, reforçando o Princípio da Instituição Permanente.

---

## 7. Domínio e os Quatro Pilares da Progressão

A campanha progride em quatro frentes simultâneas e independentes:

1. **Poder Individual (NP)** — personagens.
2. **Poder Institucional (CG)** — a Guilda.
3. **Recursos Estratégicos (RE)** — bens consumíveis/econômicos.
4. **Ativos Estratégicos (AE)** — conquistas permanentes obtidas na Dungeon.

**Categorias de AE**: Infraestrutura, Conhecimento, Diplomacia, Artefatos, Controle Territorial. **Valor Estratégico (VE)**: escala 1 (benefício local) a 5 (mudança permanente de grande escala) — use para calibrar risco x recompensa. Nem todos os AEs de um andar podem ser obtidos ao mesmo tempo: force escolhas entre objetivos conflitantes.

> **Princípio Fundamental**: personagens evoluem pelo NP; a Guilda evolui pela CG; a campanha evolui pelos Ativos Estratégicos.

---

## 8. A Guilda — Ferramentas do Mestre

### 8.1 Capacidade da Guilda (CG)
**Desacoplada do cálculo de ameaça** — nunca soma no PG nem no OA (evita contar a força da Guilda duas vezes).
```
CG = Infraestrutura + Pesquisa + Logística + Recursos
```

- Infraestrutura = Σ (nível de cada construção × peso: Fundação=1, Produção=2, Especialização=3, Institucional=5, Monumental=8)
- Pesquisa = pontos acumulados em projetos concluídos
- Logística = Capacidade de Suporte (CS) + nº de trabalhadores qualificados × 2
- Recursos = reservas de Moedas de Pacto + materiais estratégicos convertidos

**Tabela oficial por estágio** (marco a cada 5 andares):

| Estágio | Andares | Infra | Pesquisa | Logística | Recursos | **CG** |
|---|---:|---:|---:|---:|---:|---:|
| Fundação | 0 | 5 | 0 | 5 | 5 | **15** |
| Guilda Menor | 5 | 20 | 10 | 15 | 15 | **60** |
| Guilda Regional | 10 | 45 | 25 | 30 | 30 | **130** |
| Guilda Reconhecida | 15 | 80 | 45 | 50 | 50 | **225** |
| Guilda Maior | 20 | 125 | 70 | 75 | 75 | **345** |
| Guilda Renomada | 25 | 180 | 100 | 105 | 105 | **490** |
| Guilda Lendária | 30 | 245 | 135 | 140 | 140 | **660** |
| Guilda Divina | 35+ | 320 | 175 | 180 | 180 | **855** |

**8.1.1 Capacidades Derivadas — CI, CF, CS**

Diferente da CG (institucional, isolada do combate), estas três travam limites concretos de jogo, cada uma amarrada a instalações específicas (§8.2.1):
```
CS (Capacidade de Suporte) = 5 + (Nível do Centro Logístico × 2) + (Nível do Armazém × 1)
CI (Capacidade Institucional) = 3 + (Nível da Câmara do Conselho × 4) + (Nível do Centro Logístico × 1)
CF (Capacidade de Formação) = 10 + (Nível do Memorial × 3) + (Nível da Biblioteca × 1) + (Nível do Campo de Treinamento × 1)
```

| Estágio | CS | CI | CF |
|---|---:|---:|---:|
| Fundação | 6 | 3 | 11 |
| Guilda Menor | 7 | 3 | 13 |
| Guilda Regional | 10 | 4 | 18 |
| Guilda Reconhecida | 12 | 5 | 23 |
| Guilda Maior | 14 | 10 | 28 |
| Guilda Renomada | 16 | 15 | 33 |
| Guilda Lendária | 16 | 15 | 34 |
| Guilda Divina | 16 | 15 | 34 |

**CS trava**: número máximo de construções ativas/administradas simultaneamente — com 19 instalações construíveis e teto de CS em 16, até uma Guilda Divina precisa escolher o que fica ativo (excedentes ficam Inativas, sem benefício).

**CI trava**: Patronos ativos simultâneos = CI ÷ 3 (arred. p/ cima, mín. 1) · Projetos de Interlúdio simultâneos = CI ÷ 2 · Trabalhadores contratáveis no total = CI × 3.

**CF concede** (bônus de Formação num personagem novo, Manual do Jogador §3):

| CF | Bônus de Formação |
|---|---|
| 10–17 | Nenhum (Recruta padrão) |
| 18–22 | +5 pontos de perícia extra |
| 23–27 | +10 pontos de perícia extra; equipamento inicial pode ser Incomum |
| 28–32 | +15 pontos de perícia extra; equipamento Incomum garantido; +1 Talento menor extra |
| 33+ | +20 pontos de perícia extra; equipamento Raro possível; 1 perícia inicial já nasce em Grau Básico |

### 8.2 Quartel-General e Construções
Construções formam uma árvore tecnológica real. Toda construção tem: Pré-requisitos (estruturais, institucionais, de conhecimento, de recursos, humanos), Custos, Benefícios Diretos, Sinergias.

**8.2.1 Árvore Tecnológica — Lista Completa**

| # | Instalação | Peso | Teto | Pré-requisito | O que desbloqueia |
|---|---|---:|---|---|---|
| **Fundação (Peso 1)** | | | | | |
| 1 | Portão | — | Fixo | Existe desde o início | Núcleo da Dungeon; não se constrói nem melhora |
| 2 | Dormitório | 1 | V | Nenhum | Vagas de personagens/trabalhadores (Nível × 2) |
| 3 | Armazém | 1 | V | Nenhum | Armazenamento (Nível × 50 unidades) |
| 4 | Campo de Treinamento | 1 | V | Nenhum | Treino de combate; Provações de Corpo/Controle |
| **Produção (Peso 2)** | | | | | |
| 5 | Ferraria | 2 | V | Armazém I | Crafting de armas/armaduras (Comum→Raro em I-II, Épico em III+) |
| 6 | Oficina | 2 | V | Armazém I | Crafting geral (Comum/Incomum) |
| 7 | Biblioteca | 2 | VII | Dormitório I | Pesquisa Menor/Moderada; Provações de Intelecto/Percepção |
| 8 | Enfermaria | 2 | V | Dormitório I | Cura avançada, recuperação de PV no Interlúdio; Provação de Vigor |
| **Especialização (Peso 3)** | | | | | |
| 9 | Laboratório Arcano | 3 | V | Biblioteca II | Pesquisa Arcana Maior; Provação de Afinidade; Encantamento |
| 10 | Academia Militar | 3 | V | Campo de Treinamento II + Enfermaria I | Provações de Presença/Vontade; Técnicas Supremas; mercenários avançados |
| 11 | Jardim Alquímico | 3 | IV | Oficina II | Alquimia avançada (Venenos/Transmutação) |
| 12 | Oficina de Runas | 3 | IV | Ferraria II | Crafting Épico+; Encantamento de armas |
| **Institucional (Peso 5)** | | | | | |
| 13 | Memorial | 5 | IV | Biblioteca III | Acesso a Cristais de Memória; aumenta Capacidade de Formação (CF) |
| 14 | Centro Logístico | 5 | IV | Armazém III + Oficina II | Aumenta Capacidade de Suporte (CS); mais Expedições Secundárias |
| 15 | Quartel dos Mercenários | 5 | IV | Academia Militar II | Mercenários de Ranking mais alto; aumenta limite |
| 16 | Torre dos Magos | 5 | IV | Laboratório Arcano III | Pesquisa Suprema; Rituais avançados; Grimórios raros |
| **Monumental (Peso 8)** | | | | | |
| 17 | Câmara do Conselho | 8 | II | Centro Logístico III + Memorial II | Aumenta Capacidade Institucional (CI); mais Patronos/projetos simultâneos |
| 18 | Cofre Divino | 8 | II | Memorial III | Guarda Moedas de Pacto com segurança; habilita Crafting Divino |
| 19 | Observatório Dimensional | 8 | II | Torre dos Magos III | Prevê Rupturas; reduz a Pressão base de andares explorados |
| 20 | Santuário do Patrono | 8 | I–II | Câmara do Conselho I + Cofre Divino I | Fortalece o Pacto Divino; resistência a eventos Divinos negativos |

**8.2.2 Custo de Construção e Melhoria**: reaproveita os pesos já fixados na CG (§8.1). O custo de **melhorar** uma instalação de um Nível para o seguinte usa a mesma fórmula, sempre referente ao Nível-alvo (ex.: subir a Ferraria de II para III custa o valor do Nível III completo).
```
Custo em Recursos = Nível-alvo × Peso da Categoria × 10
Tempo de Construção/Melhoria = Nível-alvo × Peso da Categoria × 3 dias
Trabalhadores mínimos = Peso da Categoria
```

| Categoria (Peso) | Nível I | Nível III (se aplicável) |
|---|---|---|
| Fundação (1) | 10 recursos / 3 dias | — |
| Produção (2) | 20 recursos / 6 dias | 60 recursos / 18 dias |
| Especialização (3) | 30 recursos / 9 dias | 90 recursos / 27 dias |
| Institucional (5) | 50 recursos / 15 dias | 150 recursos / 45 dias |
| Monumental (8) | 80 recursos / 24 dias | raramente passa de Nível I-II |

Monumentais também exigem **Moedas de Pacto = Nível × 2**, além dos recursos comuns.

**Princípio da Maturidade Institucional**: pré-requisitos checam o **nível** da construção-base, não só sua existência. Nem toda construção tem o mesmo teto (Dormitório pode parar em V; Biblioteca pode ir a VII).

**Nível Tecnológico da Guilda (NTG)**: infraestrutura + conhecimento acumulado — referência para desbloqueios de ponta.

Início de campanha: só existem Portão, Dormitório e Campo de Treinamento básico.

### 8.3 Trabalhadores, Mercenários e Departamentos
Trabalhadores (Operários, Artesãos, Pesquisadores, Instrutores, Mercadores, Médicos, Administradores) têm eficiência, salário, moral e especialidade — bons, nunca tão bons quanto os jogadores. Mercenários só atuam em andares já conquistados (nunca substituem os jogadores). Departamentos (Exploração, Militar, Arcano, Logístico) agregam funções para facilitar administração em campanhas grandes.

### 8.4 Economia
Moeda comum (**Prata**) + materiais + Moedas de Pacto (moeda divina). Câmbio-base: **1 Moeda de Pacto = 10 Prata**. Financiamento: Contribuição Livre, Contrato de Guilda, Investimento de Retorno. Recompensas divididas entre Personagem / Guilda / Reserva Estratégica.

**Preços-base**: Ração 1 Prata | Estadia 2 Prata | Salário Operário 3 Prata/dia | Salário Artesão/Pesquisador 8 Prata/dia | Manutenção de construção = Peso da Categoria × 1 Prata/dia.

**Salário de Mercenário por Ranking**: Bronze 10 | Ferro 18 | Aço 30 | Prata 50 | Ouro 80 | Mithril 120 | Adamante 170 | Lendário 250 (Prata/dia).

**Geração de Renda**: Recompensas de Expedição (fatia "Guilda") · Comércio (Doutrina Comercial +10% em vendas) · Trabalhadores (Operário ~2 Prata/dia) · Expedições Secundárias (`NP do mercenário × 0,5 Prata` por sucesso) · Legado (bônus permanentes de renda).

**Manutenção Diária** = Σ(Nível × Peso × 1 Prata, por construção) + Σ(salários ativos). Sem pagar: construções ficam em Negligência (metade do benefício) e Trabalhadores perdem Moral — nunca trava o jogo.

**Inflação — Índice de Preços por Estágio da Guilda** (mesmos estágios da CG, §8.1): Fundação ×1,0 | Guilda Menor ×1,2 | Guilda Regional ×1,5 | Guilda Reconhecida ×1,8 | Guilda Maior ×2,2 | Guilda Renomada ×2,6 | Guilda Lendária ×3,2 | Guilda Divina ×4,0. `Preço Ajustado = Preço-base × Índice`. Dinheiro nunca "resolve o jogo" nos estágios avançados — o custo de operar cresce junto com a ambição da Guilda.

### 8.5 Doutrinas
Árvore de especialização institucional. A Guilda começa com **até 2 Doutrinas ativas**, desbloqueando **+1 por Nível da Câmara do Conselho** (§8.2.1), até **4 simultâneas**. Trocar uma exige projeto de Interlúdio (20 dias, Teste Difícil de Liderança/Administração).

| Doutrina | Bônus |
|---|---|
| Militar | +10% Ataque/Dano de Mercenários/NPCs de combate; -1 dia em Provações de Corpo/Controle/Presença/Vontade |
| Acadêmica | +15% velocidade de Pesquisa; -10% custo de Provações de Intelecto/Percepção |
| Comercial | +10% em vendas de excedente; -1 estágio no Índice de Preços para compras da Guilda |
| Exploração | +15% de sucesso em Expedições Secundárias; -10% de consumo de recursos do grupo principal |
| Arcana | -1 PA extra em conjuração para toda a Guilda; -25% no tempo de Provação de Afinidade |
| Engenharia | -15% no tempo de Construção/Melhoria; +10% de chance de Grande Sucesso em Crafting |
| Logística | +20% na Capacidade de Suporte (CS); -10% na Manutenção Diária |
| Diplomática | Facções novas começam com +15 de Reputação; ganhos Moderados contam como Maiores |

---

## 9. Interlúdio — Conduzindo o Tempo Entre Sessões

**Duas linhas temporais**: Tempo da Dungeon (usado na sessão) e Tempo do Mundo/Quartel (passa em semanas entre sessões, com dilatação fixa — ex.: 10 dias na Dungeon ↔ 1 dia no Quartel). Cada personagem recebe ações de interlúdio proporcionais ao tempo desde sua última expedição; o jogador declara, você resolve.

**Subsistemas**: Treinamento (`(1 + Bônus de Instalação + Bônus de Instrutor) × Multiplicador de Curva de Aprendizado` pontos/dia; enquanto Sem Treinamento — 0-9 pontos — os bônus de Instalação/Instrutor não valem, e há um teto fixo por Correlação: Nenhuma=1/dia, Baixa=2, Média=3, Alta=5; testes com a perícia nessa faixa sofrem -2. Fórmula completa e tabelas no Manual do Jogador §3.6) · Pesquisa (Descobrir→Pesquisar→Dominar→Aplicar) · Produção/Crafting (ver Manual do Jogador §7.3 para o custo em materiais) · Administração da Guilda · Expedições Secundárias (mercenários, sempre limitadas a andares conquistados).

**Custo de Pesquisa**: reaproveita os tiers de Complexidade da Magia (Manual do Jogador §6.1):

| Complexidade | Tempo | Custo em Recursos | Instalação mínima |
|---|---:|---:|---|
| Menor | 5 dias | 10 | Biblioteca/Oficina básica |
| Moderada | 10 dias | 25 | Biblioteca II+ |
| Maior | 20 dias | 50 | Laboratório correspondente |
| Suprema | 40+ dias | 100+ | Instalação avançada + 5 Moedas de Pacto |

Pesquisas coletivas dividem o tempo proporcionalmente, mas nunca abaixo de 50% do tempo-base.

**Regra de Origem dos Modificadores**: toda instalação/instrutor/equipamento que bonifica uma atividade precisa de origem rastreável.

---

## 10. Eventos Dinâmicos, Tensão e Facções

O mundo não para na ausência dos jogadores. Categorias de evento: Pessoais, da Guilda, da Dungeon, Mundiais, Divinos (geração Natural, por Consequência, ou Narrativa).

**Sistema de Tensão** — 4 indicadores acumulam valor e aumentam chance/intensidade de eventos: Tensão da Guilda, da Dungeon, Mundial, Divina. Uma **Ruptura** é o evento máximo de Tensão da Dungeon.

**Facções (FECHADO)**: existem dentro da Dungeon (Goblins, Cultistas, Mortos-vivos, Mercadores, Bestas, Aventureiros rivais), controlam território, reagem às escolhas dos jogadores, fazem alianças/guerra entre si — mas a influência delas fica restrita aos andares (não ao mundo político externo).

**Reputação** (-100 a +100, 5 níveis):

| Reputação | Nível | Comportamento padrão |
|---|---|---|
| -100 a -51 | Hostil | Ataca sempre que encontra; fecha rotas; pode colocar recompensa pelo grupo |
| -50 a -11 | Desconfiada | Preços ruins, sonega informação, exige provas |
| -10 a +10 | Neutra | Sem bônus/penalidade |
| +11 a +50 | Amistosa | Comércio/informação, passagem segura, dicas |
| +51 a +100 | Aliada | Luta ao lado do grupo, compartilha território/recursos, desbloqueia Ativos Estratégicos exclusivos |

**Consequências de escolhas**: Menor ±5 | Moderada ±15 | Maior ±30.

**Efeito prático no andar** (conecta direto ao Sistema de Encontros, §6.3): território de facção Hostil = Terreno Favorável (×1,25) ou Extremo (×1,5) no covil principal; facção Aliada numa área reforça encontros hostis locais (`PG×1,1`); facção Hostil ativa usa Objetivo "Missão crítica" (×2) com mais frequência; Reputação Amistosa+ libera Ativos Estratégicos ocultos; facção Aliada numa área reduz em -5 a Pressão gerada ali (§4.2).

Registre a Reputação de cada facção relevante na Ficha da Guilda (Influência) e no Registro de Campanha (§11).

---

## 11. Registro de Campanha

Funciona como o "save game": andares conquistados, mortes, personagens vivos, recursos, construções, trabalhadores, pesquisas, relações com facções, memórias disponíveis, eventos importantes, doutrinas. Mantenha isso atualizado — é a base para a Ficha da Guilda e para o histórico narrativo.

---

## 12. Apêndice — Fórmulas Consolidadas

```
Modificador de Atributo = Atributo − 2
Bônus de Grau do Atributo (só em Ataque) = Atributo − 1
Bônus de Grau da Perícia = Básico +0 | Adepto +1 | Especialista +2 | Mestre +3 | Lendário +4

NP (personagem) = (Atributos + Perícias) + (Talentos + Habilidades) + Equipamentos
NP (criatura) = (Atributos + Perícias Naturais) + Σ Características + Σ Habilidades + Equipamento

PG (Poder do Grupo) = Σ NP(personagens) × Fator de Sinergia
PE (Poder do Encontro) = Σ NP(criaturas) × Quantidade × Inteligência × Terreno × Objetivo
R (classificação do encontro) = PE / PG
DC (Dificuldade de Combate) = PE / PG
DO (Dificuldade de Objetivo) = calculada separadamente por tempo/ambiente/pressão/informação

OA (Orçamento de Ameaça do andar) = PG × Dificuldade do andar × Fator de Duração
Multiplicador Real de Atributos/Perícias do inimigo = 1 + (R − 1) × FCE

CG (Capacidade da Guilda) = Infraestrutura + Pesquisa + Logística + Recursos
CS (Capacidade de Suporte) = 5 + (Nível do Centro Logístico × 2) + (Nível do Armazém × 1)
CI (Capacidade Institucional) = 3 + (Nível da Câmara do Conselho × 4) + (Nível do Centro Logístico × 1)
CF (Capacidade de Formação) = 10 + (Nível do Memorial × 3) + (Nível da Biblioteca × 1) + (Nível do Campo de Treinamento × 1)
```

### Faixas de NP por Ranking

| Ranking | Faixa de NP | Andares recomendados |
|---|---:|---|
| Bronze | 40–70 | 1–5 |
| Ferro | 70–105 | 6–10 |
| Aço | 105–145 | 11–15 |
| Prata | 145–195 | 16–20 |
| Ouro | 195–260 | 21–25 |
| Mithril | 260–340 | 26–30 |
| Adamante | 340–430 | 31–35 |
| Lendário | 430–550+ | 36+ |

---

## 13. Glossário Completo

**Papéis e Estrutura**

- **Jogador** — a pessoa na mesa. **Patrono** — sua representação institucional (Conselho da Guilda), nunca entra na Dungeon. **Personagem** — o aventureiro descartável que explora.
- **Guilda** — instituição permanente que administra um Portão. **Portão** — estrutura que aprisiona um Fragmento Dimensional; contém uma Dungeon. **Fragmento Dimensional** — resto de um universo destruído. **Ruptura** — colapso dimensional quando a contenção falha.

**Progressão Individual**

- **NP** — Nível de Poder (§6.8 do GDD / Manual do Jogador). Mede a força de um personagem ou criatura.
- **Ranking** — patente do personagem (Bronze → Lendário).
- **Grau** — nível de domínio de um Atributo (I-V) ou Perícia (Básico → Lendário).
- **PA** — Pontos de Ação (3/turno em combate). **PV** — Pontos de Vida.
- **Provação** — projeto de Interlúdio para subir um Atributo.
- **Cristal de Memória** — registro póstumo de um personagem morto, acessível no Memorial.
- **CF** — Capacidade de Formação: potencial inicial de um novo recruta (fórmula em §8.1.1).

**Combate e Encontros**

- **PG** — Poder do Grupo. **PE** — Poder do Encontro. **R** — Razão de Encontro (PE/PG), classifica a dificuldade.
- **DC** — Dificuldade de Combate (mesma fórmula de R). **DO** — Dificuldade de Objetivo (tempo/ambiente/pressão, calculada à parte).
- **OA** — Orçamento de Ameaça: total de "pontos de perigo" disponíveis para montar um andar.
- **FCE** — Fator de Compressão de Encontro: amortece a tradução de R em estatísticas reais de combate.

**A Guilda**

- **CG** — Capacidade da Guilda (institucional, desacoplada do combate). **CI** — Capacidade Institucional (o que a Guilda sustenta; fórmula em §8.1.1). **CS** — Capacidade de Suporte (limite de construções administráveis simultaneamente; fórmula em §8.1.1).
- **NTG** — Nível Tecnológico da Guilda. **Doutrina** — especialização institucional permanente (até 4 simultâneas).
- **Memorial** — instalação que dá acesso aos Cristais de Memória.
- **Moeda de Pacto** — moeda divina premium (1 Moeda de Pacto = 10 Prata).
- **Dívida de Formação** — custo inicial que todo personagem deve à Guilda, quitado automaticamente com recompensas.

**A Dungeon e o Mundo**

- **Andar Especial** — ocorre a cada 5 andares; sempre solucionável com o que já foi explorado antes.
- **Pressão** — contador 0-100 por andar (Estável/Agravado/Crítico/Colapso) que aumenta o PE dos encontros restantes.
- **AE** — Ativo Estratégico: conquista permanente e não-consumível. **RE** — Recurso Estratégico: consumível. **VE** — Valor Estratégico: importância de um Ativo (escala 1-5).
- **Reputação** — relação numérica (-100 a +100) da Guilda com uma facção.

---

## 14. Pendências Conhecidas — **TODAS FECHADAS**

- ~~Custo de evolução de atributos~~ — **FECHADA** (Manual do Jogador §3.5): sistema de Provação de Atributo. Como Mestre, seu papel é garantir a instalação exigida (Nível ≥ Grau atual) e aplicar o Teste Absoluto ao final do tempo — a tabela de Provações temáticas já sugere qual perícia usar em cada atributo.
- ~~Custo final de pesquisas, construções e fabricação~~ — **FECHADA** (§8.2, §9): tabelas de tempo/recursos/Moedas de Pacto para as três (fabricação está detalhada no Manual do Jogador §7.3).
- ~~Economia completa~~ — **FECHADA** (§8.4): Prata/Moeda de Pacto, preços-base, salários de mercenário, geração de renda, manutenção e Índice de Preços por estágio (inflação).
- ~~Cálculo numérico de gatilhos da Pressão da Dungeon~~ — **FECHADA** (§4.2): contador 0-100, limiares e multiplicadores validados no teste de ponta a ponta do Andar 1 do Arco 1 ("A Vila dos Mil Monstros").
- ~~Facções: falta mecânica de reputação numérica e tabela de consequências~~ — **FECHADA** (§10): Reputação -100/+100, 5 níveis, e conexão direta com Terreno/Objetivo/Pressão dos encontros.
- ~~Um "manual do mestre" de construção de conteúdo totalmente consolidado~~ — **FECHADA** (§6.6): Guia de Construção de Conteúdo em 5 níveis (Criatura → Encontro → Andar → Arco → Campanha), amarrando todas as ferramentas já fechadas num fluxo único.
- ~~O FCE foi calibrado por simulação agregada — vale testar e ajustar com personagens reais de mesa~~ — **FECHADA** (§6.5): validado com grupos heterogêneos (Tank/DPS/Balanced, NP ±20%), resultado estável, sem ressalvas adicionais.

**Não há mais pendências conhecidas.** O sistema está completo e validado de ponta a ponta — da criação de personagem à campanha inteira, incluindo balanceamento testado estatisticamente e um caso real construído e revisado (§6.6, Nível 3).
