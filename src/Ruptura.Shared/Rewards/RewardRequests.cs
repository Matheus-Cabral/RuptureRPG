namespace Ruptura.Shared.Rewards;

public class CreateRewardRequest
{
    public string Name { get; set; } = string.Empty;
    public RewardData Data { get; set; } = new();
}

public class UpdateRewardRequest
{
    public string Name { get; set; } = string.Empty;
    public RewardData Data { get; set; } = new();
}
