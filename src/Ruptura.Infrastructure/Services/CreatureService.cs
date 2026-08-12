using System.Text.Json;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Bestiary;

namespace Ruptura.Infrastructure.Services;

public class CreatureService(
    ICreatureRepository creatureRepo,
    ICreatureStatsCalculator calculator) : ICreatureService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // Bound free-text so the DataJson blob can't grow unbounded (Kestrel's request ceiling aside).
    private const int NameMax = 200;

    public async Task<Result<IEnumerable<CreatureResponse>>> GetForGameMasterAsync(
        Guid gameMasterId, CancellationToken ct = default)
    {
        var creatures = await creatureRepo.GetForGameMasterAsync(gameMasterId, ct);
        return Result.Success(creatures.Select(MapToResponse));
    }

    public async Task<Result<CreatureResponse>> GetByIdAsync(
        Guid gameMasterId, Guid id, CancellationToken ct = default)
    {
        var creature = await creatureRepo.GetByIdAsync(id, ct);
        // Own or official → visible; another GM's homebrew → hide its existence.
        if (creature is null || !IsVisibleTo(creature, gameMasterId))
            return Result.Failure<CreatureResponse>(ErrorCodes.Bestiary.NotFound);

        return Result.Success(MapToResponse(creature));
    }

    public async Task<Result<CreatureResponse>> CreateAsync(
        Guid gameMasterId, CreateCreatureRequest request, CancellationToken ct = default)
    {
        var data = request.Data ?? new CreatureData();
        var validationError = Validate(data);
        if (validationError is not null)
            return Result.Failure<CreatureResponse>(validationError);

        Sanitize(data);

        var creature = new Creature
        {
            Id = Guid.NewGuid(),
            GameMasterId = gameMasterId,       // owned homebrew — never null (official is seed-only)
            Name = TruncName(request.Name),
            DataJson = JsonSerializer.Serialize(data, JsonOpts),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await creatureRepo.AddAsync(creature, ct);
        await creatureRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(creature));
    }

    public async Task<Result<CreatureResponse>> UpdateAsync(
        Guid gameMasterId, Guid id, UpdateCreatureRequest request, CancellationToken ct = default)
    {
        var creature = await creatureRepo.GetByIdAsync(id, ct);
        var auth = ResolveForWrite(creature, gameMasterId);
        if (auth is not null)
            return Result.Failure<CreatureResponse>(auth);

        var data = request.Data ?? new CreatureData();
        var validationError = Validate(data);
        if (validationError is not null)
            return Result.Failure<CreatureResponse>(validationError);

        Sanitize(data);

        creature!.Name = TruncName(request.Name);
        creature.DataJson = JsonSerializer.Serialize(data, JsonOpts);
        creature.UpdatedAt = DateTime.UtcNow;

        creatureRepo.Update(creature);
        await creatureRepo.SaveChangesAsync(ct);

        return Result.Success(MapToResponse(creature));
    }

    public async Task<Result> DeleteAsync(Guid gameMasterId, Guid id, CancellationToken ct = default)
    {
        var creature = await creatureRepo.GetByIdAsync(id, ct);
        var auth = ResolveForWrite(creature, gameMasterId);
        if (auth is not null)
            return Result.Failure(auth);

        creatureRepo.Remove(creature!);
        await creatureRepo.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ── Auth ──────────────────────────────────────────────────────────────────

    private static bool IsVisibleTo(Creature c, Guid gameMasterId) =>
        c.GameMasterId == gameMasterId || c.GameMasterId is null;

    // Write auth: own → OK (null); official (owner-null, readable to all) → Forbidden;
    // missing OR another GM's homebrew → NotFound (existence hidden). Returns the error code or null.
    private static string? ResolveForWrite(Creature? c, Guid gameMasterId)
    {
        if (c is null) return ErrorCodes.Bestiary.NotFound;
        if (c.GameMasterId is null) return ErrorCodes.Bestiary.Forbidden;
        if (c.GameMasterId != gameMasterId) return ErrorCodes.Bestiary.NotFound;
        return null;
    }

    // ── Validation & sanitizing ────────────────────────────────────────────────

    // Fixed-set + required-field checks (§9.5). Function/Type/weights/tiers/rarities allow custom
    // values by design (the NP calculator treats unknowns as 0), so they are not validated here.
    private static string? Validate(CreatureData data)
    {
        if (string.IsNullOrWhiteSpace(data.Fraqueza))
            return ErrorCodes.Bestiary.FraquezaRequired;
        if (!BestiaryReference.Behaviors.Contains(data.Behavior))
            return ErrorCodes.Bestiary.BehaviorInvalid;
        if (!BestiaryReference.Categories.Contains(data.Category))
            return ErrorCodes.Bestiary.CategoryInvalid;
        return null;
    }

    // Defense-in-depth structural cleanup: drop null list elements (the calculator would NPE on
    // them) and floor numeric bounds at 0 (a negative score/point would perversely reduce NP).
    // Mirrors GuildSheetService's clamp posture — the server never trusts raw wire numbers.
    private static void Sanitize(CreatureData data)
    {
        var a = data.Attributes ??= new CreatureAttributes();
        a.Corpo = Math.Max(0, a.Corpo);
        a.Controle = Math.Max(0, a.Controle);
        a.Vigor = Math.Max(0, a.Vigor);
        a.Presenca = Math.Max(0, a.Presenca);
        a.Intelecto = Math.Max(0, a.Intelecto);
        a.Percepcao = Math.Max(0, a.Percepcao);
        a.Vontade = Math.Max(0, a.Vontade);
        a.Afinidade = Math.Max(0, a.Afinidade);

        data.NaturalSkills = (data.NaturalSkills ?? []).Where(s => s is not null).ToList();
        foreach (var s in data.NaturalSkills)
            s.Points = Math.Max(0, s.Points);

        data.Characteristics = (data.Characteristics ?? []).Where(c => c is not null).ToList();
        data.Abilities = (data.Abilities ?? []).Where(x => x is not null).ToList();
        data.Equipment = (data.Equipment ?? []).Where(e => e is not null).ToList();
        data.Recompensas = (data.Recompensas ?? []).Where(r => r is not null).ToList();

        data.Pv = Math.Max(0, data.Pv);
        data.DefesaPassiva = Math.Max(0, data.DefesaPassiva);
        data.Deslocamento = Math.Max(0, data.Deslocamento);
    }

    private static string TruncName(string? name)
    {
        var value = (name ?? string.Empty).Trim();
        return value.Length > NameMax ? value[..NameMax] : value;
    }

    // ── Mapping ────────────────────────────────────────────────────────────────

    // NP is server-authoritative: always recomputed from the stored blob via the calculator.
    private CreatureResponse MapToResponse(Creature c)
    {
        var data = Deserialize(c.DataJson);
        var np = calculator.Calculate(data);
        return new CreatureResponse
        {
            Id = c.Id,
            Name = c.Name,
            IsOfficial = c.GameMasterId is null,
            Data = data,
            DerivedNp = np.Np,
            CategoryNpMin = np.NpMin,
            CategoryNpMax = np.NpMax,
            CategoryOverflow = np.CategoryOverflow
        };
    }

    // A malformed/empty blob degrades to an empty CreatureData rather than throwing on read.
    private static CreatureData Deserialize(string json)
    {
        CreatureData? data;
        try { data = JsonSerializer.Deserialize<CreatureData>(json, JsonOpts); }
        catch (JsonException) { data = null; }
        return data ?? new CreatureData();
    }
}
