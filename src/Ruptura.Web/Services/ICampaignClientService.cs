using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public interface ICampaignClientService
{
    Task<ApiResponse<IEnumerable<PlayerRosterResponse>>?> GetRosterAsync();
    Task<ApiResponse<CampaignResponse>?> CreateAsync(CreateCampaignRequest request);
    Task<ApiResponse<IEnumerable<CampaignResponse>>?> GetAllAsync();
    Task<ApiResponse<IEnumerable<CampaignMemberResponse>>?> GetMembersAsync(Guid campaignId);
    Task<ApiResponse<CampaignMemberResponse>?> AssignMemberAsync(Guid campaignId, AssignMemberRequest request);
}
