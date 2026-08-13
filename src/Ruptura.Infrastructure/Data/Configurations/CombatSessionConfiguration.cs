using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CombatSessionConfiguration : IEntityTypeConfiguration<CombatSession>
{
    public void Configure(EntityTypeBuilder<CombatSession> builder)
    {
        // Reads always filter by campaign.
        builder.HasIndex(e => e.CampaignId);

        // Fresh table — gets a real FK. Combat sessions belong to exactly one Campaign; if
        // the Campaign is deleted, its sessions go with it.
        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
