# Ficha de Personagem — Design

**Data:** 2026-08-04
**Status:** Aprovado para planejamento
**Fonte de verdade de regras:** `docs/GDD_Ruptura.md` (§3, §6, §7, §15) e `docs/fichas/Ficha_de_Personagem.pdf` (referência de conteúdo dos campos, não de layout).

## 1. Contexto e Objetivo

RuptureRPG hoje só tem autenticação, convites e um dashboard vazio. Este design cobre a primeira fatia de features "de verdade" do produto: a **ficha de personagem** digital, com cálculo automático dos campos derivados (o maior valor sobre a ficha em papel).

Jogadores têm acesso a duas fichas: a **ficha de personagem** (individual, um jogador pode ter no máximo um personagem vivo/não aposentado por campanha) e a **ficha da Guilda** (compartilhada entre os jogadores de uma campanha, administrada coletivamente pelo Conselho de Patronos — fora de escopo aqui, ver `GuildSheet`, ainda não implementada).

## 2. Papéis e Nomenclatura (GDD §3)

- **Jogador** — a pessoa.
- **Patrono** — a representação administrativa permanente do jogador; administra a **Guilda** coletivamente com os outros Patronos (Conselho de Patronos); nunca entra na Dungeon.
- **Personagem** — o aventureiro descartável recrutado pelo Patrono; é o que a ficha desta spec representa.
- **Mestre** — conduz a **Campanha** (mundo, arcos, andares); aprova conteúdo homebrew que entra nela.

Importante para o modelo de dados: o Mestre administra a **Campanha**, não a Guilda. A Guilda é dos Patronos. `Campaign` é a raiz que contém tudo (incluindo a `GuildSheet`, quando for implementada).

## 3. Escopo desta fatia

**Módulos da ficha de personagem** (cada um vira uma aba na UI):

1. Identidade (nome, Origem, Histórico, Linhagem, Aptidões, Talento Inicial, Ranking, NP, retrato)
2. Atributos (8 atributos, grau/modificador calculados)
3. Combate (PV, Defesa Passiva, Deslocamento, Iniciativa, PA, tabela de armas, condições ativas)
4. Perícias (pontos investidos → grau calculado)
5. Talentos e Habilidades
6. Magias Conhecidas
7. Técnicas/Posturas
8. Equipamentos e Inventário (peso/capacidade de carga calculados)
9. Provação de Atributo (entrada manual — sem calendário de campanha ainda)
10. Registro da Guilda (dentro da ficha do personagem: ingresso, estado, expedições, andares, morto/aposentado)
11. Diário (entradas com texto + imagens)

**Pré-requisito estrutural** (também nesta fatia, ver §4.1): entidade `Campaign`, vínculo Mestre↔Jogador, roster, atribuição de jogadores a campanhas, concessão de fichas.

**Explicitamente fora desta fatia:**
- Wizard de criação guiada de personagem (fica pra depois; por ora, todo campo — incluindo os catálogo-referenciados — é preenchido diretamente pelo jogador/Mestre).
- Motor de "Curva de Aprendizado" (redução de dificuldade por correlação de perícias).
- Criação de magias/técnicas via Pesquisa Arcana (projeto com tempo).
- Avanço automático de Provação de Atributo por calendário.
- Validação de pré-requisito de Talento/Técnica (Ranking/perícia mínima) — Mestre pode adicionar manualmente sem o sistema bloquear.
- `GuildSheet` (ficha da Guilda em si) — só ganha `CampaignId` para consistência de esquema.
- Bestiário, lista de materiais e outros catálogos globais reutilizáveis entre campanhas — a modelagem do `CatalogEntry` já os suporta (via `CampaignId = null`), mas populá-los é trabalho futuro.

## 4. Modelo de Dados

### 4.1 Campaign e vínculo Mestre↔Jogador (fundação)

```
Campaign
├─ Id (Guid)
├─ Name (string)
├─ GameMasterId (Guid)
├─ CreatedAt / UpdatedAt

CampaignMembership
├─ Id (Guid)
├─ CampaignId (Guid, FK → Campaign)
├─ PlayerId (Guid)
├─ AssignedAt (DateTime)
```

`ApplicationUser` (Infrastructure) ganha `RecruitedByGameMasterId (Guid?)`, preenchido no registro a partir de `InviteCode.CreatedByGameMasterId` do código usado. Isso define o **roster** do Mestre (todo jogador com esse campo apontando pra ele) sem precisar de uma tabela nova — `InviteCode` já guarda `CreatedByGameMasterId`/`UsedByPlayerId`, só estamos denormalizando pra consulta rápida.

