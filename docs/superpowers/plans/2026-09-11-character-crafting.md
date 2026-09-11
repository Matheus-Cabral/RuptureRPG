# Character Interlude — Personal Crafting Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** A character can craft an item from a "Receita Conhecida" (an `EquipmentItem` catalog entry they're permitted to make) during Interlúdio: start a project (server validates the recipe is known and the Guild has the minimum installation for its Raridade), advance days client-side, and have the GM resolve the Teste Absoluto — Sucesso appends a new `CharacterEquipmentEntry` to `Data.Equipment`, Falha resets the invested days.

**Architecture:** One new read-only server endpoint (`GET .../crafting-projects/validate-start`) — everything else (creating the project, advancing days, both outcomes) is a client-side mutation riding the existing autosave, continuing #1/#2/#3's established posture. This sub-project reuses more existing code than any prior one: `TechniqueProjectValidation` (the DTO, verbatim, no new type), `EquipmentReference.MaxDurabilityFor` (for the crafted item's starting durability), and `CharacterSheetCatalogRefListTab` (the existing generic list-management component, for known-recipe management — no new Razor component for that half of the feature).

**Tech Stack:** .NET 8, EF Core 8 + Npgsql, Blazor WASM 8, xUnit + FluentAssertions + Testcontainers.PostgreSql.

