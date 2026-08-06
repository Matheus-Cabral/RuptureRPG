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

public class MediaControllerTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    // A minimal, valid 1x1 PNG (correct magic bytes) used across every upload test.
    private static readonly byte[] TinyPng =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D,
        0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
        0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53, 0xDE, 0x00, 0x00, 0x00,
        0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00,
        0x00, 0x03, 0x01, 0x01, 0x00, 0x18, 0xDD, 0x8D, 0xB0, 0x00, 0x00, 0x00,
        0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82
    ];

    private static MultipartFormDataContent BuildUploadForm(string entityType, Guid entityId, byte[]? bytes = null)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes ?? TinyPng);
        content.Add(fileContent, "file", "upload.png");
        content.Add(new StringContent(entityType), "entityType");
        content.Add(new StringContent(entityId.ToString()), "entityId");
        return content;
    }

    private async Task<(HttpClient Client, Guid SheetId, string PlayerToken, string GmToken)> GrantACharacterAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Media Test" });
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
    public async Task Upload_PortraitAsOwner_SavesFileAndUpdatesSheet()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var upload = (await response.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        upload.Path.Should().StartWith($"character-sheets/{sheetId}/portrait-");
        File.Exists(Path.Combine(factory.MediaRoot, upload.Path)).Should().BeTrue();

        var sheetResponse = await client.GetAsync($"api/character-sheets/{sheetId}");
        var sheet = (await sheetResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        sheet.PortraitImagePath.Should().Be(upload.Path);
    }

    [Fact]
    public async Task Upload_PortraitReplacement_DeletesTheOldFile()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var firstResponse = await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));
        var firstUpload = (await firstResponse.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        var firstFullPath = Path.Combine(factory.MediaRoot, firstUpload.Path);
        File.Exists(firstFullPath).Should().BeTrue();

        await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));

        File.Exists(firstFullPath).Should().BeFalse();
    }

    [Fact]
    public async Task Upload_PortraitAsUnrelatedPlayer_Returns404()
    {
        var (client, sheetId, _, _) = await GrantACharacterAsync();
        var outsider = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var response = await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Upload_JournalImageAsOwner_AppendsToImagePaths()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "Photo day" });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;

        var response = await client.PostAsync("api/media", BuildUploadForm("JournalEntryImage", entry.Id));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await client.GetAsync($"api/character-sheets/{sheetId}/journal-entries");
        var refreshed = (await listResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!
            .Data!.Single(e => e.Id == entry.Id);
        refreshed.ImagePaths.Should().ContainSingle();
    }

    [Fact]
    public async Task Upload_JournalImageAsCampaignGameMaster_Returns404()
    {
        var (client, sheetId, playerToken, gmToken) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var createResponse = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "x" });
        var entry = (await createResponse.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!;

        AuthHelper.SetBearerToken(client, gmToken);
        var response = await client.PostAsync("api/media", BuildUploadForm("JournalEntryImage", entry.Id));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Upload_WithUnrecognizedFileBytes_Returns400()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.PostAsync("api/media",
            BuildUploadForm("CharacterSheetPortrait", sheetId, "not an image"u8.ToArray()));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_WithInvalidEntityType_Returns400()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.PostAsync("api/media", BuildUploadForm("SomethingElse", sheetId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Download_PortraitAsOwner_Returns200WithImageBytes()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var uploadResponse = await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));
        var upload = (await uploadResponse.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;

        var downloadResponse = await client.GetAsync($"api/media/{upload.Path}");

        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        bytes.Should().BeEquivalentTo(TinyPng);
    }

    [Fact]
    public async Task Download_AsUnrelatedCaller_Returns404()
    {
        var (client, sheetId, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var uploadResponse = await client.PostAsync("api/media", BuildUploadForm("CharacterSheetPortrait", sheetId));
        var upload = (await uploadResponse.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;

        var outsider = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var downloadResponse = await client.GetAsync($"api/media/{upload.Path}");

        downloadResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Download_WithMalformedPath_Returns404()
    {
        var (client, _, playerToken, _) = await GrantACharacterAsync();
        AuthHelper.SetBearerToken(client, playerToken);

        var response = await client.GetAsync("api/media/not-a-real-path");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
