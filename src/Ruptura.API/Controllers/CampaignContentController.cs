using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Common;
using Ruptura.Shared.Content;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

// Session-prep content (GM-5): arcs and floors scoped to one campaign. Every action is
// campaign-ownership authoritative (a non-owned/missing campaign, arc or floor → 404, existence
// hidden); every validation failure → 400.
[ApiController]
[Route("api/campaigns/{campaignId:guid}")]
[Authorize(Roles = "GameMaster")]
public class CampaignContentController(
    ICampaignContentService contentService,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    // ── Arcs ──

    [HttpGet("arcs")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ArcResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArcs(Guid campaignId, CancellationToken ct)
    {
        var result = await contentService.GetArcsForCampaignAsync(CurrentGameMasterId(), campaignId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<IEnumerable<ArcResponse>>.Ok(result.Value!));
    }

    [HttpGet("arcs/{arcId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ArcResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArcById(Guid campaignId, Guid arcId, CancellationToken ct)
    {
        var result = await contentService.GetArcByIdAsync(CurrentGameMasterId(), campaignId, arcId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<ArcResponse>.Ok(result.Value!));
    }

    [HttpPost("arcs")]
    [ProducesResponseType(typeof(ApiResponse<ArcResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateArc(
        Guid campaignId, [FromBody] CreateArcRequest request, CancellationToken ct)
    {
        var result = await contentService.CreateArcAsync(CurrentGameMasterId(), campaignId, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ArcResponse>.Ok(result.Value!));
    }

    [HttpPut("arcs/{arcId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ArcResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateArc(
        Guid campaignId, Guid arcId, [FromBody] UpdateArcRequest request, CancellationToken ct)
    {
        var result = await contentService.UpdateArcAsync(CurrentGameMasterId(), campaignId, arcId, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<ArcResponse>.Ok(result.Value!));
    }

    [HttpDelete("arcs/{arcId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteArc(Guid campaignId, Guid arcId, CancellationToken ct)
    {
        var result = await contentService.DeleteArcAsync(CurrentGameMasterId(), campaignId, arcId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse.Ok());
    }

    // ── Floors ──

    [HttpGet("arcs/{arcId:guid}/floors")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FloorResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFloorsForArc(Guid campaignId, Guid arcId, CancellationToken ct)
    {
        var result = await contentService.GetFloorsForArcAsync(CurrentGameMasterId(), campaignId, arcId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<IEnumerable<FloorResponse>>.Ok(result.Value!));
    }

    [HttpGet("floors")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<FloorResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFloors(Guid campaignId, CancellationToken ct)
    {
        var result = await contentService.GetFloorsForCampaignAsync(CurrentGameMasterId(), campaignId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<IEnumerable<FloorResponse>>.Ok(result.Value!));
    }

    [HttpGet("floors/{floorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FloorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFloorById(Guid campaignId, Guid floorId, CancellationToken ct)
    {
        var result = await contentService.GetFloorByIdAsync(CurrentGameMasterId(), campaignId, floorId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<FloorResponse>.Ok(result.Value!));
    }

    [HttpPost("floors")]
    [ProducesResponseType(typeof(ApiResponse<FloorResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateFloor(
        Guid campaignId, [FromBody] CreateFloorRequest request, CancellationToken ct)
    {
        var result = await contentService.CreateFloorAsync(CurrentGameMasterId(), campaignId, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<FloorResponse>.Ok(result.Value!));
    }

    [HttpPut("floors/{floorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FloorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFloor(
        Guid campaignId, Guid floorId, [FromBody] UpdateFloorRequest request, CancellationToken ct)
    {
        var result = await contentService.UpdateFloorAsync(CurrentGameMasterId(), campaignId, floorId, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<FloorResponse>.Ok(result.Value!));
    }

    [HttpDelete("floors/{floorId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFloor(Guid campaignId, Guid floorId, CancellationToken ct)
    {
        var result = await contentService.DeleteFloorAsync(CurrentGameMasterId(), campaignId, floorId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse.Ok());
    }

    private Guid CurrentGameMasterId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    // Missing/non-owned campaign, arc or floor → 404 (existence hidden); every validation failure → 400.
    private IActionResult Failure(string error) => error switch
    {
        ErrorCodes.Content.NotFound => NotFound(ApiResponse.Fail(localizer[error])),
        _ => BadRequest(ApiResponse.Fail(localizer[error]))
    };
}
