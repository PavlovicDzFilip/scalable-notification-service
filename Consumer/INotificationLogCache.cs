namespace Notifications.Consumer;

public interface INotificationLogCache
{
    Task<bool> IsHandledAsync(long id);
    Task AddAsync(NotificationLog notificationLog);
}
