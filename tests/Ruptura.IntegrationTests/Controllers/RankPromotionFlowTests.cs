using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;
using Ruptura.Shared.Notifications;

namespace Ruptura.IntegrationTests.Controllers;

public class RankPromotionFlowTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    [Fact]
    public async Task FullFlow_NpExceedsRankCeiling_NotifiesPromotesAndDismisses()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Promotion Campaign" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        var playerId = player.User.Id;
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = playerId });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        // A homebrew "Divino" item alone (NP weight 50) plus every attribute maxed at 6
        // (grade bonus 5 each × 8 = 40) totals 90 NP — comfortably past Bronze's 70-point
        // ceiling but still under Ferro's 105, so exactly one promotion should be offered.
        var itemResponse = await client.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaign.Id,
            Type = "EquipmentItem",
            Name = "Relíquia Divina",
            DataJson = """{"Category":"item","Rarity":"Divino","Weight":0}"""
        });
        itemResponse.EnsureSuccessStatusCode();
        var item = (await itemResponse.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>())!.Data!;

        sheet.Data.Attributes.Corpo = 6;
        sheet.Data.Attributes.Controle = 6;
        sheet.Data.Attributes.Vigor = 6;
        sheet.Data.Attributes.Presenca = 6;
        sheet.Data.Attributes.Intelecto = 6;
        sheet.Data.Attributes.Percepcao = 6;
        sheet.Data.Attributes.Vontade = 6;
        sheet.Data.Attributes.Afinidade = 6;
        sheet.Data.Equipment.Add(new CharacterEquipmentEntry { CatalogEntryId = item.Id });

        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName,
            DataJson = JsonSerializer.Serialize(sheet.Data)
        });
        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        updated.DerivedStats.Np.Should().Be(90);
        updated.Data.GuildRegistry.Ranking.Should().Be("Bronze"); // unchanged — promotion is opt-in, not automatic

        // A notification should now be waiting for the GM.
        var groups = await GetNotificationGroupsAsync(client);
        groups.Should().ContainSingle(g => g.CampaignId == campaign.Id);
        var group = groups.Single(g => g.CampaignId == campaign.Id);
        group.Notifications.Should().ContainSingle(n => n.RelatedCharacterSheetId == sheet.Id && !n.IsRead);
        var notification = group.Notifications.Single(n => n.RelatedCharacterSheetId == sheet.Id);

        // Saving again without further changes must not create a second notification —
        // one is already unread for this sheet.
        var resaveResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = updated.CharacterName,
            DataJson = JsonSerializer.Serialize(updated.Data)
        });
        resaveResponse.EnsureSuccessStatusCode();
        var groupsAfterResave = await GetNotificationGroupsAsync(client);
        groupsAfterResave.Single(g => g.CampaignId == campaign.Id).Notifications
            .Count(n => n.RelatedCharacterSheetId == sheet.Id).Should().Be(1);

        // Promoting advances exactly one rank and clears the notification.
        var promoteResponse = await client.PostAsync($"api/notifications/{notification.Id}/promote", null);
        promoteResponse.EnsureSuccessStatusCode();

        var sheetAfterPromoteResponse = await client.GetAsync($"api/character-sheets/{sheet.Id}");
        var sheetAfterPromote = (await sheetAfterPromoteResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        sheetAfterPromote.Data.GuildRegistry.Ranking.Should().Be("Ferro");

        var groupsAfterPromote = await GetNotificationGroupsAsync(client);
        groupsAfterPromote.SelectMany(g => g.Notifications).Should().NotContain(n => n.Id == notification.Id);
    }

    [Fact]
    public async Task Dismiss_MarksNotificationReadWithoutChangingRanking()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Dismiss Campaign" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        var playerId = player.User.Id;
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = playerId });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Dame Lysbet" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        var itemResponse = await client.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaign.Id,
            Type = "EquipmentItem",
            Name = "Relíquia Divina 2",
            DataJson = """{"Category":"item","Rarity":"Divino","Weight":0}"""
        });
        var item = (await itemResponse.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>())!.Data!;

        sheet.Data.Attributes.Corpo = 6;
        sheet.Data.Attributes.Controle = 6;
        sheet.Data.Attributes.Vigor = 6;
        sheet.Data.Attributes.Presenca = 6;
        sheet.Data.Attributes.Intelecto = 6;
        sheet.Data.Attributes.Percepcao = 6;
        sheet.Data.Attributes.Vontade = 6;
        sheet.Data.Attributes.Afinidade = 6;
        sheet.Data.Equipment.Add(new CharacterEquipmentEntry { CatalogEntryId = item.Id });

        await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName,
            DataJson = JsonSerializer.Serialize(sheet.Data)
        });

        var groups = await GetNotificationGroupsAsync(client);
        var notification = groups.Single(g => g.CampaignId == campaign.Id).Notifications.Single(n => n.RelatedCharacterSheetId == sheet.Id);

        var dismissResponse = await client.PostAsync($"api/notifications/{notification.Id}/dismiss", null);
        dismissResponse.EnsureSuccessStatusCode();

        var groupsAfterDismiss = await GetNotificationGroupsAsync(client);
        groupsAfterDismiss.SelectMany(g => g.Notifications).Should().NotContain(n => n.Id == notification.Id);

        var sheetAfterDismissResponse = await client.GetAsync($"api/character-sheets/{sheet.Id}");
        var sheetAfterDismiss = (await sheetAfterDismissResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        sheetAfterDismiss.Data.GuildRegistry.Ranking.Should().Be("Bronze"); // dismiss never changes the rank
    }

    [Fact]
    public async Task Player_AttemptingToChangeRanking_IsSilentlyIgnored()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Rank Campaign" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        var playerId = player.User.Id;
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = playerId });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Dame Lysbet" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        AuthHelper.SetBearerToken(client, player.AccessToken);
        sheet.Data.GuildRegistry.Ranking = "Ferro";
        var newCharacterName = "Dame Lysbet the Bold";

        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = newCharacterName,
            DataJson = JsonSerializer.Serialize(sheet.Data)
        });

        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        updated.CharacterName.Should().Be(newCharacterName); // rest of the payload still applied
        updated.Data.GuildRegistry.Ranking.Should().Be("Bronze"); // server stayed authoritative

        var getResponse = await client.GetAsync($"api/character-sheets/{sheet.Id}");
        var persisted = (await getResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        persisted.Data.GuildRegistry.Ranking.Should().Be("Bronze");
    }

    [Fact]
    public async Task Notification_IsInvisibleAndInaccessibleToADifferentGameMaster()
    {
        var clientA = factory.CreateClient();
        var gmA = await AuthHelper.RegisterGameMasterAsync(clientA, Faker.Internet.Email());
        AuthHelper.SetBearerToken(clientA, gmA.AccessToken);

        var campaignResponse = await clientA.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "GM-A's Campaign" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await clientA.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(clientA, inviteCode, Faker.Internet.Email());
        var playerId = player.User.Id;
        await clientA.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = playerId });

        var grantResponse = await clientA.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "GM-A's Character" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        var itemResponse = await clientA.PostAsJsonAsync("api/catalog", new CreateCatalogEntryRequest
        {
            CampaignId = campaign.Id,
            Type = "EquipmentItem",
            Name = "Relíquia Divina 3",
            DataJson = """{"Category":"item","Rarity":"Divino","Weight":0}"""
        });
        var item = (await itemResponse.Content.ReadFromJsonAsync<ApiResponse<CatalogEntryResponse>>())!.Data!;

        sheet.Data.Attributes.Corpo = 6;
        sheet.Data.Attributes.Controle = 6;
        sheet.Data.Attributes.Vigor = 6;
        sheet.Data.Attributes.Presenca = 6;
        sheet.Data.Attributes.Intelecto = 6;
        sheet.Data.Attributes.Percepcao = 6;
        sheet.Data.Attributes.Vontade = 6;
        sheet.Data.Attributes.Afinidade = 6;
        sheet.Data.Equipment.Add(new CharacterEquipmentEntry { CatalogEntryId = item.Id });

        await clientA.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName,
            DataJson = JsonSerializer.Serialize(sheet.Data)
        });

        var groupsA = await GetNotificationGroupsAsync(clientA);
        var notification = groupsA.Single(g => g.CampaignId == campaign.Id).Notifications.Single(n => n.RelatedCharacterSheetId == sheet.Id);

        // A second, unrelated GM must see none of this and must not be able to act on it.
        var clientB = factory.CreateClient();
        var gmB = await AuthHelper.RegisterGameMasterAsync(clientB, Faker.Internet.Email());
        AuthHelper.SetBearerToken(clientB, gmB.AccessToken);

        var groupsB = await GetNotificationGroupsAsync(clientB);
        groupsB.SelectMany(g => g.Notifications).Should().NotContain(n => n.Id == notification.Id);

        var promoteAsGmBResponse = await clientB.PostAsync($"api/notifications/{notification.Id}/promote", null);
        promoteAsGmBResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var dismissAsGmBResponse = await clientB.PostAsync($"api/notifications/{notification.Id}/dismiss", null);
        dismissAsGmBResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // GM-A's own view is unaffected by GM-B's failed attempts.
        var groupsAAfter = await GetNotificationGroupsAsync(clientA);
        groupsAAfter.SelectMany(g => g.Notifications).Should().Contain(n => n.Id == notification.Id);
    }

    private static async Task<List<NotificationGroupResponse>> GetNotificationGroupsAsync(HttpClient client)
    {
        var response = await client.GetAsync("api/notifications");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<NotificationGroupResponse>>>();
        return body!.Data!.ToList();
    }
}
