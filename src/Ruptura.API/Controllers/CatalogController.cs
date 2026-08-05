using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Common;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Catalog;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/catalog")]
[Authorize]
public class CatalogController(
    ICatalogEntryService catalogService,
    IStringLocalizer<SharedResources> localizer,
    IValidator<CreateCatalogEntryRequest> createValidator,
    IValidator<UpdateCatalogEntryRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CatalogEntryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByType(
        [FromQuery] string type, [FromQuery] Guid campaignId, [FromQuery] bool includeArchived, CancellationToken ct)
    {
        var callerId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await catalogService.GetByTypeAsync(callerId, type, campaignId, includeArchived, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Catalog.InvalidType
                ? BadRequest(ApiResponse.Fail(localizer[result.Error!]))
                : NotFound(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<IEnumerable<CatalogEntryResponse>>.Ok(result.Value!));
    }

    [HttpPost]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<CatalogEntryResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateCatalogEntryRequest request, CancellationToken ct)
    {
        var validation = await createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await catalogService.CreateAsync(gameMasterId, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Catalog.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<CatalogEntryResponse>.Ok(result.Value!, localizer["Catalog.Created"]));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse<CatalogEntryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCatalogEntryRequest request, CancellationToken ct)
    {
        var validation = await updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await catalogService.UpdateAsync(gameMasterId, id, request, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Catalog.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<CatalogEntryResponse>.Ok(result.Value!, localizer["Catalog.Updated"]));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "GameMaster")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var gameMasterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await catalogService.DeleteAsync(gameMasterId, id, ct);
        if (result.IsFailure)
            return result.Error == ErrorCodes.Catalog.NotFound
                ? NotFound(ApiResponse.Fail(localizer[result.Error!]))
                : BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse.Ok(localizer["Catalog.Deleted"]));
    }
}
