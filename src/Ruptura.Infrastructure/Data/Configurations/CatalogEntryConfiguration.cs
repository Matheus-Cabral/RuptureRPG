using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data.Seed;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CatalogEntryConfiguration : IEntityTypeConfiguration<CatalogEntry>
{
    public void Configure(EntityTypeBuilder<CatalogEntry> builder)
    {
        // Explicit column default (not just the CLR property default) — this is what backfills
        // IsPublic for any row the AddColumn migration's ALTER TABLE touches, including real
        // GM-created homebrew entries that predate this column and aren't covered by HasData.
        // Without this, EF scaffolds AddColumn with defaultValue: false (the bool CLR default),
        // which would silently make every already-existing homebrew entry private on upgrade.
        builder.Property(c => c.IsPublic).HasDefaultValue(true);

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

        // Homebrew entries belong to exactly one Campaign; if the Campaign is ever
        // deleted, its homebrew catalog goes with it (CatalogEntry.CampaignId was a
        // bare Guid? before this — decided 2026-08-05, see design spec §4.2).
        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(c => c.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(CatalogSeedData.Origins);
        builder.HasData(CatalogSeedData.Backgrounds);
        builder.HasData(CatalogSeedData.Lineages);
        builder.HasData(CatalogSeedData.Aptitudes);
        builder.HasData(CatalogSeedData.Talents);
        builder.HasData(CatalogSeedData.Skills);
        builder.HasData(CatalogSeedData.Spells);
        builder.HasData(CatalogSeedData.Techniques);
        builder.HasData(CatalogSeedData.Installations);
        builder.HasData(CatalogSeedData.Doctrines);
    }
}
