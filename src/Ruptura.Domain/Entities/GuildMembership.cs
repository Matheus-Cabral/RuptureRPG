namespace Ruptura.Domain.Entities;

public class GuildMembership
{
    public Guid Id { get; set; }
    public Guid GuildSheetId { get; set; }
    public GuildSheet GuildSheet { get; set; } = null!;
    public Guid PlayerId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
