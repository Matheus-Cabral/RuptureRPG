# GDD MESTRE — RPG Dungeon Crawler Hardcore
### Versão consolidada (a partir de todo o material desenvolvido até agora)

> **Nota:** este documento reúne e organiza tudo o que foi decidido ao longo do desenvolvimento, eliminando repetições e apresentando o **estado final** de cada decisão (quando um conceito foi revisado mais de uma vez, aqui está a versão mais recente). No final há o histórico de fechamento do sistema (§17) — todas as pendências foram resolvidas.

---

## Sumário

0. [Conceito em uma frase](#0-conceito-em-uma-frase)
1. [Pilares do Sistema](#1-pilares-do-sistema) — Princípios de Design (16)
2. [Cosmologia e Lore Estrutural](#2-cosmologia-e-lore-estrutural-a-arquitetura-do-mundo)
3. [Os Três Papéis](#3-os-três-papéis-distinção-fundamental-de-nomenclatura)
4. [Estrutura da Campanha](#4-estrutura-da-campanha) — Arcos, Andares, Andares Especiais
5. [Núcleo de Resolução](#5-núcleo-de-resolução-regras-globais--sistema-universal) — Dados, Testes, Dificuldade, Rankings
6. [Sistema de Personagens](#6-sistema-de-personagens) — Criação completa, Atributos, Perícias, Talentos, Magia/Técnicas, Equipamentos, NP
7. [Combate](#7-combate-fechado)
8. [Exploração](#8-exploração-fechado)
9. [A Dungeon](#9-a-dungeon) — Pressão, Criaturas, Encontros, Orçamento de Ameaça, FCE, Ativos Estratégicos
10. [A Guilda](#10-a-guilda) — Ficha, Quartel-General (árvore tecnológica), Economia, CG
11. [Interlúdio](#11-interlúdio-o-segundo-coração-do-sistema)
12. [Eventos Dinâmicos e Tensão](#12-eventos-dinâmicos-e-tensão)
13. [Facções](#13-facções-fechado)
14. [Registro da Campanha](#14-registro-da-campanha)
15. [Apêndice — Fórmulas Consolidadas](#15-apêndice--fórmulas-consolidadas)
16. [Glossário Rápido](#16-glossário-rápido)
17. [Histórico de Fechamento do Sistema](#17-histórico-de-fechamento-do-sistema)

---

## 0. Conceito em uma frase

> **Um dungeon crawler hardcore onde os jogadores administram, como Conselho, uma Guilda permanente de exploradores a serviço de uma divindade; a Guilda é a verdadeira protagonista da campanha, e os personagens que descem à Dungeon são recursos valiosos, porém descartáveis.**

Esse conceito funciona como filtro: toda regra nova deve ser testada contra ele. Se não fortalece essa identidade, provavelmente não pertence ao sistema.

---

## 1. Pilares do Sistema

- Dungeon crawler hardcore, com alta letalidade.
- Mundo persistente (o tempo passa e o mundo muda mesmo sem sessão).
- Guilda permanente — o verdadeiro "personagem principal" da campanha.
- Personagens descartáveis (a morte é definitiva e faz parte do jogo).
- Progressão baseada em ações realizadas, nunca em XP genérico.
- Exploração recompensadora; informação vale tanto quanto poder.
- Interlúdio estratégico (o período entre sessões é tão importante quanto a expedição).
- **Tempo é o recurso mais importante do jogo.**

### Princípios de Design (lista consolidada)

1. **Princípio da Dominância da Dungeon** — toda forma de progresso obtida fora da Dungeon deve aumentar a eficiência da próxima expedição, mas nunca superar o que se ganharia explorando. `Dungeon >>> Interlúdio >>> Inatividade.`
2. **Princípio da Especialização** — toda evolução é consequência direta da atividade praticada. Não existe XP universal.
3. **Princípio da Origem dos Modificadores** — todo bônus/penalidade precisa de uma fonte identificável (equipamento, talento, instalação, doutrina, estado, magia, evento).
4. **Regra de Ouro** — nenhuma atividade gera progresso ilimitado sem consumir um recurso limitado (tempo, dinheiro, materiais, trabalhadores, espaço, prestígio, conhecimento).
5. **Princípio da Simetria** — as mesmas regras que valem para os jogadores valem para o mundo (NPCs, facções, construções e organizações seguem as mesmas leis fundamentais).
6. **Princípio da Progressão Linear** — toda atividade concede uma quantidade base fixa de progresso; bônus modificam esse valor, mas a base nunca escala com o Ranking.
7. **Princípio dos Fracassos como Consequência** — falhar em um andar não bloqueia a campanha, gera consequências (perda de artefato, facção mais forte, chefe evoluído, recursos reduzidos).
8. **Princípio da Coerência Narrativa** — a narrativa existe para justificar as mecânicas, nunca para substituí-las (nem o contrário).
9. **Princípio da Instituição Permanente / Continuidade** — a Guilda nunca retrocede completamente; o personagem é substituível, a organização não.
10. **Princípio dos Marcos** — a evolução deve ser perceptível em marcos claros, não apenas em incrementos numéricos invisíveis.
11. **Princípio do Limite Natural** — todo atributo/perícia possui um teto natural (Grau V); superá-lo exige Transcendência (ver §6.3).
12. **Princípio da Escala de Conflito / Organização / Comportamento / Informação** (do sistema de criaturas e hordas) — conflitos em massa seguem as mesmas regras fundamentais, apenas em escala diferente; a inteligência e organização de um grupo inimigo alteram sua ameaça tanto quanto seu poder bruto; informação sobre o inimigo é, ela mesma, um recurso.
13. **Princípio da Automatização / Fronteira da Exploração** — NPCs e mercenários nunca substituem os jogadores; eles só atuam em áreas já conquistadas.
14. **Princípio do Mundo Vivo / Indicadores Dinâmicos** — o mundo evolui sozinho durante a ausência dos jogadores, e estados importantes (Pressão, Tensão, Capacidade da Guilda, Nível de Poder) possuem valores mecânicos próprios.
15. **Princípio da Progressão Irreversível** — andares concluídos não são repetidos pelos personagens (ver Expedições Secundárias/Mercenários para exploração posterior).
16. **Princípio do Domínio** — a verdadeira vitória sobre a Dungeon não é apenas sobreviver, é conquistar influência permanente sobre o universo que ela representa (Ativos Estratégicos).

---

## 2. Cosmologia e Lore Estrutural (a "Arquitetura do Mundo")

A lore existe apenas para justificar as mecânicas — nunca para criar exceções a elas.

- No passado, diversas divindades criaram universos independentes. Muitos foram destruídos por guerras, cataclismos ou pelo fim natural de seu ciclo.
- Um universo destruído nunca desaparece por completo: ele deixa um **Fragmento Dimensional**, que tende a colidir com outras realidades.
- Para conter isso, as divindades construíram um **Mundo Central** com **Portões** — estruturas que aprisionam cada Fragmento. Cada Portão contém uma **Dungeon**.
- Cada andar de uma Dungeon é um pedaço preservado de um universo morto — por isso andares podem ter biomas, tecnologias, criaturas e leis físicas completamente diferentes entre si.
- **Estabilidade Dimensional**: os fragmentos acumulam pressão constante para retornar ao mundo real. Explorar a Dungeon reduz essa pressão. Se a estabilidade se perde, ocorre uma **Ruptura** — parte da Dungeon invade o Mundo Central, criaturas escapam, regiões são corrompidas ou substituídas.
- Cada divindade é responsável por certos Portões e compete por influência através da eficiência das Guildas que administram seus Portões (substituindo guerras diretas entre deuses).
- **Guildas**: instituições permanentes responsáveis por manter a estabilidade de um Portão — organizam expedições, preservam conhecimento, desenvolvem infraestrutura, formam aventureiros.
- **Patronos**: cada jogador, no papel administrativo, é um Patrono. Fez um pacto direto com uma divindade que lhe concede autoridade sobre a Guilda, em troca de responsabilidade permanente pela estabilidade do Portão.
- **Pacto Divino**: o Patrono jamais pode atravessar o Portão (é uma "Âncora" — sua presença estável fora da Dungeon é o que mantém o Portão contido); deve manter a Guilda ativa e as expedições contínuas; deve preservar o conhecimento acumulado.
- Se um Patrono atravessa o Portão, o pacto se rompe. Se morre sem sucessor legítimo, a Guilda perde autoridade sobre o Portão, a estabilidade colapsa e ocorre uma Ruptura.

**Hierarquia da campanha:**
```
Jogador → Patrono → Guilda → Portão → Dungeon → Personagens
```

Essa fundação narrativa explica organicamente quase todo o sistema mecânico: Registro da Guilda (exigência divina de controle), Rankings (certificação de quem pode conter instabilidades maiores), Interlúdio (preparo contínuo da contenção), Construções (capacidade operacional), Doutrinas (filosofias ensinadas pela divindade), Cristais de Memória (conhecimento que não pode depender de um único indivíduo), Metaprogressão (capacidade da Guilda de cumprir seu dever cósmico).

---

## 3. Os Três Papéis (distinção fundamental de nomenclatura)

- **Jogador** — a pessoa sentada à mesa.
- **Patrono** — a representação permanente do jogador dentro do Conselho da Guilda; administra a Guilda durante o interlúdio; nunca entra na Dungeon.
- **Personagem** — o aventureiro recrutado pelo Patrono para explorar a Dungeon; descartável do ponto de vista institucional.

O jogador nunca "é" o personagem — ele é um Patrono que envia sucessivos personagens para cumprir o pacto divino.

---

## 4. Estrutura da Campanha

### 4.1 Arcos
Cada **Arco** representa um universo que encerrou seu ciclo de existência (um Fragmento Dimensional inteiro). Um arco possui: tema, história, conflito, objetivo final, pressão específica, ecossistema próprio, recursos próprios, mecânica exclusiva e ao menos cinco andares.

Estrutura narrativa sugerida para os andares dentro de um arco: Introdução → Investigação → Desenvolvimento → Preparação → Clímax → Consequência.

### 4.2 Andares
Cada andar é uma etapa de exploração dentro de um arco, com tema e objetivo fixos. Tipos de objetivo previstos: Exploração, Reconhecimento, Defesa, Ataque, Caça, Escolta, Sobrevivência, Puzzle, Eliminação, objetivos secretos.

Classificação complementar dos andares:

- **Andares Transitórios** — de passagem, menor relevância estratégica.
- **Andares Estratégicos** — concedem Ativos Estratégicos importantes.
- **Andares Narrativos** — avançam a história do arco.
- **Andares de Marco** — pontos de virada da campanha.

### 4.3 Andares Especiais
A cada cinco andares existe um **Andar Especial**, com dificuldade muito elevada. Regra fixa: os cinco andares anteriores sempre contêm as ferramentas necessárias para vencê-lo (informações, atalhos, itens, aliados, equipamentos, conhecimento). Quem explora pouco ainda consegue chegar ao chefe; quem explora muito consegue sobreviver a ele.

### 4.4 Progressão Irreversível
Andares concluídos não podem ser repetidos pelos personagens jogadores (mercenários e expedições secundárias podem operar em andares já conquistados — ver §9).

---

## 5. Núcleo de Resolução (Regras Globais / Sistema Universal)

### 5.1 Dados
**Decisão final: 2d10** (não d20). Justificativa: gera curva normal (resultados médios mais frequentes, extremos raros), reduz a influência do acaso ao longo de centenas de sessões, e escala melhor que um d20 puro conforme os bônus crescem.

### 5.2 Tipos de teste

- **Testes Opostos**: quando há oposição direta (combate, furtividade x percepção, agarrar, intimidação, corrida). Vence quem tira o maior resultado.
- **Testes Absolutos**: contra uma dificuldade fixa (percepção, tradução, pesquisa, fabricação, escalada, medicina, sobrevivência). Sucesso quando resultado ≥ dificuldade.

### 5.3 Dificuldade
`Dificuldade = Categoria da tarefa + Escala do desafio (esperado para o Ranking)`. Valores preliminares:

| Dificuldade | Valor |
|---|---:|
| Trivial | 8 |
| Fácil | 12 |
| Moderada | 16 |
| Difícil | 20 |
| Muito Difícil | 24 |
| Heroica | 28 |
| Lendária | 32 |

### 5.4 Graus de resultado

| Resultado | Efeito |
|---|---|
| Muito abaixo | Falha crítica |
| Abaixo | Falha |
| Igual ou superior | Sucesso |
| Muito superior | Grande sucesso |
| Extremamente superior | Sucesso extraordinário |

A **Margem de Sucesso** (diferença entre resultado e dificuldade) é usada ativamente para determinar a qualidade do efeito, não apenas sucesso/falha.

### 5.5 Críticos
Ocorrem por resultado natural máximo/mínimo ou por diferença extrema no teste. Críticos positivos geram feitos excepcionais; críticos negativos geram consequências graves.

### 5.6 Hierarquia de Regras (para resolver conflitos)
```

1. Regras Globais
2. Dungeon
3. Andar
4. Evento
5. Personagem
6. Equipamentos
7. Efeitos Temporários
```

### 5.7 Nível de Poder (NP)
Valor calculado automaticamente para balanceamento, desbloqueios, recomendações de conteúdo e cálculo de dificuldade. O jogador pode consultá-lo, mas **nunca** o usa diretamente na mesa.

### 5.8 Rankings
Patente do personagem (ex.: Bronze → Ferro → Aço → Prata → Ouro → Mithril → Adamante → Lendário). Cada Ranking define: limite de atributos, limite de perícias, equipamentos permitidos, tecnologias acessíveis, instalações utilizáveis, conteúdo recomendado. Avança por **conquistas** (ex.: alcançar determinado andar), nunca por acúmulo simples de XP.

### 5.9 Interlúdio (definição oficial)
> **Interlúdio é o período compreendido entre duas expedições consecutivas de um personagem, durante o qual ele realiza atividades no Quartel-General.** Toda atividade consome tempo, possui requisitos e produz progresso específico.

---

## 6. Sistema de Personagens

### 6.1 Fluxo de Criação (oficial — versão final)
```

1. Origem            (§6.1.2)  → +25 pts perícia (15+10), benefício, equipamento, gancho
2. Histórico          (§6.1.4)  → benefício + complicação (sem perícia/atributo)
3. Linhagem           (§6.1.7)  → ajuste de teto em 2 atributos + 1 traço racial
4. Aptidões (2)       (§6.1.5)  → facilidade de aprendizado + instinto natural
5. Atributos          (§6.3)    → 20 pts, compra livre, mín 1 / máx 5 (ou 6/4 se ajustado pela Linhagem)
6. Perícias Iniciais             → as da Origem já entram; distribuir eventuais pontos extras
7. Talento Inicial (1)(§6.1.6)
8. Equipamentos                  → os da Origem + o que a Guilda fornecer
9. Nível de Poder (§6.8)         → deve cair na faixa Bronze (40–70)
10. Registro da Guilda            → nome, nº de registro, Ranking (Bronze), Dívida de Formação, data de ingresso
```

- **Origem**: passado social/profissional (Soldado, Caçador, Artesão, Camponês, etc.). Concede 1 benefício mecânico, perícias e/ou equipamentos iniciais e uma justificativa narrativa. Regra: origem cria personagens *diferentes*, nunca *superiores*.
- **Histórico**: evento marcante que moldou o personagem (evento + consequência + benefício + possível complicação). Ver manual completo e lista fechada em §6.1.3/§6.1.4.
- **Linhagem**: espécie/ascendência do personagem. Ver manual completo e lista fechada em §6.1.7.
- **Aptidões Iniciais**: inclinações que reduzem dificuldades e melhoram o aprendizado inicial — nunca bloqueiam caminhos futuros. Ver manual completo e lista fechada em §6.1.5.
- **Registro da Guilda**: todo personagem recebe registro oficial (nome, número de registro, Ranking, data de ingresso, Nível de Poder, estado — ativo/ferido/ausente/aposentado/desaparecido/morto —, expedições realizadas, andares conquistados, especializações).
- Todos começam como **Recrutas** da Guilda (ponto zero comum), com a **Dívida de Formação** já fechada em §6.2.

### 6.1.1 Manual de Criação de Origens

Toda Origem — oficial ou criada pelo Mestre/jogador — precisa ter exatamente estes 4 componentes:

1. **Benefício Mecânico Principal** — um único efeito passivo *leve*. Nunca bônus direto em dano, PA ou atributo. Tipos permitidos: redução de dificuldade em uma categoria específica de teste; acesso a algo exclusivo (contato, perícia rara, local); um recurso pontual reutilizável (ex.: 1x por expedição).
2. **Perícias Iniciais (regra fixa)** — sempre **1 perícia primária (15 pontos) + 1 perícia secundária (10 pontos)** = **25 pontos totais em todas as Origens**, sem exceção. Isso garante que nenhuma Origem seja objetivamente "melhor" em quantidade — apenas em direção.
3. **Equipamento Inicial** — 0 a 2 itens simples, nunca acima de raridade Incomum.
4. **Gancho Narrativo** — 1-2 frases que dão ao Mestre um fio para puxar depois.

**Checklist de balanceamento** (validação obrigatória de qualquer Origem nova):

- **Regra do Não-Superior**: a Origem deve deixar o personagem *diferente*, nunca objetivamente melhor de forma geral.
- **Regra do Custo Equivalente**: sempre exatamente 15+10 pontos de perícia.
- **Regra da Contrapartida** (recomendada): Origens muito vantajosas em um nicho específico ganham uma pequena fragilidade narrativa/mecânica correspondente.

**Passo a passo**: (1) definir o conceito social de origem; (2) escolher o Benefício Mecânico Principal; (3) escolher as 2 Perícias Iniciais (15+10); (4) definir 0-2 Equipamentos Iniciais; (5) escrever o Gancho Narrativo; (6) validar contra a checklist.

### 6.1.2 Lista Oficial de 20 Origens (FECHADA)

| # | Origem | Benefício Mecânico Principal | Perícia Primária (15) | Perícia Secundária (10) | Equipamento Inicial | Gancho Narrativo |
|---|---|---|---|---|---|---|
| 1 | Soldado | -1 dificuldade em testes de Disciplina/formação em combate organizado | Espadas | Armaduras | Espada curta, armadura leve | Desertou ou foi dispensado de uma força militar local |
| 2 | Caçador | -1 dificuldade em Rastreamento na natureza | Rastreamento | Arcos | Arco simples, capa | Vive das terras selvagens há anos |
| 3 | Artesão | Pode identificar qualidade de materiais sem teste | Ferraria | Avaliação | Ferramentas de artesão | Aprendeu um ofício com um mestre exigente |
| 4 | Camponês | +1 recuperação extra em descanso longo | Sobrevivência | Conhecimento de Animais | Foice, roupas simples | Cresceu trabalhando a terra |
| 5 | Estudioso | 1x por interlúdio, resolve uma dúvida factual sem gastar tempo de pesquisa | História (ou Teoria Arcana) | Linguagens | Livro pessoal | Passou a juventude entre pergaminhos |
| 6 | Comerciante | Preços com o comerciante viajante 10% melhores | Comércio | Avaliação | Bolsa de moedas extra | Cresceu entre balcões e negociações |
| 7 | Nobre Decaído | Possui 1 contato de influência acionável (uso limitado) | Liderança | Diplomacia | Anel de família (sem valor comercial) | Perdeu título ou herança |
| 8 | Criminoso | -1 dificuldade em Furtividade em ambiente urbano | Furtividade | Manipulação | Ferramentas de arrombamento | Tem um passado que a Guilda desconhece |
| 9 | Sacerdote | 1x por expedição, realiza uma pequena bênção ritual (efeito menor) | Religião | Rituais | Símbolo sagrado | Serviu um templo antes de ingressar na Guilda |
| 10 | Marinheiro | -1 dificuldade em Equilíbrio/terreno instável | Natação | Armas de Arremesso | Corda, faca | Passou anos em embarcações |
| 11 | Nômade | Nunca fica "perdido" narrativamente (sempre sabe a direção geral) | Navegação | Sobrevivência | Cantil resistente | Nunca teve um lar fixo |
| 12 | Mineiro | -1 dificuldade em identificar instabilidades em cavernas e túneis | Construção | Percepção | Picareta | Trabalhou em minas antes de se tornar aventureiro |
| 13 | Curandeiro | 1x por expedição, estabiliza um ferido grave sem instalação | Medicina | Poções | Kit médico básico | Cuidou de doentes numa vila ou tropa |
| 14 | Menestrel | -1 dificuldade em testes sociais para obter informação de estranhos | Diplomacia | Manipulação | Instrumento simples | Viajou de vila em vila contando histórias |
| 15 | Órfão de Rua | -1 dificuldade em Percepção para notar armadilhas/emboscadas em ambientes fechados | Percepção | Furtividade | Faca pequena escondida | Sobreviveu sozinho nas ruas |
| 16 | Exilado | Conhece 1 idioma/símbolo raro exclusivo do grupo | Linguagens | Rastreamento | Nenhum (perdeu tudo) | Foi expulso de sua terra natal por um motivo que só ele sabe |
| 17 | Ex-Cultista | Reconhece automaticamente símbolos/rituais de cultos, sem teste | Rituais | Religião | Adaga cerimonial | Abandonou um culto antes que fosse tarde demais |
| 18 | Pupilo da Guilda | Recebe 5 pontos extras de perícia para investir em Dungeonologia | Dungeonologia | Estratégia | Mapa desatualizado da Guilda | Cresceu dentro da própria Guilda, filho de um veterano |
| 19 | Caçador de Recompensas | -1 dificuldade em Rastreamento de um alvo específico definido | Rastreamento | Intimidação | Grilhões, arco leve | Vivia de capturar fugitivos e criaturas fugidas |
| 20 | Estudante Arcano | -1 dificuldade no primeiro teste de qualquer nova magia aprendida | Controle Mágico | Teoria Arcana | Grimório incompleto | Estudou magia formalmente, mas nunca se formou |

### 6.1.3 Manual de Criação de Históricos

**Diferença fundamental entre Histórico e Origem**: Origem representa a vida comum e concede pontos de perícia (15+10). Histórico representa um **evento pontual** que mudou o personagem e **nunca concede pontos de perícia ou atributo** — evitando sobreposição de papéis entre as duas camadas. O Histórico concede efeitos situacionais, contatos, conhecimento pontual ou fragilidades.

Estrutura obrigatória de todo Histórico:

1. **Evento Marcante** — o que aconteceu no passado, em poucas frases.
2. **Consequência** — como isso mudou o cotidiano do personagem.
3. **Benefício Mecânico** — efeito leve, dos mesmos tipos permitidos em Origem (redução de dificuldade em nicho específico; acesso exclusivo a algo; recurso pontual reutilizável).
4. **Complicação (obrigatória, diferente da Origem)** — uma fragilidade narrativa e/ou mecânica de peso equivalente ao benefício.

Regras de balanceamento:

- **Regra do Equilíbrio** — Benefício e Complicação precisam ter peso equivalente.
- **Regra da Não-Duplicação** — Histórico jamais concede pontos de perícia/atributo (isso é papel exclusivo de Origem, Aptidões e evolução natural).
- **Regra do Gancho Vivo** — toda Complicação precisa ser algo que o Mestre possa trazer de volta durante a campanha; se não serve como gancho narrativo futuro, não é uma Complicação válida.

**Passo a passo**: (1) definir o evento marcante; (2) definir a consequência no cotidiano; (3) escolher o Benefício Mecânico leve; (4) criar a Complicação de peso equivalente; (5) validar contra a Regra do Equilíbrio e a Regra da Não-Duplicação.

### 6.1.4 Lista Oficial de 20 Históricos (FECHADA)

| # | Histórico | Evento Marcante | Benefício | Complicação |
|---|---|---|---|---|
| 1 | Sobrevivente de Ruína | Explorou uma construção antiga e escapou | -1 dificuldade para identificar riscos estruturais/desabamentos | Algo daquela ruína ainda o procura |
| 2 | Sobreviveu a uma Emboscada | Seu grupo anterior foi dizimado | 1x por expedição, ignora a condição de Surpreendido | Sofre reações intensas a situações que lembrem a emboscada |
| 3 | Foi Preso | Passou tempo confinado, injustamente ou não | Vantagem para escapar de contenções físicas (cordas, algemas) | Possui um registro criminal reconhecível por autoridades |
| 4 | Serviu no Exército | Sua unidade foi dizimada em combate | Resistência maior ao medo em combate organizado | Um superior sobrevivente o culpa pela derrota |
| 5 | Estudou com um Mestre | Teve um mentor renomado que sumiu | Pode invocar o nome do mestre para abrir portas em um círculo específico | O desaparecimento do mestre esconde algo perigoso |
| 6 | Viveu nas Ruas | Período de miséria extrema | Aguenta mais tempo sem comida antes de sofrer penalidades | Deve favores a uma rede do submundo |
| 7 | Herdou uma Ferramenta | Recebeu um objeto de família com história | O item herdado carrega uma pequena propriedade extra | Alguém mais também quer aquele objeto de volta |
| 8 | Descobriu um Manuscrito | Achou um documento que não deveria ter achado | Conhece um fragmento raro de informação (nome, símbolo, local) | Outros sabem que ele tem o manuscrito e o procuram |
| 9 | Traído por um Aliado | Foi traído por alguém de confiança | -1 dificuldade para perceber traição/mentira de aliados próximos | Penalidade em testes sociais para formar vínculos rápidos |
| 10 | Salvou uma Vila | Feito heróico publicamente reconhecido | Reputação positiva e acesso a favores menores na região | A vila cobra ajuda contínua; recusar custa reputação |
| 11 | Perdeu Alguém na Dungeon | Um familiar desapareceu ou morreu em uma expedição | -1 dificuldade em testes ligados a rastrear aquele tipo de perigo específico | Obsessão que pode levá-lo a riscos desnecessários |
| 12 | Fez um Pacto Menor | Selou um pequeno acordo com uma entidade | Pequeno benefício sobrenatural (definido com o Mestre) | A entidade cobrará algo em troca, em algum momento |
| 13 | Sobreviveu a uma Doença Grave | Quase morreu de uma praga | Resistência aumentada contra doenças e venenos | Carrega uma sequela física leve e permanente |
| 14 | Acusado Injustamente | Teve a reputação manchada por um crime que não cometeu | Bônus em Diplomacia quando precisa se defender de acusações | Ainda é malvisto ou procurado em determinado lugar |
| 15 | Guardião de um Segredo | Sabe de algo perigoso que não devia saber | Possui informação valiosa, negociável | Outros sabem que ele sabe — e isso o torna um alvo |
| 16 | Marcado por um Ritual | Passou por um ritual incompleto | Sensibilidade leve a presenças mágicas próximas | A marca do ritual é perceptível ou reage mal a certos estímulos |
| 17 | Resgatado por Estranhos | Deve a vida a alguém que nunca identificou | Possui um contato misterioso que pode ajudar 1x | Não sabe quem foi — a dívida pode ser cobrada a qualquer momento |
| 18 | Perdeu Tudo em um Desastre | Um incêndio ou colapso destruiu sua vida anterior | Bônus de Vontade contra desespero e perda | Não possui posses, contatos ou apoio financeiro antigos |
| 19 | Testemunhou uma Ruptura | Viu de perto o fenômeno mais temido do mundo | Resistência a pânico diante de fenômenos dimensionais | Hipervigilância: penalidade em ambientes que lembram o evento |
| 20 | Criado pela Guilda | Cresceu dentro da própria instituição | Bônus em testes administrativos/burocráticos internos da Guilda | Nunca teve vida "normal": penalidade leve em situações sociais fora da Guilda |

### 6.1.5 Aptidões Iniciais (FECHADA)

Diferente de Origem e Histórico (efeitos narrativos pontuais), Aptidão é puramente estrutural: facilita o aprendizado dentro de um domínio inteiro de perícias.

**Efeito mecânico de cada Aptidão escolhida**, dentro do seu domínio:

1. **Facilidade de Aprendizado** — perícias do domínio sobem 1 categoria de correlação na Curva de Aprendizado (Baixa→Média, Média→Alta) ao serem aprendidas do zero, reduzindo a resistência inicial de §6.4.
2. **Instinto Natural** — **-1 grau de dificuldade** em testes Absolutos com perícias do domínio enquanto ainda estiverem em "Sem treinamento" (0 pontos).

Aptidão nunca bloqueia nada: um personagem sem Aptidão em Magia ainda pode virar mago, só terá uma trajetória inicial mais difícil (sem os dois bônus acima).

**Regra de escolha**: todo personagem escolhe exatamente **2 Aptidões** na criação, entre as 6 abaixo.

| Aptidão | Áreas de Perícia cobertas |
|---|---|
| Combate | Combate — Armas, Combate — Defesa, Combate Corporal, Combate à Distância |
| Exploração | Exploração |
| Conhecimento | Conhecimento, Cura |
| Ofício | Artesanato, Alquimia |
| Magia | Magia |
| Liderança | Social |

Juntas, as 6 Aptidões cobrem exatamente as 11 Áreas de Perícia fechadas em §6.4 — nenhuma perícia fica sem domínio.

**Manual de Aptidões Homebrew**: (1) o novo domínio deve ser um subconjunto claro de uma ou mais Áreas de Perícia existentes, nunca uma área nova inventada só para a Aptidão; (2) os dois efeitos (Facilidade de Aprendizado + Instinto Natural) são fixos, não podem ser trocados por outro efeito; (3) se o domínio homebrew for mais estreito que os oficiais, ainda conta como 1 das 2 escolhas do personagem — nunca ganha bônus extra por ser menor.

### 6.1.6 Talento Inicial (FECHADA)

Talentos "de verdade" (§6.5) são raros e exigem requisitos de Ranking/atributo/perícia — não fazem sentido como menu livre num Recruta que ainda não tem nada disso. Por isso existe uma sublista própria de **Talentos Iniciais (Ranking 0)**: mais simples e genéricos, pensados só para o momento de criação.

**Regra de escolha**: o jogador escolhe **1 Talento Inicial**, sem pré-requisito de Ranking/atributo/perícia. Um Talento Inicial nunca é tão forte quanto um Talento conquistado em jogo — equivale sempre a "Talento menor" na escala de NP (§6.8, valor 1).

**Lista Oficial de 20 Talentos Iniciais:**

| # | Talento | Categoria | Efeito |
|---|---|---|---|
| 1 | Golpe Certeiro | Combate | 1x por combate, repete um dado de ataque que considere ruim |
| 2 | Reflexos de Combate | Combate | +1 na primeira Esquiva de cada combate |
| 3 | Fúria Contida | Combate | 1x por combate, ignora a primeira penalidade de ferimento leve |
| 4 | Faro para o Perigo | Exploração | -1 dificuldade no primeiro teste de Percepção de cada andar |
| 5 | Pé Leve | Exploração | Não sofre penalidade de terreno difícil ao se mover sozinho |
| 6 | Instinto de Sobrevivência | Exploração | 1x por expedição, evita ficar sem uma ração/tocha por um dia |
| 7 | Mãos Habilidosas | Produção | Reduz em 1 dia o tempo do primeiro projeto de fabricação de cada interlúdio |
| 8 | Olho Clínico | Produção | Identifica automaticamente a Qualidade de um item ao examiná-lo |
| 9 | Precisão Artesanal | Produção | 1x por interlúdio, trata um resultado "Sucesso" de fabricação como "Grande Sucesso" |
| 10 | Reciclador | Produção | Recupera metade dos materiais ao falhar em uma fabricação |
| 11 | Vislumbre Arcano | Arcanos | Sente a presença de magia ativa num raio curto, sem gastar ação |
| 12 | Fôlego Ritual | Arcanos | +1 PA disponível especificamente para conjurar magia, 1x por expedição |
| 13 | Toque Elemental | Arcanos | Gera um efeito elemental cosmético/mínimo (luz, calor leve, brisa) sem gastar PA |
| 14 | Memória Arcana | Arcanos | 1x por pesquisa, reduz o tempo necessário em 1 dia |
| 15 | Presença Firme | Social | +1 em testes de Intimidação/Liderança quando em desvantagem numérica |
| 16 | Voz Confiável | Social | 1x por interlúdio, obtém uma informação de um NPC sem precisar de teste |
| 17 | Diplomata Nato | Social | -1 dificuldade no primeiro teste de Diplomacia com uma facção desconhecida |
| 18 | Sorte de Recruta | Extraordinário | 1x por expedição, transforma uma Falha (não crítica) em Sucesso simples |
| 19 | Marca Estranha | Extraordinário | Traço sobrenatural pequeno e inexplicado (definido com o Mestre) — narrativamente rico, mecanicamente neutro até ser investigado em jogo |
| 20 | Sina Protegida | Extraordinário | 1x na campanha inteira, sobrevive a um golpe que o mataria, ficando Incapacitado em vez de morto (efeito consumido após o uso) |

**Manual de Criação de Talentos Iniciais Homebrew**: (1) deve ter efeito único e pontual (1x por combate/expedição/interlúdio) ou um bônus fixo pequeno (+1) — nunca contínuo forte; (2) nunca concede PA extra permanente, aumento de atributo, ou substitui um teste inteiro sem gasto de recurso; (3) deve se encaixar em uma das 6 categorias de Talento já existentes (§6.5); (4) peso equivalente a "Talento menor" (NP = 1).

### 6.1.7 Linhagens (Raças/Espécies) — FECHADA

Estrutura de toda **Linhagem**: (1) **Ajuste Racial** — desloca o **teto** de dois atributos (nunca os 20 pontos gastos na criação): +1 no máximo permitido de um atributo (de 5 para 6) e −1 no máximo de outro (de 5 para 4); nunca concede perícia. (2) **1 Traço Racial** — efeito inato, peso equivalente a Talento menor (NP=1). (3) Dados narrativos (porte, expectativa de vida) — sabor puro, sem efeito mecânico.

**Lista Oficial de 10 Linhagens:**

| Linhagem | Ajuste Racial | Traço Racial |
|---|---|---|
| Humano | Nenhum (todos os atributos no teto padrão 5) | Adaptável: pode trocar 1 Aptidão escolhida na criação, 1x na campanha |
| Anão | +1 máx. Vigor / −1 máx. Controle | Resistência a venenos e doenças |
| Elfo | +1 máx. Percepção / −1 máx. Corpo | Visão em baixa luminosidade |
| Meio-Orc | +1 máx. Corpo / −1 máx. Intelecto | 1x por expedição, ignora uma penalidade de ferimento leve |
| Halfling | +1 máx. Controle / −1 máx. Presença | -1 dificuldade em testes de Furtividade |
| Gnomo | +1 máx. Intelecto / −1 máx. Vigor | -1 dificuldade no primeiro teste de qualquer perícia de Artesanato aprendida |
| Meio-Elfo | Jogador escolhe livremente qual atributo recebe +1 e qual recebe −1 | Aptidão extra pode ser trocada 1x (versatilidade) |
| Draconato | +1 máx. Presença / −1 máx. Controle | Resistência a um tipo elemental (escolhido na criação) |
| Descendente Sombrio | +1 máx. Vontade / −1 máx. Presença | Resistência a medo sobrenatural |
| Fragmentado *(rara, exige aprovação do Mestre)* | +1 máx. Afinidade / −1 máx. Vigor | Sente a proximidade de Rupturas e instabilidade dimensional — liga-se diretamente à cosmologia (§2) |

**Manual de Linhagens Homebrew**: (1) ajuste líquido sempre +1/−1 num par de atributos (ou 0, como Humano); (2) exatamente 1 Traço Racial, peso = Talento menor (NP=1); (3) nunca concede perícia; (4) porte/expectativa de vida são só sabor.

### 6.2 Contrato de Exploração (relação Guilda–Personagem)
A Guilda fornece estrutura (equipamento básico, treinamento, alojamento); o aventureiro, em troca, cumpre expedições e devolve parte dos ganhos. Existe **Patrimônio da Guilda** (institucional) separado do **Patrimônio Pessoal** do personagem. Aposentadoria é uma saída possível para personagens (diferente de morte).

**Dívida de Formação (FECHADA)**: todo personagem novo entra com uma dívida fixa, equivalente ao custo de equipamento básico + treinamento + alojamento fornecidos pela Guilda. Essa dívida é abatida automaticamente da fatia "Personagem" na Distribuição de Recompensas (§10.6) de cada expedição, até ser quitada — nunca trava evolução, apenas reduz temporariamente o ganho pessoal em Moedas de Pacto/recursos. Fecha automaticamente após paga, sem rastreamento manual complexo.

### 6.3 Atributos
Filosofia: **Atributos = capacidade ("é capaz?"). Perícias = experiência ("sabe fazer?").** Atributos nunca concedem perícia automaticamente, apenas modificam eficiência. Não existe "atributo principal" universal — todos devem servir a algo.

Oito atributos, quatro físicos e quatro mentais:

**Físicos**

- **Corpo** — força, potência, capacidade de carga, impacto físico.
- **Controle** — coordenação, precisão, reflexos, equilíbrio.
- **Vigor** — resistência, recuperação, fôlego, tolerância ao esforço.
- **Presença** — postura, imponência, coragem, domínio do espaço.

**Mentais**

- **Intelecto** — lógica, aprendizagem, memória, raciocínio analítico.
- **Percepção** — observação, atenção, leitura de ambiente.
- **Vontade** — disciplina, autocontrole, resistência mental.
- **Afinidade** — conexão com o sobrenatural, compreensão de magia, sensibilidade a artefatos e fenômenos dimensionais (não é "mana").

Regras fundamentais:

1. Atributos nunca representam treinamento (isso é perícia).
2. Atributos modificam eficiência, nunca substituem perícia.
3. Nenhum atributo concede conhecimento automaticamente.
4. Toda perícia se relaciona principalmente a um atributo, mas essa relação pode variar por contexto (ex.: Espadas normalmente usa Controle, mas pode usar Corpo para um golpe bruto).

**Evolução dos atributos é rara** — só evoluem por mudança física/mental real (meses de condicionamento, pesquisa profunda, provações extremas), nunca por uso contínuo em combate. Isso os torna um dos principais mecanismos de controle da escalada de poder em campanhas longas.

**Custo de Evolução — Provação de Atributo (FECHADO)**: diferente do Treinamento de Perícia (progresso diário garantido), subir um Atributo exige uma **Provação** — um projeto de Interlúdio dedicado e temático, ligado ao atributo específico. Apenas **1 Provação ativa por vez** por personagem (Princípio da Especialização Imperfeita).

```
Tempo da Provação = Grau atual × 10 dias
Custo em Recursos = Grau atual × 5 (Moedas de Pacto ou materiais de valor equivalente)
```

| De Grau → Para Grau | Tempo | Custo |
|---|---:|---:|
| I → II | 10 dias | 5 |
| II → III | 20 dias | 10 |
| III → IV | 30 dias | 15 |
| IV → V | 40 dias | 20 |

Exige instalação mínima correspondente ao atributo, com Nível ≥ Grau atual. Ao final do tempo, Teste Absoluto (perícia temática) vs Dificuldade **Difícil + (Grau atual × 2)**. Sucesso avança o Grau; Falha consome o tempo e metade dos recursos, mas não bloqueia — pode tentar de novo (Princípio dos Fracassos como Consequência).

**Provações temáticas por Atributo**:

| Atributo | Provação | Perícia do Teste | Instalação mínima |
|---|---|---|---|
| Corpo | Resistência Extrema (trabalho físico brutal sustentado) | Corpo (bruto) | Campo de Treinamento |
| Controle | Precisão Absoluta (treino extenuante de coordenação) | Perícia de arma/estilo principal | Campo de Treinamento |
| Vigor | Provação de Fôlego (exaustão supervisionada, jejum controlado) | Sobrevivência | Enfermaria |
| Presença | Provação de Domínio (enfrentar medo real, comandar sob pressão) | Liderança/Intimidação | Academia Militar |
| Intelecto | Provação Intelectual (resolver um problema teórico real) | Teoria Arcana/História | Biblioteca |
| Percepção | Provação Sensorial (meditação extrema, treino perceptivo) | Percepção | Biblioteca/Campo de Treinamento |
| Vontade | Provação de Disciplina (jejum, provação psicológica) | Vontade (própria) | Academia Militar |
| Afinidade | Provação Arcana (contato controlado com o sobrenatural) | Controle Mágico/Rituais | Laboratório Arcano |

Além do Grau V, o processo normal de Provação nunca ultrapassa o teto natural — exige Transcendência (bênçãos, rituais, eventos divinos). Linhagens que ajustam o teto máximo usam a mesma fórmula, só com o novo teto como limite.

**Escala**: 0–10. **Modificador = Atributo − 2.** Personagem inicial recebe **20 pontos** de atributo (decisão final, reduzida de uma proposta anterior de 28). **Método de distribuição FECHADO: Compra Livre** — o jogador distribui os 20 pontos livremente entre os 8 atributos, respeitando mínimo **1** e máximo **5** por atributo (nenhum array pré-montado obrigatório).

**Graus dos Atributos**: existe um Grau Máximo Natural (Grau V); além dele, apenas via **Transcendência** (ver Regra Global de Limite Natural) — uma mudança extraordinária que rompe o teto humano comum (bênçãos, rituais, eventos divinos).

**Princípio do Potencial**: os atributos definem o limite natural de um personagem; as perícias definem até onde ele chegou dentro desse limite. (Ideia complementar em discussão, não fechada: atributos como "teto efetivo" que limita o quanto do treinamento em perícia é convertido em desempenho real.)

### 6.4 Perícias
Estrutura de conhecimento em três camadas: **Área de Conhecimento → Perícia → Especialização.**

Existe uma **lista oficial de Perícias Fundamentais** (fechada, para criação/balanceamento) mais **Perícias Personalizadas** (abertas, sujeitas a validação do Mestre) — sistema híbrido.

Lista base por área (com Especializações — a terceira camada, escolhida ao atingir o marco **Adepto, 25 pontos**; quanto mais específica a especialização, maior a eficiência nela e menor a aplicabilidade fora dela):

- **Combate — Armas** *(Controle; Corpo em golpes brutos)*: Espadas *(Espada Longa, Espada Curta, Espada Bastarda, Florete)*; Machados *(Machado de Batalha, Machadinha, Machado Duplo)*; Martelos *(Martelo de Guerra, Maça, Marreta)*; Lanças *(Lança, Alabarda, Tridente)*; Armas Improvisadas *(Objetos do Ambiente, Ferramentas como Arma)*; Armas Exóticas *(Chicotes/Correntes, Armas Duplas, Armas Articuladas)*.
- **Combate — Defesa** *(Controle/Vigor)*: Escudos *(Pequeno, Grande, Torre)*; Armaduras *(Leve, Média, Pesada)*; Esquiva *(Reativa, Acrobática)*; Bloqueio *(com Arma, Corporal)*.
- **Combate Corporal** *(Corpo/Controle)*: Artes Marciais *(Estilo de Punho, Estilo de Chute, Estilo Misto)*; Luta Desarmada *(Golpes Contundentes, Pontos Vitais)*; Agarramento *(Imobilização, Projeção/Arremesso)*.
- **Combate à Distância** *(Controle)*: Arcos *(Curto, Longo, Tiro em Movimento)*; Bestas *(Leve, Pesada)*; Armas de Arremesso *(Facas, Machadinhas, Lanças Curtas)*.
- **Exploração** *(Percepção/Vigor/Controle)*: Percepção *(Observação Visual, Audição, Detecção de Armadilhas)*; Rastreamento *(Rastros Terrestres, Rastros em Água/Neve)*; Sobrevivência *(Forrageamento, Orientação Selvagem, Abrigo)*; Navegação *(Terrestre, Subterrânea, Estelar/Marítima)*; Furtividade *(Movimento Silencioso, Camuflagem)*; Armadilhas *(Detecção, Desarme, Instalação)*; Exploração de Dungeon *(Leitura de Estrutura, Perigo Ambiental)*; Escalada *(Superfícies Rochosas, Estruturas Artificiais)*; Natação *(Águas Calmas, Correntezas)*.
- **Conhecimento** *(Intelecto)*: História *(Antiga, da Guilda, Divina)*; Geografia *(Cartografia, Regiões Selvagens)*; Criaturas *(Bestas, Mortos-vivos, Entidades Extraplanares)*; Religião *(Teologia, Rituais Religiosos)*; Linguagens *(Idiomas Comuns, Idiomas Antigos, Códigos e Cifras)*; Estratégia *(Tática de Combate, Logística Militar)*; Dungeonologia *(Estrutura de Andares, Padrões de Fragmentos)*; Conhecimento de Animais *(Comportamento Animal, Domesticação)*; Ocultismo *(Símbolos Arcanos, Cultos e Seitas)*; Avaliação *(de Itens, de Materiais)*.
- **Cura** *(Intelecto/Percepção)*: Medicina *(Primeiros Socorros, Diagnóstico, Tratamento de Doenças)*; Cirurgia *(Procedimentos Invasivos, Remoção de Corrupção)*; Farmacologia *(Preparo de Remédios, Dosagem)*.
- **Artesanato** *(Controle/Intelecto)*: Ferraria *(Armas, Armaduras, Ferramentas)*; Carpintaria *(Estruturas, Mobiliário, Componentes de Madeira)*; Alfaiataria *(Vestimentas, Armaduras Leves, Acessórios)*; Engenharia *(Mecanismos, Estruturas Complexas, Armadilhas Mecânicas)*; Construção *(Fortificações, Reparos Estruturais)*; Criação de Equipamentos *(Ferramentas Especializadas, Itens Utilitários)*; Culinária *(Preparo de Refeições, Conservação de Alimentos)*.
- **Alquimia** *(Intelecto)*: Poções *(Cura, Buffs, Utilitárias)*; Venenos *(Contato, Ingestão, Inalação)*; Materiais *(Identificação, Extração, Purificação)*; Transmutação *(Metais, Orgânicos, Elementos)*.
- **Magia** *(Afinidade)*: Controle Mágico *(Precisão de Conjuração, Estabilidade de Fluxo)*; Teoria Arcana *(Compreensão de Fórmulas, Pesquisa Teórica)*; Rituais *(de Ligação, de Invocação)*; Afinidade Elemental *(Fogo, Água, Terra, Ar)*; Encantamentos *(de Armas, de Itens)*.
- **Social** *(Presença/Intelecto)*: Diplomacia *(Negociação, Mediação de Conflitos)*; Liderança *(Comando de Grupo, Motivação)*; Comércio *(Avaliação de Preços, Negociação Comercial)*; Intimidação *(Ameaça Física, Ameaça Psicológica)*; Manipulação *(Persuasão Enganosa, Disfarce Social)*.

**Perícias iniciais** representam a história do personagem (ligadas à Origem), não são pontos gratuitos aleatórios.

**Curva de Aprendizado**: aprender algo novo é mais fácil quanto maior a correlação com conhecimento já dominado.

- Correlação Alta (ex.: Espada curta → Florete): redução grande de dificuldade.
- Correlação Média (ex.: Espada → Lança): redução moderada.
- Correlação Baixa (ex.: Espada → Magia): pouca ou nenhuma redução.
- Marco: 0–50 pontos = "Fase de Aprendizado Inicial" (resistência maior); depois de 50, progressão normal.

**Marcos de Perícia** (escala conceitual):

| Pontos | Grau |
|---|---|
| 0 | Sem treinamento |
| 10 | Básico |
| 25 | Adepto |
| 50 | Especialista |
| 75 | Mestre |
| 100 | Lendário |

**Princípio do Treinamento Garantido**: todo dia de treinamento gera um valor **base fixo** de progresso na perícia treinada, independente do Ranking do personagem; instalações, instrutores e a Curva de Aprendizado modificam esse valor (nunca o eliminam).

**Pontos de Treinamento por Dia (FECHADO)** — como cada dia real entre sessões equivale a 1 dia de Interlúdio, o valor base precisa ser pequeno o suficiente para não trivializar os Marcos de Perícia:
```
Pontos de Treinamento/dia = (1 + Bônus de Instalação + Bônus de Instrutor) × Multiplicador de Curva de Aprendizado
```

- **Base**: 1 ponto/dia.
- **Bônus de Instalação**: `Nível da instalação relevante × 0,5` (instalações "avançadas" dedicadas ao domínio, como Academia Militar para Combate, dobram esse bônus: `Nível × 1`).
- **Bônus de Instrutor**: +1 ponto/dia se um Trabalhador Instrutor (§10.4) estiver dedicado àquele personagem/perícia.

**Instalação relevante por Área de Perícia** (mesmo mapeamento das Aptidões, §6.1.5):

| Área de Perícia | Instalação (bônus normal) | Instalação avançada (bônus dobrado) |
|---|---|---|
| Combate — Armas/Defesa/Corporal/Distância | Campo de Treinamento | Academia Militar |
| Exploração | Campo de Treinamento (metade do bônus) | — |
| Conhecimento | Biblioteca | Arquivo/Torre dos Magos (conforme o tema) |
| Cura | Enfermaria | — |
| Artesanato | Oficina / Ferraria (conforme a perícia) | Oficina de Runas |
| Alquimia | Jardim Alquímico (ou Oficina, se ainda não construído) | — |
| Magia | Laboratório Arcano | Torre dos Magos |
| Social | Nenhuma (só Base + Instrutor) | Academia Militar (só Liderança) |

**Multiplicador de Curva de Aprendizado** (retomando §6.4):

| Situação | Multiplicador |
|---|---:|
| Correlação Alta (perícia muito parecida com uma já dominada) | ×1,5 |
| Correlação Média (padrão) | ×1,0 |
| Correlação Baixa (pouco relacionada) | ×0,5 |
| Sem nenhuma correlação, ainda na Fase de Aprendizado Inicial (0-50 pontos) | ×0,25 |

Correlação Alta ou Média pula a resistência extra da Fase de Aprendizado Inicial; só perícias sem correlação alguma sofrem o ×0,25 até os 50 pontos.

*Exemplo*: um Recruta treinando uma perícia de Correlação Média, num Campo de Treinamento Nível II (bônus +1), sem instrutor: `(1+1+0) × 1,0 = 2 pontos/dia`. Uma Guilda madura, com Campo de Treinamento V (+2,5) e um Instrutor dedicado, treinando algo de Correlação Alta: `(1+2,5+1) × 1,5 ≈ 6,75 pontos/dia` — o investimento institucional realmente acelera o jogo, sem tornar o treinamento trivial desde o início.

**Tabela de Penalidade por Curva de Aprendizado (FECHADA)** — diferente do multiplicador acima (que rege a *velocidade* de treino), esta é a penalidade sofrida em **testes** enquanto a perícia ainda não chegou a Básico. Estende a mesma tabela de Bônus de Grau já usada em Ataque/Dano (§7.5):

| Pontos | Grau | Bônus de Grau |
|---|---|---:|
| **0–9** | **Sem Treinamento** | **-2** |
| 10–24 | Básico | +0 |
| 25–49 | Adepto | +1 |
| 50–74 | Especialista | +2 |
| 75–99 | Mestre | +3 |
| 100+ | Lendário | +4 |

O **-2** entra em qualquer teste que use "Bônus de Grau da Perícia" (Ataque, Dano, Testes Absolutos/Opostos relacionados). A Aptidão do domínio (§6.1.5) já reduz a Dificuldade do teste em 1 grau enquanto Sem Treinamento — os dois efeitos se somam, mas nunca eliminam de vez o risco de tentar algo totalmente novo.

**Tabela de Treinamento em Sem Treinamento (FECHADA)**: enquanto a perícia estiver entre 0-9 pontos, os Bônus de Instalação/Instrutor da fórmula de §6.4 são **ignorados** — nenhuma infraestrutura acelera a fase de aprendizado bruto. Em vez disso, aplica-se um teto fixo por Correlação:
```
Pontos de Treinamento/dia (Sem Treinamento) = MIN(Fórmula normal de §6.4, Teto por Correlação)
```

| Correlação | Teto de Pontos/dia | Dias até Básico (10 pontos) |
|---|---:|---:|
| Nenhuma | 1 | 10 dias |
| Baixa | 2 | 5 dias |
| Média | 3 | ~4 dias |
| Alta | 5 | 2 dias |

Assim que a perícia atinge Básico (10+), o teto desaparece e a fórmula completa de §6.4 (com Bônus de Instalação/Instrutor) passa a valer — o investimento institucional acelera o jogo a partir daí, nunca antes.

**Capacidade de Aprendizado / Maestria** — não há limite para *conhecer* perícias, mas há limite para *manter excelência* em muitas ao mesmo tempo. Dois sublimites, calculados a partir dos atributos:

- **Capacidade Técnica** (ligada a atributos físicos) — quantas áreas físicas o personagem consegue dominar bem.
- **Capacidade Intelectual** (ligada a atributos mentais) — quantas áreas mentais consegue dominar bem.

**Princípio da Especialização Imperfeita**: todo conhecimento pode ser adquirido, mas excelência exige dedicação — quem tenta fazer tudo dificilmente será o melhor em qualquer coisa.

### 6.5 Talentos
Não possuem níveis (são binários: tem ou não tem). Categorias: Combate, Arcanos, Exploração, Produção, Sociais, Extraordinários. Possuem origem obrigatória (não surgem "à toa"), requisitos (Ranking, atributo mínimo, perícia mínima, evento narrativo) e podem gerar sinergias entre si. **Princípio da Singularidade**: talentos devem ser raros e significativos, nunca uma lista genérica que todo personagem acumula igual.

### 6.6 Magia e Técnicas Marciais (FECHADO)

#### 6.6.1 Escolas de Magia (lista oficial — 8 escolas)

| Escola | Foco |
|---|---|
| Evocação | Dano direto, energia, elementos |
| Abjuração | Proteção, escudos, resistências |
| Controle | Debuffs, imobilização, controle de área |
| Convocação | Invocar criaturas/objetos |
| Transmutação | Alterar forma/matéria (versão arcana, distinta da Transmutação alquímica) |
| Ilusão | Enganar sentidos, disfarces |
| Necromancia | Manipular vida/morte, dreno, corrupção |
| Adivinação | Informação, detecção, precognição |

#### 6.6.2 Estrutura de uma Magia Individual
Toda magia é definida por: Nome, Escola, Custo (PA), Alcance (reaproveita as Zonas de combate, §7.1), Área (Único Alvo / Área Pequena / Área Grande / Linha), Duração (Instantânea / Turnos / Cena / Persistente), Teste (Oposto vs. Vontade/Afinidade do alvo, ou Absoluto contra dificuldade fixa se não houver resistência ativa), Efeito.

#### 6.6.3 Custo e Redução por Domínio

| Complexidade | PA | Observação |
|---|---:|---|
| Menor | 1 | efeito leve (dano/cura pequenos, utilidade) |
| Moderada | 2 | efeito padrão |
| Maior | 3 | efeito forte |
| Suprema | Conjuração Prolongada (múltiplos turnos) | efeitos que mudam o rumo de um encontro/andar |

**Redução por Grau de Controle Mágico**: Básico +0 | Adepto +0 | Especialista −1 PA (mín. 1) | Mestre −1 PA e −1 Turno de Conjuração Prolongada | Lendário −2 PA (mín. 1) e −1 Turno.

**Interrupção**: durante Conjuração Prolongada, sofrer dano ou falhar um Teste de Vontade (Absoluto, dificuldade = dano recebido) interrompe a magia — PA já gasto é perdido.

#### 6.6.4 Criação de Novas Magias
Via Pesquisa Arcana (§11.2): projeto com tempo por complexidade (Menor 5 dias | Moderada 10 | Maior 20 | Suprema 40+, exigindo Forja/Laboratório Divino), finalizado por Teste Absoluto (Teoria Arcana) que fixa a estrutura definitiva da magia. **Grimórios** armazenam magias conhecidas fisicamente, mas perder o grimório não apaga o conhecimento já aprendido — magia aprendida é permanente (Princípio da Persistência do Conhecimento, §11.3).

**Manual de Criação de Magias Homebrew** — passo a passo: (1) escolher a Escola (§6.6.1), que define o "sabor" do efeito; (2) escolher a Complexidade (Menor/Moderada/Maior/Suprema, §6.6.3), que já fixa o Custo em PA e o teto de poder; (3) definir Alcance (Zona); (4) definir Área (Único Alvo/Área Pequena/Área Grande/Linha); (5) definir Duração (Instantânea/Turnos/Cena/Persistente); (6) definir Teste (Oposto ou Absoluto); (7) definir o Efeito Único, redigido em termos de mecânicas já existentes (dano equivalente a uma categoria de arma, Condição aplicada, bônus/penalidade em Defesa Passiva ou teste); (8) validar contra a checklist abaixo.

**Checklist de Balanceamento**: **Regra do Escalonamento** — aumentar Área, Duração ou Alcance além do padrão daquela Complexidade custa +1 PA extra ou obriga a subir de Complexidade; **Regra do Efeito Único** — uma magia faz uma coisa bem definida, combinar múltiplos efeitos fortes exige Complexidade Maior/Suprema ou deve virar duas magias; **Regra da Origem do Conhecimento** — nenhuma magia nasce sem Pesquisa Arcana, Grimório, mestre ou ritual documentado; **Regra da Simetria** — criaturas que usam magia seguem os mesmos parâmetros, qualquer exceção precisa vir de uma Característica Única na ficha da criatura (§9.5), nunca de "porque é monstro".

#### 6.6.5 Encantamento de Itens e Rituais
**Encantamento**: ao fabricar/modificar um item (§6.7.4/§6.7.5), adicionar uma Propriedade mágica exige Teste Absoluto adicional (Encantamentos), na instalação mínima Torre dos Magos ou Laboratório Arcano.

**Rituais**: diferente de magias de combate, usam Turnos de Exploração (§8.1, não PA), permitem efeitos grandes demais para o combate (invocações maiores, selos, contato com entidades). Exigem Teste (Rituais), tempo em Turnos, materiais/catalisadores, e podem envolver múltiplos participantes contribuindo Vontade ou Afinidade. Falha em Ritual é mais perigosa que falha em magia comum (risco de efeito reverso/backfire — Princípio da Complexidade Arcana).

#### 6.6.6 Magias de Exemplo (ponto de partida — 1 por Escola, com evolução Menor → Moderada → Maior)

| Escola | Menor (1 PA) | Moderada (2 PA) | Maior (3 PA) |
|---|---|---|---|
| Evocação | **Lança de Fogo** — 1 alvo, Contato/Curta, dano de fogo instantâneo | **Rajada Flamejante** — linha, Média, dano maior + ignição leve | **Tempestade de Chamas** — área pequena, dano contínuo por 2 turnos |
| Abjuração | **Escudo Arcano** — +2 Defesa Passiva, 1 turno | **Barreira Protetora** — +4 Defesa Passiva, Cena, só a si mesmo | **Muralha Absoluta** — +4 Defesa Passiva à área pequena (aliados), Cena |
| Controle | **Amarras de Vontade** — Imobiliza 1 alvo, 1 turno | **Grilhões Arcanos** — Imobiliza + Enfraquecido, 2 turnos | **Prisão de Vontade** — Imobiliza área pequena, Cena |
| Convocação | **Lâmina Espectral** — invoca arma temporária (1 turno) | **Familiar de Batalha** — invoca criatura pequena, Cena | **Avatar Convocado** — invoca aliado poderoso, Cena, Conjuração Prolongada |
| Transmutação | **Toque Deformante** — altera superfície/objeto pequeno | **Metamorfose Parcial** — altera parte do próprio corpo, ganho utilitário, Cena | **Transfiguração Completa** — altera a forma por completo, Cena |
| Ilusão | **Névoa Enganosa** — camufla 1 alvo, +Furtividade | **Duplicata Ilusória** — imagem falsa, confunde 1 ataque | **Véu da Mentira** — ilude um grupo/área inteira, Cena |
| Necromancia | **Toque Debilitante** — dreno pequeno de PV/Vigor | **Sopro Sombrio** — dreno em área pequena | **Chamado da Sepultura** — invoca mortos-vivos menores temporários, Conjuração Prolongada |
| Adivinação | **Vislumbre** — revela 1 informação simples sobre alvo/ambiente | **Leitura do Fio do Destino** — prevê a próxima ação de 1 alvo, concede Vantagem | **Olho Onisciente** — revela mapa/segredos de uma área inteira, Cena |

#### 6.6.7 Árvore de Técnicas por Estilo
Cada grande grupo de combate tem sua própria árvore, nas 4 categorias já existentes:

- **Posturas** — passivas, ativadas no início do turno por 1 PA, mantidas sem custo depois.
- **Técnicas** — ativas, custam 1-2 PA (podem evoluir de Técnica I para Técnica II com mais PA/efeito).
- **Reações** — usam a Reação do turno (§7.3), efeitos defensivos/contra-ataque.
- **Técnicas Supremas** — custam os 3 PA do turno, limite de uso (1x por combate ou expedição).

**Requisitos formais por categoria:**

| Categoria | Perícia mínima na arma/estilo | Ranking mínimo |
|---|---|---|
| Postura | Adepto (25) | — |
| Técnica | Especialista (50) | — |
| Reação | Especialista (50) | — |
| Técnica Suprema | Mestre (75) | Prata+ |

**Criação de Técnicas Novas**: via "Projeto de Técnica" no Interlúdio — tempo por categoria (Postura 5 dias | Técnica 10 | Reação 10 | Suprema 25), finalizado por Teste Absoluto na perícia da arma/estilo correspondente. **Variações**: uma técnica-base pode ganhar variações situacionais (mesmo efeito principal, contexto diferente) desbloqueadas via alta correlação de Especialização (§6.4).

**Manual de Criação de Técnicas Homebrew** — passo a passo: (1) escolher o Estilo/Arma-base, que precisa corresponder a uma Perícia já existente em §6.4; (2) escolher a Categoria (Postura/Técnica/Reação/Suprema), que já fixa o custo de PA e a perícia mínima exigida (tabela acima); (3) definir o Efeito, sempre referenciando mecânicas existentes (bônus/penalidade de Defesa Passiva ou dano, Condição aplicada, alcance/área afetada, uso da Reação); (4) validar contra a checklist abaixo.

**Checklist de Balanceamento**: **Regra do Efeito Único** — mesma regra da Magia, uma técnica faz uma coisa bem definida; **Regra da Progressão** — se a técnica tiver estágios (Técnica I → II), o estágio II sempre exige Perícia Mestre e custa +1 PA a mais que o estágio I; **Regra da Supremacia Rara** — Técnicas Supremas são sempre limitadas a 1x por combate ou expedição, nunca de uso livre; **Regra da Compatibilidade de Arma** — a técnica só funciona com a categoria de arma/estilo correspondente, nunca é genérica entre estilos diferentes.

#### 6.6.8 Técnicas de Exemplo (ponto de partida — 3 estilos)

**Espadas**

| Categoria | Técnica | Efeito |
|---|---|---|
| Postura | Postura Ofensiva | 1 PA, +1 dano / −1 Defesa Passiva enquanto mantida |
| Técnica I → II | Golpe Giratório | I (1 PA): atinge 2 alvos em Contato → II (2 PA, Mestre): atinge todos em Contato |
| Reação | Aparar | Reação, +Defesa Passiva contra 1 ataque; se suceder, permite contra-ataque com dano reduzido |
| Suprema | Corte que Divide o Véu | 3 PA, 1x/combate: ignora metade da Redução de Dano da armadura e aplica Sangrando |

**Combate Corporal (Luta Desarmada)**

| Categoria | Técnica | Efeito |
|---|---|---|
| Postura | Guarda Fechada | 1 PA, +2 Defesa Passiva / −1 dano enquanto mantida |
| Técnica I → II | Golpe Articulado | I (1 PA): ataque com chance de Atordoado leve → II (2 PA, Mestre): chance/efeito maior |
| Reação | Contragolpe | Reação, se a Defesa Ativa suceder, aplica dano imediato ao atacante |
| Suprema | Ruptura de Pontos Vitais | 3 PA, 1x/combate: ignora totalmente a Redução de Dano da armadura, aplica Ferido Grave |

**Arcos (Distância)**

| Categoria | Técnica | Efeito |
|---|---|---|
| Postura | Mira Calculada | 1 PA, +1 precisão contra um alvo marcado, mantida até trocar de alvo |
| Técnica I → II | Tiro Encadeado | I (2 PA): atinge 2 alvos na mesma linha → II (3 PA, Mestre): atinge até 4 alvos |
| Reação | Disparo de Interceptação | Reação, ataca um inimigo que entra na Zona Curta |
| Suprema | Flecha que Perfura o Véu | 3 PA, 1x/combate: ignora Cobertura (Parcial/Total) e a Redução de Dano da armadura |



#### 6.6.9 Magias e Técnicas Iniciais (FECHADA)

Pelas regras normais, aprender uma Postura/Técnica exige Perícia Adepto (25 pontos) e magias exigem projeto de Pesquisa Arcana — mas a Origem só concede 15 pontos na perícia primária. Sem uma regra específica, nenhum personagem começaria com qualquer magia ou técnica utilizável.

**Regra do Conhecimento Herdado**: a criação de personagem concede um pacote fixo e pequeno de magias/técnicas, representando treinamento incompleto trazido de antes da Guilda — esse conhecimento **ignora o requisito normal de perícia mínima** (é bagagem prévia, não experiência de campo). Usá-las continua custando PA normalmente — a regra libera apenas o *conhecimento*, nunca o *custo de uso* (Regra de Ouro intacta).

- **Aptidão em Magia** (§6.1.5) → conhece **2 Magias de Complexidade Menor** (da lista §6.6.6 ou homebrew aprovada antes da campanha). Se a Origem também for arcana (ex.: Estudante Arcano, §6.1.2) → +1 extra (total 3).
- **Aptidão em Combate** (§6.1.5) → conhece **1 Postura + 1 Técnica (estágio I)**, de um estilo compatível com a Perícia primária da Origem.
- Sem nenhuma dessas Aptidões, mas ainda quer 1 magia/técnica pontual → **troca o Talento Inicial (§6.1.6)** por 1 Magia Menor ou 1 Técnica/Postura básica.

#### 6.6.10 Magia Intuitiva (Magia Livre) — FECHADA

Um personagem com ao menos 1 ponto em Controle Mágico pode tentar produzir, na hora, um efeito mágico que não conhece como magia formal — desde que caiba logicamente em uma Escola na qual tenha Afinidade praticada.

- **Custo**: sempre **+1 PA a mais** que a Complexidade equivalente estimada pelo Mestre (falta de estrutura de um feitiço improvisado).
- **Teste duplo**: além do teste normal do efeito (se houver alvo/resistência), o jogador faz um **Teste Absoluto adicional de Controle Mágico**, dificuldade definida pela Complexidade estimada — representa "montar" a magia ali mesmo.
- **Falha** = PA consumido, sem efeito. **Falha Crítica** = Interrupção Reversa — o personagem sofre uma Condição leve, ou dano igual à Complexidade estimada, ou gera um pico na Tensão Arcana/Divina (§12), a critério do Mestre.
- **Limite**: nunca reproduz efeito de Complexidade Suprema; nunca cria item físico permanente, só efeitos de cena/instantâneos.
- **Consequência positiva**: se usada com sucesso, o Mestre pode formalizá-la como **Magia Descoberta** — passa a ser conhecida oficialmente sem custo extra de Pesquisa, podendo depois ser refinada via Pesquisa Arcana (§6.6.4) para reduzir seu custo ao padrão. Reforça o "aprender fazendo" que já permeia o sistema.

### 6.7 Equipamentos e Crafting (FECHADO)

Filosofia: equipamentos devem ampliar possibilidades, não apenas números (**Princípio da Identidade dos Equipamentos**). Estrutura em quatro pilares: Qualidade, Material, Construção, Modificações (**Princípio da Modularidade**). Carregam conhecimento/história (**Princípio do Legado Material** — um item pode "ensinar" algo a quem o estuda).

**6.7.1 Raridade** (já usada no NP, §6.8, agora com efeito mecânico completo):

| Raridade | Propriedades Máximas | Bônus Base (Ataque/Dano/Defesa) | NP |
|---|---|---|---:|
| Comum | 0 | +0 | 1 |
| Incomum | 1 | +1 | 3 |
| Raro | 2 | +2 | 7 |
| Épico | 3 | +3 | 15 |
| Lendário | 4 | +4 | 30 |
| Divino | 5+ | +5 ou efeito único | 50+ |

**6.7.2 Categorias**: Armas, Armaduras, Escudos, Ferramentas, Consumíveis, Artefatos, Relíquias.

**6.7.3 Propriedades e Encantamentos** (lista fechada de 20 — cada uma ocupa 1 slot de Propriedade, até o teto da Raridade):

Afiado (+1 dado de dano) · Preciso (-1 dificuldade no ataque) · Resistente (+2 Golpes de Desgaste, §6.7.6) · Leve (reduz peso) · Flamejante / Gélido / Corrosivo (dano elemental extra) · Perfurante (ignora parte da Redução de Dano da armadura) · Vampírico (cura fração do dano causado) · Ressonante (bônus em conjuração, só itens arcanos) · Camuflado (bônus de Furtividade) · Selado (resistência a 1 Condição específica) · Instável (efeito forte, mas com chance de falha/backfire) · Regenerativo (armadura recupera 1 PV extra a cada Descanso Curto) · Silencioso (bônus de Furtividade ao se mover com o item equipado) · Ancorado (a arma não pode ser desarmada) · Adaptável (alterna entre 2 categorias de dano sem custo de ação) · Amplificador (+1 zona no alcance de conjuração de itens arcanos) · Fragmentador (dano leve em área pequena ao redor do alvo) · Selante (reduz chance de gerar a Condição Sangrando) · Amaldiçoado (efeito forte com penalidade fixa sempre ativa, definida na criação).

**Manual de Propriedades Homebrew**: (1) **Regra do Slot Único** — toda propriedade ocupa exatamente 1 slot, independente de quão "boa" pareça; (2) **Regra do Peso Calibrado** — o efeito deve equivaler a um destes, nunca mais: +1 dado de dano, -1 grau de dificuldade num nicho específico, um recurso pontual reutilizável (1x/expedição ou interlúdio), ou resistência/imunidade a 1 Condição; (3) **Regra da Compatibilidade** — precisa fazer sentido físico com a categoria do item; (4) **Regra da Contrapartida** — propriedades muito fortes (equivalentes a Instável/Amaldiçoado) exigem penalidade sempre ativa ou chance real de efeito reverso.

**6.7.4 Criação (Crafting)**:
```
Teste Absoluto (Perícia de Artesanato) vs Dificuldade da Receita
```

| Raridade-alvo | Dificuldade | Tempo | Custo em Materiais (FECHADO) | Instalação mínima |
|---|---|---|---:|---|
| Comum | Trivial/Fácil | 1 dia | 5 | Oficina Básica |
| Incomum | Fácil/Moderada | 3 dias | 15 | Oficina Básica |
| Raro | Moderada/Difícil | 7 dias | 35 | Ferraria |
| Épico | Difícil/Muito Difícil | 14 dias | 75 | Ferraria Avançada |
| Lendário | Muito Difícil/Heroica | 30 dias | 150 | Forja Rúnica |
| Divino | Lendária (exige Moeda de Pacto) | Requer projeto de Pesquisa prévio | 250 + 10 Moedas de Pacto | Forja Divina |

Resultado pela Margem de Sucesso (§5.4): Sucesso = item padrão da raridade-alvo | Grande Sucesso = +1 Propriedade extra (sem exceder o teto de raridade) | Sucesso Extraordinário = tempo reduzido à metade ou material bônus reaproveitável | Falha = metade dos materiais perdida | Falha Crítica = materiais perdidos por completo (risco de dano à instalação/ferramenta).

**6.7.5 Melhoria, Modificação e Reconstrução**: **Melhoria** reforça o Bônus Base dentro da mesma raridade (até o teto dela); **Modificação** troca 1 Propriedade existente por outra de custo equivalente; **Reconstrução** eleva o item para a raridade seguinte — mesmo custo de material da criação do zero, mas com metade do tempo.

**6.7.6 Durabilidade — Golpes de Desgaste**: em vez de "HP de item", um item perde 1 Golpe de Desgaste apenas em **Falha Crítica** de ataque/defesa usando-o, ou em evento narrativo específico (armadilha, corrosão, ambiente extremo).

| Raridade | Golpes de Desgaste até precisar manutenção |
|---|---:|
| Comum | 3 |
| Incomum | 4 |
| Raro | 5 |
| Épico | 6 |
| Lendário | 8 |
| Divino | 10 |

Ao esgotar, o item fica **Danificado** (-1 no Bônus Base) até ser reparado no Interlúdio (Perícia de Artesanato correspondente, tempo curto). A Propriedade Resistente concede +2 Golpes de Desgaste extras.

**6.7.7 Guia Completo de Criação de Itens**

Passo a passo geral (vale para qualquer origem do item): (1) Categoria; (2) Raridade-alvo (§6.7.1, já define teto de Propriedades e Bônus Base); (3) Propriedades até o teto, oficiais ou homebrew validadas; (4) Golpes de Desgaste (§6.7.6, +2 se Resistente); (5) se arma/armadura, categoria de dano/proteção (Leve/Média/Pesada/Duas Mãos, §7.5); (6) Gancho Narrativo (recomendado para Raro+, ligado ao Princípio do Legado Material); (7) validação final — o item amplia possibilidades ou é só um número maior?

- **Caminho A — Mestre criando item para a Dungeon (loot/recompensa)**: o item já nasce pronto, sem passar pelo Teste de Crafting. Se for Ativo Estratégico (§9.10), atribuir seu Valor Estratégico (1-5). Itens Lendários/Divinos únicos devem ter uma **Complicação Material** (equivalente à Complicação de Histórico, §6.1.3) — todo item extraordinário carrega um peso ou consequência.
- **Caminho B — Jogador fabricando via Crafting (Interlúdio)**: segue §6.7.4, na instalação mínima exigida. Exige ter a Receita Conhecida ou um Projeto Descoberto (§11.2) — não é possível fabricar uma raridade sem antes ter a receita correspondente.

### 6.8 Nível de Poder — fórmula final aprovada
Após testar uma fórmula multiplicativa (que inflacionava demais a especialização), a versão final aprovada usa **soma ponderada**:

```
NP = Poder Base + Poder de Especialização + Equipamentos

Poder Base = Atributos + Perícias

Poder de Especialização = Talentos + Habilidades
  Talento menor = 1 | médio = 3 | maior = 5
  Habilidade comum = 5 | avançada = 10 | suprema = 20

Equipamentos:
  Comum = 1 | Incomum = 3 | Raro = 7 | Épico = 15 | Lendário = 30 | Divino = 50+
```

**Faixas oficiais de NP por Ranking (FECHADO)** — simulado através dos 8 Rankings, com ±15% de variação individual aceitável:

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

O incremento entre Rankings vizinhos varia entre +25 e +75 pontos — nunca dobra de um rank para o outro. Esse crescimento é deliberadamente **suave e previsível** (nunca exponencial): mesmo no topo, a maior parte do NP vem de Perícias e Equipamentos (que têm teto natural via Grau V / raridade máxima), não de um multiplicador desenfreado, então o dado (2d10) continua relevante em qualquer Ranking. Essa tabela conecta-se diretamente ao Orçamento de Ameaça (§9.9): o Mestre usa o Ranking médio do grupo como referência imediata de qual faixa de andar está calibrada para ele.

### 6.9 Morte, Legado e Cristais de Memória

- Morte é permanente e sem proteção narrativa. O jogador (Patrono) cria outro personagem.
- Ao morrer, o personagem **dropa um Cristal de Memória** — uma "caixa preta" que só pode ser acessado no **Memorial**. Acessar memórias custa tempo e não transmite atributos nem perícias automaticamente — apenas conhecimento concreto vivido pelo personagem (mapas, idiomas, armadilhas conhecidas, soluções de puzzles).
- Três níveis de recuperação de memória: **Registro** (fatos simples) → **Técnica** (procedimentos/métodos) → **Memória Integral** (mais completa, mais cara em tempo).
- **Nível de Recrutamento / Capacidade de Formação (CF)**: um novo personagem nunca começa do zero absoluto — a Guilda já evoluiu, então ele recebe uma formação compatível com a infraestrutura atual (atributos iniciais, perícias iniciais, talentos disponíveis, equipamentos fornecidos, técnicas básicas). Ele nunca alcança o nível médio dos veteranos, mas também nunca fica muito abaixo — evitando tanto trivializar a morte quanto punir demais quem perdeu um personagem.

---

## 7. Combate (FECHADO)

### 7.1 Movimento — Sistema Híbrido
A Escala do Encontro (§9.7) define o modo de movimento:

- **Pequena Escala** (Comando Individual/Tático) → **Grid/Hex**, medido em **quadros** (grid quadrado ou hexagonal são intercambiáveis, mecanicamente idênticos).
- **Larga Escala** (Hordas, Comando Militar/Estratégico) → **Zonas** (Contato/Curta/Média/Longa), 1 PA por zona adjacente.

**Deslocamento (modo Grid)**: `Deslocamento = 4 + Mod(Vigor)` quadros por PA gasto em Mover.

**Tabela de conversão de alcance** (unifica os dois modos):

| Zona | Grid/Hex (quadros) | Penalidade de alcance |
|---|---|---|
| Contato | 0–1 | Armas de longe sofrem penalidade grande |
| Curta | 2–6 | Alcance ideal da maioria de arcos/bestas |
| Média | 7–12 | -1 grau de dificuldade adicional |
| Longa | 13+ | -2 graus de dificuldade adicional |

Cobertura (válida nos dois modos): **Leve** (+2 Defesa Passiva) | **Parcial** (+4 Defesa Passiva, metade do dano se acertar) | **Total** (impossível de atingir com ataque direto).

### 7.2 Iniciativa
`Iniciativa = 2d10 + Mod(Controle)`. Ordem decrescente; empate resolvido por maior Percepção.

### 7.3 Ações e Pontos de Ação
**3 PA por turno** (valor base) + **1 Reação**. PA só aumenta por talentos raros, equipamentos, perícias especiais ou poderes divinos — atributos nunca aumentam PA diretamente. Ações: Mover (1 PA/zona ou até o Deslocamento em quadros), Atacar (1-2 PA conforme categoria de arma), Defender (1 PA, ativa Teste Oposto — ver §7.5), Usar Item (1 PA), Preparar Ação (guarda PA para reagir a um gatilho).

**Ataques de Oportunidade**: não existem como mecânica própria — são cobertos pela Reação já existente ("Interceptação": um personagem pode usar sua Reação quando um inimigo sai de sua Zona/quadro de Contato sem se desvincular com cautela).

### 7.4 Defesa Híbrida
Combate é Teste Oposto por definição (§5.2), mas por padrão isso tornaria todo ataque lento. Por isso:

- **Defesa Passiva** (padrão, sem custar PA) — o atacante precisa superá-la, funcionando como Teste Absoluto na prática:
```
Defesa Passiva = 10 + Mod(Controle) + Bônus Base do Equipamento (armadura, §6.7.1) + Bônus Base do Equipamento (escudo, se equipado)
```

- **Defesa Ativa** (opcional) — o defensor gasta 1 PA (ação Defender) ou sua Reação, e o ataque vira Teste Oposto de verdade (o defensor rola contra o atacante).

Isso resolve a tensão entre "combate rápido" e "combate é Teste Oposto": rápido por padrão, tático quando o jogador investe recurso.

### 7.5 Ataque e Dano (CORRIGIDO por Playtest, §17.10)
```
Ataque = 2d10 + Bônus de Grau do Atributo + Bônus de Grau da Perícia  →  vs Defesa Passiva (ou Teste Oposto se defendido ativamente)
  Bônus de Grau do Atributo = Atributo (score) − 1   [Grau I=+0 | II=+1 | III=+2 | IV=+3 | V=+4; além de V, só via Transcendência, §6.3]
  Bônus de Grau da Perícia  = Básico +0 | Adepto +1 | Especialista +2 | Mestre +3 | Lendário +4

Dano = Dado da categoria de arma + Mod(Atributo) + Bônus de Grau de Perícia + Bônus Base do Equipamento (arma, §6.7.1)
  Armas Leves: 1d6 | Médias: 1d8 | Pesadas: 1d10 | Duas Mãos: 2d6

Redução de Dano da Armadura: Leve -1 | Média -2 | Pesada -3 (mínimo 1 de dano sempre passa)
```
**Errata de balanceamento (Playtest, §17.10)**: a versão original desta fórmula somava a Perícia da arma como valor bruto ao Ataque — como Perícia cresce até 100-200+ pontos, isso tornava o 2d10 irrelevante a partir de Aço/Prata e quebrava a escala entre Rankings. A versão corrigida usa **Bônus de Grau** (do Atributo e da Perícia) nos dois lados da fórmula, mantendo o dado sempre relevante. Também foi corrigida a ausência do **Bônus Base do Equipamento** (§6.7.1) em Dano e Defesa Passiva — antes, equipamentos melhores não tinham nenhum efeito na matemática real de combate. Por decisão de design, **Equipamento nunca entra em Ataque** (não deve influenciar a taxa de acerto, só Dano e Defesa).

A Margem de Sucesso (§5.4) modifica o resultado: Sucesso = dano normal | Grande Sucesso = +1 dado de dano extra | Sucesso Extraordinário = +2 dados de dano extra.

### 7.6 Pontos de Vida e Recuperação
```
PV = 10 + (Vigor × 2) + Bônus de Ranking
Bônus de Ranking: Bronze +0 | Ferro +5 | Aço +10 | Prata +15 | Ouro +20 | Mithril +25 | Adamante +30 | Lendário +35
```
Recuperação natural só ocorre no Interlúdio (via §11/Cura); dentro da Dungeon, descanso curto recupera apenas uma fração pequena (a critério do Mestre conforme a Pressão do andar) — reforça a letalidade.

### 7.7 Condições (lista fechada)
Ferido Leve, Ferido Grave, Sangrando, Atordoado, Enfraquecido, Amedrontado, Imobilizado, Agonizante, Morto.

### 7.8 Morte
Ao chegar a 0 PV, o personagem fica **Agonizante** (inconsciente; testes de Medicina podem estabilizar, 1 PA). Qualquer dano adicional recebido enquanto Agonizante causa **morte instantânea**. Isso preserva "sem piedade, sem proteção narrativa" (§1) e ainda dá uma janela real para os aliados agirem. Ao morrer, ativa o Cristal de Memória (§6.9).

### 7.9 Índice de Ameaça
Relaciona o Nível de Poder da Dungeon/inimigos com o do grupo — ver §9.8/§9.9 para a versão final e expandida como Sistema de Encontros e Orçamento de Ameaça.

---

## 8. Exploração (FECHADO)

### 8.1 Turno de Exploração
Fora do combate, a Dungeon avança em **Turnos de Exploração = 10 minutos** cada. Em 1 turno o grupo pode: mover-se entre pontos de interesse, procurar armadilhas/segredos, descansar brevemente, ou realizar uma ação de perícia mais longa. Consumo de recursos (tochas, ritmo de comida/água) é contado por Turno de Exploração — PA continua exclusivo de combate/ações rápidas.

### 8.2 Visão e Iluminação

| Condição | Efeito |
|---|---|
| Iluminado | Sem penalidade |
| Penumbra | -1 grau de dificuldade em testes visuais e ataques à distância |
| Escuridão Total | Testes visuais impossíveis sem sentido especial; deslocamento reduzido à metade |

Fontes de luz: **Tocha** (raio Curto, dura 6 Turnos de Exploração = 1 hora) | **Luz Mágica** (raio Médio, duração ligada ao custo de PA/magia, §6.6) | luz ambiente natural (varia por Arco/andar).

### 8.3 Navegação e Mapas
Perícia Exploração de Dungeon/Navegação mantém a rota e evita ficar perdido. Falha crítica = grupo perdido (gasta 1 Turno extra, risco de evento/encontro). Mapas físicos e mapas obtidos como Ativo Estratégico (§9.10) reduzem a dificuldade de navegação em andares já parcialmente mapeados — reforça que Informação é Recurso (§9.4).

### 8.4 Armadilhas

- **Detecção**: Teste Absoluto (Percepção/Armadilhas) vs dificuldade ligada ao Ranking do andar (§5.3).
- **Desarme**: Teste Absoluto (Armadilhas); falha pode ativar a armadilha.
- **Dano**: segue a mesma lógica de §7.5 (Leve/Média/Pesada), podendo incluir efeitos adicionais (veneno, Condição).
- Falha nunca bloqueia a exploração — gera consequência (Princípio dos Fracassos como Consequência, §1).

### 8.5 Exploração em Grupo
Papéis sugeridos (guia, não obrigatórios): Batedor (percepção/furtividade à frente), Guarda-Costas (retaguarda), Navegador (mantém rota), Especialista (armadilhas/puzzles). O grupo pode se dividir em subgrupos para agir em paralelo no mesmo Turno — mas cada subgrupo separado reduz o Poder do Grupo (§9.8) localmente, se um encontro ocorrer.

### 8.6 Acampamento e Descanso

- **Descanso Curto** (1 Turno dedicado): recupera fração pequena de PV (mesma regra de §7.6), permite reorganizar equipamento.
- **Acampamento Completo** (bloco maior de tempo, consumindo comida/água): recupera mais PV, mas exige local sem Pressão ativa (§9.2) e sempre corre risco de evento conforme a Pressão do andar.
- Descansar sempre custa tempo — nunca é "grátis" (Regra de Ouro, §1).

### 8.7 Consumo de Recursos

| Recurso | Consumo |
|---|---|
| Comida | 1 ração/personagem por dia de Tempo da Dungeon |
| Água | 1 cantil/personagem por dia (dobra em ambientes áridos) |
| Tocha | 1 unidade por 6 Turnos de Exploração |
| Corda | Consumível por uso específico (escalada, poços) |
| Munição | 1 unidade por ataque à distância realizado |
| Capacidade de Carga | `Corpo × 5` (peso); exceder gera penalidade de Deslocamento e testes físicos |

Falta de comida/água por dias seguidos gera as Condições **Faminto/Desidratado** (penalidades crescentes, nunca morte direta — consequência, não bloqueio).

---

## 9. A Dungeon

### 9.1 Estrutura dos Andares
Cada andar possui: Identidade (bioma/tema herdado do universo-fragmento de origem), Objetivo Principal, Objetivos Secundários, Condição de Fracasso. Tipos: Exploração, Defesa, Ataque, Caçada (entre outros já listados em §4.2).

### 9.2 Pressão da Dungeon (FECHADO por Teste de Ponta a Ponta, §17.10)
Escala de estado: **Estável → Agravado → Crítico → Colapso.** Representa a urgência/deterioração/corrupção crescente de um andar; alimenta eventos, penalidades e mudanças ambientais (**Princípio da Pressão Temática** — cada tipo de andar "pressiona" de um jeito coerente com seu tema: floresta viva cresce e sufoca, andar vulcânico aumenta o calor, fortaleza reforça defesas, etc.).

**Contador numérico**: cada andar tem um contador de Pressão de **0 a 100**, que **reinicia a cada novo andar** (consequências narrativas de ter chegado a Crítico/Colapso podem ecoar no andar seguinte, a critério do Mestre, mas o contador em si não acumula entre andares).

| Estado | Faixa | Multiplicador no PE dos encontros restantes |
|---|---:|---:|
| Estável | 0–24 | ×1,00 |
| Agravado | 25–59 | ×1,10 |
| Crítico | 60–89 | ×1,25 |
| Colapso | 90–100 | ×1,50 + dispara automaticamente um Evento de Colapso (definido pelo Mestre: reforços inimigos, mudança ambiental drástica, ou risco imediato à Condição de Fracasso do andar) |

**Fontes padrão de Pressão** (o Mestre soma conforme a narrativa exige; a lista é um ponto de partida, não uma tabela rígida):

- Cada Turno de Exploração além do previsto pela Duração do andar (§9.9): **+5**.
- Cada combate concluído (barulho, rastro, atenção da Dungeon): **+10**.
- Falha crítica em teste relevante (armadilha disparada, alarme, erro grave): **+15**.
- Evento narrativo específico definido pelo Mestre (ex.: a horda percebe os jogadores): **+20 a +60**, conforme o impacto.

O multiplicador de Pressão se soma aos multiplicadores de Terreno/Inteligência/Objetivo já existentes na fórmula de Poder do Encontro (§9.8), reforçando mecanicamente que a Dungeon reage à presença dos jogadores em vez de esperar parada.

### 9.3 Estado dos Andares
Inexplorado → Explorado → Conquistado → Dominado.

### 9.4 Recompensas e Informação
Recompensas: Conhecimento, Recursos, Progresso. **Informação é tratada como recurso concreto** — conhecer previamente um chefe deve dar vantagem real, equivalente a poder bruto.

### 9.5 Criaturas (FECHADO)

**9.5.1 Tipos** (natureza/origem — lista fechada, 8 tipos): Bestas · Mortos-vivos · Aberrações · Espíritos · Constructos · Humanoides Corrompidos · Dracônicos · Entidades Extraplanares. *(Tipo descreve a natureza; Função, abaixo, descreve o papel na Dungeon — os dois se combinam livremente: um Guardião pode ser Constructo ou Morto-vivo, por exemplo.)*

**9.5.2 Função na Dungeon**: Predador, Guardião, Soldado, Parasita, Evento Vivo.

**9.5.3 Comportamento (IA)** — liga a categoria narrativa ao multiplicador de Inteligência do Sistema de Encontros (§9.8):

| Comportamento | Multiplicador equivalente | Regra de ação do Mestre |
|---|---|---|
| Instintiva | Instinto (×1) | Ataca sempre o alvo mais próximo ou de menor Defesa Passiva; nunca usa tática de grupo; foge quando PV < 25% ou Moral cai |
| Inteligente | Tático (×1,2) ou Militar (×1,5) | Escolhe alvo pela ameaça percebida; pode recuar 1 zona para reposicionar; usa a Reação de forma otimizada |
| Estratégica | Genial (×2) | Coordena com outras criaturas do grupo, evitando sobrepor alvos e mirando fraquezas conhecidas; pode fingir retirada; usa terreno/Cobertura ativamente; prioriza eliminar suporte/conjuradores primeiro |

**9.5.4 Tabela de Características Naturais** (custo em NP, mesma lógica de peso de Talentos/Habilidades, §6.8):

| Peso | NP | Exemplos |
|---|---:|---|
| Menor | 1 | Visão no Escuro, Olfato Apurado, Sentido Sísmico, Resistência a 1 elemento |
| Média | 3 | Carapaça (Redução de Dano +2), Voo, Camuflagem Natural, Múltiplos Olhos (imune a Surpreendido) |
| Maior | 5 | Regeneração (recupera PV/turno), Veneno Potente (Condição automática em acerto), Ataques Múltiplos (+1 ataque/turno sem PA extra) |
| Suprema | 10 | Metamorfose, Núcleo Dimensional (revive 1x ao ser destruído), Imunidade a uma categoria inteira de dano |

**9.5.5 Fórmula de NP de Criatura**:
```
NP(criatura) = (Atributos + Perícias Naturais) + Σ Características + Σ Habilidades + Equipamento
```
(mesma lógica de §6.8; Habilidade comum=5/avançada=10/suprema=20)

**9.5.6 Categorias de Criatura** (mapeadas nas faixas de Ranking, §6.8, Princípio da Simetria):

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

**9.5.7 Ficha Simplificada de Criatura** (formato de mesa, rápido de usar):
```
NOME (Tipo — Função — Categoria: NP XX)
Comportamento: Instintiva / Inteligente / Estratégica
PV: XX | Defesa Passiva: XX | Deslocamento: XX
Ataque principal: 2d10 + X vs Defesa | Dano: XdX+X
Características: [lista breve, 1 linha cada]
Habilidades: [lista breve]
Fraqueza: [1 linha]
Recompensas: [lista breve — Materiais/Conhecimento/Técnicas/Cristais, §9.4]
```

**9.5.8 Manual de Criação de Criaturas** — passo a passo: (1) Conceito (nome, tema); (2) Tipo (§9.5.1, oficial ou homebrew — ver §9.5.9); (3) Função (§9.5.2); (4) Comportamento (§9.5.3, já fixa o multiplicador de encontro); (5) Categoria-alvo (§9.5.6, já define a faixa de NP desejada); (6) distribuir o NP-alvo entre Atributos+Perícias Naturais, Características (§9.5.4), Habilidades e Equipamento, até bater o NP da categoria escolhida; (7) definir 1 Fraqueza obrigatória; (8) definir Recompensas; (9) validar contra a checklist abaixo.

**Checklist de Balanceamento**: **Regra da Fraqueza** — toda criatura precisa de ao menos 1 Fraqueza clara, sem exceção; **Regra da Função Clara** — a criatura tem 1 função primária definida, nunca um "monstro genérico que ataca"; **Regra do Teto de Categoria** — o NP total não pode ultrapassar a faixa da Categoria escolhida em mais de 15%.

**9.5.9 Manual de Criação de Tipos Homebrew**: (1) um Tipo Homebrew descreve a **natureza/origem** da criatura, nunca sua função ou comportamento (essas são camadas separadas, §9.5.2/§9.5.3); (2) precisa ser compatível com a cosmologia (§2) — como cada andar é fragmento de um universo morto, um Tipo novo deve ter justificativa de qual fragmento/arco ele representa; (3) **nunca concede bônus mecânico próprio** — Tipo é classificação puramente narrativa/organizacional (diferente de Características, que têm custo em NP); (4) deve permitir combinação livre com qualquer Função e Comportamento já existentes.

**9.5.10 Bestiário Base (10 criaturas prontas para jogar)**

| Nome | Tipo | Função | Comportamento | Categoria | Características-chave | Fraqueza |
|---|---|---|---|---|---|---|
| Goblin Saqueador | Humanoide Corrompido | Soldado | Instintiva | Fraca | Sentidos Aprimorados | Foge abaixo de 50% PV |
| Rato Pragado | Besta | Parasita | Instintiva | Fraca | Olfato Apurado | Vulnerável a fogo |
| Esqueleto Guardião | Morto-vivo | Guardião | Instintiva | Comum | Carapaça (óssea) | Vulnerável a dano contundente |
| Cultista Corrompido | Humanoide Corrompido | Soldado | Inteligente | Comum | Ritual menor (habilidade comum) | Vontade baixa (fácil intimidar) |
| Aranha das Profundezas | Besta | Predador | Instintiva | Veterana | Veneno Potente, Camuflagem Natural | Sensível a luz forte/vibração |
| Cavaleiro Corrompido | Morto-vivo | Guardião | Estratégica | Elite | Carapaça, Regeneração | Vulnerável a magia sagrada |
| Bruxa do Pântano | Aberração | Soldado (Controle) | Estratégica | Elite | Habilidade avançada de Controle | Fraca em combate corpo a corpo |
| Golem de Pedra Fragmentado | Constructo | Guardião | Instintiva | Campeã | Carapaça dupla, Imune a Veneno/Medo | Núcleo exposto (ponto fraco) |
| Comandante Espectral | Espírito | Soldado (Comando, §9.7) | Estratégica | Campeã | Voo, Comando supremo (buff de horda) | Dissipa-se com luz sagrada/Selo |
| Dragão do Eclipse | Dracônico | Chefe (Soberano) | Estratégica | Chefe de Arco | Voo, Regeneração, Ataques Múltiplos, sopro (habilidade suprema) | Núcleo exposto após certa fase |

### 9.6 Escala das Criaturas frente ao Grupo
Relação NP-personagem × NP-criatura calibrada por categoria (Comum, Elite, Chefe), com **Fator de Horda** (multiplicador por quantidade de inimigos simultâneos) e regras específicas para Chefes (fases, Ações Lendárias). **Princípio da Superioridade da Dungeon**: a Dungeon deve, por padrão, superar levemente o grupo — nunca ser trivial.


### 9.7 Hordas, Cerco e Conflitos em Massa
Tipos de horda: Enxame, Exército, Invasão, Catástrofe. Tamanho: Pequena, Média, Grande, Massiva. Possui Poder, Pressão, Origem, Comando e Turnos próprios (a horda age em blocos, não criatura a criatura). Objetivos possíveis: Sobrevivência, Defesa, Escolta, Contenção, Retirada. Sistema de Comando em escala (Individual → Tático → Militar → Estratégico), com atributos próprios (Liderança, Estratégia, Conhecimento Militar, Informações) e Moral.

### 9.8 Sistema de Encontros (fórmulas de mesa)
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

Validado em testes práticos (5 goblins fracos → "muito fácil"; 10 soldados corrompidos em terreno favorável → "quase impossível"; dragão sozinho → "chefe de arco" plausível).

### 9.9 Orçamento de Ameaça do Andar (ferramenta central do Mestre)
```
OA = PG × Dificuldade × Fator de Duração
```

- Dificuldade desejada pelo Mestre: Seguro 0,75 | Normal 1,0 | Perigoso 1,25 | Mortal 1,5 | Infernal 2,0 | Apocalíptico 3,0
- Duração: Curto (1-2 encontros)→1 | Normal (3-5)→2 | Longo (6-10)→3 | Extenso→4

O Mestre distribui o OA entre criaturas, armadilhas, eventos, elite e chefe, seguindo proporções sugeridas por tipo de andar (ex.: andar de combate ≈ 70% criaturas/15% ambiente/15% eventos; andar de chefe ≈ 70% chefe/20% mecânicas/10% ambiente).

**Fator de Compressão de Encontro (FCE) — FECHADO por Playtest (§17.10)**: simulações mostraram que usar a Razão de Encontro (R = PE/PG, §9.8) diretamente como multiplicador das estatísticas reais do inimigo cria um "penhasco" (o grupo vence quase sempre ou perde quase sempre, sem gradação real). Ao montar as estatísticas de combate de uma criatura/grupo para atingir uma Razão-alvo R, o Mestre deve aplicar:
```
Multiplicador Real de Atributos/Perícias = 1 + (R − 1) × FCE
```

| Ranking do Grupo | FCE |
|---|---:|
| Bronze–Ferro | 0,40 |
| Aço–Prata | 0,25 |
| Ouro–Mithril | 0,15 |
| Adamante–Lendário | 0,10 |

O FCE diminui conforme o Ranking sobe: grupos de Ranking baixo têm PV reduzido, então a variância dos dados já suaviza a dificuldade sozinha; grupos de Ranking alto têm PV alto, tornando os combates mais determinísticos, exigindo uma compressão mais forte para preservar a gradação entre Favorável/Equilibrado/Desfavorável/Impossível.

**Validação com grupos heterogêneos (FECHADA)**: o FCE foi recalibrado inicialmente com builds "médios" idênticos entre si — reteste feito com grupos realistas (1 Tank, 2 Balanced, 1 DPS, NP individual variando ±20%) confirmou que a tabela se mantém estável, e ficou até mais consistente entre Rankings do que com builds uniformes (Favorável 77-98%, Equilibrado 53-65%, Desfavorável 15-32%, Impossível 0-5%). O FCE está validado para uso direto em mesa.

**Dificuldades separadas**: **Dificuldade de Combate (DC = PE/PG)** mede o quão duro é vencer os inimigos; **Dificuldade de Objetivo (DO)** mede o quão duro é cumprir a missão em si (tempo, ambiente, pressão) — as duas são independentes (um combate fácil pode acontecer dentro de uma missão muito difícil).

### 9.10 Domínio, Ativos Estratégicos e os Quatro Pilares da Progressão
A campanha progride em quatro frentes simultâneas e independentes:

1. **Poder Individual (NP)** — personagens.
2. **Poder Institucional (CG — Capacidade da Guilda)** — a organização.
3. **Recursos Estratégicos (RE)** — bens consumíveis/econômicos.
4. **Ativos Estratégicos (AE)** — conquistas permanentes e não-consumíveis obtidas na Dungeon.

Categorias de Ativos Estratégicos: Infraestrutura (minas, oficinas, torres, laboratórios encontrados), Conhecimento (diários, mapas, fraquezas, rituais), Diplomacia (alianças, sobreviventes resgatados), Artefatos (chaves dimensionais, relíquias), Controle Territorial (pontes, fortes, portais estabilizados).

**Valor Estratégico (VE)** — escala de 1 (benefício local) a 5 (mudança permanente de grande escala), usada pelo Mestre para calibrar risco x recompensa.

Nem todos os Ativos Estratégicos de um andar podem ser obtidos ao mesmo tempo — os jogadores frequentemente escolhem entre objetivos conflitantes, e essas escolhas alteram permanentemente o rumo da Guilda e da campanha.

> **Princípio Fundamental de Progressão**: os personagens evoluem pelo Nível de Poder; a Guilda evolui pela Capacidade da Guilda; a campanha evolui pelos Ativos Estratégicos.

---

## 10. A Guilda

### 10.1 Estrutura Institucional
Conselho (Patronos) → Personagens (agentes de campo) → Hierarquia própria → Prestígio, Influência, Legado, Recursos Institucionais, Especializações, Capacidades Institucionais.

### 10.2 Ficha da Guilda

1. **Identidade** — nome, brasão, divindade patrona, doutrina principal, data de fundação, ranking da Guilda.
2. **Prestígio** — reconhecimento (afeta recrutamento, contratos, influência, eventos).
3. **Influência** — relações políticas, separadas por cidade/facção/outra Guilda/divindade.
4. **Recursos** — Moedas de Pacto, materiais, Fragmentos Dimensionais, artefatos, estoques.
5. **Quartel-General** — lista de instalações e seus níveis.
6. **Funcionários** — artesãos, pesquisadores, trabalhadores, mercenários, administradores.
7. **Conhecimento** — mapas, receitas, pesquisas, inimigos catalogados, chefes derrotados, fraquezas, registros históricos (memória permanente da campanha).
8. **Doutrinas** — doutrinas ativas e seus efeitos.
9. **Logística** — capacidade de armazenamento, nº máximo de trabalhadores, limite de mercenários, expedições simultâneas, alcance de exploração.
10. **Expedições** — registro de cada incursão (data, participantes, objetivo, resultado, perdas, recursos obtidos) — funciona como o "diário" da campanha.
11. **Legado** — maiores feitos históricos (primeiro andar conquistado, primeira Ruptura evitada, etc.), podendo conceder benefícios permanentes.
12. **Capacidade Institucional (CI)** — mede o quanto a organização consegue *sustentar* (nº de Patronos ativos, trabalhadores, instalações, projetos simultâneos, mercenários, tamanho de armazéns). Fórmula fechada em §10.9.
13. **Capacidade de Formação (CF)** — determina o potencial inicial de um personagem recém-recrutado (ver §6.9). Fórmula fechada em §10.9.
14. **Capacidade de Suporte (CS)** — limite estrutural para quantidade de construções que a Guilda pode administrar simultaneamente (ampliada por instalações administrativas/logísticas). Fórmula fechada em §10.9.

### 10.3 Quartel-General e Construções
Filosofia: as construções formam uma **árvore tecnológica real**, não uma lista de compras independente.

Toda construção possui: **Pré-requisitos** (estruturais, institucionais, de conhecimento, de recursos, humanos), **Custos**, **Benefícios Diretos**, **Sinergias** com outras construções.

**Hierarquia por categoria:**

| Nível | Categoria | Exemplos |
|---|---|---|
| I | Fundação | Portão, Dormitório, Armazém, Campo de Treinamento |
| II | Produção | Ferraria, Oficina, Biblioteca, Enfermaria |
| III | Especialização | Laboratório Arcano, Academia Militar, Jardim Alquímico, Oficina de Runas |
| IV | Institucional | Memorial, Centro Logístico, Quartel dos Mercenários, Torre dos Magos |
| V | Monumental | Câmara do Conselho, Cofre Divino, Observatório Dimensional, Santuário do Patrono |

**Princípio da Maturidade Institucional**: os pré-requisitos verificam não apenas a *existência* de uma construção-base, mas o **nível** dela (ex.: uma Universidade Arcana exige Biblioteca III + Laboratório Arcano I, não apenas "ter uma biblioteca"). Nem todas as construções têm o mesmo teto de nível (Dormitório pode parar em V; Biblioteca pode ir até VII; Portão Dimensional pode ter só II, porém ser extremamente caro).

**10.3.1 Lista Oficial de Instalações e Árvore Tecnológica (FECHADA)**

| # | Instalação | Peso | Teto de Nível | Pré-requisito | O que desbloqueia |
|---|---|---:|---|---|---|
| **Fundação** | | **1** | | | |
| 1 | Portão | — | Fixo (I) | Nenhum — existe desde o início | Núcleo da Dungeon; não é construído nem melhorado |
| 2 | Dormitório | 1 | V | Nenhum | Capacidade de personagens/trabalhadores residentes (Nível × 2 vagas) |
| 3 | Armazém | 1 | V | Nenhum | Capacidade de armazenamento (Nível × 50 unidades de recurso) |
| 4 | Campo de Treinamento | 1 | V | Nenhum | Treino básico de perícias de combate; Provações de Corpo e Controle (§6.3) |
| **Produção** | | **2** | | | |
| 5 | Ferraria | 2 | V | Armazém I | Crafting de armas/armaduras (Comum até Raro em Nível I-II; Épico em III+) |
| 6 | Oficina | 2 | V | Armazém I | Crafting geral — ferramentas, itens utilitários (Comum/Incomum) |
| 7 | Biblioteca | 2 | VII | Dormitório I | Pesquisa Menor/Moderada (§11.2); Provações de Intelecto e Percepção |
| 8 | Enfermaria | 2 | V | Dormitório I | Cura avançada, melhora recuperação de PV no Interlúdio; Provação de Vigor |
| **Especialização** | | **3** | | | |
| 9 | Laboratório Arcano | 3 | V | Biblioteca II | Pesquisa Arcana Maior; Provação de Afinidade; Encantamento (junto com Torre dos Magos) |
| 10 | Academia Militar | 3 | V | Campo de Treinamento II + Enfermaria I | Provações de Presença e Vontade; Técnicas Supremas (§6.6.7); treino de mercenários avançados |
| 11 | Jardim Alquímico | 3 | IV | Oficina II | Alquimia avançada — Venenos e Transmutação em nível competitivo |
| 12 | Oficina de Runas | 3 | IV | Ferraria II | Crafting Épico+; Encantamento de armas (junto com Laboratório Arcano) |
| **Institucional** | | **5** | | | |
| 13 | Memorial | 5 | IV | Biblioteca III | Acesso a Cristais de Memória (§6.9); aumenta Capacidade de Formação (CF) |
| 14 | Centro Logístico | 5 | IV | Armazém III + Oficina II | Aumenta Capacidade de Suporte (CS); mais Expedições Secundárias simultâneas |
| 15 | Quartel dos Mercenários | 5 | IV | Academia Militar II | Contratação de Mercenários de Ranking mais alto; aumenta limite de mercenários |
| 16 | Torre dos Magos | 5 | IV | Laboratório Arcano III | Pesquisa Suprema; Rituais avançados; Grimórios raros |
| **Monumental** | | **8** | | | |
| 17 | Câmara do Conselho | 8 | II | Centro Logístico III + Memorial II | Aumenta Capacidade Institucional (CI); mais Patronos ativos/projetos simultâneos |
| 18 | Cofre Divino | 8 | II | Memorial III | Armazenamento seguro de Moedas de Pacto; habilita Crafting Divino (§6.7.4) |
| 19 | Observatório Dimensional | 8 | II | Torre dos Magos III | Detecta/prevê Rupturas com antecedência; reduz a Pressão base de andares explorados |
| 20 | Santuário do Patrono | 8 | I–II | Câmara do Conselho I + Cofre Divino I | Fortalece o Pacto Divino; concede resistência a eventos Divinos negativos (§12) |

**Custo de Construção (FECHADO)** — reaproveita os pesos já fixados na fórmula de CG (§10.8: Fundação=1, Produção=2, Especialização=3, Institucional=5, Monumental=8):
```
Custo em Recursos = Nível da construção × Peso da Categoria × 10
Tempo de Construção = Nível da construção × Peso da Categoria × 3 dias
Trabalhadores mínimos envolvidos = Peso da Categoria
```

| Categoria (Peso) | Nível I | Nível III (se aplicável) |
|---|---|---|
| Fundação (1) | 10 recursos / 3 dias | — |
| Produção (2) | 20 recursos / 6 dias | 60 recursos / 18 dias |
| Especialização (3) | 30 recursos / 9 dias | 90 recursos / 27 dias |
| Institucional (5) | 50 recursos / 15 dias | 150 recursos / 45 dias |
| Monumental (8) | 80 recursos / 24 dias | — (raramente passa de Nível I-II) |

Construções **Monumentais** exigem também **Moedas de Pacto = Nível × 2**, além dos recursos comuns — reforça que são conquistas de campanha, não só dinheiro.

**Nível Tecnológico da Guilda (NTG)**: indicador derivado da infraestrutura + conhecimento acumulado, usado como referência para desbloqueios de ponta (construções monumentais, equipamentos lendários, magias muito complexas, pesquisa com Fragmentos Dimensionais).

Início da campanha: apenas Portão, Dormitório e Campo de Treinamento básico existem — todo o resto é construído pelos jogadores.

### 10.4 Trabalhadores e Mercenários

- **Trabalhadores**: Operários, Artesãos, Pesquisadores, Instrutores, Mercadores, Médicos, Administradores. Cada um possui eficiência, salário, moral e especialidade — fazem tarefas razoavelmente bem, nunca tão bem quanto os jogadores.
- **Mercenários**: NPCs contratados para patrulhar, coletar, minerar, transportar e explorar **apenas andares já conquistados**. Regra fixa: mercenários nunca entram em andares desconhecidos — nunca "jogam pelos jogadores" (Princípio da Fronteira da Exploração).

### 10.5 Departamentos da Guilda
Exploração, Militar, Arcano, Logístico — cada um agrega funções e trabalhadores relacionados, facilitando a administração em campanhas grandes.

### 10.6 Economia da Guilda (FECHADO)
Moeda comum (**Prata**) + recursos materiais + **Moedas de Pacto** (moeda divina especial, obtida na Dungeon, com valor comercial, valor material para crafting divino e valor divino/simbólico). Câmbio-base: **1 Moeda de Pacto = 10 Prata**. Financiamento pode vir de Contribuição Livre dos personagens, Contrato de Guilda ou Investimento de Retorno. Distribuição de recompensas entre Personagem / Guilda / Reserva Estratégica. Manutenção institucional consome recursos continuamente (Regra de Ouro aplicada à Guilda).

**10.6.1 Preços-base**:

| Item/Serviço | Preço-base |
|---|---:|
| Ração de comida (1 dia) | 1 Prata |
| Estadia simples (1 noite) | 2 Prata |
| Salário diário — Operário | 3 Prata |
| Salário diário — Artesão/Pesquisador | 8 Prata |
| Manutenção de construção | Peso da Categoria × 1 Prata/dia |

**Salário diário de Mercenário** (por Ranking):

| Ranking | Salário/dia |
|---|---:|
| Bronze | 10 |
| Ferro | 18 |
| Aço | 30 |
| Prata | 50 |
| Ouro | 80 |
| Mithril | 120 |
| Adamante | 170 |
| Lendário | 250 |

**10.6.2 Geração de Renda**: (1) Recompensas de Expedição — a fatia "Guilda" da distribuição já fechada; (2) Comércio — Doutrina Comercial concede +10% em venda de materiais excedentes; (3) Produção de Trabalhadores — Operários geram ~2 Prata/dia de valor, Artesãos/Pesquisadores geram itens/pesquisa em vez de Prata direta; (4) Expedições Secundárias: `Rendimento = NP do mercenário × 0,5 Prata por expedição secundária bem-sucedida`; (5) Legado — feitos históricos podem conceder bônus permanentes de renda (ex.: +5% numa fonte específica).

**10.6.3 Manutenção**:
```
Manutenção Diária = Σ (Nível × Peso da Categoria × 1 Prata, por construção) + Σ (salários diários de Trabalhadores/Mercenários ativos)
```
Se não paga: construções entram em **Negligência** (metade do benefício até quitar) e Trabalhadores perdem Moral (eficiência reduzida) — nunca trava o jogo, é consequência (Princípio dos Fracassos como Consequência).

**10.6.4 Inflação — Índice de Preços por Estágio da Guilda** (reaproveita os estágios já fixados na CG, §10.8):
```
Preço Ajustado = Preço-base × Índice de Preços do Estágio atual
```

| Estágio da Guilda | Índice de Preços |
|---|---:|
| Fundação | ×1,0 |
| Guilda Menor | ×1,2 |
| Guilda Regional | ×1,5 |
| Guilda Reconhecida | ×1,8 |
| Guilda Maior | ×2,2 |
| Guilda Renomada | ×2,6 |
| Guilda Lendária | ×3,2 |
| Guilda Divina | ×4,0 |

Isso garante que dinheiro nunca "resolve o jogo" nos estágios avançados — o custo de operar cresce junto com a ambição da Guilda, mantendo a Regra de Ouro ativa em toda a campanha.

### 10.7 Doutrinas da Guilda (FECHADO)
Especialização permanente da filosofia operacional da organização (funciona como uma árvore de especialização institucional). Cada campanha desenvolve sua própria combinação, dando identidade única a cada Guilda mesmo usando o mesmo sistema base.

**Regra de escolha**: a Guilda começa com **até 2 Doutrinas ativas**. Desbloqueia **+1 Doutrina extra por Nível da Câmara do Conselho** (§10.3.1), até um máximo de **4 Doutrinas simultâneas**. Trocar uma Doutrina ativa por outra exige um projeto de Interlúdio (tempo = 20 dias, Dificuldade Difícil num Teste de Liderança/Administração) — não é uma escolha trivial.

| Doutrina | Bônus |
|---|---|
| **Militar** | +10% em Ataque/Dano de Mercenários e NPCs de combate da Guilda; -1 dia no tempo de Provações de Corpo/Controle/Presença/Vontade |
| **Acadêmica** | +15% de velocidade em projetos de Pesquisa (reduz tempo); -10% de custo em Recursos para Provações de Intelecto/Percepção |
| **Comercial** | +10% em toda venda de materiais excedentes; reduz o Índice de Preços de Inflação em 1 estágio para compras da própria Guilda |
| **Exploração** | +15% de chance de sucesso em Expedições Secundárias; -10% no consumo de Comida/Água/Tochas do grupo principal |
| **Arcana** | -1 PA adicional em conjuração para todos os personagens da Guilda (empilha com Grau de Controle Mágico); -25% no tempo de Provação de Afinidade |
| **Engenharia** | -15% no Tempo de Construção/Melhoria de instalações; +10% de chance de Grande Sucesso em Crafting |
| **Logística** | +20% na Capacidade de Suporte (CS); -10% na Manutenção Diária |
| **Diplomática** | Facções recém-descobertas começam com +15 de Reputação; ganhos de Reputação de peso Moderado contam como Maior (perdas continuam normais) |

### 10.8 Capacidade da Guilda (CG) — fórmula FECHADA

**Decisão de arquitetura**: a CG é **desacoplada** do cálculo de ameaça de combate. Ela nunca é somada ao Poder do Grupo (PG) nem entra no Orçamento de Ameaça (OA) — isso evitaria contar a força da Guilda duas vezes (uma via equipamentos/formação já embutidos no NP dos personagens, outra somada de novo na conta de perigo). A CG é um valor **puramente institucional**, que mede o que a Guilda consegue *sustentar* (trabalhadores, mercenários, construções simultâneas, Capacidade de Formação de novos recrutas, quais Ativos Estratégicos avançados ela consegue manter). Isso preserva os **Quatro Pilares da Progressão** (§9.10) como trilhas realmente independentes: NP (personagem), CG (Guilda), RE (recursos), AE (ativos estratégicos).

```
CG = Infraestrutura + Pesquisa + Logística + Recursos
```
onde:

- **Infraestrutura** = Σ (nível de cada construção × peso da categoria: Fundação=1, Produção=2, Especialização=3, Institucional=5, Monumental=8)
- **Pesquisa** = pontos acumulados em projetos concluídos
- **Logística** = Capacidade de Suporte (CS) + nº de trabalhadores qualificados × 2
- **Recursos** = reservas de Moedas de Pacto + materiais estratégicos (valor convertido)

**Tabela oficial de CG por estágio da Guilda** (marco a cada 5 andares conquistados, acompanhando os Andares Especiais):

| Estágio da Guilda | Andares conquistados | Infraestrutura | Pesquisa | Logística | Recursos | **CG** |
|---|---:|---:|---:|---:|---:|---:|
| Fundação | 0 | 5 | 0 | 5 | 5 | **15** |
| Guilda Menor | 5 | 20 | 10 | 15 | 15 | **60** |
| Guilda Regional | 10 | 45 | 25 | 30 | 30 | **130** |
| Guilda Reconhecida | 15 | 80 | 45 | 50 | 50 | **225** |
| Guilda Maior | 20 | 125 | 70 | 75 | 75 | **345** |
| Guilda Renomada | 25 | 180 | 100 | 105 | 105 | **490** |
| Guilda Lendária | 30 | 245 | 135 | 140 | 140 | **660** |
| Guilda Divina | 35+ | 320 | 175 | 180 | 180 | **855** |

A curva evolui no mesmo ritmo dos marcos de 5 andares, reforçando mecanicamente que Guilda e Dungeon avançam juntas.

### 10.9 Capacidades Derivadas — CI, CF, CS (FECHADO)

Diferente da CG (institucional, isolada do combate), estas três capacidades **travam limites concretos de jogo**, cada uma amarrada a instalações específicas da árvore tecnológica (§10.3.1):

```
CS (Capacidade de Suporte) = 5 + (Nível do Centro Logístico × 2) + (Nível do Armazém × 1)

CI (Capacidade Institucional) = 3 + (Nível da Câmara do Conselho × 4) + (Nível do Centro Logístico × 1)

CF (Capacidade de Formação) = 10 + (Nível do Memorial × 3) + (Nível da Biblioteca × 1) + (Nível do Campo de Treinamento × 1)
```

**Progressão por Estágio da Guilda** (mesmos 8 estágios de §10.8):

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

**CS — o que trava**: número máximo de construções que a Guilda consegue manter **ativas/administradas** ao mesmo tempo. Como a árvore tecnológica tem 19 instalações construíveis e o teto de CS é 16, mesmo uma Guilda Divina precisa escolher quais ficam ativas — as excedentes ficam **Inativas** (sem benefício) até o jogador desativar outra ou aumentar CS. Reforça a Regra de Ouro até no topo do jogo.

**CI — o que trava**:
- Patronos ativos simultâneos = CI ÷ 3 (arredondado para cima, mínimo 1)
- Projetos de Interlúdio simultâneos (pesquisa/construção/etc. em paralelo) = CI ÷ 2
- Trabalhadores contratáveis no total = CI × 3

**CF — o que concede** (bônus de Formação na criação de um personagem novo, §6.9):

| CF | Bônus de Formação |
|---|---|
| 10–17 | Nenhum (Recruta padrão) |
| 18–22 | +5 pontos de perícia extra |
| 23–27 | +10 pontos de perícia extra; equipamento inicial pode ser Incomum |
| 28–32 | +15 pontos de perícia extra; equipamento Incomum garantido; +1 Talento menor extra |
| 33+ | +20 pontos de perícia extra; equipamento Raro possível; 1 perícia inicial já nasce em Grau Básico |

---

## 11. Interlúdio (o "segundo coração" do sistema)

### 11.1 Duas linhas temporais

- **Tempo da Dungeon**: usado durante a sessão (ex.: uma expedição pode durar 10 dias "internos").
- **Tempo do Mundo/Quartel**: passa em semanas entre sessões, com uma **dilatação temporal fixa** para simplificar contas (ex.: 10 dias na Dungeon ↔ apenas 1 dia no Quartel), eliminando o problema clássico de "meu personagem ficou preso na Dungeon porque eu faltei".
- Cada personagem recebe um número de **ações de interlúdio** proporcional ao tempo disponível desde sua última expedição. As ações são declaradas pelo jogador antes da sessão seguinte e resolvidas pelo Mestre.

### 11.2 Subsistemas do Interlúdio

1. **Treinamento** (§6.4) — progresso garantido, fixo por dia, modificado por instalações/instrutores/equipamentos/conhecimento da Guilda; a curva de aprendizado se aplica normalmente.
2. **Pesquisa** — tipos: Arcana, Biológica, Tecnológica, Dimensional, Histórica, Militar. Fluxo: **Descobrir → Pesquisar → Dominar → Aplicar**. Projetos de pesquisa têm progresso próprio, podem ser coletivos (vários pesquisadores), podem gerar descobertas parciais e podem ser interrompidos. Instalações relevantes: Biblioteca, Laboratório Arcano, Oficina, Memorial.

**Custo de Pesquisa (FECHADO)** — reaproveita os tiers de Complexidade já fixados para Magia (§6.6.3), estendidos para qualquer tipo de pesquisa:

| Complexidade | Tempo | Custo em Recursos | Instalação mínima |
|---|---:|---:|---|
| Menor | 5 dias | 10 | Biblioteca/Oficina básica |
| Moderada | 10 dias | 25 | Biblioteca II+ |
| Maior | 20 dias | 50 | Laboratório correspondente |
| Suprema | 40+ dias | 100+ | Instalação avançada + 5 Moedas de Pacto |

Pesquisas coletivas (múltiplos pesquisadores) dividem o tempo proporcionalmente, mas nunca abaixo de 50% do tempo-base.

3. **Produção e Criação (Crafting)** — categorias: Forja, Alquimia, Encantamento, Engenharia, Artefatos. Artesãos possuem especialização, grau de domínio e eficiência. Receitas podem ser Conhecidas, Projetos Descobertos ou Receitas Únicas. Qualidade dos itens: Comum, Superior, Raro, Épico, Lendário, Divino.
4. **Administração da Guilda** — economia institucional, distribuição de recompensas, gestão de trabalhadores/mercenários, departamentos, manutenção, eventos administrativos, Prestígio.
5. **Expedições Secundárias** (mercenários) — coleta, recuperação, patrulha, transporte, apoio, pesquisa de campo; sempre limitadas a andares já conquistados (Princípio da Fronteira); geram relatórios, risco de baixas e rendimento variável conforme especialização da equipe.

### 11.3 Regra de Origem dos Modificadores aplicada
Toda instalação, instrutor, equipamento e conhecimento institucional que bonifica uma atividade de interlúdio deve ter origem claramente rastreável (nada de "+2 porque sim").

---

## 12. Eventos Dinâmicos e Tensão

O mundo não fica parado durante a ausência dos jogadores. Categorias de eventos: Pessoais, da Guilda, da Dungeon, Mundiais, Divinos. Geração pode ser Natural, por Consequência de ações passadas, ou Narrativa (decisão do Mestre).

**Sistema de Tensão** — quatro indicadores acumulam valor ao longo do tempo e aumentam a chance/intensidade de eventos:

- Tensão da Guilda
- Tensão da Dungeon
- Tensão Mundial
- Tensão Divina

Eventos podem ser Positivos, Negativos ou Mistos, podem se encadear, e uma **Calamidade** (nome oficial: **Ruptura**) é o evento máximo de tensão da Dungeon — quando um Fragmento rompe a contenção e invade o Mundo Central. Existe registro histórico permanente de eventos importantes.

---

## 13. Facções (FECHADO)

Conceito: facções existem dentro da Dungeon (Goblins, Cultistas, Mortos-vivos, Mercadores, Bestas, Aventureiros rivais etc.), controlam território, possuem objetivos, reagem às escolhas dos jogadores, fazem alianças e entram em guerra entre si — mas sua influência fica restrita apenas aos andares da Dungeon (não ao mundo político externo), para não expandir demais o escopo do sistema.

**13.1 Reputação de Facção**: escala de **-100 a +100**, dividida em 5 níveis:

| Reputação | Nível | Comportamento padrão |
|---|---|---|
| -100 a -51 | Hostil | Ataca o grupo sempre que encontra; fecha rotas; pode colocar recompensa pela cabeça do grupo |
| -50 a -11 | Desconfiada | Preços ruins, sonega informação, exige provas antes de ajudar |
| -10 a +10 | Neutra | Comportamento padrão, sem bônus/penalidade |
| +11 a +50 | Amistosa | Acesso a comércio/informação, cede passagem segura, oferece dicas |
| +51 a +100 | Aliada | Luta ao lado do grupo, compartilha território/recursos, desbloqueia Ativos Estratégicos exclusivos |

**13.2 Tabela de Consequências de Escolhas**:

| Peso da Escolha | Variação |
|---|---:|
| Menor (favor pequeno, gesto de boa vontade ou ofensa leve) | ±5 |
| Moderada (cumprir/quebrar um acordo, ajudar/atacar um membro) | ±15 |
| Maior (decidir o destino da facção num conflito, traição, resgate de líder) | ±30 |

**13.3 Como uma Facção Muda um Andar na Prática** — conecta Facções direto ao Sistema de Encontros (§9.8):

- **Território sob controle da facção**: aplica o multiplicador de Terreno da fórmula de PE — Hostil defendendo o próprio território = Favorável (×1,25) ou Extremo (×1,5) se for o covil principal; Reputação Neutra = Terreno Neutro (×1).
- **Facção Aliada em uma área**: encontros hostis daquela área recebem reforço — trate como `PG × 1,1` só para aquele encontro.
- **Facção Hostil ativa**: seus encontros usam Objetivo "Missão crítica" (×2) com mais frequência — ela luta para impedir o grupo, não só para sobreviver.
- **Acesso a Ativos Estratégicos**: alguns só ficam disponíveis com Reputação Amistosa+ (a facção mostra um caminho oculto) ou exigem conflito direto se Hostil.
- **Efeito em Pressão**: manter uma facção Aliada numa área reduz em **-5** a Pressão gerada por eventos naquela área (§9.2), empilhável com as demais fontes.

**13.4 Registro**: a Reputação de cada facção relevante entra na Ficha da Guilda (§10.2, item 3 "Influência") e no Registro da Campanha (§14) — histórico de decisões que moveram o número, para consistência narrativa entre sessões.

---

## 14. Registro da Campanha

Funciona como o "save game" da campanha — registra automaticamente andares conquistados, mortes, personagens vivos, recursos, construções, trabalhadores, pesquisas, relações com facções, memórias disponíveis, eventos importantes, doutrinas. Base para o histórico e para a Ficha da Guilda (§10.2, item 10).

---

## 15. Apêndice — Fórmulas Consolidadas

```
Modificador de Atributo = Atributo − 2

NP (personagem) = (Atributos + Perícias) + (Talentos + Habilidades) + Equipamentos

PG (Poder do Grupo) = Σ NP(personagens) × Fator de Sinergia

PE (Poder do Encontro) = Σ NP(criaturas) × Quantidade × Inteligência × Terreno × Objetivo

R (classificação do encontro) = PE / PG

DC (Dificuldade de Combate) = PE / PG      [mesma fórmula que R, aplicada ao combate isolado]
DO (Dificuldade de Objetivo) = calculada separadamente por tempo/ambiente/pressão/informação

OA (Orçamento de Ameaça do andar) = PG × Dificuldade do andar × Fator de Duração

CG (Capacidade da Guilda) [FECHADA — institucional, desacoplada do combate] = Infraestrutura + Pesquisa + Logística + Recursos
CS (Capacidade de Suporte) = 5 + (Nível do Centro Logístico × 2) + (Nível do Armazém × 1)
CI (Capacidade Institucional) = 3 + (Nível da Câmara do Conselho × 4) + (Nível do Centro Logístico × 1)
CF (Capacidade de Formação) = 10 + (Nível do Memorial × 3) + (Nível da Biblioteca × 1) + (Nível do Campo de Treinamento × 1)
```

---

## 16. Glossário Rápido

- **Patrono** — jogador no papel administrativo (Conselho da Guilda).
- **Personagem** — aventureiro descartável que explora a Dungeon.
- **Ruptura** — evento de colapso dimensional quando um andar escapa da contenção.
- **Cristal de Memória** — registro póstumo das memórias de um personagem morto.
- **NP** — Nível de Poder (individual).
- **CG** — Capacidade da Guilda (institucional).
- **CI** — Capacidade Institucional (o que a Guilda consegue sustentar).
- **CF** — Capacidade de Formação (nível inicial de um novo recruta).
- **CS** — Capacidade de Suporte (limite de construções administráveis).
- **NTG** — Nível Tecnológico da Guilda.
- **AE** — Ativo Estratégico (conquista permanente, não consumível).
- **RE** — Recurso Estratégico (consumível).
- **VE** — Valor Estratégico (importância de um Ativo, escala 1–5).
- **OA** — Orçamento de Ameaça (ferramenta de construção de andar pelo Mestre).
- **PG / PE** — Poder do Grupo / Poder do Encontro.

---

## 17. HISTÓRICO DE FECHAMENTO DO SISTEMA

> **Status atual: 100% fechado.** Todas as pendências identificadas ao longo do desenvolvimento (fórmulas, criação de personagem, combate, exploração, equipamentos, magia/técnicas, criaturas, balanceamento/playtest, custo de atributos/pesquisa/construção/fabricação, economia, pressão, facções e ferramentas de construção de conteúdo) foram resolvidas e validadas. Esta seção agora funciona como **registro histórico** de como cada sistema chegou ao estado final — útil para entender o raciocínio por trás de cada número, caso algo precise ser revisitado no futuro.

### 17.1 Fórmulas — **FECHADA POR COMPLETO**

- ~~Fórmula definitiva do Nível de Poder~~ — **FECHADA** (§6.8): faixas oficiais de NP por Ranking, validadas em simulação nos 8 Rankings.
- ~~Fórmula definitiva da Capacidade da Guilda (CG)~~ — **FECHADA** (§10.8): CG desacoplada do cálculo de combate, com tabela oficial por estágio da Guilda.
- ~~Custo de evolução de atributos~~ — **FECHADA** (§6.3): sistema de Provação de Atributo, com tempo/custo escalando por Grau e Provações temáticas por atributo.
- ~~Custo de pesquisas, construções e fabricação~~ — **FECHADA** (§10.3, §11.2, §6.7.4): tabelas de tempo/recursos/Moedas de Pacto para as três, reaproveitando as escalas já fixadas de CG, Complexidade Mágica e Raridade.
- ~~Economia completa~~ — **FECHADA** (§10.6): Prata/Moeda de Pacto com câmbio-base, preços-base, salários de mercenário por Ranking, geração de renda, manutenção diária, e Índice de Preços por estágio da Guilda (inflação).
- ~~Cálculo final da Pressão da Dungeon~~ — **FECHADA** (§9.2): contador 0-100 por andar, com limiares e multiplicadores de PE, validado em teste de ponta a ponta (§17.10).

### 17.2 Criação de Personagem — **FECHADA POR COMPLETO**
Todos os itens abaixo foram resolvidos: distribuição de atributos (§6.3, Compra Livre), Origens (§6.1.1/§6.1.2), Históricos (§6.1.3/§6.1.4), Aptidões (§6.1.5), Talento Inicial (§6.1.6), Linhagens/Raças (§6.1.7), Dívida de Formação (§6.2) e o procedimento final passo a passo (§6.1). Nenhuma pendência restante neste sistema.

### 17.3 Combate — **FECHADA POR COMPLETO**
Todos os itens abaixo foram resolvidos em §7: Movimento Híbrido (Grid/Hex para pequena escala, Zonas para larga escala), Iniciativa, Alcance/Cobertura, Ataques de Oportunidade (cobertos pela Reação), fórmulas finais de Ataque/Dano/Armadura, Defesa Híbrida (Passiva/Ativa), Pontos de Vida e recuperação, Condições (lista fechada), e procedimento de morte (Agonizante → morte instantânea em novo dano). Nenhuma pendência restante neste sistema.

### 17.4 Exploração — **FECHADA POR COMPLETO**
Todos os itens abaixo foram resolvidos em §8: Turno de Exploração (10 min), Visão/Iluminação, Navegação e Mapas, Armadilhas (detecção/desarme/dano), Exploração em Grupo (papéis e subgrupos), Acampamento/Descanso, e Consumo de Recursos (comida, água, tochas, corda, munição, capacidade de carga). Nenhuma pendência restante neste sistema.

### 17.5 Equipamentos e Crafting — **FECHADA POR COMPLETO**
Todos os itens abaixo foram resolvidos em §6.7: Raridade (tabela de propriedades máx./bônus base/NP), Categorias, 20 Propriedades e Encantamentos + manual homebrew, processo de Criação (Crafting), Melhoria/Modificação/Reconstrução, Durabilidade (Golpes de Desgaste), e o Guia Completo de Criação de Itens (Caminho Mestre / Caminho Jogador). Nenhuma pendência restante neste sistema.

### 17.6 Magia — **FECHADA POR COMPLETO**
Todos os itens abaixo foram resolvidos em §6.6: 8 Escolas de Magia oficiais, estrutura mecânica de uma magia (custo/alcance/área/duração/teste/efeito), custo e redução por Grau de Controle Mágico, Interrupção, criação de novas magias via Pesquisa Arcana, Encantamento de Itens, Rituais, e uma lista de 24 magias de exemplo (1 por Escola, evoluindo Menor→Moderada→Maior). Nenhuma pendência restante neste sistema.

### 17.7 Técnicas Marciais — **FECHADA POR COMPLETO**
Todos os itens abaixo foram resolvidos em §6.6.7/§6.6.8: árvore de técnicas por estilo (Postura/Técnica/Reação/Suprema), requisitos formais por categoria, processo de criação de técnicas novas via Interlúdio, e uma lista de técnicas de exemplo para 3 estilos (Espadas, Combate Corporal, Arcos), com evolução Técnica I→II. Nenhuma pendência restante neste sistema.

### 17.8 Criaturas — **FECHADA POR COMPLETO**
Todos os itens abaixo foram resolvidos em §9.5: 8 Tipos oficiais, Função na Dungeon, Comportamento/IA com regras de mesa concretas, tabela de Características Naturais com custo em NP, fórmula de NP de criatura, Categorias mapeadas por faixa de NP/Ranking, Ficha Simplificada de Criatura, Manual de Criação de Criaturas + checklist de balanceamento, Manual de Tipos Homebrew, e um Bestiário Base de 10 criaturas prontas para jogar. Nenhuma pendência restante neste sistema.

### 17.9 Facções — **FECHADA POR COMPLETO**
Sistema fechado em §13: Reputação numérica (-100 a +100, 5 níveis), Tabela de Consequências de Escolhas, e conexão direta com o Sistema de Encontros (Terreno/Objetivo/reforço de PG) e com a Pressão da Dungeon. Nenhuma pendência restante neste sistema.

### 17.10 Balanceamento Geral e Playtest — Resultado da Simulação

Rodada uma simulação Monte Carlo (2d10, grupos de 4 personagens, 300-500 combates por célula) cruzando os 8 Rankings × 4 condições solicitadas (Favorável/Equilibrado/Desfavorável/Impossível). O processo revelou e corrigiu dois bugs críticos de balanceamento (ver erratas em §7.5 e §7.4) e calibrou o Fator de Compressão de Encontro (§9.9). Tabela final validada (com as correções + FCE por Ranking aplicados):

| Ranking | Favorável | Equilibrado | Desfavorável | Impossível |
|---|---:|---:|---:|---:|
| Bronze | 93% | 50% | 15% | 0% |
| Ferro | 96% | 52% | 15% | 0% |
| Aço | 99% | 56% | 16% | 0% |
| Prata | 93% | 53% | 15% | 0% |
| Ouro | 94% | 53% | 11% | 0% |
| Mithril | 94% | 50% | 19% | 0% |
| Adamante | 98% | 54% | 25% | 0% |
| Lendário | 92% | 40% | 30% | 0% |

Leitura: Favorável e Impossível se comportam de forma consistente em todos os 8 Rankings (quase sempre vitória / quase nunca vitória). Equilibrado ficou estável entre 40-56% ("sucesso se não cometer muitos erros"). Desfavorável ficou entre 11-30% ("precisa de resultados excelentes") — ainda com alguma variação entre Rankings que pode se beneficiar de mais uma rodada de ajuste fino do FCE em uma sessão real de mesa, mas dentro de uma faixa aceitável para uso imediato.

**Pendências remanescentes de balanceamento** (ajuste fino, não bloqueiam o uso do sistema):

- Guia de Balanceamento e Construção de Conteúdo consolidado (criar criaturas, andares, arcos e campanhas inteiras) — parcialmente coberto pelo Orçamento de Ameaça (§9.9) e pelo FCE (§9.9), mas ainda sem um "manual do mestre" único.
- Testar o sistema completo em um andar real do primeiro arco ("A Vila dos Mil Monstros") de ponta a ponta, incluindo pressão, recompensas e ativos estratégicos.
- Validar o FCE em jogo real (a simulação usa builds agregados/médios; personagens reais com builds especializados podem se comportar de forma diferente).

### 17.11 Exploração de conteúdo / ferramentas do Mestre — **FECHADA POR COMPLETO**

- ~~Guia de Balanceamento e Construção de Conteúdo~~ — **FECHADA** (Manual do Mestre §6.6): Guia de Construção de Conteúdo em 5 níveis (Criatura → Encontro → Andar → Arco → Campanha), amarrando Orçamento de Ameaça, FCE, Pressão e Facções num fluxo de trabalho único.
- ~~Testar o sistema completo em um andar real do primeiro arco~~ — **FEITO**: Arco 1 "A Vila dos Mil Monstros", Andar 1 "O Silêncio Antes da Horda", montado de ponta a ponta (PG=315, OA=630, dois encontros classificados via §9.8, Ativo Estratégico com VE atribuído, Pressão numérica aplicada em tempo real). Resultado: o sistema gerou combate equilibrado no início e difícil no clímax, exploração com informação de valor mecânico real, uma escolha permanente sem opção "errada", e validou cruzado com os números do Playtest (§17.10). O teste revelou a lacuna na Pressão numérica, que foi fechada em §9.2 como consequência direta.
- ~~Validar o FCE com builds reais/heterogêneos~~ — **FEITO**: reteste com grupos Tank/DPS/Balanced e NP ±20% confirmou que o FCE se mantém estável (e até mais consistente) fora do cenário de builds "médios" idênticos.

### 17.12 Sugestão de ordem de trabalho (retomando o próprio plano em curso)

1. ~~Fechar as fórmulas fundamentais (NP e CG definitivos) com simulação de vários personagens/Guildas hipotéticos.~~ — **CONCLUÍDO**.
2. ~~Fechar a criação de personagem como procedimento jogável.~~ — **CONCLUÍDO**.
3. ~~Fechar Combate.~~ — **CONCLUÍDO**.
4. ~~Fechar Exploração.~~ — **CONCLUÍDO**.
5. ~~Fechar Equipamentos e Crafting.~~ — **CONCLUÍDO**.
6. ~~Fechar Magia e Técnicas Marciais.~~ — **CONCLUÍDO**.
7. ~~Fechar Criaturas (bestiário base).~~ — **CONCLUÍDO**.
8. ~~Balanceamento geral e playtest.~~ — **CONCLUÍDO** (§17.10).

---

*Fim do documento. Este GDD reflete o estado consolidado de todas as decisões tomadas até o momento; qualquer alteração futura deve ser registrada aqui para evitar contradições entre módulos.*
