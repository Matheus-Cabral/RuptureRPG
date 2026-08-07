using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CraftingOrderConfiguration : IEntityTypeConfiguration<CraftingOrder>
{
    public void Configure(EntityTypeBuilder<CraftingOrder> builder)
    {
        builder.HasIndex(o => o.GuildSheetId);
        builder.Property(o => o.Category).HasConversion<string>();
        builder.Property(o => o.Status).HasConversion<string>();
        builder.HasOne<GuildSheet>()
            .WithMany()
            .HasForeignKey(o => o.GuildSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
