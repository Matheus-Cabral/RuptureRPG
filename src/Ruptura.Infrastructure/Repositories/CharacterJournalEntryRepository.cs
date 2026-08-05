using Microsoft.EntityFrameworkCore;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Infrastructure.Data;

namespace Ruptura.Infrastructure.Repositories;

public class CharacterJournalEntryRepository(AppDbContext db)
    : BaseRepository<CharacterJournalEntry>(db), ICharacterJournalEntryRepository
{
    public async Task<IEnumerable<CharacterJournalEntry>> GetByCharacterSheetAsync(
        Guid characterSheetId, CancellationToken ct = default) =>
        await Set
            .Where(e => e.CharacterSheetId == characterSheetId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
}
