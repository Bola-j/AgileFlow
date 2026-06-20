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

/// <summary>Returned by register, login, and refresh.</summary>
public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string Role
);
