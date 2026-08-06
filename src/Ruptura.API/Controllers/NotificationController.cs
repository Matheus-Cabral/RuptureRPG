using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Common;
using Ruptura.Shared.Notifications;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController(
    INotificationService notificationService,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<NotificationGroupResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await notificationService.GetForGameMasterAsync(gameMasterId, ct);
        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<IEnumerable<NotificationGroupResponse>>.Ok(result.Value!));
    }

    [HttpPost("{id:guid}/promote")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Promote(Guid id, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await notificationService.PromoteAsync(gameMasterId, id, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Notification.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse.Ok(localizer["Notification.Promoted"]));
    }

    [HttpPost("{id:guid}/dismiss")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await notificationService.DismissAsync(gameMasterId, id, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse.Ok(localizer["Notification.Dismissed"]));
    }
}
