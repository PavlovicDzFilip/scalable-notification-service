using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure;

namespace Notifications;

public class SimpleNotificationSender(
    IServiceProvider serviceProvider,
    IEmailService emailService)
{
    public async Task<int> Send(CancellationToken cancellationToken)
    {
        var totalNotificationsSent = -1;
        bool hasMoreNotifications;
        do
        {
            hasMoreNotifications = await TrySendNext();
            totalNotificationsSent++;
        } while (hasMoreNotifications && !cancellationToken.IsCancellationRequested);

        return totalNotificationsSent;
    }

    private async Task<bool> TrySendNext()
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<NotificationContext>();
        var notification = await dbContext.Notifications.FirstOrDefaultAsync(x => x.SendDate < DateTime.UtcNow);
        if (notification is null)
        {
            return false;
        }

        await emailService.Send(notification.Payload);
        dbContext.Notifications.Remove(notification);
        dbContext.NotificationLogs.Add(new NotificationLog
        {
            Id = notification.Id,
            SentDate = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
        return true;
    }
}