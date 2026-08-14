using System.Text.Json;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Bestiary;

namespace Ruptura.Infrastructure.Services;

// NPCs are non-combat records: no NP calculation, no Fraqueza requirement. NpcData needs only
// structural cleanup (valid JSON, no null free-text). Same own+official/read-only auth model as
// CreatureService: read own+official; write/delete own only; official → Forbidden; other-GM → NotFound.
public class NpcService(INpcRepository npcRepo) : INpcService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // Bound free-text so the DataJson blob can't grow unbounded (Kestrel's request ceiling aside).
    private const int NameMax = 200;
    private const int NotesMax = 2000;

    public async Task<Result<IEnumerable<NpcResponse>>> GetForGameMasterAsync(
        Guid gameMasterId, CancellationToken ct = default)
    {
        var npcs = await npcRepo.GetForGameMasterAsync(gameMasterId, ct);
        return Result.Success(npcs.Select(MapToResponse));
    }

    public async Task<Result<NpcResponse>> GetByIdAsync(
        Guid gameMasterId, Guid id, CancellationToken ct = default)
    {
        var npc = await npcRepo.GetByIdAsync(id, ct);
        // Own or official → visible; another GM's homebrew → hide its existence.
        if (npc is null || !IsVisibleTo(npc, gameMasterId))
            return Result.Failure<NpcResponse>(ErrorCodes.Bestiary.NotFound);

        return Result.Success(MapToResponse(npc));
    }

    public async Task<Result<NpcResponse>> CreateAsync(
        Guid gameMasterId, CreateNpcRequest request, CancellationToken ct = default)
    {
        var data = Sanitize(request.Data ?? new NpcData());

        var npc = new Npc
        {
            Id = Guid.NewGuid(),
            GameMasterId = gameMasterId,       // owned homebrew — never null (official is seed-only)
            Name = TruncName(request.Name),
            DataJson = JsonSerializer.Serialize(data, JsonOpts),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await npcRepo.AddAsync(npc, ct);
        await npcRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(npc));
    }

    public async Task<Result<NpcResponse>> UpdateAsync(
        Guid gameMasterId, Guid id, UpdateNpcRequest request, CancellationToken ct = default)
    {
        var npc = await npcRepo.GetByIdAsync(id, ct);
        var auth = ResolveForWrite(npc, gameMasterId);
        if (auth is not null)
            return Result.Failure<NpcResponse>(auth);

        var data = Sanitize(request.Data ?? new NpcData());

        npc!.Name = TruncName(request.Name);
        npc.DataJson = JsonSerializer.Serialize(data, JsonOpts);
        npc.UpdatedAt = DateTime.UtcNow;

        npcRepo.Update(npc);
        await npcRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(npc));
    }

    public async Task<Result> DeleteAsync(Guid gameMasterId, Guid id, CancellationToken ct = default)
    {
        var npc = await npcRepo.GetByIdAsync(id, ct);
        var auth = ResolveForWrite(npc, gameMasterId);
        if (auth is not null)
            return Result.Failure(auth);

        npcRepo.Remove(npc!);
        await npcRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ── Auth ──────────────────────────────────────────────────────────────────

    private static bool IsVisibleTo(Npc n, Guid gameMasterId) =>
        n.GameMasterId == gameMasterId || n.GameMasterId is null;

    // Write auth: own → OK (null); official (owner-null, readable to all) → Forbidden;
    // missing OR another GM's homebrew → NotFound (existence hidden). Returns the error code or null.
    private static string? ResolveForWrite(Npc? n, Guid gameMasterId)
    {
        if (n is null) return ErrorCodes.Bestiary.NotFound;
        if (n.GameMasterId is null) return ErrorCodes.Bestiary.Forbidden;
        if (n.GameMasterId != gameMasterId) return ErrorCodes.Bestiary.NotFound;
        return null;
    }

    // ── Sanitizing ─────────────────────────────────────────────────────────────

    // Structural-only cleanup: coalesce null free-text to empty strings so the stored blob is
    // well-formed. NPC fields (Role/Faction/Disposition/Location/Notes) allow custom values —
    // there is no fixed-set validation and no combat math.
    private static NpcData Sanitize(NpcData data) => new()
    {
        Role = Trunc((data.Role ?? string.Empty).Trim(), NameMax),
        Faction = Trunc((data.Faction ?? string.Empty).Trim(), NameMax),
        Disposition = Trunc((data.Disposition ?? string.Empty).Trim(), NameMax),
        Location = Trunc((data.Location ?? string.Empty).Trim(), NameMax),
        // Bound the long free-text description so the blob can't grow unbounded (mirrors Name→200).
        Notes = Trunc(data.Notes ?? string.Empty, NotesMax)
    };

    private static string TruncName(string? name) => Trunc((name ?? string.Empty).Trim(), NameMax);

    private static string Trunc(string? value, int max)
    {
        var v = value ?? string.Empty;
        return v.Length > max ? v[..max] : v;
    }

    // ── Mapping ────────────────────────────────────────────────────────────────

    private static NpcResponse MapToResponse(Npc n) => new()
    {
        Id = n.Id,
        Name = n.Name,
        IsOfficial = n.GameMasterId is null,
        Data = Deserialize(n.DataJson)
    };

    // A malformed/empty blob degrades to an empty NpcData rather than throwing on read.
    private static NpcData Deserialize(string json)
    {
        NpcData? data;
        try { data = JsonSerializer.Deserialize<NpcData>(json, JsonOpts); }
        catch (JsonException) { data = null; }
        return data ?? new NpcData();
    }
}
