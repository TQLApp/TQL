namespace Launcher.Abstractions;

public class ConfigurationChangedEventArgs : EventArgs
{
    public Guid PluginId { get; }
    public string? Configuration { get; }

    public ConfigurationChangedEventArgs(Guid pluginId, string? configuration)
    {
        PluginId = pluginId;
        Configuration = configuration;
    }
}