**Spec:** `docs/superpowers/specs/2026-09-11-character-crafting-design.md` (read in full — this plan implements it, with one refinement: §7 described the "recipe doesn't resolve" and "recipe's Rarity isn't craftable" cases as a 400. Reading `CharacterSheetService.PreviewTrainingAsync`'s actual `SkillNotFound` check shows the established precedent collapses "doesn't exist," "wrong type," and "cross-campaign" into ONE 404 — not a 404/400 split. This plan follows that established precedent instead: a single `RecipeNotFound` (404) covers not-found, wrong-type, cross-campaign, AND non-craftable-Rarity in one check, mirroring `SkillNotFound`'s exact shape. No new 400-only error code is needed for this endpoint.

## Global Constraints

- **`Ruptura.Shared` stays ZERO project references.**
- **No migration** — `KnownRecipes`/`CraftingProjects` are new lists inside the existing `CharacterSheetData` JSON blob.
- **No dedicated apply/create/resolve endpoint** — only `validate-start` is a server call (read-only); the client owns everything else, per #1/#2/#3's established posture. Sucesso needs NO server call at all (no catalog creation — the recipe already exists in the catalog).
- **`RecipeNotFound` → 404** (not a 400/404 split) — see the plan header note above.
- **New `ErrorCodes` constants need resx entries in the SAME task** — the existing `CharacterSheetErrorCodeLocalizationTests` (reflection-based over `ErrorCodes.CharacterSheet`) covers them automatically, no new guard test needed.
- **Rarity matching is case/whitespace-insensitive** (`.Trim()` + `StringComparer.OrdinalIgnoreCase`), matching `EquipmentReference`'s established convention.
- **Every client-side list ships with a Remove action from day one** — lesson from sub-project #3's final review, now a standing rule ([[project-character-interlude]] decision 3).
- **A reference table's key-set consistency gets a unit test proactively**, not after a review asks for it — lesson from sub-project #3 ([[project-character-interlude]] decision 5).
- **Any async Razor handler that re-reads `@bind`-ed fields after an `await` must capture them into locals FIRST** — lesson from sub-project #3 ([[project-character-interlude]] decision 4). Apply this to the crafting-project start handler from the start.
- **The FULL solution test suite (`dotnet test`, no filter) must be run at least once before any task is marked done** — standing rule since sub-project #2's final review.
- **Commit after each task** on `main`; end commit messages with `Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>`.

## File Structure

**Create:**
- `src/Ruptura.Shared/CharacterSheets/CraftingReference.cs`
- `src/Ruptura.Web/Pages/CharacterSheetCraftingProjectsTab.razor`
- `tests/Ruptura.UnitTests/CharacterSheets/CraftingReferenceTests.cs`
- `tests/Ruptura.IntegrationTests/Controllers/CharacterSheetCraftingProjectTests.cs`

**Modify:**
- `src/Ruptura.Shared/Guilds/GuildCatalogIds.cs` (+`Ferraria`)
- `src/Ruptura.Shared/CharacterSheets/CharacterSheetData.cs` (+`KnownRecipes`, +`CraftingProjects`, +`CharacterCraftingProject`)
- `src/Ruptura.Application/Common/ErrorCodes.cs` (+`CharacterSheet.RecipeNotFound`)
- `src/Ruptura.Application/Interfaces/ICharacterSheetService.cs` (+`ValidateCraftingProjectStartAsync`)
- `src/Ruptura.Infrastructure/Services/CharacterSheetService.cs` (+method, +`NormalizeSheetData` line)
- `src/Ruptura.API/Controllers/CharacterSheetController.cs` (+endpoint)
- `src/Ruptura.API/Resources/SharedResources.resx`, `SharedResources.pt-BR.resx`
- `src/Ruptura.Web/Services/ICharacterSheetClientService.cs`, `CharacterSheetClientService.cs` (+client method)
- `src/Ruptura.Web/Pages/CharacterSheetEquipmentTab.razor` (+ Known Recipes section)
- `src/Ruptura.Web/Pages/CharacterSheetEditor.razor` (mount the new tab component in the existing "training" tab block)
- `src/Ruptura.Web/Resources/AppStrings.resx`, `AppStrings.pt-BR.resx`

---

### Task 1: `CraftingReference` + `CharacterSheetData` modules

**Files:** create `CraftingReference.cs`, `CraftingReferenceTests.cs`; modify `GuildCatalogIds.cs`, `CharacterSheetData.cs`.

**Interfaces:**
- Produces: `CraftingReference.CraftableRarities`/`RequiredDaysByRarity`/`MaterialsCostByRarity`/`InstallationByRarity`; `CharacterCraftingProject { Id, RecipeCatalogEntryId, RequiredDays, DaysInvested }`; `CharacterSheetData.KnownRecipes`/`CraftingProjects`; `GuildCatalogIds.Ferraria`.

- [ ] **Step 1: `GuildCatalogIds.Ferraria`**

In `src/Ruptura.Shared/Guilds/GuildCatalogIds.cs`, add after `CampoDeTreinamento`:
```csharp
    public static readonly Guid Ferraria = Guid.Parse("d0000000-0000-0000-0000-000000000005");
```
(Verified against `CatalogSeedData.Installations.cs` row 5, `"Ferraria"`.)

- [ ] **Step 2: `CraftingReference`**

`src/Ruptura.Shared/CharacterSheets/CraftingReference.cs`:
```csharp
using Ruptura.Shared.Guilds;

namespace Ruptura.Shared.CharacterSheets;

// GDD §6.7.4 — Dias de Criação e Custo em Materiais by Raridade (FECHADO for those two
// columns). Divino is deliberately absent — the GDD itself states Divino "Requer projeto de
// Pesquisa prévio," a system this app doesn't have (and won't — the would-be sub-project for
// it was dropped as redundant). Installation mapping is NOT a verbatim GDD table — §6.7.4
// names "Oficina Básica/Ferraria/Ferraria Avançada/Forja Rúnica/Forja Divina," none of which
// are separate seeded installations; this is reconciled against the ALREADY-SEEDED
// installations' own §10.3.1 descriptions (see design spec §4): Ferraria's own description
// says "Comum até Raro em Nível I-II; Épico em III+", and Oficina de Runas's says
// "Crafting Épico+" — the only rune-themed installation in the 20-item list.
public static class CraftingReference
{
    public static readonly IReadOnlyList<string> CraftableRarities =
        ["Comum", "Incomum", "Raro", "Épico", "Lendário"];

    public static readonly IReadOnlyDictionary<string, int> RequiredDaysByRarity =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Comum"] = 1, ["Incomum"] = 3, ["Raro"] = 7, ["Épico"] = 14, ["Lendário"] = 30
        };

    public static readonly IReadOnlyDictionary<string, int> MaterialsCostByRarity =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Comum"] = 5, ["Incomum"] = 15, ["Raro"] = 35, ["Épico"] = 75, ["Lendário"] = 150
        };

    public static readonly IReadOnlyDictionary<string, (Guid InstallationId, int MinLevel)> InstallationByRarity =
        new Dictionary<string, (Guid, int)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Comum"] = (GuildCatalogIds.Oficina, 1),
            ["Incomum"] = (GuildCatalogIds.Oficina, 1),
            ["Raro"] = (GuildCatalogIds.Ferraria, 1),
            ["Épico"] = (GuildCatalogIds.Ferraria, 3),
            ["Lendário"] = (GuildCatalogIds.OficinaDeRunas, 1)
        };

    public static bool IsCraftable(string rarity) =>
        CraftableRarities.Any(r => string.Equals(r, rarity.Trim(), StringComparison.OrdinalIgnoreCase));
}
```

- [ ] **Step 3: Failing unit test for table-key consistency**

`tests/Ruptura.UnitTests/CharacterSheets/CraftingReferenceTests.cs`:
```csharp
using FluentAssertions;
using Ruptura.Shared.CharacterSheets;
using Xunit;

namespace Ruptura.UnitTests.CharacterSheets;

public class CraftingReferenceTests
{
    [Theory]
    [MemberData(nameof(Rarities))]
    public void EveryCraftableRarity_HasAnEntryInRequiredDaysByRarity(string rarity) =>
        CraftingReference.RequiredDaysByRarity.Should().ContainKey(rarity);

    [Theory]
    [MemberData(nameof(Rarities))]
    public void EveryCraftableRarity_HasAnEntryInMaterialsCostByRarity(string rarity) =>
        CraftingReference.MaterialsCostByRarity.Should().ContainKey(rarity);

    [Theory]
    [MemberData(nameof(Rarities))]
    public void EveryCraftableRarity_HasAnEntryInInstallationByRarity(string rarity) =>
        CraftingReference.InstallationByRarity.Should().ContainKey(rarity);

    public static IEnumerable<object[]> Rarities() =>
        CraftingReference.CraftableRarities.Select(r => new object[] { r });

    [Theory]
    [InlineData("comum", true)]
    [InlineData("COMUM", true)]
    [InlineData(" Comum ", true)]
    [InlineData("Divino", false)]
    [InlineData("Homebrew", false)]
    public void IsCraftable_IsCaseAndWhitespaceInsensitive_AndExcludesDivino(string rarity, bool expected) =>
        CraftingReference.IsCraftable(rarity).Should().Be(expected);
}
```

- [ ] **Step 4: Run → fail** (`CraftingReference` doesn't exist yet — compile error).

- [ ] **Step 5: `CharacterSheetData` modules**

In `src/Ruptura.Shared/CharacterSheets/CharacterSheetData.cs`, add to the `CharacterSheetData` class (after `TechniqueProjects`):
```csharp
    public List<CharacterCatalogRefEntry> KnownRecipes { get; set; } = [];
    public List<CharacterCraftingProject> CraftingProjects { get; set; } = [];
```
Add a new class near `CharacterTechniqueProject`:
```csharp
// Module: Projetos de Fabricação (GDD §6.7.4, Caminho B). RecipeCatalogEntryId must be
// present in Data.KnownRecipes at project-start time (validated server-side) — unlike
// CharacterTechniqueProject, there is no Skill linkage (the GDD states no Perícia mínima gate
// for crafting) and Sucesso never creates a new catalog entry (the recipe already exists —
// that's what "knowing" it means). "Ready for test" is DaysInvested >= RequiredDays, same
// derive-don't-duplicate posture as every other Interlude module.
public class CharacterCraftingProject
{
    public Guid Id { get; set; }
    public Guid RecipeCatalogEntryId { get; set; }
    public int RequiredDays { get; set; }
    public int DaysInvested { get; set; }
}
```

- [ ] **Step 6: Run → pass**

Run: `dotnet test tests/Ruptura.UnitTests --filter FullyQualifiedName~CraftingReferenceTests` → all pass. Then `dotnet build` clean.

- [ ] **Step 7: Commit**

```bash
git add src/Ruptura.Shared/Guilds/GuildCatalogIds.cs src/Ruptura.Shared/CharacterSheets \
  tests/Ruptura.UnitTests/CharacterSheets/CraftingReferenceTests.cs
git commit -m "feat: add CraftingReference and CharacterSheetData crafting modules

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 2: Validate-start endpoint + integration tests

**Files:** modify `ErrorCodes.cs`, `ICharacterSheetService.cs`, `CharacterSheetService.cs`, `CharacterSheetController.cs`, `SharedResources*.resx`; create `CharacterSheetCraftingProjectTests.cs`.

**Interfaces:**
- Consumes: `CraftingReference.*` (Task 1), `IGuildSheetRepository`/`IGuildBuildingRepository` (already injected into `CharacterSheetService`).
- Produces: `ICharacterSheetService.ValidateCraftingProjectStartAsync(Guid callerId, Guid sheetId, Guid recipeCatalogEntryId, CancellationToken ct = default) → Task<Result<TechniqueProjectValidation>>` (reuses the existing `TechniqueProjectValidation` type — no new DTO).

- [ ] **Step 1: Error code**

In `src/Ruptura.Application/Common/ErrorCodes.cs`, inside `public static class CharacterSheet`, add after `CategoryInvalid`:
```csharp
        public const string RecipeNotFound = "CharacterSheet.RecipeNotFound";
```

- [ ] **Step 2: resx entry**

`src/Ruptura.API/Resources/SharedResources.resx`, add after the existing `CharacterSheet.CategoryInvalid` entry (search for it):
```xml
  <data name="CharacterSheet.RecipeNotFound"><value>The recipe was not found.</value></data>
```
`src/Ruptura.API/Resources/SharedResources.pt-BR.resx`, same position:
```xml
  <data name="CharacterSheet.RecipeNotFound"><value>A receita não foi encontrada.</value></data>
```

- [ ] **Step 3: Write the failing integration tests**

`tests/Ruptura.IntegrationTests/Controllers/CharacterSheetCraftingProjectTests.cs`:
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

public class CharacterSheetCraftingProjectTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, CampaignResponse Campaign, CharacterSheetResponse Sheet, Guid RecipeId, string PlayerToken, string GmToken)>
        SetUpCharacterWithRecipeAsync(string rarity, bool known = true)
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Crafting Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var recipeResponse = await client.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaign.Id, Type = "EquipmentItem", Name = $"Test Item ({rarity})",
            DataJson = $$"""{"Category":"item","Rarity":"{{rarity}}","AttackBonus":0,"DamageBonus":0,"DefenseBonus":0,"Weight":1}"""
        });
        var recipe = (await recipeResponse.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>())!.Data!;

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = "Crafter" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        if (known)
        {
            sheet.Data.KnownRecipes.Add(new CharacterCatalogRefEntry { CatalogEntryId = recipe.Id });
            var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
            {
                CharacterName = sheet.CharacterName, DataJson = System.Text.Json.JsonSerializer.Serialize(sheet.Data)
            });
            sheet = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        }

        return (client, campaign, sheet, recipe.Id, player.AccessToken, gm.AccessToken);
    }

    [Fact]
    public async Task ValidateStart_ComumRecipeKnown_OficinaBuilt_CanStartTrue_With1Day()
    {
        var (client, campaign, sheet, recipeId, playerToken, gmToken) = await SetUpCharacterWithRecipeAsync("Comum");

        AuthHelper.SetBearerToken(client, gmToken);
        await client.GetAsync($"api/campaigns/{campaign.Id}/guild"); // get-or-create
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Oficina, Level = 1, IsActive = true });

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/crafting-projects/validate-start?recipeCatalogEntryId={recipeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>())!.Data!;
        body.CanStart.Should().BeTrue();
        body.RequiredDays.Should().Be(1);
    }

    [Fact]
    public async Task ValidateStart_RecipeNotKnown_CanStartFalse_RecipeNotKnown()
    {
        var (client, _, sheet, recipeId, playerToken, _) = await SetUpCharacterWithRecipeAsync("Comum", known: false);
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/crafting-projects/validate-start?recipeCatalogEntryId={recipeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>())!.Data!;
        body.CanStart.Should().BeFalse();
        body.BlockedReason.Should().Be("RecipeNotKnown");
    }

    [Fact]
    public async Task ValidateStart_RaroRecipeKnown_NoGuildYet_MissingInstallation()
    {
        var (client, _, sheet, recipeId, playerToken, _) = await SetUpCharacterWithRecipeAsync("Raro");
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/crafting-projects/validate-start?recipeCatalogEntryId={recipeId}");

        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>())!.Data!;
        body.CanStart.Should().BeFalse();
        body.BlockedReason.Should().Be("MissingInstallation");
    }

    [Fact]
    public async Task ValidateStart_EpicoRecipeKnown_FerrariaBelowRequiredLevel_MissingInstallation()
    {
        var (client, campaign, sheet, recipeId, playerToken, gmToken) = await SetUpCharacterWithRecipeAsync("Épico");

        AuthHelper.SetBearerToken(client, gmToken);
        await client.GetAsync($"api/campaigns/{campaign.Id}/guild");
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Ferraria, Level = 1, IsActive = true }); // Épico needs Level 3

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/crafting-projects/validate-start?recipeCatalogEntryId={recipeId}");

        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>())!.Data!;
        body.CanStart.Should().BeFalse();
        body.BlockedReason.Should().Be("MissingInstallation");
    }

    [Fact]
    public async Task ValidateStart_EpicoRecipeKnown_FerrariaAtRequiredLevel_CanStartTrue_With14Days()
    {
        var (client, campaign, sheet, recipeId, playerToken, gmToken) = await SetUpCharacterWithRecipeAsync("Épico");

        AuthHelper.SetBearerToken(client, gmToken);
        await client.GetAsync($"api/campaigns/{campaign.Id}/guild");
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Ferraria, Level = 3, IsActive = true });

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/crafting-projects/validate-start?recipeCatalogEntryId={recipeId}");

        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>())!.Data!;
        body.CanStart.Should().BeTrue();
        body.RequiredDays.Should().Be(14);
    }

    [Fact]
    public async Task ValidateStart_DivinoRarityRecipe_Returns404()
    {
        var (client, _, sheet, recipeId, playerToken, _) = await SetUpCharacterWithRecipeAsync("Divino");
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/crafting-projects/validate-start?recipeCatalogEntryId={recipeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ValidateStart_UnknownRecipeId_Returns404()
    {
        var (client, _, sheet, _, playerToken, _) = await SetUpCharacterWithRecipeAsync("Comum");
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/crafting-projects/validate-start?recipeCatalogEntryId={Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ValidateStart_AsNonOwnerNonGm_Returns404()
    {
        var (client, _, sheet, recipeId, _, _) = await SetUpCharacterWithRecipeAsync("Comum");

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var stranger = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        AuthHelper.SetBearerToken(client, stranger.AccessToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/crafting-projects/validate-start?recipeCatalogEntryId={recipeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

- [ ] **Step 4: Run → fail** (endpoint doesn't exist yet).

- [ ] **Step 5: Service interface**

Add to `src/Ruptura.Application/Interfaces/ICharacterSheetService.cs`, after `ValidateTechniqueProjectStartAsync`:
```csharp
    Task<Result<TechniqueProjectValidation>> ValidateCraftingProjectStartAsync(
        Guid callerId, Guid sheetId, Guid recipeCatalogEntryId, CancellationToken ct = default);
```

- [ ] **Step 6: Implement in `CharacterSheetService`**

Add this method (anywhere among the public methods, e.g. right after `ValidateTechniqueProjectStartAsync`):
```csharp
    public async Task<Result<TechniqueProjectValidation>> ValidateCraftingProjectStartAsync(
        Guid callerId, Guid sheetId, Guid recipeCatalogEntryId, CancellationToken ct = default)
    {
        var authorized = await AuthorizeAccessAsync(callerId, sheetId, ct);
        if (authorized.IsFailure)
            return Result.Failure<TechniqueProjectValidation>(authorized.Error!);
        var sheet = authorized.Value!;

        var recipeEntry = await catalogRepo.GetByIdAsync(recipeCatalogEntryId, ct);
        var rarity = recipeEntry is not null ? SafeDeserializeEquipment(recipeEntry.DataJson)?.Rarity ?? string.Empty : string.Empty;
        if (recipeEntry is null || recipeEntry.Type != CatalogEntryType.EquipmentItem ||
            (recipeEntry.CampaignId is { } scope && scope != sheet.CampaignId) ||
            !CraftingReference.IsCraftable(rarity))
            return Result.Failure<TechniqueProjectValidation>(ErrorCodes.CharacterSheet.RecipeNotFound);

        var data = DeserializeSheetData(sheet.DataJson);
        var requiredDays = CraftingReference.RequiredDaysByRarity[rarity];

        if (data.KnownRecipes.All(r => r.CatalogEntryId != recipeCatalogEntryId))
            return Result.Success(new TechniqueProjectValidation
            {
                CanStart = false, BlockedReason = "RecipeNotKnown", RequiredDays = requiredDays
            });

        var (installationId, minLevel) = CraftingReference.InstallationByRarity[rarity];
        var guild = await guildRepo.GetByCampaignAsync(sheet.CampaignId, ct);
        var hasInstallation = guild is not null &&
            (await buildingRepo.GetByGuildAsync(guild.Id, ct)).Any(b =>
                b.CatalogEntryId == installationId && b.IsActive && b.Level >= minLevel);
        if (!hasInstallation)
            return Result.Success(new TechniqueProjectValidation
            {
                CanStart = false, BlockedReason = "MissingInstallation", RequiredDays = requiredDays
            });

        return Result.Success(new TechniqueProjectValidation { CanStart = true, RequiredDays = requiredDays });
    }

    // Mirrors CharacterStatsCalculator.SafeDeserialize / SafeDeserializeSkill — a GM's
    // malformed homebrew EquipmentItem DataJson must never 500 a crafting validation.
    private static EquipmentItemCatalogData? SafeDeserializeEquipment(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<EquipmentItemCatalogData>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
```

In `NormalizeSheetData`, add after `data.TechniqueProjects ??= [];`:
```csharp
        data.KnownRecipes ??= [];
        data.CraftingProjects ??= [];
```

- [ ] **Step 7: Controller endpoint**

In `src/Ruptura.API/Controllers/CharacterSheetController.cs`, add after `ValidateTechniqueProjectStart`:
```csharp
    [HttpGet("character-sheets/{id:guid}/crafting-projects/validate-start")]
    [ProducesResponseType(typeof(ApiResponse<TechniqueProjectValidation>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateCraftingProjectStart(
        Guid id, [FromQuery] Guid recipeCatalogEntryId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.ValidateCraftingProjectStartAsync(callerId, id, recipeCatalogEntryId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<TechniqueProjectValidation>.Ok(result.Value!));
    }
```
(No `[ProducesResponseType(..., Status400BadRequest)]`/no `BadRequest` branch — every failure path from `ValidateCraftingProjectStartAsync` is `NotFound`/`RecipeNotFound`, both 404, per the plan header's precedent note.)

- [ ] **Step 8: Run tests → pass; full sweep; commit**

Run: `dotnet test tests/Ruptura.IntegrationTests --filter FullyQualifiedName~CharacterSheetCraftingProjectTests` then the FULL suite: `dotnet build && dotnet test tests/Ruptura.UnitTests && dotnet test tests/Ruptura.IntegrationTests` (no filters).

```bash
git add src/Ruptura.Application src/Ruptura.Infrastructure/Services/CharacterSheetService.cs \
  src/Ruptura.API/Controllers/CharacterSheetController.cs src/Ruptura.API/Resources \
  tests/Ruptura.IntegrationTests/Controllers/CharacterSheetCraftingProjectTests.cs
git commit -m "feat: add crafting-project start validation endpoint

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

### Task 3: Blazor UI — Known Recipes + Crafting Projects

**Files:** modify `ICharacterSheetClientService.cs`, `CharacterSheetClientService.cs`, `CharacterSheetEquipmentTab.razor`, `CharacterSheetEditor.razor`, `AppStrings*.resx`; create `CharacterSheetCraftingProjectsTab.razor`.

**Interfaces:**
- Consumes: `ICharacterSheetClientService.ValidateCraftingProjectStartAsync`, `EquipmentReference.MaxDurabilityFor` (sub-project #2), `ICatalogClientService.GetByTypeAsync` (existing), `TechniqueProjectValidation` (reused).

- [ ] **Step 1: Client method**

Add to `src/Ruptura.Web/Services/ICharacterSheetClientService.cs`, after `ValidateTechniqueProjectStartAsync`:
```csharp
    Task<ApiResponse<TechniqueProjectValidation>?> ValidateCraftingProjectStartAsync(Guid sheetId, Guid recipeCatalogEntryId);
```

Add to `src/Ruptura.Web/Services/CharacterSheetClientService.cs`, after `ValidateTechniqueProjectStartAsync`:
```csharp
    public async Task<ApiResponse<TechniqueProjectValidation>?> ValidateCraftingProjectStartAsync(
        Guid sheetId, Guid recipeCatalogEntryId)
    {
        var response = await Http.GetAsync(
            $"api/character-sheets/{sheetId}/crafting-projects/validate-start?recipeCatalogEntryId={recipeCatalogEntryId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<TechniqueProjectValidation>>();
    }
```

- [ ] **Step 2: Known Recipes section on the Equipment tab**

In `src/Ruptura.Web/Pages/CharacterSheetEquipmentTab.razor`, add a new section right after the closing `</div>` of the currency/carry-capacity block (before `<div class="master-detail">`):
```razor
<div style="margin-bottom:1.5rem">
    <h4>@L["Sheet.Crafting.KnownRecipes"]</h4>
    <CharacterSheetCatalogRefListTab Entries="Data.KnownRecipes" CampaignId="CampaignId" CatalogType="EquipmentItem" />
</div>
```
No `@code` changes needed in this file — `CharacterSheetCatalogRefListTab` is fully self-contained (it fetches its own catalog list via its own `CatalogType` parameter).

- [ ] **Step 3: `CharacterSheetCraftingProjectsTab.razor`**

`src/Ruptura.Web/Pages/CharacterSheetCraftingProjectsTab.razor`:
```razor
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
        <h4>@L["Sheet.Crafting.StartProject"]</h4>
        <select class="form-select" @bind="_newProjectRecipeId">
            <option value="">@L["Sheet.Crafting.SelectRecipe"]</option>
            @foreach (var recipe in Data.KnownRecipes)
            {
                <option value="@recipe.CatalogEntryId">@NameOf(recipe.CatalogEntryId)</option>
            }
        </select>
        <button class="btn btn-primary btn-sm" @onclick="ValidateAndStartAsync" disabled="@(_validating || _newProjectRecipeId == Guid.Empty)">
            @if (_validating) { <span class="spinner-border spinner-border-sm me-1"></span> }
            @L["Sheet.Crafting.ValidateAndStart"]
        </button>
    </div>

    @if (Data.CraftingProjects.Count == 0)
    {
        <p>@L["Sheet.Crafting.NoProjects"]</p>
    }
    else
    {
        <div class="ledger-table-wrap">
            <table class="ledger-table stack-mobile">
                <thead>
                    <tr>
                        <th>@L["Gm.CampaignDetail.Col.Name"]</th>
                        <th>@L["Sheet.Technique.Progress"]</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var project in Data.CraftingProjects)
                    {
                        <tr>
                            <td data-label="@L["Gm.CampaignDetail.Col.Name"]">@NameOf(project.RecipeCatalogEntryId)</td>
                            <td data-label="@L["Sheet.Technique.Progress"]">@project.DaysInvested / @project.RequiredDays</td>
                            <td data-label="">
                                @if (project.DaysInvested < project.RequiredDays)
                                {
                                    <input type="number" class="form-control form-control-sm" style="width:80px;display:inline-block" min="1" value="1" @onchange="e => _addDays[project.Id] = ParseInt(e.Value)" />
                                    <button class="btn btn-outline-secondary btn-sm" @onclick="() => AdvanceDays(project)">@L["Sheet.Technique.AdvanceDays"]</button>
                                }
                                else if (IsGameMaster)
                                {
                                    <button class="btn btn-primary btn-sm" @onclick="() => ResolveSuccessAsync(project)">@L["Sheet.Technique.Sucesso"]</button>
                                    <button class="btn btn-outline-danger btn-sm" @onclick="() => ResolveFailureAsync(project)">@L["Sheet.Technique.Falha"]</button>
                                }
                                else
                                {
                                    <span>@L["Sheet.Technique.AwaitingGm"]</span>
                                }
                                <button class="btn btn-outline-secondary btn-sm" @onclick="() => RemoveProject(project)">@L["Sheet.RefList.Remove"]</button>
                            </td>
                        </tr>
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

    private List<CatalogEntryResponse> _allRecipes = [];
    private bool _loading = true;
    private Guid _newProjectRecipeId;
    private bool _validating;
    private readonly Dictionary<Guid, int> _addDays = [];

    protected override async Task OnInitializedAsync()
    {
        _allRecipes = (await CatalogService.GetByTypeAsync("EquipmentItem", CampaignId, includeArchived: true))?.Data?.ToList() ?? [];
        _loading = false;
    }

    private string NameOf(Guid id) => _allRecipes.FirstOrDefault(e => e.Id == id)?.Name ?? id.ToString();
    private string RarityOf(Guid id) =>
        CatalogEntryData.Parse(_allRecipes.FirstOrDefault(e => e.Id == id)?.DataJson).GetString("Rarity");
    private static int ParseInt(object? value) => int.TryParse(value?.ToString(), out var v) && v > 0 ? v : 1;

    // Capture the bound value into a local BEFORE the await — see design spec / [[project-character-interlude]]
    // decision 4 (sub-project #3's final review caught this exact race: re-reading a @bind field
    // after an await lets the user change it mid-flight, storing a project the server never validated).
    private async Task ValidateAndStartAsync()
    {
        if (_newProjectRecipeId == Guid.Empty) return;
        var recipeId = _newProjectRecipeId;

        _validating = true;
        try
        {
            var result = await SheetService.ValidateCraftingProjectStartAsync(SheetId, recipeId);
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

            Data.CraftingProjects.Add(new CharacterCraftingProject
            {
                Id = Guid.NewGuid(), RecipeCatalogEntryId = recipeId, RequiredDays = result.Data.RequiredDays, DaysInvested = 0
            });
            Toast.Success(L["Sheet.Crafting.Started"]);
            _newProjectRecipeId = Guid.Empty;
        }
        finally
        {
            _validating = false;
        }
    }

    private void RemoveProject(CharacterCraftingProject project)
    {
        Data.CraftingProjects.Remove(project);
        _addDays.Remove(project.Id);
    }

    private void AdvanceDays(CharacterCraftingProject project)
    {
        var days = _addDays.GetValueOrDefault(project.Id, 1);
        project.DaysInvested = Math.Min(project.RequiredDays, project.DaysInvested + days);
    }

    private void ResolveSuccessAsync(CharacterCraftingProject project)
    {
        var maxDurability = EquipmentReference.MaxDurabilityFor(RarityOf(project.RecipeCatalogEntryId));
        Data.Equipment.Add(new CharacterEquipmentEntry
        {
            CatalogEntryId = project.RecipeCatalogEntryId, Quantity = 1, DurabilityRemaining = maxDurability
        });
        Data.CraftingProjects.Remove(project);
        Toast.Success(L["Sheet.Crafting.Success"]);
    }

    private async Task ResolveFailureAsync(CharacterCraftingProject project)
    {
        var confirmed = await Confirm.AskAsync(
            L["Sheet.Technique.FailureConfirm.Title"], L["Sheet.Technique.FailureConfirm.Message"],
            L["Sheet.Technique.Falha"], L["Sheet.Equipment.Cancel"]);
        if (!confirmed) return;

        project.DaysInvested = 0;
        Toast.Success(L["Sheet.Technique.Failed"]);
    }
}
```
> `ResolveSuccessAsync` is synchronous (`void`, not `async Task`) despite its name — Sucesso here makes NO server call at all (Decision #8: the recipe already exists in the catalog, nothing to POST), unlike Técnica Creation's Sucesso. Keep the `Async` suffix for naming symmetry with `ResolveFailureAsync`/the sibling component's method names, but don't add an unnecessary `async`/`await Task.CompletedTask` — a plain synchronous method assigned to `@onclick` works fine in Blazor.
>
> This component reuses `Sheet.Technique.Progress`/`Sheet.Technique.AdvanceDays`/`Sheet.Technique.Sucesso`/`Sheet.Technique.Falha`/`Sheet.Technique.AwaitingGm`/`Sheet.Technique.Blocked.{X}`/`Sheet.Technique.FailureConfirm.*`/`Sheet.Technique.Failed`/`Sheet.RefList.Remove`/`Sheet.Equipment.Cancel` — all already exist from sub-project #3, no duplicate keys needed for these. `Sheet.Technique.Blocked.RecipeNotKnown` does NOT exist yet (sub-project #3 only added `InsufficientSkill`/`InsufficientRanking`/`MissingInstallation`) — Step 4 must add it. `MissingInstallation` already exists and is reused as-is (same meaning, same string).

- [ ] **Step 4: Mount + i18n**

In `src/Ruptura.Web/Pages/CharacterSheetEditor.razor`, add the new component right after `CharacterSheetTechniqueProjectsTab` inside the existing `"training"` branch:
```razor
        else if (_activeTab == "training")
        {
            <CharacterSheetTrainingTab Data="_data" CampaignId="CampaignId" SheetId="SheetId" IsGameMaster="CanEditStatus" />
            <hr />
            <CharacterSheetTechniqueProjectsTab Data="_data" CampaignId="CampaignId" SheetId="SheetId" IsGameMaster="CanEditStatus" />
            <hr />
            <CharacterSheetCraftingProjectsTab Data="_data" CampaignId="CampaignId" SheetId="SheetId" IsGameMaster="CanEditStatus" />
        }
```

`src/Ruptura.Web/Resources/AppStrings.resx`, add after the existing `Sheet.Equipment.NoneSkill` entry (search for it — the Known Recipes section's own label lives near the Equipment tab's other strings):
```xml
  <data name="Sheet.Crafting.KnownRecipes"><value>Known Recipes</value></data>
```
Add after the existing `Sheet.Technique.Failed` entry (search for it — the crafting-project strings live near the sibling Técnica ones):
```xml
  <data name="Sheet.Crafting.StartProject"><value>Start Crafting Project</value></data>
  <data name="Sheet.Crafting.SelectRecipe"><value>— select a recipe —</value></data>
  <data name="Sheet.Crafting.ValidateAndStart"><value>Validate and Start</value></data>
  <data name="Sheet.Crafting.Started"><value>Project started.</value></data>
  <data name="Sheet.Crafting.NoProjects"><value>No crafting projects in progress.</value></data>
  <data name="Sheet.Crafting.Success"><value>Item crafted!</value></data>
  <data name="Sheet.Technique.Blocked.RecipeNotKnown"><value>This recipe is not known.</value></data>
```
`src/Ruptura.Web/Resources/AppStrings.pt-BR.resx`, same positions:
```xml
  <data name="Sheet.Crafting.KnownRecipes"><value>Receitas Conhecidas</value></data>
```
```xml
  <data name="Sheet.Crafting.StartProject"><value>Iniciar Projeto de Fabricação</value></data>
  <data name="Sheet.Crafting.SelectRecipe"><value>— selecione uma receita —</value></data>
  <data name="Sheet.Crafting.ValidateAndStart"><value>Validar e Iniciar</value></data>
  <data name="Sheet.Crafting.Started"><value>Projeto iniciado.</value></data>
  <data name="Sheet.Crafting.NoProjects"><value>Nenhum projeto de fabricação em andamento.</value></data>
  <data name="Sheet.Crafting.Success"><value>Item fabricado!</value></data>
  <data name="Sheet.Technique.Blocked.RecipeNotKnown"><value>Essa receita não é conhecida.</value></data>
```

- [ ] **Step 5: Build + verify + full sweep + commit**

Run: `dotnet build` (clean, both API and Web projects). If feasible, run the app and confirm: a known recipe can start a project once the right installation is built; advancing days works and caps at `RequiredDays`; GM-only Sucesso adds the item to Equipment and removes the project; Falha resets days; Remove deletes a project. Else confirm a clean build and note it. Then run the FULL solution test suite (`dotnet build && dotnet test tests/Ruptura.UnitTests && dotnet test tests/Ruptura.IntegrationTests`, no filters).

```bash
git add src/Ruptura.Web
git commit -m "feat: add character crafting UI (known recipes, start/advance/resolve project)

Co-Authored-By: Claude Sonnet 5 <noreply@anthropic.com>"
```

---

## Self-Review

**1. Spec coverage:**
- §3/§4 tables (RequiredDays/MaterialsCost/InstallationByRarity, Divino excluded, Ferraria/OficinaDeRunas reconciliation) → Task 1 `CraftingReference`. ✓
- §5 data model (`KnownRecipes`, `CraftingProjects`, `CharacterCraftingProject`) → Task 1. ✓
- §6 `TechniqueProjectValidation` reuse (no new DTO) → Task 1/2 (no new file created for it). ✓
- §7/§8 API (validate-start endpoint, RecipeNotFound/RecipeNotKnown/MissingInstallation logic) → Task 2, with the plan-header status-code refinement applied (single 404, not a 400/404 split). ✓
- §9.1 Known Recipes reusing `CharacterSheetCatalogRefListTab` → Task 3 Step 2 (zero new component). ✓
- §9.2 Crafting Projects UI → Task 3 Step 3. ✓
- §10 testing (table-key-consistency unit test done proactively in Task 1, not deferred to a review) → Task 1 Step 2. ✓
- **Deliberately out of scope (per spec §1, not gaps):** Divino, material-cost deduction, 5-tier outcome scale, Melhoria/Modificação/Reconstrução, a skill-linkage/Perícia-mínima gate.

**2. Placeholder scan:** every step has complete, real code. The `ResolveSuccessAsync` non-async-despite-name note and the "which resx keys are reused vs. new" note are called out explicitly rather than left for the implementer to guess.

**3. Type consistency:** `TechniqueProjectValidation` used identically to sub-project #3 (no field changes). `ValidateCraftingProjectStartAsync(Guid, Guid, Guid, CancellationToken)` identical across the interface, implementation, controller, and client. `CharacterCraftingProject{Id, RecipeCatalogEntryId, RequiredDays, DaysInvested}` identical across the model (Task 1) and every read/write site (Task 3). `CraftingReference.InstallationByRarity`'s `(Guid InstallationId, int MinLevel)` tuple shape is read identically in the one place it's consumed (Task 2 Step 6). `BlockedReason` wire values (`"RecipeNotKnown"`/`"MissingInstallation"`) are consistent across the service, the tests, and the UI's resx-key interpolation (`Sheet.Technique.Blocked.{X}`, reusing sub-project #3's established interpolation pattern and its existing `MissingInstallation` key, adding only the new `RecipeNotKnown` key).
