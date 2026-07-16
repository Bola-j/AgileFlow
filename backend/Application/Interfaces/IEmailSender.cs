namespace AgileFlow.Application.Interfaces;

/// <summary>
/// Abstracts the email delivery transport.
/// Use this interface in application/infrastructure services instead of depending
/// on a concrete SMTP client so that tests can substitute a no-op sender.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message.
    /// Implementations must throw on permanent delivery failures so callers
    /// can catch and log to <see cref="AgileFlow.Domain.Entities.EmailNotificationLog"/>.
    /// </summary>
    /// <param name="toEmail">Recipient address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="htmlBody">HTML body content.</param>
    /// <param name="textBody">Optional plain-text fallback body.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default);
}
