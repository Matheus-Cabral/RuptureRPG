using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.Common;

namespace Ruptura.IntegrationTests.Controllers;

public class CatalogControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, Guid CampaignId)> SetupGameMasterWithCampaignAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "Catalog Test Campaign"
        });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        return (client, campaign.Id);
    }

    [Fact]
    public async Task GetByType_ReturnsOfficialSeedEntries()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();

        var response = await client.GetAsync($"api/catalog?type=Origin&campaignId={campaignId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>();
        body!.Data.Should().NotBeEmpty();
        body.Data.Should().OnlyContain(e => e.Type == "Origin");
        body.Data.Should().Contain(e => e.IsGlobal);
    }

    [Fact]
    public async Task GetByType_WithInvalidType_Returns400()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();

        var response = await client.GetAsync($"api/catalog?type=NotAType&campaignId={campaignId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByType_WhenCallerNotCampaignMember_Returns404()
    {
        var (_, campaignId) = await SetupGameMasterWithCampaignAsync();
        var (strangerClient, _) = await SetupGameMasterWithCampaignAsync();

        var response = await strangerClient.GetAsync($"api/catalog?type=Origin&campaignId={campaignId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateHomebrewEntry_ThenListIncludesIt_Returns201()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();

        var createResponse = await client.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaignId,
            Type = "Talent",
            Name = "Talento Homebrew de Teste",
            DataJson = "{\"Category\":\"Combate\",\"Effect\":\"teste\",\"PowerTier\":\"menor\"}"
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await client.GetAsync($"api/catalog?type=Talent&campaignId={campaignId}");
        var list = (await listResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!;
        list.Should().Contain(e => e.Name == "Talento Homebrew de Teste" && !e.IsGlobal);
    }

    [Fact]
    public async Task CreateHomebrewEntry_WithDuplicateName_Returns400()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();
        var request = new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "Duplicado", DataJson = "{}"
        };
        await client.PostAsJsonAsync("api/catalog", request);

        var response = await client.PostAsJsonAsync("api/catalog", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateGlobalEntry_Returns400()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();
        var listResponse = await client.GetAsync($"api/catalog?type=Origin&campaignId={campaignId}");
        var globalEntry = (await listResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!.First();

        var response = await client.PutAsJsonAsync($"api/catalog/{globalEntry.Id}", new UpdateCatalogEntryRequest
        {
            Name = "Tentativa de Edição", DataJson = "{}"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateHomebrewEntry_ByAnotherGameMaster_Returns404()
    {
        var (owner, campaignId) = await SetupGameMasterWithCampaignAsync();
        var createResponse = await owner.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "Meu Talento", DataJson = "{}"
        });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>())!.Data!;

        var (stranger, _) = await SetupGameMasterWithCampaignAsync();
        var response = await stranger.PutAsJsonAsync($"api/catalog/{entry.Id}", new UpdateCatalogEntryRequest
        {
            Name = "Roubado", DataJson = "{}"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteHomebrewEntry_RemovesIt()
    {
        var (client, campaignId) = await SetupGameMasterWithCampaignAsync();
        var createResponse = await client.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "Para Apagar", DataJson = "{}"
        });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>())!.Data!;

        var deleteResponse = await client.DeleteAsync($"api/catalog/{entry.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await client.GetAsync($"api/catalog?type=Talent&campaignId={campaignId}");
        var list = (await listResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CatalogEntryResponse>>>())!.Data!;
        list.Should().NotContain(e => e.Id == entry.Id);
    }

    [Fact]
    public async Task WriteEndpoints_WithoutGameMasterRole_Return403()
    {
        var (gmClient, campaignId) = await SetupGameMasterWithCampaignAsync();

        // Register a player under this GM and try to create a homebrew entry as them.
        var inviteResponse = await gmClient.PostAsync("api/invites", null);
        var invite = (await inviteResponse.Content
            .ReadFromJsonAsync<ApiResponse<Ruptura.Shared.Invites.InviteCodeResponse>>())!.Data!;
        var player = await AuthHelper.RegisterPlayerAsync(factory.CreateClient(), invite.Code, Faker.Internet.Email());

        var playerClient = factory.CreateClient();
        AuthHelper.SetBearerToken(playerClient, player.AccessToken);

        var response = await playerClient.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaignId, Type = "Talent", Name = "X", DataJson = "{}"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
