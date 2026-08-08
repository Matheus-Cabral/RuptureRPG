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
    Task<ApiResponse<GuildBuildingResponse>?> AddBuildingAsync(Guid campaignId, CreateBuildingRequest request);
    Task<ApiResponse<GuildBuildingResponse>?> UpdateBuildingAsync(Guid campaignId, Guid buildingId, UpdateBuildingRequest request);
    Task<ApiResponse?> DeleteBuildingAsync(Guid campaignId, Guid buildingId);
    Task<ApiResponse<GuildStaffResponse>?> AddStaffAsync(Guid campaignId, CreateStaffRequest request);
    Task<ApiResponse<GuildStaffResponse>?> UpdateStaffAsync(Guid campaignId, Guid staffId, UpdateStaffRequest request);
    Task<ApiResponse?> DeleteStaffAsync(Guid campaignId, Guid staffId);
    Task<ApiResponse<ResearchProjectResponse>?> AddResearchAsync(Guid campaignId, CreateResearchProjectRequest request);
    Task<ApiResponse<ResearchProjectResponse>?> UpdateResearchAsync(Guid campaignId, Guid researchId, UpdateResearchProjectRequest request);
    Task<ApiResponse?> DeleteResearchAsync(Guid campaignId, Guid researchId);
    Task<ApiResponse<CraftingOrderResponse>?> AddCraftingAsync(Guid campaignId, CreateCraftingOrderRequest request);
    Task<ApiResponse<CraftingOrderResponse>?> UpdateCraftingAsync(Guid campaignId, Guid craftingId, UpdateCraftingOrderRequest request);
    Task<ApiResponse?> DeleteCraftingAsync(Guid campaignId, Guid craftingId);
    Task<ApiResponse<InterludeProjection>?> PreviewInterludeAsync(Guid campaignId, int days);
    Task<GuildSaveResult> ApplyInterludeAsync(Guid campaignId, ApplyInterludeRequest request);
}
