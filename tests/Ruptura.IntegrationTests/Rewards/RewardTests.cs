using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Bestiary;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Encounters;
using Ruptura.Shared.Rewards;

namespace Ruptura.IntegrationTests.Rewards;

// End-to-end coverage for the reward planner (GM-3). The reward package is persisted as a typed
// blob scoped to one campaign; the service is campaign-ownership authoritative (a non-owned/missing
// campaign or a foreign reward yields Reward.NotFound — existence hidden), clamps VE/resource ints,
// validates strategic-asset categories against RewardReference, and validates an optional EncounterId
// against the same campaign, resolving the linked encounter's name at read time.
public class RewardTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, string GmToken, CampaignResponse Campaign)> SetupGmWithCampaignAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Reward Campaign" });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        return (client, gm.AccessToken, campaign);
    }

    private static async Task<Guid> CreateCreatureAsync(HttpClient client, string name = "Goblin")
    {
        var resp = await client.PostAsJsonAsync("api/bestiary/creatures", new CreateCreatureRequest
        {
            Name = name,
            Data = new CreatureData
            {
                Type = "homebrew",
                Function = "Predador",
                Behavior = "Instintiva",
                Category = "Comum",
                Characteristics = [new CreatureCharacteristic { Name = "Casca", Weight = "Media" }],
                Abilities = [new CreatureAbility { Name = "Mordida", Tier = "Comum" }],
                Equipment = [new CreatureEquipment { Name = "Adaga", Rarity = "Raro" }],
                Fraqueza = "Fogo",
                Pv = 20,
                DefesaPassiva = 12,
                Deslocamento = 9
            }
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resp.Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>())!.Data!.Id;
    }

    private static async Task<Guid> CreateEncounterAsync(HttpClient client, Guid campaignId, string name = "Ambush")
    {
        var creatureId = await CreateCreatureAsync(client);
        var resp = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/encounters",
            new CreateEncounterRequest
            {
                Name = name,
                Data = new EncounterData
                {
                    Creatures = [new EncounterCreature { CreatureId = creatureId, Quantity = 1 }],
                    Intelligence = "Instinto",
                    Terrain = "Neutro",
                    Objective = "Eliminar",
                    DesiredDifficulty = "Normal",
                    Duration = "Curto",
                    ApplyPressure = false
                }
            });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resp.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!.Id;
    }

    private static RewardData BaseData() => new()
    {
        Silver = 100,
        PactCoins = 5,
        Fragments = 2,
        Cristais = 1,
        Materials = [new RewardMaterial { Name = "Iron", Quantity = 3 }],
        StrategicAssets = [new RewardAsset { Name = "Outpost", Category = "Infraestrutura", Ve = 3 }],
        Knowledge = ["Ancient rune"],
        Items = ["Sword of X"],
        Notes = "Loot from the dungeon"
    };

    [Fact]
    public async Task Create_AsCampaignGameMaster_Returns201()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Chest", Data = BaseData() });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;
        body.Id.Should().NotBeEmpty();
        body.Name.Should().Be("Chest");
        body.Data.Silver.Should().Be(100);
        body.Data.StrategicAssets.Should().ContainSingle(a => a.Name == "Outpost");
    }

    [Fact]
    public async Task GetForCampaign_ReturnsCreatedRewards()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Chest", Data = BaseData() });

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/rewards");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<RewardResponse>>>())!.Data!;
        body.Should().ContainSingle(r => r.Name == "Chest");
    }

    [Fact]
    public async Task GetById_ReturnsReward()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Chest", Data = BaseData() });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/rewards/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;
        body.Id.Should().Be(created.Id);
        body.Name.Should().Be("Chest");
    }

    [Fact]
    public async Task Update_ChangesNameAndData()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Chest", Data = BaseData() });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;

        var data = BaseData();
        data.Silver = 999;
        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards/{created.Id}",
            new UpdateRewardRequest { Name = "Bigger Chest", Data = data });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;
        body.Name.Should().Be("Bigger Chest");
        body.Data.Silver.Should().Be(999);
    }

    [Fact]
    public async Task Delete_RemovesReward()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Chest", Data = BaseData() });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;

        var delete = await client.DeleteAsync($"api/campaigns/{campaign.Id}/rewards/{created.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await client.GetAsync($"api/campaigns/{campaign.Id}/rewards/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ByADifferentGameMaster_Returns404()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Secret", Data = BaseData() });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;

        var (otherClient, otherToken, _) = await SetupGmWithCampaignAsync();
        AuthHelper.SetBearerToken(otherClient, otherToken);

        var response = await otherClient.GetAsync($"api/campaigns/{campaign.Id}/rewards/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithMissingName_Returns400NameRequired()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "   ", Data = BaseData() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Success.Should().BeFalse();
        body.Message.Should().Be("Reward.NameRequired");
    }

    [Fact]
    public async Task Create_ClampsVeToBounds_OnReadBack()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var data = BaseData();
        data.StrategicAssets =
        [
            new RewardAsset { Name = "High", Category = "Infraestrutura", Ve = 9 },
            new RewardAsset { Name = "Low", Category = "Infraestrutura", Ve = 0 }
        ];

        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Clamp", Data = data });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;

        created.Data.StrategicAssets.Single(a => a.Name == "High").Ve.Should().Be(5);
        created.Data.StrategicAssets.Single(a => a.Name == "Low").Ve.Should().Be(1);
    }

    [Fact]
    public async Task Create_WithUnknownCategory_Returns400CategoryInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var data = BaseData();
        data.StrategicAssets = [new RewardAsset { Name = "Bad", Category = "Nonsense", Ve = 2 }];

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Bad Category", Data = data });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Success.Should().BeFalse();
        body.Message.Should().Be("Reward.CategoryInvalid");
    }

    [Fact]
    public async Task Create_WithEncounterFromAnotherCampaign_Returns400EncounterInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        // Same GM owns a second campaign with its own encounter.
        var otherCampaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Other Campaign" });
        var otherCampaign = (await otherCampaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;
        var foreignEncounterId = await CreateEncounterAsync(client, otherCampaign.Id);

        var data = BaseData();
        data.EncounterId = foreignEncounterId;

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Wrong Encounter", Data = data });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Success.Should().BeFalse();
        body.Message.Should().Be("Reward.EncounterInvalid");
    }

    [Fact]
    public async Task Create_WithEncounterFromSameCampaign_ResolvesEncounterName()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var encounterId = await CreateEncounterAsync(client, campaign.Id, "Boss Fight");

        var data = BaseData();
        data.EncounterId = encounterId;

        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Boss Loot", Data = data });

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await create.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;
        body.Data.EncounterId.Should().Be(encounterId);
        body.EncounterName.Should().Be("Boss Fight");
    }

    [Fact]
    public async Task IsGranted_RoundTrips()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var data = BaseData();
        data.IsGranted = true;

        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/rewards",
            new CreateRewardRequest { Name = "Granted", Data = data });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;

        var get = await client.GetAsync($"api/campaigns/{campaign.Id}/rewards/{created.Id}");
        var body = (await get.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!;
        body.Data.IsGranted.Should().BeTrue();
    }
}
