# Character Skill-Training Interlude Calculator — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A preview-and-apply Skill Training calculator on `CharacterSheet` — the owner or GM picks a known (or brand-new homebrew) skill, a day count, and a Curva de Aprendizado correlation level; sees the GDD §6.4 Pontos de Treinamento/dia formula computed server-side (installation/instructor bonuses read live from the campaign's Guild); and applies it, which bumps that skill's `Points`.

**Architecture:** A pure `ITrainingCalculator` (Application) mirrors the shape of the guild's existing `IInterludeCalculator` but implements the character-level formula. Unlike the guild's calculator, Apply is **not** a dedicated persisting endpoint — `CharacterSkillEntry.Points` already lives inside the fully client-editable `CharacterSheetData` blob (the existing Skills tab lets the owner/GM type any value into it directly, no server validation of the total), so Apply is a client-side mutation of `Data.Skills` that rides the character sheet editor's existing `AutosaveWatcher`/Save path. Only Preview is a server round-trip (it's the only part that needs data the client doesn't have: the campaign's Guild buildings/staff).

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-09-10-character-training-interlude-design.md` (read in full — this plan implements it verbatim, including the Apply-plumbing correction made while writing this plan).

## Global Constraints

- **Formula is FECHADO (GDD §6.4/§6.5)** — reproduce the tables exactly (see spec §3), not just their observable effect.
- **Correlation wire values are unaccented:** `"Alta"`, `"Media"`, `"Baixa"`, `"Nenhuma"` (matches the repo's existing enum-as-string convention, e.g. `CraftingStatus.Concluido`).
- **`Ruptura.Shared` stays ZERO project references** — nothing in this feature's Shared code may reference Domain/Application types.
- **Every visible string via `IStringLocalizer`**, both Web resx (en + pt-BR) and both API resx where relevant — no hardcoded UI text.
- **New `ErrorCodes` constants need resx entries in the SAME task** that introduces them — `GuildErrorCodeLocalizationTests`/the new `CharacterSheetErrorCodeLocalizationTests` (Task 3) will fail the build otherwise.
- **Catalog `DataJson` is deserialized with DEFAULT `JsonSerializerOptions`, never `JsonSerializerDefaults.Web`** — the seed/catalog convention (see `CharacterStatsCalculator.DeserializeSkill`/`SafeDeserialize`).
- **Free-text catalog fields compare case/whitespace-insensitively** (`CategoryIs` convention) — applies to Área matching in the calculator.
- **No FK** on the two new `GuildStaff` dedication columns — bare `Guid?`/`string?`, matching the repo-wide soft-reference convention (`CharacterSheet.CampaignId`, `Campaign.GameMasterId`, etc.).
- **Integration tests** use `IntegrationTestFactory`, `IClassFixture<>`; the suite runs with `parallelizeTestCollections: false` (pre-existing Serilog flake) — a lone failing test may just need a re-run.
- **Commit after each task** on `main`; end commit messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:**
- `src/Ruptura.Shared/CharacterSheets/TrainingReference.cs` (Área closed list; expanded in Task 2 with the Área→Instalação mapping)
- `src/Ruptura.Shared/CharacterSheets/TrainingProjection.cs`
- `src/Ruptura.Application/Interfaces/ITrainingCalculator.cs`
- `src/Ruptura.Application/Services/TrainingCalculator.cs`
- `src/Ruptura.Web/Pages/CharacterSheetTrainingTab.razor`
- `tests/Ruptura.UnitTests/Application/TrainingCalculatorTests.cs`
- `tests/Ruptura.IntegrationTests/Controllers/CharacterSheetTrainingTests.cs`
- `tests/Ruptura.IntegrationTests/Controllers/CharacterSheetErrorCodeLocalizationTests.cs`

**Modify:**
- `src/Ruptura.Domain/Entities/GuildStaff.cs` (+2 nullable fields)
- `src/Ruptura.Shared/Guilds/GuildCatalogIds.cs` (+6 installation GUIDs)
- `src/Ruptura.Shared/Guilds/CreateStaffRequest.cs`, `UpdateStaffRequest.cs`, `GuildStaffResponse.cs` (+2 fields each)
- `src/Ruptura.Application/Common/ErrorCodes.cs` (+`Guild.StaffDedicationInvalid`, +3 `CharacterSheet.*`)
- `src/Ruptura.Application/Interfaces/ICharacterSheetService.cs` (+`PreviewTrainingAsync`)
- `src/Ruptura.Infrastructure/Services/GuildSheetService.cs` (staff dedication validation + mapping)
- `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs` (+`PreviewTrainingAsync`)
- `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs` (register `ITrainingCalculator` singleton)
- `src/Ruptura.API/Controllers/CharacterSheetController.cs` (+preview endpoint)
- `src/Ruptura.API/Resources/SharedResources*.resx`, `src/Ruptura.Web/Resources/AppStrings*.resx`
- `src/Ruptura.Web/Pages/GuildStaffTab.razor`, `src/Ruptura.Web/Pages/GuildSheet.razor` (dedication picker, GM-only)
- `src/Ruptura.Web/Pages/CharacterSheetEditor.razor` (mount the Interlúdio tab)
- `src/Ruptura.Web/Services/ICharacterSheetClientService.cs`, `CharacterSheetClientService.cs` (+`PreviewTrainingAsync`)
- `tests/Ruptura.IntegrationTests/Guilds/GuildStaffTests.cs` (+dedication tests)

---

### Task 1: `GuildStaff` Instrutor dedication (GM-only)

**Files:** modify `GuildStaff.cs`, `GuildCatalogIds.cs`, `CreateStaffRequest.cs`, `UpdateStaffRequest.cs`, `GuildStaffResponse.cs`, `ErrorCodes.cs`, `GuildSheetService.cs`, `GuildStaffTab.razor`, `GuildSheet.razor`; create `TrainingReference.cs` (Área list only for now); modify `GuildStaffTests.cs`.

**Interfaces:**
- Produces: `GuildStaff.DedicatedCharacterSheetId` (`Guid?`), `GuildStaff.DedicatedSkillArea` (`string?`); `TrainingReference.AreaNames` (`IReadOnlyList<string>`, 11 entries); `ErrorCodes.Guild.StaffDedicationInvalid`.

- [ ] **Step 1: `TrainingReference.AreaNames`**

`src/Ruptura.Shared/CharacterSheets/TrainingReference.cs`:
```csharp
namespace Ruptura.Shared.CharacterSheets;

// GDD §6.4/§6.5 — the 11 Área de Perícia values SkillCatalogData.Area actually uses (the
// seeded catalog treats the 4 Combate sub-areas as distinct, unlike §6.5's table row which
// groups them into one line). Reused by: the Guild Staff tab's "dedicate an Instrutor" Área
// picker (server-side dedication validation) and, from TrainingCalculator (Task 2), the
// Área→Instalação bonus mapping.
public static class TrainingReference
{
    public static readonly IReadOnlyList<string> AreaNames =
    [
        "Combate — Armas", "Combate — Defesa", "Combate Corporal", "Combate à Distância",
        "Exploração", "Conhecimento", "Cura", "Artesanato", "Alquimia", "Magia", "Social"
    ];
}
```

- [ ] **Step 2: `GuildStaff` entity fields**

In `src/Ruptura.Domain/Entities/GuildStaff.cs`, add after `Morale`:
```csharp
    // Dedicating an Instrutor to a character+área grants that character's Skill Training
    // calculator (Ruptura.Shared.CharacterSheets.TrainingProjection) a +1 pts/day Bônus de
    // Instrutor (GDD §6.4) when training a skill in that Área. Bare Guid?/string? — no FK,
    // matching the repo's soft-reference convention.
    public Guid? DedicatedCharacterSheetId { get; set; }
    public string? DedicatedSkillArea { get; set; }        // one of TrainingReference.AreaNames
```

- [ ] **Step 3: Migration**

Run:
```bash
dotnet ef migrations add AddGuildStaffDedication \
  --project src/Ruptura.Infrastructure --startup-project src/Ruptura.API
```
Inspect the generated migration — it should add two nullable columns (`dedicated_character_sheet_id uuid`, `dedicated_skill_area text` or similar, per the repo's existing naming convention) to the guild staff table, no data loss, no config changes needed (`GuildStaffConfiguration.cs` needs no edit — plain nullable scalars need no fluent config).

- [ ] **Step 4: `GuildCatalogIds` — add the 6 installations Task 2 will need**

In `src/Ruptura.Shared/Guilds/GuildCatalogIds.cs`, add after `Biblioteca`:
```csharp
    public static readonly Guid Oficina = Guid.Parse("d0000000-0000-0000-0000-000000000006");
    public static readonly Guid Enfermaria = Guid.Parse("d0000000-0000-0000-0000-000000000008");
    public static readonly Guid LaboratorioArcano = Guid.Parse("d0000000-0000-0000-0000-000000000009");
    public static readonly Guid AcademiaMilitar = Guid.Parse("d0000000-0000-0000-0000-000000000010");
    public static readonly Guid JardimAlquimico = Guid.Parse("d0000000-0000-0000-0000-000000000011");
    public static readonly Guid TorreDosMagos = Guid.Parse("d0000000-0000-0000-0000-000000000016");
