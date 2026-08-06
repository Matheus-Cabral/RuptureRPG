namespace Ruptura.Shared.Media;

/// <summary>
/// Fixed transport-level ceiling for media uploads, shared between the Blazor client
/// (as the InputFile.OpenReadStream maxAllowedSize) and the API host (as Kestrel's
/// MaxRequestBodySize). This is deliberately a generous fixed value, well above any
/// sane configured business limit — the actual business-logic enforcement is
/// MediaSettings.MaxFileSizeMb, checked by MediaController.Upload. Raising this
/// ceiling just ensures requests up to a reasonable size can reach that check at all.
/// </summary>
public static class MediaLimits
{
    public const long ClientMaxUploadBytes = 100L * 1024 * 1024;
}
