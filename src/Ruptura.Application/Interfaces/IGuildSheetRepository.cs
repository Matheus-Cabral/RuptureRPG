using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface IGuildSheetRepository : IRepository<GuildSheet>
{
    Task<GuildSheet?> GetByCampaignAsync(Guid campaignId, CancellationToken ct = default);

    // Detach a tracked entity so a failed INSERT is not re-attempted by a later SaveChanges in the same scope.
    void Detach(GuildSheet entity);

    // Set the xmin OriginalValue so EF emits UPDATE ... WHERE "Id" = @id AND xmin = @expectedVersion,
    // turning a concurrent write (advanced xmin) into a DbUpdateConcurrencyException (0 rows matched).
    void SetExpectedVersion(GuildSheet guild, uint expectedVersion);
}
