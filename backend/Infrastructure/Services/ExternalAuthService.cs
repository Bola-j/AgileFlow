using AgileFlow.Application.Constants;
using AgileFlow.Application.DTOs.Auth;
using AgileFlow.Application.Exceptions;
using AgileFlow.Application.Interfaces;
using AgileFlow.Application.Models;
using AgileFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgileFlow.Infrastructure.Services;

/// <summary>
/// Links Google/GitHub OAuth logins to ASP.NET Identity users and reuses the existing JWT session flow.
/// </summary>
public sealed class ExternalAuthService(
    UserManager<AppUser> userManager,
    IOAuthProviderService oauthProviderService,
    IAuthService authService,
    IConfiguration configuration,
    ILogger<ExternalAuthService> logger) : IExternalAuthService
{
    public Task<AuthResponseDto> GoogleLoginAsync(OAuthLoginRequestDto request) =>
        ExternalLoginAsync(OAuthProviders.Google, oauthProviderService.GetGoogleUserAsync, request);

    public Task<AuthResponseDto> GitHubLoginAsync(OAuthLoginRequestDto request) =>
        ExternalLoginAsync(OAuthProviders.GitHub, oauthProviderService.GetGitHubUserAsync, request);

    private async Task<AuthResponseDto> ExternalLoginAsync(
        string provider,
        Func<string, string, Task<ExternalUserInfo>> fetchProfile,
        OAuthLoginRequestDto request)
    {
        ValidateRequest(request);

        var profile = await fetchProfile(request.Code, request.RedirectUri);
        var loginInfo = new UserLoginInfo(provider, profile.ProviderKey, provider);

        var user = await userManager.FindByLoginAsync(provider, profile.ProviderKey);
        if (user is not null)
        {
            await EnsureEmailConfirmedAsync(user);
            return await authService.CreateSessionForUserAsync(user);
        }

        var existingByEmail = await userManager.FindByEmailAsync(profile.Email);
        if (existingByEmail is not null)
        {
            await EnsureProviderNotLinkedToAnotherAccountAsync(provider, profile.ProviderKey, existingByEmail.Id);
            await LinkExternalLoginAsync(existingByEmail, loginInfo, profile);
            await EnsureEmailConfirmedAsync(existingByEmail);
            return await authService.CreateSessionForUserAsync(existingByEmail);
        }

        user = new AppUser(
            firstName: profile.FirstName,
            lastName: profile.LastName,
            email: profile.Email,
            profilePicture: profile.ProfilePicture,
            githubUsername: profile.GithubUsername);

        user.UserName = profile.Email;
        user.EmailConfirmed = true;

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            logger.LogWarning(
                "Failed to create external user for {Provider} {Email}: {Errors}",
                provider,
                profile.Email,
                string.Join("; ", createResult.Errors.Select(e => e.Description)));

            throw new InvalidOperationException(
                string.Join("; ", createResult.Errors.Select(e => e.Description)));
        }

        await LinkExternalLoginAsync(user, loginInfo, profile);
        return await authService.CreateSessionForUserAsync(user);
    }

    private void ValidateRequest(OAuthLoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("Authorization code is required.");

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
            throw new ArgumentException("Redirect URI is required.");

        if (!Uri.TryCreate(request.RedirectUri, UriKind.Absolute, out var redirectUri))
        {
            throw new ArgumentException("Redirect URI must be an absolute URL.");
        }

        if (redirectUri.Scheme != Uri.UriSchemeHttps && !redirectUri.IsLoopback)
        {
            throw new ArgumentException("Redirect URI must use HTTPS outside local development.");
        }

        var allowedRedirectUris = configuration
            .GetSection("OAuth:AllowedRedirectUris")
            .GetChildren()
            .Select(setting => setting.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!);

        if (!allowedRedirectUris.Contains(request.RedirectUri, StringComparer.Ordinal))
        {
            throw new ArgumentException("The OAuth redirect URI is not allowed.");
        }
    }

    private async Task EnsureProviderNotLinkedToAnotherAccountAsync(
        string provider,
        string providerKey,
        string expectedUserId)
    {
        var linkedUser = await userManager.FindByLoginAsync(provider, providerKey);
        if (linkedUser is not null && linkedUser.Id != expectedUserId)
        {
            throw new DuplicateExternalLoginException(
                "This external account is already linked to another AgileFlow user.");
        }
    }

    private async Task LinkExternalLoginAsync(AppUser user, UserLoginInfo loginInfo, ExternalUserInfo profile)
    {
        var existingLogins = await userManager.GetLoginsAsync(user);
        if (existingLogins.Any(l =>
                l.LoginProvider == loginInfo.LoginProvider &&
                l.ProviderKey == loginInfo.ProviderKey))
        {
            return;
        }

        var result = await userManager.AddLoginAsync(user, loginInfo);
        if (!result.Succeeded)
        {
            var duplicateLogin = result.Errors.Any(e => e.Code is "LoginAlreadyAssociated");
            if (duplicateLogin)
            {
                throw new DuplicateExternalLoginException(
                    "This external account is already linked to another AgileFlow user.");
            }

            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (!string.IsNullOrWhiteSpace(profile.GithubUsername)
            && string.IsNullOrWhiteSpace(user.Github_Username))
        {
            user.SetGithubUsername(profile.GithubUsername);
            await userManager.UpdateAsync(user);
        }
    }

    private async Task EnsureEmailConfirmedAsync(AppUser user)
    {
        if (user.EmailConfirmed)
            return;

        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);
    }
}
