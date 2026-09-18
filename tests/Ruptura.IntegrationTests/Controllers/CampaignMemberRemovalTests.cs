using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Auth;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;
using Ruptura.Shared.Journal;

namespace Ruptura.IntegrationTests.Controllers;

public class CampaignMemberRemovalTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private const string GmPassword = "TestPass1"; // AuthHelper's default
    private static readonly Faker Faker = new();

    private record Setup(
        HttpClient Gm, HttpClient Player, Guid CampaignId, Guid PlayerId, Guid SheetId, string PlayerEmail);

    /// <summary>GM + campaign + one member with a character sheet and a journal entry.</summary>
    private async Task<Setup> SetupCampaignWithMemberAsync()
    {
        var gm = factory.CreateClient();
        var gmAuth = await AuthHelper.RegisterGameMasterAsync(gm, Faker.Internet.Email(), GmPassword);
        AuthHelper.SetBearerToken(gm, gmAuth.AccessToken);

        var campaign = (await (await gm.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Removal" }))
            .Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var invite = (await (await gm.PostAsync("api/invites", null)).Content
            .ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!;
        var playerEmail = Faker.Internet.Email();
        var playerAuth = await AuthHelper.RegisterPlayerAsync(gm, invite.Code, playerEmail);
        var playerId = playerAuth.User.Id;
        (await gm.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members",
            new AssignMemberRequest { PlayerId = playerId })).EnsureSuccessStatusCode();

        var sheet = (await (await gm.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
                new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" }))
            .Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        var player = factory.CreateClient();
        AuthHelper.SetBearerToken(player, playerAuth.AccessToken);
        (await player.PostAsJsonAsync($"api/character-sheets/{sheet.Id}/journal-entries",
            new CreateJournalEntryRequest { Text = "Day one." })).EnsureSuccessStatusCode();

        return new Setup(gm, player, campaign.Id, playerId, sheet.Id, playerEmail);
    }

    private static Task<HttpResponseMessage> RemoveAsync(Setup s, string password) =>
        s.Gm.PostAsJsonAsync($"api/campaigns/{s.CampaignId}/members/{s.PlayerId}/remove",
            new RemoveMemberRequest { Password = password });

    [Fact]
    public async Task FullFlow_RemovingMember_CutsPlayerOffButKeepsDataForGm_AndReAddRestoresAccess()
    {
        var s = await SetupCampaignWithMemberAsync();
        (await s.Player.GetAsync($"api/character-sheets/{s.SheetId}")).StatusCode.Should().Be(HttpStatusCode.OK);

        // 1. GM removes the member with their own password
        var removed = await RemoveAsync(s, GmPassword);
        removed.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. The member list no longer has them
        var members = (await (await s.Gm.GetAsync($"api/campaigns/{s.CampaignId}/members")).Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignMemberResponse>>>())!.Data!;
        members.Should().NotContain(m => m.PlayerId == s.PlayerId);

        // 3. The player lost the campaign, the sheet (read + write), the journal and "mine"
        var mine = (await (await s.Player.GetAsync("api/campaigns/mine")).Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignResponse>>>())!.Data!;
        mine.Should().NotContain(c => c.Id == s.CampaignId);
        (await s.Player.GetAsync($"api/character-sheets/{s.SheetId}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await s.Player.GetAsync($"api/campaigns/{s.CampaignId}/character-sheets/mine")).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
        (await s.Player.GetAsync($"api/character-sheets/{s.SheetId}/journal-entries")).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
        (await s.Player.PostAsJsonAsync($"api/character-sheets/{s.SheetId}/journal-entries",
            new CreateJournalEntryRequest { Text = "sneaky" })).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await s.Player.PutAsJsonAsync($"api/character-sheets/{s.SheetId}",
            new UpdateCharacterSheetRequest { CharacterName = "Hacked", DataJson = "{}" })).StatusCode
            .Should().Be(HttpStatusCode.NotFound);

        // 4. The GM still sees the sheet and the journal — nothing was deleted
        (await s.Gm.GetAsync($"api/character-sheets/{s.SheetId}")).StatusCode.Should().Be(HttpStatusCode.OK);
        var journal = (await (await s.Gm.GetAsync($"api/character-sheets/{s.SheetId}/journal-entries")).Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<JournalEntryResponse>>>())!.Data!;
        journal.Should().ContainSingle(j => j.Text == "Day one.");
        var sheets = (await (await s.Gm.GetAsync($"api/campaigns/{s.CampaignId}/character-sheets")).Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CharacterSheetResponse>>>())!.Data!;
        sheets.Should().ContainSingle(x => x.Id == s.SheetId);

        // 5. Re-adding the player brings their access back
        (await s.Gm.PostAsJsonAsync($"api/campaigns/{s.CampaignId}/members",
            new AssignMemberRequest { PlayerId = s.PlayerId })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await s.Player.GetAsync($"api/character-sheets/{s.SheetId}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Remove_WithWrongPassword_Returns400AndKeepsTheMember()
    {
        var s = await SetupCampaignWithMemberAsync();

        var response = await RemoveAsync(s, "WrongPass9");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var members = (await (await s.Gm.GetAsync($"api/campaigns/{s.CampaignId}/members")).Content
            .ReadFromJsonAsync<ApiResponse<IEnumerable<CampaignMemberResponse>>>())!.Data!;
        members.Should().Contain(m => m.PlayerId == s.PlayerId);
        (await s.Player.GetAsync($"api/character-sheets/{s.SheetId}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Remove_WithEmptyPassword_Returns400()
    {
        var s = await SetupCampaignWithMemberAsync();

        (await RemoveAsync(s, "")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Remove_ByAnotherGameMaster_Returns404EvenWithTheirOwnCorrectPassword()
    {
        var s = await SetupCampaignWithMemberAsync();
        var otherGm = factory.CreateClient();
        var other = await AuthHelper.RegisterGameMasterAsync(otherGm, Faker.Internet.Email(), "OtherPass1");
        AuthHelper.SetBearerToken(otherGm, other.AccessToken);

        var response = await otherGm.PostAsJsonAsync($"api/campaigns/{s.CampaignId}/members/{s.PlayerId}/remove",
            new RemoveMemberRequest { Password = "OtherPass1" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await s.Player.GetAsync($"api/character-sheets/{s.SheetId}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Remove_WhenPlayerIsNotAMember_Returns404()
    {
        var s = await SetupCampaignWithMemberAsync();

        var response = await s.Gm.PostAsJsonAsync($"api/campaigns/{s.CampaignId}/members/{Guid.NewGuid()}/remove",
            new RemoveMemberRequest { Password = GmPassword });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Remove_AsPlayer_Returns403()
    {
        var s = await SetupCampaignWithMemberAsync();

        var response = await s.Player.PostAsJsonAsync($"api/campaigns/{s.CampaignId}/members/{s.PlayerId}/remove",
            new RemoveMemberRequest { Password = "TestPass1" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
