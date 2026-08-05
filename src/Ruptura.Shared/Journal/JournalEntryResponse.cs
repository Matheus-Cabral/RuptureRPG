namespace Ruptura.Shared.Journal;

public class JournalEntryResponse
{
    public Guid Id { get; set; }
    public Guid CharacterSheetId { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> ImagePaths { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
