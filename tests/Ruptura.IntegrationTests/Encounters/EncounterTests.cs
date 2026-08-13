using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Bestiary;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using Ruptura.Shared.Encounters;
using Ruptura.Shared.Invites;

namespace Ruptura.IntegrationTests.Encounters;

// End-to-end coverage for the encounter generator (GM-2, Task 3). All threat math is
// server-authoritative: the client never sends Pe/R — the service resolves party NP from
// the campaign's alive party, creature NP from the bestiary, Pressão from the campaign, and
// runs EncounterCalculator. Expected values are hand-computed from the seeded creature NP and
// the created character's DerivedStats.Np using EncounterReference (the single source of truth).
public class EncounterTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, string GmToken, CampaignResponse Campaign)> SetupGmWithCampaignAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);

        var campaignResponse = await client.PostAsJsonAsync(
            "api/campaigns", new CreateCampaignRequest { Name = "Encounter Campaign" });
        var campaign = (await campaignResponse.Content
            .ReadFromJsonAsync<ApiResponse<CampaignResponse>>())!.Data!;

        return (client, gm.AccessToken, campaign);
    }

    private static async Task<string> NewInviteCodeAsync(HttpClient client)
    {
        var invite = await client.PostAsync("api/invites", null);
        return (await invite.Content.ReadFromJsonAsync<ApiResponse<InviteCodeResponse>>())!.Data!.Code;
    }

    // Creates an alive party member and bumps its attributes so its NP is non-zero and known
    // (default sheets have NP 0). Returns the character's server-computed NP.
    private async Task<int> SeedAlivePartyMemberAsync(HttpClient client, string gmToken, Guid campaignId)
    {
        var code = await NewInviteCodeAsync(client);
        var player = await AuthHelper.RegisterPlayerAsync(client, code, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gmToken);
        await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/members", new AssignMemberRequest { PlayerId = player.User.Id });

        var grant = await client.PostAsJsonAsync(
            $"api/campaigns/{campaignId}/character-sheets",
            new GrantCharacterSheetRequest { PlayerId = player.User.Id, CharacterName = "Hero" });
        grant.EnsureSuccessStatusCode();
        var sheet = (await grant.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;

        // Bump every attribute to 2 → each contributes (2-1)=1 to NP (8 attributes → NP 8).
        var data = sheet.Data;
        data.Attributes.Corpo = 2; data.Attributes.Controle = 2; data.Attributes.Vigor = 2;
        data.Attributes.Presenca = 2; data.Attributes.Intelecto = 2; data.Attributes.Percepcao = 2;
        data.Attributes.Vontade = 2; data.Attributes.Afinidade = 2;

        var update = await client.PutAsJsonAsync($"api/character-sheets/{sheet.Id}", new UpdateCharacterSheetRequest
        {
            CharacterName = sheet.CharacterName,
            DataJson = JsonSerializer.Serialize(data)
        });
        update.EnsureSuccessStatusCode();
        var updated = (await update.Content.ReadFromJsonAsync<ApiResponse<CharacterSheetResponse>>())!.Data!;
        updated.DerivedStats.Np.Should().BeGreaterThan(0);
        return updated.DerivedStats.Np;
    }

    // A homebrew creature with a hand-verifiable NP of 15 (see CreatureTests): attributes all 1
    // (Σ = 0) + Media characteristic (3) + Comum ability (5) + Raro equipment (7).
    private static CreateCreatureRequest CreatureRequest(string name = "Goblin") => new()
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
            Pv = 20,
            DefesaPassiva = 12,
            Deslocamento = 9
        }
    };

    private static async Task<(Guid Id, int Np)> CreateCreatureAsync(HttpClient client, string name = "Goblin")
    {
        var resp = await client.PostAsJsonAsync("api/bestiary/creatures", CreatureRequest(name));
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await resp.Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>())!.Data!;
        return (body.Id, body.DerivedNp);
    }

    private static EncounterData BaseData(Guid creatureId, int quantity = 1) => new()
    {
        Creatures = [new EncounterCreature { CreatureId = creatureId, Quantity = quantity }],
        Intelligence = "Instinto",
        Terrain = "Neutro",
        Objective = "Eliminar",
        DesiredDifficulty = "Normal",
        Duration = "Curto",
        ApplyPressure = false
    };

    [Fact]
    public async Task Create_AsCampaignGameMaster_Returns201()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var (creatureId, _) = await CreateCreatureAsync(client);

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/encounters",
            new CreateEncounterRequest { Name = "Ambush", Data = BaseData(creatureId) });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!;
        body.Id.Should().NotBeEmpty();
        body.Name.Should().Be("Ambush");
        body.CampaignId.Should().Be(campaign.Id);
        body.Creatures.Should().ContainSingle(c => c.CreatureId == creatureId);
    }

    [Fact]
    public async Task Get_ComputesThreatFromAlivePartyAndCreature()
    {
        var (client, gmToken, campaign) = await SetupGmWithCampaignAsync();
        var charNp = await SeedAlivePartyMemberAsync(client, gmToken, campaign.Id);
        var (creatureId, creatureNp) = await CreateCreatureAsync(client);
        creatureNp.Should().Be(15);

        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/encounters",
            new CreateEncounterRequest { Name = "Skirmish", Data = BaseData(creatureId) });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!;

        var getResp = await client.GetAsync($"api/campaigns/{campaign.Id}/encounters/{created.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = (await getResp.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!;

        // ---- Hand-computed (§9.8/§9.9) ----
        // PG = Σ NP(party) × Synergy(1) = charNp × 1.0
        var expectedPg = charNp;
        // PE = 15 × QuantityMult(1)=1 × Instinto=1 × Neutro=1 × Eliminar=1 × Pressure(off)=1
        var expectedPe = 15;
        var expectedR = (decimal)expectedPe / expectedPg;
        // OA = PG × Normal(1.0) × Curto(1)
        var expectedOa = expectedPg;
        // FCE band: default Ranking "Bronze" → "BronzeFerro" → 0.40
        var expectedFce = 0.40m;
        var expectedRsm = 1m + (expectedR - 1m) * expectedFce;

        body.PartyResolved.Should().BeTrue();
        body.Pg.Should().Be(expectedPg);
        body.Pe.Should().Be(expectedPe);
        body.R.Should().Be(expectedR);
        body.RLabel.Should().Be(EncounterReference.RLabelFor(expectedR));
        body.Oa.Should().Be(expectedOa);
        body.Fce.Should().Be(expectedFce);
        body.RealStatMultiplier.Should().Be(expectedRsm);
        body.PressureApplied.Should().BeFalse();

        var resolved = body.Creatures.Single();
        resolved.CreatureId.Should().Be(creatureId);
        resolved.Np.Should().Be(15);
        resolved.Quantity.Should().Be(1);
    }

    [Fact]
    public async Task Get_ByADifferentGameMaster_Returns404()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var (creatureId, _) = await CreateCreatureAsync(client);
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/encounters",
            new CreateEncounterRequest { Name = "Secret", Data = BaseData(creatureId) });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!;

        var (otherClient, otherToken, _) = await SetupGmWithCampaignAsync();
        AuthHelper.SetBearerToken(otherClient, otherToken);

        var response = await otherClient.GetAsync($"api/campaigns/{campaign.Id}/encounters/{created.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithUnknownTerrain_Returns400TerrainInvalid()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var (creatureId, _) = await CreateCreatureAsync(client);
        var data = BaseData(creatureId);
        data.Terrain = "Nonsense";

        var response = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/encounters",
            new CreateEncounterRequest { Name = "Bad Terrain", Data = data });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse>();
        body!.Success.Should().BeFalse();
        body.Message.Should().Be("Encounter.TerrainInvalid");
    }

    [Fact]
    public async Task Create_WithQuantityAbove9999_ClampsTo9999OnReadBack()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();
        var (creatureId, _) = await CreateCreatureAsync(client);

        var data = BaseData(creatureId, quantity: 100_000);
        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/encounters",
            new CreateEncounterRequest { Name = "Swarm", Data = data });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!;
        created.Creatures.Single().Quantity.Should().Be(9999);

        // Read-back confirms the clamp is persisted, not just applied on the create response.
        var getResp = await client.GetAsync($"api/campaigns/{campaign.Id}/encounters/{created.Id}");
        var body = (await getResp.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!;
        body.Creatures.Single().Quantity.Should().Be(9999);
    }

    [Fact]
    public async Task Create_WithMoreThan200CreatureLines_PersistsOnly200()
    {
        var (client, _, campaign) = await SetupGmWithCampaignAsync();

        // 250 distinct lines (unknown creature IDs are allowed — they resolve to Np 0, still listed).
        var data = BaseData(Guid.NewGuid());
        data.Creatures = Enumerable.Range(0, 250)
            .Select(_ => new EncounterCreature { CreatureId = Guid.NewGuid(), Quantity = 1 })
            .ToList();

        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/encounters",
            new CreateEncounterRequest { Name = "Horde", Data = data });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!;
        created.Creatures.Should().HaveCount(200);

        var getResp = await client.GetAsync($"api/campaigns/{campaign.Id}/encounters/{created.Id}");
        var body = (await getResp.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!;
        body.Creatures.Should().HaveCount(200);
    }

    [Fact]
    public async Task Get_PartyNpOverride_OverridesAutoPg()
    {
        var (client, gmToken, campaign) = await SetupGmWithCampaignAsync();
        var charNp = await SeedAlivePartyMemberAsync(client, gmToken, campaign.Id);
        var (creatureId, _) = await CreateCreatureAsync(client);

        var overrideNp = charNp + 1000;   // distinct from the auto party sum
        var data = BaseData(creatureId);
        data.PartyNpOverride = overrideNp;

        var create = await client.PostAsJsonAsync(
            $"api/campaigns/{campaign.Id}/encounters",
            new CreateEncounterRequest { Name = "Override", Data = data });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!;

        var getResp = await client.GetAsync($"api/campaigns/{campaign.Id}/encounters/{created.Id}");
        var body = (await getResp.Content.ReadFromJsonAsync<ApiResponse<EncounterResponse>>())!.Data!;

        // Override wins: PG = overrideNp × Synergy(1) = overrideNp, NOT the auto charNp.
        body.Pg.Should().Be(overrideNp);
        body.Pg.Should().NotBe(charNp);
        body.PartyNpOverride.Should().Be(overrideNp);
        body.PartyResolved.Should().BeTrue();
    }
}
