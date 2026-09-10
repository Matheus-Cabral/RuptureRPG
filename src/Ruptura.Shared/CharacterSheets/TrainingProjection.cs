namespace Ruptura.Shared.CharacterSheets;

// Server-computed preview of GDD §6.4's Pontos de Treinamento/dia formula. The client shows
// this and, on Aplicar, mutates Data.Skills directly (design spec §5.3) — this DTO is
// display-only, never posted back.
public class TrainingProjection
{
    public Guid SkillCatalogEntryId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
    public double PointsPerDay { get; set; }
    public int Days { get; set; }
    public string Correlation { get; set; } = string.Empty;
    public int PointsToAdd { get; set; }
    public int ProjectedTotalPoints { get; set; }
}
