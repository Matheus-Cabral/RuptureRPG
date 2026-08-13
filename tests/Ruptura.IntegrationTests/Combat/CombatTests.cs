using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Bestiary;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Combat;
using Ruptura.Shared.Common;
using Ruptura.Shared.Encounters;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Combat;

// End-to-end coverage for the in-session combat tracker (GM-4). The tracker is persisted as a typed
// CombatState blob scoped to one campaign; the service is campaign-ownership authoritative (a
// non-owned/missing campaign or a foreign session yields Combat.NotFound — existence hidden).
// StartFromEncounter is server-authoritative: it expands the encounter's creatures ×quantity from the
// bestiary PV and imports the alive party from each sheet's DerivedStats.MaxHp/Data.Combat.CurrentHp.
// The whole-state PUT clamps PV, validates conditions against CombatReference, and recomputes IsDefeated.
public class CombatTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, string GmToken, CampaignResponse Campaign)> SetupGmWithCampaignAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Combat Campaign" });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        return (client, gm.AccessToken, campaign);
    }

    private static async Task<string> NewInviteCodeAsync(HttpClient client)
    {
        var invite = await client.PostAsync("api/invites", null);
        return (await invite.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
    }

    // Creates an alive party member with a known CurrentHp so the imported character combatant's PV is
    // hand-verifiable. Returns the full sheet (MaxHp lives in DerivedStats, CurrentHp in Data.Combat).
    private async Task<CharacterSheetResponse> SeedAlivePartyMemberAsync(
        HttpClient client, string gmToken, Guid campaignId, int currentHp, string name = "Hero")
    {
        var code = await NewInviteCodeAsync(client);
        var player = await AuthHelper.RegisterPlayerAsync(client, code, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gmToken);
        await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var grant = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = name });
        grant.EnsureSuccessStatusCode();
        var sheet = (await grant.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        // Bump Vigor (drives MaxHp = 10 + Vigor*2 + rankingBonus) and Percepcao, set CurrentHp.
        var data = sheet.Data;
        data.Attributes.Vigor = 3;
        data.Attributes.Percepcao = 4;
        data.Combat.CurrentHp = currentHp;

        var update = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName,
            DataJson = JsonSerializer.Serialize(data)
        });
        update.EnsureSuccessStatusCode();
        return (await update.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
    }

    private static async Task<Guid> CreateCreatureAsync(HttpClient client, int pv, string name = "Goblin")
    {
        var resp = await client.PostAsJsonAsync("api/bestiary/creatures", new CreateCreatureRequest
        {
            Name = name,
            Data = new CreatureData
            {
                Type = "homebrew",
                Function = "Predador",
                Behavior = "Instintiva",
                Category = "Comum",
                Characteristics = [new CreatureCharacteristic { Name = "Casca", Weight = "Media" }],
                Abilities = [new CreatureAbility { Name = "Mordida", Tier = "Comum" }],
                Equipment = [new CreatureEquipment { Name = "Adaga", Rarity = "Raro" }],
                Fraqueza = "Fogo",
                Pv = pv,
                DefesaPassiva = 12,
                Deslocamento = 9
            }
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resp.Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>())!.Data!.Id;
    }

    private static async Task<Guid> CreateEncounterAsync(
        HttpClient client, Guid campaignId, Guid creatureId, int quantity, string name = "Ambush")
    {
        var resp = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/encounters",
            new CreateEncounterRequest
            {
                Name = name,
                Data = new EncounterData
                {
                    Creatures = [new EncounterCreature { CreatureId = creatureId, Quantity = quantity }],
                    Intelligence = "Instinto",
                    Terrain = "Neutro",
                    Objective = "Eliminar",
                    DesiredDifficulty = "Normal",
                    Duration = "Curto",
                    ApplyPressure = false
                }
            });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resp.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!.Id;
    }

    [Fact]
    public async Task Create_EmptySession_Returns201()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat",
            new CreateCombatSessionRequest { Name = "Skirmish" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>())!.Data!;
        body.Id.Should().NotBeEmpty();
        body.Name.Should().Be("Skirmish");
        body.State.Round.Should().Be(1);
        body.State.CurrentIndex.Should().Be(0);
        body.State.Combatants.Should().BeEmpty();
        body.PressureStateKey.Should().Be("Estavel");
    }

    [Fact]
    public async Task StartFromEncounter_ExpandsCreaturesAndImportsAliveParty()
    {
        var (client, gmToken, campaign) = await SetupGmWithCampaignAsync();
        var sheet = await SeedAlivePartyMemberAsync(client, gmToken, campaign.Id, currentHp: 9);
        var creatureId = await CreateCreatureAsync(client, pv: 20);
        var encounterId = await CreateEncounterAsync(client, campaign.Id, creatureId, quantity: 3);

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat/start-from-encounter",
            new StartFromEncounterRequest { Name = "Boss Fight", EncounterId = encounterId });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>())!.Data!;

        var creatures = body.State.Combatants.Where(c => c.Kind == "Creature").ToList();
        creatures.Should().HaveCount(3);
        creatures.Should().OnlyContain(c => c.MaxPv == 20 && c.CurrentPv == 20 && !c.IsDefeated);
        creatures.Select(c => c.Name).Should().BeEquivalentTo("Goblin #1", "Goblin #2", "Goblin #3");

        var characters = body.State.Combatants.Where(c => c.Kind == "Character").ToList();
        characters.Should().ContainSingle();
        var hero = characters.Single();
        hero.Name.Should().Be("Hero");
        hero.SourceId.Should().Be(sheet.Id);
        hero.MaxPv.Should().Be(sheet.DerivedStats.MaxHp);
        hero.CurrentPv.Should().Be(9);
        hero.Percepcao.Should().Be(sheet.Data.Attributes.Percepcao);

        body.State.Combatants.Should().HaveCount(4);
        body.State.Round.Should().Be(1);
    }

    [Fact]
    public async Task UpdateState_ClampsPvAndRecomputesDefeated()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat",
            new CreateCombatSessionRequest { Name = "Session" });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>())!.Data!;

        var state = new CombatState
        {
            Round = 2,
            CurrentIndex = 0,
            Combatants =
            [
                new Combatant { Name = "Over", Kind = "Adhoc", MaxPv = 10, CurrentPv = 999, Initiative = 5 },
                new Combatant { Name = "Under", Kind = "Adhoc", MaxPv = 10, CurrentPv = -5, Initiative = 3 }
            ]
        };

        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat/{created.Id}",
            new UpdateCombatStateRequest { Name = "Session", IsActive = true, State = state });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>())!.Data!;

        var over = body.State.Combatants.Single(c => c.Name == "Over");
        over.CurrentPv.Should().Be(10);
        over.IsDefeated.Should().BeFalse();

        var under = body.State.Combatants.Single(c => c.Name == "Under");
        under.CurrentPv.Should().Be(0);
        under.IsDefeated.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateState_WithUnknownCondition_Returns400ConditionInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat",
            new CreateCombatSessionRequest { Name = "Session" });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>())!.Data!;

        var state = new CombatState
        {
            Combatants =
            [
                new Combatant { Name = "Bad", Kind = "Adhoc", MaxPv = 10, CurrentPv = 10, Conditions = ["Nonsense"] }
            ]
        };

        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat/{created.Id}",
            new UpdateCombatStateRequest { Name = "Session", State = state });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Success.Should().BeFalse();
        body.Message.Should().Be("Combat.ConditionInvalid");
    }

    [Fact]
    public async Task UpdateState_PersistsInitiativeOrder_AndCursorFollowsCombatantIdentity()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat",
            new CreateCombatSessionRequest { Name = "Session" });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>())!.Data!;

        // Roster is deliberately NOT in initiative order; the cursor points at the low-initiative
        // combatant ("Slow") which sorts to the end.
        var slowId = Guid.NewGuid();
        var midId = Guid.NewGuid();
        var fastId = Guid.NewGuid();
        var state = new CombatState
        {
            Round = 3,
            CurrentIndex = 0,
            Combatants =
            [
                new Combatant { Id = slowId, Name = "Slow", Kind = "Adhoc", MaxPv = 10, CurrentPv = 10, Initiative = 1 },
                new Combatant { Id = midId, Name = "Mid", Kind = "Adhoc", MaxPv = 10, CurrentPv = 10, Initiative = 10 },
                new Combatant { Id = fastId, Name = "Fast", Kind = "Adhoc", MaxPv = 10, CurrentPv = 10, Initiative = 20 }
            ]
        };

        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat/{created.Id}",
            new UpdateCombatStateRequest { Name = "Session", State = state });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>())!.Data!;

        // Persisted/response roster is initiative-descending: Fast, Mid, Slow.
        body.State.Combatants.Select(c => c.Name).Should().ContainInOrder("Fast", "Mid", "Slow");
        // The cursor still points at the same combatant Id it did before the sort ("Slow", now last).
        body.State.Combatants[body.State.CurrentIndex].Id.Should().Be(slowId);
        body.State.Round.Should().Be(3);
    }

    [Fact]
    public async Task UpdateState_WithNullState_Returns400StateRequired()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat",
            new CreateCombatSessionRequest { Name = "Session" });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>())!.Data!;

        var response = await client.PutAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat/{created.Id}",
            new UpdateCombatStateRequest { Name = "Session", State = null! });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Success.Should().BeFalse();
        body.Message.Should().Be("Combat.StateRequired");
    }

    [Fact]
    public async Task GetById_ByADifferentGameMaster_Returns404()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat",
            new CreateCombatSessionRequest { Name = "Secret" });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<CombatSessionResponse>>())!.Data!;

        var (otherClient, otherToken, _) = await SetupGmWithCampaignAsync();
        AuthHelper.SetBearerToken(otherClient, otherToken);

        var response = await otherClient.GetAsync($"api/campaigns/{campaign.Id}/combat/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StartFromEncounter_WithEncounterFromAnotherCampaign_Returns400EncounterInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        // Same GM owns a second campaign with its own encounter.
        var otherCampaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Other Campaign" });
        var otherCampaign = (await otherCampaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;
        var creatureId = await CreateCreatureAsync(client, pv: 12);
        var foreignEncounterId = await CreateEncounterAsync(client, otherCampaign.Id, creatureId, quantity: 1);

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/combat/start-from-encounter",
            new StartFromEncounterRequest { Name = "Wrong Encounter", EncounterId = foreignEncounterId });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Success.Should().BeFalse();
        body.Message.Should().Be("Combat.EncounterInvalid");
    }
}
