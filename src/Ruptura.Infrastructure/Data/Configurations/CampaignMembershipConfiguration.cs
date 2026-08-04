using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ruptura.Domain.Entities;

namespace Ruptura.Infrastructure.Data.Configurations;

public class CampaignMembershipConfiguration : IEntityTypeConfiguration<CampaignMembership>
{
    public void Configure(EntityTypeBuilder<CampaignMembership> builder)
    {
        builder.HasIndex(m => new { m.CampaignId, m.PlayerId }).IsUnique();
    }
}
