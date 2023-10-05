using Launcher.Abstractions;
using System.Collections.Concurrent;

namespace Launcher.App.Services;

internal class CacheManagerManager
{
    private volatile int _loading;
    private readonly ConcurrentDictionary<Type, Action> _invalidateActions = new();

    public bool IsLoading => _loading > 0;

    public event EventHandler? LoadingChanged;

    public void Register<T>(ICache<T> cache)
    {
        _invalidateActions[typeof(T)] = cache.Invalidate;
    }

    public void InvalidateAllCaches()
    {
        foreach (var action in _invalidateActions.Values)
        {
            action();
        }
    }

    public void StartLoading()
    {
        Interlocked.Increment(ref _loading);

        OnLoadingChanged();
    }

    public void StopLoading()
    {
        Interlocked.Decrement(ref _loading);

        OnLoadingChanged();
    }

    protected virtual void OnLoadingChanged()
    {
        LoadingChanged?.Invoke(this, EventArgs.Empty);
    }
}
