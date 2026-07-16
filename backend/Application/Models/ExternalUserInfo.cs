namespace AgileFlow.Application.Models;

/// <summary>Normalized profile returned by an OAuth provider after code exchange.</summary>
public sealed record ExternalUserInfo(
    string ProviderKey,
    string Email,
    string FirstName,
    string LastName,
    string? ProfilePicture,
    string? GithubUsername
);
