using AgileFlow.Application.Models;

namespace AgileFlow.Application.Interfaces;

/// <summary>Exchanges an OAuth authorization code for a normalized user profile.</summary>
public interface IOAuthProviderService
{
    Task<ExternalUserInfo> GetGoogleUserAsync(string code, string redirectUri);

    Task<ExternalUserInfo> GetGitHubUserAsync(string code, string redirectUri);
}
