using Microsoft.Extensions.Logging;

namespace Tql.App.Services;

internal class LifecycleService(ILogger<LifecycleService> logger)
{
    private readonly object _syncRoot = new();
    private readonly List<Action> _beforeHostTermination = new();
    private readonly List<Action> _afterHostTermination = new();
    private readonly List<Action> _beforeShutdown = new();

    public void RegisterBeforeHostTermination(Action action)
    {
        lock (_syncRoot)
        {
            _beforeHostTermination.Add(action);
        }
    }

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

    public void RaiseBeforeHostTermination() => Raise(_beforeHostTermination);

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
