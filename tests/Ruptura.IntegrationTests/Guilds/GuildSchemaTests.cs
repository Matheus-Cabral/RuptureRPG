using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;
using Ruptura.IntegrationTests.Helpers;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildSchemaTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task DeletingCampaign_CascadeDeletes_GuildSheetAndChildren()
    {
        // Arrange: create a Campaign, a GuildSheet for it, and one child of each kind.
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var campaign = new Campaign { Id = Guid.NewGuid(), Name = "Cascade Test", GameMasterId = Guid.NewGuid() };
        db.Campaigns.Add(campaign);
        var guild = new GuildSheet { Id = Guid.NewGuid(), CampaignId = campaign.Id, GuildName = "G", CreatedByGameMasterId = campaign.GameMasterId };
        db.GuildSheets.Add(guild);
        db.GuildBuildings.Add(new GuildBuilding { Id = Guid.NewGuid(), GuildSheetId = guild.Id, CatalogEntryId = Guid.NewGuid(), Level = 1 });
        db.Expeditions.Add(new Expedition { Id = Guid.NewGuid(), GuildSheetId = guild.Id });
        await db.SaveChangesAsync();

        // Act: delete the campaign.
        db.Campaigns.Remove(campaign);
        await db.SaveChangesAsync();

        // Assert: guild and its children are gone.
        (await db.GuildSheets.CountAsync(g => g.Id == guild.Id)).Should().Be(0);
        (await db.GuildBuildings.CountAsync(b => b.GuildSheetId == guild.Id)).Should().Be(0);
        (await db.Expeditions.CountAsync(e => e.GuildSheetId == guild.Id)).Should().Be(0);
    }

    [Fact]
    public async Task SecondGuildForSameCampaign_ViolatesUniqueIndex()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var campaign = new Campaign { Id = Guid.NewGuid(), Name = "Unique Test", GameMasterId = Guid.NewGuid() };
        db.Campaigns.Add(campaign);
        db.GuildSheets.Add(new GuildSheet { Id = Guid.NewGuid(), CampaignId = campaign.Id, GuildName = "A", CreatedByGameMasterId = campaign.GameMasterId });
        await db.SaveChangesAsync();

        db.GuildSheets.Add(new GuildSheet { Id = Guid.NewGuid(), CampaignId = campaign.Id, GuildName = "B", CreatedByGameMasterId = campaign.GameMasterId });

        var act = async () => await db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task DuplicateBuildingForSameInstallation_ViolatesUniqueIndex()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var campaign = new Campaign { Id = Guid.NewGuid(), Name = "Dup Building", GameMasterId = Guid.NewGuid() };
        db.Campaigns.Add(campaign);
        var guild = new GuildSheet { Id = Guid.NewGuid(), CampaignId = campaign.Id, GuildName = "G", CreatedByGameMasterId = campaign.GameMasterId };
        db.GuildSheets.Add(guild);
        var installationId = Guid.NewGuid();
        db.GuildBuildings.Add(new GuildBuilding { Id = Guid.NewGuid(), GuildSheetId = guild.Id, CatalogEntryId = installationId, Level = 1 });
        await db.SaveChangesAsync();

        db.GuildBuildings.Add(new GuildBuilding { Id = Guid.NewGuid(), GuildSheetId = guild.Id, CatalogEntryId = installationId, Level = 3 });
        var act = async () => await db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}
