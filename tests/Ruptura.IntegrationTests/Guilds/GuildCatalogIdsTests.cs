using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Infrastructure.Data;
using Ruptura.IntegrationTests.Helpers;
using Ruptura.Shared.Guilds;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildCatalogIdsTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task GuildCatalogIds_MatchSeededInstallationsAndDoctrines()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        async Task AssertName(Guid id, string expectedName)
        {
            var e = await db.CatalogEntries.FirstOrDefaultAsync(c => c.Id == id);
            e.Should().NotBeNull($"catalog id {id} must be seeded");
            e!.Name.Should().Be(expectedName);
        }

        await AssertName(GuildCatalogIds.Portao, "Portão");
        await AssertName(GuildCatalogIds.Dormitorio, "Dormitório");
        await AssertName(GuildCatalogIds.Armazem, "Armazém");
        await AssertName(GuildCatalogIds.CampoDeTreinamento, "Campo de Treinamento");
        await AssertName(GuildCatalogIds.Biblioteca, "Biblioteca");
        await AssertName(GuildCatalogIds.Memorial, "Memorial");
        await AssertName(GuildCatalogIds.CentroLogistico, "Centro Logístico");
        await AssertName(GuildCatalogIds.CamaraDoConselho, "Câmara do Conselho");
        await AssertName(GuildCatalogIds.DoctrineLogistica, "Logística");
        await AssertName(GuildCatalogIds.DoctrineComercial, "Comercial");
    }
}
