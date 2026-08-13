using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        // Reads always filter by campaign.
        builder.HasIndex(e => e.CampaignId);

        // Fresh table — gets a real FK. Rewards belong to exactly one Campaign; if
        // the Campaign is deleted, its rewards go with it.
        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
