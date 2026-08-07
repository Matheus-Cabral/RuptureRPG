using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class ResearchProject
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResearchType { get; set; } = string.Empty;  // Arcana|Biológica|Tecnológica|Dimensional|Histórica|Militar
    public ResearchComplexity Complexity { get; set; }
    public ResearchStage Stage { get; set; } = ResearchStage.Descobrir;
    public int ProgressDays { get; set; }
    public int RequiredDays { get; set; }                     // from complexity tier (5/10/20/40)
    public int Researchers { get; set; } = 1;                 // splits time, floor 50% of base
    public int Points { get; set; }                           // awarded to CG's Pesquisa term on completion
    public bool IsComplete { get; set; }
}
