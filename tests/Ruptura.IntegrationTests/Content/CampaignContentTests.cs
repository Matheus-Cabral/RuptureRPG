using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Bestiary;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Content;
using Ruptura.Shared.Encounters;
using Ruptura.Shared.Rewards;

namespace Ruptura.IntegrationTests.Content;

// End-to-end coverage for session-prep content (GM-5): arcs and floors, persisted as typed blobs
// scoped to one campaign. The service is campaign-ownership authoritative (a non-owned/missing
// campaign, arc or floor yields Content.NotFound — existence hidden). Floors validate their ArcId,
// ObjectiveType, and soft links (encounters/rewards) against the same campaign, resolving link names
// at read time. Deleting an arc cascades its floors (DECISION D1).
public class CampaignContentTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, string GmToken, CampaignResponse Campaign)> SetupGmWithCampaignAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Content Campaign" });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        return (client, gm.AccessToken, campaign);
    }

    private static async Task<Guid> CreateArcAsync(HttpClient client, Guid campaignId, string name = "Arc I")
    {
        var resp = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/arcs",
            new CreateArcRequest { Name = name, Order = 1, Data = new ArcData { Theme = "Descent" } });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resp.Content.ReadFromJsonAsync<ApiResponse<ArcResponse>>())!.Data!.Id;
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

    private static async Task<Guid> CreateRewardAsync(HttpClient client, Guid campaignId, string name = "Chest")
    {
        var resp = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/rewards",
            new CreateRewardRequest { Name = name, Data = new RewardData { Silver = 10 } });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resp.Content.ReadFromJsonAsync<ApiResponse<RewardResponse>>())!.Data!.Id;
    }

    private static FloorData BaseFloorData() => new()
    {
        ObjectiveType = "Exploracao",
        Identity = "The Sunken Hall",
        MainObjective = "Reach the far gate",
        SecondaryObjectives = ["Find the key"],
        FailureCondition = "The water rises",
        Notes = "Slippery"
    };

    // ── Arc CRUD ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateArc_AsCampaignGameMaster_Returns201()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/arcs",
            new CreateArcRequest { Name = "Prologue", Order = 2, Data = new ArcData { Theme = "Dread" } });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<ArcResponse>>())!.Data!;
        body.Id.Should().NotBeEmpty();
        body.Name.Should().Be("Prologue");
        body.Order.Should().Be(2);
        body.Data.Theme.Should().Be("Dread");
    }

    [Fact]
    public async Task GetArcs_ReturnsCreatedArcs()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        await CreateArcAsync(client, campaign.Id, "Arc One");

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/arcs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ArcResponse>>>())!.Data!;
        body.Should().ContainSingle(a => a.Name == "Arc One");
    }

    [Fact]
    public async Task GetArcById_ReturnsArc()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/arcs/{arcId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<ArcResponse>>())!.Data!;
        body.Id.Should().Be(arcId);
    }

    [Fact]
    public async Task UpdateArc_ChangesNameAndData()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);

        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/arcs/{arcId}",
            new UpdateArcRequest { Name = "Renamed", Order = 5, Data = new ArcData { Conflict = "War" } });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<ArcResponse>>())!.Data!;
        body.Name.Should().Be("Renamed");
        body.Order.Should().Be(5);
        body.Data.Conflict.Should().Be("War");
    }

    [Fact]
    public async Task DeleteArc_RemovesArc()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);

        var delete = await client.DeleteAsync($"api/campaigns/{campaign.Id}/arcs/{arcId}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await client.GetAsync($"api/campaigns/{campaign.Id}/arcs/{arcId}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateArc_WithMissingName_Returns400NameRequired()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/arcs",
            new CreateArcRequest { Name = "   ", Order = 1, Data = new ArcData() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Message.Should().Be("Content.NameRequired");
    }

    [Fact]
    public async Task GetArcById_ByADifferentGameMaster_Returns404()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);

        var (otherClient, otherToken, _) = await SetupGmWithCampaignAsync();
        AuthHelper.SetBearerToken(otherClient, otherToken);

        var response = await otherClient.GetAsync($"api/campaigns/{campaign.Id}/arcs/{arcId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Floor CRUD ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateFloor_AsCampaignGameMaster_Returns201()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "Floor 1", Data = BaseFloorData() });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>())!.Data!;
        body.Id.Should().NotBeEmpty();
        body.ArcId.Should().Be(arcId);
        body.Name.Should().Be("Floor 1");
        body.Data.ObjectiveType.Should().Be("Exploracao");
    }

    [Fact]
    public async Task CreateFloor_WithBlankName_IsAllowed()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "", Data = BaseFloorData() });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetFloorsForArc_ReturnsOnlyThatArcsFloors()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arc1 = await CreateArcAsync(client, campaign.Id, "Arc 1");
        var arc2 = await CreateArcAsync(client, campaign.Id, "Arc 2");
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arc1, Number = 1, Name = "A1F1", Data = BaseFloorData() });
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arc2, Number = 1, Name = "A2F1", Data = BaseFloorData() });

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/arcs/{arc1}/floors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<FloorResponse>>>())!.Data!;
        body.Should().ContainSingle(f => f.Name == "A1F1");
    }

    [Fact]
    public async Task GetFloorsForCampaign_ReturnsAllFloors()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arc1 = await CreateArcAsync(client, campaign.Id, "Arc 1");
        var arc2 = await CreateArcAsync(client, campaign.Id, "Arc 2");
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arc1, Number = 1, Name = "A1F1", Data = BaseFloorData() });
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arc2, Number = 1, Name = "A2F1", Data = BaseFloorData() });

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/floors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<FloorResponse>>>())!.Data!;
        body.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFloorById_ReturnsFloor()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);
        var create = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "Floor 1", Data = BaseFloorData() });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>())!.Data!;

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/floors/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>())!.Data!;
        body.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task UpdateFloor_ChangesNameAndData()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);
        var create = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "Floor 1", Data = BaseFloorData() });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>())!.Data!;

        var data = BaseFloorData();
        data.ObjectiveType = "Defesa";
        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/floors/{created.Id}",
            new UpdateFloorRequest { ArcId = arcId, Number = 2, Name = "Floor 2", Data = data });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>())!.Data!;
        body.Name.Should().Be("Floor 2");
        body.Number.Should().Be(2);
        body.Data.ObjectiveType.Should().Be("Defesa");
    }

    [Fact]
    public async Task DeleteFloor_RemovesFloor()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);
        var create = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "Floor 1", Data = BaseFloorData() });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>())!.Data!;

        var delete = await client.DeleteAsync($"api/campaigns/{campaign.Id}/floors/{created.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await client.GetAsync($"api/campaigns/{campaign.Id}/floors/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetFloorById_ByADifferentGameMaster_Returns404()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);
        var create = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "Secret", Data = BaseFloorData() });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>())!.Data!;

        var (otherClient, otherToken, _) = await SetupGmWithCampaignAsync();
        AuthHelper.SetBearerToken(otherClient, otherToken);

        var response = await otherClient.GetAsync($"api/campaigns/{campaign.Id}/floors/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Floor validation ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateFloor_WithArcFromAnotherCampaign_Returns400ArcInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var otherCampaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Other Campaign" });
        var otherCampaign = (await otherCampaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;
        var foreignArcId = await CreateArcAsync(client, otherCampaign.Id);

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = foreignArcId, Number = 1, Name = "F", Data = BaseFloorData() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Message.Should().Be("Content.ArcInvalid");
    }

    [Fact]
    public async Task CreateFloor_WithUnknownObjectiveType_Returns400ObjectiveTypeInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);
        var data = BaseFloorData();
        data.ObjectiveType = "Nonsense";

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "F", Data = data });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Message.Should().Be("Content.ObjectiveTypeInvalid");
    }

    [Fact]
    public async Task CreateFloor_WithEmptyObjectiveType_Returns400ObjectiveTypeInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);
        var data = BaseFloorData();
        data.ObjectiveType = "";

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "F", Data = data });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Message.Should().Be("Content.ObjectiveTypeInvalid");
    }

    [Fact]
    public async Task CreateFloor_WithEncounterFromAnotherCampaign_Returns400LinkInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);

        var otherCampaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Other Campaign" });
        var otherCampaign = (await otherCampaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;
        var foreignEncounterId = await CreateEncounterAsync(client, otherCampaign.Id);

        var data = BaseFloorData();
        data.LinkedEncounterIds = [foreignEncounterId];

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "F", Data = data });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Message.Should().Be("Content.LinkInvalid");
    }

    [Fact]
    public async Task CreateFloor_WithRewardFromAnotherCampaign_Returns400LinkInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);

        var otherCampaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Other Campaign" });
        var otherCampaign = (await otherCampaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;
        var foreignRewardId = await CreateRewardAsync(client, otherCampaign.Id);

        var data = BaseFloorData();
        data.LinkedRewardIds = [foreignRewardId];

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "F", Data = data });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Message.Should().Be("Content.LinkInvalid");
    }

    [Fact]
    public async Task CreateFloor_WithSameCampaignLinks_ResolvesNames()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);
        var encounterId = await CreateEncounterAsync(client, campaign.Id, "Boss Fight");
        var rewardId = await CreateRewardAsync(client, campaign.Id, "Boss Loot");

        var data = BaseFloorData();
        data.LinkedEncounterIds = [encounterId];
        data.LinkedRewardIds = [rewardId];

        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "Boss Floor", Data = data });

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await create.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>())!.Data!;
        body.Encounters.Should().ContainSingle(e => e.Id == encounterId && e.Name == "Boss Fight");
        body.Rewards.Should().ContainSingle(r => r.Id == rewardId && r.Name == "Boss Loot");
    }

    // ── Cascade delete (DECISION D1) ──────────────────────────────────────────────

    [Fact]
    public async Task DeleteArc_CascadesItsFloors()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var arcId = await CreateArcAsync(client, campaign.Id);
        var create = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/floors",
            new CreateFloorRequest { ArcId = arcId, Number = 1, Name = "Doomed Floor", Data = BaseFloorData() });
        var floor = (await create.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>())!.Data!;

        var delete = await client.DeleteAsync($"api/campaigns/{campaign.Id}/arcs/{arcId}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        var getFloor = await client.GetAsync($"api/campaigns/{campaign.Id}/floors/{floor.Id}");
        getFloor.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var listFloors = await client.GetAsync($"api/campaigns/{campaign.Id}/floors");
        var floors = (await listFloors.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<FloorResponse>>>())!.Data!;
        floors.Should().BeEmpty();
    }
}