**Novo fluxo:**
```
1. Jogador registra com InviteCode → entra no roster do Mestre (RecruitedByGameMasterId)
2. Mestre cria uma Campaign
3. Mestre atribui jogadores do seu roster à Campaign (CampaignMembership)
4. Mestre concede uma CharacterSheet a um membro da Campaign
5. Se o personagem morre/aposenta (IsDead/IsRetired = true), o Mestre pode conceder um novo
```

**Regra "1 personagem vivo/não aposentado por jogador por Campaign"**, em duas camadas:
- Application (`CampaignService`/`CharacterSheetService`): valida antes de criar, retorna `Result.Failure` com mensagem clara.
- Banco (rede de segurança contra concorrência): índice único parcial no Postgres —
  ```sql
  CREATE UNIQUE INDEX ux_character_sheets_owner_campaign_alive
  ON character_sheets (owner_id, campaign_id)
  WHERE NOT is_dead AND NOT is_retired;
  ```

### 4.2 Catálogo unificado (`CatalogEntry`)

Cobre Origem, Histórico, Linhagem, Aptidão, Talento, Perícia, Magia, Técnica e Item de Equipamento — todos têm, no GDD, uma lista oficial fechada **e** um manual de criação homebrew.

```
CatalogEntry
├─ Id (Guid)
├─ Type (enum: Origin, Background, Lineage, Aptitude, Talent, Skill, Spell, Technique, EquipmentItem)
├─ CampaignId (Guid?)         — null = oficial/global (seed, reutilizável entre campanhas); preenchido = homebrew daquela Campaign
├─ Name (string)
├─ DataJson                   — campos específicos do tipo (ver §4.2.1)
├─ CreatedByGameMasterId (Guid?) — null para os itens oficiais seedados
├─ CreatedAt / UpdatedAt
```

Índice único `(Type, CampaignId, Name)` evita duplicatas dentro do mesmo escopo.

**Seed data** (migration inicial, `CampaignId = null`): as 20 Origens, 20 Históricos, 10 Linhagens, 6 Aptidões, 20 Talentos Iniciais (GDD §6.1.2–6.1.7), as 8 Escolas de Magia + magias de exemplo (§6.6.6), técnicas de exemplo por estilo (§6.6.8), e a lista de Perícias Fundamentais por Área (§6.4). Somente Mestres criam/editam entradas homebrew (`POST/PUT/DELETE`), escopadas à própria Campaign.

#### 4.2.1 Campos por tipo (dentro de `DataJson`)

Só os campos relevantes para o motor de cálculo (§5) precisam existir logo; o resto é texto livre exibido na ficha:

- **Talent**: `Category`, `Effect`, `PowerTier` (`menor`|`médio`|`maior` → peso de NP 1/3/5)
- **Skill**: `Area`, `RelatedAttribute` (qual dos 8 atributos)
- **Spell**: `School`, `ComplexityPaCost`, `Range`, `Area`, `Duration`, `Test`, `Effect`
- **Technique**: `Style`, `Category` (Postura/Técnica/Reação/Suprema), `PaCost`, `Effect`
- **EquipmentItem**: `Category` (arma/armadura/escudo/item), `Rarity` (Comum/Incomum/Raro/Épico/Lendário/Divino → peso de NP 1/3/7/15/30/50), `AttackBonus`, `DamageBonus`, `DefenseBonus`, `WeaponDiceCategory` (Leve 1d6/Média 1d8/Pesada 1d10/DuasMãos 2d6, se arma), `ArmorDamageReduction` (Leve−1/Média−2/Pesada−3, se armadura), `Weight`
- **Origin/Background/Lineage/Aptitude**: campos narrativos/mecânicos leves conforme GDD (benefício, complicação, ajuste de teto, etc.) — texto livre, sem impacto no motor de cálculo nesta fatia (a wizard de criação, fora de escopo, é quem os aplicaria automaticamente).

### 4.3 CharacterSheet

```
CharacterSheet
├─ Id (Guid)
├─ CharacterName (string)
├─ OwnerId (Guid)                    — jogador dono
├─ CampaignId (Guid, FK → Campaign)
├─ GrantedByGameMasterId (Guid)      — auditoria de quem concedeu
├─ IsDead (bool)                     — coluna real (não DataJson) — só Mestre edita; usada na regra de unicidade
├─ IsRetired (bool)                  — idem
├─ PortraitImagePath (string?)
├─ CreatedAt / UpdatedAt
├─ DataJson                          — ver §4.3.1
```

