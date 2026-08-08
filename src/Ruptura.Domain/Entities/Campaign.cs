namespace Ruptura.Domain.Entities;

public class Campaign
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid GameMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int CurrentFloor { get; set; } = 1;
    public string FloorName { get; set; } = string.Empty;
    public string FloorState { get; set; } = "Inexplorado"; // Inexplorado|Explorado|Conquistado|Dominado
    public int Pressure { get; set; }                        // 0..100 (§4.2)
}
