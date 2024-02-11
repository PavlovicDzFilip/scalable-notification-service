using RabbitMQ.Client;

namespace Notifications.Infrastructure;

public static class MessageQueue
{
    public static void EnsureExists(string connectionString)
    {
        var queueName = "notifications";

        var factory = new ConnectionFactory() { Uri = new Uri(connectionString) };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }
}