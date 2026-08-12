using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class EncounterConfiguration : IEntityTypeConfiguration<Encounter>
{
    public void Configure(EntityTypeBuilder<Encounter> builder)
    {
        // Reads always filter by campaign.
        builder.HasIndex(e => e.CampaignId);

        // Fresh table — gets a real FK. Encounters belong to exactly one Campaign; if
        // the Campaign is deleted, its encounters go with it.
        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
