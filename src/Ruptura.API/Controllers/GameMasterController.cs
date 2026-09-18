using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/gamemaster")]
[Authorize(Roles = "GameMaster")]
public class GameMasterController(
    ICampaignService campaignService,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpGet("players")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PlayerRosterResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Players(CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.GetRosterAsync(gameMasterId, ct);

        return Ok(ApiResponse<IEnumerable<PlayerRosterResponse>>.Ok(result.Value!));
    }

    [HttpPost("players/{playerId:guid}/reset-password")]
    [ProducesResponseType(typeof(ApiResponse<ResetPlayerPasswordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPlayerPassword(Guid playerId, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.ResetPlayerPasswordAsync(gameMasterId, playerId, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Campaign.PlayerNotInRoster
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<ResetPlayerPasswordResponse>.Ok(
            result.Value!, localizer["Campaign.PlayerPasswordReset"]));
    }
}
