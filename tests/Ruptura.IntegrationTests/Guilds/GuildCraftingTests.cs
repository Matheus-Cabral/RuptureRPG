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

public class GuildCraftingTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, CampaignResponse Campaign, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Guild Crafting Test" });
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

    private static async Task<CraftingOrderResponse> ReadCraftingAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ApiResponse<CraftingOrderResponse>>())!.Data!;

    [Fact]
    public async Task Member_AddCrafting_Returns201_AndGetShowsIt()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var request = new CreateCraftingOrderRequest
        {
            Category = "Forja",
            ItemName = "Espada",
            Quality = "Raro",
            RequiredDays = 6,
            Status = "EmAndamento"
        };
        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/crafting", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await ReadCraftingAsync(response);
        body.Category.Should().Be("Forja");
        body.ItemName.Should().Be("Espada");
        body.Quality.Should().Be("Raro");
        body.RequiredDays.Should().Be(6);
        body.Status.Should().Be("EmAndamento");

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Crafting.Should().ContainSingle();
        guild.Crafting[0].ItemName.Should().Be("Espada");
        guild.Crafting[0].RequiredDays.Should().Be(6);
    }

    [Fact]
    public async Task UpdateCrafting_StatusAndProgress_Returns200_AndReflected()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/crafting",
            new CreateCraftingOrderRequest { Category = "Forja", ItemName = "Espada", Quality = "Raro", RequiredDays = 6, Status = "EmAndamento" });
        var crafting = await ReadCraftingAsync(created);

        var update = new UpdateCraftingOrderRequest
        {
            Category = "Forja",
            ItemName = "Espada",
            Quality = "Raro",
            ProgressDays = 6,
            RequiredDays = 6,
            Status = "Concluido"
        };
        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/crafting/{crafting.Id}", update);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadCraftingAsync(response);
        body.Status.Should().Be("Concluido");
        body.ProgressDays.Should().Be(6);

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Crafting.Should().ContainSingle();
        guild.Crafting[0].Status.Should().Be("Concluido");
        guild.Crafting[0].ProgressDays.Should().Be(6);
    }

    [Fact]
    public async Task AddCrafting_InvalidCategory_Returns400()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/crafting",
            new CreateCraftingOrderRequest { Category = "Nope", ItemName = "X", Status = "EmAndamento" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddCrafting_InvalidStatus_Returns400()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/crafting",
            new CreateCraftingOrderRequest { Category = "Forja", ItemName = "X", Status = "Nonsense" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddCrafting_NegativeProgressAndRequiredDays_ClampedToZero()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/crafting",
            new CreateCraftingOrderRequest
            {
                Category = "Alquimia",
                ItemName = "Poção",
                ProgressDays = -3,
                RequiredDays = -6,
                Status = "EmAndamento"
            });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await ReadCraftingAsync(response);
        body.ProgressDays.Should().Be(0);
        body.RequiredDays.Should().Be(0);
    }

    [Fact]
    public async Task DeleteCrafting_RemovesIt()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await GetGuildAsync(client, campaign.Id);

        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/crafting",
            new CreateCraftingOrderRequest { Category = "Forja", ItemName = "Espada", Status = "EmAndamento" });
        var crafting = await ReadCraftingAsync(created);

        var response = await client.DeleteAsync($"api/campaigns/{campaign.Id}/guild/crafting/{crafting.Id}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        var guild = await GetGuildAsync(client, campaign.Id);
        guild.Crafting.Should().BeEmpty();
    }

    [Fact]
    public async Task NonMember_AddUpdateDelete_Returns404()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();

        AuthHelper.SetBearerToken(client, gmToken);
        await GetGuildAsync(client, campaign.Id);
        var created = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/crafting",
            new CreateCraftingOrderRequest { Category = "Forja", ItemName = "Espada", Status = "EmAndamento" });
        var crafting = await ReadCraftingAsync(created);

        var otherGm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, otherGm.AccessToken);
        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var outsider = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var add = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/guild/crafting",
            new CreateCraftingOrderRequest { Category = "Forja", ItemName = "X", Status = "EmAndamento" });
        add.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var upd = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/guild/crafting/{crafting.Id}",
            new UpdateCraftingOrderRequest { Category = "Forja", ItemName = "X", Status = "EmAndamento" });
        upd.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var del = await client.DeleteAsync($"api/campaigns/{campaign.Id}/guild/crafting/{crafting.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateDeleteCrafting_OfAnotherGuild_Returns404()
    {
        var (clientA, campaignA, _, gmTokenA) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(clientA, gmTokenA);
        await GetGuildAsync(clientA, campaignA.Id);
        var createdA = await clientA.PostAsJsonAsync($"api/campaigns/{campaignA.Id}/guild/crafting",
            new CreateCraftingOrderRequest { Category = "Forja", ItemName = "Espada", Status = "EmAndamento" });
        var craftingA = await ReadCraftingAsync(createdA);

        var campaignBResponse = await clientA.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Campaign B" });
        var campaignB = (await campaignBResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;
        await GetGuildAsync(clientA, campaignB.Id);

        var upd = await clientA.PutAsJsonAsync(
            $"api/campaigns/{campaignB.Id}/guild/crafting/{craftingA.Id}",
            new UpdateCraftingOrderRequest { Category = "Forja", ItemName = "Espada", Status = "EmAndamento" });
        upd.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var del = await clientA.DeleteAsync($"api/campaigns/{campaignB.Id}/guild/crafting/{craftingA.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var guildA = await GetGuildAsync(clientA, campaignA.Id);
        guildA.Crafting.Should().ContainSingle(c => c.Id == craftingA.Id);
    }
}
