using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class CreateResearchProjectRequest
{
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(60)]
    public string ResearchType { get; set; } = string.Empty;
    [Required]
    public string Complexity { get; set; } = string.Empty; // -> RequiredDays derived server-side
    public string Stage { get; set; } = "Descobrir";
    public int ProgressDays { get; set; }
    public int Researchers { get; set; } = 1;
    public int Points { get; set; }                        // client pre-fills by complexity, overridable
    public bool IsComplete { get; set; }
}
