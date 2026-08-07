using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;
using Ruptura.IntegrationTests.Helpers;

namespace Ruptura.IntegrationTests.Guilds;

public class GuildSheetRepositoryTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    [Fact]
    public async Task GetByCampaignAsync_ReturnsTheCampaignsGuild_OrNull()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IGuildSheetRepository>();

        var campaign = new Campaign { Id = Guid.NewGuid(), Name = "Repo Test", GameMasterId = Guid.NewGuid() };
        db.Campaigns.Add(campaign);
        db.GuildSheets.Add(new GuildSheet { Id = Guid.NewGuid(), CampaignId = campaign.Id, GuildName = "Repo Guild", CreatedByGameMasterId = campaign.GameMasterId });
        await db.SaveChangesAsync();

        (await repo.GetByCampaignAsync(campaign.Id)).Should().NotBeNull();
        (await repo.GetByCampaignAsync(Guid.NewGuid())).Should().BeNull();
    }
}
