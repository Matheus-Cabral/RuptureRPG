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

public class GuildExpeditionTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, CampaignResponse Campaign, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Guild Expedition Test" });
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

    private static CreateExpeditionRequest NewExpedition(string objective, string kind = "Principal") => new()
    {
        Kind = kind,
        Date = DateTime.UtcNow,
        Participants = "Alpha, Beta",
        Objective = objective,
        Result = "Success",
        Losses = "None",
        ResourcesGained = "Gold"
    };

    [Fact]
    public async Task Member_AddExpedition_Returns201AndShowsInGuildOrderedByDateDesc()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        // Ensure the guild exists.
        await GetGuildAsync(client, campaign.Id);

        var older = NewExpedition("Older run");
        older.Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var newer = NewExpedition("Newer run");
        newer.Date = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var r1 = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/expeditions", older);
        r1.StatusCode.Should().Be(HttpStatusCode.Created);
        var r2 = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/expeditions", newer);
        r2.StatusCode.Should().Be(HttpStatusCode.Created);

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Expeditions.Should().HaveCount(2);
        // Ordered by Date desc.
        guild.Expeditions[0].Objective.Should().Be("Newer run");
        guild.Expeditions[1].Objective.Should().Be("Older run");
        guild.Expeditions[0].Kind.Should().Be("Principal");
    }

    [Fact]
    public async Task Member_UpdateExpedition_Returns200AndChangesReflected()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/expeditions", NewExpedition("Initial"));
        var createdBody = (await created.Content.ReadFromJsonAsync<ApiResponse<ExpeditionResponse>>())!.Data!;

        var update = new UpdateExpeditionRequest
        {
            Kind = "Secundaria",
            Date = DateTime.UtcNow,
            Participants = "Solo",
            Objective = "Revised objective",
            Result = "Retreat",
            Losses = "Two",
            ResourcesGained = "Nothing"
        };
        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/expeditions/{createdBody.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<ExpeditionResponse>>())!.Data!;
        body.Objective.Should().Be("Revised objective");
        body.Kind.Should().Be("Secundaria");

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Expeditions.Should().ContainSingle();
        guild.Expeditions[0].Objective.Should().Be("Revised objective");
        guild.Expeditions[0].Kind.Should().Be("Secundaria");
    }

    [Fact]
    public async Task Member_DeleteExpedition_RemovesFromGuild()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/expeditions", NewExpedition("To delete"));
        var createdBody = (await created.Content.ReadFromJsonAsync<ApiResponse<ExpeditionResponse>>())!.Data!;

        var response = await client.DeleteAsync($"api/campaigns/{campaign.Id}/guild/expeditions/{createdBody.Id}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Expeditions.Should().BeEmpty();
    }

    [Fact]
    public async Task NonMember_AddUpdateDelete_Returns404()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();

        // Seed an expedition as GM so update/delete have a real target id.
        AuthHelper.SetBearerToken(client, gmToken);
        await GetGuildAsync(client, campaign.Id);
        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/expeditions", NewExpedition("GM run"));
        var createdBody = (await created.Content.ReadFromJsonAsync<ApiResponse<ExpeditionResponse>>())!.Data!;

        // An outsider never added to this campaign.
        var otherGm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, otherGm.AccessToken);
        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var outsider = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var add = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/expeditions", NewExpedition("Nope"));
        add.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var upd = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/expeditions/{createdBody.Id}",
            new UpdateExpeditionRequest { Kind = "Principal", Date = DateTime.UtcNow, Objective = "Nope" });
        upd.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var del = await client.DeleteAsync($"api/campaigns/{campaign.Id}/guild/expeditions/{createdBody.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AddExpedition_WithUnspecifiedKindDateTime_IsAcceptedAndStored()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var request = NewExpedition("Unspecified date run");
        // DateTimeKind.Unspecified — Npgsql throws on write to timestamptz unless normalized to UTC.
        request.Date = new DateTime(2026, 1, 1);
        request.Date.Kind.Should().Be(DateTimeKind.Unspecified);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/expeditions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Expeditions.Should().ContainSingle(e => e.Objective == "Unspecified date run");
    }

    [Fact]
    public async Task DeleteExpedition_OfAnotherGuild_Returns404()
    {
        // Campaign A with its guild + expedition.
        var (clientA, campaignA, _, gmTokenA) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(clientA, gmTokenA);
        await GetGuildAsync(clientA, campaignA.Id);
        var createdA = await clientA.PostAsJsonAsync($"api/campaigns/{campaignA.Id}/guild/expeditions", NewExpedition("Campaign A run"));
        var expeditionA = (await createdA.Content.ReadFromJsonAsync<ApiResponse<ExpeditionResponse>>())!.Data!;

        // Campaign B (same GM, its own guild).
        var campaignBResponse = await clientA.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Campaign B" });
        var campaignB = (await campaignBResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;
        await GetGuildAsync(clientA, campaignB.Id);

        // Try to delete campaign A's expedition through campaign B's route -> cross-guild -> 404.
        var response = await clientA.DeleteAsync($"api/campaigns/{campaignB.Id}/guild/expeditions/{expeditionA.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Expedition A still there under campaign A.
        var guildA = await GetGuildAsync(clientA, campaignA.Id);
        guildA.Expeditions.Should().ContainSingle(e => e.Id == expeditionA.Id);
    }
}
