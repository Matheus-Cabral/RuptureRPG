using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Guilds;

// Interlude preview + per-indicator server-recomputed apply. The security invariant under test:
// ApplyInterludeRequest carries ONLY {Kind, TargetId?, Days} — no numeric delta — so the applied
// Silver/day change always equals the SERVER's rate×days, never anything the client could spoof.
public class GuildInterludeTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private async Task<(HttpClient Client, CampaignResponse Campaign, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Guild Interlude Test" });
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

    // Adds an active Operário worker: 2 Prata/day income + DailySalary/day maintenance.
    private async Task AddOperarioAsync(HttpClient client, Guid campaignId, int dailySalary)
    {
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaignId}/guild/staff",
            new CreateStaffRequest
            {
                Kind = "Worker",
                TypeOrRanking = GuildStaffTypes.Operario,
                Name = "Op",
                DailySalary = dailySalary,
                IsActive = true
            });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // Sets Resources.Silver via the version-safe PUT so preview/apply see a real balance.
    private async Task SetSilverAsync(HttpClient client, Guid campaignId, int silver)
    {
        var current = await GetGuildAsync(client, campaignId);
        current.Data.Resources.Silver = silver;
        var response = await client.PutAsJsonAsync($"api/campaigns/{campaignId}/guild",
            new UpdateGuildSheetRequest
            {
                GuildName = current.GuildName,
                DataJson = JsonSerializer.Serialize(current.Data, JsonOpts),
                Version = current.Version
            });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<ResearchProjectResponse> AddResearchAsync(
        HttpClient client, Guid campaignId, string complexity, int researchers, int points, int progressDays = 0)
    {
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaignId}/guild/research",
            new CreateResearchProjectRequest
            {
                Name = "Proj",
                Complexity = complexity,
                Researchers = researchers,
                Points = points,
                ProgressDays = progressDays,
                IsComplete = false
            });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ApiResponse<ResearchProjectResponse>>())!.Data!;
    }

    private async Task<CraftingOrderResponse> AddCraftingAsync(
        HttpClient client, Guid campaignId, int requiredDays, int progressDays = 0)
    {
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaignId}/guild/crafting",
            new CreateCraftingOrderRequest
            {
                Category = "Forja",
                ItemName = "Sword",
                Quality = "Comum",
                ProgressDays = progressDays,
                RequiredDays = requiredDays,
                Status = "EmAndamento"
            });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ApiResponse<CraftingOrderResponse>>())!.Data!;
    }

    private static async Task<InterludeProjection> ReadProjectionAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ApiResponse<InterludeProjection>>())!.Data!;

    private static async Task<GuildSheetResponse> ReadGuildAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;

    [Fact]
    public async Task Preview_ShowsMaintenanceIncomeResearchCrafting_WithCorrectSignsAndMagnitudes()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        await AddOperarioAsync(client, campaign.Id, dailySalary: 5); // income 2/d, maintenance 5/d
        await SetSilverAsync(client, campaign.Id, 1000);
        var research = await AddResearchAsync(client, campaign.Id, "Maior", researchers: 1, points: 3);
        var crafting = await AddCraftingAsync(client, campaign.Id, requiredDays: 30);

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/guild/interlude/preview?days=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var projection = await ReadProjectionAsync(response);

        projection.Days.Should().Be(10);
        var maintenance = projection.Indicators.Single(i => i.Kind == "Maintenance");
        maintenance.SilverDelta.Should().Be(-50); // 5/d × 10 (negative)
        var income = projection.Indicators.Single(i => i.Kind == "Income");
        income.SilverDelta.Should().Be(20); // 2/d × 10 (positive)

        var researchInd = projection.Indicators.Single(i => i.Kind == "ResearchProgress");
        researchInd.TargetId.Should().Be(research.Id);
        researchInd.DaysAdded.Should().Be(10); // min(1,2)×10, capped at remaining 20

        var craftingInd = projection.Indicators.Single(i => i.Kind == "CraftingProgress");
        craftingInd.TargetId.Should().Be(crafting.Id);
        craftingInd.DaysAdded.Should().Be(10); // 1/d × 10, capped at remaining 30
    }

    [Fact]
    public async Task ApplyMaintenance_DeductsSilver_ByServerRateTimesDays()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);
        await AddOperarioAsync(client, campaign.Id, dailySalary: 5); // maintenance 5/d
        await SetSilverAsync(client, campaign.Id, 100);

        var current = await GetGuildAsync(client, campaign.Id);
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "Maintenance", Days = 10, Version = current.Version });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadGuildAsync(response)).Data.Resources.Silver.Should().Be(50); // 100 - 5×10

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Data.Resources.Silver.Should().Be(50);
    }

    // Regression for the "interlude deduction silently reverted by the next Save" bug: after a
    // Maintenance apply mutates blob Silver server-side and bumps the Version, the page's fixed client
    // adopts refreshed.Data.Resources into _data and later PUTs that blob under the new Version. This
    // simulates exactly that follow-up Save (reduced Silver + bumped Version) and asserts the server
    // keeps the deduction — i.e. the deduction is not resurrected on the next blob write.
    [Fact]
    public async Task ApplyMaintenance_ThenNextBlobSave_KeepsDeduction()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);
        await AddOperarioAsync(client, campaign.Id, dailySalary: 5); // maintenance 5/d
        await SetSilverAsync(client, campaign.Id, 100);

        var current = await GetGuildAsync(client, campaign.Id);
        var applyResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "Maintenance", Days = 10, Version = current.Version });
        applyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await ReadGuildAsync(applyResponse);
        refreshed.Data.Resources.Silver.Should().Be(50); // 100 - 5×10

        // A plain GET already shows the reduced balance.
        (await GetGuildAsync(client, campaign.Id)).Data.Resources.Silver.Should().Be(50);

        // The fixed client saves the ADOPTED blob (reduced Silver) under the BUMPED Version.
        var save = await client.PutAsJsonAsync($"api/campaigns/{campaign.Id}/guild",
            new UpdateGuildSheetRequest
            {
                GuildName = refreshed.GuildName,
                DataJson = JsonSerializer.Serialize(refreshed.Data, JsonOpts),
                Version = refreshed.Version
            });
        save.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadGuildAsync(save)).Data.Resources.Silver.Should().Be(50);

        // And a subsequent GET confirms the deduction persisted — never resurrected to 100.
        (await GetGuildAsync(client, campaign.Id)).Data.Resources.Silver.Should().Be(50);
    }

    // A Maintenance apply is a blob read-modify-write, so it must be version-aware: if a concurrent
    // blob save moves the row's xmin between the client's load and the apply, the apply must 409 (and
    // NOT deduct) rather than silently swallow the concurrent writer's change. Regression for the
    // "SetExpectedVersion(guild, guild.Version) compares the row against itself" no-op bug.
    [Fact]
    public async Task ApplyMaintenance_WithStaleVersion_Returns409_AndDoesNotDeduct()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);
        await AddOperarioAsync(client, campaign.Id, dailySalary: 5); // maintenance 5/d
        await SetSilverAsync(client, campaign.Id, 100);

        // The client's projection is based on v1.
        var v1 = (await GetGuildAsync(client, campaign.Id)).Version;

        // A concurrent blob save bumps the row to v2 (Silver → 200), invalidating v1.
        await SetSilverAsync(client, campaign.Id, 200);

        // Applying Maintenance with the STALE v1 must be rejected, not applied on top of the v2 state.
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "Maintenance", Days = 10, Version = v1 });
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // The concurrent writer's balance stands; no deduction leaked through.
        (await GetGuildAsync(client, campaign.Id)).Data.Resources.Silver.Should().Be(200);
    }

    [Fact]
    public async Task ApplyMaintenance_FloorsSilverAtZero_WhenItWouldGoNegative()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);
        await AddOperarioAsync(client, campaign.Id, dailySalary: 5); // maintenance 5/d → 50 over 10 days
        await SetSilverAsync(client, campaign.Id, 10); // less than the 50 due

        var current = await GetGuildAsync(client, campaign.Id);
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "Maintenance", Days = 10, Version = current.Version });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadGuildAsync(response)).Data.Resources.Silver.Should().Be(0); // floored, not negative
    }

    [Fact]
    public async Task ApplyIncome_AddsSilver_ByServerRateTimesDays()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);
        await AddOperarioAsync(client, campaign.Id, dailySalary: 0); // income 2/d, no maintenance
        await SetSilverAsync(client, campaign.Id, 100);

        var current = await GetGuildAsync(client, campaign.Id);
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "Income", Days = 10, Version = current.Version });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadGuildAsync(response)).Data.Resources.Silver.Should().Be(120); // 100 + 2×10
    }

    [Fact]
    public async Task ApplyResearchProgress_Completes_SetsIsCompleteAndRaisesCgPesquisa()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);
        // Menor = 5 required days; 1 researcher × 10 days = 10 ≥ 5 → completes.
        var research = await AddResearchAsync(client, campaign.Id, "Menor", researchers: 1, points: 3);

        var before = await GetGuildAsync(client, campaign.Id);
        before.DerivedStats.CgPesquisa.Should().Be(0);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "ResearchProgress", TargetId = research.Id, Days = 10 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await GetGuildAsync(client, campaign.Id);
        var updated = after.Research.Single(r => r.Id == research.Id);
        updated.IsComplete.Should().BeTrue();
        updated.ProgressDays.Should().Be(updated.RequiredDays); // capped, not overshot
        after.DerivedStats.CgPesquisa.Should().Be(3); // completed → Points contribute
    }

    [Fact]
    public async Task ApplyResearchProgress_PartialAdvance_ByMinResearchersTwoTimesDays()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);
        // Maior = 20 required days; min(3,2)=2 researchers × 3 days = 6 → partial, not complete.
        var research = await AddResearchAsync(client, campaign.Id, "Maior", researchers: 3, points: 3);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "ResearchProgress", TargetId = research.Id, Days = 3 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await GetGuildAsync(client, campaign.Id);
        var updated = after.Research.Single(r => r.Id == research.Id);
        updated.ProgressDays.Should().Be(6); // min(3,2) × 3
        updated.IsComplete.Should().BeFalse();
        after.DerivedStats.CgPesquisa.Should().Be(0); // still incomplete
    }

    [Fact]
    public async Task ApplyCraftingProgress_Completes_SetsConcluido()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);
        var crafting = await AddCraftingAsync(client, campaign.Id, requiredDays: 10);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "CraftingProgress", TargetId = crafting.Id, Days = 10 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await GetGuildAsync(client, campaign.Id);
        var updated = after.Crafting.Single(c => c.Id == crafting.Id);
        updated.ProgressDays.Should().Be(10); // capped at RequiredDays
        updated.Status.Should().Be("Concluido");
    }

    [Fact]
    public async Task ApplyCraftingProgress_PartialAdvance_StaysEmAndamento()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);
        var crafting = await AddCraftingAsync(client, campaign.Id, requiredDays: 30);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "CraftingProgress", TargetId = crafting.Id, Days = 10 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await GetGuildAsync(client, campaign.Id);
        var updated = after.Crafting.Single(c => c.Id == crafting.Id);
        updated.ProgressDays.Should().Be(10);
        updated.Status.Should().Be("EmAndamento");
    }

    [Theory]
    [InlineData(3651)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Preview_DaysOutOfRange_Returns400(int days)
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.GetAsync($"api/campaigns/{campaign.Id}/guild/interlude/preview?days={days}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(3651)]
    [InlineData(0)]
    public async Task Apply_DaysOutOfRange_Returns400(int days)
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "Income", Days = days });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Apply_BadKind_Returns400()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "Nonsense", Days = 10 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApplyResearchProgress_ForeignTarget_Returns404()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "ResearchProgress", TargetId = Guid.NewGuid(), Days = 10 });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApplyResearchProgress_AlreadyCompleteTarget_Returns404()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);
        // Seed a research already complete → produces no indicator → 404 ResearchNotFound.
        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/research",
            new CreateResearchProjectRequest { Name = "Done", Complexity = "Menor", Points = 1, IsComplete = true });
        var research = (await created.Content.ReadFromJsonAsync<ApiResponse<ResearchProjectResponse>>())!.Data!;

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "ResearchProgress", TargetId = research.Id, Days = 10 });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApplyCraftingProgress_MissingTarget_Returns404()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "CraftingProgress", TargetId = Guid.NewGuid(), Days = 10 });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task NonMember_PreviewAndApply_Returns404()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);
        await GetGuildAsync(client, campaign.Id);

        // An unrelated player from a different campaign.
        var otherGm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, otherGm.AccessToken);
        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var outsider = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var preview = await client.GetAsync($"api/campaigns/{campaign.Id}/guild/interlude/preview?days=10");
        preview.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var apply = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/interlude/apply",
            new ApplyInterludeRequest { Kind = "Income", Days = 10 });
        apply.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