```
(These GUIDs are verified against `CatalogSeedData.Installations` — row numbers 6/8/9/10/11/16.)

- [ ] **Step 5: `ErrorCodes.Guild.StaffDedicationInvalid`**

In `src/Ruptura.Application/Common/ErrorCodes.cs`, inside `public static class Guild`, add after `StaffKindInvalid`:
```csharp
        public const string StaffDedicationInvalid = "Guild.StaffDedicationInvalid";
```

- [ ] **Step 6: resx entries**

`src/Ruptura.API/Resources/SharedResources.resx`, add a line right after the `Guild.StaffKindInvalid` entry (search for it):
```xml
  <data name="Guild.StaffDedicationInvalid"><value>The Instrutor dedication is invalid.</value></data>
```
`src/Ruptura.API/Resources/SharedResources.pt-BR.resx`, same position:
```xml
  <data name="Guild.StaffDedicationInvalid"><value>A dedicação do Instrutor é inválida.</value></data>
```

- [ ] **Step 7: DTOs — add the 2 fields to Create/Update/Response**

`src/Ruptura.Shared/Guilds/CreateStaffRequest.cs`, add after `Morale`:
```csharp
    public Guid? DedicatedCharacterSheetId { get; set; }
    public string? DedicatedSkillArea { get; set; }
```
Same addition to `UpdateStaffRequest.cs` and to `GuildStaffResponse.cs` (all three files have an identical `Morale` property to anchor after).

- [ ] **Step 8: Validate + wire in `GuildSheetService`**

`GuildSheetService`'s primary constructor gains `ICharacterSheetRepository sheetRepo` (add after `ICraftingOrderRepository craftingRepo`, before `IGuildStatsCalculator calculator`). Add `using Ruptura.Shared.CharacterSheets;` to the file's usings.

Add this private helper right above `MapStaff`:
```csharp
    private async Task<Result> ValidateStaffDedicationAsync(
        Guid campaignId, Guid? characterSheetId, string? skillArea, CancellationToken ct)
    {
        if (skillArea is not null && !TrainingReference.AreaNames.Contains(skillArea))
            return Result.Failure(ErrorCodes.Guild.StaffDedicationInvalid);

        if (characterSheetId is { } id)
        {
            var target = await sheetRepo.GetByIdAsync(id, ct);
            if (target is null || target.CampaignId != campaignId)
                return Result.Failure(ErrorCodes.Guild.StaffDedicationInvalid);
        }

        return Result.Success();
    }
```

In `AddStaffAsync`, right after the existing `Kind` validation (`if (!Enum.TryParse<GuildStaffKind>(...` block) and before constructing `staff`, add:
```csharp
        var dedicationCheck = await ValidateStaffDedicationAsync(
            campaignId, request.DedicatedCharacterSheetId, request.DedicatedSkillArea, ct);
        if (dedicationCheck.IsFailure)
            return Result.Failure<GuildStaffResponse>(dedicationCheck.Error!);
```
Then add to the `new GuildStaff { ... }` initializer, after `Morale = request.Morale`:
```csharp
            DedicatedCharacterSheetId = request.DedicatedCharacterSheetId,
            DedicatedSkillArea = request.DedicatedSkillArea
```

In `UpdateStaffAsync`, add the identical validation call right after its own `Kind` validation and before the `staff is null` lookup guard, then after `staff.Morale = request.Morale;` add:
```csharp
        staff.DedicatedCharacterSheetId = request.DedicatedCharacterSheetId;
        staff.DedicatedSkillArea = request.DedicatedSkillArea;
```

In `MapStaff`, add after `Morale = s.Morale`:
```csharp
        DedicatedCharacterSheetId = s.DedicatedCharacterSheetId,
        DedicatedSkillArea = s.DedicatedSkillArea
```

- [ ] **Step 9: Integration tests**

Append to `tests/Ruptura.IntegrationTests/Guilds/GuildStaffTests.cs`:
```csharp
    [Fact]
    public async Task AddWorker_WithDedication_PersistsBothFields()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        await GetGuildAsync(client, campaign.Id);

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var otherPlayer = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = otherPlayer.User.Id });
        AuthHelper.SetBearerToken(client, gmToken);
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = otherPlayer.User.Id, CharacterName = "Trainee" });
        var sheetId = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!.Id;

        var request = new CreateStaffRequest
        {
            Kind = "Worker", TypeOrRanking = GuildStaffTypes.Instrutor, Name = "Mestre Aldo",
            DailySalary = 5, IsActive = true,
            DedicatedCharacterSheetId = sheetId, DedicatedSkillArea = "Magia"
        };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/staff", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildStaffResponse>>())!.Data!;
        body.DedicatedCharacterSheetId.Should().Be(sheetId);
        body.DedicatedSkillArea.Should().Be("Magia");
    }

    [Fact]
    public async Task AddWorker_WithUnknownArea_Returns400()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        await GetGuildAsync(client, campaign.Id);

        var request = new CreateStaffRequest
        {
            Kind = "Worker", TypeOrRanking = GuildStaffTypes.Instrutor, Name = "Mestre Aldo",
            DailySalary = 5, IsActive = true, DedicatedSkillArea = "Culinária Extrema"
        };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/staff", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddWorker_WithCharacterFromAnotherCampaign_Returns400()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        await GetGuildAsync(client, campaign.Id);

        // A sheet from a completely different campaign (fresh Guid never granted here).
        var request = new CreateStaffRequest
        {
            Kind = "Worker", TypeOrRanking = GuildStaffTypes.Instrutor, Name = "Mestre Aldo",
            DailySalary = 5, IsActive = true,
            DedicatedCharacterSheetId = Guid.NewGuid(), DedicatedSkillArea = "Magia"
        };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/staff", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
```
Add `using Ruptura.Shared.CharacterSheets;` to the test file's usings (for `GrantCharacterSheetRequest`/`CharacterSheetResponse`).

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildStaffTests` → all pass (re-run once if the Serilog flake hits).

- [ ] **Step 10: `GuildErrorCodeLocalizationTests` still passes**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~GuildErrorCodeLocalizationTests` → pass (the new `StaffDedicationInvalid` const is picked up automatically by the reflection-based test; Step 6 already added both resx entries).

- [ ] **Step 11: GM-only dedication picker UI**

In `src/Ruptura.Web/Pages/GuildSheet.razor`, change the local `var isGm = ...` (inside `OnInitializedAsync`) into a field so it can be passed to child tabs. Add near the other private fields:
```csharp
    private bool _isGm;
```
Replace `var isGm = state.User.FindFirst("role")?.Value == "GameMaster";` with:
```csharp
        _isGm = state.User.FindFirst("role")?.Value == "GameMaster";
```
and update the one other use of `isGm` in that method (the `campaignsCrumb` ternary) to `_isGm`. Then update the `GuildStaffTab` mount:
```razor
<GuildStaffTab CampaignId="CampaignId" Staff="_guild.Staff" OnChanged="RefreshStaffAsync" IsGameMaster="_isGm" />
```

In `src/Ruptura.Web/Pages/GuildStaffTab.razor`:
- Add `@using Ruptura.Shared.CharacterSheets` and `@inject ICharacterSheetClientService SheetService` to the top imports.
- Add a new parameter: `[Parameter] public bool IsGameMaster { get; set; }`.
- Add two fields: `private List<CharacterSheetResponse> _campaignSheets = [];` and load them once when `IsGameMaster` is true:
```csharp
    protected override async Task OnParametersSetAsync()
    {
        if (IsGameMaster && _campaignSheets.Count == 0)
        {
            var result = await SheetService.GetByCampaignAsync(CampaignId);
            _campaignSheets = result?.Data?.ToList() ?? [];
        }
    }
