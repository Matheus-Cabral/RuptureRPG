using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class CampaignFlowTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    [Fact]
    public async Task FullFlow_RegisterRecruitCreateCampaignAssign_Succeeds()
    {
        var client = factory.CreateClient();

        // 1. GM registers
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        // 2. GM generates an invite
        var inviteResponse = await client.PostAsync("api/invites", null);
        var invite = (await inviteResponse.Content
            .ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!;

        // 3. Player registers with that invite → appears in GM's roster
        var playerEmail = Faker.Internet.Email();
        await AuthHelper.RegisterPlayerAsync(client, invite.Code, playerEmail);

        var rosterResponse = await client.GetAsync("api/gamemaster/players");
        var roster = (await rosterResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<PlayerRosterResponse>>>())!.Data!.ToList();
        roster.Should().ContainSingle(p => p.Email == playerEmail);
        var playerId = roster.Single(p => p.Email == playerEmail).Id;

        // 4. GM creates a Campaign
        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest
        {
            Name = "The Sunken Gate"
        });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        // 5. GM assigns the player to the Campaign
        var assignResponse = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = playerId });
        assignResponse.EnsureSuccessStatusCode();

        // 6. Campaign now lists the player as a member
        var membersResponse = await client.GetAsync($"api/campaigns/{campaign.Id}/members");
        var members = (await membersResponse.Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignMemberResponse>>>())!.Data!;
        members.Should().ContainSingle(m => m.PlayerId == playerId && m.Email == playerEmail);
    }
}
