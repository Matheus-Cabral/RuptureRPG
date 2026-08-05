namespace Ruptura.Domain.Entities;

public class CharacterJournalEntry
{
    public Guid Id { get; set; }
    public Guid CharacterSheetId { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string> ImagePaths { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
