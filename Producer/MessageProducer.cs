using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure;
using RabbitMQ.Client;

namespace Notifications;

public class MessageProducer
{
    private readonly ISerializer _serializer;
    private readonly NotificationContext _dbContext;
    private readonly ConnectionFactory _factory;
    private readonly int _batchSize;

    public MessageProducer(
        ISerializer serializer,
        NotificationContext dbContext,
        IConfiguration configuration)
    {
        _factory = new ConnectionFactory
        {
            Uri = new Uri(configuration.GetConnectionString("MessageBroker")!)
        };

        _batchSize = configuration.GetValue<int>("BatchSize");
        _serializer = serializer;
        _dbContext = dbContext;
    }

    public async Task<int> Send(CancellationToken cancellationToken)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ConfirmSelect();

        var totalNotificationsSent = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            totalNotificationsSent += await PublishNextBatch(channel);
        }

        return totalNotificationsSent;
    }

    private async Task<int> PublishNextBatch(IModel channel)
    {
        var now = DateTime.UtcNow;
        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.SendDate < now)
            .OrderBy(x => x.Id)
            .Take(_batchSize)
            .ToListAsync();

        if (notifications.Count == 0)
        {
            throw new Exception("Not enough data to produce messages");
        }

        var props = channel.CreateBasicProperties();
        props.DeliveryMode = 2; // Persistent

        notifications
            .ForEach(x =>
            {
                var messageBody = _serializer.Serialize(x);
                channel.BasicPublish(string.Empty, "notifications", true, props, messageBody);
            });

        channel.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));

        await _dbContext.Notifications
            .Where(x => x.Id <= notifications[notifications.Count - 1].Id)
            .Where(x => x.SendDate < now)
            .ExecuteDeleteAsync();

        return notifications.Count;
    }
}