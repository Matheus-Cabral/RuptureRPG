using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data.Seed;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CatalogEntryConfiguration : IEntityTypeConfiguration<CatalogEntry>
{
    public void Configure(EntityTypeBuilder<CatalogEntry> builder)
    {
        // Global (official) entries: unique by (Type, Name) among CampaignId IS NULL rows.
        builder.HasIndex(c => new { c.Type, c.Name })
            .IsUnique()
            .HasFilter("\"CampaignId\" IS NULL")
            .HasDatabaseName("ux_catalog_entries_global_type_name");

        // Homebrew entries: unique by (Type, CampaignId, Name) among CampaignId IS NOT NULL rows.
        builder.HasIndex(c => new { c.Type, c.CampaignId, c.Name })
            .IsUnique()
            .HasFilter("\"CampaignId\" IS NOT NULL")
            .HasDatabaseName("ux_catalog_entries_scoped_type_campaign_name");

        builder.HasData(CatalogSeedData.Origins);
        builder.HasData(CatalogSeedData.Backgrounds);
        builder.HasData(CatalogSeedData.Lineages);
        builder.HasData(CatalogSeedData.Aptitudes);
        builder.HasData(CatalogSeedData.Talents);
    }
}
