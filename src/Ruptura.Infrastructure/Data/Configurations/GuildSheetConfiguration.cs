using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class GuildSheetConfiguration : IEntityTypeConfiguration<GuildSheet>
{
    public void Configure(EntityTypeBuilder<GuildSheet> builder)
    {
        // 1 guild per campaign — enforced at the DB level (unlike CharacterSheet.CampaignId,
        // which is a soft reference; here the 1:1 invariant must hold).
        builder.HasIndex(g => g.CampaignId)
            .IsUnique()
            .HasDatabaseName("ux_guild_sheets_campaign");

        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(g => g.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optimistic concurrency for the blob under shared write.
        // Maps PostgreSQL's xmin system column to a round-trippable CLR token so
        // the read DTO can return it and the write can require it (guards the
        // cross-request stale write, not just the in-request window).
        builder.Property(g => g.Version)
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
