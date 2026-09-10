# Character Interlude — Technique Creation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A character can start a Projeto de Técnica (GDD §6.6.7) tied to an already-invested weapon/style Perícia; the server validates prerequisites (Perícia mínima, Ranking mínimo + Academia Militar for Suprema) at start; day-by-day progress and the project record itself are client-side; once `DaysInvested >= RequiredDays`, the GM manually resolves the Teste Absoluto — Sucesso creates a homebrew Technique and adds it to the character's known Techniques, Falha resets the invested days.

**Architecture:** One new read-only server endpoint (`GET .../technique-projects/validate-start`) is the only round-trip this feature needs — everything else (creating the project after a successful validation, advancing `DaysInvested`, resetting it on Falha, and adding the finished Technique to `Data.Techniques`) is a client-side mutation of a new `CharacterSheetData.TechniqueProjects` blob module, riding the existing `AutosaveWatcher` — the same posture sub-project #1 (Skill Training) established and #2 (Equipment Repair) reused.

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-09-10-character-technique-creation-design.md` (read in full — this plan implements it, with one wording fix: §7 says "400 if... skillCatalogEntryId doesn't resolve," but the same paragraph says this "mirrors #1's SkillNotFound/validation pattern exactly" — sub-project #1's `SkillNotFound` actually maps to **404** in `CharacterSheetController`, not 400. This plan follows the precedent (404), not the imprecise 400 wording — `CategoryInvalid` alone is a genuine 400 request-shape error).

## Global Constraints

- **Category wire values are unaccented:** `"Postura"`, `"Tecnica"`, `"Reacao"`, `"Suprema"` — matches the `"Media"`-not-`"Média"` convention from sub-project #1.
- **`Ruptura.Shared` stays ZERO project references.**
- **No migration** — `TechniqueProjects` is a new list inside the existing `CharacterSheetData` JSON blob, same posture as #1/#2.
- **No dedicated apply/create/resolve endpoint** — only `validate-start` is a server call (read-only); the client owns creating, advancing, and resolving a project, exactly like #1's Apply model and #2's Reparar action.
- **`RankProgression.Ordered` (`Ruptura.Shared`) and `GuildCatalogIds.AcademiaMilitar` (`Ruptura.Shared.Guilds`, added in #1) are REUSED, never re-derived.**
- **`SkillNotFound` → 404, `CategoryInvalid`/`TrainingDaysInvalid`-style request-shape errors → 400** — mirrors `CharacterSheetController.PreviewTraining`'s exact branching (`result.Error is ErrorCodes.CharacterSheet.NotFound or ErrorCodes.CharacterSheet.SkillNotFound ? NotFound(...) : BadRequest(...)`).
- **Every visible string via `IStringLocalizer`**, both Web resx (en + pt-BR) and both API resx where relevant.
- **New `ErrorCodes` constants need resx entries in the SAME task** — the existing `CharacterSheetErrorCodeLocalizationTests` (created in #1, reflection-based over `ErrorCodes.CharacterSheet`) will fail the build otherwise; no new guard test needs to be written, the existing one already covers any new consts.
- **`TechniqueCatalogData`'s `PowerTier` values are the closed 3-value set** (`"comum"`/`"avançada"`/`"suprema"`, resx keys `Gm.Catalog.PowerTier.Common`/`.Advanced`/`.Supreme`) — reuse these exact values/keys in the Sucesso mini-form's select, don't invent new ones.
- **Homebrew Technique creation on Sucesso reuses the existing GM-only `POST /api/catalog`** (`[Authorize(Roles="GameMaster")]`) — no controller change needed there.
- **`dotnet test` (full solution, no filter) at least once before any task is marked done** — per [[project-character-interlude]] decision 1, learned from #2's final review catching a filtered-test-only miss.
- **Commit after each task** on `main`; end commit messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:**
- `src/Ruptura.Shared/CharacterSheets/TechniqueReference.cs`
- `src/Ruptura.Shared/CharacterSheets/TechniqueProjectValidation.cs`
- `src/Ruptura.Web/Pages/CharacterSheetTechniqueProjectsTab.razor`
- `tests/Ruptura.IntegrationTests/Controllers/CharacterSheetTechniqueProjectTests.cs`

**Modify:**
- `src/Ruptura.Shared/CharacterSheets/CharacterSheetData.cs` (+`TechniqueProjects` list, +`CharacterTechniqueProject` class)
- `src/Ruptura.Application/Common/ErrorCodes.cs` (+`CharacterSheet.CategoryInvalid`)
- `src/Ruptura.Application/Interfaces/ICharacterSheetService.cs` (+`ValidateTechniqueProjectStartAsync`)
- `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs` (+method, +`NormalizeSheetData` line, +usings)
- `src/Ruptura.API/Controllers/CharacterSheetController.cs` (+endpoint)
- `src/Ruptura.API/Resources/SharedResources.resx`, `SharedResources.pt-BR.resx`
- `src/Ruptura.Web/Services/ICharacterSheetClientService.cs`, `CharacterSheetClientService.cs` (+client method)
- `src/Ruptura.Web/Pages/CharacterSheetEditor.razor` (mount the new component in the existing "training" tab block)
- `src/Ruptura.Web/Resources/AppStrings.resx`, `AppStrings.pt-BR.resx`

---

### Task 1: `TechniqueReference` + `CharacterSheetData.TechniqueProjects` module

**Files:** create `TechniqueReference.cs`; modify `CharacterSheetData.cs`.

**Interfaces:**
- Produces: `TechniqueReference.Categories`/`RequiredDaysByCategory`/`MinSkillPointsByCategory`/`MinRankingByCategory`; `CharacterTechniqueProject { Id, Name, Category, SkillCatalogEntryId, RequiredDays, DaysInvested }`; `CharacterSheetData.TechniqueProjects`.

- [ ] **Step 1: `TechniqueReference`**

`src/Ruptura.Shared/CharacterSheets/TechniqueReference.cs`:
```csharp
namespace Ruptura.Shared.CharacterSheets;

