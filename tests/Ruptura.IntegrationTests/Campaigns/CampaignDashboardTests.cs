using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Content;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Campaigns;

public class CampaignDashboardTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, string GmToken, CampaignResponse Campaign)> SetupGmWithCampaignAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Dashboard Campaign" });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        return (client, gm.AccessToken, campaign);
    }

    private static async Task<string> NewInviteCodeAsync(HttpClient client)
    {
        var invite = await client.PostAsync("api/invites", null);
        return (await invite.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
    }

    private static async Task<CharacterSheetResponse> GrantSheetAsync(
        HttpClient client, Guid campaignId, Guid playerId, string characterName)
    {
        var grant = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = characterName });
        grant.EnsureSuccessStatusCode();
        return (await grant.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
    }

    private static async Task<CampaignDashboardResponse> GetDashboardAsync(HttpClient client, Guid campaignId)
    {
        var response = await client.GetAsync($"api/campaigns/{campaignId}/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ApiResponse<CampaignDashboardResponse>>())!.Data!;
    }

    private static async Task<Guid> CreateFloorAsync(
        HttpClient client, Guid campaignId, string name, string mainObjective)
    {
        var arc = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/arcs",
            new CreateArcRequest { Name = "Arc I", Order = 1, Data = new ArcData { Theme = "Descent" } });
        arc.StatusCode.Should().Be(HttpStatusCode.Created);
        var arcId = (await arc.Content.ReadFromJsonAsync<ApiResponse<ArcResponse>>())!.Data!.Id;

        var floor = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/floors",
            new CreateFloorRequest
            {
                ArcId = arcId,
                Number = 1,
                Name = name,
                Data = new FloorData { ObjectiveType = "Exploracao", MainObjective = mainObjective }
            });
        floor.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await floor.Content.ReadFromJsonAsync<ApiResponse<FloorResponse>>())!.Data!.Id;
    }

    [Fact]
    public async Task Get_AsCampaignGameMaster_Returns200WithDefaultDungeonState()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var dashboard = await GetDashboardAsync(client, campaign.Id);

        dashboard.CampaignId.Should().Be(campaign.Id);
        dashboard.CampaignName.Should().Be(campaign.Name);
        dashboard.Dungeon.CurrentFloor.Should().Be(1);
        dashboard.Dungeon.FloorState.Should().Be("Inexplorado");
        dashboard.Dungeon.Pressure.Should().Be(0);
        dashboard.Dungeon.PressureStateKey.Should().Be("Estavel");
        dashboard.Dungeon.PeMultiplier.Should().Be(1.00m);
    }

    [Fact]
    public async Task Get_Party_ListsAliveMembersAndExcludesDeadOrRetired()
    {
        var (client, gmToken, campaign) = await SetupGmWithCampaignAsync();

        // Alive player.
        var aliveCode = await NewInviteCodeAsync(client);
        var alivePlayer = await AuthHelper.RegisterPlayerAsync(client, aliveCode, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gmToken);
        await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = alivePlayer.User.Id });
        var aliveSheet = await GrantSheetAsync(client, campaign.Id, alivePlayer.User.Id, "Alive Hero");

        // Dead player — a different owner (the alive-per-owner-per-campaign index only allows one alive sheet each).
        var deadCode = await NewInviteCodeAsync(client);
        var deadPlayer = await AuthHelper.RegisterPlayerAsync(client, deadCode, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gmToken);
        await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = deadPlayer.User.Id });
        var deadSheet = await GrantSheetAsync(client, campaign.Id, deadPlayer.User.Id, "Fallen One");

        // GM marks the second sheet dead.
        var kill = await client.PutAsJsonAsync($"api/character-sheets/{deadSheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = deadSheet.CharacterName,
            DataJson = JsonSerializer.Serialize(deadSheet.Data),
            IsDead = true
        });
        kill.EnsureSuccessStatusCode();

        var dashboard = await GetDashboardAsync(client, campaign.Id);

        dashboard.Party.Should().ContainSingle();
        var member = dashboard.Party.Single();
        member.Id.Should().Be(aliveSheet.Id);
        member.CharacterName.Should().Be("Alive Hero");
        member.Ranking.Should().Be(aliveSheet.Data.GuildRegistry.Ranking);
        member.Np.Should().Be(aliveSheet.DerivedStats.Np);
        member.CurrentHp.Should().Be(aliveSheet.Data.Combat.CurrentHp);
        member.MaxHp.Should().Be(aliveSheet.DerivedStats.MaxHp);
    }

    [Fact]
    public async Task Get_GuildIsNull_WhenNoGuildExists_WithoutCreatingOne()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        // Deliberately never call the guild endpoint (which is get-or-create).
        var dashboard = await GetDashboardAsync(client, campaign.Id);

        dashboard.Guild.Should().BeNull();
    }

    [Fact]
    public async Task Get_ByADifferentGameMaster_Returns404()
    {
        var (_, _, campaign) = await SetupGmWithCampaignAsync();

        var (otherClient, otherToken, _) = await SetupGmWithCampaignAsync();
        AuthHelper.SetBearerToken(otherClient, otherToken);

        var response = await otherClient.GetAsync($"api/campaigns/{campaign.Id}/dashboard");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutDungeon_ByADifferentGameMaster_Returns404_AndDoesNotMutate()
    {
        // Owner GM sets a known dungeon state on their campaign.
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var seed = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/dashboard/dungeon", new UpdateDungeonStateRequest
            {
                CurrentFloor = 4,
                FloorName = "Salão Original",
                FloorState = "Explorado",
                Pressure = 30
            });
        seed.StatusCode.Should().Be(HttpStatusCode.OK);

        // A different GM (owning a different campaign) attempts to PUT this campaign's dungeon.
        var (otherClient, otherToken, _) = await SetupGmWithCampaignAsync();
        AuthHelper.SetBearerToken(otherClient, otherToken);

        var response = await otherClient.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/dashboard/dungeon", new UpdateDungeonStateRequest
            {
                CurrentFloor = 99,
                FloorName = "Invasão",
                FloorState = "Inexplorado",
                Pressure = 100
            });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // The unauthorized PUT must not have mutated the owner's dungeon state.
        var dashboard = await GetDashboardAsync(client, campaign.Id);
        dashboard.Dungeon.CurrentFloor.Should().Be(4);
        dashboard.Dungeon.FloorName.Should().Be("Salão Original");
        dashboard.Dungeon.FloorState.Should().Be("Explorado");
        dashboard.Dungeon.Pressure.Should().Be(30);
    }

    [Fact]
    public async Task Get_AsPlayer_IsRoleGated()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var inviteCode = await NewInviteCodeAsync(client);
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        var playerClient = factory.CreateClient();
        AuthHelper.SetBearerToken(playerClient, player.AccessToken);

        var response = await playerClient.GetAsync($"api/campaigns/{campaign.Id}/dashboard");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutDungeon_ClampsPressureToUpperBound_AndPersists()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var putResponse = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/dashboard/dungeon", new UpdateDungeonStateRequest
            {
                CurrentFloor = 3,
                FloorName = "Cripta",
                FloorState = "Explorado",
                Pressure = 150
            });
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await putResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignDashboardResponse>>())!.Data!;
        updated.Dungeon.Pressure.Should().Be(100);
        updated.Dungeon.PressureStateKey.Should().Be("Colapso");

        var dashboard = await GetDashboardAsync(client, campaign.Id);
        dashboard.Dungeon.CurrentFloor.Should().Be(3);
        dashboard.Dungeon.FloorName.Should().Be("Cripta");
        dashboard.Dungeon.FloorState.Should().Be("Explorado");
        dashboard.Dungeon.Pressure.Should().Be(100);
        dashboard.Dungeon.PressureStateKey.Should().Be("Colapso");
    }

    [Fact]
    public async Task PutDungeon_ClampsPressureAndFloorToLowerBounds()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var putResponse = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/dashboard/dungeon", new UpdateDungeonStateRequest
            {
                CurrentFloor = 0,
                FloorName = "Entrada",
                FloorState = "Inexplorado",
                Pressure = -20
            });
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await putResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignDashboardResponse>>())!.Data!;
        updated.Dungeon.CurrentFloor.Should().Be(1);
        updated.Dungeon.Pressure.Should().Be(0);
        updated.Dungeon.PressureStateKey.Should().Be("Estavel");
    }

    [Fact]
    public async Task PutDungeon_WithInvalidFloorState_Returns400FloorStateInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/dashboard/dungeon", new UpdateDungeonStateRequest
            {
                CurrentFloor = 2,
                FloorName = "Nowhere",
                FloorState = "Nonsense",
                Pressure = 10
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Success.Should().BeFalse();
        // The API surfaces the raw error code in the response message (its runtime convention),
        // so assert the FloorStateInvalid code — this distinguishes a 400 from a 404 (Campaign.NotFound).
        body.Message.Should().Be("Campaign.FloorStateInvalid");
    }

    [Fact]
    public async Task PutDungeon_AdvanceFloorShape_RoundTrips()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var putResponse = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/dashboard/dungeon", new UpdateDungeonStateRequest
            {
                CurrentFloor = 2,
                FloorName = "Segundo Andar",
                FloorState = "Inexplorado",
                Pressure = 0
            });
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await putResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignDashboardResponse>>())!.Data!;
        updated.Dungeon.CurrentFloor.Should().Be(2);
        updated.Dungeon.FloorName.Should().Be("Segundo Andar");
        updated.Dungeon.FloorState.Should().Be("Inexplorado");
        updated.Dungeon.Pressure.Should().Be(0);
        updated.Dungeon.PressureStateKey.Should().Be("Estavel");
        updated.Dungeon.PeMultiplier.Should().Be(1.00m);
    }

    [Fact]
    public async Task PutDungeon_WithCampaignFloor_ResolvesFloorSummaryOnGet()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var floorId = await CreateFloorAsync(client, campaign.Id, "Cripta Selada", "Reach the far gate");

        var put = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/dashboard/dungeon", new UpdateDungeonStateRequest
            {
                CurrentFloor = 2,
                FloorName = "Segundo Andar",
                FloorState = "Inexplorado",
                Pressure = 0,
                CurrentFloorId = floorId
            });
        put.StatusCode.Should().Be(HttpStatusCode.OK);

        var dashboard = await GetDashboardAsync(client, campaign.Id);
        dashboard.Dungeon.CurrentFloorId.Should().Be(floorId);
        dashboard.Dungeon.CurrentFloorName.Should().Be("Cripta Selada");
        dashboard.Dungeon.CurrentFloorObjective.Should().Be("Reach the far gate");
    }

    [Fact]
    public async Task PutDungeon_WithForeignFloor_Returns400CurrentFloorInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        // A floor belonging to a different campaign (owned by another GM).
        var (otherClient, _, otherCampaign) = await SetupGmWithCampaignAsync();
        var foreignFloorId = await CreateFloorAsync(otherClient, otherCampaign.Id, "Foreign", "Nope");

        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/dashboard/dungeon", new UpdateDungeonStateRequest
            {
                CurrentFloor = 2,
                FloorName = "Segundo Andar",
                FloorState = "Inexplorado",
                Pressure = 0,
                CurrentFloorId = foreignFloorId
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Success.Should().BeFalse();
        body.Message.Should().Be("Campaign.CurrentFloorInvalid");
    }

    [Fact]
    public async Task PutDungeon_WithNullCurrentFloorId_ClearsThePointer()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var floorId = await CreateFloorAsync(client, campaign.Id, "Cripta Selada", "Reach the far gate");

        // First point at the floor.
        await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/dashboard/dungeon", new UpdateDungeonStateRequest
            {
                CurrentFloor = 2,
                FloorName = "Segundo Andar",
                FloorState = "Inexplorado",
                Pressure = 0,
                CurrentFloorId = floorId
            });

        // Then clear it with a null CurrentFloorId.
        var clear = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/dashboard/dungeon", new UpdateDungeonStateRequest
            {
                CurrentFloor = 2,
                FloorName = "Segundo Andar",
                FloorState = "Inexplorado",
                Pressure = 0,
                CurrentFloorId = null
            });
        clear.StatusCode.Should().Be(HttpStatusCode.OK);

        var dashboard = await GetDashboardAsync(client, campaign.Id);
        dashboard.Dungeon.CurrentFloorId.Should().BeNull();
        dashboard.Dungeon.CurrentFloorName.Should().BeNull();
        dashboard.Dungeon.CurrentFloorObjective.Should().BeNull();
    }
}
