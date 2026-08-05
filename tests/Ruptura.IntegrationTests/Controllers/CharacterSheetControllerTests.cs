using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Infrastructure.Data;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class CharacterSheetControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, CampaignResponse Campaign, Guid PlayerId, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Sheet Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        return (client, campaign, player.User.Id, player.AccessToken, gm.AccessToken);
    }

    [Fact]
    public async Task Grant_AsCampaignGameMaster_Returns201WithEmptyDefaultData()
    {
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        body.CharacterName.Should().Be("Sir Aldric");
        body.DerivedStats.MaxHp.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Grant_ASecondAliveCharacterForTheSamePlayer_Returns400()
    {
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "First" });

        var second = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Second" });

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetMine_AsThePlayerWithAGrantedSheet_ReturnsIt()
    {
        var (client, campaign, playerId, playerToken, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });

        AuthHelper.SetBearerToken(client, playerToken);
        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/character-sheets/mine");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        body.CharacterName.Should().Be("Sir Aldric");
    }

    [Fact]
    public async Task Update_AsPlayerAttemptingToMarkOwnCharacterDead_Returns400()
    {
        var (client, campaign, playerId, playerToken, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        AuthHelper.SetBearerToken(client, playerToken);
        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName, DataJson = "{}", IsDead = true
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_AsPlayerUpdatingOwnSheet_Returns200WithGeneralFieldsUpdated()
    {
        // The single most common real user journey — a player saving their own sheet —
        // had zero direct coverage before this test.
        var (client, campaign, playerId, playerToken, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        AuthHelper.SetBearerToken(client, playerToken);
        sheet.Data.Attributes.Corpo = 3;
        sheet.Data.Identity.PatronDisplayName = "Dom Alric";
        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = "Sir Aldric the Bold",
            DataJson = System.Text.Json.JsonSerializer.Serialize(sheet.Data),
            PortraitImagePath = "https://example.com/portrait.png"
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        body.CharacterName.Should().Be("Sir Aldric the Bold");
        body.PortraitImagePath.Should().Be("https://example.com/portrait.png");
        body.Data.Attributes.Corpo.Should().Be(3);
        body.Data.Identity.PatronDisplayName.Should().Be("Dom Alric");
        body.IsDead.Should().BeFalse();
        body.IsRetired.Should().BeFalse();
    }

    [Fact]
    public async Task Update_AsCampaignGameMaster_CanMarkCharacterDead()
    {
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName, DataJson = "{}", IsDead = true
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        body.IsDead.Should().BeTrue();
    }

    [Fact]
    public async Task Get_AsUnrelatedPlayer_Returns404()
    {
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        var outsider = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var response = await client.GetAsync($"api/character-sheets/{sheet.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Grant_TwoSimultaneousGrantsForTheSamePlayer_OnlyOneSucceeds()
    {
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var request = () => client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Race Condition" });

        var results = await Task.WhenAll(request(), request());

        results.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
        results.Count(r => r.StatusCode == HttpStatusCode.BadRequest).Should().Be(1);
    }

    [Fact]
    public async Task ArchivedHomebrewCatalogEntry_StillResolvesOnASheetThatReferencesIt()
    {
        var (client, campaign, playerId, playerToken, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var talentResponse = await client.PostAsJsonAsync("api/catalog", new Ruptura.Shared.Catalog.CreateCatalogEntryRequest
        {
            CampaignId = campaign.Id, Type = "Talent", Name = "Soon To Be Retired",
            DataJson = """{"Category":"Combate","Effect":"x","PowerTier":"menor"}"""
        });
        var talent = (await talentResponse.Content.ReadFromJsonAsync<ApiResponse<Ruptura.Shared.Catalog.CatalogEntryResponse>>())!.Data!;

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        sheet.Data.Talents.Add(new Ruptura.Shared.CharacterSheets.CharacterCatalogRefEntry { CatalogEntryId = talent.Id });
        var putResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName,
            DataJson = System.Text.Json.JsonSerializer.Serialize(sheet.Data)
        });
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // GM archives the homebrew talent (Task 9's soft-delete).
        await client.DeleteAsync($"api/catalog/{talent.Id}");

        // The character sheet still resolves the reference — no 500, NP still includes it.
        var getResponse = await client.GetAsync($"api/character-sheets/{sheet.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reread = (await getResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        reread.Data.Talents.Should().ContainSingle(t => t.CatalogEntryId == talent.Id);
    }

    [Fact]
    public async Task Get_SheetWithNullModuleInStoredDataJson_Returns200WithSensibleDefaults()
    {
        // UpdateCharacterSheetRequestValidator rejects a DataJson with a null module before
        // it can ever be saved through the API — so simulate a write path that bypasses it
        // entirely (direct DB write, a migration, a future endpoint) by writing straight to
        // the row via a scoped AppDbContext, then confirm the real read path
        // (MapToResponseAsync → DeserializeSheetData → CollectReferencedCatalogIds →
        // CharacterStatsCalculator.Calculate) tolerates it instead of 500ing.
        var (client, campaign, playerId, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Corrupted Row" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entity = await db.CharacterSheets.SingleAsync(s => s.Id == sheet.Id);
            entity.DataJson = """{"Skills":null,"Talents":null,"Equipment":null}""";
            await db.SaveChangesAsync();
        }

        var getResponse = await client.GetAsync($"api/character-sheets/{sheet.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await getResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        body.Data.Skills.Should().NotBeNull().And.BeEmpty();
        body.Data.Talents.Should().NotBeNull().And.BeEmpty();
        body.Data.Equipment.Should().NotBeNull().And.BeEmpty();
        body.DerivedStats.MaxHp.Should().BeGreaterThan(0);
    }
}
