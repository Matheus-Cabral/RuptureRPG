using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Journal;

public class CreateJournalEntryRequest
{
    [Required, MinLength(1), MaxLength(10000)]
    public string Text { get; set; } = string.Empty;
}
