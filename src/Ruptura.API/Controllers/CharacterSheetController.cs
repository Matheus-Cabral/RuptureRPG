using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.CharacterSheets;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class CharacterSheetController(
    ICharacterSheetService characterSheetService,
    INotificationService notificationService,
    IStringLocalizer<SharedResources> localizer,
    IValidator<GrantCharacterSheetRequest> grantValidator,
    IValidator<UpdateCharacterSheetRequest> updateValidator) : ControllerBase
{
    [HttpPost("campaigns/{campaignId:guid}/character-sheets")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<CharacterSheetResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Grant(
        Guid campaignId, [FromBody] GrantCharacterSheetRequest request, CancellationToken ct)
    {
        var validation = await grantValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.CreateAsync(gameMasterId, campaignId, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.CharacterSheet.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CharacterSheetResponse>.Ok(result.Value!, localizer["CharacterSheet.Granted"]));
    }

    [HttpGet("campaigns/{campaignId:guid}/character-sheets")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CharacterSheetResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCampaign(Guid campaignId, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.GetByCampaignAsync(gameMasterId, campaignId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<IEnumerable<CharacterSheetResponse>>.Ok(result.Value!));
    }

    [HttpGet("campaigns/{campaignId:guid}/character-sheets/mine")]
    [ProducesResponseType(typeof(ApiResponse<CharacterSheetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMine(Guid campaignId, CancellationToken ct)
    {
        var playerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.GetMineAsync(playerId, campaignId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<CharacterSheetResponse>.Ok(result.Value!));
    }

    [HttpGet("character-sheets/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CharacterSheetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.GetAsync(callerId, id, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<CharacterSheetResponse>.Ok(result.Value!));
    }

    [HttpPut("character-sheets/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CharacterSheetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCharacterSheetRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await characterSheetService.UpdateAsync(callerId, id, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.CharacterSheet.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        try
        {
            await notificationService.CheckAndCreateRankPromotionNotificationAsync(
                result.Value!.CampaignId, result.Value.Id,
                result.Value.Data.GuildRegistry.Ranking, result.Value.DerivedStats.Np, ct);
        }
        catch (Exception)
        {
            // The sheet save above already succeeded and committed. This check is advisory —
            // a transient fault here (e.g. a DB blip) must never turn an already-successful
            // save into an apparent failure for the client.
        }

        return Ok(ApiResponse<CharacterSheetResponse>.Ok(result.Value!, localizer["CharacterSheet.Updated"]));
    }
}