```
- Add to the `StaffForm` private class: `public Guid? DedicatedCharacterSheetId { get; set; }` and `public string? DedicatedSkillArea { get; set; }`.
- In `StartCreate`/`StartEdit`, leave the new fields at their defaults / copy them from `member` respectively (`DedicatedCharacterSheetId = member.DedicatedCharacterSheetId, DedicatedSkillArea = member.DedicatedSkillArea` added to `StartEdit`'s object initializer).
- In the editing form markup, right after the `IsActive` `form-check` div, add (only rendered for Instrutor + GM):
```razor
            @if (_editing.Kind == "Worker" && _editing.TypeOrRanking == GuildStaffTypes.Instrutor && IsGameMaster)
            {
                <div style="display:flex;flex-wrap:wrap;gap:1rem">
                    <div>
                        <label class="form-label">@L["Guild.Staff.DedicatedCharacter"]</label>
                        <select class="form-select" value="@(_editing.DedicatedCharacterSheetId?.ToString() ?? "")" @onchange="OnDedicatedCharacterChanged">
                            <option value="">@L["Guild.Staff.DedicatedCharacter.None"]</option>
                            @foreach (var sheet in _campaignSheets)
                            {
                                <option value="@sheet.Id">@sheet.CharacterName</option>
                            }
                        </select>
                    </div>
                    <div>
                        <label class="form-label">@L["Guild.Staff.DedicatedArea"]</label>
                        <select class="form-select" @bind="_editing.DedicatedSkillArea">
                            <option value="">@L["Guild.Staff.DedicatedArea.None"]</option>
                            @foreach (var area in TrainingReference.AreaNames)
                            {
                                <option value="@area">@area</option>
                            }
                        </select>
                    </div>
                </div>
            }
```
> The character picker uses plain `value`/`@onchange` rather than `@bind`, matching this file's own `Kind`/`TypeOrRanking` selects just above — `@bind` on a `<select>` needs `BindConverter` support for the bound type, which is not guaranteed for `Guid?`; `string?` (the Área select) is always supported, so that one keeps plain `@bind`.

Add this handler next to `OnKindChanged`/`OnTypeChanged`:
```csharp
    private void OnDedicatedCharacterChanged(ChangeEventArgs e)
    {
        if (_editing is null) return;
        var value = e.Value?.ToString();
        _editing.DedicatedCharacterSheetId = string.IsNullOrEmpty(value) ? null : Guid.Parse(value);
    }
```

- In `SaveAsync`, add `DedicatedCharacterSheetId = _editing.DedicatedCharacterSheetId, DedicatedSkillArea = _editing.DedicatedSkillArea` to both the `CreateStaffRequest` and `UpdateStaffRequest` object initializers.

- [ ] **Step 12: Web resx for the 4 new UI strings**

`src/Ruptura.Web/Resources/AppStrings.resx`, add after the existing `Guild.Staff.Active` entry (search for it):
```xml
  <data name="Guild.Staff.DedicatedCharacter"><value>Dedicated to character</value></data>
  <data name="Guild.Staff.DedicatedCharacter.None"><value>(none)</value></data>
  <data name="Guild.Staff.DedicatedArea"><value>Dedicated Área</value></data>
  <data name="Guild.Staff.DedicatedArea.None"><value>(none)</value></data>
```
`src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, same position:
```xml
  <data name="Guild.Staff.DedicatedCharacter"><value>Dedicado ao personagem</value></data>
  <data name="Guild.Staff.DedicatedCharacter.None"><value>(nenhum)</value></data>
  <data name="Guild.Staff.DedicatedArea"><value>Área dedicada</value></data>
  <data name="Guild.Staff.DedicatedArea.None"><value>(nenhuma)</value></data>
```

- [ ] **Step 13: Build, run full suite, commit**

Run: `dotnet build && dotnet test`.
```bash
git add src/Ruptura.Domain/Entities/GuildStaff.cs src/Ruptura.Infrastructure/Data/Migrations \
  src/Ruptura.Shared/Guilds src/Ruptura.Shared/CharacterSheets/TrainingReference.cs \
  src/Ruptura.Application/Common/ErrorCodes.cs src/Ruptura.Infrastructure/Services/GuildSheetService.cs \
  src/Ruptura.Web/Pages/GuildStaffTab.razor src/Ruptura.Web/Pages/GuildSheet.razor \
  src/Ruptura.API/Resources src/Ruptura.Web/Resources \
  tests/Ruptura.IntegrationTests/Guilds/GuildStaffTests.cs
git commit -m "feat: add GM-only Instrutor dedication to GuildStaff for character training bonus

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: `ITrainingCalculator` (pure) + full Área→Instalação mapping

**Files:** modify `TrainingReference.cs`; create `ITrainingCalculator.cs`, `TrainingCalculator.cs`, `TrainingProjection.cs`; test `TrainingCalculatorTests.cs`.

**Interfaces:**
- Consumes: `GuildBuilding { CatalogEntryId, Level, IsActive }`, `GuildStaff { Kind, TypeOrRanking, IsActive, DedicatedCharacterSheetId, DedicatedSkillArea }` (Task 1).
- Produces: `TrainingProjection`; `ITrainingCalculator.Project(Guid skillCatalogEntryId, string skillName, string skillArea, int currentPoints, IReadOnlyList<GuildBuilding> buildings, IReadOnlyList<GuildStaff> staff, Guid characterSheetId, int days, string correlation) → TrainingProjection`.

- [ ] **Step 1: `TrainingProjection` DTO**

`src/Ruptura.Shared/CharacterSheets/TrainingProjection.cs`:
```csharp
namespace Ruptura.Shared.CharacterSheets;

// Server-computed preview of GDD §6.4's Pontos de Treinamento/dia formula. The client shows
// this and, on Aplicar, mutates Data.Skills directly (design spec §5.3) — this DTO is
// display-only, never posted back.
public class TrainingProjection
{
    public Guid SkillCatalogEntryId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
    public double PointsPerDay { get; set; }
    public int Days { get; set; }
    public string Correlation { get; set; } = string.Empty;
    public int PointsToAdd { get; set; }
    public int ProjectedTotalPoints { get; set; }
}
```

- [ ] **Step 2: Write the failing unit tests**

`tests/Ruptura.UnitTests/Application/TrainingCalculatorTests.cs`:
```csharp
using FluentAssertions;
using Ruptura.Application.Services;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.Guilds;
using Xunit;

namespace Ruptura.UnitTests.Application;

public class TrainingCalculatorTests
{
    private readonly TrainingCalculator _calc = new();
    private static readonly Guid SkillId = Guid.NewGuid();
    private static readonly Guid CharacterId = Guid.NewGuid();

    private static GuildBuilding Building(Guid catalogId, int level, bool active = true) =>
        new() { Id = Guid.NewGuid(), CatalogEntryId = catalogId, Level = level, IsActive = active };

    private static GuildStaff Instrutor(Guid? dedicatedCharacter, string? dedicatedArea, bool active = true) =>
        new()
        {
            Id = Guid.NewGuid(), Kind = GuildStaffKind.Worker, TypeOrRanking = GuildStaffTypes.Instrutor,
            IsActive = active, DedicatedCharacterSheetId = dedicatedCharacter, DedicatedSkillArea = dedicatedArea
        };

