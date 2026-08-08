namespace Ruptura.Shared.Guilds;

public class GuildSheetResponse
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string GuildName { get; set; } = string.Empty;
    public GuildSheetData Data { get; set; } = new();
    public GuildDerivedStats DerivedStats { get; set; } = new();
    public List<ExpeditionResponse> Expeditions { get; set; } = [];
    public List<GuildBuildingResponse> Buildings { get; set; } = [];
    public List<GuildStaffResponse> Staff { get; set; } = [];
    public List<ResearchProjectResponse> Research { get; set; } = [];
    public List<CraftingOrderResponse> Crafting { get; set; } = [];
    public uint Version { get; set; }                  // xmin concurrency token (sub-plan #3 requires it on write)
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
