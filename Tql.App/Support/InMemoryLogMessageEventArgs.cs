using Microsoft.Extensions.Logging;

namespace Tql.App.Support;

internal class InMemoryLogMessageEventArgs : EventArgs
{
    public InMemoryLogMessage Message { get; }

    public InMemoryLogMessageEventArgs(InMemoryLogMessage message)
    {
        Message = message;
    }
}

internal record InMemoryLogMessage(
    LogLevel LogLevel,
    EventId EventId,
    string CategoryName,
    string Message
);