`IsDead`/`IsRetired` são colunas reais (não dentro do `DataJson`) porque precisam ser consultadas eficientemente pela regra de unicidade — diferente do "Estado" narrativo (ativo/ferido/ausente/desaparecido), que continua livre dentro do `DataJson.GuildRegistry.State` como campo descritivo, sem efeito mecânico.

#### 4.3.1 `CharacterSheetData` (estrutura do `DataJson`)

```
CharacterSheetData
├─ Identity          { OriginId, BackgroundId, LineageId, AptitudeIds[2], InitialTalentId, PatronDisplayName }
                       // PatronDisplayName: texto livre pro campo "Jogador/Patrono" da ficha impressa (flavor;
                       // o dono real, para autorização, é sempre CharacterSheet.OwnerId)
├─ Attributes         { Corpo, Controle, Vigor, Presença, Intelecto, Percepção, Vontade, Afinidade }  // 1-6, base
├─ Combat              { CurrentHp, ActiveConditions[] }   // só o que não é derivável (PV atual, condições)
├─ Skills[]             { CatalogEntryId, Points }
├─ Talents[]            { CatalogEntryId }
├─ Spells[]              { CatalogEntryId }
├─ Techniques[]          { CatalogEntryId }
├─ Equipment[]            { CatalogEntryId, Quantity, DurabilityRemaining }
├─ Currency                { PactCoins }
├─ AttributeTrial            { AttributeName, TargetGrade, DaysRemaining }   // entrada manual
├─ GuildRegistry               { Ranking, JoinedDate, State, Expeditions, FloorsCleared }
```

Todo campo "derivado" descrito no GDD (modificadores, PV máx., Defesa Passiva, Deslocamento, bônus de ataque/dano por arma, capacidade de carga, peso atual, NP) **não é armazenado** — é calculado em toda leitura pelo motor descrito em §5.

### 4.4 `CharacterJournalEntry`

Tabela própria (não dentro do `DataJson` da ficha, que é reescrito a cada pequena edição em sessão — um diário com dezenas de entradas e imagens não deveria viver ali):

```
CharacterJournalEntry
├─ Id (Guid)
├─ CharacterSheetId (Guid, FK)
├─ Text (string)
├─ ImagePaths (jsonb, string[])   — 0+ imagens anexadas
├─ CreatedAt / UpdatedAt
```

Autor é sempre o dono da ficha (só ele escreve — ver §6, matriz de permissões); por isso não há campo `AuthorId`.

### 4.5 `Notification`

```
Notification
├─ Id (Guid)
├─ RecipientUserId (Guid)          — o Mestre
├─ CampaignId (Guid)               — denormalizado, para agrupar por campanha na UI sem join
├─ Type (enum — extensível; único valor hoje: RankPromotionAvailable)
├─ RelatedCharacterSheetId (Guid?)
├─ IsRead (bool)
├─ CreatedAt
```

**Gatilho**: ao salvar uma `CharacterSheet`, o motor recalcula o NP; se o NP ultrapassar o teto da faixa do `Ranking` atual (tabela §6.8 — Bronze 40–70, Ferro 70–105, Aço 105–145, Prata 145–195, Ouro 195–260, Mithril 260–340, Adamante 340–430, Lendário 430–550+) e não existir notificação `RankPromotionAvailable` não-lida para essa ficha, cria uma.

**Ação semi-automática**: a notificação tem duas resoluções possíveis pelo Mestre — (a) abrir a ficha e mudar o `Ranking` manualmente, ou (b) clicar "Promover" na própria notificação, que chama `POST /api/notifications/{id}/promote`: avança o `Ranking` **exatamente um degrau** na sequência oficial (nunca pula direto pro degrau compatível com o NP atual, mesmo que o NP já qualifique para mais — mantém o princípio de crescimento suave do GDD) e marca a notificação como lida. Se o NP ainda exceder o novo teto após a promoção, uma nova notificação é gerada naturalmente na próxima recalculação — sem necessidade de lógica especial.

**UI**: `/gm/notifications`, agrupada por Campaign (cabeçalho por campanha, notificações não-lidas dentro), sem tempo real — busca ao carregar a página, mesmo padrão do resto do app (sem SignalR no stack hoje).

## 5. Motor de Cálculo (`CharacterStatsCalculator`)

