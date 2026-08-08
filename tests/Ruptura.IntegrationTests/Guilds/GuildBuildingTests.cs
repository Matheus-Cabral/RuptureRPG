using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Data;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildBuildingTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    // A seeded Skill catalog entry (NOT an Installation) — used for the InstallationInvalid case.
    private static readonly Guid SeededSkillId = Guid.Parse("60000000-0000-0000-0000-000000000001");

    private async Task<(HttpClient Client, CampaignResponse Campaign, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Guild Building Test" });
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
    public async Task Member_AddBuilding_Returns201AndShowsInGuildWithDerivedStats()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var request = new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Armazem, Level = 2, IsActive = true };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>())!.Data!;
        body.InstallationName.Should().Be("Armazém");
        body.Level.Should().Be(2);

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Buildings.Should().ContainSingle();
        guild.Buildings[0].InstallationName.Should().Be("Armazém");
        guild.Buildings[0].Level.Should().Be(2);
        guild.DerivedStats.StorageCapacity.Should().Be(100); // Armazém level 2 × 50
    }

    [Fact]
    public async Task AddBuilding_WithNonInstallationId_Returns400InstallationInvalid()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var request = new CreateBuildingRequest { CatalogEntryId = SeededSkillId, Level = 1, IsActive = true };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddBuilding_Portao_Returns400BuildingNotConstructible()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var request = new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Portao, Level = 1, IsActive = true };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddBuilding_LevelAboveCap_Returns400BuildingLevelInvalid()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        // Câmara do Conselho has LevelCap 2 — level 5 is invalid.
        var request = new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.CamaraDoConselho, Level = 5, IsActive = true };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddBuilding_SameInstallationTwice_SecondReturns400BuildingExists()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var request = new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Armazem, Level = 1, IsActive = true };
        var first = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings", request);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings", request);
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Member_UpdateBuilding_Returns200AndChangesReflected()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Armazem, Level = 1, IsActive = true });
        var createdBody = (await created.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>())!.Data!;

        var update = new UpdateBuildingRequest { Level = 3, IsActive = false };
        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/buildings/{createdBody.Id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>())!.Data!;
        body.Level.Should().Be(3);
        body.IsActive.Should().BeFalse();

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Buildings.Should().ContainSingle();
        guild.Buildings[0].Level.Should().Be(3);
        guild.Buildings[0].IsActive.Should().BeFalse();
        // Inactive building grants no benefit → StorageCapacity 0.
        guild.DerivedStats.StorageCapacity.Should().Be(0);
    }

    [Fact]
    public async Task UpdateBuilding_LevelAboveCap_Returns400()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.CamaraDoConselho, Level = 1, IsActive = true });
        var createdBody = (await created.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>())!.Data!;

        // Câmara do Conselho cap is 2.
        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/buildings/{createdBody.Id}",
            new UpdateBuildingRequest { Level = 5, IsActive = true });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Member_DeleteBuilding_RemovesFromGuild()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Armazem, Level = 1, IsActive = true });
        var createdBody = (await created.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>())!.Data!;

        var response = await client.DeleteAsync($"api/campaigns/{campaign.Id}/guild/buildings/{createdBody.Id}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Buildings.Should().BeEmpty();
    }

    [Fact]
    public async Task NonMember_AddUpdateDelete_Returns404()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();

        // Seed a building as GM so update/delete have a real target id.
        AuthHelper.SetBearerToken(client, gmToken);
        await GetGuildAsync(client, campaign.Id);
        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Armazem, Level = 1, IsActive = true });
        var createdBody = (await created.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>())!.Data!;

        // An outsider never added to this campaign.
        var otherGm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, otherGm.AccessToken);
        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var outsider = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var add = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Dormitorio, Level = 1, IsActive = true });
        add.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var upd = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/buildings/{createdBody.Id}",
            new UpdateBuildingRequest { Level = 2, IsActive = true });
        upd.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var del = await client.DeleteAsync($"api/campaigns/{campaign.Id}/guild/buildings/{createdBody.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDeleteBuilding_OfAnotherGuild_Returns404()
    {
        // Campaign A with its guild + building.
        var (clientA, campaignA, _, gmTokenA) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(clientA, gmTokenA);
        await GetGuildAsync(clientA, campaignA.Id);
        var createdA = await clientA.PostAsJsonAsync($"api/campaigns/{campaignA.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = GuildCatalogIds.Armazem, Level = 1, IsActive = true });
        var buildingA = (await createdA.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>())!.Data!;

        // Campaign B (same GM, its own guild).
        var campaignBResponse = await clientA.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Campaign B" });
        var campaignB = (await campaignBResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;
        await GetGuildAsync(clientA, campaignB.Id);

        // Cross-guild update/delete through campaign B's route → 404.
        var upd = await clientA.PutAsJsonAsync(
            $"api/campaigns/{campaignB.Id}/guild/buildings/{buildingA.Id}",
            new UpdateBuildingRequest { Level = 2, IsActive = true });
        upd.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var del = await clientA.DeleteAsync($"api/campaigns/{campaignB.Id}/guild/buildings/{buildingA.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Building A still there under campaign A.
        var guildA = await GetGuildAsync(clientA, campaignA.Id);
        guildA.Buildings.Should().ContainSingle(b => b.Id == buildingA.Id);
    }

    [Fact]
    public async Task UpdateBuilding_WhenInstallationLaterArchived_Returns200()
    {
        // Regression: archiving a homebrew installation must NOT permanently freeze a building already
        // built from it — UpdateBuildingAsync passes allowArchived:true so it stays editable/deactivatable.
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        // Seed a homebrew Installation scoped to this campaign, directly via DbContext.
        var installationId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.CatalogEntries.Add(new CatalogEntry
            {
                Id = installationId,
                Type = CatalogEntryType.Installation,
                CampaignId = campaign.Id,
                Name = "Homebrew Forge",
                // Catalog blobs use DEFAULT (PascalCase) JSON — match ValidateInstallationAsync.
                DataJson = JsonSerializer.Serialize(new InstallationCatalogData
                {
                    Category = "Produção", Weight = 1, LevelCap = 5, NonConstructible = false
                })
            });
            await db.SaveChangesAsync();
        }

        // Build it while the installation is still active.
        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
            new CreateBuildingRequest { CatalogEntryId = installationId, Level = 1, IsActive = true });
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var building = (await created.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>())!.Data!;

        // Archive the catalog entry after the building already exists.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var entry = db.CatalogEntries.Single(e => e.Id == installationId);
            entry.IsArchived = true;
            await db.SaveChangesAsync();
        }

        // Editing/deactivating the existing building must still succeed.
        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/buildings/{building.Id}",
            new UpdateBuildingRequest { Level = 2, IsActive = false });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<GuildBuildingResponse>>())!.Data!;
        body.Level.Should().Be(2);
        body.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateBuildingBeyondCs_Returns200AndDerivedStatsFlagsOverflow()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        // Base CS is 5. Add 6 active constructible installations that do NOT raise CS
        // (avoid Armazém and Centro Logístico) → ActiveBuildingCount 6 > CS 5 → overflow.
        Guid[] installations =
        [
            GuildCatalogIds.Dormitorio,
            GuildCatalogIds.CampoDeTreinamento,
            GuildCatalogIds.Biblioteca,
            Guid.Parse("d0000000-0000-0000-0000-000000000005"), // Ferraria
            Guid.Parse("d0000000-0000-0000-0000-000000000006"), // Oficina
            Guid.Parse("d0000000-0000-0000-0000-000000000008"), // Enfermaria
        ];

        foreach (var id in installations)
        {
            var r = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/buildings",
                new CreateBuildingRequest { CatalogEntryId = id, Level = 1, IsActive = true });
            r.StatusCode.Should().Be(HttpStatusCode.Created); // never blocked by CS overflow
        }

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.DerivedStats.ActiveBuildingCount.Should().Be(6);
        guild.DerivedStats.Cs.Should().Be(5);
        guild.DerivedStats.ActiveBuildingOverflow.Should().BeTrue();
    }
}
