# Rework de UX/UI — Paleta, Tipografia, Responsividade e Ferramentas de Usabilidade

**Data:** 2026-08-06
**Status:** Aprovado para planejamento
**Escopo:** `src/Ruptura.Web` — fundação visual (`wwwroot/css/app.css`), componentes de layout (`Layout/`) e todas as páginas (`Pages/`). Não altera nenhuma camada de backend (Domain/Application/Infrastructure/API).

## 1. Contexto e Objetivo

O sistema hoje usa o design system "Arcane Ledger" (estética de registro institucional de fantasia sombria: Cinzel para títulos, DM Sans para corpo, JetBrains Mono para dados, cantos retos, paleta pergaminho/vinho/dourado). A identidade funciona, mas tem problemas concretos:

- **Legibilidade**: ~39 usos de `font-size` entre 8px e 10.5px espalhados pelo CSS (labels, badges, nav, stats) — abaixo de qualquer piso confortável de leitura.
- **Contraste**: sidebar usa texto branco em opacidades baixas (`rgba(255,255,255,0.2–0.5)`), e `--text-faint`/`--text-muted` não têm garantia de contraste AA contra os fundos onde aparecem.
- **Semântica de cor confusa**: `--primary` (vermelho-vinho) é usado tanto para ações principais (botões, links) quanto para erro/validação, sem uma cor de perigo dedicada.
- **Responsividade rasa**: um único breakpoint (768px) trata tablet e smartphone da mesma forma; a sidebar em telas médias vira overlay de tela cheia em vez de um tratamento intermediário.
- **Ferramentas de usabilidade ausentes**: não existe sistema de notificação (toast), nenhuma ação destrutiva (deletar catálogo, deletar entrada de diário, dispensar notificação) pede confirmação, o padrão de loading (`spinner-border` + texto) está duplicado em ~6 páginas, não há busca/filtro nas tabelas mais longas, e não há breadcrumbs em telas aninhadas.

Este design **não troca a identidade visual** — mantém Cinzel/DM Sans/JetBrains Mono, tom sombrio, cantos retos, vermelho-vinho + dourado como cores de marca. O objetivo é refinar paleta, tipografia, responsividade e adicionar um conjunto pequeno de componentes de usabilidade reutilizáveis, que servem de **modelo para o desenvolvimento das próximas funcionalidades**.

## 2. Decisão de Arquitetura

Continuar dentro da stack atual: CSS puro com custom properties em `:root`/`[data-theme]`, overrides do Bootstrap (vendorizado como arquivo estático, sem pipeline npm), e componentes Razor pequenos com um serviço C# por trás — o mesmo padrão que `ThemeSwitcher`/`ThemeService` já usa. Nenhuma dependência nova (sem Tailwind, sem MudBlazor/Radzen) — evita pipeline de build adicional e mantém o objetivo do projeto de demonstrar arquitetura .NET limpa, não tooling de frontend.

## 3. Paleta de Cores — Light e Dark

Papel semântico separado de cor de marca. Todas as combinações texto/fundo abaixo miram **WCAG AA** (≥ 4.5:1 para texto normal, ≥ 3:1 para texto grande/UI).

