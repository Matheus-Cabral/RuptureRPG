using Ruptura.Shared.Bestiary;
using Ruptura.Shared.Common;

namespace Ruptura.Web.Services;

public interface IBestiaryClientService
{
    Task<ApiResponse<IEnumerable<CreatureResponse>>?> GetCreaturesAsync();
    Task<ApiResponse<CreatureResponse>?> GetCreatureAsync(Guid id);
    Task<ApiResponse<CreatureResponse>?> CreateCreatureAsync(CreateCreatureRequest request);
    Task<ApiResponse<CreatureResponse>?> UpdateCreatureAsync(Guid id, UpdateCreatureRequest request);
    Task<ApiResponse?> DeleteCreatureAsync(Guid id);
}
