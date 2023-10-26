using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Tql.App.Support;

[ProviderAlias("InMemory")]
internal class InMemoryLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, InMemoryLogger> _loggers = new();

    public event EventHandler<InMemoryLogMessageEventArgs>? Logged;

    public void RaiseLogged(InMemoryLogMessage message)
    {
        OnLogged(new InMemoryLogMessageEventArgs(message));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, p => new InMemoryLogger(this, p));
    }

    public void Dispose() { }

    protected virtual void OnLogged(InMemoryLogMessageEventArgs e) => Logged?.Invoke(this, e);
}
