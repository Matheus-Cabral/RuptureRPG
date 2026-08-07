using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class ResearchProjectConfiguration : IEntityTypeConfiguration<ResearchProject>
{
    public void Configure(EntityTypeBuilder<ResearchProject> builder)
    {
        builder.HasIndex(p => p.GuildSheetId);
        builder.Property(p => p.Complexity).HasConversion<string>();
        builder.Property(p => p.Stage).HasConversion<string>();
        builder.HasOne<GuildSheet>()
            .WithMany()
            .HasForeignKey(p => p.GuildSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
