using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Content;

namespace Ruptura.IntegrationTests.Content;

// End-to-end coverage for session logs (GM-5): dated prep notes persisted as a typed blob scoped to
// one campaign. Simple CRUD — the service is campaign-ownership authoritative (a non-owned/missing
// campaign or a foreign session yields Session.NotFound, existence hidden). Title is required; the
// list is ordered by Date DESCENDING (most recent first).
public class SessionLogTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, string GmToken, CampaignResponse Campaign)> SetupGmWithCampaignAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Session Campaign" });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        return (client, gm.AccessToken, campaign);
    }

    private static async Task<SessionLogResponse> CreateSessionAsync(
        HttpClient client, Guid campaignId, DateTime date, string title = "Session I")
    {
        var resp = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/sessions",
            new CreateSessionLogRequest
            {
                Date = date,
                Title = title,
                Data = new SessionLogData { Recap = "Recap", Agenda = "Agenda", Notes = "Notes" }
            });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resp.Content.ReadFromJsonAsync<ApiResponse<SessionLogResponse>>())!.Data!;
    }

    // ── CRUD ───────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSession_AsCampaignGameMaster_Returns201()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var date = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/sessions",
            new CreateSessionLogRequest
            {
                Date = date,
                Title = "Kickoff",
                Data = new SessionLogData { Recap = "Prev", Agenda = "Plan", Notes = "Misc" }
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<SessionLogResponse>>())!.Data!;
        body.Id.Should().NotBeEmpty();
        body.Title.Should().Be("Kickoff");
        body.Date.Should().Be(date);
        body.Data.Recap.Should().Be("Prev");
        body.Data.Agenda.Should().Be("Plan");
        body.Data.Notes.Should().Be("Misc");
    }

    [Fact]
    public async Task GetSessions_ReturnsCreatedSessions()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        await CreateSessionAsync(client, campaign.Id, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "Session One");

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<SessionLogResponse>>>())!.Data!;
        body.Should().ContainSingle(s => s.Title == "Session One");
    }

    [Fact]
    public async Task GetSessions_OrdersByDateDescending()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        await CreateSessionAsync(client, campaign.Id, new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Middle");
        await CreateSessionAsync(client, campaign.Id, new DateTime(2026, 3, 5, 0, 0, 0, DateTimeKind.Utc), "Newest");
        await CreateSessionAsync(client, campaign.Id, new DateTime(2025, 12, 1, 0, 0, 0, DateTimeKind.Utc), "Oldest");

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/sessions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<SessionLogResponse>>>())!.Data!;
        body.Select(s => s.Title).Should().ContainInOrder("Newest", "Middle", "Oldest");
    }

    [Fact]
    public async Task GetSessionById_ReturnsSession()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var created = await CreateSessionAsync(client, campaign.Id, new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc));

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/sessions/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<SessionLogResponse>>())!.Data!;
        body.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task UpdateSession_ChangesTitleDateAndData()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var created = await CreateSessionAsync(client, campaign.Id, new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc));

        var newDate = new DateTime(2026, 4, 4, 0, 0, 0, DateTimeKind.Utc);
        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/sessions/{created.Id}",
            new UpdateSessionLogRequest
            {
                Date = newDate,
                Title = "Renamed",
                Data = new SessionLogData { Recap = "Updated recap" }
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<SessionLogResponse>>())!.Data!;
        body.Title.Should().Be("Renamed");
        body.Date.Should().Be(newDate);
        body.Data.Recap.Should().Be("Updated recap");
    }

    [Fact]
    public async Task DeleteSession_RemovesSession()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var created = await CreateSessionAsync(client, campaign.Id, new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc));

        var delete = await client.DeleteAsync($"api/campaigns/{campaign.Id}/sessions/{created.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await client.GetAsync($"api/campaigns/{campaign.Id}/sessions/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Validation & auth ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSession_WithMissingTitle_Returns400TitleRequired()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/sessions",
            new CreateSessionLogRequest
            {
                Date = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Title = "   ",
                Data = new SessionLogData()
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Message.Should().Be("Session.TitleRequired");
    }

    [Fact]
    public async Task GetSessionById_ByADifferentGameMaster_Returns404()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var created = await CreateSessionAsync(client, campaign.Id, new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc));

        var (otherClient, otherToken, _) = await SetupGmWithCampaignAsync();
        AuthHelper.SetBearerToken(otherClient, otherToken);

        var response = await otherClient.GetAsync($"api/campaigns/{campaign.Id}/sessions/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSessionById_FromAnotherCampaign_Returns404()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var created = await CreateSessionAsync(client, campaign.Id, new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc));

        var otherCampaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Other Campaign" });
        var otherCampaign = (await otherCampaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        // A session id that belongs to a different campaign → 404 (existence hidden).
        var response = await client.GetAsync($"api/campaigns/{otherCampaign.Id}/sessions/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
