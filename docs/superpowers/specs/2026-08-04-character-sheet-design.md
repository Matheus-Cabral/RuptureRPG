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
├─ CampaignId (Guid?, FK → Campaign, ON DELETE CASCADE) — null = oficial/global (seed, reutilizável entre campanhas); preenchido = homebrew daquela Campaign
├─ Name (string)
├─ DataJson                   — campos específicos do tipo (ver §4.2.1)
├─ IsArchived (bool, default false) — soft-delete; "apagar" um homebrew seta esta flag em vez de remover a linha
├─ CreatedByGameMasterId (Guid?) — null para os itens oficiais seedados
├─ CreatedAt / UpdatedAt
```

Índice único `(Type, CampaignId, Name)` evita duplicatas dentro do mesmo escopo.

**Soft-delete (decidido no sub-plan #3):** a partir do momento em que `CharacterSheet` passa a guardar `CatalogEntry.Id`s (Perícias, Talentos, Magias, Técnicas, Equipamentos — ver §4.3.1), apagar de verdade uma entrada homebrew em uso órfãos silenciosamente as fichas que a referenciam. `DELETE /api/catalog/{id}` passa a setar `IsArchived = true` em vez de remover a linha. Leituras para popular seletores (`GET /api/catalog`) excluem arquivadas por padrão; a página de administração do Mestre (`/gm/campaigns/{id}/catalog`) continua mostrando-as (com indicação visual de arquivada), via flag de query ou endpoint próprio — detalhe a decidir no plano. Fichas que já referenciam uma entrada arquivada continuam resolvendo normalmente. `CatalogEntry.CampaignId` ganha FK real para `Campaign.Id` com `ON DELETE CASCADE` (era `Guid?` solto até aqui) — se uma Campaign for apagada no futuro, seu catálogo homebrew vai junto, sem linhas órfãs.

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

**`GuildRegistry.Ranking` é só-Mestre, decidido no sub-plan #5:** ao contrário do resto do `DataJson` (editável por dono-ou-Mestre), `Ranking` segue a mesma regra de `IsDead`/`IsRetired` — mesmo vivendo dentro do blob, não como coluna real. Como não há coluna dedicada para comparar antes/depois, `CharacterSheetService.UpdateAsync` desserializa tanto o `DataJson` atual quanto o recebido só o suficiente para extrair `GuildRegistry.Ranking`; se o valor mudou e quem chama não é o Mestre da Campaign, a request inteira falha (mesma semântica tudo-ou-nada de `IsDead`/`IsRetired`). Motivo: sub-plan #3 deixou `Ranking` editável por dono-ou-Mestre, o que conflita com a promoção controlada pelo Mestre desenhada em §4.5 — um jogador não pode simplesmente digitar o próximo Ranking e ignorar a notificação/aprovação do Mestre.

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
├─ Equipment[]            { CatalogEntryId, Quantity, DurabilityRemaining, IsEquipped, LinkedSkillEntryId? }
                       // IsEquipped: só itens equipados alimentam Combate (arma → linha na tabela de armas;
                       // armadura/escudo → somam DefenseBonus/ArmorDamageReduction na Defesa Passiva).
                       // LinkedSkillEntryId: qual Perícia (CatalogEntryId de um Skills[] investido) rege o
                       // ataque/dano desta arma — o GDD não amarra arma↔perícia no catálogo (uma "Espada Longa"
                       // pode ser usada com a perícia "Espadas"), então o jogador escolhe manualmente por item
                       // equipado; null para itens que não são armas.
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
├─ CharacterSheetId (Guid, FK → CharacterSheet, ON DELETE CASCADE)
├─ Text (string)
├─ ImagePaths (jsonb, List<string>)   — 0+ imagens anexadas, coluna jsonb via EF value converter
├─ CreatedAt / UpdatedAt
```

Autor é sempre o dono da ficha (só ele escreve — ver §6, matriz de permissões); por isso não há campo `AuthorId`.

