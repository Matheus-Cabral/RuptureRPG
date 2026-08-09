using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Infrastructure.Data;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildUpdateTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private async Task<(HttpClient Client, CampaignResponse Campaign, Guid PlayerId, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Guild Update Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());

        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        return (client, campaign, player.User.Id, player.AccessToken, gm.AccessToken);
    }

    private async Task<GuildSheetResponse> GetGuildAsync(HttpClient client, Guid campaignId)
    {
        var response = await client.GetAsync($"api/campaigns/{campaignId}/guild");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;
    }

    private static string Serialize(GuildSheetData data) => JsonSerializer.Serialize(data, JsonOpts);

    [Fact]
    public async Task Update_AsGameMaster_Returns200AndPersistsChangesAndAdvancesVersion()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var current = await GetGuildAsync(client, campaign.Id);
        current.Data.Prestige.Value = 42;
        current.Data.Identity.PatronDeity = "Ordo Lux";

        var request = new UpdateGuildSheetRequest
        {
            GuildName = "Renamed Guild",
            DataJson = Serialize(current.Data),
            Version = current.Version
        };

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;
        body.GuildName.Should().Be("Renamed Guild");
        body.Data.Prestige.Value.Should().Be(42);
        body.Data.Identity.PatronDeity.Should().Be("Ordo Lux");
        body.Version.Should().NotBe(current.Version);

        var reloaded = await GetGuildAsync(client, campaign.Id);
        reloaded.GuildName.Should().Be("Renamed Guild");
        reloaded.Data.Prestige.Value.Should().Be(42);
    }

    [Fact]
    public async Task Update_AsCampaignMember_Returns200()
    {
        var (client, campaign, _, playerToken, gmToken) = await SetUpCampaignWithMemberAsync();

        AuthHelper.SetBearerToken(client, gmToken);
        var current = await GetGuildAsync(client, campaign.Id);

        AuthHelper.SetBearerToken(client, playerToken);
        current.Data.Prestige.Value = 7;
        var request = new UpdateGuildSheetRequest
        {
            GuildName = "Member Edit",
            DataJson = Serialize(current.Data),
            Version = current.Version
        };

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;
        body.GuildName.Should().Be("Member Edit");
        body.Data.Prestige.Value.Should().Be(7);
    }

    [Fact]
    public async Task Update_AsNonMember_Returns404()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();

        AuthHelper.SetBearerToken(client, gmToken);
        var current = await GetGuildAsync(client, campaign.Id);
        var request = new UpdateGuildSheetRequest
        {
            GuildName = "Should Not Save",
            DataJson = Serialize(current.Data),
            Version = current.Version
        };

        // A player registered under a fresh invite but never added to this campaign's roster.
        var otherGm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, otherGm.AccessToken);
        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var outsider = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // THE LOAD-BEARING TEST: cross-request lost-update protection via xmin.
    [Fact]
    public async Task Update_WithStaleVersionAfterConcurrentWrite_Returns409AndStalePayloadDoesNotPersist()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        // GET -> Version v1
        var v1 = await GetGuildAsync(client, campaign.Id);

        // A second, successful update bumps xmin to v2.
        var winningData = v1.Data;
        winningData.Prestige.Value = 100;
        var winning = new UpdateGuildSheetRequest
        {
            GuildName = "Winner",
            DataJson = Serialize(winningData),
            Version = v1.Version
        };
        var winningResponse = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", winning);
        winningResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Now PUT with the STALE v1 version -> must conflict.
        var staleData = v1.Data;
        staleData.Prestige.Value = 999;
        var stale = new UpdateGuildSheetRequest
        {
            GuildName = "Loser",
            DataJson = Serialize(staleData),
            Version = v1.Version
        };
        var staleResponse = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", stale);

        staleResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // The stale payload's changes did NOT persist — the v2 winner state stands.
        var final = await GetGuildAsync(client, campaign.Id);
        final.GuildName.Should().Be("Winner");
        final.Data.Prestige.Value.Should().Be(100);
    }

    [Fact]
    public async Task Update_WithClientEmblemPath_PreservesStoredEmblem()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        // Create the guild, then seed a server-authoritative emblem path directly.
        var created = await GetGuildAsync(client, campaign.Id);
        const string emblemPath = "/media/guild-emblems/original-emblem.png";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var guild = db.GuildSheets.Single(g => g.Id == created.Id);
            var data = new GuildSheetData();
            data.Identity.EmblemImagePath = emblemPath;
            guild.DataJson = JsonSerializer.Serialize(data, JsonOpts);
            await db.SaveChangesAsync();
        }

        var current = await GetGuildAsync(client, campaign.Id);
        current.Data.Identity.EmblemImagePath.Should().Be(emblemPath);

        // Client tries to blank the emblem via the general update payload.
        current.Data.Identity.EmblemImagePath = "";
        var request = new UpdateGuildSheetRequest
        {
            GuildName = current.GuildName,
            DataJson = Serialize(current.Data),
            Version = current.Version
        };
        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var final = await GetGuildAsync(client, campaign.Id);
        final.Data.Identity.EmblemImagePath.Should().Be(emblemPath);
    }

    // Server-side reputation clamp (GDD range -100..100): out-of-range values sent by the client
    // are clamped on write rather than trusted, mirroring the research Points / staff salary clamps.
    [Fact]
    public async Task Update_WithOutOfRangeReputation_ClampsToGdRange()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var current = await GetGuildAsync(client, campaign.Id);
        current.Data.Influence =
        [
            new InfluenceRelation { Name = "Cidade Alta", Kind = "Cidade", Reputation = 999 },
            new InfluenceRelation { Name = "Culto Sombrio", Kind = "Facção", Reputation = -999 }
        ];

        var request = new UpdateGuildSheetRequest
        {
            GuildName = current.GuildName,
            DataJson = Serialize(current.Data),
            Version = current.Version
        };

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var final = await GetGuildAsync(client, campaign.Id);
        final.Data.Influence.Single(r => r.Name == "Cidade Alta").Reputation.Should().Be(100);
        final.Data.Influence.Single(r => r.Name == "Culto Sombrio").Reputation.Should().Be(-100);
    }

    // The structural validator (fix #1/#5) now rejects a null Knowledge list at the boundary
    // rather than silently coercing it on write — a null module/list is a malformed payload.
    [Fact]
    public async Task Update_WithNullKnowledgeList_Returns400AndGuildNotCorrupted()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var current = await GetGuildAsync(client, campaign.Id);
        var request = new UpdateGuildSheetRequest
        {
            GuildName = current.GuildName,
            DataJson = """{"knowledge":{"maps":null}}""",
            Version = current.Version
        };

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // The guild is untouched and still readable.
        var final = await GetGuildAsync(client, campaign.Id);
        final.Data.Knowledge.Maps.Should().NotBeNull();
    }

    // ── Fix #1/#5: malformed / null-list-element DataJson is rejected structurally at save time ──

    [Fact]
    public async Task Update_WithGarbageDataJson_Returns400AndGuildNotWiped()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var current = await GetGuildAsync(client, campaign.Id);
        current.Data.Prestige.Value = 55;
        var seed = new UpdateGuildSheetRequest
        {
            GuildName = "Seeded",
            DataJson = Serialize(current.Data),
            Version = current.Version
        };
        (await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", seed))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var seeded = await GetGuildAsync(client, campaign.Id);
        var garbage = new UpdateGuildSheetRequest
        {
            GuildName = "Wipe Attempt",
            DataJson = "garbage",
            Version = seeded.Version
        };

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", garbage);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Not a 200, not a wipe — the seeded state stands and GET still succeeds.
        var final = await GetGuildAsync(client, campaign.Id);
        final.GuildName.Should().Be("Seeded");
        final.Data.Prestige.Value.Should().Be(55);
    }

    // Server-side VE (StrategicValue) clamp (GDD range 0..5): out-of-range values sent by the client
    // are clamped on write rather than trusted, mirroring the reputation / research Points clamps.
    // VE is the CG Recursos contribution, so the clamp must also be reflected in DerivedStats.CgRecursos.
    [Fact]
    public async Task Update_WithOutOfRangeStrategicValue_ClampsToGdRangeAndCgRecursos()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var current = await GetGuildAsync(client, campaign.Id);
        // PactCoins is the other CG Recursos term — zero it so CgRecursos == the clamped VE sum.
        current.Data.Resources.PactCoins = 0;
        current.Data.Resources.Materials =
        [
            new MaterialStock { Name = "X", Quantity = 1, StrategicValue = 99 },
            new MaterialStock { Name = "Y", Quantity = 1, StrategicValue = -4 }
        ];

        var request = new UpdateGuildSheetRequest
        {
            GuildName = current.GuildName,
            DataJson = Serialize(current.Data),
            Version = current.Version
        };

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var final = await GetGuildAsync(client, campaign.Id);
        final.Data.Resources.Materials.Single(m => m.Name == "X").StrategicValue.Should().Be(5);
        final.Data.Resources.Materials.Single(m => m.Name == "Y").StrategicValue.Should().Be(0);
        // CG Recursos = PactCoins (0) + clamp(99,0,5) + clamp(-4,0,5) = 5 + 0 = 5.
        final.DerivedStats.CgRecursos.Should().Be(5);
    }

    // Backward-compat: a legacy blob whose materials carry NO strategicValue property deserializes
    // with VE 0, so those materials contribute 0 to CgRecursos — only PactCoins (face value) counts.
    // The inert quantity (9999) must not influence CgRecursos.
    [Fact]
    public async Task Update_WithLegacyMaterialsMissingStrategicValue_ContributeZeroToCgRecursos()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var current = await GetGuildAsync(client, campaign.Id);
        var request = new UpdateGuildSheetRequest
        {
            GuildName = current.GuildName,
            DataJson = """{"resources":{"pactCoins":7,"materials":[{"name":"Ferro","quantity":9999}]}}""",
            Version = current.Version
        };

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // CG Recursos = PactCoins (7) + VE (0, absent) = 7; the 9999 quantity is inert.
        var final = await GetGuildAsync(client, campaign.Id);
        final.DerivedStats.CgRecursos.Should().Be(7);
    }

    [Fact]
    public async Task Update_WithNullMaterialElement_Returns400AndGetStill200()
    {
        var (client, campaign, _, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var current = await GetGuildAsync(client, campaign.Id);
        var request = new UpdateGuildSheetRequest
        {
            GuildName = current.GuildName,
            DataJson = """{"resources":{"materials":[null]}}""",
            Version = current.Version
        };

        var response = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // The guild was not corrupted by the rejected payload.
        var final = await GetGuildAsync(client, campaign.Id);
        final.Should().NotBeNull();
        final.Data.Resources.Materials.Should().NotBeNull();
    }
}
