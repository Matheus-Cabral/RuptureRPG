using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasIndex(n => new { n.RecipientUserId, n.IsRead });
        builder.HasIndex(n => n.RelatedCharacterSheetId);

        // Fresh table — gets real FKs unlike the rest of the schema's soft-reference
        // convention (see this plan's Global Constraints). CampaignId cascades (a deleted
        // campaign's notifications go with it, same as CatalogEntry.CampaignId).
        // RelatedCharacterSheetId is nullable and sets null on delete instead — losing the
        // sheet shouldn't erase notification history, just the dangling reference.
        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(n => n.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<CharacterSheet>()
            .WithMany()
            .HasForeignKey(n => n.RelatedCharacterSheetId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
