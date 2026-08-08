using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        // DB-level defaults so existing rows backfill to valid dungeon state on migration
        // (EF does not translate the entity's C# property initializers into column defaults).
        builder.Property(c => c.CurrentFloor).HasDefaultValue(1);
        builder.Property(c => c.FloorState).HasDefaultValue("Inexplorado");
    }
}
