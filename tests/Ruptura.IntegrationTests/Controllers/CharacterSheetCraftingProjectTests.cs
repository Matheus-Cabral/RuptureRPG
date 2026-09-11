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
    public async Task ValidateStart_RarityWithWhitespace_DoesNotThrow_ResolvesNormally()
    {
        var (client, campaign, sheet, recipeId, playerToken, gmToken) = await SetUpCharacterWithRecipeAsync("  Épico  ");

        AuthHelper.SetBearerToken(client, gmToken);
        await client.GetAsync($"api/campaigns/{campaign.Id}/guild");
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Ferraria, Level = 3, IsActive = true });

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheet.Id}/crafting-projects/validate-start?recipeCatalogEntryId={recipeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
