using Ruptura.Shared.Common;
using Ruptura.Shared.Media;

namespace Ruptura.Web.Services;

public interface IMediaClientService
{
    Task<ApiResponse<MediaUploadResponse>?> UploadAsync(Stream content, string fileName, string entityType, Guid entityId);
    Task<string?> GetDataUriAsync(string? path);
}