// GDD §6.6.7 — requisitos formais por categoria de Técnica (FECHADO). Suprema additionally
// requires Academia Militar built in the campaign's Guild (§10.3.1) — that check lives in
// CharacterSheetService.ValidateTechniqueProjectStartAsync, not here (this table has no
// notion of Guild state).
public static class TechniqueReference
{
    public static readonly IReadOnlyList<string> Categories = ["Postura", "Tecnica", "Reacao", "Suprema"];

    public static readonly IReadOnlyDictionary<string, int> RequiredDaysByCategory = new Dictionary<string, int>
    {
        ["Postura"] = 5, ["Tecnica"] = 10, ["Reacao"] = 10, ["Suprema"] = 25
    };

    public static readonly IReadOnlyDictionary<string, int> MinSkillPointsByCategory = new Dictionary<string, int>
    {
        ["Postura"] = 25, ["Tecnica"] = 50, ["Reacao"] = 50, ["Suprema"] = 75
    };

    // Only Suprema has a minimum Ranking; null means "no requirement."
    public static readonly IReadOnlyDictionary<string, string?> MinRankingByCategory = new Dictionary<string, string?>
    {
        ["Postura"] = null, ["Tecnica"] = null, ["Reacao"] = null, ["Suprema"] = "Prata"
    };
}
```

- [ ] **Step 2: `TechniqueProjectValidation` DTO**

`src/Ruptura.Shared/CharacterSheets/TechniqueProjectValidation.cs`:
```csharp
namespace Ruptura.Shared.CharacterSheets;

// Result of a read-only "can this project start?" check. CanStart=false + a BlockedReason is
// a normal domain answer, not an error — the endpoint still returns 200. RequiredDays is
// always populated (display-only when CanStart=false, and what the client copies onto the new
// CharacterTechniqueProject when CanStart=true).
public class TechniqueProjectValidation
{
    public bool CanStart { get; set; }
    public string? BlockedReason { get; set; } // "InsufficientSkill" | "InsufficientRanking" | "MissingInstallation" | null
    public int RequiredDays { get; set; }
}
```

- [ ] **Step 3: `CharacterSheetData.TechniqueProjects`**

In `src/Ruptura.Shared/CharacterSheets/CharacterSheetData.cs`, add to the `CharacterSheetData` class (after the existing `GuildRegistry` property):
```csharp
    public List<CharacterTechniqueProject> TechniqueProjects { get; set; } = [];
```
Add a new class near `CharacterCatalogRefEntry` (same file):
```csharp
// Module: Projetos de Técnica (GDD §6.6.7). A project has no CatalogEntryId of its own — the
// Technique it will become doesn't exist in the catalog until the GM resolves the Teste
// Absoluto as Sucesso (CharacterSheetTechniqueProjectsTab handles that by POSTing a new
// homebrew CatalogEntry and adding a CharacterCatalogRefEntry to Techniques, then removing
// this project). "Ready for test" is DaysInvested >= RequiredDays — no separate Status field,
// same "derive, don't duplicate" posture as Equipment's computed-live Danificado state.
public class CharacterTechniqueProject
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;       // TechniqueReference.Categories value
    public Guid SkillCatalogEntryId { get; set; }               // the arma/estilo Perícia this project is tied to
    public int RequiredDays { get; set; }                        // server-derived at creation, never client-computed
    public int DaysInvested { get; set; }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build` — PASS (no consumers yet, just the new types compiling standalone).

- [ ] **Step 5: Commit**

```bash
git add src/Ruptura.Shared/CharacterSheets
git commit -m "feat: add TechniqueReference and CharacterSheetData.TechniqueProjects module

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: Validate-start endpoint + integration tests

**Files:** modify `ErrorCodes.cs`, `ICharacterSheetService.cs`, `CharacterSheetService.cs`, `CharacterSheetController.cs`, `SharedResources*.resx`; create `CharacterSheetTechniqueProjectTests.cs`.

