namespace Ruptura.Domain.Entities;

// A session log/prep note scoped to one Campaign — a dated entry the GM keeps per play
// session. The typed fields (recap, agenda, notes) live in the DataJson blob — see
// Ruptura.Shared.Content.SessionLogData.
public class SessionLog
{
    public Guid Id { get; set; }

    public Guid CampaignId { get; set; }

    public DateTime Date { get; set; }

    public string Title { get; set; } = string.Empty;

    // Typed SessionLogData blob — see Ruptura.Shared.Content.SessionLogData.
    public string DataJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
