using AgileFlow.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AgileFlow.Infrastructure.Services;

/// <summary>
/// SMTP implementation of <see cref="IEmailSender"/> using MailKit/MimeKit.
/// Configuration is read from the <c>Email:Smtp</c> section of appsettings.
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        var smtp = _configuration.GetSection("Email:Smtp");
        var host        = smtp["Host"]      ?? throw new InvalidOperationException("Email:Smtp:Host is not configured.");
        var port        = int.Parse(smtp["Port"]  ?? "587");
        var username    = smtp["Username"]  ?? string.Empty;
        var password    = smtp["Password"]  ?? string.Empty;
        var fromEmail   = smtp["FromEmail"] ?? username;
        var fromName    = smtp["FromName"]  ?? "AgileFlow";
        var useSsl      = bool.Parse(smtp["UseSsl"] ?? "true");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = textBody ?? StripHtml(htmlBody),
        };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        var secureOption = useSsl
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTlsWhenAvailable;

        _logger.LogDebug("Connecting to SMTP {Host}:{Port} (SSL={UseSsl})", host, port, useSsl);
        await client.ConnectAsync(host, port, secureOption, cancellationToken);

        if (!string.IsNullOrWhiteSpace(username))
            await client.AuthenticateAsync(username, password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        _logger.LogInformation("Email '{Subject}' sent to {Recipient}", subject, toEmail);
    }

    /// <summary>Minimal HTML stripper for plain-text fallback bodies.</summary>
    private static string StripHtml(string html)
    {
        return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty);
    }
}