Serviço puro na Application — recebe `CharacterSheetData` + os `CatalogEntry` referenciados (perícias, equipamento) e devolve os derivados. Nunca persiste resultado.

```
Modificador(Atributo) = Atributo(score) − 2

Bônus de Grau do Atributo(score) = score − 1     // Grau I(1)=0 .. V(5)=4; além de V só via Transcendência (fora de escopo)

Bônus de Grau da Perícia(pontos):                 // GDD + rodapé da ficha oficial
  0–9    → −2  (Sem Treinamento)
  10–24  → 0   (Básico)
  25–49  → +1  (Adepto)
  50–74  → +2  (Especialista)
  75–99  → +3  (Mestre)
  100+   → +4  (Lendário)

PV Máximo = 10 + (Vigor × 2) + BônusRanking       // Bronze+0 Ferro+5 Aço+10 Prata+15 Ouro+20 Mithril+25 Adamante+30 Lendário+35
Deslocamento = 4 + Modificador(Vigor)
Iniciativa (modificador exibido; rolagem 2d10 é feita à mesa) = Modificador(Controle)
Defesa Passiva = 10 + Modificador(Controle) + BônusDefesa(armadura equipada) + BônusDefesa(escudo equipado)

Por arma equipada/perícia vinculada:
  BônusDeAtaque = BônusDeGrau(Atributo da perícia) + BônusDeGrau(Perícia)
  Dano = DadoDaCategoria(arma) + Modificador(Atributo da perícia) + BônusDeGrau(Perícia) + BônusDeDano(arma)
  ReduçãoDeDanoRecebido = ArmorDamageReduction da armadura equipada (mínimo 1 de dano sempre passa)

CapacidadeDeCarga = Corpo(score) × 5
PesoAtual = Σ Weight(item) × Quantity, para itens no inventário

NP = [ Σ BônusDeGrau(cada um dos 8 Atributos) + Σ BônusDeGrau(cada Perícia investida) ]
   + [ Σ PesoDeNP(Talento/Habilidade) ]     // menor=1, médio=3, maior=5
   + [ Σ PesoDeNP(Equipamento) ]            // Comum=1 Incomum=3 Raro=7 Épico=15 Lendário=30 Divino=50+
```

Os bônus específicos de cada arma/armadura (`AttackBonus`/`DamageBonus`/`DefenseBonus`) vêm do `DataJson` do `CatalogEntry` do item — o motor não hardcoda uma tabela raridade→bônus; cada item (oficial ou homebrew) carrega seus próprios números.

## 6. API e Permissões

| Recurso | Ler | Criar/Editar | Apagar |
|---|---|---|---|
| `CharacterSheet` (geral) | Dono ou Mestre da Campaign | Dono ou Mestre | Mestre |
| `CharacterSheet.IsDead/IsRetired` | Dono ou Mestre | **Só Mestre** | — |
| `CharacterJournalEntry` | Dono ou Mestre | **Só dono** | Só dono |
| `CatalogEntry` (oficial, `CampaignId=null`) | Todos autenticados | — (seed, imutável) | — |
| `CatalogEntry` (homebrew da Campaign) | Membros da Campaign | **Só Mestre** | Só Mestre |
| Mídia (retrato) | Dono ou Mestre | Dono ou Mestre | Dono ou Mestre |
| Mídia (imagens do diário) | Dono ou Mestre | Só dono (via entrada do diário) | Só dono |
| `Campaign` | Mestre + membros | Mestre | Mestre |
| `CampaignMembership` | Mestre + membros | Mestre | Mestre |
| `Notification` | O próprio Mestre destinatário | — (gerada pelo sistema) | Mestre marca como lida |

Validação de campo restrito (`IsDead`/`IsRetired`) acontece no `CharacterSheetService`: se quem chama não é o Mestre da Campaign e o payload tenta mudar esses dois campos, a request falha (`Result.Failure`) — o resto do payload segue normalmente.

Endpoints principais:
```
GET   /api/gamemaster/players                          (roster)
POST/GET /api/campaigns
POST  /api/campaigns/{id}/members                       (atribuir jogador do roster)
POST  /api/campaigns/{id}/character-sheets               (Mestre concede — valida regra de personagem único vivo)
GET/PUT /api/character-sheets/{id}
GET/POST /api/character-sheets/{id}/journal-entries
PUT/DELETE /api/character-sheets/{id}/journal-entries/{entryId}
GET   /api/catalog?type={type}&campaignId={id}            (oficiais + homebrew da Campaign)
POST/PUT/DELETE /api/catalog/{id}                          (Mestre, homebrew)
POST  /api/media                                             (upload; retorna path)
GET   /api/media/{*path}                                      (download, autorizado)
GET   /api/notifications                                       (do Mestre logado, agrupadas por Campaign)
POST  /api/notifications/{id}/promote
POST  /api/notifications/{id}/dismiss
```

