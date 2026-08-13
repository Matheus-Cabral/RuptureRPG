using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class FloorConfiguration : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> builder)
    {
        // Reads filter by campaign, and floors are also fetched per-arc.
        builder.HasIndex(e => e.CampaignId);
        builder.HasIndex(e => e.ArcId);

        // Floors belong to exactly one Campaign; deleting the Campaign takes its floors.
        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Floors belong to exactly one Arc; deleting the Arc cascades its floors (DECISION
        // D1). PostgreSQL permits multiple cascade paths to Campaign (Floor→Campaign and
        // Floor→Arc→Campaign), so both FKs stay Cascade — no Restrict fallback needed.
        builder.HasOne<Arc>()
            .WithMany()
            .HasForeignKey(e => e.ArcId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
