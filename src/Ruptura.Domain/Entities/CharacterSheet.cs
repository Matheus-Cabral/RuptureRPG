namespace Ruptura.Domain.Entities;

public class CharacterSheet
{
    public Guid Id { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public Guid GrantedByGameMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Sheet data — populated in later iterations
    public string DataJson { get; set; } = "{}";
}
