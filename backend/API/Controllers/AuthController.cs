using AgileFlow.Application.DTOs.Auth;
using AgileFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileFlow.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Register a new user. Sends a verification email and returns a lightweight response.
    /// Tokens are NOT issued here — the user must confirm their email before logging in.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var response = await authService.RegisterAsync(request);
        return CreatedAtAction(nameof(Register), response);
    }

    /// <summary>
    /// Login with email + password. Returns access + refresh tokens.
    /// Returns 403 with requiresEmailConfirmation:true when the email is unverified.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Confirms the user's email address using the token from the verification email.
    /// Returns a safe response for both success and failure.
    /// </summary>
    [HttpGet("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ConfirmEmailResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string userId,
        [FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "userId and token are required." });

        var response = await authService.ConfirmEmailAsync(userId, token);
        return Ok(response);
    }

    /// <summary>
    /// Resends the email-confirmation email.
    /// Always returns 204 to prevent account enumeration.
    /// </summary>
    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResendConfirmation(
        [FromBody] ResendEmailConfirmationRequestDto request)
    {
        await authService.ResendConfirmationAsync(request.Email);
        return NoContent();
    }

    /// <summary>
    /// Exchange an expired access token + valid refresh token for a fresh pair.
    /// The old refresh token is revoked immediately (token rotation).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
    {
        var response = await authService.RefreshAsync(request);
        return Ok(response);
    }

    /// <summary>Revoke the given refresh token, effectively logging the user out.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        await authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }
}
