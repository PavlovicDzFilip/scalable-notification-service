namespace Notifications;

public class EmailService : IEmailService
{
    // Simulate sending a notification, and wait for 10ms
    public Task Send(string notificationPayload) 
        => Task.Delay(10);
}
