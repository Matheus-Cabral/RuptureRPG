using Ruptura.Shared.Common;
using Ruptura.Shared.Content;

namespace Ruptura.Web.Services;

public interface ICampaignContentClientService
{
    // Arcs
    Task<ApiResponse<IEnumerable<ArcResponse>>?> GetArcsAsync(Guid campaignId);
    Task<ApiResponse<ArcResponse>?> GetArcAsync(Guid campaignId, Guid arcId);
    Task<ApiResponse<ArcResponse>?> CreateArcAsync(Guid campaignId, CreateArcRequest request);
    Task<ApiResponse<ArcResponse>?> UpdateArcAsync(Guid campaignId, Guid arcId, UpdateArcRequest request);
    Task<ApiResponse?> DeleteArcAsync(Guid campaignId, Guid arcId);

    // Floors
    Task<ApiResponse<IEnumerable<FloorResponse>>?> GetFloorsAsync(Guid campaignId);
    Task<ApiResponse<IEnumerable<FloorResponse>>?> GetFloorsForArcAsync(Guid campaignId, Guid arcId);
    Task<ApiResponse<FloorResponse>?> GetFloorAsync(Guid campaignId, Guid floorId);
    Task<ApiResponse<FloorResponse>?> CreateFloorAsync(Guid campaignId, CreateFloorRequest request);
    Task<ApiResponse<FloorResponse>?> UpdateFloorAsync(Guid campaignId, Guid floorId, UpdateFloorRequest request);
    Task<ApiResponse?> DeleteFloorAsync(Guid campaignId, Guid floorId);
}
