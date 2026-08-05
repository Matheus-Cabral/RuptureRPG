using Ruptura.Domain.Entities;
using Ruptura.Domain.Interfaces;

namespace Ruptura.Application.Interfaces;

public interface ICharacterJournalEntryRepository : IRepository<CharacterJournalEntry>
{
    Task<IEnumerable<CharacterJournalEntry>> GetByCharacterSheetAsync(
        Guid characterSheetId, CancellationToken ct = default);
}
