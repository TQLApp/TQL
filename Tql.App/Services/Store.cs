using System.IO;
using Microsoft.Win32;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services;

internal class Store : IStore
{
    public string UserSettingsFolder { get; }

    public Store()
    {
        UserSettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TQL"
        );

        Directory.CreateDirectory(UserSettingsFolder);
    }

    public RegistryKey CreateBaseKey() => Registry.CurrentUser.CreateSubKey("Software\\TQL")!;

    public RegistryKey CreatePluginKey(Guid pluginId)
    {
        using var key = CreateBaseKey();

        return key.CreateSubKey($"Plugins\\{pluginId}")!;
    }
}
