using Microsoft.Extensions.Logging;

namespace Tql.App.Support;

internal class InMemoryLogger(InMemoryLoggerProvider provider, string categoryName) : ILogger
{
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        if (!IsEnabled(logLevel))
            return;

        provider.RaiseLogged(
            new InMemoryLogMessage(logLevel, eventId, categoryName, formatter(state, exception))
        );
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;
}
