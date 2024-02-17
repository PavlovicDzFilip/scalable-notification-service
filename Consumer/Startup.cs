using DbUp.Engine.Output;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Notifications.Infrastructure;
using StackExchange.Redis;

namespace Notifications.Consumer;

public static class Startup
{
    public static IServiceProvider Configure(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        var connectionString = configuration.GetConnectionString("SqlServer") ??
                               throw new Exception("SqlServer connection string is not configured");

        services.AddDbContext<NotificationContext>(
            opts => opts
                .UseSqlServer(connectionString));

        services.AddLogging(logging => logging
            .AddFilter("Microsoft", LogLevel.Warning)
            .AddFilter("System", LogLevel.Warning)
            .AddConsole());

        services.AddSingleton(configuration);

        services.AddSingleton<Deployment>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUpgradeLog, LoggerAdapter>();

        services.AddSingleton<MessageConsumer>();
        services.AddSingleton<ISerializer, MessagePackSerializer>();
        services.AddSingleton((Func<IServiceProvider, INotificationLogCache>)(x =>
        {
            var redisConnectionString = configuration.GetConnectionString("RedisCache")!;
            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            IDatabase db = redis.GetDatabase();
            var serializer = x.GetRequiredService<ISerializer>();
            return new RedisNotificationLogCache(db, serializer);
        }));

        return services.BuildServiceProvider();
    }

    public static IConfiguration BuildConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("appsettings.json", optional: false);
        configurationBuilder.AddEnvironmentVariables();
        return configurationBuilder.Build();
    }
}
