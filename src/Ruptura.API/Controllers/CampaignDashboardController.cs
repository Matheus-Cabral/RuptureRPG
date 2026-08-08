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
[Authorize(Roles = "GameMaster")]
public class CampaignDashboardController(
    ICampaignDashboardService dashboardService,
    IStringLocalizer<SharedResources> localizer) : ControllerBase
{
    [HttpGet("api/campaigns/{campaignId:guid}/dashboard")]
    [ProducesResponseType(typeof(ApiResponse<CampaignDashboardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid campaignId, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await dashboardService.GetAsync(gameMasterId, campaignId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<CampaignDashboardResponse>.Ok(result.Value!));
    }

    [HttpPut("api/campaigns/{campaignId:guid}/dashboard/dungeon")]
    [ProducesResponseType(typeof(ApiResponse<CampaignDashboardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDungeon(
        Guid campaignId, [FromBody] UpdateDungeonStateRequest request, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await dashboardService.UpdateDungeonAsync(gameMasterId, campaignId, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Campaign.FloorStateInvalid
                ? BadRequest(ApiResponse.Fail(localizer[result.Error!]))
                : NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<CampaignDashboardResponse>.Ok(result.Value!));
    }
}
