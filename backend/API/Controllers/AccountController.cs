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
    private readonly IWebHostEnvironment _environment;

    public AccountController(IAccountService accountService, IWebHostEnvironment environment)
    {
        _accountService = accountService;
        _environment = environment;
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

        var account = await _accountService.UpdateMeAsync(UserId, request);
        if (account is null)
            return NotFound(new { message = "Authenticated user was not found." });

        return Ok(account);
    }

    [HttpPost("me/profile-picture")]
    [RequestSizeLimit(5_242_880)]
    public async Task<ActionResult<AccountResponse>> UploadProfilePicture([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Profile picture file is required." });

        if (file.Length > 5_242_880)
            return BadRequest(new { message = "Profile picture cannot exceed 5 MB." });

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only image files can be uploaded." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".gif"
        };

        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Allowed image formats are JPG, PNG, WEBP, and GIF." });

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var uploadDirectory = Path.Combine(webRoot, "uploads", "profile-pictures");
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{UserId}-{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var pictureUrl = $"/uploads/profile-pictures/{fileName}";
        var account = await _accountService.UpdateProfilePictureAsync(UserId, pictureUrl);
        if (account is null)
            return NotFound(new { message = "Authenticated user was not found." });

        return Ok(account);
    }
}
