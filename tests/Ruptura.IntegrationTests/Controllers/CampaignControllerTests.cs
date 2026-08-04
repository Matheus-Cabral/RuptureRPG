using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class CampaignControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, string GmToken, string InviteCode)> SetupGameMasterWithInviteAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var inviteResponse = await client.PostAsync("api/invites", null);
        var invite = (await inviteResponse.Content
            .ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!;

        return (client, gm.AccessToken, invite.Code);
    }

    [Fact]
    public async Task Players_ReturnsOnlyRosterOfCallingGameMaster()
    {
        var (client, gmToken, inviteCode) = await SetupGameMasterWithInviteAsync();
        var playerEmail = Faker.Internet.Email();
        await AuthHelper.RegisterPlayerAsync(client, inviteCode, playerEmail);

        AuthHelper.SetBearerToken(client, gmToken);
        var response = await client.GetAsync("api/gamemaster/players");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>();
        body!.Data.Should().ContainSingle(p => p.Email == playerEmail);
    }

    [Fact]
    public async Task Players_DoesNotIncludePlayersRecruitedByAnotherGameMaster()
    {
        var (client1, gm1Token, invite1) = await SetupGameMasterWithInviteAsync();
        await AuthHelper.RegisterPlayerAsync(client1, invite1, Faker.Internet.Email());

        var (client2, gm2Token, _) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client2, gm2Token);

        var response = await client2.GetAsync("api/gamemaster/players");
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>();
        body!.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCampaign_ReturnsCampaignOwnedByCaller()
    {
        var (client, gmToken, _) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var response = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "The Sunken Gate"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>();
        body!.Data!.Name.Should().Be("The Sunken Gate");
    }

    [Fact]
    public async Task AssignMember_WithPlayerInRoster_Returns201AndListsInMembers()
    {
        var (client, gmToken, inviteCode) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        var playerEmail = Faker.Internet.Email();
        await AuthHelper.RegisterPlayerAsync(client, inviteCode, playerEmail);

        var playersResponse = await client.GetAsync("api/gamemaster/players");
        var players = (await playersResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>())!.Data!.ToList();
        var playerId = players.Single(p => p.Email == playerEmail).Id;

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "The Sunken Gate"
        });
        var campaignId = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!.Id;

        var assignResponse = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/members", new AssignMemberRequest { PlayerId = playerId });
        assignResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var membersResponse = await client.GetAsync($"api/campaigns/{campaignId}/members");
        var members = (await membersResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignMemberResponse>>>())!.Data!;
        members.Should().ContainSingle(m => m.PlayerId == playerId);
    }

    [Fact]
    public async Task AssignMember_WithPlayerNotInRoster_Returns400()
    {
        var (client, gmToken, _) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "The Sunken Gate"
        });
        var campaignId = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!.Id;

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/members", new AssignMemberRequest { PlayerId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignMember_ToCampaignNotOwnedByCaller_Returns404()
    {
        var (client1, gm1Token, invite1) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client1, gm1Token);
        var campaignResponse = await client1.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "GM1's Campaign"
        });
        var campaignId = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!.Id;

        var (client2, gm2Token, invite2) = await SetupGameMasterWithInviteAsync();
        var playerEmail = Faker.Internet.Email();
        await AuthHelper.RegisterPlayerAsync(client2, invite2, playerEmail);
        AuthHelper.SetBearerToken(client2, gm2Token);
        var players = (await (await client2.GetAsync("api/gamemaster/players")).Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>())!.Data!.ToList();

        var response = await client2.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/members",
            new AssignMemberRequest { PlayerId = players.Single(p => p.Email == playerEmail).Id });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Members_ForCampaignNotOwnedByCaller_Returns404()
    {
        var (client1, gm1Token, invite1) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client1, gm1Token);
        var campaignResponse = await client1.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "GM1's Campaign"
        });
        var campaignId = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!.Id;

        var (client2, gm2Token, _) = await SetupGameMasterWithInviteAsync();
        AuthHelper.SetBearerToken(client2, gm2Token);

        var response = await client2.GetAsync($"api/campaigns/{campaignId}/members");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CampaignEndpoints_WithoutGameMasterRole_Return403()
    {
        var (client, _, inviteCode) = await SetupGameMasterWithInviteAsync();
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        var playerClient = factory.CreateClient();
        AuthHelper.SetBearerToken(playerClient, player.AccessToken);

        var response = await playerClient.GetAsync("api/campaigns");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
