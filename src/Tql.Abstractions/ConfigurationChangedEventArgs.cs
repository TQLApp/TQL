namespace Tql.Abstractions;

/// <summary>
/// Event arguments for events that signal that the configuration of
/// a plugin has changed.
/// </summary>
public class ConfigurationChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the ID of the plugin to which the configuration belongs.
    /// </summary>
    public Guid PluginId { get; }

    /// <summary>
    /// Gets the configuration of the plugin.
    /// </summary>
    public string? Configuration { get; }

    /// <summary>
    /// Initializes a new <see cref="ConfigurationChangedEventArgs"/>.
    /// </summary>
    /// <param name="pluginId">ID of the plugin to which the configuration belongs.</param>
    /// <param name="configuration">Configuration of the plugin.</param>
    public ConfigurationChangedEventArgs(Guid pluginId, string? configuration)
    {
        PluginId = pluginId;
        Configuration = configuration;
    }
}
