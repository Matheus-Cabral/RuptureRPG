using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Campaigns;

public class UpdateDungeonStateRequest
{
    public int CurrentFloor { get; set; }
    [MaxLength(120)]
    public string FloorName { get; set; } = string.Empty;
    [Required]
    public string FloorState { get; set; } = string.Empty; // must be one of DungeonFloorStates.All
    public int Pressure { get; set; }                      // clamped [0,100] server-side

    // GM-5 content tree: soft pointer to the campaign's current Floor entity. Nullable;
    // validated in the service (no FK constraint).
    public Guid? CurrentFloorId { get; set; }
}
