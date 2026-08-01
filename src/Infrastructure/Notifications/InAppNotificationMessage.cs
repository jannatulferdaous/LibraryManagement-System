using Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Notifications;

// Stub implementation - a real one would persist to a Notifications table for display in
// the frontend's notification bell. Kept as a logging stub to demonstrate the Factory
// pattern resolves to a genuinely different implementation per channel.
public class InAppNotificationMessage : INotificationMessage
{
    private readonly ILogger<InAppNotificationMessage> _logger;

    public InAppNotificationMessage(ILogger<InAppNotificationMessage> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[IN-APP for {Recipient}] {Subject}: {Message}", recipient, subject, message);
        return Task.CompletedTask;
    }
}