**Interfaces:**
- Consumes: `TechniqueReference.*` (Task 1), `RankProgression.Ordered` (`Ruptura.Shared`, existing), `GuildCatalogIds.AcademiaMilitar` (`Ruptura.Shared.Guilds`, existing), `IGuildSheetRepository`/`IGuildBuildingRepository` (already injected into `CharacterSheetService` since sub-project #1).
- Produces: `ICharacterSheetService.ValidateTechniqueProjectStartAsync(Guid callerId, Guid sheetId, string category, Guid skillCatalogEntryId, CancellationToken ct = default) → Task<Result<TechniqueProjectValidation>>`.

- [ ] **Step 1: Error code**

In `src/Ruptura.Application/Common/ErrorCodes.cs`, inside `public static class CharacterSheet`, add after `CorrelationInvalid`:
```csharp
        public const string CategoryInvalid = "CharacterSheet.CategoryInvalid";
```

- [ ] **Step 2: resx entry**

`src/Ruptura.API/Resources/SharedResources.resx`, add after the existing `CharacterSheet.CorrelationInvalid` entry (search for it):
```xml
  <data name="CharacterSheet.CategoryInvalid"><value>The technique category is invalid.</value></data>
```
`src/Ruptura.API/Resources/SharedResources.pt-BR.resx`, same position:
```xml
  <data name="CharacterSheet.CategoryInvalid"><value>A categoria de técnica é inválida.</value></data>
```

- [ ] **Step 3: Write the failing integration tests**

`tests/Ruptura.IntegrationTests/Controllers/CharacterSheetTechniqueProjectTests.cs`:
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

public class CharacterSheetTechniqueProjectTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, CampaignResponse Campaign, CharacterSheetResponse Sheet, Guid SkillId, string PlayerToken, string GmToken)>
        SetUpCharacterWithSkillAsync(int skillPoints)
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Technique Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = "Trainee" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        var skillsResponse = await client.GetAsync($"api/catalog?type=Skill&campaignId={campaign.Id}");
        var skill = (await skillsResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!
            .Data!.First(s => s.Name == "Espadas");

        sheet.Data.Skills.Add(new CharacterSkillEntry { CatalogEntryId = skill.Id, Points = skillPoints });
        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName, DataJson = System.Text.Json.JsonSerializer.Serialize(sheet.Data)
        });
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        return (client, campaign, updated, skill.Id, player.AccessToken, gm.AccessToken);
    }

    private async Task SetRankingAsGmAsync(HttpClient client, CharacterSheetResponse sheet, string gmToken, string ranking)
    {
        AuthHelper.SetBearerToken(client, gmToken);
        sheet.Data.GuildRegistry.Ranking = ranking;
        await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName, DataJson = System.Text.Json.JsonSerializer.Serialize(sheet.Data)
        });
    }

    [Fact]
    public async Task ValidateStart_Postura_SufficientSkill_CanStartTrue_With5Days()
    {
        var (client, _, sheet, skillId, playerToken, _) = await SetUpCharacterWithSkillAsync(skillPoints: 25);
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/technique-projects/validate-start?category=Postura&skillCatalogEntryId={skillId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>())!.Data!;
        body.CanStart.Should().BeTrue();
        body.RequiredDays.Should().Be(5);
        body.BlockedReason.Should().BeNull();
    }

    [Fact]
    public async Task ValidateStart_InsufficientSkillPoints_CanStartFalse_InsufficientSkill()
    {
        var (client, _, sheet, skillId, playerToken, _) = await SetUpCharacterWithSkillAsync(skillPoints: 24); // Postura needs 25
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/technique-projects/validate-start?category=Postura&skillCatalogEntryId={skillId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>())!.Data!;
        body.CanStart.Should().BeFalse();
        body.BlockedReason.Should().Be("InsufficientSkill");
    }

    [Fact]
    public async Task ValidateStart_Suprema_SufficientSkillButRankingTooLow_InsufficientRanking()
    {
        // Default Ranking is "Bronze" — below Prata, the Suprema minimum.
        var (client, _, sheet, skillId, playerToken, _) = await SetUpCharacterWithSkillAsync(skillPoints: 75);
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/technique-projects/validate-start?category=Suprema&skillCatalogEntryId={skillId}");

        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>())!.Data!;
        body.CanStart.Should().BeFalse();
        body.BlockedReason.Should().Be("InsufficientRanking");
    }

    [Fact]
    public async Task ValidateStart_Suprema_SkillAndRankingOkButNoAcademiaMilitar_MissingInstallation()
    {
        var (client, _, sheet, skillId, playerToken, gmToken) = await SetUpCharacterWithSkillAsync(skillPoints: 75);
        await SetRankingAsGmAsync(client, sheet, gmToken, "Prata");
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/technique-projects/validate-start?category=Suprema&skillCatalogEntryId={skillId}");

        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>())!.Data!;
        body.CanStart.Should().BeFalse();
        body.BlockedReason.Should().Be("MissingInstallation");
    }

    [Fact]
    public async Task ValidateStart_Suprema_AllRequirementsMet_CanStartTrue_With25Days()
    {
        var (client, campaign, sheet, skillId, playerToken, gmToken) = await SetUpCharacterWithSkillAsync(skillPoints: 75);
        await SetRankingAsGmAsync(client, sheet, gmToken, "Prata");

        AuthHelper.SetBearerToken(client, gmToken);
        await client.GetAsync($"api/campaigns/{campaign.Id}/guild"); // get-or-create
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.AcademiaMilitar, Level = 1, IsActive = true });

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/technique-projects/validate-start?category=Suprema&skillCatalogEntryId={skillId}");

        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>())!.Data!;
        body.CanStart.Should().BeTrue();
        body.RequiredDays.Should().Be(25);
    }

    [Fact]
    public async Task ValidateStart_UnknownCategory_Returns400()
    {
        var (client, _, sheet, skillId, playerToken, _) = await SetUpCharacterWithSkillAsync(skillPoints: 25);
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/technique-projects/validate-start?category=NaoExiste&skillCatalogEntryId={skillId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ValidateStart_UnknownSkillId_Returns404()
    {
        var (client, _, sheet, _, playerToken, _) = await SetUpCharacterWithSkillAsync(skillPoints: 25);
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/technique-projects/validate-start?category=Postura&skillCatalogEntryId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ValidateStart_AsNonOwnerNonGm_Returns404()
    {
        var (client, _, sheet, skillId, _, _) = await SetUpCharacterWithSkillAsync(skillPoints: 25);

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var stranger = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        AuthHelper.SetBearerToken(client, stranger.AccessToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/technique-projects/validate-start?category=Postura&skillCatalogEntryId={skillId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

- [ ] **Step 4: Run → fail** (endpoint doesn't exist yet).

- [ ] **Step 5: Service interface**

Add to `src/Ruptura.Application/Interfaces/ICharacterSheetService.cs`, after `PreviewTrainingAsync`:
```csharp
    Task<Result<TechniqueProjectValidation>> ValidateTechniqueProjectStartAsync(
        Guid callerId, Guid sheetId, string category, Guid skillCatalogEntryId, CancellationToken ct = default);
```

- [ ] **Step 6: Implement in `CharacterSheetService`**

Add these usings at the top of `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs` (alongside the existing ones):
```csharp
using Ruptura.Shared;
using Ruptura.Shared.Guilds;
```

Add this method (anywhere among the public methods, e.g. right after `PreviewTrainingAsync`):
```csharp
    public async Task<Result<TechniqueProjectValidation>> ValidateTechniqueProjectStartAsync(
        Guid callerId, Guid sheetId, string category, Guid skillCatalogEntryId, CancellationToken ct = default)
    {
        if (!TechniqueReference.Categories.Contains(category))
            return Result.Failure<TechniqueProjectValidation>(ErrorCodes.CharacterSheet.CategoryInvalid);

        var authorized = await AuthorizeAccessAsync(callerId, sheetId, ct);
        if (authorized.IsFailure)
            return Result.Failure<TechniqueProjectValidation>(authorized.Error!);
        var sheet = authorized.Value!;

        var skillEntry = await catalogRepo.GetByIdAsync(skillCatalogEntryId, ct);
        if (skillEntry is null || skillEntry.Type != CatalogEntryType.Skill ||
            (skillEntry.CampaignId is { } scope && scope != sheet.CampaignId))
            return Result.Failure<TechniqueProjectValidation>(ErrorCodes.CharacterSheet.SkillNotFound);

        var data = DeserializeSheetData(sheet.DataJson);
        var currentPoints = data.Skills.FirstOrDefault(s => s.CatalogEntryId == skillCatalogEntryId)?.Points ?? 0;
        var requiredDays = TechniqueReference.RequiredDaysByCategory[category];

        if (currentPoints < TechniqueReference.MinSkillPointsByCategory[category])
            return Result.Success(new TechniqueProjectValidation
            {
                CanStart = false, BlockedReason = "InsufficientSkill", RequiredDays = requiredDays
            });

        if (TechniqueReference.MinRankingByCategory[category] is { } minRank)
        {
            var currentIdx = RankProgression.Ordered.IndexOf(data.GuildRegistry.Ranking);
            var minIdx = RankProgression.Ordered.IndexOf(minRank);
            if (currentIdx < minIdx)
                return Result.Success(new TechniqueProjectValidation
                {
                    CanStart = false, BlockedReason = "InsufficientRanking", RequiredDays = requiredDays
                });

            var guild = await guildRepo.GetByCampaignAsync(sheet.CampaignId, ct);
            var hasInstallation = guild is not null &&
                (await buildingRepo.GetByGuildAsync(guild.Id, ct)).Any(b =>
                    b.CatalogEntryId == GuildCatalogIds.AcademiaMilitar && b.IsActive && b.Level >= 1);
            if (!hasInstallation)
                return Result.Success(new TechniqueProjectValidation
                {
                    CanStart = false, BlockedReason = "MissingInstallation", RequiredDays = requiredDays
                });
        }

        return Result.Success(new TechniqueProjectValidation { CanStart = true, RequiredDays = requiredDays });
    }
```

In `NormalizeSheetData`, add after `data.GuildRegistry ??= new();`:
```csharp
        data.TechniqueProjects ??= [];
```

- [ ] **Step 7: Controller endpoint**

In `src/Ruptura.API/Controllers/CharacterSheetController.cs`, add after `PreviewTraining`:
```csharp
    [HttpGet("character-sheets/{id:guid}/technique-projects/validate-start")]
    [ProducesResponseType(typeof(ApiResponse<TechniqueProjectValidation>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateTechniqueProjectStart(
        Guid id, [FromQuery] string category, [FromQuery] Guid skillCatalogEntryId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.ValidateTechniqueProjectStartAsync(callerId, id, category, skillCatalogEntryId, ct);
        if (result.IsFailure)
            return result.Error is ErrorCodes.CharacterSheet.NotFound or ErrorCodes.CharacterSheet.SkillNotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<TechniqueProjectValidation>.Ok(result.Value!));
    }
```

- [ ] **Step 8: Run tests → pass; full sweep; commit**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~CharacterSheetTechniqueProjectTests` then the FULL suite: `dotnet build && dotnet test tests/Ruptura.UnitTests && dotnet test tests/Ruptura.IntegrationTests` (no filters on the second pass — per the Global Constraints note, this is not optional).

```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure/Services/CharacterSheetService.cs \
  src/Ruptura.API/Controllers/CharacterSheetController.cs src/Ruptura.API/Resources \
  tests/Ruptura.IntegrationTests/Controllers/CharacterSheetTechniqueProjectTests.cs
git commit -m "feat: add technique-project start validation endpoint

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Blazor UI — start/advance/resolve a Técnica project

**Files:** modify `ICharacterSheetClientService.cs`, `CharacterSheetClientService.cs`, `CharacterSheetEditor.razor`, `AppStrings*.resx`; create `CharacterSheetTechniqueProjectsTab.razor`.

**Interfaces:**
- Consumes: `ICharacterSheetClientService.ValidateTechniqueProjectStartAsync`, `ICatalogClientService.GetByTypeAsync`/`CreateAsync` (existing), `TechniqueProjectValidation`, `TechniqueReference` (Task 1).

- [ ] **Step 1: Client method**

Add to `src/Ruptura.Web/Services/ICharacterSheetClientService.cs`, after `PreviewTrainingAsync`:
```csharp
    Task<ApiResponse<TechniqueProjectValidation>?> ValidateTechniqueProjectStartAsync(Guid sheetId, string category, Guid skillCatalogEntryId);
```

Add to `src/Ruptura.Web/Services/CharacterSheetClientService.cs`, after `PreviewTrainingAsync`:
```csharp
    public async Task<ApiResponse<TechniqueProjectValidation>?> ValidateTechniqueProjectStartAsync(
        Guid sheetId, string category, Guid skillCatalogEntryId)
    {
        var response = await Http.GetAsync(
            $"api/character-sheets/{sheetId}/technique-projects/validate-start?category={category}&skillCatalogEntryId={skillCatalogEntryId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>();
    }
```

- [ ] **Step 2: `CharacterSheetTechniqueProjectsTab.razor`**

`src/Ruptura.Web/Pages/CharacterSheetTechniqueProjectsTab.razor`:
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
@inject ConfirmService Confirm

@if (_loading)
{
    <LoadingIndicator Text="@L["Common.Loading"]" />
}
else
{
<div style="display:flex;flex-direction:column;gap:1.5rem">
    <div style="border:1px solid var(--border);border-radius:6px;padding:1rem;max-width:640px;display:flex;flex-direction:column;gap:.5rem">
        <h4>@L["Sheet.Technique.StartProject"]</h4>
        <select class="form-select" @bind="_newProjectSkillId">
            <option value="">@L["Sheet.Technique.SelectSkill"]</option>
            @foreach (var skill in Data.Skills)
            {
                <option value="@skill.CatalogEntryId">@NameOf(skill.CatalogEntryId)</option>
            }
        </select>
        <select class="form-select" @bind="_newProjectCategory">
            @foreach (var category in TechniqueReference.Categories)
            {
                <option value="@category">@L[$"Sheet.Technique.Category.{category}"]</option>
            }
        </select>
        <input class="form-control" placeholder="@L["Sheet.Technique.Name"]" @bind="_newProjectName" @bind:event="oninput" />
        <button class="btn btn-primary btn-sm" @onclick="ValidateAndStartAsync"
                disabled="@(_validating || _newProjectSkillId == Guid.Empty || string.IsNullOrWhiteSpace(_newProjectName))">
            @if (_validating) { <span class="spinner-border spinner-border-sm me-1"></span> }
            @L["Sheet.Technique.ValidateAndStart"]
        </button>
    </div>

    @if (Data.TechniqueProjects.Count == 0)
    {
        <p>@L["Sheet.Technique.NoProjects"]</p>
    }
    else
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table stack-mobile">
                <thead>
                    <tr>
                        <th>@L["Sheet.Technique.Name"]</th>
                        <th>@L["Gm.CampaignDetail.Col.Name"]</th>
                        <th>@L["Sheet.Technique.Progress"]</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var project in Data.TechniqueProjects)
                    {
                        <tr>
                            <td data-label="@L["Sheet.Technique.Name"]">@project.Name</td>
                            <td data-label="@L["Gm.CampaignDetail.Col.Name"]">@L[$"Sheet.Technique.Category.{project.Category}"]</td>
                            <td data-label="@L["Sheet.Technique.Progress"]">@project.DaysInvested / @project.RequiredDays</td>
                            <td data-label="">
                                @if (project.DaysInvested < project.RequiredDays)
                                {
                                    <input type="number" class="form-control form-control-sm" style="width:80px;display:inline-block" min="1" value="1" @onchange="e => _addDays[project.Id] = ParseInt(e.Value)" />
                                    <button class="btn btn-outline-secondary btn-sm" @onclick="() => AdvanceDays(project)">@L["Sheet.Technique.AdvanceDays"]</button>
                                }
                                else if (IsGameMaster)
                                {
                                    <button class="btn btn-primary btn-sm" @onclick="() => _resolvingId = project.Id">@L["Sheet.Technique.ResolveTest"]</button>
                                }
                                else
                                {
                                    <span>@L["Sheet.Technique.AwaitingGm"]</span>
                                }
                            </td>
                        </tr>
                        @if (_resolvingId == project.Id)
                        {
                            <tr>
                                <td colspan="4">
                                    <div style="border:1px solid var(--border);border-radius:6px;padding:1rem;display:flex;flex-direction:column;gap:.5rem;max-width:480px">
                                        <input class="form-control" placeholder="@L["Sheet.Technique.PaCost"]" @bind="_resolvePaCost" @bind:event="oninput" />
                                        <textarea class="form-control" placeholder="@L["Sheet.Technique.Damage"]" @bind="_resolveDamage"></textarea>
                                        <textarea class="form-control" placeholder="@L["Sheet.Technique.Effect"]" @bind="_resolveEffect"></textarea>
                                        <select class="form-select" @bind="_resolvePowerTier">
                                            <option value="comum">@L["Gm.Catalog.PowerTier.Common"]</option>
                                            <option value="avançada">@L["Gm.Catalog.PowerTier.Advanced"]</option>
                                            <option value="suprema">@L["Gm.Catalog.PowerTier.Supreme"]</option>
                                        </select>
                                        <div style="display:flex;gap:.5rem">
                                            <button class="btn btn-primary btn-sm" @onclick="() => ResolveSuccessAsync(project)" disabled="@_resolving">
                                                @if (_resolving) { <span class="spinner-border spinner-border-sm me-1"></span> }
                                                @L["Sheet.Technique.Sucesso"]
                                            </button>
                                            <button class="btn btn-outline-danger btn-sm" @onclick="() => ResolveFailureAsync(project)">@L["Sheet.Technique.Falha"]</button>
                                            <button class="btn btn-outline-secondary btn-sm" @onclick="() => _resolvingId = null">@L["Sheet.Equipment.Cancel"]</button>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        }
                    }
                </tbody>
            </table>
        </div>
    }
</div>
}

@code {
    [Parameter] public CharacterSheetData Data { get; set; } = new();
    [Parameter] public Guid CampaignId { get; set; }
    [Parameter] public Guid SheetId { get; set; }
    [Parameter] public bool IsGameMaster { get; set; }

    private List<CatalogEntryResponse> _allSkills = [];
    private bool _loading = true;
    private Guid _newProjectSkillId;
    private string _newProjectCategory = TechniqueReference.Categories[0];
    private string _newProjectName = string.Empty;
    private bool _validating;
    private readonly Dictionary<Guid, int> _addDays = [];
    private Guid? _resolvingId;
    private bool _resolving;
    private string _resolvePaCost = string.Empty;
    private string _resolveDamage = string.Empty;
    private string _resolveEffect = string.Empty;
    private string _resolvePowerTier = "comum";

    protected override async Task OnInitializedAsync()
    {
        _allSkills = (await CatalogService.GetByTypeAsync("Skill", CampaignId, includeArchived: true))?.Data?.ToList() ?? [];
        _loading = false;
    }

    private string NameOf(Guid id) => _allSkills.FirstOrDefault(e => e.Id == id)?.Name ?? id.ToString();
    private static int ParseInt(object? value) => int.TryParse(value?.ToString(), out var v) && v > 0 ? v : 1;

    private async Task ValidateAndStartAsync()
    {
        if (_newProjectSkillId == Guid.Empty || string.IsNullOrWhiteSpace(_newProjectName)) return;
        _validating = true;
        try
        {
            var result = await SheetService.ValidateTechniqueProjectStartAsync(SheetId, _newProjectCategory, _newProjectSkillId);
            if (result?.Data is null)
            {
                Toast.Error(result?.Message ?? L["Common.Error"]);
                return;
            }
            if (!result.Data.CanStart)
            {
                Toast.Error(L[$"Sheet.Technique.Blocked.{result.Data.BlockedReason}"]);
                return;
            }

            Data.TechniqueProjects.Add(new CharacterTechniqueProject
            {
                Id = Guid.NewGuid(), Name = _newProjectName, Category = _newProjectCategory,
                SkillCatalogEntryId = _newProjectSkillId, RequiredDays = result.Data.RequiredDays, DaysInvested = 0
            });
            Toast.Success(L["Sheet.Technique.Started"]);
            _newProjectName = string.Empty;
            _newProjectSkillId = Guid.Empty;
        }
        finally
        {
            _validating = false;
        }
    }

    // Client-side, no HTTP call — see design spec §2 Decision #6 (no institutional bonus
    // affects Técnica project speed, unlike Skill Training).
    private void AdvanceDays(CharacterTechniqueProject project)
    {
        var days = _addDays.GetValueOrDefault(project.Id, 1);
        project.DaysInvested = Math.Min(project.RequiredDays, project.DaysInvested + days);
    }

    private async Task ResolveSuccessAsync(CharacterTechniqueProject project)
    {
        _resolving = true;
        try
        {
            var dataJson = JsonSerializer.Serialize(new TechniqueCatalogData
            {
                Style = NameOf(project.SkillCatalogEntryId), Category = project.Category,
                PaCost = _resolvePaCost, Damage = _resolveDamage, Effect = _resolveEffect, PowerTier = _resolvePowerTier
            });
            var result = await CatalogService.CreateAsync(new CreateCatalogEntryRequest
            {
                CampaignId = CampaignId, Type = "Technique", Name = project.Name, DataJson = dataJson
            });
            if (result?.Data is null)
            {
                Toast.Error(result?.Message ?? L["Common.Error"]);
                return;
            }

            Data.Techniques.Add(new CharacterCatalogRefEntry { CatalogEntryId = result.Data.Id });
            Data.TechniqueProjects.Remove(project);
            _resolvingId = null;
            Toast.Success(L["Sheet.Technique.Success"]);
        }
        finally
        {
            _resolving = false;
        }
    }

    private async Task ResolveFailureAsync(CharacterTechniqueProject project)
    {
        var confirmed = await Confirm.AskAsync(
            L["Sheet.Technique.FailureConfirm.Title"], L["Sheet.Technique.FailureConfirm.Message"],
            L["Sheet.Technique.Falha"], L["Sheet.Equipment.Cancel"]);
        if (!confirmed) return;

        project.DaysInvested = 0;
        _resolvingId = null;
        Toast.Success(L["Sheet.Technique.Failed"]);
    }
}
```
> `L[$"Sheet.Technique.Category.{project.Category}"]`/`L[$"Sheet.Technique.Blocked.{result.Data.BlockedReason}"]` build the resx key from the wire value at runtime — this only works because the wire values (`Postura`/`Tecnica`/.../`InsufficientSkill`/...) are themselves valid resx-key fragments (no spaces/accents), which Global Constraints already requires. Step 4 must add a resx entry for every one of the 4 categories and 3 blocked-reasons using exactly this naming (`Sheet.Technique.Category.Postura`, `Sheet.Technique.Blocked.InsufficientSkill`, etc.) or the lookup silently falls back to showing the raw key.

- [ ] **Step 3: Mount + i18n**

In `src/Ruptura.Web/Pages/CharacterSheetEditor.razor`, add the new component right after `CharacterSheetTrainingTab` inside the existing `"training"` branch (do NOT add a new `Tabs` dictionary entry or a new `else if` — both components share the one "Interlúdio" tab, per design spec Decision #9):
```razor
        else if (_activeTab == "training")
        {
            <CharacterSheetTrainingTab Data="_data" CampaignId="CampaignId" SheetId="SheetId" IsGameMaster="CanEditStatus" />
            <hr />
            <CharacterSheetTechniqueProjectsTab Data="_data" CampaignId="CampaignId" SheetId="SheetId" IsGameMaster="CanEditStatus" />
        }
```

`src/Ruptura.Web/Resources/AppStrings.resx`, add after the existing `Sheet.Training.Applied` entry (search for it):
```xml
  <data name="Sheet.Technique.StartProject"><value>Start Technique Project</value></data>
  <data name="Sheet.Technique.SelectSkill"><value>— select a skill —</value></data>
  <data name="Sheet.Technique.Category.Postura"><value>Postura</value></data>
  <data name="Sheet.Technique.Category.Tecnica"><value>Técnica</value></data>
  <data name="Sheet.Technique.Category.Reacao"><value>Reação</value></data>
  <data name="Sheet.Technique.Category.Suprema"><value>Técnica Suprema</value></data>
  <data name="Sheet.Technique.Name"><value>Name</value></data>
  <data name="Sheet.Technique.ValidateAndStart"><value>Validate and Start</value></data>
  <data name="Sheet.Technique.Started"><value>Project started.</value></data>
  <data name="Sheet.Technique.Blocked.InsufficientSkill"><value>Skill points are too low for this category.</value></data>
  <data name="Sheet.Technique.Blocked.InsufficientRanking"><value>Ranking is too low for this category.</value></data>
  <data name="Sheet.Technique.Blocked.MissingInstallation"><value>Requires Academia Militar to be built.</value></data>
  <data name="Sheet.Technique.NoProjects"><value>No technique projects in progress.</value></data>
  <data name="Sheet.Technique.Progress"><value>Progress (days)</value></data>
  <data name="Sheet.Technique.AdvanceDays"><value>Advance</value></data>
  <data name="Sheet.Technique.ResolveTest"><value>Resolve Test</value></data>
  <data name="Sheet.Technique.AwaitingGm"><value>Awaiting GM</value></data>
  <data name="Sheet.Technique.PaCost"><value>PA Cost</value></data>
  <data name="Sheet.Technique.Damage"><value>Damage</value></data>
  <data name="Sheet.Technique.Effect"><value>Effect</value></data>
  <data name="Sheet.Technique.Sucesso"><value>Success</value></data>
  <data name="Sheet.Technique.Falha"><value>Failure</value></data>
  <data name="Sheet.Technique.Success"><value>Technique created!</value></data>
  <data name="Sheet.Technique.FailureConfirm.Title"><value>Mark as failure</value></data>
  <data name="Sheet.Technique.FailureConfirm.Message"><value>This resets the project's invested days to 0. The player may try again.</value></data>
  <data name="Sheet.Technique.Failed"><value>Test failed — days reset.</value></data>
```
`src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, same position:
```xml
  <data name="Sheet.Technique.StartProject"><value>Iniciar Projeto de Técnica</value></data>
  <data name="Sheet.Technique.SelectSkill"><value>— selecione uma perícia —</value></data>
  <data name="Sheet.Technique.Category.Postura"><value>Postura</value></data>
  <data name="Sheet.Technique.Category.Tecnica"><value>Técnica</value></data>
  <data name="Sheet.Technique.Category.Reacao"><value>Reação</value></data>
  <data name="Sheet.Technique.Category.Suprema"><value>Técnica Suprema</value></data>
  <data name="Sheet.Technique.Name"><value>Nome</value></data>
  <data name="Sheet.Technique.ValidateAndStart"><value>Validar e Iniciar</value></data>
  <data name="Sheet.Technique.Started"><value>Projeto iniciado.</value></data>
  <data name="Sheet.Technique.Blocked.InsufficientSkill"><value>Pontos de perícia insuficientes pra essa categoria.</value></data>
  <data name="Sheet.Technique.Blocked.InsufficientRanking"><value>Ranking insuficiente pra essa categoria.</value></data>
  <data name="Sheet.Technique.Blocked.MissingInstallation"><value>Exige Academia Militar construída.</value></data>
  <data name="Sheet.Technique.NoProjects"><value>Nenhum projeto de técnica em andamento.</value></data>
  <data name="Sheet.Technique.Progress"><value>Progresso (dias)</value></data>
  <data name="Sheet.Technique.AdvanceDays"><value>Avançar</value></data>
  <data name="Sheet.Technique.ResolveTest"><value>Resolver Teste</value></data>
  <data name="Sheet.Technique.AwaitingGm"><value>Aguardando o Mestre</value></data>
  <data name="Sheet.Technique.PaCost"><value>Custo em PA</value></data>
  <data name="Sheet.Technique.Damage"><value>Dano</value></data>
  <data name="Sheet.Technique.Effect"><value>Efeito</value></data>
  <data name="Sheet.Technique.Sucesso"><value>Sucesso</value></data>
  <data name="Sheet.Technique.Falha"><value>Falha</value></data>
  <data name="Sheet.Technique.Success"><value>Técnica criada!</value></data>
  <data name="Sheet.Technique.FailureConfirm.Title"><value>Marcar como falha</value></data>
  <data name="Sheet.Technique.FailureConfirm.Message"><value>Isso zera os dias investidos do projeto. O jogador pode tentar de novo.</value></data>
  <data name="Sheet.Technique.Failed"><value>Teste falhou — dias zerados.</value></data>
```
Note `Sheet.Equipment.Cancel` is intentionally reused as-is (already exists from sub-project #2, generic "Cancel" copy) rather than adding a duplicate `Sheet.Technique.Cancel` key — confirm this key still exists at the time of implementation; if the surrounding convention has since diverged, add a dedicated key instead of forcing a cross-feature reuse.

- [ ] **Step 4: Build + verify + full sweep + commit**

Run: `dotnet build` (clean, both API and Web projects). If feasible, run the app and confirm: starting a project validates and appears in the list; advancing days works and caps at `RequiredDays`; GM-only Resolve Test section appears once ready; Sucesso creates a catalog Technique and moves it into the known list; Falha resets days. Else confirm a clean build and note it. Then run the FULL solution test suite (`dotnet build && dotnet test tests/Ruptura.UnitTests && dotnet test tests/Ruptura.IntegrationTests`, no filters) per the Global Constraints note — this is the step #2's final review found missing.

```bash
git add src/Ruptura.Web
git commit -m "feat: add character Technique Project UI (start, advance, GM-resolve)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage:**
- §3 tables (RequiredDays/MinSkillPoints/MinRanking by category) → Task 1 `TechniqueReference`. ✓
- §5 data model (`CharacterTechniqueProject`, `TechniqueProjectValidation`) → Task 1. ✓
- §6 API (validate-start endpoint, prerequisite logic incl. Academia Militar) → Task 2. ✓
- §7 UI (start/list/advance/resolve, GM-only resolve) → Task 3. ✓
- §8 `CharacterSheetService` changes → Task 2 Step 6. ✓
- §9 testing (per-category validation incl. all 3 blocked reasons, request-shape errors, auth) → Task 2. ✓
- **Deliberately out of scope (per spec §1, not gaps):** Variações, PA tracking, dice/test automation, a concurrency limit on projects.
- **Spec wording fix applied:** §7's imprecise "400 for skillCatalogEntryId not resolving" corrected to 404, matching #1's actual precedent and this plan's own controller code (see plan header note).

**2. Placeholder scan:** every step has complete, real code. No "TBD"/"add validation"/"similar to Task N". The resx-key-from-wire-value pattern (`Sheet.Technique.Category.{X}`/`Sheet.Technique.Blocked.{X}`) is called out explicitly with the exact keys Step 3 must add, not left implicit.

**3. Type consistency:** `TechniqueProjectValidation{CanStart, BlockedReason, RequiredDays}` identical across the DTO (Task 1), the service method's returns (Task 2 Step 6), the controller response (Task 2 Step 7), and the Razor component's reads (Task 3 Step 2). `CharacterTechniqueProject{Id, Name, Category, SkillCatalogEntryId, RequiredDays, DaysInvested}` identical across the model (Task 1) and every read/write site in Task 3. `ValidateTechniqueProjectStartAsync(Guid, Guid, string, Guid, CancellationToken)` identical across the interface, implementation, controller call site, and client call site. Category/BlockedReason wire-value strings are consistent across `TechniqueReference`, the service's comparisons, the tests' literals, and the UI's resx-key interpolation.
