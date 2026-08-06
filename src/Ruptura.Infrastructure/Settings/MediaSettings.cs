namespace Ruptura.Infrastructure.Settings;

public class MediaSettings
{
    public string RootPath { get; set; } = "/app/media";

    // 0 = unlimited. Should never be configured above Ruptura.Shared.Media.MediaLimits
    // .ClientMaxUploadBytes (100 MB) — that's the hard transport-level ceiling
    // (Kestrel's MaxRequestBodySize, set in Program.cs) above which a request never
    // even reaches this check.
    public int MaxFileSizeMb { get; set; } = 5;
    public int MaxImagesPerJournalEntry { get; set; } = 6;  // 0 = unlimited
}
