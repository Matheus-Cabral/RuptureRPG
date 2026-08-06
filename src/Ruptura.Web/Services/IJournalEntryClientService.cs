using Ruptura.Shared.Common;
using Ruptura.Shared.Journal;

namespace Ruptura.Web.Services;

public interface IJournalEntryClientService
{
    Task<ApiResponse<IEnumerable<JournalEntryResponse>>?> GetByCharacterSheetAsync(Guid characterSheetId);
    Task<ApiResponse<JournalEntryResponse>?> CreateAsync(Guid characterSheetId, CreateJournalEntryRequest request);
    Task<ApiResponse<JournalEntryResponse>?> UpdateAsync(Guid characterSheetId, Guid entryId, UpdateJournalEntryRequest request);
    Task<ApiResponse?> DeleteAsync(Guid characterSheetId, Guid entryId);
}
