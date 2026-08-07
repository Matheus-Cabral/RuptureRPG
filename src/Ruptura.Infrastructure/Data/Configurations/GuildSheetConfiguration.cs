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
        // Uses PostgreSQL's xmin system column via a shadow token — no physical
        // RowVersion column, and Postgres updates xmin automatically on every UPDATE.
        builder.UseXminAsConcurrencyToken();
    }
}
