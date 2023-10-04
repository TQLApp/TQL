namespace Launcher.App.Services;

internal class CacheManagerManager
{
    private volatile int _loading;

    public bool IsLoading => _loading > 0;

    public event EventHandler? LoadingChanged;

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
