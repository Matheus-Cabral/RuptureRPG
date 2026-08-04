# MANUAL DO JOGADOR
### RPG Dungeon Crawler Hardcore

> Este manual reúne tudo que você precisa para criar um personagem, jogar uma expedição e participar da vida da sua Guilda. Sempre que uma regra permitir conteúdo **homebrew** (criado por você ou pelo Mestre), isso está marcado — **converse com seu Mestre antes de usar qualquer coisa homebrew em jogo**, já que é ele quem aprova o que entra na campanha.

---

## Sumário

1. [O Mundo em Poucas Palavras](#1-o-mundo-em-poucas-palavras)
2. [Como o Sistema Funciona](#2-como-o-sistema-funciona)
3. [Criação de Personagem](#3-criação-de-personagem)
   - 3.1 [Origem](#31-origem) · 3.2 [Histórico](#32-histórico) · 3.3 [Linhagem](#33-linhagem-raçaespécie) · 3.4 [Aptidões Iniciais](#34-aptidões-iniciais)
   - 3.5 [Atributos](#35-atributos) (+ Provação) · 3.6 [Perícias](#36-perícias) · 3.7 [Talento Inicial](#37-talento-inicial) · 3.8 [Magias/Técnicas Iniciais](#38-magias-e-técnicas-iniciais)
4. [Combate](#4-combate) — Movimento, Iniciativa, PA, Defesa, Ataque/Dano, PV, Condições, Morte
5. [Exploração](#5-exploração) — Turnos, Visão, Navegação, Armadilhas, Descanso, Recursos
6. [Magia e Técnicas Marciais](#6-magia-e-técnicas-marciais) — Escolas, Exemplos, Magia Livre, Técnicas
7. [Equipamentos e Crafting](#7-equipamentos-e-crafting) — Raridade, Propriedades, Criação, Durabilidade
8. [A Guilda (visão do Patrono)](#8-a-guilda-visão-do-patrono) — Instalações, Trabalhadores, Economia
9. [Interlúdio — O Tempo Entre Expedições](#9-interlúdio--o-tempo-entre-expedições)
10. [Glossário](#10-glossário)

---

## 1. O Mundo em Poucas Palavras

No passado, diversas divindades criaram universos independentes. Muitos foram destruídos por guerras, cataclismos ou pelo fim natural de seu ciclo — mas um universo destruído nunca desaparece por completo: ele deixa um **Fragmento Dimensional**, que tende a colidir com outras realidades.

Para conter isso, as divindades construíram um **Mundo Central** com **Portões** — estruturas que aprisionam cada Fragmento. Cada Portão contém uma **Dungeon**, e cada andar dela é um pedaço preservado de um universo morto (por isso andares podem ter biomas, tecnologias e criaturas completamente diferentes entre si).

Os fragmentos acumulam pressão constante para retornar ao mundo real. Explorar a Dungeon reduz essa pressão. Se a estabilidade se perde, ocorre uma **Ruptura** — parte da Dungeon invade o Mundo Central.

**Guildas** são instituições permanentes responsáveis por manter a estabilidade de um Portão. Cada jogador, no papel administrativo, é um **Patrono** — fez um pacto direto com uma divindade que lhe concede autoridade sobre a Guilda, em troca da responsabilidade permanente pela estabilidade do Portão. O Patrono jamais pode atravessar o Portão; se morre sem sucessor legítimo, a Guilda perde autoridade e a estabilidade colapsa.

### Os Três Papéis

- **Jogador** — você, sentado à mesa.
- **Patrono** — sua representação permanente no Conselho da Guilda; administra a Guilda no Interlúdio; nunca entra na Dungeon.
- **Personagem** — o aventureiro que você recruta para explorar a Dungeon. É descartável do ponto de vista institucional — você não "é" o personagem, você é um Patrono que envia sucessivos personagens para cumprir o pacto.

```
Jogador → Patrono → Guilda → Portão → Dungeon → Personagens
```

---

## 2. Como o Sistema Funciona

- **Dados**: o sistema usa **2d10** (dois dados de dez lados, somados) para todos os testes.
- **Testes Opostos**: quando há oposição direta (combate, furtividade x percepção). Vence quem tira o maior resultado.
- **Testes Absolutos**: contra uma dificuldade fixa (percepção, pesquisa, fabricação, escalada). Sucesso quando o resultado ≥ dificuldade.

| Dificuldade | Valor |
|---|---:|
| Trivial | 8 |
| Fácil | 12 |
| Moderada | 16 |
| Difícil | 20 |
| Muito Difícil | 24 |
| Heroica | 28 |
| Lendária | 32 |

A **Margem de Sucesso** (diferença entre seu resultado e a dificuldade) importa: quanto maior a margem, melhor o efeito (Sucesso → Grande Sucesso → Sucesso Extraordinário). O oposto vale para falhas (Falha → Falha Crítica).

**Rankings**: sua patente evolui em degraus — Bronze → Ferro → Aço → Prata → Ouro → Mithril → Adamante → Lendário. Você avança por **conquistas** (alcançar certo andar, feitos importantes), nunca por acúmulo simples de pontos de experiência.

**Nível de Poder (NP)**: um número calculado nos bastidores para balancear o jogo. Você pode consultá-lo, mas nunca o usa diretamente na mesa.

---

## 3. Criação de Personagem

### Passo a Passo
```

1. Origem            → +25 pts de perícia (15+10), benefício, equipamento, gancho narrativo
2. Histórico          → benefício + complicação (sem perícia/atributo)
3. Linhagem           → ajuste de teto em 2 atributos + 1 traço racial
4. Aptidões (2)       → facilidade de aprendizado + instinto natural
5. Atributos          → 20 pontos, compra livre, mín 1 / máx 5 (ou 6/4 se ajustado pela Linhagem)
6. Perícias Iniciais  → as da Origem já entram; distribua eventuais pontos extras
7. Talento Inicial (1)
8. Equipamentos       → os da Origem + o que a Guilda fornecer
9. Nível de Poder     → deve cair na faixa Bronze (40–70)
10. Registro da Guilda → nome, nº de registro, Ranking (Bronze), Dívida de Formação, data de ingresso
```

Todos os personagens começam como **Recrutas** da Guilda e com uma **Dívida de Formação** — um valor fixo equivalente ao custo do equipamento básico, treinamento e alojamento que a Guilda te forneceu. Essa dívida é abatida automaticamente da sua fatia de recompensas a cada expedição, até quitar — nunca trava sua evolução.

### 3.1 Origem
Seu passado social/profissional. Toda Origem concede: 1 benefício mecânico leve, **1 perícia primária (15 pontos) + 1 perícia secundária (10 pontos)**, 0-2 equipamentos iniciais (nunca acima de raridade Incomum), e um gancho narrativo.

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

> 🛠️ **Homebrew**: quer uma Origem que não está na lista? **Consulte seu Mestre.** Toda Origem nova precisa de: 1 benefício leve, exatamente 15+10 pontos de perícia, 0-2 equipamentos simples e um gancho narrativo — seu Mestre vai validar se ela deixa seu personagem *diferente*, não *melhor*, do que as 20 oficiais.

### 3.2 Histórico
Um evento pontual que marcou seu personagem — **nunca concede perícia ou atributo**, apenas um benefício situacional e uma complicação de peso equivalente.

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

> 🛠️ **Homebrew**: quer criar um Histórico próprio? **Consulte seu Mestre.** Ele vai checar se o benefício e a complicação têm peso equivalente, e se a complicação é algo que pode voltar na campanha (senão, não é uma complicação válida — é só enfeite).

### 3.3 Linhagem (Raça/Espécie)
Sua ascendência ajusta o **teto** de dois atributos (nunca os pontos gastos) e concede 1 traço inato.

| Linhagem | Ajuste Racial | Traço Racial |
|---|---|---|
| Humano | Nenhum (todos os atributos no teto padrão 5) | Adaptável: pode trocar 1 Aptidão escolhida na criação, 1x na campanha |
| Anão | +1 máx. Vigor / −1 máx. Controle | Resistência a venenos e doenças |
| Elfo | +1 máx. Percepção / −1 máx. Corpo | Visão em baixa luminosidade |
| Meio-Orc | +1 máx. Corpo / −1 máx. Intelecto | 1x por expedição, ignora uma penalidade de ferimento leve |
| Halfling | +1 máx. Controle / −1 máx. Presença | -1 dificuldade em testes de Furtividade |
| Gnomo | +1 máx. Intelecto / −1 máx. Vigor | -1 dificuldade no primeiro teste de qualquer perícia de Artesanato aprendida |
| Meio-Elfo | Você escolhe livremente qual atributo recebe +1 e qual recebe −1 | Aptidão extra pode ser trocada 1x (versatilidade) |
| Draconato | +1 máx. Presença / −1 máx. Controle | Resistência a um tipo elemental (escolhido na criação) |
| Descendente Sombrio | +1 máx. Vontade / −1 máx. Presença | Resistência a medo sobrenatural |
| Fragmentado *(rara, exige aprovação do Mestre)* | +1 máx. Afinidade / −1 máx. Vigor | Sente a proximidade de Rupturas e instabilidade dimensional |

> 🛠️ **Homebrew**: Linhagens novas **sempre exigem aprovação do Mestre** — precisam de ajuste líquido +1/−1 num par de atributos, exatamente 1 traço racial, e nunca concedem perícia.

### 3.4 Aptidões Iniciais
Escolha **2 Aptidões**, entre as 6 abaixo. Cada uma facilita o aprendizado dentro do seu domínio: perícias daquele domínio sobem uma categoria na Curva de Aprendizado ao serem aprendidas do zero, e você ganha **-1 grau de dificuldade** em testes Absolutos com perícias do domínio ainda "Sem treinamento".

| Aptidão | Áreas de Perícia cobertas |
|---|---|
| Combate | Combate — Armas, Combate — Defesa, Combate Corporal, Combate à Distância |
| Exploração | Exploração |
| Conhecimento | Conhecimento, Cura |
| Ofício | Artesanato, Alquimia |
| Magia | Magia |
| Liderança | Social |

Aptidão nunca bloqueia nada: sem Aptidão em Magia você ainda pode virar mago, só terá um começo mais difícil.

> 🛠️ **Homebrew**: para uma Aptidão mais estreita (ex.: separar "Magia" em duas linhas), **consulte seu Mestre** — o novo domínio precisa ser um subconjunto claro de Áreas de Perícia já existentes.

### 3.5 Atributos
Oito atributos — quatro físicos, quatro mentais. **Modificador = Atributo − 2.**

**Físicos**: Corpo (força, carga, impacto) · Controle (coordenação, precisão, reflexos) · Vigor (resistência, fôlego, recuperação) · Presença (postura, coragem, domínio do espaço).
**Mentais**: Intelecto (lógica, aprendizagem, memória) · Percepção (observação, leitura de ambiente) · Vontade (disciplina, autocontrole) · Afinidade (conexão com o sobrenatural, compreensão de magia).

Distribua **20 pontos** livremente entre os 8 atributos, mínimo 1 e máximo 5 cada (6/4 se sua Linhagem ajustar). Atributos evoluem raramente — só por mudança física/mental real (meses de treino, provações extremas), nunca por simples uso em combate.

### Como subir um Atributo — Provação
Diferente de treinar uma perícia (progresso garantido dia a dia), subir um Atributo exige uma **Provação**: um projeto de Interlúdio dedicado, ligado ao atributo específico. Você só pode ter **1 Provação ativa por vez**.

```
Tempo da Provação = Grau atual × 10 dias
Custo em Recursos = Grau atual × 5 (Moedas de Pacto ou materiais equivalentes)
```

| De Grau → Para Grau | Tempo | Custo |
|---|---:|---:|
| I → II | 10 dias | 5 |
| II → III | 20 dias | 10 |
| III → IV | 30 dias | 15 |
| IV → V | 40 dias | 20 |

Você precisa de uma instalação da Guilda com Nível ≥ seu Grau atual no atributo (veja a tabela abaixo). Ao final do tempo, um Teste Absoluto contra Dificuldade **Difícil + (Grau atual × 2)** decide o resultado — falhar não bloqueia, só custa metade dos recursos, e você pode tentar de novo.

| Atributo | Provação | Perícia do Teste | Instalação mínima |
|---|---|---|---|
| Corpo | Resistência Extrema | Corpo (bruto) | Campo de Treinamento |
| Controle | Precisão Absoluta | Perícia de arma/estilo principal | Campo de Treinamento |
| Vigor | Provação de Fôlego | Sobrevivência | Enfermaria |
| Presença | Provação de Domínio | Liderança/Intimidação | Academia Militar |
| Intelecto | Provação Intelectual | Teoria Arcana/História | Biblioteca |
| Percepção | Provação Sensorial | Percepção | Biblioteca/Campo de Treinamento |
| Vontade | Provação de Disciplina | Vontade (própria) | Academia Militar |
| Afinidade | Provação Arcana | Controle Mágico/Rituais | Laboratório Arcano |

Além do Grau V, você precisa de **Transcendência** (bênçãos, rituais, eventos divinos) — a Provação normal nunca ultrapassa seu teto natural.

### 3.6 Perícias
Estrutura em três camadas: **Área de Conhecimento → Perícia → Especialização** (escolhida ao atingir 25 pontos/Adepto).

- **Combate — Armas** *(Controle; Corpo em golpes brutos)*: Espadas, Machados, Martelos, Lanças, Armas Improvisadas, Armas Exóticas.
- **Combate — Defesa** *(Controle/Vigor)*: Escudos, Armaduras, Esquiva, Bloqueio.
- **Combate Corporal** *(Corpo/Controle)*: Artes Marciais, Luta Desarmada, Agarramento.
- **Combate à Distância** *(Controle)*: Arcos, Bestas, Armas de Arremesso.
- **Exploração** *(Percepção/Vigor/Controle)*: Percepção, Rastreamento, Sobrevivência, Navegação, Furtividade, Armadilhas, Exploração de Dungeon, Escalada, Natação.
- **Conhecimento** *(Intelecto)*: História, Geografia, Criaturas, Religião, Linguagens, Estratégia, Dungeonologia, Conhecimento de Animais, Ocultismo, Avaliação.
- **Cura** *(Intelecto/Percepção)*: Medicina, Cirurgia, Farmacologia.
- **Artesanato** *(Controle/Intelecto)*: Ferraria, Carpintaria, Alfaiataria, Engenharia, Construção, Criação de Equipamentos, Culinária.
- **Alquimia** *(Intelecto)*: Poções, Venenos, Materiais, Transmutação.
- **Magia** *(Afinidade)*: Controle Mágico, Teoria Arcana, Rituais, Afinidade Elemental, Encantamentos.
- **Social** *(Presença/Intelecto)*: Diplomacia, Liderança, Comércio, Intimidação, Manipulação.

**Curva de Aprendizado**: aprender algo novo é mais fácil quanto maior a correlação com o que você já domina (ex.: Espada curta → Florete é fácil; Espada → Magia quase não ajuda).

| Pontos | Grau |
|---|---|
| 0 | Sem treinamento |
| 10 | Básico |
| 25 | Adepto |
| 50 | Especialista |
| 75 | Mestre |
| 100 | Lendário |

**Todo dia de treinamento no Interlúdio gera progresso na perícia treinada.** Como cada dia real entre sessões vale 1 dia de Interlúdio, o ritmo é deliberadamente lento no começo — e acelera conforme sua Guilda investe em infraestrutura:
```
Pontos de Treinamento/dia = (1 + Bônus de Instalação + Bônus de Instrutor) × Multiplicador de Curva de Aprendizado
```

- Base: **1 ponto/dia**.
- Bônus de Instalação relevante: `Nível × 0,5` (Nível × 1 se for uma instalação avançada dedicada, como Academia Militar para Combate).
- Bônus de Instrutor dedicado: **+1**.
- Multiplicador de Curva de Aprendizado: Alta ×1,5 | Média ×1,0 | Baixa ×0,5 | Sem correlação alguma (ainda na Fase de Aprendizado Inicial, 0-50 pontos) ×0,25.

*Exemplo*: treinar algo de Correlação Média num Campo de Treinamento Nível II, sem instrutor: `(1+1) × 1,0 = 2 pontos/dia`.

**Penalidade enquanto "Sem Treinamento"**: até chegar em Básico (10 pontos), sua perícia não dá +0 — dá **-2**. É a extensão natural da mesma tabela de Grau usada no Ataque (§4.5):

| Pontos | Grau | Bônus de Grau |
|---|---|---:|
| 0–9 | Sem Treinamento | **-2** |
| 10–24 | Básico | +0 |
| 25–49 | Adepto | +1 |
| 50–74 | Especialista | +2 |
| 75–99 | Mestre | +3 |
| 100+ | Lendário | +4 |

Esse -2 entra em qualquer teste que use aquela perícia (ataque, dano, testes relacionados). Se você tem a Aptidão do domínio, a Dificuldade do teste já fica 1 grau mais fácil enquanto Sem Treinamento — ajuda bastante, mas não elimina o risco de tentar algo do zero.

**Treinando enquanto Sem Treinamento**: nessa fase (0-9 pontos), os bônus de Instalação/Instrutor **não valem** — ninguém acelera o começo absoluto de um aprendizado. Em vez disso, você tem um teto fixo por Correlação:

| Correlação | Teto de Pontos/dia | Dias até Básico |
|---|---:|---:|
| Nenhuma | 1 | 10 dias |
| Baixa | 2 | 5 dias |
| Média | 3 | ~4 dias |
| Alta | 5 | 2 dias |

Assim que chega em Básico (10+), o teto some e a fórmula completa (com Instalação/Instrutor) passa a valer.

Não há limite para *conhecer* perícias, mas existe limite para *manter excelência* em muitas ao mesmo tempo (Capacidade Técnica/Intelectual, ligada aos seus atributos).

> 🛠️ **Homebrew**: precisa de uma perícia fora da lista? A lista oficial é fechada para balanceamento, mas **Perícias Personalizadas** existem — **consulte seu Mestre** para validar.

### 3.7 Talento Inicial
Escolha **1 Talento Inicial**, sem pré-requisitos. Ele é sempre mais discreto que um Talento conquistado em jogo mais tarde.

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
| 13 | Toque Elemental | Arcanos | Gera um efeito elemental cosmético/mínimo, sem gastar PA |
| 14 | Memória Arcana | Arcanos | 1x por pesquisa, reduz o tempo necessário em 1 dia |
| 15 | Presença Firme | Social | +1 em testes de Intimidação/Liderança quando em desvantagem numérica |
| 16 | Voz Confiável | Social | 1x por interlúdio, obtém uma informação de um NPC sem precisar de teste |
| 17 | Diplomata Nato | Social | -1 dificuldade no primeiro teste de Diplomacia com uma facção desconhecida |
| 18 | Sorte de Recruta | Extraordinário | 1x por expedição, transforma uma Falha (não crítica) em Sucesso simples |
| 19 | Marca Estranha | Extraordinário | Traço sobrenatural pequeno e inexplicado (definido com o Mestre) |
| 20 | Sina Protegida | Extraordinário | 1x na campanha inteira, sobrevive a um golpe que o mataria, ficando Incapacitado em vez de morto |

> 🛠️ **Homebrew**: **consulte seu Mestre** antes de criar um Talento Inicial próprio — precisa ter efeito único e pontual, nunca conceder PA extra permanente ou aumento de atributo.

### 3.8 Magias e Técnicas Iniciais
Sem uma regra especial, ninguém começaria com magia/técnica utilizável (elas exigem perícia Adepto ou mais). Por isso:

- **Aptidão em Magia** → você conhece **2 Magias de Complexidade Menor** (+1 extra se sua Origem também for arcana).
- **Aptidão em Combate** → você conhece **1 Postura + 1 Técnica (estágio I)**, de um estilo compatível com sua perícia primária.
- Sem essas Aptidões, mas quer 1 magia/técnica mesmo assim → **troque seu Talento Inicial** por 1 Magia Menor ou 1 Técnica/Postura básica.

Usar essas magias/técnicas ainda custa PA normalmente — a regra libera só o conhecimento.

---

## 4. Combate

### 4.1 Movimento

- **Combates pequenos** (poucos combatentes por lado) → **Grid/Hex**, medido em quadros. Seu Deslocamento é `4 + Mod(Vigor)` quadros por PA gasto em Mover.
- **Combates em larga escala** (hordas, batalhas) → **Zonas** (Contato/Curta/Média/Longa), 1 PA por zona adjacente.

| Zona | Grid/Hex (quadros) | Penalidade de alcance |
|---|---|---|
| Contato | 0–1 | Armas de longe sofrem penalidade grande |
| Curta | 2–6 | Alcance ideal da maioria de arcos/bestas |
| Média | 7–12 | -1 grau de dificuldade adicional |
| Longa | 13+ | -2 graus de dificuldade adicional |

Cobertura: **Leve** (+2 Defesa Passiva) | **Parcial** (+4 Defesa Passiva, metade do dano se acertar) | **Total** (impossível de atingir).

### 4.2 Iniciativa
`Iniciativa = 2d10 + Mod(Controle)`. Ordem decrescente; empate resolvido por maior Percepção.

### 4.3 Ações e Pontos de Ação (PA)
Você tem **3 PA por turno** + **1 Reação**. Ações: Mover (1 PA/zona), Atacar (1-2 PA conforme arma), Defender (1 PA, ativa Defesa Ativa), Usar Item (1 PA), Preparar Ação.

**Ataques de Oportunidade** não existem como mecânica própria — use sua Reação para "Interceptar" um inimigo que sai da sua Zona de Contato sem cuidado.

### 4.4 Defesa
Por padrão, sua **Defesa Passiva** já protege você sem gastar PA:
```
Defesa Passiva = 10 + Mod(Controle) + Bônus do Equipamento (armadura) + Bônus do Equipamento (escudo)
```
Se quiser se defender ativamente, gaste 1 PA (ação Defender) ou sua Reação — o ataque vira um Teste Oposto de verdade, onde você rola contra o atacante.

### 4.5 Ataque e Dano
```
Ataque = 2d10 + Bônus de Grau do Atributo + Bônus de Grau da Perícia
  Bônus de Grau do Atributo = Atributo (score) − 1   [Grau I=+0 | II=+1 | III=+2 | IV=+3 | V=+4]
  Bônus de Grau da Perícia  = Básico +0 | Adepto +1 | Especialista +2 | Mestre +3 | Lendário +4

Dano = Dado da arma + Mod(Atributo) + Bônus de Grau de Perícia + Bônus do Equipamento (arma)
  Armas Leves: 1d6 | Médias: 1d8 | Pesadas: 1d10 | Duas Mãos: 2d6

Redução de Dano da Armadura: Leve -1 | Média -2 | Pesada -3 (mínimo 1 de dano sempre passa)
```
Seu equipamento nunca melhora sua taxa de acerto — só seu Dano e sua Defesa.

A Margem de Sucesso modifica o dano: Sucesso = normal | Grande Sucesso = +1 dado extra | Sucesso Extraordinário = +2 dados extra.

### 4.6 Pontos de Vida
```
PV = 10 + (Vigor × 2) + Bônus de Ranking
Bônus de Ranking: Bronze +0 | Ferro +5 | Aço +10 | Prata +15 | Ouro +20 | Mithril +25 | Adamante +30 | Lendário +35
```
Recuperação natural só acontece de verdade no Interlúdio; dentro da Dungeon, um descanso curto recupera só uma fração pequena.

### 4.7 Condições
Ferido Leve, Ferido Grave, Sangrando, Atordoado, Enfraquecido, Amedrontado, Imobilizado, Agonizante, Morto.

### 4.8 Morte
Ao chegar a 0 PV, você fica **Agonizante** (inconsciente; um teste de Medicina pode te estabilizar). Qualquer dano adicional recebido enquanto Agonizante causa **morte instantânea** — não existe proteção narrativa. Ao morrer, seu personagem dropa um Cristal de Memória (§9).

---

## 5. Exploração

- **Turno de Exploração = 10 minutos.** Fora do combate, o tempo passa nessa unidade.
- **Visão**: Iluminado (sem penalidade) | Penumbra (-1 grau em testes visuais/ataques à distância) | Escuridão Total (testes visuais impossíveis, deslocamento pela metade). Uma tocha dura 6 Turnos (1 hora).
- **Navegação**: sua perícia mantém a rota; uma Falha Crítica te deixa perdido (custa 1 Turno extra e arrisca um encontro).
- **Armadilhas**: Detecção e Desarme são Testes Absolutos; uma falha nunca bloqueia a exploração, só gera consequência.
- **Exploração em grupo**: papéis sugeridos — Batedor, Guarda-Costas, Navegador, Especialista. Dividir o grupo em subgrupos reduz seu Poder de Grupo local se um encontro acontecer.
- **Descanso**: Descanso Curto (1 Turno) recupera uma fração pequena de PV; Acampamento Completo recupera mais, mas exige um local sem Pressão ativa e consome comida/água. Descansar sempre custa tempo.
- **Pressão da Dungeon**: seu Mestre acompanha um contador de tensão crescente em cada andar (Estável → Agravado → Crítico → Colapso). Você não vê o número exato, mas vai sentir o efeito: quanto mais tempo seu grupo gasta ou mais barulho faz, mais perigosa a Dungeon fica.

**Consumo de Recursos**:

| Recurso | Consumo |
|---|---|
| Comida | 1 ração/personagem por dia |
| Água | 1 cantil/personagem por dia (dobra em ambientes áridos) |
| Tocha | 1 unidade por 6 Turnos de Exploração |
| Corda | Por uso específico (escalada, poços) |
| Munição | 1 unidade por ataque à distância |
| Capacidade de Carga | `Corpo × 5` (peso); exceder penaliza deslocamento e testes físicos |

Ficar sem comida/água gera as Condições Faminto/Desidratado — nunca mata direto, mas debilita seriamente.

---

## 6. Magia e Técnicas Marciais

### 6.1 Escolas de Magia

| Escola | Foco |
|---|---|
| Evocação | Dano direto, energia, elementos |
| Abjuração | Proteção, escudos, resistências |
| Controle | Debuffs, imobilização, controle de área |
| Convocação | Invocar criaturas/objetos |
| Transmutação | Alterar forma/matéria |
| Ilusão | Enganar sentidos, disfarces |
| Necromancia | Manipular vida/morte, dreno, corrupção |
| Adivinação | Informação, detecção, precognição |

Toda magia tem: Escola, Custo em PA, Alcance (Zona), Área, Duração (Instantânea/Turnos/Cena/Persistente) e Teste (Oposto ou Absoluto).

| Complexidade | PA |
|---|---:|
| Menor | 1 |
| Moderada | 2 |
| Maior | 3 |
| Suprema | Conjuração Prolongada (múltiplos turnos) |

Seu Grau em Controle Mágico reduz o custo: Especialista -1 PA | Mestre -1 PA e -1 Turno | Lendário -2 PA e -1 Turno. Durante uma Conjuração Prolongada, sofrer dano ou falhar um Teste de Vontade interrompe a magia (o PA gasto se perde).

### 6.2 Magias de Exemplo

| Escola | Menor (1 PA) | Moderada (2 PA) | Maior (3 PA) |
|---|---|---|---|
| Evocação | Lança de Fogo | Rajada Flamejante | Tempestade de Chamas |
| Abjuração | Escudo Arcano | Barreira Protetora | Muralha Absoluta |
| Controle | Amarras de Vontade | Grilhões Arcanos | Prisão de Vontade |
| Convocação | Lâmina Espectral | Familiar de Batalha | Avatar Convocado |
| Transmutação | Toque Deformante | Metamorfose Parcial | Transfiguração Completa |
| Ilusão | Névoa Enganosa | Duplicata Ilusória | Véu da Mentira |
| Necromancia | Toque Debilitante | Sopro Sombrio | Chamado da Sepultura |
| Adivinação | Vislumbre | Leitura do Fio do Destino | Olho Onisciente |

> 🛠️ **Homebrew (Magias novas)**: **consulte seu Mestre** antes de criar uma magia nova. O processo: (1) escolher Escola; (2) escolher Complexidade (já fixa custo e teto de poder); (3) definir Alcance, Área, Duração e Teste; (4) definir um Efeito Único, redigido em mecânicas já existentes. Seu Mestre vai checar se você não está empilhando efeitos demais numa Complexidade baixa.

### 6.3 Magia Intuitiva (Magia Livre)
Com ao menos 1 ponto em Controle Mágico, você pode tentar produzir, na hora, um efeito mágico que não conhece formalmente — se couber numa Escola que você pratica.

- **Custo**: +1 PA a mais que a Complexidade estimada pelo Mestre.
- Você faz um **Teste Absoluto extra de Controle Mágico** para "montar" a magia ali mesmo.
- Falha = PA perdido, sem efeito. Falha Crítica = consequência (Condição leve, dano, ou pico de Tensão).
- Nunca reproduz efeito Supremo; nunca cria item físico permanente.
- Se der certo, seu Mestre pode formalizar como **Magia Descoberta** — passa a valer oficialmente, sem custo extra de pesquisa.

### 6.4 Técnicas Marciais
Cada estilo de combate tem sua árvore: **Posturas** (passivas, 1 PA para ativar, grátis depois) · **Técnicas** (ativas, 1-2 PA, podem evoluir de I para II) · **Reações** (usam sua Reação) · **Técnicas Supremas** (3 PA, uso limitado).

| Categoria | Perícia mínima | Ranking mínimo |
|---|---|---|
| Postura | Adepto (25) | — |
| Técnica | Especialista (50) | — |
| Reação | Especialista (50) | — |
| Técnica Suprema | Mestre (75) | Prata+ |

**Exemplos (Espadas)**: Postura Ofensiva (+1 dano/-1 Defesa) · Golpe Giratório I/II (atinge múltiplos alvos em Contato) · Aparar (Reação) · Corte que Divide o Véu (Suprema).
**Exemplos (Luta Desarmada)**: Guarda Fechada · Golpe Articulado I/II · Contragolpe (Reação) · Ruptura de Pontos Vitais (Suprema).
**Exemplos (Arcos)**: Mira Calculada · Tiro Encadeado I/II · Disparo de Interceptação (Reação) · Flecha que Perfura o Véu (Suprema).

> 🛠️ **Homebrew (Técnicas novas)**: **consulte seu Mestre**. Passo a passo: (1) escolher o Estilo/Arma-base; (2) escolher a Categoria (já fixa PA e perícia mínima); (3) definir o Efeito em termos de mecânicas existentes. Se a técnica tiver estágio II, ele sempre exige Perícia Mestre e +1 PA a mais que o estágio I.

---

## 7. Equipamentos e Crafting

### 7.1 Raridade

| Raridade | Propriedades Máximas | Bônus Base (Dano/Defesa) | NP |
|---|---|---|---:|
| Comum | 0 | +0 | 1 |
| Incomum | 1 | +1 | 3 |
| Raro | 2 | +2 | 7 |
| Épico | 3 | +3 | 15 |
| Lendário | 4 | +4 | 30 |
| Divino | 5+ | +5 ou efeito único | 50+ |

**Categorias**: Armas, Armaduras, Escudos, Ferramentas, Consumíveis, Artefatos, Relíquias.

### 7.2 Propriedades (lista oficial de 20)
Afiado · Preciso · Resistente · Leve · Flamejante / Gélido / Corrosivo · Perfurante · Vampírico · Ressonante · Camuflado · Selado · Instável · Regenerativo · Silencioso · Ancorado · Adaptável · Amplificador · Fragmentador · Selante · Amaldiçoado.

> 🛠️ **Homebrew (Propriedades novas)**: **consulte seu Mestre.** Toda propriedade ocupa exatamente 1 slot, deve equivaler a +1 dado de dano OU -1 grau de dificuldade num nicho específico OU um recurso reutilizável pontual OU resistência a 1 Condição — nunca mais que isso. Propriedades muito fortes exigem uma penalidade sempre ativa.

### 7.3 Criação (Crafting)
```
Teste Absoluto (Perícia de Artesanato) vs Dificuldade da Receita
```

| Raridade-alvo | Tempo | Custo em Materiais | Instalação mínima |
|---|---|---:|---|
| Comum | 1 dia | 5 | Oficina Básica |
| Incomum | 3 dias | 15 | Oficina Básica |
| Raro | 7 dias | 35 | Ferraria |
| Épico | 14 dias | 75 | Ferraria Avançada |
| Lendário | 30 dias | 150 | Forja Rúnica |
| Divino | Requer projeto de Pesquisa prévio | 250 + 10 Moedas de Pacto | Forja Divina |

Você precisa da **Receita Conhecida** ou de um **Projeto Descoberto** — não dá para fabricar uma raridade sem a receita correspondente.

### 7.4 Melhoria, Modificação e Reconstrução
**Melhoria** reforça o Bônus Base dentro da mesma raridade. **Modificação** troca 1 Propriedade por outra de custo equivalente. **Reconstrução** eleva o item para a raridade seguinte, com metade do tempo de criação do zero.

### 7.5 Durabilidade — Golpes de Desgaste
Seu item perde 1 Golpe de Desgaste apenas em Falha Crítica de ataque/defesa, ou em evento narrativo (armadilha, corrosão).

| Raridade | Golpes de Desgaste |
|---|---:|
| Comum | 3 |
| Incomum | 4 |
| Raro | 5 |
| Épico | 6 |
| Lendário | 8 |
| Divino | 10 |

Ao esgotar, o item fica Danificado (-1 no Bônus Base) até ser reparado no Interlúdio.

---

## 8. A Guilda (visão do Patrono)

Sua Guilda tem uma ficha própria com: Identidade (nome, brasão, divindade patrona), Prestígio, Influência, Recursos (Moedas de Pacto, materiais), Quartel-General (instalações), Funcionários, Conhecimento acumulado, Doutrinas ativas, Logística, registro de Expedições e Legado histórico.

**Quartel-General**: as construções formam uma árvore tecnológica real — cada uma tem pré-requisitos, custos, benefícios e sinergias com outras. No início da campanha só existem Portão, Dormitório e Campo de Treinamento básico; todo o resto é construído pelo Conselho de Patronos.

| Instalação | O que ela libera para você |
|---|---|
| Dormitório | Vagas para personagens e trabalhadores |
| Armazém | Capacidade de guardar recursos |
| Campo de Treinamento | Treino de combate; Provação de Corpo/Controle |
| Ferraria | Fabricar armas/armaduras |
| Oficina | Fabricar itens e ferramentas em geral |
| Biblioteca | Pesquisa; Provação de Intelecto/Percepção |
| Enfermaria | Cura melhor; Provação de Vigor |
| Laboratório Arcano | Pesquisa arcana avançada; Provação de Afinidade; Encantamentos |
| Academia Militar | Provação de Presença/Vontade; Técnicas Supremas |
| Jardim Alquímico | Alquimia avançada (venenos, transmutação) |
| Oficina de Runas | Itens Épicos+; encantamento de armas |
| Memorial | Acesso aos Cristais de Memória de personagens mortos |
| Torre dos Magos | Pesquisa e rituais no mais alto nível |

Peça ao seu Mestre a árvore completa (pré-requisitos e custos) se quiser planejar a construção com antecedência.

**Trabalhadores e Mercenários**: a Guilda emprega Operários, Artesãos, Pesquisadores, Instrutores, Mercadores, Médicos e Administradores. Mercenários podem patrulhar, coletar e explorar — mas **apenas andares já conquistados**; eles nunca entram em território desconhecido no seu lugar.

**Economia**: moeda comum + materiais + **Moedas de Pacto** (moeda divina especial, obtida na Dungeon). Toda recompensa de expedição é dividida entre Personagem / Guilda / Reserva Estratégica.

**Doutrinas**: especializações permanentes da filosofia da Guilda (Militar, Acadêmica, Comercial, Exploração, Arcana, Engenharia, Logística, Diplomática) — concedem bônus globais e dão identidade única à sua organização.

---

## 9. Interlúdio — O Tempo Entre Expedições

O **Interlúdio** é o período entre duas expedições do seu personagem, quando ele fica no Quartel-General. Toda atividade consome tempo e produz progresso específico:

1. **Treinamento** — progresso garantido e fixo por dia, melhorado por instalações/instrutores.
2. **Pesquisa** — Descobrir → Pesquisar → Dominar → Aplicar.
3. **Produção e Criação** — fabricação de itens (§7.3).
4. **Administração da Guilda** — gestão institucional (se seu personagem/Patrono participar disso).
5. **Expedições Secundárias** (mercenários) — sempre limitadas a andares já conquistados.

**Morte e Legado**: se seu personagem morre, ele dropa um **Cristal de Memória** — acessível no **Memorial**, sem transmitir atributos/perícias automaticamente, apenas conhecimento concreto vivido (mapas, idiomas, soluções de puzzles). Um novo personagem nunca começa do zero absoluto: ele recebe uma formação compatível com o quanto sua Guilda já evoluiu.

---

## 10. Glossário

- **Patrono** — você, no papel administrativo.
- **Personagem** — o aventureiro que você recruta.
- **Ruptura** — colapso dimensional quando um andar escapa da contenção.
- **Cristal de Memória** — registro póstumo de um personagem morto.
- **NP** — Nível de Poder (individual).
- **PA** — Pontos de Ação.
- **Grau** — nível de domínio de um atributo ou perícia (Básico → Lendário).
- **Ranking** — sua patente na Guilda (Bronze → Lendário).