    [Fact]
    public void CampoDeTreinamentoII_MediaCorrelacao_SemInstrutor_Gives2PerDay()
    {
        // GDD §6.4 worked example: (1 + 1 + 0) × 1.0 = 2.
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 2) };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 15, buildings, [], CharacterId, 10, "Media");
        p.PointsPerDay.Should().Be(2);
        p.PointsToAdd.Should().Be(20);
        p.ProjectedTotalPoints.Should().Be(35);
    }

    [Fact]
    public void CampoDeTreinamentoV_InstrutorDedicado_AltaCorrelacao_GivesApprox6_75PerDay()
    {
        // GDD §6.4 worked example: (1 + 2.5 + 1) × 1.5 = 6.75.
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 5) };
        var staff = new List<GuildStaff> { Instrutor(CharacterId, "Combate — Armas") };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 20, buildings, staff, CharacterId, 1, "Alta");
        p.PointsPerDay.Should().Be(6.75);
    }

    [Fact]
    public void AcademiaMilitar_DoublesTheInstallationBonus_ForCombate()
    {
        var buildings = new List<GuildBuilding>
        {
            Building(GuildCatalogIds.CampoDeTreinamento, 5), Building(GuildCatalogIds.AcademiaMilitar, 3)
        };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 15, buildings, [], CharacterId, 1, "Media");
        // Academia Militar (avançada) wins: Level 3 × 1.0 = 3, not Campo's 5 × 0.5 = 2.5.
        p.PointsPerDay.Should().Be(4); // 1 + 3
    }

    [Fact]
    public void Exploracao_HalvesTheNormalInstallationBonusAgain()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 4) };
        var p = _calc.Project(SkillId, "Rastreamento", "Exploração", 15, buildings, [], CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(2); // 1 + (4 × 0.25)
    }

    [Fact]
    public void Alquimia_FallsBackToOficina_WhenJardimAlquimicoNotBuilt()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.Oficina, 2) };
        var p = _calc.Project(SkillId, "Poções", "Alquimia", 15, buildings, [], CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(2); // 1 + (2 × 0.5)
    }

    [Fact]
    public void Social_GivesNoInstallationBonus_UnlessSkillIsLideranca()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.AcademiaMilitar, 4) };
        var diplomacia = _calc.Project(SkillId, "Diplomacia", "Social", 15, buildings, [], CharacterId, 1, "Media");
        diplomacia.PointsPerDay.Should().Be(1); // Base only

        var lideranca = _calc.Project(SkillId, "Liderança", "Social", 15, buildings, [], CharacterId, 1, "Media");
        lideranca.PointsPerDay.Should().Be(5); // 1 + (4 × 1.0)
    }

    [Fact]
    public void UnmappedArea_GivesBaseOnly()
    {
        var p = _calc.Project(SkillId, "Homebrew", "Coisa Nova", 15, [], [], CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(1);
    }

    // GDD §6.5 "Sem Treinamento" tier (Points 0-9): Instalação/Instrutor are ignored entirely,
    // so the rate is just 1 × MultCorrelação, clamped by the "Teto por Correlação" table
    // (Nenhuma=1/Baixa=2/Média=3/Alta=5 — see SemTreinamentoCeiling). A built Campo de
    // Treinamento V is present here specifically to prove it has NO effect in this tier.
    // Note the ceiling never actually binds for any of the 4 correlations (1×mult is always
    // strictly below its own ceiling: 0.25<1, 0.5<2, 1.0<3, 1.5<5) — the MIN is implemented
    // anyway per the project convention of reproducing FECHADO tables literally, but there is
    // no reachable input that exercises the ceiling actually clamping something.
    [Theory]
    [InlineData("Nenhuma", 0.25)]
    [InlineData("Baixa", 0.5)]
    [InlineData("Media", 1.0)]
    [InlineData("Alta", 1.5)]
    public void SemTreinamento_IgnoresInstallationBonus_UsesCorrelationMultiplierOnly(string correlation, double expectedRate)
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 5) };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 5, buildings, [], CharacterId, 1, correlation);
        p.PointsPerDay.Should().Be(expectedRate);
    }

    [Fact]
    public void NenhumaCorrelacao_BecomesNormalProgression_AtFiftyPoints()
    {
        var below50 = _calc.Project(SkillId, "X", "Magia", 49, [], [], CharacterId, 1, "Nenhuma");
        below50.PointsPerDay.Should().Be(0.25);

        var at50 = _calc.Project(SkillId, "X", "Magia", 50, [], [], CharacterId, 1, "Nenhuma");
        at50.PointsPerDay.Should().Be(1);
    }

    [Fact]
    public void InactiveInstallation_GivesNoBonus()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 5, active: false) };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 15, buildings, [], CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(1);
    }

    [Fact]
    public void InstrutorDedicatedToDifferentCharacterOrArea_GivesNoBonus()
    {
        var otherCharacter = Guid.NewGuid();
        var staff = new List<GuildStaff>
        {
            Instrutor(otherCharacter, "Combate — Armas"),
            Instrutor(CharacterId, "Magia")
        };
        var p = _calc.Project(SkillId, "Espadas", "Combate — Armas", 15, [], staff, CharacterId, 1, "Media");
        p.PointsPerDay.Should().Be(1);
    }

    [Fact]
    public void PointsToAdd_FloorsFractionalTotals()
    {
        var buildings = new List<GuildBuilding> { Building(GuildCatalogIds.CampoDeTreinamento, 1) };
        // (1 + 0.5) × 1.0 = 1.5/day × 3 days = 4.5 → floors to 4.
        var p = _calc.Project(SkillId, "X", "Combate — Armas", 15, buildings, [], CharacterId, 3, "Media");
        p.PointsToAdd.Should().Be(4);
    }
}
```

- [ ] **Step 3: Run → fail** (`TrainingCalculator` doesn't exist).

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~TrainingCalculatorTests` → compile error.

- [ ] **Step 4: Expand `TrainingReference` with the Área→Instalação mapping**

Replace the content of `src/Ruptura.Shared/CharacterSheets/TrainingReference.cs` with:
```csharp
using Ruptura.Shared.Guilds;

namespace Ruptura.Shared.CharacterSheets;

// GDD §6.4/§6.5 — the 11 Área de Perícia values SkillCatalogData.Area actually uses (the
// seeded catalog treats the 4 Combate sub-areas as distinct, unlike §6.5's table row which
// groups them into one line). Reused by: the Guild Staff tab's "dedicate an Instrutor" Área
// picker (server-side dedication validation, Task 1) and TrainingCalculator's installation
// bonus lookup (below).
public static class TrainingReference
{
    public static readonly IReadOnlyList<string> AreaNames =
    [
        "Combate — Armas", "Combate — Defesa", "Combate Corporal", "Combate à Distância",
        "Exploração", "Conhecimento", "Cura", "Artesanato", "Alquimia", "Magia", "Social"
    ];

    private static readonly TrainingInstallationMapping Combat =
        new(GuildCatalogIds.CampoDeTreinamento, GuildCatalogIds.AcademiaMilitar);

    // Área → installation mapping (GDD §6.5). "Social" is deliberately absent — it has no
    // installation at all in the normal case (handled as a special case, see spec §4/Key
    // Decision #5, alongside the Liderança-only Academia Militar exception, both implemented
    // directly in TrainingCalculator rather than through this table).
    public static readonly IReadOnlyDictionary<string, TrainingInstallationMapping> InstallationByArea =
        new Dictionary<string, TrainingInstallationMapping>(StringComparer.OrdinalIgnoreCase)
        {
            ["Combate — Armas"] = Combat,
            ["Combate — Defesa"] = Combat,
            ["Combate Corporal"] = Combat,
            ["Combate à Distância"] = Combat,
            ["Exploração"] = new TrainingInstallationMapping(GuildCatalogIds.CampoDeTreinamento, null, HalvedAgain: true),
            ["Conhecimento"] = new TrainingInstallationMapping(GuildCatalogIds.Biblioteca, null),
            ["Cura"] = new TrainingInstallationMapping(GuildCatalogIds.Enfermaria, null),
            ["Artesanato"] = new TrainingInstallationMapping(GuildCatalogIds.Oficina, null),
            ["Alquimia"] = new TrainingInstallationMapping(GuildCatalogIds.JardimAlquimico, null, FallbackId: GuildCatalogIds.Oficina),
            ["Magia"] = new TrainingInstallationMapping(GuildCatalogIds.LaboratorioArcano, GuildCatalogIds.TorreDosMagos)
        };
}

// NormalId: installation whose Level×0.5 is the normal Bônus de Instalação. AdvancedId: the
// "avançada" installation (Level×1 instead of ×0.5) when built and active — takes priority
// over NormalId when present. HalvedAgain: Exploração's GDD rule (Level×0.25 off NormalId).
// FallbackId: Alquimia's "Oficina, se Jardim Alquímico ainda não construído" rule — used only
// when NormalId resolves to Level 0.
public readonly record struct TrainingInstallationMapping(
    Guid NormalId, Guid? AdvancedId, bool HalvedAgain = false, Guid? FallbackId = null);
```

- [ ] **Step 5: `ITrainingCalculator` interface**

`src/Ruptura.Application/Interfaces/ITrainingCalculator.cs`:
```csharp
using Ruptura.Domain.Entities;
using Ruptura.Shared.CharacterSheets;

namespace Ruptura.Application.Interfaces;

public interface ITrainingCalculator
{
    TrainingProjection Project(
        Guid skillCatalogEntryId, string skillName, string skillArea, int currentPoints,
        IReadOnlyList<GuildBuilding> buildings, IReadOnlyList<GuildStaff> staff,
        Guid characterSheetId, int days, string correlation);
}
```

- [ ] **Step 6: Implement `TrainingCalculator`**

