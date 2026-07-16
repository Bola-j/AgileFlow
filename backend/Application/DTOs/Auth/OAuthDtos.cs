namespace AgileFlow.Application.DTOs.Auth;

/// <summary>Payload for POST /api/auth/google and POST /api/auth/github.</summary>
public sealed record OAuthLoginRequestDto(
    string Code,
    string RedirectUri
);
