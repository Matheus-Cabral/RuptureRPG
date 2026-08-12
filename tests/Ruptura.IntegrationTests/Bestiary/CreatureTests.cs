using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Bestiary;
using Ruptura.Shared.Common;

namespace Ruptura.IntegrationTests.Bestiary;

public class CreatureTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private static readonly Faker Faker = new();

    private async Task<(HttpClient Client, string Token)> RegisterGmAsync()
    {
        var client = factory.CreateClient();
        var gm = await AuthHelper.RegisterGameMasterAsync(client, Faker.Internet.Email());
        AuthHelper.SetBearerToken(client, gm.AccessToken);
        return (client, gm.AccessToken);
    }

    // A valid creature whose NP terms are easy to hand-verify:
    // attributes all 1 (Σ score-1 = 0) + one Media characteristic (3) + one Comum ability (5)
    // + one Raro equipment (7) = NP 15. Category "Comum" advises 40..70.
    private static CreateCreatureRequest ValidRequest(string name = "Goblin") => new()
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

    private static async Task<Guid> SeedOfficialCreatureAsync(IntegrationTestFactory factory, string name)
    {
        var creatureId = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Creatures.Add(new Creature
        {
            Id = creatureId,
            GameMasterId = null, // official example
            Name = name,
            DataJson = """{"Behavior":"Instintiva","Category":"Comum","Fraqueza":"Luz"}"""
        });
        await db.SaveChangesAsync();
        return creatureId;
    }

    [Fact]
    public async Task Create_AsGameMaster_Returns201WithServerComputedDerivedNp()
    {
        var (client, _) = await RegisterGmAsync();

        var response = await client.PostAsJsonAsync("api/bestiary/creatures", ValidRequest());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = (await response.Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>())!.Data!;
        body.Id.Should().NotBeEmpty();
        body.IsOfficial.Should().BeFalse();
        body.DerivedNp.Should().Be(15);        // 0 + 3 + 5 + 7
        body.CategoryNpMin.Should().Be(40);
        body.CategoryNpMax.Should().Be(70);
        body.CategoryOverflow.Should().BeFalse();
    }

    [Fact]
    public async Task Get_ReturnsOwnAndOfficial_ButNotOtherGmHomebrew()
    {
        var (clientA, _) = await RegisterGmAsync();
        var mine = (await (await clientA.PostAsJsonAsync("api/bestiary/creatures", ValidRequest("MyBeast")))
            .Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>())!.Data!;

        var officialId = await SeedOfficialCreatureAsync(factory, "Official Wyrm");

        // A second GM with their own homebrew creature.
        var (clientB, _) = await RegisterGmAsync();
        var theirs = (await (await clientB.PostAsJsonAsync("api/bestiary/creatures", ValidRequest("TheirBeast")))
            .Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>())!.Data!;

        var list = (await (await clientA.GetAsync("api/bestiary/creatures"))
            .Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CreatureResponse>>>())!.Data!.ToList();

        list.Should().Contain(c => c.Id == mine.Id);
        list.Should().Contain(c => c.Id == officialId && c.IsOfficial);
        list.Should().NotContain(c => c.Id == theirs.Id);
    }

    // The 10 GDD §9.5.10 base creatures seeded as official examples (BestiarySeedData).
    // Guids are deterministic (c0000000-…-0001 … -0010).
    private static readonly string[] SeedCreatureIds =
    [
        "c0000000-0000-0000-0000-000000000001",
        "c0000000-0000-0000-0000-000000000002",
        "c0000000-0000-0000-0000-000000000003",
        "c0000000-0000-0000-0000-000000000004",
        "c0000000-0000-0000-0000-000000000005",
        "c0000000-0000-0000-0000-000000000006",
        "c0000000-0000-0000-0000-000000000007",
        "c0000000-0000-0000-0000-000000000008",
        "c0000000-0000-0000-0000-000000000009",
        "c0000000-0000-0000-0000-000000000010",
    ];

    [Fact]
    public async Task Get_FreshGm_SeesAll10OfficialSeedCreatures_EachWithDerivedNpInCategoryRange()
    {
        var (client, _) = await RegisterGmAsync();

        var list = (await (await client.GetAsync("api/bestiary/creatures"))
            .Content.ReadFromJsonAsync<ApiResponse<IEnumerable<CreatureResponse>>>())!.Data!.ToList();

        foreach (var id in SeedCreatureIds)
        {
            var guid = Guid.Parse(id);
            var creature = list.SingleOrDefault(c => c.Id == guid);

            creature.Should().NotBeNull($"official seed creature {id} must be visible to a fresh GM");
            creature!.IsOfficial.Should().BeTrue($"seed creature {creature.Name} is a system-owned example");
            creature.Name.Should().NotBeNullOrWhiteSpace();
            creature.Data.Fraqueza.Should().NotBeNullOrWhiteSpace($"{creature.Name} must declare a Fraqueza (§9.5.8)");

            // Acceptance criterion: server-computed NP lands inside the Category's advisory range (§9.5.6).
            creature.DerivedNp.Should().BeGreaterThanOrEqualTo(creature.CategoryNpMin,
                $"{creature.Name} NP {creature.DerivedNp} must be >= min {creature.CategoryNpMin}");
            creature.DerivedNp.Should().BeLessThanOrEqualTo(creature.CategoryNpMax,
                $"{creature.Name} NP {creature.DerivedNp} must be <= max {creature.CategoryNpMax}");
        }
    }

    [Fact]
    public async Task GetById_OtherGmHomebrew_Returns404()
    {
        var (clientB, _) = await RegisterGmAsync();
        var theirs = (await (await clientB.PostAsJsonAsync("api/bestiary/creatures", ValidRequest("Hidden")))
            .Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>())!.Data!;

        var (clientA, _) = await RegisterGmAsync();
        var response = await clientA.GetAsync($"api/bestiary/creatures/{theirs.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_OfficialCreature_Returns403()
    {
        var (client, _) = await RegisterGmAsync();
        var officialId = await SeedOfficialCreatureAsync(factory, "Official Golem");

        var response = await client.PutAsJsonAsync(
            $"api/bestiary/creatures/{officialId}", new UpdateCreatureRequest
            {
                Name = "Hacked",
                Data = ValidRequest().Data
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_OfficialCreature_Returns403()
    {
        var (client, _) = await RegisterGmAsync();
        var officialId = await SeedOfficialCreatureAsync(factory, "Official Lich");

        var response = await client.DeleteAsync($"api/bestiary/creatures/{officialId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_OtherGmHomebrew_Returns404()
    {
        var (clientB, _) = await RegisterGmAsync();
        var theirs = (await (await clientB.PostAsJsonAsync("api/bestiary/creatures", ValidRequest("Yours")))
            .Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>())!.Data!;

        var (clientA, _) = await RegisterGmAsync();
        var response = await clientA.PutAsJsonAsync(
            $"api/bestiary/creatures/{theirs.Id}", new UpdateCreatureRequest { Name = "x", Data = ValidRequest().Data });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithEmptyFraqueza_Returns400FraquezaRequired()
    {
        var (client, _) = await RegisterGmAsync();
        var request = ValidRequest();
        request.Data.Fraqueza = "   ";

        var response = await client.PostAsJsonAsync("api/bestiary/creatures", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        // The error code is surfaced in the response message (localized via SharedResources).
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>();
        body!.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Create_WithUnknownBehavior_Returns400BehaviorInvalid()
    {
        var (client, _) = await RegisterGmAsync();
        var request = ValidRequest();
        request.Data.Behavior = "Nonsense";

        var response = await client.PostAsJsonAsync("api/bestiary/creatures", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>();
        body!.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Create_WithUnknownCategory_Returns400CategoryInvalid()
    {
        var (client, _) = await RegisterGmAsync();
        var request = ValidRequest();
        request.Data.Category = "Nonsense";

        var response = await client.PostAsJsonAsync("api/bestiary/creatures", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<CreatureResponse>>();
        body!.Message.Should().NotBeNullOrWhiteSpace();
    }
}
