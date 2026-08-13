using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class ArcConfiguration : IEntityTypeConfiguration<Arc>
{
    public void Configure(EntityTypeBuilder<Arc> builder)
    {
        // Reads always filter by campaign.
        builder.HasIndex(e => e.CampaignId);

        // Fresh table — gets a real FK. Arcs belong to exactly one Campaign; if the
        // Campaign is deleted, its arcs go with it.
        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
