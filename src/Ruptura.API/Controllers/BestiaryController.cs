using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Bestiary;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/bestiary")]
[Authorize(Roles = "GameMaster")]
public class BestiaryController(
    ICreatureService creatureService,
    INpcService npcService,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpGet("creatures")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CreatureResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCreatures(CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await creatureService.GetForGameMasterAsync(gameMasterId, ct);
        return Ok(ApiResponse<IEnumerable<CreatureResponse>>.Ok(result.Value!));
    }

    [HttpGet("creatures/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CreatureResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCreature(Guid id, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await creatureService.GetByIdAsync(gameMasterId, id, ct);
        if (result.IsFailure)
            return BestiaryFailure(result.Error!);
        return Ok(ApiResponse<CreatureResponse>.Ok(result.Value!));
    }

    [HttpPost("creatures")]
    [ProducesResponseType(typeof(ApiResponse<CreatureResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCreature([FromBody] CreateCreatureRequest request, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await creatureService.CreateAsync(gameMasterId, request, ct);
        if (result.IsFailure)
            return BestiaryFailure(result.Error!);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CreatureResponse>.Ok(result.Value!, localizer["Bestiary.Created"]));
    }

    [HttpPut("creatures/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CreatureResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCreature(
        Guid id, [FromBody] UpdateCreatureRequest request, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await creatureService.UpdateAsync(gameMasterId, id, request, ct);
        if (result.IsFailure)
            return BestiaryFailure(result.Error!);
        return Ok(ApiResponse<CreatureResponse>.Ok(result.Value!, localizer["Bestiary.Updated"]));
    }

    [HttpDelete("creatures/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCreature(Guid id, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await creatureService.DeleteAsync(gameMasterId, id, ct);
        if (result.IsFailure)
            return BestiaryFailure(result.Error!);
        return Ok(ApiResponse.Ok(localizer["Bestiary.Deleted"]));
    }

    // ── NPCs (non-combat; no NP calculation) ────────────────────────────────────

    [HttpGet("npcs")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NpcResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNpcs(CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await npcService.GetForGameMasterAsync(gameMasterId, ct);
        return Ok(ApiResponse<IEnumerable<NpcResponse>>.Ok(result.Value!));
    }

    [HttpGet("npcs/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NpcResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNpc(Guid id, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await npcService.GetByIdAsync(gameMasterId, id, ct);
        if (result.IsFailure)
            return BestiaryFailure(result.Error!);
        return Ok(ApiResponse<NpcResponse>.Ok(result.Value!));
    }

    [HttpPost("npcs")]
    [ProducesResponseType(typeof(ApiResponse<NpcResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNpc([FromBody] CreateNpcRequest request, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await npcService.CreateAsync(gameMasterId, request, ct);
        if (result.IsFailure)
            return BestiaryFailure(result.Error!);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<NpcResponse>.Ok(result.Value!, localizer["Bestiary.NpcCreated"]));
    }

    [HttpPut("npcs/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NpcResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNpc(
        Guid id, [FromBody] UpdateNpcRequest request, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await npcService.UpdateAsync(gameMasterId, id, request, ct);
        if (result.IsFailure)
            return BestiaryFailure(result.Error!);
        return Ok(ApiResponse<NpcResponse>.Ok(result.Value!, localizer["Bestiary.NpcUpdated"]));
    }

    [HttpDelete("npcs/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNpc(Guid id, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await npcService.DeleteAsync(gameMasterId, id, ct);
        if (result.IsFailure)
            return BestiaryFailure(result.Error!);
        return Ok(ApiResponse.Ok(localizer["Bestiary.NpcDeleted"]));
    }

    private Guid CurrentGameMasterId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    // Official write → 403 (existence known, but read-only); missing/other-GM homebrew → 404
    // (existence hidden); validation failures → 400.
    private IActionResult BestiaryFailure(string error) => error switch
    {
        ErrorCodes.Bestiary.NotFound or ErrorCodes.Bestiary.NpcNotFound
            => NotFound(ApiResponse.Fail(localizer[error])),
        ErrorCodes.Bestiary.Forbidden
            => StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail(localizer[error])),
        _ => BadRequest(ApiResponse.Fail(localizer[error]))
    };
}
