using Ruptura.Application.Common;
using Ruptura.Shared.Campaigns;

namespace Ruptura.Application.Interfaces;

public interface ICampaignDashboardService
{
    Task<Result<CampaignDashboardResponse>> GetAsync(Guid gameMasterId, Guid campaignId, CancellationToken ct = default);
    Task<Result<CampaignDashboardResponse>> UpdateDungeonAsync(
        Guid gameMasterId, Guid campaignId, UpdateDungeonStateRequest request, CancellationToken ct = default);
}
