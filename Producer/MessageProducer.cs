using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure;
using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace Notifications;

public class MessageProducer(
    IServiceProvider serviceProvider,
    IConfiguration configuration)
{
    public async Task<int> Send(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(configuration.GetConnectionString("MessageBroker")!)
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        var totalNotificationsSent = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            totalNotificationsSent += await PublishNextBatch(channel, 1000);
        }

        return totalNotificationsSent;
    }

    private async Task<int> PublishNextBatch(IModel channel, int batchSize)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<NotificationContext>();
        var notifications = await dbContext.Notifications
            .Where(x => x.SendDate < DateTime.UtcNow)
            .Take(batchSize)
            .ToListAsync();

        if (notifications.Count == 0)
        {
            return 0;
        }

        dbContext.RemoveRange(notifications);

        notifications
            .ForEach(x =>
            {
                var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(x));
                var props = channel.CreateBasicProperties();
                props.DeliveryMode = 2; // Persistent
                channel.BasicPublish(string.Empty, "notifications", true, props, messageBody);
            });

        await dbContext.SaveChangesAsync();
        return notifications.Count;
    }
}