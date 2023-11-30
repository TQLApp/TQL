namespace Tql.Abstractions;

/// <summary>
/// Represents the service for managing configuration for plugins.
/// </summary>
/// <remarks>
/// <para>
/// The configuration manager can be used to manage configuration for
/// your plugin. The configuration is stored as a simple string. It's
/// assumed you have a DTO object and JSON serialize and deserialize
/// this yourself.
/// </para>
///
/// <para>
/// The <see cref="ConfigurationChanged"/> event should be used to
/// detect changes to the configuration and invalidate the cache
/// if you're using a <see cref="ICacheManager{T}"/>.
/// </para>
///
/// <para>
/// If you're managing user's credentials, use the <see cref="IEncryption"/>
/// service to encrypt and decrypt these credentials.
/// </para>
/// </remarks>
public interface IConfigurationManager
{
    /// <summary>
    /// Occurs when the configuration for a plugin has changed.
    /// </summary>
    event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

    /// <summary>
    /// Gets the configuration for a plugin.
    /// </summary>
    /// <param name="pluginId">ID of the plugin.</param>
    /// <returns>Configuration of the plugin.</returns>
    string? GetConfiguration(Guid pluginId);

    /// <summary>
    /// Sets the configuration of a plugin.
    /// </summary>
    /// <param name="pluginId">ID of the plugin.</param>
    /// <param name="configuration">Configuration of the plugin.</param>
    void SetConfiguration(Guid pluginId, string? configuration);
}
