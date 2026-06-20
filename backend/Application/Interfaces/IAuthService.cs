using AgileFlow.Application.DTOs.Auth;

namespace AgileFlow.Application.Interfaces;

/// <summary>
/// Handles registration, login, token refresh, and logout.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Rotates the refresh token: validates the old one, issues a new access + refresh pair.
    /// </summary>
    Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request);

    /// <summary>Revokes a refresh token, effectively logging the user out.</summary>
    Task LogoutAsync(string refreshToken);
}
