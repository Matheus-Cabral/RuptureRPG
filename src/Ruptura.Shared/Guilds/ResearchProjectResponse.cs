namespace Ruptura.Shared.Guilds;

public class ResearchProjectResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResearchType { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty; // Menor|Moderada|Maior|Suprema
    public string Stage { get; set; } = string.Empty;      // Descobrir|Pesquisar|Dominar|Aplicar
    public int ProgressDays { get; set; }
    public int RequiredDays { get; set; }                  // server-derived from Complexity
    public int Researchers { get; set; }
    public int Points { get; set; }
    public bool IsComplete { get; set; }
}
