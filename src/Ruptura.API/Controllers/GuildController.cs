using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Common;
using Ruptura.Shared.Guilds;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class GuildController(
    IGuildSheetService guildService,
    IStringLocalizer<SharedResources> localizer,
    IValidator<UpdateGuildSheetRequest> updateValidator) : ControllerBase
{
    [HttpGet("campaigns/{campaignId:guid}/guild")]
    [ProducesResponseType(typeof(ApiResponse<GuildSheetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid campaignId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.GetByCampaignAsync(callerId, campaignId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));
        return Ok(ApiResponse<GuildSheetResponse>.Ok(result.Value!));
    }

    [HttpPut("campaigns/{campaignId:guid}/guild")]
    [ProducesResponseType(typeof(ApiResponse<GuildSheetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid campaignId, [FromBody] UpdateGuildSheetRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.UpdateAsync(callerId, campaignId, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Guild.Conflict
                ? Conflict(ApiResponse.Fail(localizer[result.Error!]))
                : NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<GuildSheetResponse>.Ok(result.Value!, localizer["Guild.Saved"]));
    }
}
