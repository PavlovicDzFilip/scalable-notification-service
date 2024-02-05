// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notifications;
using Notifications.Infrastructure;

var config = Startup.BuildConfiguration();
var serviceProvider = Startup.Configure(config);
var deployment = serviceProvider.GetRequiredService<Deployment>();
deployment.DeployInfrastructure();

var sender = serviceProvider.GetRequiredService<MultithreadedPartitionedNotificationSender>();
var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
var notificationsSent = await sender.Send(cancellationTokenSource.Token);

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
logger.LogWarning("Sending notifications end. Total notifications sent: {notificationsSent}", notificationsSent);
