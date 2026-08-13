using System.Text.Json;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Rewards;

namespace Ruptura.Infrastructure.Services;

// Reward planner CRUD (GM-3). The reward package is persisted as a typed DataJson blob scoped to
// one campaign. Campaign-ownership auth mirrors EncounterService/CampaignDashboardService: a
// non-owned/missing campaign or a foreign reward yields Reward.NotFound (existence hidden). On
// write the blob is sanitized (null list lines dropped, VE clamped to [1,5], resource ints and
// material quantities floored at ≥0) and validated (Name required, strategic-asset categories
// against RewardReference, an optional EncounterId against the same campaign). On read the linked
// encounter's name is resolved.
public class RewardService(
    IRewardRepository rewardRepo,
    ICampaignRepository campaignRepo,
    IEncounterRepository encounterRepo) : IRewardService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private const int NameMax = 200;
    private const int VeMin = 1;
    private const int VeMax = 5;

    // ── Reads ──────────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<RewardResponse>>> GetForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<RewardResponse>>(ErrorCodes.Reward.NotFound);

        var rewards = await rewardRepo.GetByCampaignAsync(campaignId, ct);
        var responses = new List<RewardResponse>();
        foreach (var reward in rewards)
            responses.Add(await MapToResponseAsync(reward, ct));
        return Result.Success<IEnumerable<RewardResponse>>(responses);
    }

    public async Task<Result<RewardResponse>> GetByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<RewardResponse>(ErrorCodes.Reward.NotFound);

        var reward = await rewardRepo.GetByIdAsync(id, ct);
        if (reward is null || reward.CampaignId != campaignId)
            return Result.Failure<RewardResponse>(ErrorCodes.Reward.NotFound);

        return Result.Success(await MapToResponseAsync(reward, ct));
    }

    // ── Writes ─────────────────────────────────────────────────────────────────

    public async Task<Result<RewardResponse>> CreateAsync(
        Guid gameMasterId, Guid campaignId, CreateRewardRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<RewardResponse>(ErrorCodes.Reward.NotFound);

        var data = request.Data ?? new RewardData();
        var validation = await ValidateAndSanitizeAsync(request.Name, data, campaignId, ct);
        if (validation is not null)
            return Result.Failure<RewardResponse>(validation);

        var reward = new Reward
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = TruncName(request.Name),
            DataJson = JsonSerializer.Serialize(data, JsonOpts),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await rewardRepo.AddAsync(reward, ct);
        await rewardRepo.SaveChangesAsync(ct);

        return Result.Success(await MapToResponseAsync(reward, ct));
    }

    public async Task<Result<RewardResponse>> UpdateAsync(
        Guid gameMasterId, Guid campaignId, Guid id, UpdateRewardRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<RewardResponse>(ErrorCodes.Reward.NotFound);

        var reward = await rewardRepo.GetByIdAsync(id, ct);
        if (reward is null || reward.CampaignId != campaignId)
            return Result.Failure<RewardResponse>(ErrorCodes.Reward.NotFound);

        var data = request.Data ?? new RewardData();
        var validation = await ValidateAndSanitizeAsync(request.Name, data, campaignId, ct);
        if (validation is not null)
            return Result.Failure<RewardResponse>(validation);

        reward.Name = TruncName(request.Name);
        reward.DataJson = JsonSerializer.Serialize(data, JsonOpts);
        reward.UpdatedAt = DateTime.UtcNow;

        rewardRepo.Update(reward);
        await rewardRepo.SaveChangesAsync(ct);

        return Result.Success(await MapToResponseAsync(reward, ct));
    }

    public async Task<Result> DeleteAsync(
        Guid gameMasterId, Guid campaignId, Guid id, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure(ErrorCodes.Reward.NotFound);

        var reward = await rewardRepo.GetByIdAsync(id, ct);
        if (reward is null || reward.CampaignId != campaignId)
            return Result.Failure(ErrorCodes.Reward.NotFound);

        rewardRepo.Remove(reward);
        await rewardRepo.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ── Auth ──────────────────────────────────────────────────────────────────

    private async Task<Campaign?> LoadOwnedCampaignAsync(Guid gameMasterId, Guid campaignId, CancellationToken ct)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        return campaign is not null && campaign.GameMasterId == gameMasterId ? campaign : null;
    }

    // ── Validation & sanitizing ─────────────────────────────────────────────────

    // Mutates `data` in place (null lines dropped, VE/ints clamped) and returns the first error
    // code, or null when the package is valid. Name is required; strategic-asset categories are
    // validated against RewardReference; an optional EncounterId must belong to this campaign.
    private async Task<string?> ValidateAndSanitizeAsync(
        string? name, RewardData data, Guid campaignId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ErrorCodes.Reward.NameRequired;

        Sanitize(data);

        if (data.StrategicAssets.Any(a => !RewardReference.Categories.Contains(a.Category)))
            return ErrorCodes.Reward.CategoryInvalid;

        if (data.EncounterId is { } encounterId)
        {
            var encounter = await encounterRepo.GetByIdAsync(encounterId, ct);
            if (encounter is null || encounter.CampaignId != campaignId)
                return ErrorCodes.Reward.EncounterInvalid;
        }

        return null;
    }

    // Defense-in-depth: drop null list lines, clamp each VE to [1,5], and floor resource ints and
    // material quantities at ≥0 so a stray negative can't corrupt a payout.
    private static void Sanitize(RewardData data)
    {
        data.Silver = Math.Max(0, data.Silver);
        data.PactCoins = Math.Max(0, data.PactCoins);
        data.Fragments = Math.Max(0, data.Fragments);
        data.Cristais = Math.Max(0, data.Cristais);

        data.Materials = (data.Materials ?? [])
            .Where(m => m is not null)
            .Select(m => { m.Quantity = Math.Max(0, m.Quantity); return m; })
            .ToList();

        data.StrategicAssets = (data.StrategicAssets ?? [])
            .Where(a => a is not null)
            .Select(a => { a.Ve = Math.Clamp(a.Ve, VeMin, VeMax); return a; })
            .ToList();

        data.Knowledge = (data.Knowledge ?? []).Where(k => k is not null).ToList();
        data.Items = (data.Items ?? []).Where(i => i is not null).ToList();
    }

    private static string TruncName(string? name)
    {
        var value = (name ?? string.Empty).Trim();
        return value.Length > NameMax ? value[..NameMax] : value;
    }

    // ── Mapping ──────────────────────────────────────────────────────────────────

    private async Task<RewardResponse> MapToResponseAsync(Reward reward, CancellationToken ct)
    {
        var data = Deserialize(reward.DataJson);

        string? encounterName = null;
        if (data.EncounterId is { } encounterId)
        {
            var encounter = await encounterRepo.GetByIdAsync(encounterId, ct);
            if (encounter is not null && encounter.CampaignId == reward.CampaignId)
                encounterName = encounter.Name;
        }

        return new RewardResponse
        {
            Id = reward.Id,
            Name = reward.Name,
            Data = data,
            EncounterName = encounterName
        };
    }

    private static RewardData Deserialize(string json)
    {
        RewardData? data;
        try { data = JsonSerializer.Deserialize<RewardData>(json, JsonOpts); }
        catch (JsonException) { data = null; }
        return data ?? new RewardData();
    }
}
