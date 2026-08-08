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
            return result.Error switch
            {
                ErrorCodes.Guild.Conflict => Conflict(ApiResponse.Fail(localizer[result.Error!])),
                ErrorCodes.Guild.DoctrineInvalid
                    => BadRequest(ApiResponse.Fail(localizer[result.Error!])),
                _ => NotFound(ApiResponse.Fail(localizer[result.Error!]))
            };

        return Ok(ApiResponse<GuildSheetResponse>.Ok(result.Value!, localizer["Guild.Saved"]));
    }

    [HttpPost("campaigns/{campaignId:guid}/guild/expeditions")]
    [ProducesResponseType(typeof(ApiResponse<ExpeditionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddExpedition(
        Guid campaignId, [FromBody] CreateExpeditionRequest request, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.AddExpeditionAsync(callerId, campaignId, request, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ExpeditionResponse>.Ok(result.Value!));
    }

    [HttpPut("campaigns/{campaignId:guid}/guild/expeditions/{expeditionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ExpeditionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExpedition(
        Guid campaignId, Guid expeditionId, [FromBody] UpdateExpeditionRequest request, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.UpdateExpeditionAsync(callerId, campaignId, expeditionId, request, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));
        return Ok(ApiResponse<ExpeditionResponse>.Ok(result.Value!));
    }

    [HttpDelete("campaigns/{campaignId:guid}/guild/expeditions/{expeditionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExpedition(
        Guid campaignId, Guid expeditionId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.DeleteExpeditionAsync(callerId, campaignId, expeditionId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));
        return Ok(ApiResponse.Ok());
    }

    [HttpPost("campaigns/{campaignId:guid}/guild/buildings")]
    [ProducesResponseType(typeof(ApiResponse<GuildBuildingResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddBuilding(
        Guid campaignId, [FromBody] CreateBuildingRequest request, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.AddBuildingAsync(callerId, campaignId, request, ct);
        if (result.IsFailure)
            return BuildingFailure(result.Error!);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<GuildBuildingResponse>.Ok(result.Value!));
    }

    [HttpPut("campaigns/{campaignId:guid}/guild/buildings/{buildingId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GuildBuildingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBuilding(
        Guid campaignId, Guid buildingId, [FromBody] UpdateBuildingRequest request, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.UpdateBuildingAsync(callerId, campaignId, buildingId, request, ct);
        if (result.IsFailure)
            return BuildingFailure(result.Error!);
        return Ok(ApiResponse<GuildBuildingResponse>.Ok(result.Value!));
    }

    [HttpDelete("campaigns/{campaignId:guid}/guild/buildings/{buildingId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBuilding(
        Guid campaignId, Guid buildingId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.DeleteBuildingAsync(callerId, campaignId, buildingId, ct);
        if (result.IsFailure)
            return BuildingFailure(result.Error!);
        return Ok(ApiResponse.Ok());
    }

    // Validation failures (bad installation/level/duplicate) → 400; missing/cross-guild/non-member → 404.
    private IActionResult BuildingFailure(string error) => error switch
    {
        ErrorCodes.Guild.InstallationInvalid
            or ErrorCodes.Guild.BuildingNotConstructible
            or ErrorCodes.Guild.BuildingLevelInvalid
            or ErrorCodes.Guild.BuildingExists
            => BadRequest(ApiResponse.Fail(localizer[error])),
        _ => NotFound(ApiResponse.Fail(localizer[error]))
    };

    [HttpPost("campaigns/{campaignId:guid}/guild/staff")]
    [ProducesResponseType(typeof(ApiResponse<GuildStaffResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddStaff(
        Guid campaignId, [FromBody] CreateStaffRequest request, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.AddStaffAsync(callerId, campaignId, request, ct);
        if (result.IsFailure)
            return StaffFailure(result.Error!);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<GuildStaffResponse>.Ok(result.Value!));
    }

    [HttpPut("campaigns/{campaignId:guid}/guild/staff/{staffId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GuildStaffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStaff(
        Guid campaignId, Guid staffId, [FromBody] UpdateStaffRequest request, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.UpdateStaffAsync(callerId, campaignId, staffId, request, ct);
        if (result.IsFailure)
            return StaffFailure(result.Error!);
        return Ok(ApiResponse<GuildStaffResponse>.Ok(result.Value!));
    }

    [HttpDelete("campaigns/{campaignId:guid}/guild/staff/{staffId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStaff(
        Guid campaignId, Guid staffId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.DeleteStaffAsync(callerId, campaignId, staffId, ct);
        if (result.IsFailure)
            return StaffFailure(result.Error!);
        return Ok(ApiResponse.Ok());
    }

    // Unknown Kind → 400; missing/cross-guild/non-member → 404.
    private IActionResult StaffFailure(string error) => error switch
    {
        ErrorCodes.Guild.StaffKindInvalid => BadRequest(ApiResponse.Fail(localizer[error])),
        _ => NotFound(ApiResponse.Fail(localizer[error]))
    };

    [HttpPost("campaigns/{campaignId:guid}/guild/research")]
    [ProducesResponseType(typeof(ApiResponse<ResearchProjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddResearch(
        Guid campaignId, [FromBody] CreateResearchProjectRequest request, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.AddResearchAsync(callerId, campaignId, request, ct);
        if (result.IsFailure)
            return ResearchFailure(result.Error!);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ResearchProjectResponse>.Ok(result.Value!));
    }

    [HttpPut("campaigns/{campaignId:guid}/guild/research/{researchId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ResearchProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateResearch(
        Guid campaignId, Guid researchId, [FromBody] UpdateResearchProjectRequest request, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.UpdateResearchAsync(callerId, campaignId, researchId, request, ct);
        if (result.IsFailure)
            return ResearchFailure(result.Error!);
        return Ok(ApiResponse<ResearchProjectResponse>.Ok(result.Value!));
    }

    [HttpDelete("campaigns/{campaignId:guid}/guild/research/{researchId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteResearch(
        Guid campaignId, Guid researchId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.DeleteResearchAsync(callerId, campaignId, researchId, ct);
        if (result.IsFailure)
            return ResearchFailure(result.Error!);
        return Ok(ApiResponse.Ok());
    }

    // Invalid Complexity/Stage → 400; missing/cross-guild/non-member → 404.
    private IActionResult ResearchFailure(string error) => error switch
    {
        ErrorCodes.Guild.ResearchComplexityInvalid
            or ErrorCodes.Guild.ResearchStageInvalid
            => BadRequest(ApiResponse.Fail(localizer[error])),
        _ => NotFound(ApiResponse.Fail(localizer[error]))
    };

    [HttpPost("campaigns/{campaignId:guid}/guild/crafting")]
    [ProducesResponseType(typeof(ApiResponse<CraftingOrderResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCrafting(
        Guid campaignId, [FromBody] CreateCraftingOrderRequest request, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.AddCraftingAsync(callerId, campaignId, request, ct);
        if (result.IsFailure)
            return CraftingFailure(result.Error!);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CraftingOrderResponse>.Ok(result.Value!));
    }

    [HttpPut("campaigns/{campaignId:guid}/guild/crafting/{craftingId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CraftingOrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCrafting(
        Guid campaignId, Guid craftingId, [FromBody] UpdateCraftingOrderRequest request, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.UpdateCraftingAsync(callerId, campaignId, craftingId, request, ct);
        if (result.IsFailure)
            return CraftingFailure(result.Error!);
        return Ok(ApiResponse<CraftingOrderResponse>.Ok(result.Value!));
    }

    [HttpDelete("campaigns/{campaignId:guid}/guild/crafting/{craftingId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCrafting(
        Guid campaignId, Guid craftingId, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await guildService.DeleteCraftingAsync(callerId, campaignId, craftingId, ct);
        if (result.IsFailure)
            return CraftingFailure(result.Error!);
        return Ok(ApiResponse.Ok());
    }

    // Invalid Category/Status → 400; missing/cross-guild/non-member → 404.
    private IActionResult CraftingFailure(string error) => error switch
    {
        ErrorCodes.Guild.CraftingCategoryInvalid
            or ErrorCodes.Guild.CraftingStatusInvalid
            => BadRequest(ApiResponse.Fail(localizer[error])),
        _ => NotFound(ApiResponse.Fail(localizer[error]))
    };
}
