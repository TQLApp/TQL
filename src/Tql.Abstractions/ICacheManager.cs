namespace Tql.Abstractions;

/// <summary>
/// Represents a cache manager.
/// </summary>
/// <typeparam name="T">Type of the cache.</typeparam>
/// <remarks>
/// <para>
/// Register an implementation of this interface to expose a cache
/// that TQL will manage for you. TQL takes care of cache invalidation,
/// refresh and storage.
/// </para>
///
/// <para>
/// The type of the cache should be fully immutable. Easiest is to limit
/// yourself to use records and immutable collections like <see cref="ImmutableArray{T}"/>.
/// </para>
///
/// <para>
/// TQL uses JsonSerializer to serialize and deserialize data.
/// </para>
///
/// <para>
/// Caches are automatically rebuilt after it expires. The default
/// expiration interval is 60 minutes and can be configured by the
/// user. Caches will also be invalidated if the <see cref="CacheInvalidationRequired"/>
/// event is raised. You should raise this event e.g. when the configuration
/// of the plugin changes. Caches will also be invalidated if the
/// version of the cache differs from <see cref="Version"/>. You can
/// use this to initiate a cache rebuilt if the data you're storing
/// has changed.
/// </para>
/// </remarks>
public interface ICacheManager<T>
{
    /// <summary>
    /// Gets the version of the cache.
    /// </summary>
    /// <remarks>
    /// Bump this version number whenever you deploy a new version of
    /// the plugin that changes the data that is being cached.
    /// </remarks>
    int Version { get; }

    /// <summary>
    /// Occurs when the cache should be invalidated.
    /// </summary>
    event EventHandler<CacheInvalidationRequiredEventArgs>? CacheInvalidationRequired;

    /// <summary>
    /// Creates a new version of the cache.
    /// </summary>
    /// <remarks>
    /// This method is called by TQL whenever it requires a new version of
    /// the cache. If a cache rebuilt was forced (either because it's the first
    /// time it's built, the version number changed or the <see cref="CacheInvalidationRequired"/>
    /// event is raised with the <see cref="CacheInvalidationRequiredEventArgs.Force"/>
    /// property set to true), any exception returned from this method will
    /// be thrown when the cache is retrieved. This puts the cache in a "faulted"
    /// state. In this state, a cache rebuilt is requested every time the
    /// cache is requested.
    /// </remarks>
    /// <returns>New version of the cache.</returns>
    Task<T> Create();
}
