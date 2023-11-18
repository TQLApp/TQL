namespace Tql.Abstractions;

/// <summary>
/// Event arguments for an event that signals cache invalidation.
/// </summary>
public class CacheInvalidationRequiredEventArgs : EventArgs
{
    /// <summary>
    /// Gets whether an immediate rebuilt is required.
    /// </summary>
    public bool Force { get; }

    /// <summary>
    /// Initializes a new <see cref="CacheInvalidationRequiredEventArgs"/>.
    /// </summary>
    /// <param name="force">Whether an immediate rebuilt is required.</param>
    /// <remarks>
    /// By default, this event will cause a cache rebuilt to start in the
    /// background. However, if the plugin depends on the new version of the
    /// cache, an immediate rebuilt can be forced. This will cause any requests
    /// of the cache to be delayed until the cache is rebuilt.
    /// </remarks>
    public CacheInvalidationRequiredEventArgs(bool force)
    {
        Force = force;
    }
}
