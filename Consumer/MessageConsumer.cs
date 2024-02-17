using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Notifications.Consumer;

public class MessageConsumer(
        IServiceProvider serviceProvider,
        IEmailService emailService,
        IConfiguration configuration)
{
    public async Task<int> Consume()
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(configuration.GetConnectionString("MessageBroker")!),
            DispatchConsumersAsync = true
        };

        var numberOfHandlers = configuration.GetValue<int>("NumberOfHandlers");
        var qos = configuration.GetValue<ushort>("HandlerQoS");
        var killswitch = new KillSwitch();

        var notificationLogCache = serviceProvider.GetRequiredService<INotificationLogCache>();
        var serializer = serviceProvider.GetRequiredService<ISerializer>();
        using var connection = factory.CreateConnection();


        var consumers = Enumerable.Range(0, numberOfHandlers)
            .Select(_ => new NotificationSender(emailService, connection, serializer, notificationLogCache, killswitch, qos))
            .ToList();

        try
        {
            await Task.Delay(-1, killswitch.CancellationToken);
        }
        catch { }

        var messagesHandled = consumers.Sum(x => x.MessagesHandled);
        consumers.ForEach(x => x.Dispose());
        return messagesHandled;
    }
}
