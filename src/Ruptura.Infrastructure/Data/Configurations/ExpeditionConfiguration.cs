using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class ExpeditionConfiguration : IEntityTypeConfiguration<Expedition>
{
    public void Configure(EntityTypeBuilder<Expedition> builder)
    {
        builder.HasIndex(e => e.GuildSheetId);
        builder.Property(e => e.Kind).HasConversion<string>();
        builder.HasOne<GuildSheet>()
            .WithMany()
            .HasForeignKey(e => e.GuildSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
