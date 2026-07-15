namespace AgileFlow.Application.DTOs.Auth;

/// <summary>Payload for POST /api/auth/register</summary>
public sealed record RegisterRequestDto(
    string FirstName,
    string LastName,
    string Email,
    string Password
);

/// <summary>Payload for POST /api/auth/login</summary>
public sealed record LoginRequestDto(
    string Email,
    string Password
);

/// <summary>Payload for POST /api/auth/refresh</summary>
public sealed record RefreshRequestDto(
    string AccessToken,
    string RefreshToken
);

/// <summary>Payload for POST /api/auth/logout</summary>
public sealed record LogoutRequestDto(
    string RefreshToken
);

/// <summary>
/// Returned by register, login, and refresh.
/// Login and refresh return this after full authentication succeeds.
/// </summary>
public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string Role
);

/// <summary>
/// Returned by POST /api/auth/register.
/// Registration no longer issues tokens — the user must confirm their email first.
/// </summary>
public sealed record RegisterResponseDto(
    string UserId,
    string Email,
    bool RequiresEmailConfirmation,
    string Message
);

/// <summary>Returned by GET /api/auth/confirm-email.</summary>
public sealed record ConfirmEmailResponseDto(
    string Email,
    bool Confirmed,
    string Message
);

/// <summary>Payload for POST /api/auth/resend-confirmation.</summary>
public sealed record ResendEmailConfirmationRequestDto(
    string Email
);
