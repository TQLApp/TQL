using Microsoft.Extensions.Logging;
using NuGet.Common;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = NuGet.Common.LogLevel;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tql.App.Services.Packages.NuGet;

internal class NuGetLogger(ILogger logger) : LoggerBase
{
    public override void Log(ILogMessage message)
    {
        var logLevel = message.Level switch
        {
            LogLevel.Debug => MicrosoftLogLevel.Debug,
            LogLevel.Verbose => MicrosoftLogLevel.Trace,
            LogLevel.Information => MicrosoftLogLevel.Information,
            LogLevel.Minimal => MicrosoftLogLevel.Information,
            LogLevel.Warning => MicrosoftLogLevel.Warning,
            LogLevel.Error => MicrosoftLogLevel.Error,
            _ => throw new ArgumentOutOfRangeException()
        };

        logger.Log(logLevel, message.Message);
    }

    public override Task LogAsync(ILogMessage message)
    {
        Log(message);

        return Task.CompletedTask;
    }
}
