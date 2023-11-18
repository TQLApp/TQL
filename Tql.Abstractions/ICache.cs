﻿namespace Tql.Abstractions;

/// <summary>
/// Represents a reference to a cache.
/// </summary>
/// <typeparam name="T">Type of the cache.</typeparam>
/// <remarks>
/// Instances of <see cref="ICache{T}"/> can be resolved after a cache
/// manager for the same type has been registered. The
/// implementation of this interface manages
/// </remarks>
public interface ICache<T>
{
    /// <summary>
    /// Gets whether the cache is available or whether the cache
    /// is being rebuilt.
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Occurs when the cache has updated.
    /// </summary>
    event EventHandler<CacheEventArgs<T>> Updated;

    /// <summary>
    /// Gets the current version of the cache.
    /// </summary>
    /// <returns>Current version of the cache.</returns>
    Task<T> Get();
}
