using DbUp.Engine.Output;
using Microsoft.Extensions.Logging;
#pragma warning disable CA2254

namespace Notifications.Infrastructure;

public class LoggerAdapter(ILogger<LoggerAdapter> logger)
    : IUpgradeLog
{
    public void WriteInformation(string format, params object[] args)
        => logger.LogInformation(format, args);

    public void WriteError(string format, params object[] args)
        => logger.LogError(format, args);

    public void WriteWarning(string format, params object[] args)
        => logger.LogWarning(format, args);
}