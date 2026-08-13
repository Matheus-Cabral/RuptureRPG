using Ruptura.Shared.Combat;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public interface ICombatClientService
{
    Task<ApiResponse<IEnumerable<CombatSessionResponse>>?> GetAllAsync(Guid campaignId);
    Task<ApiResponse<CombatSessionResponse>?> GetByIdAsync(Guid campaignId, Guid id);
    Task<ApiResponse<CombatSessionResponse>?> CreateAsync(Guid campaignId, CreateCombatSessionRequest request);
    Task<ApiResponse<CombatSessionResponse>?> StartFromEncounterAsync(Guid campaignId, StartFromEncounterRequest request);
    Task<ApiResponse<CombatSessionResponse>?> UpdateStateAsync(Guid campaignId, Guid id, UpdateCombatStateRequest request);
    Task<ApiResponse?> DeleteAsync(Guid campaignId, Guid id);
}
