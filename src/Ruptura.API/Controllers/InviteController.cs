using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Common;
using Ruptura.Shared.Invites;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/invites")]
[Authorize(Roles = "GameMaster")]
public class InviteController(
    IInviteCodeService inviteService,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<InviteCodeResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Generate(CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await inviteService.GenerateAsync(gameMasterId, ct);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<InviteCodeResponse>.Ok(result.Value!, localizer["Invite.Generated"]));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InviteCodeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await inviteService.GetByGameMasterAsync(gameMasterId, ct);

        return Ok(ApiResponse<IEnumerable<InviteCodeResponse>>.Ok(result.Value!));
    }

    [HttpGet("{code}/validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<InviteCodeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Validate(string code, CancellationToken ct)
    {
        var result = await inviteService.GetByCodeAsync(code, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<InviteCodeResponse>.Ok(result.Value!));
    }
}