**FK real, decidido no sub-plan #4:** ao contrário da convenção de referência fraca do resto do repo (`CharacterSheet.OwnerId`, `CatalogEntry.CreatedByGameMasterId`), `CharacterJournalEntry.CharacterSheetId` ganha uma FK de verdade com `ON DELETE CASCADE` — é uma tabela nova sendo criada do zero (não um retrofit em coluna existente), e apagar uma ficha deveria mesmo levar seu diário junto. A limpeza dos arquivos de imagem em disco correspondentes não é coberta pelo `ON DELETE CASCADE` do Postgres (arquivos não vivem no banco) — o `CharacterSheetService`/`JournalEntryService`, se algum dia implementar exclusão de ficha, precisa apagar os arquivos manualmente antes ou depois do cascade.

**Edição completa:** `PUT` substitui `Text` **e** `ImagePaths` juntos (mesmo formato da criação) — não existe edição só-de-texto. Uma imagem removida da lista tem seu arquivo apagado do disco pelo mesmo request. `DELETE` da entrada apaga a entrada e todos os arquivos de imagem associados.

**Ordenação:** lista sempre mais recente primeiro (`OrderByDescending(CreatedAt)`), consistente com o resto do app (campanhas, etc.).

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

**Gatilho**: ao salvar uma `CharacterSheet`, o motor recalcula o NP; se o NP ultrapassar o teto da faixa do `Ranking` atual (tabela §6.8 — Bronze 40–70, Ferro 70–105, Aço 105–145, Prata 145–195, Ouro 195–260, Mithril 260–340, Adamante 340–430, Lendário 430–550+) e não existir notificação `RankPromotionAvailable` não-lida para essa ficha, cria uma. Lendário não tem teto (faixa aberta) — nunca dispara. A tabela de tetos e a sequência de Rankings vivem em `NotificationService` (não em `CharacterStatsCalculator`): é uma regra de negócio de gravação (quando notificar), não um stat derivado exibido na ficha.

**Ação semi-automática**: a notificação tem duas resoluções possíveis pelo Mestre — (a) abrir a ficha e mudar o `Ranking` manualmente (agora só-Mestre, ver §4.3), ou (b) clicar "Promover" na própria notificação, que chama `POST /api/notifications/{id}/promote`: avança o `Ranking` **exatamente um degrau** na sequência oficial (nunca pula direto pro degrau compatível com o NP atual, mesmo que o NP já qualifique para mais — mantém o princípio de crescimento suave do GDD) e marca a notificação como lida. Se o NP ainda exceder o novo teto após a promoção, uma nova notificação é gerada naturalmente na próxima recalculação — sem necessidade de lógica especial. `DELETE`-like "descartar": `POST /api/notifications/{id}/dismiss` marca como lida sem tocar no `Ranking` — o Mestre decidiu não promover agora.

**Autorização de `PromoteAsync`/`DismissAsync`**: comparar `notification.RecipientUserId == callerId` já basta — uma notificação só é criada com `RecipientUserId` = o Mestre da própria Campaign, então essa checagem prova posse sem precisar carregar a `Campaign` de novo.

