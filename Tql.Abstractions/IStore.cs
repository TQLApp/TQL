namespace Tql.Abstractions;

/// <summary>
/// Represents the service to manage data persistence.
/// </summary>
public interface IStore
{
    /// <summary>
    /// Gets a cache folder for the plugin.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If you want to cache data, you should use the <see cref="ICacheManager{T}"/>
    /// service. However, some data does not suite the cache manager.
    /// The cache manager is meant to store store data cached from a REST
    /// API that can be retrieved in one go. If you instead e.g. want to
    /// cache images (like work item icons, Gravatar images, etc) storing
    /// these in files on disk is a better option. In that case, use the folder
    /// returned by this method to store these files.
    /// </para>
    ///
    /// <para>
    /// The utilities library contains a class IconManager&lt;T&gt; that
    /// provides this service.
    /// </para>
    /// </remarks>
    /// <param name="pluginId">ID of the plugin.</param>
    /// <returns>Path to the cache folder.</returns>
    string GetCacheFolder(Guid pluginId);
}
