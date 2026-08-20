using Ruptura.Domain.Enums;

namespace Ruptura.Domain.Entities;

public class CatalogEntry
{
    public Guid Id { get; set; }
    public CatalogEntryType Type { get; set; }
    public Guid? CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public bool IsArchived { get; set; }
    // Global (CampaignId == null) entries are always visible to players regardless of this flag.
    // Most global types are fully immutable (CannotModifyGlobalEntry), so there's no admin UI path
    // to set this false for them; Spell/Technique are the one exception that IS GM-editable (see
    // CatalogEntryService.UpdateAsync) but that path deliberately ignores the request's IsPublic
    // for global rows — a shared entry going private would mean "invisible to every campaign",
    // not a state this app supports. Only meaningful for campaign-scoped homebrew entries.
    // Defaults true so shipping this column never hides anything that was already visible.
    public bool IsPublic { get; set; } = true;
    public Guid? CreatedByGameMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
