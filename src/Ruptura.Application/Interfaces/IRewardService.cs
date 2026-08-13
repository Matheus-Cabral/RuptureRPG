using Ruptura.Application.Common;
using Ruptura.Shared.Rewards;

namespace Ruptura.Application.Interfaces;

// Reward planner CRUD (GM-3). Every method is scoped to a campaign the caller GM owns; a
// non-owned/missing campaign or reward yields Reward.NotFound (existence hidden). The reward
// package is persisted as a typed blob: VE and resource ints are clamped, strategic-asset
// categories are validated against RewardReference, and an optional EncounterId is validated
// against the same campaign — its name resolved at read time.
public interface IRewardService
{
    Task<Result<IEnumerable<RewardResponse>>> GetForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);

    Task<Result<RewardResponse>> GetByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default);

    Task<Result<RewardResponse>> CreateAsync(
        Guid gameMasterId, Guid campaignId, CreateRewardRequest request, CancellationToken ct = default);

    Task<Result<RewardResponse>> UpdateAsync(
        Guid gameMasterId, Guid campaignId, Guid id, UpdateRewardRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default);
}
