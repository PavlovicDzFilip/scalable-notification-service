using Microsoft.Extensions.Configuration;

namespace Notifications;

public class MultithreadedPartitionedNotificationSender
{
    private readonly SinglePartitionNotificationSender[] _senders;

    public MultithreadedPartitionedNotificationSender(
        IServiceProvider serviceProvider,
        IEmailService emailService,
        IConfiguration configuration)
    {
        var numberOfPartitions = configuration.GetValue<int>("NumberOfPartitions");
        _senders = Enumerable.Range(0, numberOfPartitions)
        .Select(x => new SinglePartitionNotificationSender(serviceProvider, emailService, x, numberOfPartitions))
        .ToArray();
    }

    public async Task<int> Send(CancellationToken cancellationToken)
    {
        var tasks = _senders.Select(x => x.Send(cancellationToken)).ToArray();
        await Task.WhenAll(tasks);
        return tasks.Sum(x => x.Result);
    }
}
