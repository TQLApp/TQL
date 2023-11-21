using Microsoft.Extensions.Logging;

namespace Tql.App.Support;

internal class InMemoryLogMessageEventArgs(InMemoryLogMessage message) : EventArgs
{
    public InMemoryLogMessage Message { get; } = message;
}

internal record InMemoryLogMessage(
    LogLevel LogLevel,
    EventId EventId,
    string CategoryName,
    string Message
);
