using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;
using Ruptura.IntegrationTests.Helpers;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildRepositoryReadTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task GetByGuildAsync_ReturnsOnlyRowsForTheGivenGuild()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var buildingRepo = scope.ServiceProvider.GetRequiredService<IGuildBuildingRepository>();
        var staffRepo = scope.ServiceProvider.GetRequiredService<IGuildStaffRepository>();

        // Arrange: Create 2 campaigns (one guild per campaign constraint)
        var gmId = Guid.NewGuid();
        var campaign1 = new Campaign { Id = Guid.NewGuid(), Name = "Test Campaign 1", GameMasterId = gmId };
        var campaign2 = new Campaign { Id = Guid.NewGuid(), Name = "Test Campaign 2", GameMasterId = gmId };
        db.Campaigns.AddRange(campaign1, campaign2);

        var guildSheet = new GuildSheet
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign1.Id,
            GuildName = "Primary Guild",
            CreatedByGameMasterId = gmId
        };
        db.GuildSheets.Add(guildSheet);

        // Create a second guild in another campaign to verify isolation
        var otherGuild = new GuildSheet
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign2.Id,
            GuildName = "Other Guild",
            CreatedByGameMasterId = gmId
        };
        db.GuildSheets.Add(otherGuild);

        // Add 2 buildings for primary guild + 1 for other guild
        var building1 = new GuildBuilding
        {
            Id = Guid.NewGuid(),
            GuildSheetId = guildSheet.Id,
            CatalogEntryId = Guid.NewGuid(),
            Level = 1,
            IsActive = true
        };
        var building2 = new GuildBuilding
        {
            Id = Guid.NewGuid(),
            GuildSheetId = guildSheet.Id,
            CatalogEntryId = Guid.NewGuid(),
            Level = 2,
            IsActive = true
        };
        var otherBuilding = new GuildBuilding
        {
            Id = Guid.NewGuid(),
            GuildSheetId = otherGuild.Id,
            CatalogEntryId = Guid.NewGuid(),
            Level = 1,
            IsActive = true
        };
        db.GuildBuildings.AddRange(building1, building2, otherBuilding);

        // Add 2 staff for primary guild + 1 for other guild
        var staff1 = new GuildStaff
        {
            Id = Guid.NewGuid(),
            GuildSheetId = guildSheet.Id,
            Kind = Domain.Enums.GuildStaffKind.Worker,
            TypeOrRanking = "Builder",
            Name = "Alice",
            DailySalary = 100,
            IsActive = true
        };
        var staff2 = new GuildStaff
        {
            Id = Guid.NewGuid(),
            GuildSheetId = guildSheet.Id,
            Kind = Domain.Enums.GuildStaffKind.Worker,
            TypeOrRanking = "Guard",
            Name = "Bob",
            DailySalary = 150,
            IsActive = true
        };
        var otherStaff = new GuildStaff
        {
            Id = Guid.NewGuid(),
            GuildSheetId = otherGuild.Id,
            Kind = Domain.Enums.GuildStaffKind.Worker,
            TypeOrRanking = "Scout",
            Name = "Charlie",
            DailySalary = 200,
            IsActive = true
        };
        db.GuildStaff.AddRange(staff1, staff2, otherStaff);

        await db.SaveChangesAsync();

        // Act
        var buildings = await buildingRepo.GetByGuildAsync(guildSheet.Id);
        var staff = await staffRepo.GetByGuildAsync(guildSheet.Id);

        // Assert: Only 2 buildings and 2 staff for the primary guild
        buildings.Should().HaveCount(2);
        buildings.Should().Contain(b => b.Id == building1.Id);
        buildings.Should().Contain(b => b.Id == building2.Id);
        buildings.Should().NotContain(b => b.Id == otherBuilding.Id);

        staff.Should().HaveCount(2);
        staff.Should().Contain(s => s.Id == staff1.Id);
        staff.Should().Contain(s => s.Id == staff2.Id);
        staff.Should().NotContain(s => s.Id == otherStaff.Id);
    }
}
