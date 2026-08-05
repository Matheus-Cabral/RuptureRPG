namespace Ruptura.Shared.CharacterSheets;

public class CharacterSheetResponse
{
    public Guid Id { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid GrantedByGameMasterId { get; set; }
    public bool IsDead { get; set; }
    public bool IsRetired { get; set; }
    public string? PortraitImagePath { get; set; }
    public CharacterSheetData Data { get; set; } = new();
    public CharacterDerivedStats DerivedStats { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
