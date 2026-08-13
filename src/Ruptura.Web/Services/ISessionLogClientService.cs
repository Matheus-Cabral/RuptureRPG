using Ruptura.Shared.Common;
using Ruptura.Shared.Content;

namespace Ruptura.Web.Services;

public interface ISessionLogClientService
{
    Task<ApiResponse<IEnumerable<SessionLogResponse>>?> GetAllAsync(Guid campaignId);
    Task<ApiResponse<SessionLogResponse>?> GetByIdAsync(Guid campaignId, Guid id);
    Task<ApiResponse<SessionLogResponse>?> CreateAsync(Guid campaignId, CreateSessionLogRequest request);
    Task<ApiResponse<SessionLogResponse>?> UpdateAsync(Guid campaignId, Guid id, UpdateSessionLogRequest request);
    Task<ApiResponse?> DeleteAsync(Guid campaignId, Guid id);
}
