# Guild Sheet — Pesquisa & Crafting (Sub-plan #5) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let the guild track **research projects** (the Descobrir→Pesquisar→Dominar→Aplicar pipeline, with complexity-derived duration and a completion that feeds the CG's Pesquisa term) and **crafting orders** (category/item/quality/progress/status), surfaced through two new editor tabs — and finally **wire completed-research points into the CG calculator** (replacing the hardcoded `0`).

**Architecture:** Research projects and crafting orders are child entities (same pattern as buildings/staff/expeditions — own CRUD endpoints, returned inside `GuildSheetResponse`, targeted UI refresh). `ResearchProject.RequiredDays` is server-derived from `Complexity` (§11.2: 5/10/20/40); `Points` pre-fills by complexity on the client (Menor 1 / Moderada 2 / Maior 3 / Suprema 5) but is overridable and stored as given. `GuildSheetService.MapToResponseAsync` now sums `Points` over **completed** research projects and passes it as `researchPoints` to `GuildStatsCalculator` (which already accepts the parameter). Day-by-day progress advancement is **NOT** in this sub-plan — that's the Interlude Calculator (#6); here `ProgressDays`/`Stage`/`IsComplete` are edited manually.

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-08-07-guild-sheet-design.md` §3.2 (child entities), §4 (CG Pesquisa term), §7 (Pesquisa/Crafting tabs), §10 (§10.2 item 7 Conhecimento is already the blob Knowledge tab from #3), §12.5. GDD §11.2 (research tiers + workflow). Carry-in (memory `project_campaign_architecture.md` sub-plan #4 item 6b): **`researchPoints: 0` is hardcoded and this sub-plan wires it.**

## Global Constraints

- **Shared write, no field-level gating:** GM or member may CRUD research/crafting. Non-member → 404 (`Guild.NotFound`). Reuse `GuildSheetService.AuthorizeAsync(callerId, campaignId, ct)`.
- **`Ruptura.Shared` must NOT reference `Ruptura.Domain`** — DTOs carry enum-like values as `string` (`Complexity`/`Stage`/`Category`/`Status`); the service maps string↔enum with `Enum.TryParse(...) && Enum.IsDefined(...)` (rejecting numeric/undefined like `"5"` per the #4 lesson), returning the matching `Guild.*Invalid` code — never throws. `ResearchType` and `Quality` stay free strings (record-keeping; the UI picker supplies canonical values), not hard-validated.
- **`ResearchProject.RequiredDays` is server-authoritative:** derived from `Complexity` via `ResearchReference.RequiredDays` (Menor 5 / Moderada 10 / Maior 20 / Suprema 40) on BOTH create and update (re-derive if complexity changes); the create/update request does NOT carry `RequiredDays`. **`Points`** is client-supplied (pre-filled by complexity on the client, overridable) and stored clamped `>= 0`.
- **`CraftingOrder.RequiredDays` is manual** (no GDD crafting-time formula) — client-supplied, stored `>= 0`. `ProgressDays` clamped `>= 0` on both.
- **CG Pesquisa wiring:** `researchPoints = Σ (ResearchProject.Points where IsComplete)`. Only **completed** projects count. Incomplete projects contribute 0. This replaces the hardcoded `researchPoints: 0` in `MapToResponseAsync`.
- **Cross-guild safety:** update/delete verify the row's `GuildSheetId == the campaign's guild.Id`, else the matching `*NotFound` code.
- **Every visible string via `IStringLocalizer`**, in BOTH Web resx (en + pt-BR); API error strings in BOTH API resx. **The `GuildErrorCodeLocalizationTests` guard (from #4) will fail if any new `ErrorCodes.Guild` code lacks a resx string in either culture** — add the strings.
- **Integration tests** use `IntegrationTestFactory`, `IClassFixture<>`, `parallelizeTestCollections: false`; lone Serilog flake = known race, re-run once.
- **Commit after each task** on `main`; end messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:**
- `src/Ruptura.Shared/Guilds/ResearchProjectResponse.cs`, `CreateResearchProjectRequest.cs`, `UpdateResearchProjectRequest.cs`, `CraftingOrderResponse.cs`, `CreateCraftingOrderRequest.cs`, `UpdateCraftingOrderRequest.cs`, `ResearchReference.cs`
- `src/Ruptura.Application/Interfaces/IResearchProjectRepository.cs`, `ICraftingOrderRepository.cs`
- `src/Ruptura.Infrastructure/Repositories/ResearchProjectRepository.cs`, `CraftingOrderRepository.cs`
- `src/Ruptura.Web/Pages/GuildResearchTab.razor`, `GuildCraftingTab.razor`
- `tests/Ruptura.IntegrationTests/Guilds/GuildResearchTests.cs`, `GuildCraftingTests.cs`

**Modify:**
- `src/Ruptura.Shared/Guilds/GuildSheetResponse.cs` (+ `Research`, `Crafting` lists)
- `src/Ruptura.Application/Common/ErrorCodes.cs` (+ research/crafting codes)
- `src/Ruptura.Application/Interfaces/IGuildSheetService.cs` (+ research/crafting CRUD)
- `src/Ruptura.Infrastructure/Services/GuildSheetService.cs` (CRUD, researchPoints wiring, include in response)
- `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` (register the two repos)
- `src/Ruptura.API/Controllers/GuildController.cs` (+ research/crafting endpoints)
- `src/Ruptura.API/Resources/*.resx`, Web resx pair (strings)
- `src/Ruptura.Web/Pages/GuildSheet.razor` (mount the two tabs)
- the Web guild client service (research/crafting CRUD methods)

---

### Task 1: DTOs, research reference data, response additions, error codes

**Files:** the six DTOs + `ResearchReference`; modify `GuildSheetResponse`, `ErrorCodes`.

**Interfaces:**
- Produces: the six DTOs; `ResearchReference`; `GuildSheetResponse.Research`/`.Crafting`; new `ErrorCodes.Guild.*`.

- [ ] **Step 1: Research DTOs**

`src/Ruptura.Shared/Guilds/ResearchProjectResponse.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public class ResearchProjectResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResearchType { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty; // Menor|Moderada|Maior|Suprema
    public string Stage { get; set; } = string.Empty;      // Descobrir|Pesquisar|Dominar|Aplicar
    public int ProgressDays { get; set; }
    public int RequiredDays { get; set; }                  // server-derived from Complexity
    public int Researchers { get; set; }
    public int Points { get; set; }
    public bool IsComplete { get; set; }
}
```
`src/Ruptura.Shared/Guilds/CreateResearchProjectRequest.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class CreateResearchProjectRequest
{
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(60)]
    public string ResearchType { get; set; } = string.Empty;
    [Required]
    public string Complexity { get; set; } = string.Empty; // -> RequiredDays derived server-side
    public string Stage { get; set; } = "Descobrir";
    public int ProgressDays { get; set; }
    public int Researchers { get; set; } = 1;
    public int Points { get; set; }                        // client pre-fills by complexity, overridable
    public bool IsComplete { get; set; }
}
```
`src/Ruptura.Shared/Guilds/UpdateResearchProjectRequest.cs` — same fields as create (Name, ResearchType, Complexity, Stage, ProgressDays, Researchers, Points, IsComplete). No `RequiredDays` (server re-derives).

- [ ] **Step 2: Crafting DTOs**

`src/Ruptura.Shared/Guilds/CraftingOrderResponse.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public class CraftingOrderResponse
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;   // Forja|Alquimia|Encantamento|Engenharia|Artefatos
    public string ItemName { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;    // Comum|Superior|Raro|Épico|Lendário|Divino
    public int ProgressDays { get; set; }
    public int RequiredDays { get; set; }
    public string Status { get; set; } = string.Empty;     // EmAndamento|Concluido|Cancelado
}
```
`src/Ruptura.Shared/Guilds/CreateCraftingOrderRequest.cs`:
```csharp
using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class CreateCraftingOrderRequest
{
    [Required]
    public string Category { get; set; } = string.Empty;
    [MaxLength(120)]
    public string ItemName { get; set; } = string.Empty;
    [MaxLength(40)]
    public string Quality { get; set; } = string.Empty;
    public int ProgressDays { get; set; }
    public int RequiredDays { get; set; }                  // manual (no GDD formula)
    public string Status { get; set; } = "EmAndamento";
}
```
`src/Ruptura.Shared/Guilds/UpdateCraftingOrderRequest.cs` — same fields as create.

- [ ] **Step 3: Research reference data**

`src/Ruptura.Shared/Guilds/ResearchReference.cs`:
```csharp
namespace Ruptura.Shared.Guilds;

public static class ResearchReference
{
    // GDD §11.2 research tiers — base required days by complexity.
    public static readonly IReadOnlyDictionary<string, int> RequiredDays = new Dictionary<string, int>
    {
        ["Menor"] = 5, ["Moderada"] = 10, ["Maior"] = 20, ["Suprema"] = 40,
    };

    // Default CG Pesquisa points by complexity (house default — GDD doesn't fix this; overridable).
    public static readonly IReadOnlyDictionary<string, int> DefaultPoints = new Dictionary<string, int>
    {
        ["Menor"] = 1, ["Moderada"] = 2, ["Maior"] = 3, ["Suprema"] = 5,
    };

    public static readonly IReadOnlyList<string> Complexities = ["Menor", "Moderada", "Maior", "Suprema"];
    public static readonly IReadOnlyList<string> Stages = ["Descobrir", "Pesquisar", "Dominar", "Aplicar"];
    public static readonly IReadOnlyList<string> ResearchTypes =
        ["Arcana", "Biológica", "Tecnológica", "Dimensional", "Histórica", "Militar"];

    public static readonly IReadOnlyList<string> CraftingCategories =
        ["Forja", "Alquimia", "Encantamento", "Engenharia", "Artefatos"];
    public static readonly IReadOnlyList<string> CraftingStatuses = ["EmAndamento", "Concluido", "Cancelado"];
    public static readonly IReadOnlyList<string> Qualities =
        ["Comum", "Superior", "Raro", "Épico", "Lendário", "Divino"];
}
```

- [ ] **Step 4: Response additions**

In `GuildSheetResponse.cs`:
```csharp
    public List<ResearchProjectResponse> Research { get; set; } = [];
    public List<CraftingOrderResponse> Crafting { get; set; } = [];
```

- [ ] **Step 5: Error codes**

In `ErrorCodes.Guild`:
```csharp
        public const string ResearchNotFound = "Guild.ResearchNotFound";
        public const string ResearchComplexityInvalid = "Guild.ResearchComplexityInvalid";
        public const string ResearchStageInvalid = "Guild.ResearchStageInvalid";
        public const string CraftingNotFound = "Guild.CraftingNotFound";
        public const string CraftingCategoryInvalid = "Guild.CraftingCategoryInvalid";
        public const string CraftingStatusInvalid = "Guild.CraftingStatusInvalid";
```
> Add the matching en + pt-BR resx strings for all six in Task 2/3 (or now) — the `GuildErrorCodeLocalizationTests` guard from #4 fails otherwise.

- [ ] **Step 6: Build** — `dotnet build` PASS.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Shared/Guilds src/Ruptura.Application/Common/ErrorCodes.cs
git commit -m "feat: add guild research/crafting DTOs, research reference, error codes

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: Research project CRUD + CG Pesquisa wiring

**Files:** create `IResearchProjectRepository.cs`, `ResearchProjectRepository.cs`; modify `IGuildSheetService.cs`, `GuildSheetService.cs`, `GuildController.cs`, `InfrastructureExtensions.cs`, API resx; test `GuildResearchTests.cs`.

**Interfaces:**
- Produces: `IResearchProjectRepository.GetByGuildAsync`; `IGuildSheetService.AddResearchAsync/UpdateResearchAsync/DeleteResearchAsync`.

- [ ] **Step 1: Write the failing tests**

`tests/Ruptura.IntegrationTests/Guilds/GuildResearchTests.cs` — mirror `GuildBuildingTests`. Cases:
```
- Member adds a research project (Complexity "Maior", Points 3, IsComplete false) -> 201; GET shows it in Research with RequiredDays==20 (server-derived); DerivedStats.CgPesquisa == 0 (not complete yet).
- Mark it complete (update IsComplete true) -> 200; DerivedStats.CgPesquisa == 3 and Cg increased by 3.
- Add a second completed project (Menor, Points 1, IsComplete true) -> CgPesquisa == 4.
- Complexity re-derives RequiredDays on update (change "Maior"->"Menor" -> RequiredDays 5).
- Invalid Complexity string ("Huge") -> 400 Guild.ResearchComplexityInvalid; invalid Stage -> 400 Guild.ResearchStageInvalid.
- Points/ProgressDays negative -> clamped to 0 (or rejected — pick clamp per the plan).
- Delete -> gone; CgPesquisa drops.
- Non-member -> 404; cross-guild update/delete -> 404.
```

- [ ] **Step 2: Run → fail.**

- [ ] **Step 3: Repository**

`IResearchProjectRepository.cs` (in `Ruptura.Application.Interfaces`): `: IRepository<ResearchProject>` with `Task<IEnumerable<ResearchProject>> GetByGuildAsync(Guid guildSheetId, CancellationToken ct = default)`.
`ResearchProjectRepository.cs` (in `Ruptura.Infrastructure.Repositories`): `: BaseRepository<ResearchProject>(db)`, `GetByGuildAsync` = `Set.Where(r => r.GuildSheetId == guildSheetId).ToListAsync(ct)`. Register `AddScoped` in `InfrastructureExtensions`.

- [ ] **Step 4: Service methods + CG wiring**

Add to `IGuildSheetService.cs`: `AddResearchAsync`/`UpdateResearchAsync(…, Guid researchId, …)`/`DeleteResearchAsync` (parallel to buildings, returning `Result<ResearchProjectResponse>`/`Result`). Inject `IResearchProjectRepository researchRepo`.

Implement in `GuildSheetService.cs`:
- Parse `Complexity`/`Stage` strings via `Enum.TryParse<ResearchComplexity>(request.Complexity, out var c) && Enum.IsDefined(c)` → else `Guild.ResearchComplexityInvalid` / `Guild.ResearchStageInvalid` (never throw).
- `RequiredDays = ResearchReference.RequiredDays[complexity-string]` (server-authoritative; re-derive on update).
- `Points = Math.Max(0, request.Points)`, `ProgressDays = Math.Max(0, request.ProgressDays)`, `Researchers = Math.Max(1, request.Researchers)`.
- Cross-guild check on update/delete → `Guild.ResearchNotFound`.
- `MapResearch(ResearchProject)` → response (`Complexity = r.Complexity.ToString()`, `Stage = r.Stage.ToString()`).
- **CG wiring:** in `MapToResponseAsync`, replace `researchPoints: 0` with:
  ```csharp
  var research = (await researchRepo.GetByGuildAsync(guild.Id, ct)).ToList();
  var researchPoints = research.Where(r => r.IsComplete).Sum(r => r.Points);
  var derived = calculator.Calculate(data, buildings, staff, researchPoints, installationCatalog);
  ```
  and set `response.Research = research.Select(MapResearch).ToList();`.

- [ ] **Step 5: Controller endpoints + resx**

`POST/PUT/DELETE campaigns/{campaignId:guid}/guild/research[/{researchId:guid}]`, `[Authorize]`, POST→201/PUT→200/DELETE→200; `ResearchComplexityInvalid`/`ResearchStageInvalid` → 400, `ResearchNotFound`/`NotFound` → 404. Add en + pt-BR resx for the three research codes.

- [ ] **Step 6: Run tests → pass; commit**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildResearchTests` → PASS.
```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Guilds/GuildResearchTests.cs
git commit -m "feat: add guild research CRUD and wire completed-research points into CG

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Crafting order CRUD

**Files:** create `ICraftingOrderRepository.cs`, `CraftingOrderRepository.cs`; modify `IGuildSheetService.cs`, `GuildSheetService.cs`, `GuildController.cs`, `InfrastructureExtensions.cs`, API resx; test `GuildCraftingTests.cs`.

**Interfaces:**
- Produces: `ICraftingOrderRepository.GetByGuildAsync`; `AddCraftingAsync`/`UpdateCraftingAsync`/`DeleteCraftingAsync`.

- [ ] **Step 1: Write the failing tests**

`tests/Ruptura.IntegrationTests/Guilds/GuildCraftingTests.cs`:
```
- Member adds an order (Category "Forja", ItemName "Espada", Quality "Raro", RequiredDays 6, Status "EmAndamento") -> 201; GET shows it in Crafting.
- Update status to "Concluido" / progress -> 200; reflected.
- Invalid Category ("Nope") -> 400 Guild.CraftingCategoryInvalid; invalid Status -> 400 Guild.CraftingStatusInvalid.
- ProgressDays/RequiredDays negative -> clamped to 0.
- Delete -> gone.
- Non-member -> 404; cross-guild update/delete -> 404.
```

- [ ] **Step 2: Run → fail.**

- [ ] **Step 3: Repository** — `ICraftingOrderRepository`/`CraftingOrderRepository` (`GetByGuildAsync`), registered in DI, mirroring research.

- [ ] **Step 4: Service methods** — `AddCraftingAsync`/`UpdateCraftingAsync`/`DeleteCraftingAsync` (inject `ICraftingOrderRepository craftingRepo`). Parse `Category`/`Status` via `Enum.TryParse && Enum.IsDefined` → `Guild.CraftingCategoryInvalid`/`Guild.CraftingStatusInvalid`. `Quality` free string. `RequiredDays`/`ProgressDays` clamped `>= 0`. Cross-guild → `Guild.CraftingNotFound`. `MapCrafting` (`Category`/`Status` `.ToString()`). Include `Crafting` in `MapToResponseAsync`.

- [ ] **Step 5: Controller endpoints + resx** — `POST/PUT/DELETE .../guild/crafting[/{craftingId:guid}]`; category/status invalid → 400, not-found → 404. Add en + pt-BR resx for the three crafting codes.

- [ ] **Step 6: Run tests → pass; commit**

```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure src/Ruptura.API tests/Ruptura.IntegrationTests/Guilds/GuildCraftingTests.cs
git commit -m "feat: add guild crafting order CRUD

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 4: Pesquisa & Crafting tabs (Blazor)

Two child-entity tabs, mirroring the #4 building/staff tabs.

**Files:** create `GuildResearchTab.razor`, `GuildCraftingTab.razor`; modify `GuildSheet.razor` (mount), the Web guild client service (research/crafting CRUD), Web resx pair.

- [ ] **Step 1: Client methods** — add research + crafting CRUD to the Web guild client (mirror the building/staff client methods; surface non-200s for localized toasts). Each mutation refetches ONLY the affected list + `DerivedStats` + `Version` (never `_data`/`_guildName`).

- [ ] **Step 2: `GuildResearchTab`** (child entity) — table of `_guild.Research`; add-row: Name, ResearchType `<select>` (`ResearchReference.ResearchTypes`), Complexity `<select>` (`Complexities`) that shows the derived RequiredDays (`ResearchReference.RequiredDays`) and pre-fills Points (`ResearchReference.DefaultPoints`, overridable), Researchers, Stage `<select>` (`Stages`), ProgressDays, IsComplete toggle. Edit/delete (`Confirm.AskAsync`). Show RequiredDays + a note that completed projects add Points to CG. `.ledger-table.stack-mobile`. Localized.

- [ ] **Step 3: `GuildCraftingTab`** (child entity) — table of `_guild.Crafting`; add-row: Category `<select>` (`CraftingCategories`), ItemName, Quality `<select>` (`Qualities`), RequiredDays, ProgressDays, Status `<select>` (`CraftingStatuses`). Edit/delete. Localized.

- [ ] **Step 4: Mount + i18n** — add the two tabs to `GuildSheet.razor`'s strip. Every visible string in BOTH Web resx (tab titles, field labels, the ResearchType/Complexity/Stage/Category/Status/Quality display names, add/remove, the "completed adds to CG" note). English default + pt-BR.

- [ ] **Step 5: Build + verify + commit**

Run: `dotnet build` (clean). If feasible, run the app and confirm a completed research project bumps the Capacidades CG; else confirm clean build and note it.
```bash
git add src/Ruptura.Web
git commit -m "feat: add guild Pesquisa and Crafting editor tabs

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage (§3.2, §4, §7 Pesquisa/Crafting tabs, §12.5):**
- ResearchProject CRUD + workflow (Complexity-derived RequiredDays, Stage, IsComplete) → Task 2 + Task 4 tab. ✓
- CraftingOrder CRUD → Task 3 + Task 4 tab. ✓
- **CG Pesquisa wiring** (the carry-in — replace hardcoded 0 with Σ completed Points) → Task 2 Step 4. ✓
- Research reference (complexity→days, default points, picker option lists) → Task 1 `ResearchReference`. ✓
- Conhecimento (§10.2 item 7) is already the blob Knowledge tab from #3 — not re-done here. ✓
- **Deliberately deferred (not gaps):** day-by-day ProgressDays advancement + the researcher-split-time (§11.2) rule → the Interlude Calculator (#6); `ProgressDays`/`Stage`/`IsComplete` are manual here. Research/crafting material costs (Manual do Jogador §7.3) — not modeled (record-keeping; the guild's Resources are edited manually). Recursos inflation still open (spec §11 item 3), untouched.

**2. Placeholder scan:** Backend Tasks 1–3 carry complete code / precise method specs mirroring the #4 building/staff CRUD (a committed, reviewed pattern). Controller endpoints and UI (Task 4) are described structurally with exact DTOs/endpoints/reference lists to use — concrete Razor/client conventions read from the repo at execution (same posture as prior sub-plans). No "TBD"/"handle appropriately".

**3. Type consistency:** `Add/Update/DeleteResearchAsync` and the crafting trio share the `(callerId, campaignId, [id,] request, ct)` shape across interface/impl/controller. DTO `Complexity`/`Stage`/`Category`/`Status` are `string`, mapped to `ResearchComplexity`/`ResearchStage`/`CraftingCategory`/`CraftingStatus` in the service (`Enum.TryParse && Enum.IsDefined`). `ResearchReference.RequiredDays`/`DefaultPoints` keyed by the same complexity strings the DTO carries. `researchPoints` param (calculator, already present) fed from `Σ completed Points`. `GuildSheetResponse.Research`/`.Crafting` consumed by mapping + UI.
