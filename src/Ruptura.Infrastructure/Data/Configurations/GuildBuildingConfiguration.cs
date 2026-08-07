using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class GuildBuildingConfiguration : IEntityTypeConfiguration<GuildBuilding>
{
    public void Configure(EntityTypeBuilder<GuildBuilding> builder)
    {
        builder.HasIndex(b => b.GuildSheetId);
        builder.HasOne<GuildSheet>()
            .WithMany()
            .HasForeignKey(b => b.GuildSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
