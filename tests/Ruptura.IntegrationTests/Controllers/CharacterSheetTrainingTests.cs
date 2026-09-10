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
        // CatalogEntryService.CreateAsync rejects a name that collides with a GLOBAL entry of the
        // same type regardless of campaign (ErrorCodes.Catalog.AlreadyExists) — "Espadas" is already
        // seeded as a global Skill (CatalogSeedData.Skills.cs), so a unique suffix per call keeps
        // this test isolated from that seed data instead of colliding with it.
        var uniqueName = $"{name} {Guid.NewGuid():N}";
        var response = await client.PostAsJsonAsync("api/catalog", new
        {
            CampaignId = campaignId, Type = "Skill", Name = uniqueName,
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

        // GDD §6.5 "Sem Treinamento": below 10 Points, installation/instructor bonuses are ignored
        // entirely (TrainingCalculatorTests.SemTreinamento_*) — give the skill 15 Points so the
        // Campo de Treinamento bonus this test is actually named for has a chance to apply.
        await client.PutAsJsonAsync($"api/character-sheets/{sheetId}", new UpdateCharacterSheetRequest
        {
            CharacterName = "Trainee",
            DataJson = $"{{\"Skills\":[{{\"CatalogEntryId\":\"{skillId}\",\"Points\":15}}]}}"
        });

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync(
            $"api/character-sheets/{sheetId}/training/preview?skillCatalogEntryId={skillId}&days=10&correlation=Media");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<TrainingProjection>>())!.Data!;
        body.CurrentPoints.Should().Be(15);
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
