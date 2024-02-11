using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Threading;

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
        var killswitch = new KillSwitch();

        using var connection = factory.CreateConnection();
        var consumers = Enumerable.Range(0, numberOfHandlers)
            .Select(_ => new NotificationSender(serviceProvider, emailService, connection, killswitch))
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
