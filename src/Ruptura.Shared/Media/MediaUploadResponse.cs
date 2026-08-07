namespace Ruptura.Shared.Media;

public class MediaUploadResponse
{
    public string Path { get; set; } = string.Empty;

    // Populated only for GuildEmblem uploads: the guild row's new xmin after the version-
    // checkpointed emblem write, so the client can refresh its token without re-GETting.
    public uint? Version { get; set; }
}
