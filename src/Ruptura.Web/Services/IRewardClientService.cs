using Ruptura.Shared.Common;
using Ruptura.Shared.Rewards;

namespace Ruptura.Web.Services;

public interface IRewardClientService
{
    Task<ApiResponse<IEnumerable<RewardResponse>>?> GetAllAsync(Guid campaignId);
    Task<ApiResponse<RewardResponse>?> GetByIdAsync(Guid campaignId, Guid id);
    Task<ApiResponse<RewardResponse>?> CreateAsync(Guid campaignId, CreateRewardRequest request);
    Task<ApiResponse<RewardResponse>?> UpdateAsync(Guid campaignId, Guid id, UpdateRewardRequest request);
    Task<ApiResponse?> DeleteAsync(Guid campaignId, Guid id);
}
