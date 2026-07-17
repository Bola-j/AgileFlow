namespace AgileFlow.Application.Exceptions;

/// <summary>OAuth code exchange or profile fetch failed.</summary>
public sealed class OAuthAuthenticationException : UnauthorizedAccessException
{
    public OAuthAuthenticationException(string message) : base(message) { }

    public OAuthAuthenticationException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>The provider did not return a usable email address.</summary>
public sealed class OAuthMissingEmailException : ArgumentException
{
    public OAuthMissingEmailException()
        : base("The OAuth provider did not return an email address. Grant email access or make your email public.") { }
}

/// <summary>External login is already linked to a different account.</summary>
public sealed class DuplicateExternalLoginException : InvalidOperationException
{
    public DuplicateExternalLoginException(string message) : base(message) { }
}
