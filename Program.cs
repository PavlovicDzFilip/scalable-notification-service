// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Notifications;
using Notifications.Infrastructure;

var config = Startup.BuildConfiguration();
var serviceProvider = Startup.Configure(config);
var deployment = serviceProvider.GetRequiredService<Deployment>();
deployment.DeployInfrastructure();
Console.WriteLine("Done");

