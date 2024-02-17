using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notifications.Consumer;

public class NotificationSender : AsyncEventingBasicConsumer, IDisposable
{
    private readonly IEmailService _emailService;
    private readonly ISerializer _serializer;
    private readonly INotificationLogCache _notificationLogCache;
    private readonly KillSwitch _killSwitch;

    public int MessagesHandled { get; private set; }

    public NotificationSender(
        IEmailService emailService,
        IConnection connection,
        ISerializer serializer,
        INotificationLogCache notificationLogCache,
        KillSwitch killSwitch,
        ushort qos) :
        this(
            emailService,
            connection.CreateModel(),
            serializer,
            notificationLogCache,
            killSwitch,
            qos)
    {
    }

    private NotificationSender(
        IEmailService emailService,
        IModel channel,
        ISerializer serializer,
        INotificationLogCache notificationLogCache,
        KillSwitch killSwitch,
        ushort qos)
        : base(channel)
    {
        _emailService = emailService;
        _serializer = serializer;
        _notificationLogCache = notificationLogCache;
        _killSwitch = killSwitch;
        Received += OnReceived;
        channel.BasicQos(0, qos, false);
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
        var notifications = _serializer.Deserialize<Notification[]>(@event.Body)
                         ?? throw new Exception();

        foreach (var notification in notifications)
        {
            var isAlreadyHandled = await _notificationLogCache.IsHandledAsync(notification.Id);
            if (isAlreadyHandled)
            {
                continue;
            }

            await _emailService.Send(notification.Payload);

            await _notificationLogCache.AddAsync(new NotificationLog
            {
                Id = notification.Id,
                SentDate = DateTime.UtcNow
            });

            MessagesHandled++;
        }
    }

    public void Dispose()
    {
        Received -= OnReceived;
        Model.Close();
        Model.Dispose();
        GC.SuppressFinalize(this);
    }
}
