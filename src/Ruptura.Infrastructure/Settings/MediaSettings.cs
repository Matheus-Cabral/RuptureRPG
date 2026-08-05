namespace Ruptura.Infrastructure.Settings;

public class MediaSettings
{
    public string RootPath { get; set; } = "/app/media";
    public int MaxFileSizeMb { get; set; } = 5;             // 0 = unlimited
    public int MaxImagesPerJournalEntry { get; set; } = 6;  // 0 = unlimited
}
