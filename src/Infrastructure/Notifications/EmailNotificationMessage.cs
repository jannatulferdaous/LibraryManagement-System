using Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Infrastructure.Notifications;

// Sends real email via SMTP (MailKit) when Smtp:Host is configured. Falls back to
// logging-only when it isn't - this keeps local dev/CI/reviewer runs working out of the
// box without requiring real SMTP credentials, while still being a genuine, swappable
// production implementation behind the same INotificationMessage interface.
public class EmailNotificationMessage : INotificationMessage
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationMessage> _logger;

    public EmailNotificationMessage(IConfiguration configuration, ILogger<EmailNotificationMessage> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Smtp:Host"];

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation("[EMAIL to {Recipient}] (Smtp:Host not configured, logging only) {Subject}: {Message}",
                recipient, subject, message);
            return;
        }

        var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@library.local";
        var fromName = _configuration["Smtp:FromName"] ?? "Library Management System";
        var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
        var enableSsl = !bool.TryParse(_configuration["Smtp:EnableSsl"], out var ssl) || ssl;
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(fromName, fromEmail));
        email.To.Add(MailboxAddress.Parse(recipient));
        email.Subject = subject;
        email.Body = new TextPart("plain") { Text = message };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                await client.AuthenticateAsync(username, password, cancellationToken);

            await client.SendAsync(email, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent to {Recipient}: {Subject}", recipient, subject);
        }
        catch (Exception ex)
        {
            // A failed notification should never take down the operation that triggered
            // it (e.g. a reservation fulfillment) - log and move on rather than throwing.
            _logger.LogError(ex, "Failed to send email to {Recipient}", recipient);
        }
    }
}
