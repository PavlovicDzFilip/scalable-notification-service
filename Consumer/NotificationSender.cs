using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Infrastructure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Notifications.Consumer;

public class NotificationSender : AsyncEventingBasicConsumer, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEmailService _emailService;
    private readonly KillSwitch _killSwitch;

    public int MessagesHandled { get; private set; }

    public NotificationSender(
        IServiceProvider serviceProvider,
        IEmailService emailService,
        IConnection connection,
        KillSwitch killSwitch) : 
        this(
            serviceProvider, 
            emailService, 
            connection.CreateModel(),
            killSwitch)
    {
    }

    private NotificationSender(IServiceProvider serviceProvider, IEmailService emailService, IModel channel, KillSwitch killSwitch)
        : base(channel)
    {
        _serviceProvider = serviceProvider;
        _emailService = emailService;
        _killSwitch = killSwitch;
        Received += OnReceived;
        channel.BasicQos(0, 100, false);
        var queueName = "notifications";
        channel.BasicConsume(queueName, false, this);
    }

    private async Task OnReceived(object sender, BasicDeliverEventArgs @event)
    {
        _killSwitch.Activate();
        try
        {
            await Send(@event);
            Model.BasicAck(deliveryTag: @event.DeliveryTag, multiple: false);
        }
        catch
        {
            Model.BasicNack(deliveryTag: @event.DeliveryTag, multiple: false, requeue: true);
        }
    }

    private async Task Send(BasicDeliverEventArgs @event)
    {
        var notification = JsonSerializer.Deserialize<Notification>(Encoding.UTF8.GetString(@event.Body.Span))
                         ?? throw new Exception();

        await using var scope = _serviceProvider.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<NotificationContext>();

        var isAlreadyHandled = await dbContext.NotificationLogs.AnyAsync(x => x.Id == notification.Id);
        if (isAlreadyHandled)
        {
            return;
        }

        await _emailService.Send(notification.Payload);
        dbContext.NotificationLogs.Add(new NotificationLog
        {
            Id = notification.Id,
            SentDate = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();

        MessagesHandled++;
    }

    public void Dispose()
    {
        Received -= OnReceived;
        Model.Close();
        Model.Dispose();
        GC.SuppressFinalize(this);
    }
}
