using System.IO;
using Launcher.Abstractions;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace Launcher.App.Services;

internal class Store : IStore
{
    public string UserSettingsFolder { get; }

    public Store()
    {
        UserSettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Launcher"
        );

        Directory.CreateDirectory(UserSettingsFolder);
    }

    public RegistryKey CreateBaseKey() => Registry.CurrentUser.CreateSubKey("Software\\Launcher")!;

    public RegistryKey CreatePluginKey(Guid pluginId)
    {
        using var key = CreateBaseKey();

        return key.CreateSubKey($"Plugins\\{pluginId}")!;
    }
}
