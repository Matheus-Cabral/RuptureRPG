using Ruptura.Shared.Notifications;

namespace Ruptura.Shared.Campaigns;

public class CampaignDashboardResponse
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public DungeonStateDto Dungeon { get; set; } = new();
    public List<PartyMemberDto> Party { get; set; } = [];
    public GuildSnapshotDto? Guild { get; set; }              // null when no guild exists yet
    public List<NotificationResponse> PendingNotifications { get; set; } = [];
}

public class DungeonStateDto
{
    public int CurrentFloor { get; set; }
    public string FloorName { get; set; } = string.Empty;
    public string FloorState { get; set; } = string.Empty;
    public int Pressure { get; set; }
    public string PressureStateKey { get; set; } = string.Empty; // derived
    public decimal PeMultiplier { get; set; }                    // derived

    // GM-5 content tree: soft pointer to the campaign's current Floor entity, resolved to
    // name/objective at read time. Null when unset or unresolved (Task 4 wires these up).
    public Guid? CurrentFloorId { get; set; }
    public string? CurrentFloorName { get; set; }
    public string? CurrentFloorObjective { get; set; }
}

public class PartyMemberDto
{
    public Guid Id { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string Ranking { get; set; } = string.Empty;
    public int Np { get; set; }
    public int CurrentHp { get; set; }
    public int MaxHp { get; set; }
}

public class GuildSnapshotDto
{
    public string Stage { get; set; } = string.Empty;
    public int Cg { get; set; }
    public int FloorsConquered { get; set; }
    public int Silver { get; set; }
    public int PactCoins { get; set; }
}
