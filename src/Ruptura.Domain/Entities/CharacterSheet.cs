namespace Ruptura.Domain.Entities;

public class CharacterSheet
{
    public Guid Id { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid GrantedByGameMasterId { get; set; }
    public bool IsDead { get; set; }
    public bool IsRetired { get; set; }
    public string? PortraitImagePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Everything else (attributes, skills, talents, spells, techniques,
    // equipment, currency, attribute trial, guild registry) lives here as
    // JSON — see Ruptura.Shared.CharacterSheets.CharacterSheetData.
    public string DataJson { get; set; } = "{}";
}
