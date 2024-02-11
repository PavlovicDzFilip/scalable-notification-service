namespace Notifications.Consumer;

public interface IEmailService
{
    Task Send(string notificationPayload);
}