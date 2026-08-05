using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CharacterJournalEntryConfiguration : IEntityTypeConfiguration<CharacterJournalEntry>
{
    public void Configure(EntityTypeBuilder<CharacterJournalEntry> builder)
    {
        // ImagePaths round-trips through a jsonb column. The service layer always
        // reassigns a new List<string> instance on change (never mutates in place),
        // so no ValueComparer is needed for the change tracker to notice edits —
        // see this plan's Global Constraints.
        builder.Property(e => e.ImagePaths)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("jsonb");

        builder.HasIndex(e => e.CharacterSheetId);

        // Fresh table, not a retrofit — gets a real FK unlike the rest of the
        // schema's soft-reference convention (see this plan's Global Constraints).
        builder.HasOne<CharacterSheet>()
            .WithMany()
            .HasForeignKey(e => e.CharacterSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
