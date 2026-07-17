using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgileFlow.Application.Constants;
using AgileFlow.Application.Exceptions;
using AgileFlow.Application.Interfaces;
using AgileFlow.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgileFlow.Infrastructure.Services;

/// <summary>Exchanges OAuth authorization codes and fetches provider user profiles.</summary>
public sealed class OAuthProviderService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<OAuthProviderService> logger) : IOAuthProviderService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public Task<ExternalUserInfo> GetGoogleUserAsync(string code, string redirectUri) =>
        ExchangeAndFetchProfileAsync(
            provider: OAuthProviders.Google,
            tokenEndpoint: "https://oauth2.googleapis.com/token",
            buildTokenRequest: () => BuildGoogleTokenRequest(code, redirectUri),
            fetchProfile: FetchGoogleProfileAsync);

    public Task<ExternalUserInfo> GetGitHubUserAsync(string code, string redirectUri) =>
        ExchangeAndFetchProfileAsync(
            provider: OAuthProviders.GitHub,
            tokenEndpoint: "https://github.com/login/oauth/access_token",
            buildTokenRequest: () => BuildGitHubTokenRequest(code, redirectUri),
            fetchProfile: FetchGitHubProfileAsync);

    private async Task<ExternalUserInfo> ExchangeAndFetchProfileAsync(
        string provider,
        string tokenEndpoint,
        Func<HttpRequestMessage> buildTokenRequest,
        Func<string, Task<ExternalUserInfo>> fetchProfile)
    {
        try
        {
            using var tokenRequest = buildTokenRequest();
            using var tokenResponse = await httpClient.SendAsync(tokenRequest);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorBody = await tokenResponse.Content.ReadAsStringAsync();
                logger.LogWarning(
                    "{Provider} token exchange failed with status {StatusCode}: {Body}",
                    provider,
                    tokenResponse.StatusCode,
                    errorBody);
                throw new OAuthAuthenticationException("Invalid or expired authorization code.");
            }

            var tokenPayload = await tokenResponse.Content.ReadFromJsonAsync<OAuthTokenResponse>(JsonOptions);
            if (string.IsNullOrWhiteSpace(tokenPayload?.AccessToken))
            {
                var error = tokenPayload?.ErrorDescription ?? tokenPayload?.Error ?? "missing access token";
                logger.LogWarning("{Provider} token exchange returned no access token: {Error}", provider, error);
                throw new OAuthAuthenticationException("Invalid or expired authorization code.");
            }

            return await fetchProfile(tokenPayload.AccessToken);
        }
        catch (OAuthAuthenticationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Provider} OAuth exchange failed.", provider);
            throw new OAuthAuthenticationException("OAuth provider authentication failed.", ex);
        }
    }

    private HttpRequestMessage BuildGoogleTokenRequest(string code, string redirectUri)
    {
        var google = configuration.GetSection("Google");
        var clientId = google["ClientId"] ?? throw new InvalidOperationException("Google:ClientId is not configured.");
        var clientSecret = google["ClientSecret"] ?? throw new InvalidOperationException("Google:ClientSecret is not configured.");

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code",
        });

        return new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = content,
        };
    }

    private HttpRequestMessage BuildGitHubTokenRequest(string code, string redirectUri)
    {
        var github = configuration.GetSection("GitHub");
        var clientId = github["ClientId"] ?? throw new InvalidOperationException("GitHub:ClientId is not configured.");
        var clientSecret = github["ClientSecret"] ?? throw new InvalidOperationException("GitHub:ClientSecret is not configured.");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
        {
            Content = JsonContent.Create(new
            {
                client_id = clientId,
                client_secret = clientSecret,
                code,
                redirect_uri = redirectUri,
            }),
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private async Task<ExternalUserInfo> FetchGoogleProfileAsync(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new OAuthAuthenticationException("Failed to fetch Google user profile.");

        var profile = await response.Content.ReadFromJsonAsync<GoogleUserResponse>(JsonOptions)
            ?? throw new OAuthAuthenticationException("Failed to fetch Google user profile.");

        if (string.IsNullOrWhiteSpace(profile.Id))
            throw new OAuthAuthenticationException("Google profile is missing a user identifier.");

        if (string.IsNullOrWhiteSpace(profile.Email))
            throw new OAuthMissingEmailException();

        var (firstName, lastName) = SplitName(profile.GivenName, profile.FamilyName, profile.Name, profile.Email);

        return new ExternalUserInfo(
            ProviderKey: profile.Id,
            Email: profile.Email.Trim(),
            FirstName: firstName,
            LastName: lastName,
            ProfilePicture: profile.Picture,
            GithubUsername: null);
    }

    private async Task<ExternalUserInfo> FetchGitHubProfileAsync(string accessToken)
    {
        using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        userRequest.Headers.UserAgent.ParseAdd("AgileFlow");

        using var userResponse = await httpClient.SendAsync(userRequest);
        if (!userResponse.IsSuccessStatusCode)
            throw new OAuthAuthenticationException("Failed to fetch GitHub user profile.");

        var profile = await userResponse.Content.ReadFromJsonAsync<GitHubUserResponse>(JsonOptions)
            ?? throw new OAuthAuthenticationException("Failed to fetch GitHub user profile.");

        if (profile.Id is null or 0)
            throw new OAuthAuthenticationException("GitHub profile is missing a user identifier.");

        var email = profile.Email;
        if (string.IsNullOrWhiteSpace(email))
            email = await FetchGitHubPrimaryEmailAsync(accessToken);

        if (string.IsNullOrWhiteSpace(email))
            throw new OAuthMissingEmailException();

        var displayName = profile.Name ?? profile.Login ?? email;
        var (firstName, lastName) = SplitName(null, null, displayName, email);

        return new ExternalUserInfo(
            ProviderKey: profile.Id.Value.ToString(),
            Email: email.Trim(),
            FirstName: firstName,
            LastName: lastName,
            ProfilePicture: profile.AvatarUrl,
            GithubUsername: profile.Login);
    }

    private async Task<string?> FetchGitHubPrimaryEmailAsync(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.ParseAdd("AgileFlow");

        using var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var emails = await response.Content.ReadFromJsonAsync<List<GitHubEmailResponse>>(JsonOptions);
        if (emails is null || emails.Count == 0)
            return null;

        return emails
            .Where(e => e.Verified && !string.IsNullOrWhiteSpace(e.Email))
            .OrderByDescending(e => e.Primary)
            .Select(e => e.Email)
            .FirstOrDefault();
    }

    private static (string FirstName, string LastName) SplitName(
        string? givenName,
        string? familyName,
        string? displayName,
        string email)
    {
        if (!string.IsNullOrWhiteSpace(givenName))
            return (givenName.Trim(), familyName?.Trim() ?? string.Empty);

        if (!string.IsNullOrWhiteSpace(displayName))
        {
            var parts = displayName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
                return (parts[0], parts[1]);
            return (parts[0], string.Empty);
        }

        var localPart = email.Split('@')[0];
        return (localPart, string.Empty);
    }

    private sealed record OAuthTokenResponse(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("error")] string? Error,
        [property: JsonPropertyName("error_description")] string? ErrorDescription);

    private sealed record GoogleUserResponse(
        [property: JsonPropertyName("id")] string? Id,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("given_name")] string? GivenName,
        [property: JsonPropertyName("family_name")] string? FamilyName,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("picture")] string? Picture);

    private sealed record GitHubUserResponse(
        [property: JsonPropertyName("id")] long? Id,
        [property: JsonPropertyName("login")] string? Login,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("avatar_url")] string? AvatarUrl);

    private sealed record GitHubEmailResponse(
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("primary")] bool Primary,
        [property: JsonPropertyName("verified")] bool Verified);
}