| Token | Papel | Light | Dark |
|---|---|---|---|
| `--primary` | Marca / ação principal (botões, links, foco) | `#7A1B1B` | `#B23A2E` |
| `--primary-dark` | Hover/active de `--primary` | `#5A1313` (mais escuro — hover escurece sobre fundo claro) | `#C94E40` (mais claro — hover clareia sobre fundo escuro) |
| `--accent` | Destaque secundário (dourado — banners, ranks, ênfase) | `#8A6A22` | `#D4AF5A` |
| `--danger` | Erro/validação/exclusão — **separado do primary** | `#B3261E` | `#F2685C` |
| `--success` | Confirmação/status ativo | `#2E7D32` | `#6FCF8A` |
| `--info` | Neutro informativo | `#1A5FA8` | `#7AB4F0` |
| `--bg` | Fundo de página | `#EDE9E1` (mantido) | `#100D0A` (mantido) |
| `--bg-surface` | Fundo de cartão/superfície elevada | `#F5F2EC` (mantido) | `#1C1813` (mantido) |
| `--bg-nav` | Fundo da sidebar (fixo, sempre escuro nos dois temas) | `#15100B` | `#15100B` |
| `--text` | Texto principal | `#1A1511` | `#F0EAE0` |
| `--text-muted` | Texto secundário — precisa AA em corpo de texto | `#57504A` | `#B8AFA5` |
| `--text-faint` | Terciário/decorativo — **nunca usado em corpo de texto**, só ornamentos (regras, marcas d'água) | `#847A70` | `#7A7168` |
| `--border` / `--border-strong` | Mantidos, revisados para contraste suficiente contra `--bg`/`--bg-surface` | — | — |

Cores de badge (`badge-active/used/expired`) e alertas passam a derivar de `--success`/`--info`/`--text-muted` + `--danger`, em vez de valores hex soltos.

**Sidebar**: texto de nav passa de `rgba(255,255,255,0.5)` para `rgba(255,255,255,0.72)` (inativo) e `0.92` (hover/ativo) — ela é sempre um fundo escuro fixo, independente do tema, então o contraste é calculado uma vez só.

## 4. Tipografia

Escala nomeada substitui os `rem` soltos; piso de **11px** em qualquer lugar do sistema (chega a 8px hoje):

```
--text-2xs: 0.6875rem (11px)  → só tags/labels maiúsculas curtas
--text-xs:  0.75rem   (12px)  → nav, badges, metadados
--text-sm:  0.875rem  (14px)  → texto secundário, tabelas
--text-base:1rem      (16px)  → corpo de texto (piso do body)
--text-lg:  1.125rem
--text-xl:  1.375rem  → títulos de seção
--text-2xl: 1.75rem   → títulos de página
--text-3xl: 2.25rem   → números de estatística
```

Cinzel (display), DM Sans (corpo), JetBrains Mono (dados/códigos) mantidos. `line-height: 1.6` no corpo mantido.

## 5. Responsividade — três faixas

| Faixa | Largura | Comportamento |
|---|---|---|
| **Desktop** | ≥ 1024px | Sidebar fixa (220px) + topbar fixa, como hoje. |
| **Tablet** | 600–1023px | Sidebar colapsa para **rail de ícones** (56px, sem texto, tooltip no hover/foco); toque/clique expande temporariamente. Tabelas usam scroll horizontal contido (`.ledger-table-wrap`) em vez de espremer colunas. |
| **Mobile** | < 600px | Sidebar em overlay de tela cheia via hambúrguer (padrão atual mantido); tabelas longas (Jogadores, Catálogo) viram **cartões empilhados** por linha em vez de tabela com scroll. |

Regras transversais:
- Alvos de toque **≥ 44×44px** em qualquer elemento interativo dentro de 1023px (botões, links de nav, `copy-btn`, itens de badge clicáveis).
- Títulos grandes usam `clamp()` em vez de `rem` fixo (hero, `page-heading h1`) para não estourar em telas pequenas.

## 6. Componentes Novos de Usabilidade

Todos em C#/Razor, sem dependência externa, com strings via `IStringLocalizer<AppStrings>` (ver §7):

1. **`ToastService` + `ToastContainer`** — notificação de sucesso/erro/info no canto da tela; auto-dismiss configurável + botão fechar; região `aria-live="polite"`. Substitui `alert-danger` inline solto e cobre ações que hoje não dão feedback nenhum (ex.: salvar ficha).
2. **`ConfirmService` + `ConfirmDialog`** — `await Confirm.AskAsync(title, message)` retorna `bool`; usado antes de qualquer ação destrutiva (`DeleteAsync` de catálogo/diário, `DismissAsync` de notificação, `Remove` de perícia/equipamento/condição na ficha).
3. **`LoadingIndicator` / `SkeletonRows`** — substitui o padrão `<span class="spinner-border">...Carregando...` duplicado em ~6 páginas; tabelas longas mostram linhas-esqueleto (shimmer) em vez de spinner genérico.
4. **`TableSearchBox`** — filtro client-side instantâneo, aplicado em Jogadores, Catálogo e Notificações (GM). Atalho `Ctrl+K` / `/` foca a busca da página atual.
5. **`Breadcrumbs`** — para telas aninhadas (GM → Campanha → Ficha do Personagem), com link de volta em cada nível.
6. **Atalhos de teclado básicos** — `Esc` fecha modal/dropdown/sidebar mobile aberta; `Ctrl+K`/`/` foca a busca da página. Sem command palette — fora de escopo.
7. **Acessibilidade transversal** — `:focus-visible` visível em toda a paleta nova; `aria-label` em botões só-ícone (hambúrguer, copiar, seletor de tema); contraste AA (§3).

## 7. Bilinguismo e Tema — requisitos preservados

- **Bilíngue por padrão**: todo texto novo introduzido por este rework (toasts, `ConfirmDialog`, breadcrumbs, placeholder de busca, tooltips, `aria-label`) usa `IStringLocalizer<AppStrings>` com chave em `AppStrings.pt-BR.resx` **e** `AppStrings.en.resx`. Nenhum texto novo hardcoded — mesmo que a tradução completa do GDD/manuais para inglês seja trabalho futuro separado.
- **Tema**: `ThemeSwitcher`/`ThemeService` (Light / System / Dark) mantidos como estão estruturalmente — só recebem tratamento visual (ícones em vez de letras soltas `L`/`S`/`D`, contraste adequado na nova paleta). O padrão continua "System" até o usuário escolher explicitamente; a escolha continua persistida como já é feita hoje.

## 8. Plano de Rollout

Como a maior parte das páginas herda classes compartilhadas de `app.css` (só `NavMenu.razor.css` é CSS "scoped" hoje), a fundação propaga sozinha para a maioria das telas; o restante é uma varredura dirigida.

**Fase 1 — Fundação** (`app.css`): tokens de cor (§3), escala tipográfica (§4), breakpoints responsivos (§5), `:focus-visible` global. Efeito imediato na maioria das páginas.

**Fase 2 — Componentes novos** (§6): implementados uma vez, registrados em DI/`MainLayout`, com testes próprios.

**Fase 3 — Varredura por página**, em grupos, nesta ordem:
1. **Casca**: `MainLayout`, `NavMenu`, `ThemeSwitcher`, `LanguageSwitcher` — rail de ícones no tablet, contraste da sidebar, ícones do tema.
2. **Autenticação**: `Login`, `Register`, `Index` (landing).
3. **GM**: `GmDashboard`, `GmPlayers`, `GmCatalog`, `GmCampaigns`, `GmCampaignDetail`, `GmFields`, `GmNotifications`, `GmCharacterSheet` — aplica busca (Jogadores/Catálogo/Notificações), `ConfirmDialog` em deletar/dispensar, breadcrumbs em Campanha→Ficha.
4. **Player**: `PlayerDashboard`, `PlayerCampaigns`, `PlayerCharacter`, `PlayerFields`.
5. **Ficha de personagem** (`CharacterSheetEditor` + 10 abas): aplica `ConfirmDialog` em "Remove" (perícias/equipamento/condições) e `LoadingIndicator`/`SkeletonRows` consistente.

Cada fase termina com build + verificação visual (skill `run`, captura de tela) antes de avançar para a próxima.

## 9. Fora de Escopo

- Tradução completa do GDD/manuais para inglês (só a infraestrutura de chaves de recurso é garantida aqui).
- Command palette / busca global cross-página.
- Troca de biblioteca de componentes (MudBlazor/Radzen) ou introdução de pipeline de build frontend (Tailwind etc.).
- Alteração de qualquer regra de negócio, modelo de dados ou endpoint de API.
- Dark mode com paletas alternativas além de light/dark/system (ex.: alto-contraste dedicado) — pode ser trabalho futuro.

## 10. Testes

- Nenhum teste de backend é afetado (mudança é só front-end).
- `ToastService`/`ConfirmService` ganham testes unitários (comportamento do serviço: fila de toasts, resolução da `Task<bool>` do confirm).
- Verificação visual manual (skill `run`) em cada fase do rollout, nos três breakpoints (desktop/tablet/mobile) e nos dois temas (light/dark), como critério de "pronto" — não há suíte automatizada de regressão visual neste projeto.