`src/Ruptura.Application/Services/TrainingCalculator.cs`:
```csharp
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Guilds;

namespace Ruptura.Application.Services;

public class TrainingCalculator : ITrainingCalculator
{
    // GDD §6.5 "Tabela de Treinamento em Sem Treinamento" — teto de pontos/dia while Points < 10.
    private static readonly IReadOnlyDictionary<string, double> SemTreinamentoCeiling = new Dictionary<string, double>
    {
        ["Nenhuma"] = 1, ["Baixa"] = 2, ["Media"] = 3, ["Alta"] = 5
    };

    public TrainingProjection Project(
        Guid skillCatalogEntryId, string skillName, string skillArea, int currentPoints,
        IReadOnlyList<GuildBuilding> buildings, IReadOnlyList<GuildStaff> staff,
        Guid characterSheetId, int days, string correlation)
    {
        var perDay = RatePerDay(skillArea, skillName, currentPoints, buildings, staff, characterSheetId, correlation);
        var toAdd = (int)Math.Floor(perDay * days);

        return new TrainingProjection
        {
            SkillCatalogEntryId = skillCatalogEntryId,
            SkillName = skillName,
            CurrentPoints = currentPoints,
            PointsPerDay = perDay,
            Days = days,
            Correlation = correlation,
            PointsToAdd = toAdd,
            ProjectedTotalPoints = currentPoints + toAdd
        };
    }

    private static double RatePerDay(
        string area, string skillName, int currentPoints, IReadOnlyList<GuildBuilding> buildings,
        IReadOnlyList<GuildStaff> staff, Guid characterSheetId, string correlation)
    {
        var multiplier = CorrelationMultiplier(currentPoints, correlation);

        if (currentPoints < 10)
        {
            var ceiling = SemTreinamentoCeiling.GetValueOrDefault(correlation, 1);
            return Math.Min(1 * multiplier, ceiling);
        }

        var installationBonus = InstallationBonus(area, skillName, buildings);
        var instructorBonus = HasDedicatedInstructor(area, staff, characterSheetId) ? 1 : 0;
        return (1 + installationBonus + instructorBonus) * multiplier;
    }

    private static double CorrelationMultiplier(int currentPoints, string correlation) => correlation switch
    {
        "Alta" => 1.5,
        "Baixa" => 0.5,
        "Nenhuma" => currentPoints < 50 ? 0.25 : 1.0,
        _ => 1.0 // "Media" and any unrecognized value (the service layer validates before calling) default here.
    };

    private static double InstallationBonus(string area, string skillName, IReadOnlyList<GuildBuilding> buildings)
    {
        // GDD §6.5 — Social has no normal installation at all; only "Liderança" gets the
        // Academia Militar avançada bonus (unlike every other Área's advanced tier, which
        // applies to the whole Área regardless of the specific skill).
        if (AreaEquals(area, "Social"))
            return AreaEquals(skillName, "Liderança") ? LevelOf(buildings, GuildCatalogIds.AcademiaMilitar) : 0;

        if (!TrainingReference.InstallationByArea.TryGetValue(area.Trim(), out var mapping))
            return 0;

        if (mapping.AdvancedId is { } advancedId)
        {
            var advancedLevel = LevelOf(buildings, advancedId);
            if (advancedLevel > 0) return advancedLevel * 1.0;
        }

        var normalLevel = LevelOf(buildings, mapping.NormalId);
        if (normalLevel == 0 && mapping.FallbackId is { } fallbackId)
            normalLevel = LevelOf(buildings, fallbackId);

        return normalLevel * (mapping.HalvedAgain ? 0.25 : 0.5);
    }

    private static int LevelOf(IReadOnlyList<GuildBuilding> buildings, Guid catalogEntryId) =>
        buildings.FirstOrDefault(b => b.CatalogEntryId == catalogEntryId && b.IsActive)?.Level ?? 0;

    private static bool HasDedicatedInstructor(
        string area, IReadOnlyList<GuildStaff> staff, Guid characterSheetId) =>
        staff.Any(s =>
            s.IsActive && s.Kind == GuildStaffKind.Worker && s.TypeOrRanking == GuildStaffTypes.Instrutor &&
            s.DedicatedCharacterSheetId == characterSheetId && AreaEquals(s.DedicatedSkillArea, area));

    // Free-text Área matching, case/whitespace-insensitive — same reasoning as
    // CharacterStatsCalculator.CategoryIs (a GM-typed or catalog-authored value's casing must
    // never silently zero out a mechanic).
    private static bool AreaEquals(string? a, string b) =>
        a is not null && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
}
```
> `InstallationByArea` is built with `StringComparer.OrdinalIgnoreCase` (Step 4) so `TryGetValue` is already case/whitespace-tolerant (after `.Trim()`) — a differently-cased homebrew skill's Area string still resolves instead of silently falling through to "unmapped → Base only".

- [ ] **Step 7: Run → pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~TrainingCalculatorTests` → all pass.

- [ ] **Step 8: Register the calculator**

In `src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs`, add next to the other pure calculators (after `IInterludeCalculator`):
```csharp
        services.AddSingleton<ITrainingCalculator, TrainingCalculator>(); // pure & stateless
```

- [ ] **Step 9: Build + commit**

Run: `dotnet build` (clean).
```bash
git add src/Ruptura.Shared/CharacterSheets src/Ruptura.Application/Interfaces/ITrainingCalculator.cs \
  src/Ruptura.Application/Services/TrainingCalculator.cs src/Ruptura.Infrastructure/Extensions/InfrastructureExtensions.cs \
  tests/Ruptura.UnitTests/Application/TrainingCalculatorTests.cs
git commit -m "feat: add pure TrainingCalculator implementing GDD §6.4 skill training formula

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Preview endpoint + integration tests

**Files:** modify `ErrorCodes.cs`, `ICharacterSheetService.cs`, `CharacterSheetService.cs`, `CharacterSheetController.cs`, `SharedResources*.resx`; create `CharacterSheetTrainingTests.cs`, `CharacterSheetErrorCodeLocalizationTests.cs`.

**Interfaces:**
- Produces: `ICharacterSheetService.PreviewTrainingAsync(Guid callerId, Guid sheetId, Guid skillCatalogEntryId, int days, string correlation, CancellationToken ct = default) → Task<Result<TrainingProjection>>`.

- [ ] **Step 1: Error codes**

In `src/Ruptura.Application/Common/ErrorCodes.cs`, inside `public static class CharacterSheet`, add after `OnlyGameMasterCanChangeStatus`:
```csharp
        public const string SkillNotFound = "CharacterSheet.SkillNotFound";
        public const string TrainingDaysInvalid = "CharacterSheet.TrainingDaysInvalid";
        public const string CorrelationInvalid = "CharacterSheet.CorrelationInvalid";
```

- [ ] **Step 2: resx entries**

`src/Ruptura.API/Resources/SharedResources.resx`, add after the existing `CharacterSheet.OnlyGameMasterCanChangeStatus` entry (search for it):
```xml
  <data name="CharacterSheet.SkillNotFound"><value>The skill was not found.</value></data>
  <data name="CharacterSheet.TrainingDaysInvalid"><value>The number of training days is invalid.</value></data>
  <data name="CharacterSheet.CorrelationInvalid"><value>The correlation level is invalid.</value></data>
```
`src/Ruptura.API/Resources/SharedResources.pt-BR.resx`, same position:
```xml
  <data name="CharacterSheet.SkillNotFound"><value>A perícia não foi encontrada.</value></data>
  <data name="CharacterSheet.TrainingDaysInvalid"><value>O número de dias de treinamento é inválido.</value></data>
  <data name="CharacterSheet.CorrelationInvalid"><value>O nível de correlação é inválido.</value></data>
```

- [ ] **Step 3: `CharacterSheetErrorCodeLocalizationTests`**

