using Microsoft.Extensions.Logging;
using Tql.Abstractions;

namespace Tql.App.Services;

internal class LifecycleService(ILogger<LifecycleService> logger) : ILifecycleService
{
    private readonly object _syncRoot = new();
    private readonly List<Action> _afterHostTermination = new();
    private readonly List<Action> _beforeShutdown = new();

    public void RegisterAfterHostTermination(Action action)
    {
        lock (_syncRoot)
        {
            _afterHostTermination.Add(action);
        }
    }

    public void RegisterBeforeShutdown(Action action)
    {
        lock (_syncRoot)
        {
            _beforeShutdown.Add(action);
        }
    }

    public void RaiseAfterHostTermination() => Raise(_afterHostTermination);

    public void RaiseBeforeShutdown() => Raise(_beforeShutdown);

    private void Raise(List<Action> actions)
    {
        foreach (var action in actions)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lifecycle event failed");
            }
        }
    }
}
