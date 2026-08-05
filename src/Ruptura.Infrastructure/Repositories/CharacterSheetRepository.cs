using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CharacterSheetRepository(AppDbContext db)
    : BaseRepository<CharacterSheet>(db), ICharacterSheetRepository
{
    public async Task<IEnumerable<CharacterSheet>> GetByCampaignAsync(
        Guid campaignId, CancellationToken ct = default) =>
        await Set
            .Where(c => c.CampaignId == campaignId)
            .OrderBy(c => c.CharacterName)
            .ToListAsync(ct);

    public async Task<CharacterSheet?> GetAliveByOwnerAndCampaignAsync(
        Guid ownerId, Guid campaignId, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(
            c => c.OwnerId == ownerId && c.CampaignId == campaignId && !c.IsDead && !c.IsRetired, ct);
}