`tests/Ruptura.IntegrationTests/Controllers/CharacterSheetErrorCodeLocalizationTests.cs` (mirrors `GuildErrorCodeLocalizationTests.cs` exactly):
```csharp
using System.Globalization;
using System.Reflection;
using System.Resources;
using FluentAssertions;
using Ruptura.API.Resources;
using Ruptura.Application.Common;

namespace Ruptura.IntegrationTests.Controllers;

public class CharacterSheetErrorCodeLocalizationTests
{
    private static readonly ResourceManager Resources =
        new("Ruptura.API.Resources.SharedResources", typeof(SharedResources).Assembly);

    public static IEnumerable<object[]> CharacterSheetErrorCodes() =>
        typeof(ErrorCodes.CharacterSheet)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => new object[] { (string)f.GetValue(null)! });

    [Theory]
    [MemberData(nameof(CharacterSheetErrorCodes))]
    public void EveryCharacterSheetErrorCode_Resolves_InEnglishAndPortuguese(string code)
    {
        var english = Resources
            .GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: false)!
            .GetString(code);
        english.Should().NotBeNullOrWhiteSpace(
            "error code '{0}' must have an English (neutral) resource string", code);

        var portuguese = Resources
            .GetResourceSet(CultureInfo.GetCultureInfo("pt-BR"), createIfNotExists: true, tryParents: false)!
            .GetString(code);
        portuguese.Should().NotBeNullOrWhiteSpace(
            "error code '{0}' must have a pt-BR resource string", code);
    }
}
```

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~CharacterSheetErrorCodeLocalizationTests` → pass.

- [ ] **Step 4: Write the failing integration tests**

`tests/Ruptura.IntegrationTests/Controllers/CharacterSheetTrainingTests.cs`:
```csharp
using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class CharacterSheetTrainingTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, CampaignResponse Campaign, Guid SheetId, string PlayerToken, string GmToken)>
        SetUpCharacterAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Training Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = "Trainee" });
        var sheetId = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!.Id;

        return (client, campaign, sheetId, player.AccessToken, gm.AccessToken);
    }

    private async Task<Guid> CreateSkillAsync(HttpClient client, Guid campaignId, string name, string area, string gmToken)
    {
        AuthHelper.SetBearerToken(client, gmToken);
        var response = await client.PostAsJsonAsync("api/catalog", new
        {
            CampaignId = campaignId, Type = "Skill", Name = name,
            DataJson = $"{{\"Area\":\"{area}\",\"RelatedAttribute\":\"Controle\"}}"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>();
        return body!.Data!.Id;
    }

    [Fact]
    public async Task Preview_ForKnownSkill_UsesGuildInstallationBonus()
    {
        var (client, campaign, sheetId, playerToken, gmToken) = await SetUpCharacterAsync();
        var skillId = await CreateSkillAsync(client, campaign.Id, "Espadas", "Combate — Armas", gmToken);

        AuthHelper.SetBearerToken(client, gmToken);
        await client.GetAsync($"api/campaigns/{campaign.Id}/guild"); // get-or-create
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.CampoDeTreinamento, Level = 2, IsActive = true });

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheetId}/training/preview?skillCatalogEntryId={skillId}&days=10&correlation=Media");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TrainingProjection>>())!.Data!;
        body.CurrentPoints.Should().Be(0);
        body.PointsPerDay.Should().Be(2);
        body.PointsToAdd.Should().Be(20);
    }

    [Fact]
    public async Task Preview_WithNoGuildYet_UsesBaseRateOnly()
    {
        var (client, campaign, sheetId, playerToken, gmToken) = await SetUpCharacterAsync();
        var skillId = await CreateSkillAsync(client, campaign.Id, "Espadas", "Combate — Armas", gmToken);

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheetId}/training/preview?skillCatalogEntryId={skillId}&days=1&correlation=Media");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TrainingProjection>>())!.Data!;
        body.PointsPerDay.Should().Be(1);
    }

    [Fact]
    public async Task Preview_WithInvalidDays_Returns400()
    {
        var (client, campaign, sheetId, playerToken, gmToken) = await SetUpCharacterAsync();
        var skillId = await CreateSkillAsync(client, campaign.Id, "Espadas", "Combate — Armas", gmToken);

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheetId}/training/preview?skillCatalogEntryId={skillId}&days=0&correlation=Media");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Preview_WithInvalidCorrelation_Returns400()
    {
        var (client, campaign, sheetId, playerToken, gmToken) = await SetUpCharacterAsync();
        var skillId = await CreateSkillAsync(client, campaign.Id, "Espadas", "Combate — Armas", gmToken);

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheetId}/training/preview?skillCatalogEntryId={skillId}&days=1&correlation=Extrema");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Preview_WithUnknownSkillId_Returns404()
    {
        var (client, _, sheetId, playerToken, _) = await SetUpCharacterAsync();

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheetId}/training/preview?skillCatalogEntryId={Guid.NewGuid()}&days=1&correlation=Media");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Preview_AsNonOwnerNonGm_Returns404()
    {
        var (client, campaign, sheetId, _, gmToken) = await SetUpCharacterAsync();
        var skillId = await CreateSkillAsync(client, campaign.Id, "Espadas", "Combate — Armas", gmToken);

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var stranger = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        AuthHelper.SetBearerToken(client, stranger.AccessToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheetId}/training/preview?skillCatalogEntryId={skillId}&days=1&correlation=Media");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

- [ ] **Step 5: Run → fail** (endpoint doesn't exist yet).

- [ ] **Step 6: Service interface**

Add to `src/Ruptura.Application/Interfaces/ICharacterSheetService.cs`, after `UpdateAsync`:
```csharp
    Task<Result<TrainingProjection>> PreviewTrainingAsync(
        Guid callerId, Guid sheetId, Guid skillCatalogEntryId, int days, string correlation, CancellationToken ct = default);
```
Add `using Ruptura.Shared.CharacterSheets;` if not already covering `TrainingProjection` (the file already imports `Ruptura.Shared.CharacterSheets` for `CharacterSheetResponse` etc. — confirm, no new using needed).

- [ ] **Step 7: Implement in `CharacterSheetService`**

Change the primary constructor to add four new dependencies (after `ICatalogEntryRepository catalogRepo`, before `ICharacterStatsCalculator calculator`):
```csharp
public class CharacterSheetService(
    ICharacterSheetRepository sheetRepo,
    ICampaignRepository campaignRepo,
    ICampaignMembershipRepository membershipRepo,
    ICatalogEntryRepository catalogRepo,
    IGuildSheetRepository guildRepo,
    IGuildBuildingRepository buildingRepo,
    IGuildStaffRepository staffRepo,
    ITrainingCalculator trainingCalculator,
    ICharacterStatsCalculator calculator) : ICharacterSheetService
```
Add these usings at the top: `using Ruptura.Domain.Enums;` and `using Ruptura.Shared.Catalog;`.

Add this method (anywhere among the public methods, e.g. right after `UpdateAsync`):
```csharp
    private const int MaxTrainingDays = 3650;
    private static readonly string[] ValidCorrelations = ["Alta", "Media", "Baixa", "Nenhuma"];

    public async Task<Result<TrainingProjection>> PreviewTrainingAsync(
        Guid callerId, Guid sheetId, Guid skillCatalogEntryId, int days, string correlation,
        CancellationToken ct = default)
    {
        if (days < 1 || days > MaxTrainingDays)
            return Result.Failure<TrainingProjection>(ErrorCodes.CharacterSheet.TrainingDaysInvalid);

        if (!ValidCorrelations.Contains(correlation))
            return Result.Failure<TrainingProjection>(ErrorCodes.CharacterSheet.CorrelationInvalid);

        var authorized = await AuthorizeAccessAsync(callerId, sheetId, ct);
        if (authorized.IsFailure)
            return Result.Failure<TrainingProjection>(authorized.Error!);
        var sheet = authorized.Value!;

        var skillEntry = await catalogRepo.GetByIdAsync(skillCatalogEntryId, ct);
        if (skillEntry is null || skillEntry.Type != CatalogEntryType.Skill ||
            (skillEntry.CampaignId is { } scope && scope != sheet.CampaignId))
            return Result.Failure<TrainingProjection>(ErrorCodes.CharacterSheet.SkillNotFound);

        var area = SafeDeserializeSkill(skillEntry.DataJson)?.Area ?? string.Empty;
        var data = DeserializeSheetData(sheet.DataJson);
        var currentPoints = data.Skills.FirstOrDefault(s => s.CatalogEntryId == skillCatalogEntryId)?.Points ?? 0;

        var guild = await guildRepo.GetByCampaignAsync(sheet.CampaignId, ct);
        var buildings = guild is null
            ? new List<GuildBuilding>()
            : (await buildingRepo.GetByGuildAsync(guild.Id, ct)).ToList();
        var staff = guild is null
            ? new List<GuildStaff>()
            : (await staffRepo.GetByGuildAsync(guild.Id, ct)).ToList();

        var projection = trainingCalculator.Project(
            skillCatalogEntryId, skillEntry.Name, area, currentPoints, buildings, staff, sheetId, days, correlation);

        return Result.Success(projection);
    }

    // Mirrors CharacterStatsCalculator.SafeDeserialize — a GM's malformed homebrew Skill
    // DataJson must never 500 a training preview.
    private static SkillCatalogData? SafeDeserializeSkill(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<SkillCatalogData>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
```

- [ ] **Step 8: Controller endpoint**

In `src/Ruptura.API/Controllers/CharacterSheetController.cs`, add after `Update`:
```csharp
    [HttpGet("character-sheets/{id:guid}/training/preview")]
    [ProducesResponseType(typeof(ApiResponse<TrainingProjection>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PreviewTraining(
        Guid id, [FromQuery] Guid skillCatalogEntryId, [FromQuery] int days, [FromQuery] string correlation,
        CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.PreviewTrainingAsync(callerId, id, skillCatalogEntryId, days, correlation, ct);
        if (result.IsFailure)
            return result.Error is ErrorCodes.CharacterSheet.NotFound or ErrorCodes.CharacterSheet.SkillNotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<TrainingProjection>.Ok(result.Value!));
    }
```
Add `using Ruptura.Shared.CharacterSheets;` if not already present (it is, via `CharacterSheetResponse` etc.).

- [ ] **Step 9: Run tests → pass**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~CharacterSheetTrainingTests` then the full suite: `dotnet build && dotnet test`.

- [ ] **Step 10: Commit**

```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure/Services/CharacterSheetService.cs \
  src/Ruptura.API/Controllers/CharacterSheetController.cs src/Ruptura.API/Resources \
  tests/Ruptura.IntegrationTests/Controllers/CharacterSheetTrainingTests.cs \
  tests/Ruptura.IntegrationTests/Controllers/CharacterSheetErrorCodeLocalizationTests.cs
git commit -m "feat: add server-computed skill training preview endpoint

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 4: Blazor "Interlúdio" tab

**Files:** modify `ICharacterSheetClientService.cs`, `CharacterSheetClientService.cs`, `CharacterSheetEditor.razor`, `AppStrings*.resx`; create `CharacterSheetTrainingTab.razor`.

**Interfaces:**
- Consumes: `ICharacterSheetClientService.PreviewTrainingAsync`, `ICatalogClientService.GetByTypeAsync`/`CreateAsync` (existing), `TrainingProjection`.

- [ ] **Step 1: Client method**

Add to `src/Ruptura.Web/Services/ICharacterSheetClientService.cs`, after `UpdateAsync`:
```csharp
    Task<ApiResponse<TrainingProjection>?> PreviewTrainingAsync(Guid sheetId, Guid skillCatalogEntryId, int days, string correlation);
```

Add to `src/Ruptura.Web/Services/CharacterSheetClientService.cs`, after `UpdateAsync`:
```csharp
    public async Task<ApiResponse<TrainingProjection>?> PreviewTrainingAsync(
        Guid sheetId, Guid skillCatalogEntryId, int days, string correlation)
    {
        var response = await Http.GetAsync(
            $"api/character-sheets/{sheetId}/training/preview?skillCatalogEntryId={skillCatalogEntryId}&days={days}&correlation={correlation}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<TrainingProjection>>();
    }
```

- [ ] **Step 2: `CharacterSheetTrainingTab.razor`**

`src/Ruptura.Web/Pages/CharacterSheetTrainingTab.razor`:
```razor
@using System.Text.Json
@using Microsoft.Extensions.Localization
@using Ruptura.Web.Resources
@using Ruptura.Shared.CharacterSheets
@using Ruptura.Shared.Catalog
@inject IStringLocalizer<AppStrings> L
@inject ICharacterSheetClientService SheetService
@inject ICatalogClientService CatalogService
@inject ToastService Toast

@if (_loading)
{
    <LoadingIndicator Text="@L["Common.Loading"]" />
}
else
{
<div style="display:flex;flex-direction:column;gap:1rem;max-width:640px">
    <div>
        <label class="form-label">@L["Sheet.Training.Skill"]</label>
        <select class="form-select" @bind="_selectedSkillId">
            <optgroup label="@L["Sheet.Training.KnownSkills"]">
                @foreach (var skill in Data.Skills)
                {
                    <option value="@skill.CatalogEntryId">@NameOf(skill.CatalogEntryId)</option>
                }
            </optgroup>
            <optgroup label="@L["Sheet.Training.OtherSkills"]">
                @foreach (var entry in _available)
                {
                    <option value="@entry.Id">@entry.Name</option>
                }
            </optgroup>
        </select>
    </div>

    @if (IsGameMaster)
    {
        <div>
            <button class="btn btn-outline-secondary btn-sm" @onclick="() => _creatingSkill = !_creatingSkill">
                @L["Sheet.Training.NewSkill"]
            </button>
            @if (_creatingSkill)
            {
                <div style="border:1px solid var(--border);border-radius:6px;padding:1rem;margin-top:.5rem;display:flex;flex-direction:column;gap:.5rem">
                    <input class="form-control" placeholder="@L["Sheet.Training.NewSkill.Name"]" @bind="_newSkillName" @bind:event="oninput" />
                    <select class="form-select" @bind="_newSkillArea">
                        @foreach (var area in TrainingReference.AreaNames)
                        {
                            <option value="@area">@area</option>
                        }
                    </select>
                    <input class="form-control" placeholder="@L["Sheet.Training.NewSkill.Attribute"]" @bind="_newSkillAttribute" @bind:event="oninput" />
                    <button class="btn btn-primary btn-sm" @onclick="CreateSkillAsync" disabled="@(_savingNewSkill || string.IsNullOrWhiteSpace(_newSkillName))">
                        @if (_savingNewSkill) { <span class="spinner-border spinner-border-sm me-1"></span> }
                        @L["Sheet.Training.NewSkill.Create"]
                    </button>
                </div>
            }
        </div>
    }

    <div style="display:flex;gap:1rem;flex-wrap:wrap">
        <div>
            <label class="form-label">@L["Sheet.Training.Days"]</label>
            <input type="number" class="form-control" style="max-width:120px" min="1" max="3650" @bind="_days" />
        </div>
        <div>
            <label class="form-label">@L["Sheet.Training.Correlation"]</label>
            <select class="form-select" @bind="_correlation">
                <option value="Alta">@L["Sheet.Training.Correlation.Alta"]</option>
                <option value="Media">@L["Sheet.Training.Correlation.Media"]</option>
                <option value="Baixa">@L["Sheet.Training.Correlation.Baixa"]</option>
                <option value="Nenhuma">@L["Sheet.Training.Correlation.Nenhuma"]</option>
            </select>
        </div>
    </div>

    <div style="display:flex;gap:.5rem">
        <button class="btn btn-outline-primary btn-sm" @onclick="PreviewAsync" disabled="@(_previewing || _selectedSkillId == Guid.Empty)">
            @if (_previewing) { <span class="spinner-border spinner-border-sm me-1"></span> }
            @L["Sheet.Training.Preview"]
        </button>
        @if (_projection is not null)
        {
            <button class="btn btn-primary btn-sm" @onclick="Apply">@L["Sheet.Training.Apply"]</button>
        }
    </div>

    @if (_projection is not null)
    {
        <div style="border:1px solid var(--border);border-radius:6px;padding:1rem">
            <p>@L["Sheet.Training.PointsPerDay"]: @_projection.PointsPerDay</p>
            <p>@L["Sheet.Training.PointsToAdd"]: @_projection.PointsToAdd</p>
            <p>@L["Sheet.Training.ProjectedTotal"]: @_projection.ProjectedTotalPoints</p>
        </div>
    }
</div>
}

@code {
    [Parameter] public CharacterSheetData Data { get; set; } = new();
    [Parameter] public Guid CampaignId { get; set; }
    [Parameter] public Guid SheetId { get; set; }
    [Parameter] public bool IsGameMaster { get; set; }

    private List<CatalogEntryResponse> _all = [];
    private bool _loading = true;
    private Guid _selectedSkillId;
    private int _days = 1;
    private string _correlation = "Media";
    private TrainingProjection? _projection;
    private bool _previewing;
    private bool _creatingSkill;
    private bool _savingNewSkill;
    private string _newSkillName = string.Empty;
    private string _newSkillArea = TrainingReference.AreaNames[0];
    private string _newSkillAttribute = string.Empty;

    private IEnumerable<CatalogEntryResponse> _available =>
        _all.Where(e => !e.IsArchived && Data.Skills.All(s => s.CatalogEntryId != e.Id));

    protected override async Task OnInitializedAsync()
    {
        _all = (await CatalogService.GetByTypeAsync("Skill", CampaignId, includeArchived: true))?.Data?.ToList() ?? [];
        _loading = false;
    }

    private string NameOf(Guid id) => _all.FirstOrDefault(e => e.Id == id)?.Name ?? id.ToString();

    private async Task CreateSkillAsync()
    {
        if (string.IsNullOrWhiteSpace(_newSkillName)) return;
        _savingNewSkill = true;
        try
        {
            var dataJson = JsonSerializer.Serialize(new SkillCatalogData
            {
                Area = _newSkillArea, RelatedAttribute = _newSkillAttribute
            });
            var result = await CatalogService.CreateAsync(new CreateCatalogEntryRequest
            {
                CampaignId = CampaignId, Type = "Skill", Name = _newSkillName, DataJson = dataJson
            });
            if (result?.Data is null)
            {
                Toast.Error(result?.Message ?? L["Common.Error"]);
                return;
            }
            _all.Add(result.Data);
            _selectedSkillId = result.Data.Id;
            _creatingSkill = false;
            _newSkillName = string.Empty;
            _newSkillAttribute = string.Empty;
        }
        finally
        {
            _savingNewSkill = false;
        }
    }

    private async Task PreviewAsync()
    {
        if (_selectedSkillId == Guid.Empty) return;
        _previewing = true;
        try
        {
            var result = await SheetService.PreviewTrainingAsync(SheetId, _selectedSkillId, _days, _correlation);
            if (result?.Data is null)
            {
                Toast.Error(result?.Message ?? L["Common.Error"]);
                _projection = null;
                return;
            }
            _projection = result.Data;
        }
        finally
        {
            _previewing = false;
        }
    }

    // Client-side apply — see design spec §5.3. Mutates the shared Data.Skills the rest of the
    // editor manages; the existing AutosaveWatcher/Save button persists it like any other edit.
    // Note: re-Previewing immediately after Apply (before autosave has round-tripped) reads
    // stale server-side Points — harmless (the owner can already set Points to anything
    // directly), but the UI clears _projection after Apply so a second click can't double-apply
    // the SAME projection.
    private void Apply()
    {
        if (_projection is null) return;
        var entry = Data.Skills.FirstOrDefault(s => s.CatalogEntryId == _projection.SkillCatalogEntryId);
        if (entry is null)
        {
            entry = new CharacterSkillEntry { CatalogEntryId = _projection.SkillCatalogEntryId, Points = 0 };
            Data.Skills.Add(entry);
        }
        entry.Points = _projection.ProjectedTotalPoints;
        Toast.Success(L["Sheet.Training.Applied"]);
        _projection = null;
    }
}
```

- [ ] **Step 3: Mount the tab**

In `src/Ruptura.Web/Pages/CharacterSheetEditor.razor`, add to the `Tabs` dictionary (after `"skills"`):
```csharp
        ["training"] = "Sheet.Tab.Training",
```
Add a new branch after the `skills` block:
```razor
        else if (_activeTab == "training")
        {
            <CharacterSheetTrainingTab Data="_data" CampaignId="CampaignId" SheetId="SheetId" IsGameMaster="CanEditStatus" />
        }
```

- [ ] **Step 4: i18n — both Web resx**

`src/Ruptura.Web/Resources/AppStrings.resx`, add after the existing `Sheet.Tab.Skills` entry (search for it):
```xml
  <data name="Sheet.Tab.Training"><value>Interlude</value></data>
  <data name="Sheet.Training.Skill"><value>Skill</value></data>
  <data name="Sheet.Training.KnownSkills"><value>Known skills</value></data>
  <data name="Sheet.Training.OtherSkills"><value>Other catalog skills</value></data>
  <data name="Sheet.Training.NewSkill"><value>New Skill</value></data>
  <data name="Sheet.Training.NewSkill.Name"><value>Name</value></data>
  <data name="Sheet.Training.NewSkill.Attribute"><value>Related attribute</value></data>
  <data name="Sheet.Training.NewSkill.Create"><value>Create and select</value></data>
  <data name="Sheet.Training.Days"><value>Days</value></data>
  <data name="Sheet.Training.Correlation"><value>Learning-curve correlation</value></data>
  <data name="Sheet.Training.Correlation.Alta"><value>High</value></data>
  <data name="Sheet.Training.Correlation.Media"><value>Medium</value></data>
  <data name="Sheet.Training.Correlation.Baixa"><value>Low</value></data>
  <data name="Sheet.Training.Correlation.Nenhuma"><value>None</value></data>
  <data name="Sheet.Training.Preview"><value>Preview</value></data>
  <data name="Sheet.Training.Apply"><value>Apply</value></data>
  <data name="Sheet.Training.PointsPerDay"><value>Points per day</value></data>
  <data name="Sheet.Training.PointsToAdd"><value>Points to add</value></data>
  <data name="Sheet.Training.ProjectedTotal"><value>Projected total</value></data>
  <data name="Sheet.Training.Applied"><value>Training applied — remember to save.</value></data>
```
`src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, same position:
```xml
  <data name="Sheet.Tab.Training"><value>Interlúdio</value></data>
  <data name="Sheet.Training.Skill"><value>Perícia</value></data>
  <data name="Sheet.Training.KnownSkills"><value>Perícias conhecidas</value></data>
  <data name="Sheet.Training.OtherSkills"><value>Outras perícias do catálogo</value></data>
  <data name="Sheet.Training.NewSkill"><value>Nova Perícia</value></data>
  <data name="Sheet.Training.NewSkill.Name"><value>Nome</value></data>
  <data name="Sheet.Training.NewSkill.Attribute"><value>Atributo relacionado</value></data>
  <data name="Sheet.Training.NewSkill.Create"><value>Criar e selecionar</value></data>
  <data name="Sheet.Training.Days"><value>Dias</value></data>
  <data name="Sheet.Training.Correlation"><value>Correlação da Curva de Aprendizado</value></data>
  <data name="Sheet.Training.Correlation.Alta"><value>Alta</value></data>
  <data name="Sheet.Training.Correlation.Media"><value>Média</value></data>
  <data name="Sheet.Training.Correlation.Baixa"><value>Baixa</value></data>
  <data name="Sheet.Training.Correlation.Nenhuma"><value>Nenhuma</value></data>
  <data name="Sheet.Training.Preview"><value>Prever</value></data>
  <data name="Sheet.Training.Apply"><value>Aplicar</value></data>
  <data name="Sheet.Training.PointsPerDay"><value>Pontos por dia</value></data>
  <data name="Sheet.Training.PointsToAdd"><value>Pontos a adicionar</value></data>
  <data name="Sheet.Training.ProjectedTotal"><value>Total projetado</value></data>
  <data name="Sheet.Training.Applied"><value>Treinamento aplicado — lembre-se de salvar.</value></data>
```

- [ ] **Step 5: Build + verify + commit**

Run: `dotnet build` (clean, both API and Web projects). If feasible, run the app (`make up` or `dotnet run` per project) and confirm: the Interlúdio tab appears on a character sheet, Preview shows a computed rate, Aplicar bumps the Skills tab's Points and the editor shows unsaved-changes/autosave state. Else confirm a clean build and note it.

```bash
git add src/Ruptura.Web
git commit -m "feat: add character Interlúdio tab (skill training preview + apply)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage:**
- §3 formula (base, installation, instructor, correlation multiplier, Sem-Treinamento ceiling) → Task 2 `TrainingCalculator`. ✓ (unit tests cover every table row + the Points≥50 transition + floor rounding.)
- §4 Área→Instalação mapping incl. both simplifications (Conhecimento no advanced tier, Artesanato→Oficina always) and the Alquimia fallback and Exploração halved-again and Social/Liderança special case → Task 2 `TrainingReference` + `TrainingCalculator.InstallationBonus`. ✓
- §5.1 `GuildStaff` dedication fields → Task 1. ✓ (migration, DTOs, validation, GM-only UI picker.)
- §5.3 `TrainingProjection` + the corrected client-side Apply model → Task 2 (DTO) + Task 4 (Apply behavior). ✓
- §6 `ITrainingCalculator` → Task 2. ✓
- §7 API & Permissions (route, owner-or-GM auth, homebrew-skill-creation reuse) → Task 3. ✓
- §8 UI (skill picker incl. inline homebrew creation, days/correlation, preview/apply) → Task 4. ✓
- §9 testing → unit tests Task 2, integration tests Task 1 (dedication) + Task 3 (preview), resx guard Task 3. ✓
- **Deliberately out of scope (per spec §1, not gaps):** Provação automation, Reparo de Equipamento, Criação de Técnicas, Crafting pessoal, Pesquisa de Magia — future sub-projects in the decomposition.

**2. Placeholder scan:** every step has real, complete code (no "TBD"/"add validation"/"similar to Task N"). Migration content (Task 1 Step 3) is generated by `dotnet ef`, not hand-authored — the instruction states exactly what to run and what to verify, consistent with how every prior migration task in this repo's plans is written (see the guild plans' own migration steps).

**3. Type consistency:** `ITrainingCalculator.Project(Guid, string, string, int, IReadOnlyList<GuildBuilding>, IReadOnlyList<GuildStaff>, Guid, int, string)` is identical across the interface (Task 2 Step 5), implementation (Step 6), unit tests (Step 2), and the service call site (Task 3 Step 7). `TrainingProjection`'s fields (`SkillCatalogEntryId`, `SkillName`, `CurrentPoints`, `PointsPerDay`, `Days`, `Correlation`, `PointsToAdd`, `ProjectedTotalPoints`) are identical in the DTO (Task 2 Step 1), the calculator's construction (Step 6), the controller response (Task 3 Step 8), and the Razor component's reads (Task 4 Step 2). `GuildStaff.DedicatedCharacterSheetId`/`DedicatedSkillArea` (Task 1 Step 2) flow unchanged through `CreateStaffRequest`/`UpdateStaffRequest`/`GuildStaffResponse` (Step 7) and `GuildSheetService` (Step 8) to the Razor form (Step 11). Correlation wire values (`"Alta"|"Media"|"Baixa"|"Nenhuma"`) are consistent across the calculator, the service's `ValidCorrelations` array, the integration tests, and the Razor `<select>` options.
