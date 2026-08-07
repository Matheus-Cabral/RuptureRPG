namespace Ruptura.Shared.Guilds;

public class UpdateExpeditionRequest
{
    public string Kind { get; set; } = string.Empty;  // "Principal" | "Secundaria"
    public DateTime Date { get; set; }
    public string Participants { get; set; } = string.Empty;
    public string Objective { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public string Losses { get; set; } = string.Empty;
    public string ResourcesGained { get; set; } = string.Empty;
}
