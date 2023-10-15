using Microsoft.Win32;

namespace Tql.Abstractions;

public interface IStore
{
    string UserSettingsFolder { get; }

    RegistryKey CreateBaseKey();

    RegistryKey CreatePluginKey(Guid pluginId);
}
