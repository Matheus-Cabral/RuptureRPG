using System.ComponentModel.DataAnnotations;

namespace Ruptura.Shared.Campaigns;

public class CreateCampaignRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
