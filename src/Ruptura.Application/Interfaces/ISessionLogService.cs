using Ruptura.Application.Common;
using Ruptura.Shared.Content;

namespace Ruptura.Application.Interfaces;

// Session log CRUD (GM-5): dated prep notes persisted as a typed blob scoped to one campaign. Every
// method is campaign-ownership authoritative — a non-owned/missing campaign or a foreign session
// yields Session.NotFound (existence hidden). Title is required; the list is ordered by Date
// DESCENDING (most recent first).
public interface ISessionLogService
{
    Task<Result<IEnumerable<SessionLogResponse>>> GetForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default);

    Task<Result<SessionLogResponse>> GetByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid sessionId, CancellationToken ct = default);

    Task<Result<SessionLogResponse>> CreateAsync(
        Guid gameMasterId, Guid campaignId, CreateSessionLogRequest request, CancellationToken ct = default);

    Task<Result<SessionLogResponse>> UpdateAsync(
        Guid gameMasterId, Guid campaignId, Guid sessionId, UpdateSessionLogRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(
        Guid gameMasterId, Guid campaignId, Guid sessionId, CancellationToken ct = default);
}
