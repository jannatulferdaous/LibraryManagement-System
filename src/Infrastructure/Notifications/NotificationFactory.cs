using Application.Common.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Notifications;

public class NotificationFactory : INotificationFactory
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public INotificationMessage Create(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => _serviceProvider.GetRequiredService<EmailNotificationMessage>(),
        NotificationChannel.InApp => _serviceProvider.GetRequiredService<InAppNotificationMessage>(),
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unsupported notification channel.")
    };
}
