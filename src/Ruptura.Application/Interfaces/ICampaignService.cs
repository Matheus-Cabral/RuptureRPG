using Ruptura.Application.Common;
using Ruptura.Shared.Campaigns;

namespace Ruptura.Application.Interfaces;

public interface ICampaignService
{
    Task<Result<CampaignResponse>> CreateAsync(
        Guid gameMasterId, CreateCampaignRequest request, CancellationToken ct = default);

    Task<Result<IEnumerable<CampaignResponse>>> GetByGameMasterAsync(
        Guid gameMasterId, CancellationToken ct = default);

    Task<Result<IEnumerable<PlayerRosterResponse>>> GetRosterAsync(
        Guid gameMasterId, CancellationToken ct = default);

    Task<Result<CampaignMemberResponse>> AssignMemberAsync(
        Guid gameMasterId, Guid campaignId, AssignMemberRequest request, CancellationToken ct = default);

    Task<Result<IEnumerable<CampaignMemberResponse>>> GetMembersAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);

    Task<Result<IEnumerable<CampaignResponse>>> GetMyMembershipsAsync(
        Guid callerId, bool isGameMaster, CancellationToken ct = default);
}
