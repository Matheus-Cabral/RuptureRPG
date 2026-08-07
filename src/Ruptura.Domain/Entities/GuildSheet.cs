namespace Ruptura.Domain.Entities;

public class GuildSheet
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }              // real FK + unique index (1 guild per campaign)
    public string GuildName { get; set; } = string.Empty;
    public Guid CreatedByGameMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Stable, low-churn modules (Identity, Prestige, Influence, Resources, active
    // doctrines, Knowledge, Legado, FloorsConquered) — see Ruptura.Shared.Guilds.GuildSheetData.
    // High-churn lists live in dedicated child tables, not here.
    public string DataJson { get; set; } = "{}";

    // Postgres xmin system column, surfaced as a round-trippable optimistic-concurrency
    // token (sub-plan #3 returns it in the read DTO and requires it on write).
    public uint Version { get; set; }
}
