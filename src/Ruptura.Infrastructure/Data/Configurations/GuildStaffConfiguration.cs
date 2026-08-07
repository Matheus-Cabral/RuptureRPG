using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class GuildStaffConfiguration : IEntityTypeConfiguration<GuildStaff>
{
    public void Configure(EntityTypeBuilder<GuildStaff> builder)
    {
        builder.HasIndex(s => s.GuildSheetId);
        builder.Property(s => s.Kind).HasConversion<string>();
        builder.HasOne<GuildSheet>()
            .WithMany()
            .HasForeignKey(s => s.GuildSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
