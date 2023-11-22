using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Tql.App.Services.Packages.PackageStore;

internal class PackageExportFinder
{
    public static List<PackageExport> Resolve(string path, ILogger logger)
    {
        throw new NotImplementedException();
        /*
        var appDomain = AppDomain.CreateDomain(
            nameof(PackageExportFinder),
            AppDomain.CurrentDomain.Evidence,
            new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                ApplicationName = nameof(PackageExportFinder)
            }
        );

        try
        {
            var loader = (Loader)
                appDomain.CreateInstanceAndUnwrap(
                    typeof(Loader).Assembly.FullName,
                    typeof(Loader).FullName!
                );

            return loader
                .Load(path, new LoggerMarshal(logger))
                .Select(p => new PackageExport(p.FileName, p.TypeName))
                .ToList();
        }
        finally
        {
            AppDomain.Unload(appDomain);
        }
    }

    private class Loader : MarshalByRefObject
    {
        public List<(string FileName, string TypeName)> Load(
            string path,
            LoggerMarshal loggerMarshal
        )
        {
            var logger = new Logger(loggerMarshal);

            using var loader = new SideloadedPluginLoader(path, logger);

            var entries = new List<(string FileName, string TypeName)>();

            foreach (var type in loader.GetPluginTypes())
            {
                logger.Log(LogLevel.Information, "Discovered {TypeName}", type.FullName);

                entries.Add((type.Assembly.Location, type.FullName!));
            }

            return entries;
        }
    }

    private class LoggerMarshal(ILogger logger) : MarshalByRefObject
    {
        public void Log(LogLevel level, string message, params object?[] args)
        {
            logger.Log(level, message, args);
        }
    }

    private class Logger(LoggerMarshal logger) : ILogger
    {
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter
        )
        {
            logger.Log(logLevel, formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }*/
    }
}