## 7. Armazenamento de Mídia

Sem storage de arquivos hoje (só Postgres + API + Blazor estático via nginx). Decisão: **disco local via volume Docker** (novo volume `character_media`, nos moldes do `api_logs` já existente), escondido atrás de `IFileStorageService` (interface na Application, implementação na Infrastructure) — troca futura para S3/MinIO não toca domínio/aplicação.

Registrado aqui deliberadamente: optamos por disco local em vez de object storage porque este projeto não tem necessidade de ser tão enxuto/distribuído (instância única, self-hosted) — não é uma limitação técnica, é uma escolha consciente de simplicidade.

## 8. UI

**Lado Mestre (novo):**
- `/gm/players` — roster
- `/gm/campaigns` — lista + criar
- `/gm/campaigns/{id}` — membros, fichas da campanha (conceder/ver/editar, marcar morto/aposentado)
- `/gm/campaigns/{id}/catalog` — admin do catálogo homebrew por tipo (oficiais só-leitura ao lado dos homebrew editáveis)
- `/gm/notifications` — agrupadas por Campaign

**Lado Jogador (novo):**
- `/campaigns` — campanhas em que participo
- `/campaigns/{id}/character` — minha ficha (ou aviso de que aguardo o Mestre conceder uma)

**Ficha de personagem**: uma página, módulos em **abas** — `Identidade | Atributos | Combate | Perícias | Talentos | Magias | Técnicas | Equipamento | Provação | Registro da Guilda | Diário`. Reaproveita os padrões visuais existentes (`page-content`, `page-heading`, `ledger-table`). Diário com lista cronológica de entradas + upload de imagem.

## 9. Testes

- **Unit (`Ruptura.UnitTests`)**: `CharacterStatsCalculator` (tabela de casos com os exemplos do GDD/PDF — modificadores, PV, Defesa Passiva, Deslocamento, Ataque/Dano, capacidade de carga, NP); permissões em `CharacterSheetService` (dono/Mestre editam geral; só Mestre muda `IsDead`/`IsRetired`; só dono escreve diário); regra de personagem único vivo em `CampaignService`; gatilho de notificação de promoção de Ranking.
- **Integration (`Ruptura.IntegrationTests`)**: fluxo completo via `WebApplicationFactory` + Postgres real — registro → roster → criar Campaign → atribuir jogador → conceder ficha → editar módulos → tentar conceder 2ª ficha viva (falha) → marcar morta → conceder nova (sucesso); matriz de autorização (jogador alheio → 403); upload/download de mídia autorizado; índice único parcial sob concorrência (duas concessões simultâneas, só uma vence).

## 10. Decisões registradas

- Campos derivados são sempre calculados, nunca digitados — maior diferencial sobre a ficha em papel.
- Catálogo unificado (uma tabela, discriminador `Type`) em vez de uma tabela por tipo — menos migrations, mais fácil de acompanhar o ritmo de mudança do GDD.
- Homebrew escopado por `Campaign` (não por `GuildSheet`) — Guilda é dos Patronos, Campanha é do Mestre; homebrew é aprovação do Mestre.
- `IsDead`/`IsRetired` são colunas reais, não campos dentro do JSON — precisam ser consultados pela regra de unicidade.
- Diário vira tabela própria com múltiplas entradas + imagens, não um campo de texto solto.
- Mídia em disco local via volume Docker, atrás de uma interface — escolha consciente de simplicidade, documentada para não parecer descuido.
- Promoção de Ranking é semi-automática: notificação ao Mestre, que escolhe promover com um clique (um degrau por vez) ou editar manualmente.
- NP = Σ Bônus de Grau (Atributos) + Σ Bônus de Grau (Perícias) + Σ peso de Talentos/Habilidades + Σ peso de Equipamentos (raridade).

## 11. Próximos passos (fora desta spec)

- Wizard de criação guiada de personagem (opcional, usa os mesmos catálogos).
- Ficha da Guilda (`GuildSheet`) propriamente dita.
- Motor de Curva de Aprendizado, Pesquisa Arcana, Provações automatizadas por calendário.
- Bestiário e demais catálogos globais reutilizáveis entre campanhas.
