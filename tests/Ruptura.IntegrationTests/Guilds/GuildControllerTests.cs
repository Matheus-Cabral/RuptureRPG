using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, CampaignResponse Campaign, Guid PlayerId, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Guild Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        return (client, campaign, player.User.Id, player.AccessToken, gm.AccessToken);
    }

    [Fact]
    public async Task Get_AsCampaignGameMaster_Returns200WithGuildAndDerivedStats()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/guild");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;
        body.GuildName.Should().Be(campaign.Name);   // get-or-create seeds GuildName from campaign name
        body.Data.Should().NotBeNull();
        body.DerivedStats.Should().NotBeNull();
        body.DerivedStats.Stage.Should().Be(GuildStage.Fundacao); // no floors conquered yet
        body.Version.Should().NotBe(default(uint));
    }

    [Fact]
    public async Task Get_AsCampaignMemberPlayer_Returns200()
    {
        var (client, campaign, _, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/guild");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;
        body.CampaignId.Should().Be(campaign.Id);
    }

    [Fact]
    public async Task Get_AsNonMemberPlayer_Returns404()
    {
        var (client, campaign, _, _, _) = await SetUpCampaignWithMemberAsync();

        // A player registered under a fresh invite but never added to this campaign's roster.
        AuthHelper.SetBearerToken(client, (await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email())).AccessToken);
        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var outsider = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/guild");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_TwiceForTheSameCampaign_ReturnsTheSameGuildId()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var first = (await (await client.GetAsync($"api/campaigns/{campaign.Id}/guild"))
            .Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;
        var second = (await (await client.GetAsync($"api/campaigns/{campaign.Id}/guild"))
            .Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;

        second.Id.Should().Be(first.Id);   // get-or-create is idempotent
    }

    [Fact]
    public async Task Get_WithSeededArmazemLevel2_ReturnsStorageAndInfraDerivedStats()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        // First GET creates the guild.
        var created = (await (await client.GetAsync($"api/campaigns/{campaign.Id}/guild"))
            .Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;

        // Seed an Armazém (weight 1, seeded globally) at level 2 directly on the guild.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.GuildBuildings.Add(new GuildBuilding
            {
                Id = Guid.NewGuid(),
                GuildSheetId = created.Id,
                CatalogEntryId = GuildCatalogIds.Armazem,
                Level = 2,
                IsActive = true
            });
            await db.SaveChangesAsync();
        }

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/guild");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;
        body.DerivedStats.StorageCapacity.Should().Be(100); // Armazém level 2 × 50
        body.DerivedStats.CgInfra.Should().Be(2);           // level 2 × weight 1
    }
}
