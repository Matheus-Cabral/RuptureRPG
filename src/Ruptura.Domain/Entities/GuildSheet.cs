namespace Ruptura.Domain.Entities;

public class GuildSheet
{
    public Guid Id { get; set; }
    public string GuildName { get; set; } = string.Empty;
    public Guid CreatedByGameMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Sheet data — populated in later iterations
    public string DataJson { get; set; } = "{}";

    public ICollection<GuildMembership> Memberships { get; set; } = [];
}
