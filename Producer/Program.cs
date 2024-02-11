using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notifications;
using Notifications.Infrastructure;

var config = Startup.BuildConfiguration();
var serviceProvider = Startup.Configure(config);
var deployment = serviceProvider.GetRequiredService<Deployment>();
deployment.DeployInfrastructure();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

#if !DEBUG
logger.LogWarning("Waiting for other services to start");
await Task.Delay(TimeSpan.FromMinutes(1));
logger.LogWarning("Waiting for other services to start complete");
#endif

// Send message to the queue
var producer = serviceProvider.GetRequiredService<MessageProducer>();
var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
var notificationsSent = await producer.Send(cancellationTokenSource.Token);


logger.LogWarning("Sent {notificationsSent} messages to the queue", notificationsSent);

await Task.Delay(-1);