**FKs (decidido no sub-plan #5)**: `CampaignId` ganha FK real para `Campaign.Id` com `ON DELETE CASCADE` (mesma lógica do `CatalogEntry.CampaignId` — apagar a campanha apaga suas notificações). `RelatedCharacterSheetId` ganha FK real para `CharacterSheet.Id` com `ON DELETE SET NULL` (é nullable; perder a ficha não deveria apagar o histórico de notificação, só a referência). `RecipientUserId` continua `Guid` solto, sem FK — segue a convenção do resto do repo de não referenciar as tabelas do Identity (`Campaign.GameMasterId`, `CharacterSheet.OwnerId`).

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
| `CharacterSheet.GuildRegistry.Ranking` | Dono ou Mestre | **Só Mestre** | — |
| `CharacterJournalEntry` | Dono ou Mestre | **Só dono** | Só dono |
| `CatalogEntry` (oficial, `CampaignId=null`) | Todos autenticados | — (seed, imutável) | — |
| `CatalogEntry` (homebrew da Campaign) | Membros da Campaign | **Só Mestre** | Só Mestre |
| Mídia (retrato) | Dono ou Mestre | Dono ou Mestre | Dono ou Mestre |
| Mídia (imagens do diário) | Dono ou Mestre | Só dono (via entrada do diário) | Só dono |
| `Campaign` | Mestre + membros | Mestre | Mestre |
| `CampaignMembership` | Mestre + membros | Mestre | Mestre |
| `Notification` | O próprio Mestre destinatário | — (gerada pelo sistema) | Mestre marca como lida |

Validação de campo restrito (`IsDead`/`IsRetired`/`GuildRegistry.Ranking`) acontece no `CharacterSheetService`: se quem chama não é o Mestre da Campaign e o payload tenta mudar algum desses três campos, a request falha (`Result.Failure`) — o resto do payload segue normalmente. `IsDead`/`IsRetired` são colunas reais, comparadas diretamente; `Ranking` exige desserializar `DataJson` (atual e recebido) para extrair e comparar `GuildRegistry.Ranking`, já que não tem coluna própria.

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

### 7.1 Autorização por path (decidido no sub-plan #4)

`GET /api/media/{*path}` não usa uma tabela de metadados própria para checar permissão — o path em si carrega a entidade dona (`character-sheets/{sheetId}/portrait-{guid}.ext` ou `journal-entries/{entryId}/{guid}.ext`). O endpoint faz o parse do tipo+id, carrega a entidade real (`CharacterSheet` ou `CharacterJournalEntry`) e reaplica exatamente a mesma checagem dono-ou-Mestre que os endpoints normais já usam — sem tabela nova, sem duplicar a lógica de autorização. Path traversal (`..`) é rejeitado explicitamente antes de qualquer lookup.

`POST /api/media` é multipart (`file` + `entityType` [`CharacterSheetPortrait`|`JournalEntryImage`] + `entityId`) e **muda a entidade-alvo diretamente no servidor** — não é só "salva e devolve o path": para `CharacterSheetPortrait`, apaga o arquivo antigo de `PortraitImagePath` (se houver) e grava o novo path na ficha; para `JournalEntryImage`, adiciona o novo path em `ImagePaths` daquela entrada. O cliente nunca precisa de um PUT de acompanhamento só para "linkar" a imagem recém-enviada.

Tipos aceitos: jpg/png/webp/**gif**, validados por assinatura de bytes (magic number), não só pela extensão do arquivo.

### 7.2 Limites configuráveis (decidido no sub-plan #4)

`MediaSettings` (bind igual a `JwtSettings`, seção `MediaSettings` no `appsettings.json`/env): `MaxFileSizeMb` e `MaxImagesPerJournalEntry`, ambos configuráveis via `.env` (`MEDIA_MAX_FILE_SIZE_MB`, `MEDIA_MAX_IMAGES_PER_ENTRY`) — **`0` = sem limite**. Sem limite embutido no código; quem hospeda decide o teto.

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

**Diário e retrato (sub-plan #4):** a aba Diário só mostra os controles de escrita (criar/editar/apagar entrada) quando quem está vendo é o **dono** da ficha — diferente de `CanEditStatus` (que só controla `IsDead`/`IsRetired`), o componente `CharacterSheetEditor` ganha um parâmetro `IsOwner` próprio para isso; um Mestre vendo a ficha de um jogador enxerga o Diário, mas só em modo leitura. O campo de retrato no cabeçalho, hoje um `<input>` de texto puro (path/URL), vira um upload de arquivo de verdade com preview, usando o mesmo fluxo de `/api/media`.

**Ranking só-Mestre (sub-plan #5):** o `<select>` de Ranking em `CharacterSheetGuildRegistryTab` passa a usar o mesmo `CanEditStatus` já usado por `IsDead`/`IsRetired` no cabeçalho — dono vê o valor como texto somente-leitura, Mestre vê o `<select>` editável.

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
- `CatalogEntry` apagado por soft-delete (`IsArchived`), não FK `Restrict` (decidido no sub-plan #3, ver §4.2) — GM pode "apagar" um homebrew em uso sem que isso quebre fichas existentes; seletores de nova seleção escondem entradas arquivadas.
- `CatalogEntry.CampaignId` ganha FK real para `Campaign.Id` com `ON DELETE CASCADE` (decidido no sub-plan #3) — antes era `Guid?` solto, seguindo a convenção de referência fraca do resto do repo; decidimos reforçar aqui por já estar mexendo na tabela.
- Sub-plan #3 escopo: `CharacterSheet` (entidade + fluxo de concessão) + `CharacterStatsCalculator` + as 9 abas que não são Diário (#4) nem Notificações (#5) — um único plano SDD, direto na `main`, mesmo padrão dos sub-plans #1 e #2.
- `Equipment[]` ganha `IsEquipped` (bool) e `LinkedSkillEntryId` (Guid?) — lacuna do desenho original: o motor de cálculo (§5) precisa saber quais itens estão equipados (só esses alimentam Combate) e qual Perícia rege cada arma (o catálogo não amarra item↔perícia; o jogador escolhe manualmente por item equipado).
- Sub-plan #4 escopo (decidido 2026-08-05): `CharacterJournalEntry` (CRUD completo, dono-only) + `IFileStorageService`/disco local + a aba Diário + upload de retrato real. Autorização de mídia por path (sem tabela de metadados), decidido para não duplicar lógica de autorização já existente em `CharacterSheet`/`CharacterJournalEntry` (ver §7.1).
- `CharacterJournalEntry.CharacterSheetId` ganha FK real com `ON DELETE CASCADE` (ver §4.4) — exceção deliberada à convenção de referência fraca do repo, justificada por ser tabela nova, não retrofit.
- Limites de upload (tamanho de arquivo, imagens por entrada) são configuráveis via `.env`, com `0` = sem limite — não hardcoded (ver §7.2). Tipos aceitos: jpg/png/webp/gif, validados por magic bytes.
- Upload de retrato substitui o arquivo antigo (apaga do disco antes de gravar o novo) — não deixa órfãos.
- Edição de entrada de diário é completa (texto + imagens juntos), não só-texto — uma única forma de editar, sem endpoint separado para adicionar/remover imagem de uma entrada existente.
- Lista de diário é sempre mais recente primeiro.
- Sub-plan #5 escopo (decidido 2026-08-06): `Notification` (entidade + `NotificationService`) + gatilho de promoção no `CharacterSheetService.UpdateAsync` + endpoints de promover/descartar + `/gm/notifications`. Fecha a lacuna deixada pelo sub-plan #3: `GuildRegistry.Ranking` passa a ser só-Mestre (mesma regra tudo-ou-nada de `IsDead`/`IsRetired`), tanto no `CharacterSheetGuildRegistryTab` (UI) quanto no `CharacterSheetService.UpdateAsync` (servidor) — ver §4.3 e §6. Sem essa restrição, a promoção controlada pelo Mestre desenhada aqui não faria sentido (o jogador poderia simplesmente digitar o próximo Ranking).
- `Notification.CampaignId` ganha FK real com `ON DELETE CASCADE`; `Notification.RelatedCharacterSheetId` ganha FK real com `ON DELETE SET NULL` (nullable, não deveria apagar histórico); `RecipientUserId` continua `Guid` solto (convenção do repo de não referenciar tabelas do Identity).
- Tabela de tetos de NP por Ranking (para o gatilho de notificação) e a sequência de avanço vivem em `NotificationService`, não em `CharacterStatsCalculator` — é regra de quando notificar, não um stat derivado exibido na ficha.
- Autorização de `promote`/`dismiss` usa só `notification.RecipientUserId == callerId` — não precisa recarregar a `Campaign`, já que uma notificação só existe com o Mestre da própria campanha como destinatário.

## 11. Próximos passos (fora desta spec)

- Wizard de criação guiada de personagem (opcional, usa os mesmos catálogos).
- Ficha da Guilda (`GuildSheet`) propriamente dita.
- Motor de Curva de Aprendizado, Pesquisa Arcana, Provações automatizadas por calendário.
- Bestiário e demais catálogos globais reutilizáveis entre campanhas.
