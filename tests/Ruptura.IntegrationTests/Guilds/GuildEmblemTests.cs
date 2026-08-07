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
using Ruptura.Shared.Media;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildEmblemTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    // A minimal, valid 1x1 PNG (correct magic bytes) — mirrors MediaControllerTests.TinyPng.
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

    private static MultipartFormDataContent BuildEmblemForm(Guid entityId, uint version, byte[]? bytes = null)
    {
        var content = BuildUploadForm("GuildEmblem", entityId, bytes);
        content.Add(new StringContent(version.ToString()), "version");
        return content;
    }

    // Bump the guild's xmin via a valid PUT (no emblem set) so a previously-read version goes stale.
    private static async Task<uint> BumpVersionAsync(HttpClient client, Guid campaignId)
    {
        var current = await GetGuildAsync(client, campaignId);
        var request = new UpdateGuildSheetRequest
        {
            GuildName = current.GuildName + " v2",
            DataJson = JsonSerializer.Serialize(current.Data, new JsonSerializerOptions(JsonSerializerDefaults.Web)),
            Version = current.Version
        };
        var response = await client.PutAsJsonAsync($"api/campaigns/{campaignId}/guild", request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await GetGuildAsync(client, campaignId)).Version;
    }

    private async Task<(HttpClient Client, CampaignResponse Campaign, string PlayerToken, string GmToken)>
        SetUpCampaignWithMemberAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Emblem Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        return (client, campaign, player.AccessToken, gm.AccessToken);
    }

    private static async Task<GuildSheetResponse> GetGuildAsync(HttpClient client, Guid campaignId)
    {
        var response = await client.GetAsync($"api/campaigns/{campaignId}/guild");
        return (await response.Content.ReadFromJsonAsync<ApiResponse<GuildSheetResponse>>())!.Data!;
    }

    [Fact]
    public async Task Upload_EmblemAsMember_SavesFileAndUpdatesGuildBlob()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var guild = await GetGuildAsync(client, campaign.Id);

        var response = await client.PostAsync("api/media", BuildEmblemForm(guild.Id, guild.Version));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var upload = (await response.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        upload.Path.Should().StartWith($"guild-sheets/{guild.Id}/emblem-");
        upload.Version.Should().NotBeNull();
        File.Exists(Path.Combine(factory.MediaRoot, upload.Path)).Should().BeTrue();

        var reloaded = await GetGuildAsync(client, campaign.Id);
        reloaded.Data.Identity.EmblemImagePath.Should().Be(upload.Path);
    }

    [Fact]
    public async Task Upload_EmblemAsNonMember_Returns404()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var guild = await GetGuildAsync(client, campaign.Id);

        var outsider = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var response = await client.PostAsync("api/media", BuildEmblemForm(guild.Id, guild.Version));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Download_EmblemAsMember_Returns200WithImageBytes()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var guild = await GetGuildAsync(client, campaign.Id);
        var uploadResponse = await client.PostAsync("api/media", BuildEmblemForm(guild.Id, guild.Version));
        var upload = (await uploadResponse.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;

        var downloadResponse = await client.GetAsync($"api/media/{upload.Path}");

        downloadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await downloadResponse.Content.ReadAsByteArrayAsync();
        bytes.Should().BeEquivalentTo(TinyPng);
    }

    [Fact]
    public async Task Download_EmblemAsNonMember_Returns404()
    {
        var (client, campaign, playerToken, _) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, playerToken);
        var guild = await GetGuildAsync(client, campaign.Id);
        var uploadResponse = await client.PostAsync("api/media", BuildEmblemForm(guild.Id, guild.Version));
        var upload = (await uploadResponse.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;

        var outsider = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, outsider.AccessToken);

        var downloadResponse = await client.GetAsync($"api/media/{upload.Path}");

        downloadResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Fix #2/#3: version-checkpointed emblem upload ──

    [Fact]
    public async Task Upload_EmblemWithStaleVersion_Returns409AndEmblemUnchangedAndNoOrphanFile()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        // Read v1, then bump to v2 via a plain PUT (no emblem set) so v1 is now stale.
        var v1 = (await GetGuildAsync(client, campaign.Id)).Version;
        await BumpVersionAsync(client, campaign.Id);

        var guildId = (await GetGuildAsync(client, campaign.Id)).Id;
        var response = await client.PostAsync("api/media", BuildEmblemForm(guildId, v1));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Emblem unchanged (still none) and the rejected upload left no file behind.
        var final = await GetGuildAsync(client, campaign.Id);
        final.Data.Identity.EmblemImagePath.Should().BeEmpty();

        var guildDir = Path.Combine(factory.MediaRoot, "guild-sheets", guildId.ToString());
        var orphaned = Directory.Exists(guildDir) && Directory.GetFiles(guildDir).Length > 0;
        orphaned.Should().BeFalse();
    }

    [Fact]
    public async Task Upload_EmblemWithStaleVersion_WhenEmblemAlreadyExists_PreservesOldFileAndBlob()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        // First upload with the correct version establishes an emblem (file + blob path).
        var guild = await GetGuildAsync(client, campaign.Id);
        var firstResponse = await client.PostAsync("api/media", BuildEmblemForm(guild.Id, guild.Version));
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstUpload = (await firstResponse.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        var originalPath = firstUpload.Path;
        var originalVersion = firstUpload.Version!.Value;
        File.Exists(Path.Combine(factory.MediaRoot, originalPath)).Should().BeTrue();

        // Bump the guild's xmin so originalVersion is now stale, then attempt a SECOND upload with it.
        await BumpVersionAsync(client, campaign.Id);
        var staleResponse = await client.PostAsync("api/media", BuildEmblemForm(guild.Id, originalVersion));

        staleResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // The original emblem file is still on disk and the blob still references it.
        File.Exists(Path.Combine(factory.MediaRoot, originalPath)).Should().BeTrue();
        var final = await GetGuildAsync(client, campaign.Id);
        final.Data.Identity.EmblemImagePath.Should().Be(originalPath);
    }

    [Fact]
    public async Task Upload_EmblemWithCorrectVersion_Returns200WithNewVersion()
    {
        var (client, campaign, _, gmToken) = await SetUpCampaignWithMemberAsync();
        AuthHelper.SetBearerToken(client, gmToken);

        var guild = await GetGuildAsync(client, campaign.Id);
        var response = await client.PostAsync("api/media", BuildEmblemForm(guild.Id, guild.Version));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var upload = (await response.Content.ReadFromJsonAsync<ApiResponse<MediaUploadResponse>>())!.Data!;
        upload.Version.Should().NotBeNull();
        upload.Version.Should().NotBe(guild.Version);
        File.Exists(Path.Combine(factory.MediaRoot, upload.Path)).Should().BeTrue();
    }
}
