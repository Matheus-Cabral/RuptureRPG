using Ruptura.Shared.Common;
using Ruptura.Shared.Encounters;

namespace Ruptura.Web.Services;

public interface IEncounterClientService
{
    Task<ApiResponse<IEnumerable<EncounterResponse>>?> GetAllAsync(Guid campaignId);
    Task<ApiResponse<EncounterResponse>?> GetByIdAsync(Guid campaignId, Guid id);
    Task<ApiResponse<EncounterResponse>?> CreateAsync(Guid campaignId, CreateEncounterRequest request);
    Task<ApiResponse<EncounterResponse>?> UpdateAsync(Guid campaignId, Guid id, UpdateEncounterRequest request);
    Task<ApiResponse?> DeleteAsync(Guid campaignId, Guid id);
}
