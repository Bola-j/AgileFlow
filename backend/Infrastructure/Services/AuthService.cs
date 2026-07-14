using System.Security.Claims;
using AgileFlow.Application.DTOs.Auth;
using AgileFlow.Application.Interfaces;
using AgileFlow.Domain.Entities;
using AgileFlow.Infrastructure.Persistence.Data;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AgileFlow.Infrastructure.Services;

/// <summary>
/// Register, login, refresh token (with rotation), and logout.
/// Uses AppUser's domain constructor: AppUser(firstName, lastName, email, ...).
/// Reads role from UserWorkspace.Role (Domain.Enums.UserRole).
/// </summary>
public sealed class AuthService(
    UserManager<AppUser> userManager,
    AgileFlowDbContext context,
    ITokenService tokenService,
    IConfiguration configuration) : IAuthService
{
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(7);

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        // Use the domain constructor — AppUser(firstName, lastName, email)
        var user = new AppUser(request.FirstName, request.LastName, request.Email);
        user.UserName = request.Email;

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));

        // New users have no workspace yet — default to Developer
        return await IssueTokenPairAsync(user, nameof(UserRole.Developer));
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var role = await ResolveHighestRoleAsync(user.Id);
        return await IssueTokenPairAsync(user, role);
    }

    //public async Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request)
    //{
    //    ClaimsPrincipal principal;
    //    try { principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken); }
    //    catch { throw new UnauthorizedAccessException("Invalid access token."); }

    //    var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
    //        ?? throw new UnauthorizedAccessException("Token missing subject claim.");

    //    // Find a valid, non-revoked refresh token for this user
    //    var stored = await context.RefreshTokens
    //        .Include(r => r.AppUser)
    //        .FirstOrDefaultAsync(r =>
    //            r.Token == request.RefreshToken &&
    //            r.UserId == userId &&
    //            !r.IsRevoked &&
    //            r.ExpiresAt > DateTime.UtcNow);

    //    if (stored is null)
    //        throw new UnauthorizedAccessException("Refresh token is invalid or has expired.");

    //    // Rotate: revoke the consumed token
    //    stored.Revoke();

    //    var role = await ResolveHighestRoleAsync(userId);
    //    var response = await IssueTokenPairAsync(stored.AppUser, role);

    //    await context.SaveChangesAsync();
    //    return response;
    //}

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

    // ── Helpers ──────────────────────────────────────────────────────────────

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
    /// Priority: Admin > TeamLead > Developer.
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
