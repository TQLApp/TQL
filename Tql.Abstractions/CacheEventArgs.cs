namespace Tql.Abstractions;

/// <summary>
/// Event arguments for events related to cache updates.
/// </summary>
/// <typeparam name="T">Type of the cache.</typeparam>
public class CacheEventArgs<T> : EventArgs
{
    /// <summary>
    /// Gets cache version associated with the event.
    /// </summary>
    public T Cache { get; }

    /// <summary>
    /// Initializes a new <see cref="CacheEventArgs{T}"/>.
    /// </summary>
    /// <param name="cache">Cache version associated with the event.</param>
    public CacheEventArgs(T cache)
    {
        Cache = cache;
    }
}
