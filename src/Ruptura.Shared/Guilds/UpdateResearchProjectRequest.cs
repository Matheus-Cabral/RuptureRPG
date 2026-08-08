using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Guilds;

public class UpdateResearchProjectRequest
{
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(60)]
    public string ResearchType { get; set; } = string.Empty;
    [Required]
    public string Complexity { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public int ProgressDays { get; set; }
    public int Researchers { get; set; }
    public int Points { get; set; }
    public bool IsComplete { get; set; }
}
