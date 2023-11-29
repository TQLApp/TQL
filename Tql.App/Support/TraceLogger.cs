using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Tql.App.Support;

internal class TraceLogger : ILogger
{
    public static readonly TraceLogger Instance = new();

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        Trace.WriteLine($"[{logLevel}] {formatter(state, exception)}");
        if (exception != null)
            Trace.WriteLine(exception.PrintStackTrace());
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
    {
        return null;
    }
}

internal class TraceLogger<T> : TraceLogger, ILogger<T> { }
