using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public interface ICharacterSheetClientService
{
    Task<ApiResponse<CharacterSheetResponse>?> GrantAsync(Guid campaignId, GrantCharacterSheetRequest request);
    Task<ApiResponse<IEnumerable<CharacterSheetResponse>>?> GetByCampaignAsync(Guid campaignId);
    Task<ApiResponse<CharacterSheetResponse>?> GetMineAsync(Guid campaignId);
    Task<ApiResponse<CharacterSheetResponse>?> GetAsync(Guid id);
    Task<ApiResponse<CharacterSheetResponse>?> UpdateAsync(Guid id, UpdateCharacterSheetRequest request);
}
