using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Journal;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class JournalEntryControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, Guid SheetId, string PlayerToken, string GmToken)> GrantACharacterAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Journal Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        return (client, sheet.Id, player.AccessToken, gm.AccessToken);
    }

    [Fact]
    public async Task Create_AsOwner_Returns201()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "First day in the Dungeon." });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var entry = (await response.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;
        entry.Text.Should().Be("First day in the Dungeon.");
        entry.ImagePaths.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_AsCampaignGameMaster_Returns404()
    {
        var (client, sheetId, _, gmToken) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var response = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "GM trying to write." });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByCharacterSheet_AsCampaignGameMaster_Returns200()
    {
        var (client, sheetId, playerToken, gmToken) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "Entry one." });

        AuthHelper.SetBearerToken(client, gmToken);
        var response = await client.GetAsync($"api/character-sheets/{sheetId}/journal-entries");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var entries = (await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!;
        entries.Should().ContainSingle(e => e.Text == "Entry one.");
    }

    [Fact]
    public async Task GetByCharacterSheet_NewestFirst()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries", new CreateJournalEntryRequest { Text = "Older" });
        await Task.Delay(10); // ensure a distinct CreatedAt
        await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries", new CreateJournalEntryRequest { Text = "Newer" });

        var response = await client.GetAsync($"api/character-sheets/{sheetId}/journal-entries");
        var entries = (await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!.ToList();

        entries.Should().HaveCount(2);
        entries[0].Text.Should().Be("Newer");
        entries[1].Text.Should().Be("Older");
    }

    [Fact]
    public async Task Update_AsOwner_ReplacesText()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "Original" });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;

        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries/{entry.Id}",
            new UpdateJournalEntryRequest { Text = "Edited", ImagePaths = [] });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;
        updated.Text.Should().Be("Edited");
    }

    [Fact]
    public async Task Update_AsCampaignGameMaster_Returns404()
    {
        var (client, sheetId, playerToken, gmToken) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "Original" });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;

        AuthHelper.SetBearerToken(client, gmToken);
        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries/{entry.Id}",
            new UpdateJournalEntryRequest { Text = "GM trying to edit", ImagePaths = [] });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsOwner_Returns200AndEntryIsGone()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "To be deleted" });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;

        var deleteResponse = await client.DeleteAsync($"api/character-sheets/{sheetId}/journal-entries/{entry.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await client.GetAsync($"api/character-sheets/{sheetId}/journal-entries");
        var entries = (await listResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!;
        entries.Should().NotContain(e => e.Id == entry.Id);
    }
}
