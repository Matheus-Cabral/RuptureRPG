using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Journal;

public class UpdateJournalEntryRequest
{
    [Required, MinLength(1), MaxLength(10000)]
    public string Text { get; set; } = string.Empty;

    public List<string> ImagePaths { get; set; } = [];
}
