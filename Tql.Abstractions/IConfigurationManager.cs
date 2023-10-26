namespace Tql.Abstractions;

public interface IConfigurationManager
{
    event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

    string? GetConfiguration(Guid pluginId);

    void SetConfiguration(Guid pluginId, string? configuration);
}
