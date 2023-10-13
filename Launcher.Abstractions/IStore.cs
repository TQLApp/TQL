using Microsoft.Win32;

namespace Launcher.Abstractions;

public interface IStore
{
    string UserSettingsFolder { get; }

    RegistryKey CreateBaseKey();

    RegistryKey CreatePluginKey(Guid pluginId);
}
