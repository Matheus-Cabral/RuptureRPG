using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Controllers;

public class CharacterSheetFlowTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    [Fact]
    public async Task FullFlow_GrantEditModulesMarkDeadGrantReplacement_Succeeds()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "E2E Campaign" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        // 1. Register a player via invite and assign to the campaign.
        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        var playerId = player.User.Id;
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = playerId });

        // 2. Grant a character.
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Sir Aldric" });
        grantResponse.EnsureSuccessStatusCode();
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        sheet.DerivedStats.Np.Should().Be(0); // no attributes above 1, no skills/talents/equipment yet

        // 3. Read a real Skill from the official catalog and invest points in it.
        var skillsResponse = await client.GetAsync($"api/catalog?type=Skill&campaignId={campaign.Id}");
        var skill = (await skillsResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<Ruptura.Shared.Catalog.CatalogEntryResponse>>>())!
            .Data!.First(s => s.Name == "Espadas");

        sheet.Data.Attributes.Controle = 3;
        sheet.Data.Skills.Add(new CharacterSkillEntry { CatalogEntryId = skill.Id, Points = 25 });

        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName,
            DataJson = System.Text.Json.JsonSerializer.Serialize(sheet.Data)
        });
        updateResponse.EnsureSuccessStatusCode();
        var updated = (await updateResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        updated.DerivedStats.Np.Should().BeGreaterThan(0);
        updated.DerivedStats.SkillGradeBonuses[skill.Id].Should().Be(1); // 25 points → Adepto → +1

        // 4. GM marks the character dead.
        var killResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = updated.CharacterName,
            DataJson = System.Text.Json.JsonSerializer.Serialize(updated.Data),
            IsDead = true
        });
        killResponse.EnsureSuccessStatusCode();

        // 5. GM grants a replacement character for the same player — succeeds now that
        //    the first one is dead (the unique-alive index no longer blocks it).
        var replacementResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Dame Lysbet" });
        replacementResponse.EnsureSuccessStatusCode();

        // 6. GM's campaign detail sheet list shows both.
        var listResponse = await client.GetAsync($"api/campaigns/{campaign.Id}/character-sheets");
        var list = (await listResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CharacterSheetResponse>>>())!.Data!.ToList();
        list.Should().HaveCount(2);
        list.Should().Contain(s => s.CharacterName == "Sir Aldric" && s.IsDead);
        list.Should().Contain(s => s.CharacterName == "Dame Lysbet" && !s.IsDead);
    }

    [Fact]
    public async Task FullFlow_GrantHomebrewWeaponEquipAndLinkInvestedSkill_ProducesExpectedWeaponRow()
    {
        // The feature's headline value path — a homebrew weapon, equipped and linked to an
        // invested Skill, producing a correct computed attack/damage row — had zero direct
        // coverage before this test. It would have caught Findings 1a and 2 directly.
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync("api/campaigns", new CreateCampaignRequest { Name = "Weapon Flow Campaign" });
        var campaign = (await campaignResponse.Content.ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        var inviteResponse = await client.PostAsync("api/invites", null);
        var inviteCode = (await inviteResponse.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
        var player = await AuthHelper.RegisterPlayerAsync(client, inviteCode, Faker.Internet.Email());
        var playerId = player.User.Id;
        await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/members", new AssignMemberRequest { PlayerId = playerId });

        // 1. GM grants a character.
        var grantResponse = await client.PostAsJsonAsync($"api/campaigns/{campaign.Id}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = playerId, CharacterName = "Weaponsmith" });
        grantResponse.EnsureSuccessStatusCode();
        var sheet = (await grantResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        // 2. GM creates a homebrew EquipmentItem weapon via the raw-DataJson catalog admin endpoint.
        var weaponResponse = await client.PostAsJsonAsync("api/catalog", new Ruptura.Shared.Catalog.CreateCatalogEntryRequest
        {
            CampaignId = campaign.Id, Type = "EquipmentItem", Name = "Machado Rúnico",
            DataJson = """
                {"Category":"arma","Rarity":"Incomum","AttackBonus":0,"DamageBonus":2,
                 "DefenseBonus":0,"WeaponDiceCategory":"Pesada","ArmorDamageReduction":null,"Weight":4.5}
                """
        });
        weaponResponse.EnsureSuccessStatusCode();
        var weapon = (await weaponResponse.Content.ReadFromJsonAsync<ApiResponse<Ruptura.Shared.Catalog.CatalogEntryResponse>>())!.Data!;

        // 3. Player invests points in a real Skill, equips the homebrew weapon and links it
        //    to that skill.
        AuthHelper.SetBearerToken(client, player.AccessToken);
        var skillsResponse = await client.GetAsync($"api/catalog?type=Skill&campaignId={campaign.Id}");
        var skill = (await skillsResponse.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<Ruptura.Shared.Catalog.CatalogEntryResponse>>>())!
            .Data!.First(s => s.Name == "Espadas");

        sheet.Data.Attributes.Controle = 4; // modifier +2, grade bonus +3
        sheet.Data.Skills.Add(new CharacterSkillEntry { CatalogEntryId = skill.Id, Points = 50 }); // grade +2
        sheet.Data.Equipment.Add(new CharacterEquipmentEntry
        {
            CatalogEntryId = weapon.Id, Quantity = 1, IsEquipped = true, LinkedSkillEntryId = skill.Id
        });

        var updateResponse = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName,
            DataJson = System.Text.Json.JsonSerializer.Serialize(sheet.Data)
        });
        updateResponse.EnsureSuccessStatusCode();

        // 4. GET the sheet and assert the derived weapon row.
        var getResponse = await client.GetAsync($"api/character-sheets/{sheet.Id}");
        getResponse.EnsureSuccessStatusCode();
        var reread = (await getResponse.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        var row = reread.DerivedStats.Weapons.Should().ContainSingle(w => w.CatalogEntryId == weapon.Id).Subject;
        row.AttackBonus.Should().Be(3 + 2); // attribute grade bonus (Controle 4 → 3) + skill grade bonus (50 pts → +2)
        row.DamageFormula.Should().Be("1d10 +6"); // Pesada dice + (attr modifier +2 + skill grade +2 + item damageBonus +2)
    }
}
