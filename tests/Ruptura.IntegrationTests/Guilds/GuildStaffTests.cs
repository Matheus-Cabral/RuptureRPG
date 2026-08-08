using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildStaffTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, CampaignResponse Campaign, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Guild Staff Test" });
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

    [Fact]
    public async Task Member_AddWorker_Returns201AndShowsInGuildWithDerivedStats()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var request = new CreateStaffRequest
        {
            Kind = "Worker",
            TypeOrRanking = GuildStaffTypes.Operario,
            Name = "Zeca",
            DailySalary = 3,
            IsActive = true
        };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/staff", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildStaffResponse>>())!.Data!;
        body.Kind.Should().Be("Worker");
        body.TypeOrRanking.Should().Be(GuildStaffTypes.Operario);
        body.DailySalary.Should().Be(3);

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Staff.Should().ContainSingle();
        guild.Staff[0].Kind.Should().Be("Worker");
        guild.Staff[0].TypeOrRanking.Should().Be(GuildStaffTypes.Operario);
        // Active Operário worker → +2 income per day, and salary counts toward daily maintenance.
        guild.DerivedStats.WorkerIncomePerDay.Should().Be(2);
        guild.DerivedStats.DailyMaintenance.Should().Be(3);
    }

    [Fact]
    public async Task Member_AddMercenary_Returns201MaintenanceIncludesItNotCountedAsWorker()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var request = new CreateStaffRequest
        {
            Kind = "Mercenary",
            TypeOrRanking = "Bronze",
            Name = "Kael",
            DailySalary = 10,
            IsActive = true
        };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/staff", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildStaffResponse>>())!.Data!;
        body.Kind.Should().Be("Mercenary");

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Staff.Should().ContainSingle();
        // Mercenary salary counts toward maintenance...
        guild.DerivedStats.DailyMaintenance.Should().Be(10);
        // ...but it is not a worker → no worker income.
        guild.DerivedStats.WorkerIncomePerDay.Should().Be(0);
    }

    [Fact]
    public async Task AddStaff_WithUnknownKind_Returns400StaffKindInvalid()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var request = new CreateStaffRequest
        {
            Kind = "Nonsense",
            TypeOrRanking = GuildStaffTypes.Operario,
            DailySalary = 3,
            IsActive = true
        };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/staff", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Member_UpdateStaff_Returns200AndMaintenanceReflectsInactiveExcluded()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/staff",
            new CreateStaffRequest { Kind = "Worker", TypeOrRanking = GuildStaffTypes.Operario, DailySalary = 3, IsActive = true });
        var createdBody = (await created.Content.ReadFromJsonAsync<ApiResponse<GuildStaffResponse>>())!.Data!;

        // Raise salary and deactivate → inactive staff excluded from maintenance & worker income.
        var update = new UpdateStaffRequest
        {
            Kind = "Worker",
            TypeOrRanking = GuildStaffTypes.Operario,
            DailySalary = 7,
            IsActive = false
        };
        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/staff/{createdBody.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildStaffResponse>>())!.Data!;
        body.DailySalary.Should().Be(7);
        body.IsActive.Should().BeFalse();

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Staff.Should().ContainSingle();
        guild.Staff[0].DailySalary.Should().Be(7);
        guild.Staff[0].IsActive.Should().BeFalse();
        // Inactive → no maintenance cost, no worker income.
        guild.DerivedStats.DailyMaintenance.Should().Be(0);
        guild.DerivedStats.WorkerIncomePerDay.Should().Be(0);
    }

    [Fact]
    public async Task Member_DeleteStaff_RemovesFromGuild()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/staff",
            new CreateStaffRequest { Kind = "Worker", TypeOrRanking = GuildStaffTypes.Operario, DailySalary = 3, IsActive = true });
        var createdBody = (await created.Content.ReadFromJsonAsync<ApiResponse<GuildStaffResponse>>())!.Data!;

        var response = await client.DeleteAsync($"api/campaigns/{campaign.Id}/guild/staff/{createdBody.Id}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Staff.Should().BeEmpty();
    }

    [Fact]
    public async Task NonMember_AddUpdateDelete_Returns404()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();

        // Seed staff as GM so update/delete have a real target id.
        AuthHelper.SetBearerToken(client, gmToken);
        await GetGuildAsync(client, campaign.Id);
        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/staff",
            new CreateStaffRequest { Kind = "Worker", TypeOrRanking = GuildStaffTypes.Operario, DailySalary = 3, IsActive = true });
        var createdBody = (await created.Content.ReadFromJsonAsync<ApiResponse<GuildStaffResponse>>())!.Data!;

        // An outsider never added to this campaign.
        var otherGm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, otherGm.AccessToken);
        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var outsider = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var add = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/staff",
            new CreateStaffRequest { Kind = "Worker", TypeOrRanking = GuildStaffTypes.Operario, DailySalary = 3, IsActive = true });
        add.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var upd = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/staff/{createdBody.Id}",
            new UpdateStaffRequest { Kind = "Worker", TypeOrRanking = GuildStaffTypes.Operario, DailySalary = 5, IsActive = true });
        upd.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var del = await client.DeleteAsync($"api/campaigns/{campaign.Id}/guild/staff/{createdBody.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDeleteStaff_OfAnotherGuild_Returns404()
    {
        // Campaign A with its guild + staff.
        var (clientA, campaignA, _, gmTokenA) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(clientA, gmTokenA);
        await GetGuildAsync(clientA, campaignA.Id);
        var createdA = await clientA.PostAsJsonAsync($"api/campaigns/{campaignA.Id}/guild/staff",
            new CreateStaffRequest { Kind = "Worker", TypeOrRanking = GuildStaffTypes.Operario, DailySalary = 3, IsActive = true });
        var staffA = (await createdA.Content.ReadFromJsonAsync<ApiResponse<GuildStaffResponse>>())!.Data!;

        // Campaign B (same GM, its own guild).
        var campaignBResponse = await clientA.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Campaign B" });
        var campaignB = (await campaignBResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;
        await GetGuildAsync(clientA, campaignB.Id);

        // Cross-guild update/delete through campaign B's route → 404.
        var upd = await clientA.PutAsJsonAsync(
            $"api/campaigns/{campaignB.Id}/guild/staff/{staffA.Id}",
            new UpdateStaffRequest { Kind = "Worker", TypeOrRanking = GuildStaffTypes.Operario, DailySalary = 5, IsActive = true });
        upd.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var del = await clientA.DeleteAsync($"api/campaigns/{campaignB.Id}/guild/staff/{staffA.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Staff A still there under campaign A.
        var guildA = await GetGuildAsync(clientA, campaignA.Id);
        guildA.Staff.Should().ContainSingle(s => s.Id == staffA.Id);
    }
}
