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

    Task<ApiResponse<IEnumerable<NpcResponse>>?> GetNpcsAsync();
    Task<ApiResponse<NpcResponse>?> GetNpcAsync(Guid id);
    Task<ApiResponse<NpcResponse>?> CreateNpcAsync(CreateNpcRequest request);
    Task<ApiResponse<NpcResponse>?> UpdateNpcAsync(Guid id, UpdateNpcRequest request);
    Task<ApiResponse?> DeleteNpcAsync(Guid id);
}
