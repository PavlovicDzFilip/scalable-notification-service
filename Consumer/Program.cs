using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notifications.Consumer;
using Notifications.Infrastructure;

var config = Startup.BuildConfiguration();
var serviceProvider = Startup.Configure(config);
var deployment = serviceProvider.GetRequiredService<Deployment>();
deployment.DeployInfrastructure();

var messageConsumer = serviceProvider.GetRequiredService<MessageConsumer>();
var notificationsSent = await messageConsumer.Consume();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
logger.LogWarning("Sending notifications end. Total notifications sent: {notificationsSent}", notificationsSent);

await Task.Delay(-1);
