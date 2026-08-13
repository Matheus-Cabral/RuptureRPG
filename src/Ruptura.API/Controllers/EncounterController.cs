using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Common;
using Ruptura.Shared.Encounters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId:guid}/encounters")]
[Authorize(Roles = "GameMaster")]
public class EncounterController(
    IEncounterService encounterService,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<EncounterResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid campaignId, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await encounterService.GetForCampaignAsync(gameMasterId, campaignId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<IEnumerable<EncounterResponse>>.Ok(result.Value!));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EncounterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid campaignId, Guid id, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await encounterService.GetByIdAsync(gameMasterId, campaignId, id, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<EncounterResponse>.Ok(result.Value!));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EncounterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        Guid campaignId, [FromBody] CreateEncounterRequest request, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await encounterService.CreateAsync(gameMasterId, campaignId, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<EncounterResponse>.Ok(result.Value!));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EncounterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid campaignId, Guid id, [FromBody] UpdateEncounterRequest request, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await encounterService.UpdateAsync(gameMasterId, campaignId, id, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<EncounterResponse>.Ok(result.Value!));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid campaignId, Guid id, CancellationToken ct)
    {
        var gameMasterId = CurrentGameMasterId();
        var result = await encounterService.DeleteAsync(gameMasterId, campaignId, id, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse.Ok());
    }

    private Guid CurrentGameMasterId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    // Missing/non-owned campaign or encounter → 404 (existence hidden);
    // every validation failure → 400.
    private IActionResult Failure(string error) => error switch
    {
        ErrorCodes.Encounter.NotFound => NotFound(ApiResponse.Fail(localizer[error])),
        _ => BadRequest(ApiResponse.Fail(localizer[error]))
    };
}
