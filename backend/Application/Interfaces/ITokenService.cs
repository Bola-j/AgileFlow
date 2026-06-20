using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Interfaces;

/// <summary>
/// Generates and validates JWT access tokens and refresh tokens.
/// </summary>
public interface ITokenService
{
    /// <summary>Generates a signed JWT for the given user with their workspace role claim.</summary>
    string GenerateAccessToken(AppUser user, string role);

    /// <summary>Generates a cryptographically random refresh token string.</summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Reads the ClaimsPrincipal from an expired access token without validating lifetime.
    /// Used during refresh to extract the userId claim safely.
    /// </summary>
    System.Security.Claims.ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
