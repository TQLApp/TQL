using NuGet.Common;

namespace Tql.App.Test;

internal class NuGetLogger : LoggerBase
{
    public override void Log(ILogMessage message)
    {
        TestContext.Progress.WriteLine($"[{message.Level}] {message.Message}");
    }

    public override Task LogAsync(ILogMessage message)
    {
        Log(message);

        return Task.CompletedTask;
    }
}
