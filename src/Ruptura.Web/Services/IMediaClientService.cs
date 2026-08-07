using Ruptura.Shared.Common;
using Ruptura.Shared.Media;

namespace Ruptura.Web.Services;

// Result of an emblem upload. A 409 (xmin conflict) must be distinguishable from other
// failures so the page can refetch-and-retry instead of just surfacing an error toast
// (mirrors GuildSaveResult).
public record MediaUploadResult(ApiResponse<MediaUploadResponse>? Response, bool IsConflict);

public interface IMediaClientService
{
    Task<ApiResponse<MediaUploadResponse>?> UploadAsync(Stream content, string fileName, string entityType, Guid entityId);

    // Version-checkpointed emblem upload: passes the caller's expected guild xmin so a concurrent
    // write yields a distinct 409 rather than a silent lost update.
    Task<MediaUploadResult> UploadEmblemAsync(Stream content, string fileName, Guid guildId, uint version);

    Task<string?> GetDataUriAsync(string? path);
}
