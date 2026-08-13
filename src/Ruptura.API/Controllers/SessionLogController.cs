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

// Session logs (GM-5): dated prep notes scoped to one campaign. Every action is campaign-ownership
// authoritative (a non-owned/missing campaign or session → 404, existence hidden); every validation
// failure → 400. Simple CRUD; the list is ordered by Date DESCENDING.
[ApiController]
[Route("api/campaigns/{campaignId:guid}/sessions")]
[Authorize(Roles = "GameMaster")]
public class SessionLogController(
    ISessionLogService sessionService,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SessionLogResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessions(Guid campaignId, CancellationToken ct)
    {
        var result = await sessionService.GetForCampaignAsync(CurrentGameMasterId(), campaignId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<IEnumerable<SessionLogResponse>>.Ok(result.Value!));
    }

    [HttpGet("{sessionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SessionLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionById(Guid campaignId, Guid sessionId, CancellationToken ct)
    {
        var result = await sessionService.GetByIdAsync(CurrentGameMasterId(), campaignId, sessionId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<SessionLogResponse>.Ok(result.Value!));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SessionLogResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSession(
        Guid campaignId, [FromBody] CreateSessionLogRequest request, CancellationToken ct)
    {
        var result = await sessionService.CreateAsync(CurrentGameMasterId(), campaignId, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SessionLogResponse>.Ok(result.Value!));
    }

    [HttpPut("{sessionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SessionLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSession(
        Guid campaignId, Guid sessionId, [FromBody] UpdateSessionLogRequest request, CancellationToken ct)
    {
        var result = await sessionService.UpdateAsync(CurrentGameMasterId(), campaignId, sessionId, request, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse<SessionLogResponse>.Ok(result.Value!));
    }

    [HttpDelete("{sessionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSession(Guid campaignId, Guid sessionId, CancellationToken ct)
    {
        var result = await sessionService.DeleteAsync(CurrentGameMasterId(), campaignId, sessionId, ct);
        if (result.IsFailure)
            return Failure(result.Error!);
        return Ok(ApiResponse.Ok());
    }

    private Guid CurrentGameMasterId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    // Missing/non-owned campaign or session → 404 (existence hidden); every validation failure → 400.
    private IActionResult Failure(string error) => error switch
    {
        ErrorCodes.Session.NotFound => NotFound(ApiResponse.Fail(localizer[error])),
        _ => BadRequest(ApiResponse.Fail(localizer[error]))
    };
}
