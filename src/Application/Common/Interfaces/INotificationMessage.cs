namespace Application.Common.Interfaces;

public interface INotificationMessage
{
    Task SendAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default);
}
