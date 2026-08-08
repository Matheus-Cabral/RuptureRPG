using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildDoctrineTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // Additional seeded Doctrine ids (beyond GuildCatalogIds.DoctrineLogistica/DoctrineComercial).
    private static readonly Guid DoctrineMilitar = Guid.Parse("d1000000-0000-0000-0000-000000000001");
    private static readonly Guid DoctrineAcademica = Guid.Parse("d1000000-0000-0000-0000-000000000002");

    private async Task<(HttpClient Client, CampaignResponse Campaign, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Guild Doctrine Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        return (client, campaign, player.AccessToken, gm.AccessToken);
    }

    private async Task<GuildSheetResponse> GetGuildAsync(HttpClient client, Guid campaignId)
    {
        var response = await client.GetAsync($"api/campaigns/{campaignId}/guild");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;
    }

    private static string Serialize(GuildSheetData data) => JsonSerializer.Serialize(data, JsonOpts);

    private static UpdateGuildSheetRequest UpdateWith(GuildSheetResponse current) => new()
    {
        GuildName = current.GuildName,
        DataJson = Serialize(current.Data),
        Version = current.Version
    };

    [Fact]
    public async Task Update_TwoDoctrinesNoCamara_Returns200AndPersists()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var current = await GetGuildAsync(client, campaign.Id);
        current.Data.ActiveDoctrineIds =
            [GuildCatalogIds.DoctrineLogistica, GuildCatalogIds.DoctrineComercial];

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", UpdateWith(current));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reloaded = await GetGuildAsync(client, campaign.Id);
        reloaded.Data.ActiveDoctrineIds.Should().BeEquivalentTo(
            new[] { GuildCatalogIds.DoctrineLogistica, GuildCatalogIds.DoctrineComercial });
    }

    [Fact]
    public async Task Update_ThreeDoctrinesNoCamara_Returns200AndFlagsOverflow()
    {
        // The doctrine limit is ADVISORY (mirrors CS active-building overflow): being over the limit
        // never blocks the save — DerivedStats.ActiveDoctrineOverflow surfaces it instead.
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var current = await GetGuildAsync(client, campaign.Id);
        current.Data.ActiveDoctrineIds =
            [GuildCatalogIds.DoctrineLogistica, GuildCatalogIds.DoctrineComercial, DoctrineMilitar];

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", UpdateWith(current));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reloaded = await GetGuildAsync(client, campaign.Id);
        reloaded.Data.ActiveDoctrineIds.Should().HaveCount(3);
        reloaded.DerivedStats.DoctrineLimit.Should().Be(2); // no Câmara → min(4, 2+0)
        reloaded.DerivedStats.ActiveDoctrineCount.Should().Be(3);
        reloaded.DerivedStats.ActiveDoctrineOverflow.Should().BeTrue();
    }

    [Fact]
    public async Task Update_CamaraLevel2ThenFourDoctrines_Returns200()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        // Câmara do Conselho level 2 (active) → doctrine limit = min(4, 2 + 2) = 4.
        var built = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.CamaraDoConselho, Level = 2, IsActive = true });
        built.StatusCode.Should().Be(HttpStatusCode.Created);

        var current = await GetGuildAsync(client, campaign.Id);
        current.Data.ActiveDoctrineIds =
        [
            GuildCatalogIds.DoctrineLogistica,
            GuildCatalogIds.DoctrineComercial,
            DoctrineMilitar,
            DoctrineAcademica
        ];

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", UpdateWith(current));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reloaded = await GetGuildAsync(client, campaign.Id);
        reloaded.Data.ActiveDoctrineIds.Should().HaveCount(4);
        reloaded.DerivedStats.DoctrineLimit.Should().Be(4);
        reloaded.DerivedStats.ActiveDoctrineOverflow.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ActiveDoctrineIdNotADoctrine_Returns400DoctrineInvalid()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var current = await GetGuildAsync(client, campaign.Id);
        // Armazém is an Installation, not a Doctrine.
        current.Data.ActiveDoctrineIds = [GuildCatalogIds.Armazem];

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", UpdateWith(current));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_MainDoctrineIdNotADoctrine_Returns400DoctrineInvalid()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var current = await GetGuildAsync(client, campaign.Id);
        // Armazém is an Installation, not a Doctrine.
        current.Data.Identity.MainDoctrineId = GuildCatalogIds.Armazem;

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", UpdateWith(current));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_EmptyActiveDoctrineIds_Returns200()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var current = await GetGuildAsync(client, campaign.Id);
        current.Data.ActiveDoctrineIds = [];
        current.Data.Identity.MainDoctrineId = null;

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", UpdateWith(current));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
