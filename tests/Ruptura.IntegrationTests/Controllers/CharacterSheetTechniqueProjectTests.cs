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
