using Application.DTOs.Account;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("me")]
    public async Task<ActionResult<AccountResponse>> GetMe()
    {
        var account = await _accountService.GetMeAsync(UserId);
        if (account is null)
            return NotFound(new { message = "Authenticated user was not found." });

        return Ok(account);
    }

    [HttpPut("me")]
    public async Task<ActionResult<AccountResponse>> UpdateMe([FromBody] UpdateAccountRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var account = await _accountService.UpdateMeAsync(UserId, request);
            if (account is null)
                return NotFound(new { message = "Authenticated user was not found." });

            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var success = await _accountService.ChangePasswordAsync(UserId, request);
            if (!success)
                return NotFound(new { message = "Authenticated user was not found." });

            return Ok(new { message = "Password updated successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("change-email")]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var success = await _accountService.ChangeEmailAsync(UserId, request);
            if (!success)
                return NotFound(new { message = "Authenticated user was not found." });

            return Ok(new { message = "Email updated successfully. Please use your new email for future logins." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

}
