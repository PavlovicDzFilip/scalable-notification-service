namespace Notifications;

public interface IEmailService
{
    Task Send(string notificationPayload);
}