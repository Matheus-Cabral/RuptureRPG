using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Campaigns;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/gamemaster")]
[Authorize(Roles = "GameMaster")]
public class GameMasterController(ICampaignService campaignService) : ControllerBase
{
    [HttpGet("players")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PlayerRosterResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Players(CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.GetRosterAsync(gameMasterId, ct);

        return Ok(ApiResponse<IEnumerable<PlayerRosterResponse>>.Ok(result.Value!));
    }
}
