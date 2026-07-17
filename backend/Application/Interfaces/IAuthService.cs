using AgileFlow.Application.DTOs.Auth;
using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.Interfaces;

/// <summary>
/// Handles registration, login, token refresh, logout, and email confirmation.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Creates a new user and sends a verification email.
    /// Does NOT issue tokens — the user must confirm their email before logging in.
    /// </summary>
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Returns tokens on success. Throws <see cref="Application.Exceptions.EmailNotVerifiedException"/>
    /// when credentials are valid but the email has not been confirmed.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Rotates the refresh token: validates the old one, issues a new access + refresh pair.
    /// </summary>
    Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request);

    /// <summary>Revokes a refresh token, effectively logging the user out.</summary>
    Task LogoutAsync(string refreshToken);

    /// <summary>
    /// Confirms the user's email address using the token generated during registration.
    /// Returns a safe response for both success and failure (prevents token oracle attacks).
    /// </summary>
    Task<ConfirmEmailResponseDto> ConfirmEmailAsync(string userId, string token);

    /// <summary>
    /// Generates a new confirmation token and re-sends the verification email.
    /// Silently succeeds when the email is not found to prevent account enumeration.
    /// </summary>
    Task ResendConfirmationAsync(string email);

    /// <summary>
    /// Development/test helper that marks a user as email-confirmed without requiring SMTP access.
    /// The API endpoint that calls this must be disabled outside Development.
    /// </summary>
    Task<ConfirmEmailResponseDto> ConfirmEmailForDevelopmentAsync(string email);

    /// <summary>
    /// Issues a JWT + refresh token pair for an already authenticated user.
    /// Used by external OAuth login after Identity linking succeeds.
    /// </summary>
    Task<AuthResponseDto> CreateSessionForUserAsync(AppUser user);
}
