using Microsoft.Extensions.Logging;
using NuGet.Common;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = NuGet.Common.LogLevel;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tql.App.Services.Packages;

internal class NuGetLogger : LoggerBase
{
    private readonly ILogger _logger;

    public NuGetLogger(ILogger logger)
    {
        _logger = logger;
    }

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

        _logger.Log(logLevel, message.Message);
    }

    public override Task LogAsync(ILogMessage message)
    {
        Log(message);

        return Task.CompletedTask;
    }
}
