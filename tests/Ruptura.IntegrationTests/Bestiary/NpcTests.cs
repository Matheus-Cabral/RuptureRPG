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

public class NpcTests(IntegrationTestFactory factory)
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

    private static CreateNpcRequest ValidRequest(string name = "Sereno o Mercador") => new()
    {
        Name = name,
        Data = new NpcData
        {
            Role = "Comerciante",
            Faction = "Guilda dos Mercadores",
            Disposition = "Neutro",
            Location = "Porto Velho",
            Notes = "Vende relíquias raras."
        }
    };

    private static async Task<Guid> SeedOfficialNpcAsync(IntegrationTestFactory factory, string name)
    {
        var npcId = Guid.NewGuid();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Npcs.Add(new Npc
        {
            Id = npcId,
            GameMasterId = null, // official example
            Name = name,
            DataJson = """{"Role":"Patrono","Disposition":"Aliado","Faction":"Coroa"}"""
        });
        await db.SaveChangesAsync();
        return npcId;
    }

    [Fact]
    public async Task Crud_RoundTrip_CreateGetUpdateDelete()
    {
        var (client, _) = await RegisterGmAsync();

        // Create
        var createResp = await client.PostAsJsonAsync("api/bestiary/npcs", ValidRequest());
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await createResp.Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>())!.Data!;
        created.Id.Should().NotBeEmpty();
        created.IsOfficial.Should().BeFalse();
        created.Name.Should().Be("Sereno o Mercador");

        // Get by id
        var getResp = await client.GetAsync($"api/bestiary/npcs/{created.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = (await getResp.Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>())!.Data!;
        fetched.Id.Should().Be(created.Id);

        // Update
        var updateResp = await client.PutAsJsonAsync($"api/bestiary/npcs/{created.Id}", new UpdateNpcRequest
        {
            Name = "Sereno o Aposentado",
            Data = new NpcData { Role = "Contato", Disposition = "Aliado", Location = "Vila Nova" }
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await updateResp.Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>())!.Data!;
        updated.Name.Should().Be("Sereno o Aposentado");
        updated.Data.Role.Should().Be("Contato");
        updated.Data.Location.Should().Be("Vila Nova");

        // Delete
        var deleteResp = await client.DeleteAsync($"api/bestiary/npcs/{created.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Gone
        var goneResp = await client.GetAsync($"api/bestiary/npcs/{created.Id}");
        goneResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_PersistsAllFields_RoundTrip()
    {
        var (client, _) = await RegisterGmAsync();

        var created = (await (await client.PostAsJsonAsync("api/bestiary/npcs", ValidRequest("Field Test")))
            .Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>())!.Data!;

        var fetched = (await (await client.GetAsync($"api/bestiary/npcs/{created.Id}"))
            .Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>())!.Data!;

        fetched.Data.Role.Should().Be("Comerciante");
        fetched.Data.Faction.Should().Be("Guilda dos Mercadores");
        fetched.Data.Disposition.Should().Be("Neutro");
        fetched.Data.Location.Should().Be("Porto Velho");
        fetched.Data.Notes.Should().Be("Vende relíquias raras.");
    }

    [Fact]
    public async Task Get_ReturnsOwnAndOfficial_ButNotOtherGmHomebrew()
    {
        var (clientA, _) = await RegisterGmAsync();
        var mine = (await (await clientA.PostAsJsonAsync("api/bestiary/npcs", ValidRequest("MyNpc")))
            .Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>())!.Data!;

        var officialId = await SeedOfficialNpcAsync(factory, "Official Patron");

        var (clientB, _) = await RegisterGmAsync();
        var theirs = (await (await clientB.PostAsJsonAsync("api/bestiary/npcs", ValidRequest("TheirNpc")))
            .Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>())!.Data!;

        var list = (await (await clientA.GetAsync("api/bestiary/npcs"))
            .Content.ReadFromJsonAsync<ApiResponse<IEnumerable<NpcResponse>>>())!.Data!.ToList();

        list.Should().Contain(n => n.Id == mine.Id);
        list.Should().Contain(n => n.Id == officialId && n.IsOfficial);
        list.Should().NotContain(n => n.Id == theirs.Id);
    }

    [Fact]
    public async Task GetById_OtherGmHomebrew_Returns404()
    {
        var (clientB, _) = await RegisterGmAsync();
        var theirs = (await (await clientB.PostAsJsonAsync("api/bestiary/npcs", ValidRequest("Hidden")))
            .Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>())!.Data!;

        var (clientA, _) = await RegisterGmAsync();
        var response = await clientA.GetAsync($"api/bestiary/npcs/{theirs.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_OfficialNpc_Returns403()
    {
        var (client, _) = await RegisterGmAsync();
        var officialId = await SeedOfficialNpcAsync(factory, "Official Broker");

        var response = await client.PutAsJsonAsync(
            $"api/bestiary/npcs/{officialId}", new UpdateNpcRequest
            {
                Name = "Hacked",
                Data = ValidRequest().Data
            });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_OfficialNpc_Returns403()
    {
        var (client, _) = await RegisterGmAsync();
        var officialId = await SeedOfficialNpcAsync(factory, "Official Envoy");

        var response = await client.DeleteAsync($"api/bestiary/npcs/{officialId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_OtherGmHomebrew_Returns404()
    {
        var (clientB, _) = await RegisterGmAsync();
        var theirs = (await (await clientB.PostAsJsonAsync("api/bestiary/npcs", ValidRequest("Yours")))
            .Content.ReadFromJsonAsync<ApiResponse<NpcResponse>>())!.Data!;

        var (clientA, _) = await RegisterGmAsync();
        var response = await clientA.PutAsJsonAsync(
            $"api/bestiary/npcs/{theirs.Id}", new UpdateNpcRequest { Name = "x", Data = ValidRequest().Data });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
