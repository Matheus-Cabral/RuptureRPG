using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ruptura.API.Resources;
using Ruptura.Application.Interfaces;
using Ruptura.Shared.Auth;
using Ruptura.Shared.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ruptura.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    IStringLocalizer<SharedResources> localizer,
    IValidator<LoginRequest> loginValidator,
    IValidator<RegisterRequest> registerValidator,
    IValidator<RegisterPlayerRequest> registerPlayerValidator) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var validation = await loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var result = await authService.LoginAsync(request, ct);
        if (result.IsFailure)
            return Unauthorized(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<AuthResponse>.Ok(result.Value!, localizer["Auth.LoggedIn"]));
    }

    [HttpPost("register/gamemaster")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterGameMaster(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var validation = await registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var result = await authService.RegisterGameMasterAsync(request, ct);
        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<AuthResponse>.Ok(result.Value!, localizer["Auth.GameMasterRegistered"]));
    }

    [HttpPost("register/player")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterPlayer(
        [FromBody] RegisterPlayerRequest request,
        CancellationToken ct)
    {
        var validation = await registerPlayerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(ApiResponse.Fail(
                localizer["Error.ValidationFailed"],
                validation.Errors.Select(e => e.ErrorMessage).ToArray()));

        var result = await authService.RegisterPlayerAsync(request, ct);
        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<AuthResponse>.Ok(result.Value!, localizer["Auth.PlayerRegistered"]));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        var result = await authService.RefreshTokenAsync(request, ct);
        if (result.IsFailure)
            return Unauthorized(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse<AuthResponse>.Ok(result.Value!));
    }

    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        var refreshToken = Request.Headers["X-Refresh-Token"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest(ApiResponse.Fail(localizer["Auth.InvalidRefreshToken"]));

        var result = await authService.RevokeTokenAsync(refreshToken, ct);
        if (result.IsFailure)
            return BadRequest(ApiResponse.Fail(localizer[result.Error!]));

        return Ok(ApiResponse.Ok(localizer["Auth.LoggedOut"]));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfo>), StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var user = new UserInfo
        {
            Id = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!),
            DisplayName = User.FindFirstValue("name")!,
            Email = User.FindFirstValue(JwtRegisteredClaimNames.Email)!,
            Role = User.FindFirstValue("role")!
        };

        return Ok(ApiResponse<UserInfo>.Ok(user));
    }
}
