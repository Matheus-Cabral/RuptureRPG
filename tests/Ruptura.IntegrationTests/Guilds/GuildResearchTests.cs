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

public class GuildResearchTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, CampaignResponse Campaign, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Guild Research Test" });
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

    private static async Task<ResearchProjectResponse> ReadResearchAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ApiResponse<ResearchProjectResponse>>())!.Data!;

    [Fact]
    public async Task Member_AddResearch_Returns201WithServerDerivedRequiredDays_AndNoCgWhenIncomplete()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var request = new CreateResearchProjectRequest
        {
            Name = "Arcane Ballistics",
            ResearchType = "Arcana",
            Complexity = "Maior",
            Points = 3,
            IsComplete = false
        };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await ReadResearchAsync(response);
        body.Complexity.Should().Be("Maior");
        body.RequiredDays.Should().Be(20);   // server-derived from complexity, not from the request
        body.Points.Should().Be(3);
        body.IsComplete.Should().BeFalse();

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Research.Should().ContainSingle();
        guild.Research[0].RequiredDays.Should().Be(20);
        // Incomplete research contributes nothing to CG Pesquisa.
        guild.DerivedStats.CgPesquisa.Should().Be(0);
    }

    [Fact]
    public async Task CompletingResearch_RaisesCgPesquisaAndCg()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "R1", Complexity = "Maior", Points = 3, IsComplete = false });
        var research = await ReadResearchAsync(created);

        var before = await GetGuildAsync(client, campaign.Id);
        before.DerivedStats.CgPesquisa.Should().Be(0);
        var cgBefore = before.DerivedStats.Cg;

        var update = new UpdateResearchProjectRequest
        {
            Name = "R1",
            Complexity = "Maior",
            Stage = "Aplicar",
            Points = 3,
            Researchers = 1,
            IsComplete = true
        };
        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/research/{research.Id}", update);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadResearchAsync(response)).IsComplete.Should().BeTrue();

        var after = await GetGuildAsync(client, campaign.Id);
        after.DerivedStats.CgPesquisa.Should().Be(3);
        after.DerivedStats.Cg.Should().Be(cgBefore + 3);
    }

    [Fact]
    public async Task SecondCompletedProject_AccumulatesCgPesquisa()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "Maior", Complexity = "Maior", Points = 3, IsComplete = true });
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "Menor", Complexity = "Menor", Points = 1, IsComplete = true });

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Research.Should().HaveCount(2);
        guild.DerivedStats.CgPesquisa.Should().Be(4); // 3 + 1
    }

    [Fact]
    public async Task UpdateResearch_ComplexityChange_ReDerivesRequiredDays()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "R", Complexity = "Maior", Points = 3 });
        var research = await ReadResearchAsync(created);
        research.RequiredDays.Should().Be(20);

        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/research/{research.Id}",
            new UpdateResearchProjectRequest { Name = "R", Complexity = "Menor", Stage = "Descobrir", Points = 1, Researchers = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadResearchAsync(response)).RequiredDays.Should().Be(5); // re-derived Menor -> 5
    }

    [Fact]
    public async Task AddResearch_InvalidComplexity_Returns400()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "R", Complexity = "Huge", Points = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddResearch_InvalidStage_Returns400()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "R", Complexity = "Menor", Stage = "Nonsense", Points = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddResearch_NegativePointsAndProgress_ClampedToZero()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest
            {
                Name = "R",
                Complexity = "Menor",
                Points = -5,
                ProgressDays = -3,
                Researchers = 0,
                IsComplete = false
            });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await ReadResearchAsync(response);
        body.Points.Should().Be(0);
        body.ProgressDays.Should().Be(0);
        body.Researchers.Should().Be(1); // floor of 1 researcher
    }

    [Fact]
    public async Task CompletedResearch_WithMaxIntPoints_IsClampedAndGuildReadStays200()
    {
        // Regression: unbounded Points (int.MaxValue) once overflowed the CHECKED Sum in the guild read,
        // permanently 500ing GET /guild for the campaign. Points must be top-clamped to 1000 on write.
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "Overflow", Complexity = "Suprema", Points = int.MaxValue, IsComplete = true });
        var research = await ReadResearchAsync(created);
        research.Points.Should().Be(1000); // clamped at write

        // The whole guild read must still return 200, not a 500 from an OverflowException.
        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Research.Should().ContainSingle();
        guild.Research[0].Points.Should().Be(1000);
        guild.DerivedStats.CgPesquisa.Should().Be(1000);
    }

    [Fact]
    public async Task DeleteResearch_RemovesItAndDropsCgPesquisa()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "R", Complexity = "Maior", Points = 3, IsComplete = true });
        var research = await ReadResearchAsync(created);

        var withResearch = await GetGuildAsync(client, campaign.Id);
        withResearch.DerivedStats.CgPesquisa.Should().Be(3);

        var response = await client.DeleteAsync($"api/campaigns/{campaign.Id}/guild/research/{research.Id}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Research.Should().BeEmpty();
        guild.DerivedStats.CgPesquisa.Should().Be(0);
    }

    [Fact]
    public async Task NonMember_AddUpdateDelete_Returns404()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();

        AuthHelper.SetBearerToken(client, gmToken);
        await GetGuildAsync(client, campaign.Id);
        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "R", Complexity = "Menor", Points = 1 });
        var research = await ReadResearchAsync(created);

        var otherGm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, otherGm.AccessToken);
        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var outsider = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var add = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "X", Complexity = "Menor", Points = 1 });
        add.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var upd = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/research/{research.Id}",
            new UpdateResearchProjectRequest { Name = "X", Complexity = "Menor", Stage = "Descobrir", Points = 1, Researchers = 1 });
        upd.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var del = await client.DeleteAsync($"api/campaigns/{campaign.Id}/guild/research/{research.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDeleteResearch_OfAnotherGuild_Returns404()
    {
        var (clientA, campaignA, _, gmTokenA) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(clientA, gmTokenA);
        await GetGuildAsync(clientA, campaignA.Id);
        var createdA = await clientA.PostAsJsonAsync($"api/campaigns/{campaignA.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "R", Complexity = "Maior", Points = 3 });
        var researchA = await ReadResearchAsync(createdA);

        var campaignBResponse = await clientA.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Campaign B" });
        var campaignB = (await campaignBResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;
        await GetGuildAsync(clientA, campaignB.Id);

        var upd = await clientA.PutAsJsonAsync(
            $"api/campaigns/{campaignB.Id}/guild/research/{researchA.Id}",
            new UpdateResearchProjectRequest { Name = "R", Complexity = "Maior", Stage = "Descobrir", Points = 3, Researchers = 1 });
        upd.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var del = await clientA.DeleteAsync($"api/campaigns/{campaignB.Id}/guild/research/{researchA.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var guildA = await GetGuildAsync(clientA, campaignA.Id);
        guildA.Research.Should().ContainSingle(r => r.Id == researchA.Id);
    }
}
