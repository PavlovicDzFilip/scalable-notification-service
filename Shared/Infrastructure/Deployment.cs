using DbUp;
using DbUp.Engine.Output;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Notifications.Infrastructure;
public class Deployment(IConfiguration configuration, IUpgradeLog upgradeLog)
{
    public void DeployInfrastructure()
    {
        UpgradeDatabase(configuration, upgradeLog);
        MessageQueue.EnsureExists(
            configuration.GetConnectionString("MessageBroker") ?? 
            throw new Exception("MessageBroker configuration not found"));
    }

    private static void UpgradeDatabase(IConfiguration configuration, IUpgradeLog upgradeLog)
    {
        var connectionString = configuration.GetConnectionString("SqlServer");
        EnsureDatabase.For.SqlDatabase(connectionString);

        var result =
            DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogTo(upgradeLog)
                .Build()
                .PerformUpgrade();

        if (!result.Successful)
        {
            Environment.Exit(-1);
        }
    }
}