using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Combat;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId:guid}/combat")]
[Authorize(Roles = "GameMaster")]
public class CombatController(
    ICombatService combatService,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CombatSessionResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid campaignId, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await combatService.GetForCampaignAsync(gameMasterId, campaignId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<IEnumerable<CombatSessionResponse>>.Ok(result.Value!));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CombatSessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid campaignId, Guid id, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await combatService.GetByIdAsync(gameMasterId, campaignId, id, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<CombatSessionResponse>.Ok(result.Value!));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CombatSessionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        Guid campaignId, [FromBody] CreateCombatSessionRequest request, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await combatService.CreateAsync(gameMasterId, campaignId, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CombatSessionResponse>.Ok(result.Value!));
    }

    [HttpPost("start-from-encounter")]
    [ProducesResponseType(typeof(ApiResponse<CombatSessionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartFromEncounter(
        Guid campaignId, [FromBody] StartFromEncounterRequest request, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await combatService.StartFromEncounterAsync(gameMasterId, campaignId, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CombatSessionResponse>.Ok(result.Value!));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CombatSessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid campaignId, Guid id, [FromBody] UpdateCombatStateRequest request, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await combatService.UpdateStateAsync(gameMasterId, campaignId, id, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<CombatSessionResponse>.Ok(result.Value!));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid campaignId, Guid id, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await combatService.DeleteAsync(gameMasterId, campaignId, id, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse.Ok());
    }

    private Guid CurrentGameMasterId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    // Missing/non-owned campaign or session → 404 (existence hidden); every validation failure → 400.
    private IActionResult Failure(string error) => error switch
    {
        ErrorCodes.Combat.NotFound => NotFound(ApiResponse.Fail(localizer[error])),
        _ => BadRequest(ApiResponse.Fail(localizer[error]))
    };
}
