using FluentValidation;
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
[Route("api/campaigns")]
[Authorize]
public class CampaignController(
    ICampaignService campaignService,
    IStringLocalizer<SharedResources> localizer,
    IValidator<CreateCampaignRequest> createValidator,
    IValidator<AssignMemberRequest> assignValidator) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<CampaignResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.CreateAsync(gameMasterId, request, ct);

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CampaignResponse>.Ok(result.Value!, localizer["Campaign.Created"]));
    }

    [HttpGet]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CampaignResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.GetByGameMasterAsync(gameMasterId, ct);

        return Ok(ApiResponse<IEnumerable<CampaignResponse>>.Ok(result.Value!));
    }

    [HttpGet("{campaignId:guid}/members")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CampaignMemberResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Members(Guid campaignId, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.GetMembersAsync(gameMasterId, campaignId, ct);
        if (result.IsFailure)
            return NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<IEnumerable<CampaignMemberResponse>>.Ok(result.Value!));
    }

    [HttpPost("{campaignId:guid}/members")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<CampaignMemberResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignMember(
        Guid campaignId, [FromBody] AssignMemberRequest request, CancellationToken ct)
    {
        var validation = await assignValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await campaignService.AssignMemberAsync(gameMasterId, campaignId, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Campaign.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CampaignMemberResponse>.Ok(result.Value!, localizer["Campaign.MemberAssigned"]));
    }

    [HttpGet("mine")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CampaignResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Mine(CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var isGameMaster = User.IsInRole("GameMaster");
        var result = await campaignService.GetMyMembershipsAsync(callerId, isGameMaster, ct);

        return Ok(ApiResponse<IEnumerable<CampaignResponse>>.Ok(result.Value!));
    }
}
