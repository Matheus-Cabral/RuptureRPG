using System.Text.Json;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Content;

namespace Ruptura.Infrastructure.Services;

// Session logs (GM-5): dated prep notes, each persisted as a typed SessionLogData blob scoped to one
// campaign. Campaign-ownership auth mirrors CampaignContentService/RewardService: a non-owned/missing
// campaign, or a foreign session, yields Session.NotFound (existence hidden — checked FIRST in every
// method). Title is required (trimmed non-empty). The read list is ordered by Date DESCENDING.
public class SessionLogService(
    ISessionLogRepository sessionRepo,
    ICampaignRepository campaignRepo) : ISessionLogService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private const int TitleMax = 200;

    // ── Reads ────────────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<SessionLogResponse>>> GetForCampaignAsync(
        Guid gameMasterId, Guid campaignId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<IEnumerable<SessionLogResponse>>(ErrorCodes.Session.NotFound);

        var sessions = await sessionRepo.GetByCampaignAsync(campaignId, ct);
        return Result.Success<IEnumerable<SessionLogResponse>>(sessions.Select(Map).ToList());
    }

    public async Task<Result<SessionLogResponse>> GetByIdAsync(
        Guid gameMasterId, Guid campaignId, Guid sessionId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<SessionLogResponse>(ErrorCodes.Session.NotFound);

        var session = await sessionRepo.GetByIdAsync(sessionId, ct);
        if (session is null || session.CampaignId != campaignId)
            return Result.Failure<SessionLogResponse>(ErrorCodes.Session.NotFound);

        return Result.Success(Map(session));
    }

    // ── Writes ───────────────────────────────────────────────────────────────────

    public async Task<Result<SessionLogResponse>> CreateAsync(
        Guid gameMasterId, Guid campaignId, CreateSessionLogRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<SessionLogResponse>(ErrorCodes.Session.NotFound);

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<SessionLogResponse>(ErrorCodes.Session.TitleRequired);

        var data = request.Data ?? new SessionLogData();

        var session = new SessionLog
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Date = Utc(request.Date),
            Title = TruncTitle(request.Title),
            DataJson = JsonSerializer.Serialize(data, JsonOpts),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await sessionRepo.AddAsync(session, ct);
        await sessionRepo.SaveChangesAsync(ct);

        return Result.Success(Map(session));
    }

    public async Task<Result<SessionLogResponse>> UpdateAsync(
        Guid gameMasterId, Guid campaignId, Guid sessionId, UpdateSessionLogRequest request, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure<SessionLogResponse>(ErrorCodes.Session.NotFound);

        var session = await sessionRepo.GetByIdAsync(sessionId, ct);
        if (session is null || session.CampaignId != campaignId)
            return Result.Failure<SessionLogResponse>(ErrorCodes.Session.NotFound);

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<SessionLogResponse>(ErrorCodes.Session.TitleRequired);

        var data = request.Data ?? new SessionLogData();

        session.Date = Utc(request.Date);
        session.Title = TruncTitle(request.Title);
        session.DataJson = JsonSerializer.Serialize(data, JsonOpts);
        session.UpdatedAt = DateTime.UtcNow;

        sessionRepo.Update(session);
        await sessionRepo.SaveChangesAsync(ct);

        return Result.Success(Map(session));
    }

    public async Task<Result> DeleteAsync(
        Guid gameMasterId, Guid campaignId, Guid sessionId, CancellationToken ct = default)
    {
        var campaign = await LoadOwnedCampaignAsync(gameMasterId, campaignId, ct);
        if (campaign is null)
            return Result.Failure(ErrorCodes.Session.NotFound);

        var session = await sessionRepo.GetByIdAsync(sessionId, ct);
        if (session is null || session.CampaignId != campaignId)
            return Result.Failure(ErrorCodes.Session.NotFound);

        sessionRepo.Remove(session);
        await sessionRepo.SaveChangesAsync(ct);
        return Result.Success();
    }

    // ── Auth ─────────────────────────────────────────────────────────────────────

    private async Task<Campaign?> LoadOwnedCampaignAsync(Guid gameMasterId, Guid campaignId, CancellationToken ct)
    {
        var campaign = await campaignRepo.GetByIdAsync(campaignId, ct);
        return campaign is not null && campaign.GameMasterId == gameMasterId ? campaign : null;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static string TruncTitle(string? title)
    {
        var value = (title ?? string.Empty).Trim();
        return value.Length > TitleMax ? value[..TitleMax] : value;
    }

    // Npgsql rejects a non-UTC Kind on a timestamptz column; normalize before saving.
    // Local → convert instant-preserving; Unspecified → assume already UTC and just stamp the Kind.
    private static DateTime Utc(DateTime d) => d.Kind switch
    {
        DateTimeKind.Utc => d,
        DateTimeKind.Local => d.ToUniversalTime(),
        _ => DateTime.SpecifyKind(d, DateTimeKind.Utc)
    };

    // ── Mapping ────────────────────────────────────────────────────────────────────

    private static SessionLogResponse Map(SessionLog session) => new()
    {
        Id = session.Id,
        Date = session.Date,
        Title = session.Title,
        Data = Deserialize(session.DataJson)
    };

    private static SessionLogData Deserialize(string json)
    {
        SessionLogData? data;
        try { data = JsonSerializer.Deserialize<SessionLogData>(json, JsonOpts); }
        catch (JsonException) { data = null; }
        return data ?? new SessionLogData();
    }
}
