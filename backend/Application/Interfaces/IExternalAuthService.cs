using AgileFlow.Application.DTOs.Auth;

namespace AgileFlow.Application.Interfaces;

/// <summary>Links external OAuth logins to Identity users and issues application tokens.</summary>
public interface IExternalAuthService
{
    Task<AuthResponseDto> GoogleLoginAsync(OAuthLoginRequestDto request);

    Task<AuthResponseDto> GitHubLoginAsync(OAuthLoginRequestDto request);
}
