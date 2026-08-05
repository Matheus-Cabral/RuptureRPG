using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CharacterSheetConfiguration : IEntityTypeConfiguration<CharacterSheet>
{
    public void Configure(EntityTypeBuilder<CharacterSheet> builder)
    {
        // "1 personagem vivo/não aposentado por jogador por Campaign" — application-level
        // check happens in CharacterSheetService; this is the concurrency safety net.
        builder.HasIndex(c => new { c.OwnerId, c.CampaignId })
            .IsUnique()
            .HasFilter("NOT \"IsDead\" AND NOT \"IsRetired\"")
            .HasDatabaseName("ux_character_sheets_owner_campaign_alive");
    }
}
