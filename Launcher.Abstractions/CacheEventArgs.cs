namespace Launcher.Abstractions;

public class CacheEventArgs<T> : EventArgs
{
    public T Cache { get; }

    public CacheEventArgs(T cache)
    {
        Cache = cache;
    }
}
