using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class InviteCodeRepository(AppDbContext db)
    : BaseRepository<InviteCode>(db), IInviteCodeRepository
{
    public async Task<InviteCode?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(c => c.Code == code, ct);

    public async Task<IEnumerable<InviteCode>> GetByGameMasterAsync(
        Guid gameMasterId,
        CancellationToken ct = default) =>
        await Set
            .Where(c => c.CreatedByGameMasterId == gameMasterId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
}
