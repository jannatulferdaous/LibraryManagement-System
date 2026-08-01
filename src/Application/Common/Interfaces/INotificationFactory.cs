using Domain.Enums;

namespace Application.Common.Interfaces;

public interface INotificationFactory
{
    INotificationMessage Create(NotificationChannel channel);
}
