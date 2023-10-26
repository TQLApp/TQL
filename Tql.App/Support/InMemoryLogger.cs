using Microsoft.Extensions.Logging;

namespace Tql.App.Support;

internal class InMemoryLogger : ILogger
{
    private readonly InMemoryLoggerProvider _provider;
    private readonly string _categoryName;

    public InMemoryLogger(InMemoryLoggerProvider provider, string categoryName)
    {
        _provider = provider;
        _categoryName = categoryName;
    }

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

        _provider.RaiseLogged(
            new InMemoryLogMessage(logLevel, eventId, _categoryName, formatter(state, exception))
        );
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;
}
