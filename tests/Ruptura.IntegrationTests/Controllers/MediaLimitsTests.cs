using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;
using Ruptura.Shared.Journal;
using Ruptura.Shared.Media;

namespace Ruptura.IntegrationTests.Controllers;

/// <summary>
/// A dedicated factory that overrides MediaSettings:MaxImagesPerJournalEntry to a small
/// number (2) so its enforcement can be exercised without depending on the production
/// default configured elsewhere. Subclasses IntegrationTestFactory rather than modifying
/// it directly — other test classes depend on its unmodified config override behavior.
/// </summary>
public class MediaLimitsTestFactory : IntegrationTestFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MediaSettings:MaxImagesPerJournalEntry"] = "2"
            }));
    }
}

public class MediaLimitsTests(MediaLimitsTestFactory factory) : IClassFixture<MediaLimitsTestFactory>
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

    private static MultipartFormDataContent BuildUploadForm(Guid entryId) =>
        new()
        {
            { new ByteArrayContent(TinyPng), "file", "photo.png" },
            { new StringContent("JournalEntryImage"), "entityType" },
            { new StringContent(entryId.ToString()), "entityId" }
        };

    private async Task<(HttpClient Client, Guid SheetId)> GrantACharacterAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Media Limits Test" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = "Sir Aldric" });
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        AuthHelper.SetBearerToken(client, player.AccessToken);
        return (client, sheet.Id);
    }

    private async Task<Guid> CreateJournalEntryAsync(HttpClient client, Guid sheetId, string text)
    {
        var response = await client.PostAsJsonAsync($"api/character-sheets/{sheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = text });
        return (await response.Content.ReadFromJsonAsync<ApiResponse<JournalEntryResponse>>())!.Data!.Id;
    }

    [Fact]
    public async Task Upload_BeyondTheConfiguredMaxImagesPerJournalEntry_Returns400TooManyImages()
    {
        var (client, sheetId) = await GrantACharacterAsync();
        var entryId = await CreateJournalEntryAsync(client, sheetId, "Photo album");

        // Configured limit is 2 — the first two uploads must succeed.
        var first = await client.PostAsync("api/media", BuildUploadForm(entryId));
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        var second = await client.PostAsync("api/media", BuildUploadForm(entryId));
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        // The third must be rejected.
        var third = await client.PostAsync("api/media", BuildUploadForm(entryId));
        third.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var listResponse = await client.GetAsync($"api/character-sheets/{sheetId}/journal-entries");
        var entry = (await listResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!
            .Data!.Single(e => e.Id == entryId);
        entry.ImagePaths.Should().HaveCount(2);
    }

    [Fact]
    public async Task Upload_LimitIsEnforcedPerEntry_NotGlobally()
    {
        // With the same small configured limit (2), a second, distinct journal entry on the
        // same sheet can independently receive up to the limit — proving the check is scoped
        // to this entry's own ImagePaths.Count, not some global upload counter shared across
        // every entry the caller owns.
        var (client, sheetId) = await GrantACharacterAsync();
        var entryOneId = await CreateJournalEntryAsync(client, sheetId, "Entry one");
        var entryTwoId = await CreateJournalEntryAsync(client, sheetId, "Entry two");

        // Exhaust entry one's limit.
        (await client.PostAsync("api/media", BuildUploadForm(entryOneId))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostAsync("api/media", BuildUploadForm(entryOneId))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostAsync("api/media", BuildUploadForm(entryOneId))).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Entry two, being a separate entry, still has its own full allowance.
        (await client.PostAsync("api/media", BuildUploadForm(entryTwoId))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostAsync("api/media", BuildUploadForm(entryTwoId))).StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await client.GetAsync($"api/character-sheets/{sheetId}/journal-entries");
        var entries = (await listResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!.ToList();
        entries.Single(e => e.Id == entryOneId).ImagePaths.Should().HaveCount(2);
        entries.Single(e => e.Id == entryTwoId).ImagePaths.Should().HaveCount(2);
    }
}
