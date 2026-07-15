using System.Security.Claims;
using AgileFlow.Application.DTOs.Auth;
using AgileFlow.Application.Exceptions;
using AgileFlow.Application.Interfaces;
using AgileFlow.Domain.Entities;
using AgileFlow.Infrastructure.Persistence.Data;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgileFlow.Infrastructure.Services;

/// <summary>
/// Register, login, refresh token (with rotation), logout, and email confirmation.
/// Registration creates the account and sends a verification email but does NOT
/// issue tokens — the user must confirm their email before logging in.
/// </summary>
public sealed class AuthService(
    UserManager<AppUser> userManager,
    AgileFlowDbContext context,
    ITokenService tokenService,
    IEmailSender emailSender,
    IEmailNotificationLogRepository emailLogRepository,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(7);

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new AppUser(request.FirstName, request.LastName, request.Email);
        user.UserName = request.Email;

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));

        // Generate confirmation token and send verification email (fire-and-forget style — failure is logged)
        await SendVerificationEmailAsync(user);

        return new RegisterResponseDto(
            UserId: user.Id,
            Email: user.Email!,
            RequiresEmailConfirmation: true,
            Message: "Account created. Please check your email to verify your address before logging in.");
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Block login until email is confirmed
        if (!user.EmailConfirmed)
            throw new EmailNotVerifiedException(user.Email!);

        if (user.IsDeleted)
            throw new UnauthorizedAccessException("This account has been deactivated.");

        var role = await ResolveHighestRoleAsync(user.Id);
        return await IssueTokenPairAsync(user, role);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request)
    {
        ClaimsPrincipal principal;
        try { principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken); }
        catch (Exception ex) { throw new UnauthorizedAccessException("Invalid access token.", ex); }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Token missing subject claim.");

        var stored = await context.RefreshTokens
            .FirstOrDefaultAsync(r =>
                r.Token == request.RefreshToken &&
                r.UserId == userId &&
                !r.IsRevoked &&
                r.ExpiresAt > DateTime.UtcNow);

        if (stored is null)
            throw new UnauthorizedAccessException("Refresh token is invalid or has expired.");

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || user.IsDeleted)
            throw new UnauthorizedAccessException("User account is inactive or no longer exists.");

        stored.Revoke();
        var role = await ResolveHighestRoleAsync(userId);
        var response = await IssueTokenPairAsync(user, role);
        await context.SaveChangesAsync();
        return response;
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var stored = await context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken && !r.IsRevoked);

        if (stored is null) return;
        stored.Revoke();
        await context.SaveChangesAsync();
    }

    public async Task<ConfirmEmailResponseDto> ConfirmEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            // Safe response — do not reveal whether the user exists
            return new ConfirmEmailResponseDto(
                Email: string.Empty,
                Confirmed: false,
                Message: "The confirmation link is invalid or has expired.");
        }

        if (user.EmailConfirmed)
        {
            return new ConfirmEmailResponseDto(
                Email: user.Email!,
                Confirmed: true,
                Message: "Your email address is already confirmed. You can log in.");
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            return new ConfirmEmailResponseDto(
                Email: user.Email!,
                Confirmed: false,
                Message: "The confirmation link is invalid or has expired. Please request a new one.");
        }

        return new ConfirmEmailResponseDto(
            Email: user.Email!,
            Confirmed: true,
            Message: "Email confirmed successfully. You can now log in.");
    }

    public async Task ResendConfirmationAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        // Silently succeed when user is not found to prevent account enumeration
        if (user is null) return;

        // If already confirmed, do nothing
        if (user.EmailConfirmed) return;

        await SendVerificationEmailAsync(user);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task SendVerificationEmailAsync(AppUser user)
    {
        try
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            // URL-encode the token (it may contain + / = characters)
            var encodedToken = Uri.EscapeDataString(token);
            var frontendBase = configuration["Email:Smtp:FrontendBaseUrl"]?.TrimEnd('/')
                               ?? "http://localhost:5500/frontend";

            var confirmUrl = $"{frontendBase}/verify-email.html?userId={user.Id}&token={encodedToken}";

            var subject = "Verify your AgileFlow email address";
            var html = $"""
                <h2>Welcome to AgileFlow!</h2>
                <p>Hi {user.First_Name},</p>
                <p>Thanks for signing up. Please confirm your email address by clicking the button below:</p>
                <p style="text-align:center;margin:24px 0;">
                    <a href="{confirmUrl}"
                       style="background:#4f46e5;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none;font-weight:600;">
                        Confirm Email
                    </a>
                </p>
                <p>Or copy and paste this link into your browser:</p>
                <p><a href="{confirmUrl}">{confirmUrl}</a></p>
                <p>This link will expire according to your token provider settings.</p>
                <br/>
                <p>— The AgileFlow Team</p>
                """;

            await emailSender.SendAsync(user.Email!, subject, html);

            // Audit log
            var log = EmailNotificationLog.CreateSuccess(
                recipientEmail: user.Email!,
                eventType: EmailEventType.EmailVerification,
                deduplicationKey: $"verify:{user.Id}:{DateTime.UtcNow:yyyyMMddHHmm}",
                subject: subject);

            await emailLogRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            // Do not fail registration if the email cannot be sent
            logger.LogError(ex, "Failed to send verification email to {Email}", user.Email);

            try
            {
                var failLog = EmailNotificationLog.CreateFailure(
                    recipientEmail: user.Email!,
                    eventType: EmailEventType.EmailVerification,
                    deduplicationKey: $"verify:{user.Id}:{DateTime.UtcNow:yyyyMMddHHmm}",
                    subject: "Verify your AgileFlow email address",
                    errorMessage: ex.Message);
                await emailLogRepository.AddAsync(failLog);
            }
            catch (Exception logEx)
            {
                logger.LogError(logEx, "Also failed to write EmailNotificationLog for {Email}", user.Email);
            }
        }
    }

    private async Task<AuthResponseDto> IssueTokenPairAsync(AppUser user, string role)
    {
        var expiryMinutes = int.TryParse(
            configuration.GetSection("Jwt")["ExpiryMinutes"], out var m) ? m : 60;

        var accessToken = tokenService.GenerateAccessToken(user, role);
        var refreshValue = tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken(
            token: refreshValue,
            userId: user.Id,
            expiresAt: DateTime.UtcNow.Add(RefreshLifetime));

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync();

        return new AuthResponseDto(
            AccessToken: accessToken,
            RefreshToken: refreshValue,
            ExpiresAt: DateTime.UtcNow.AddMinutes(expiryMinutes),
            UserId: user.Id,
            Email: user.Email!,
            Role: role
        );
    }

    /// <summary>
    /// Highest-privilege role across all workspaces.
    /// Priority: Admin &gt; TeamLead &gt; Developer.
    /// </summary>
    private async Task<string> ResolveHighestRoleAsync(string userId)
    {
        var roles = await context.UserWorkspaces
            .Where(uw => uw.AppUserId == userId)
            .Select(uw => uw.Role)
            .ToListAsync();

        if (!roles.Any()) return nameof(UserRole.Developer);
        if (roles.Any(r => r == UserRole.Admin)) return nameof(UserRole.Admin);
        if (roles.Any(r => r == UserRole.TeamLead)) return nameof(UserRole.TeamLead);
        return nameof(UserRole.Developer);
    }
}
