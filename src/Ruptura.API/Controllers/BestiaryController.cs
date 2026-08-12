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
            return CreatureFailure(result.Error!);
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
            return CreatureFailure(result.Error!);
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
            return CreatureFailure(result.Error!);
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
            return CreatureFailure(result.Error!);
        return Ok(ApiResponse.Ok(localizer["Bestiary.Deleted"]));
    }

    private Guid CurrentGameMasterId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    // Official write → 403 (existence known, but read-only); missing/other-GM homebrew → 404
    // (existence hidden); validation failures → 400.
    private IActionResult CreatureFailure(string error) => error switch
    {
        ErrorCodes.Bestiary.NotFound => NotFound(ApiResponse.Fail(localizer[error])),
        ErrorCodes.Bestiary.Forbidden
            => StatusCode(StatusCodes.Status403Forbidden, ApiResponse.Fail(localizer[error])),
        _ => BadRequest(ApiResponse.Fail(localizer[error]))
    };
}
