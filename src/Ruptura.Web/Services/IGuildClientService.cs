using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;

namespace Ruptura.Web.Services;

// Result of a guild save. A 409 (xmin conflict) must be distinguishable from other
// failures so the page can refetch-and-retry instead of just surfacing an error toast.
public record GuildSaveResult(ApiResponse<GuildSheetResponse>? Response, bool IsConflict);

public interface IGuildClientService
{
    Task<ApiResponse<GuildSheetResponse>?> GetGuildAsync(Guid campaignId);
    Task<GuildSaveResult> UpdateGuildAsync(Guid campaignId, UpdateGuildSheetRequest request);
    Task<ApiResponse<ExpeditionResponse>?> AddExpeditionAsync(Guid campaignId, CreateExpeditionRequest request);
    Task<ApiResponse<ExpeditionResponse>?> UpdateExpeditionAsync(Guid campaignId, Guid expeditionId, UpdateExpeditionRequest request);
    Task<ApiResponse?> DeleteExpeditionAsync(Guid campaignId, Guid expeditionId);
}
