namespace Ruptura.Shared.Campaigns;

public class CampaignDashboardResponse
{
    public Guid CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public DungeonStateDto Dungeon { get; set; } = new();
    public List<PartyMemberDto> Party { get; set; } = [];
    public GuildSnapshotDto? Guild { get; set; }              // null when no guild exists yet
    public List<PendingNotificationDto> PendingNotifications { get; set; } = [];
}

public class DungeonStateDto
{
    public int CurrentFloor { get; set; }
    public string FloorName { get; set; } = string.Empty;
    public string FloorState { get; set; } = string.Empty;
    public int Pressure { get; set; }
    public string PressureStateKey { get; set; } = string.Empty; // derived
    public decimal PeMultiplier { get; set; }                    // derived
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

public class PendingNotificationDto
{
    public Guid Id { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
