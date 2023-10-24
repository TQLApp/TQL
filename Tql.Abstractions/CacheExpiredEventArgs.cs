namespace Tql.Abstractions;

public class CacheExpiredEventArgs : EventArgs
{
    public bool Force { get; }

    public CacheExpiredEventArgs(bool force)
    {
        Force = force;
    }
}
