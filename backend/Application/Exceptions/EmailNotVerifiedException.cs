namespace AgileFlow.Application.Exceptions;

/// <summary>
/// Thrown by <see cref="AgileFlow.Application.Interfaces.IAuthService.LoginAsync"/> when the
/// user's credentials are correct but their email address has not been confirmed.
/// The exception middleware maps this to 403 Forbidden with a body that includes
/// <c>requiresEmailConfirmation: true</c> so the frontend can distinguish it from
/// a standard "wrong password" or "locked out" failure.
/// </summary>
public sealed class EmailNotVerifiedException : UnauthorizedAccessException
{
    public string Email { get; }

    public EmailNotVerifiedException(string email)
        : base("Email address has not been confirmed. Please check your inbox or request a new verification email.")
    {
        Email = email;
    }
}
