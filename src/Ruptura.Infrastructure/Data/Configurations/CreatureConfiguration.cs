using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data.Seed;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CreatureConfiguration : IEntityTypeConfiguration<Creature>
{
    public void Configure(EntityTypeBuilder<Creature> builder)
    {
        // Library reads filter by owner (own entries + official owner-null examples).
        builder.HasIndex(c => c.GameMasterId);

        // The 10 GDD §9.5.10 base creatures as official examples (GameMasterId = null).
        builder.HasData(BestiarySeedData.Creatures);
    }
}
