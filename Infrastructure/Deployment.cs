using DbUp;
using DbUp.Engine.Output;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Notifications.Infrastructure;
public class Deployment(IConfiguration configuration, IUpgradeLog upgradeLog)
{
    public void DeployInfrastructure()
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