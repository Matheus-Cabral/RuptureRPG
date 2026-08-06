using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;
using Ruptura.Shared.Journal;
using Ruptura.Shared.Media;

namespace Ruptura.IntegrationTests.Controllers;

public class JournalMediaFlowTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private static readonly byte[] TinyPng =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
        0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE, 0x00, 0x00, 0x00,
        0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
        0x00, 0x03, 0x01, 0x01, 0x00, 0x18, 0xDD, 0x8D, 0xB0, 0x00, 0x00, 0x00,
        0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
    ];

    private static MultipartFormDataContent BuildUploadForm(string entityType, Guid entityId) =>
        new()
        {
            { new ByteArrayContent(TinyPng), "file", "photo.png" },
            { new StringContent(entityType), "entityType" },
            { new StringContent(entityId.ToString()), "entityId" }
        };

    [Fact]
    public async Task FullFlow_JournalLifecycleMediaLifecyclePortraitReplace_Succeeds()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Journal/Media E2E" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        // 1. Player creates a journal entry (text-only at creation).
        AuthHelper.SetBearerToken(client, player.AccessToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheet.Id}/journal-entries",
            new CreateJournalEntryRequest { Text = "Arrived at the Dungeon gates." });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;
        entry.ImagePaths.Should().BeEmpty();

        // 2. Player attaches two images.
        var upload1 = (await (await client.PostAsync("api/media", BuildUploadForm("JournalEntryImage", entry.Id)))
            .Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        var upload2 = (await (await client.PostAsync("api/media", BuildUploadForm("JournalEntryImage", entry.Id)))
            .Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;

        File.Exists(Path.Combine(factory.MediaRoot, upload1.Path)).Should().BeTrue();
        File.Exists(Path.Combine(factory.MediaRoot, upload2.Path)).Should().BeTrue();

        var afterUploads = (await (await client.GetAsync($"api/character-sheets/{sheet.Id}/journal-entries"))
            .Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!.Single(e => e.Id == entry.Id);
        afterUploads.ImagePaths.Should().BeEquivalentTo([upload1.Path, upload2.Path]);

        // 3. Player edits the entry, dropping the first image — its file must be deleted.
        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}/journal-entries/{entry.Id}",
            new UpdateJournalEntryRequest { Text = "Arrived at the Dungeon gates. (edited)", ImagePaths = [upload2.Path] });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        File.Exists(Path.Combine(factory.MediaRoot, upload1.Path)).Should().BeFalse();
        File.Exists(Path.Combine(factory.MediaRoot, upload2.Path)).Should().BeTrue();

        // 4. Player deletes the entry — its remaining image file must be deleted too.
        var deleteResponse = await client.DeleteAsync($"api/character-sheets/{sheet.Id}/journal-entries/{entry.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        File.Exists(Path.Combine(factory.MediaRoot, upload2.Path)).Should().BeFalse();

        // 5. Player uploads a portrait, then replaces it — the old file must be deleted.
        var portrait1 = (await (await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheet.Id)))
            .Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        File.Exists(Path.Combine(factory.MediaRoot, portrait1.Path)).Should().BeTrue();

        var portrait2 = (await (await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheet.Id)))
            .Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        File.Exists(Path.Combine(factory.MediaRoot, portrait1.Path)).Should().BeFalse();
        File.Exists(Path.Combine(factory.MediaRoot, portrait2.Path)).Should().BeTrue();

        var refreshedSheet = (await (await client.GetAsync($"api/character-sheets/{sheet.Id}"))
            .Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        refreshedSheet.PortraitImagePath.Should().Be(portrait2.Path);

        // 6. The portrait is downloadable by its owner.
        var downloadResponse = await client.GetAsync($"api/media/{portrait2.Path}");
        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        (await downloadResponse.Content.ReadAsByteArrayAsync()).Should().BeEquivalentTo(TinyPng);

        // 7. The GM can read the journal (now empty) and the portrait, but cannot write either.
        AuthHelper.SetBearerToken(client, gm.AccessToken);
        var gmJournalRead = await client.GetAsync($"api/character-sheets/{sheet.Id}/journal-entries");
        gmJournalRead.StatusCode.Should().Be(HttpStatusCode.OK);
        (await gmJournalRead.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!.Should().BeEmpty();

        var gmPortraitDownload = await client.GetAsync($"api/media/{portrait2.Path}");
        gmPortraitDownload.StatusCode.Should().Be(HttpStatusCode.OK);

        var gmJournalWrite = await client.PostAsJsonAsync($"api/character-sheets/{sheet.Id}/journal-entries",
            new CreateJournalEntryRequest { Text = "GM trying to write." });
        gmJournalWrite.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 8. A completely unrelated GM (different campaign) is blocked from everything.
        var outsider = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        (await client.GetAsync($"api/character-sheets/{sheet.Id}/journal-entries")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.GetAsync($"api/media/{portrait2.Path}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
