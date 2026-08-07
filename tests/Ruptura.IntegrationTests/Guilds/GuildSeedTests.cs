using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Domain.Enums;
using Ruptura.Infrastructure.Data;
using Ruptura.IntegrationTests.Helpers;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildSeedTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task Seed_Has20Installations_AllGlobalWithValidShape()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var installations = await db.CatalogEntries
            .Where(c => c.Type == CatalogEntryType.Installation && c.CampaignId == null)
            .ToListAsync();

        installations.Should().HaveCount(20);
        foreach (var i in installations)
        {
            using var doc = JsonDocument.Parse(i.DataJson);
            doc.RootElement.GetProperty("Category").GetString().Should().NotBeNullOrEmpty();
            doc.RootElement.GetProperty("Weight").GetInt32().Should().BeGreaterThan(0);
            doc.RootElement.GetProperty("LevelCap").GetInt32().Should().BeGreaterThan(0);
        }
        installations.Should().ContainSingle(i => i.Name == "Portão");
    }

    [Fact]
    public async Task Seed_Has8Doctrines_AllGlobalWithBonusText()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var doctrines = await db.CatalogEntries
            .Where(c => c.Type == CatalogEntryType.Doctrine && c.CampaignId == null)
            .ToListAsync();

        doctrines.Should().HaveCount(8);
        doctrines.Select(d => d.Name).Should().Contain(new[] { "Militar", "Logística", "Comercial" });
        foreach (var d in doctrines)
        {
            using var doc = JsonDocument.Parse(d.DataJson);
            doc.RootElement.GetProperty("Bonus").GetString().Should().NotBeNullOrEmpty();
        }
    }
}
