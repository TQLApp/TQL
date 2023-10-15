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
#if DEBUG
            "TQL - Debug"
#else
            "TQL"
#endif
        );

        Directory.CreateDirectory(UserSettingsFolder);
    }

    public RegistryKey CreateBaseKey()
    {
        return Registry.CurrentUser.CreateSubKey("Software\\" +
#if DEBUG
                "TQL - Debug"
#else
                "TQL"
#endif
        )!;
    }

    public RegistryKey CreatePluginKey(Guid pluginId)
    {
        using var key = CreateBaseKey();

        return key.CreateSubKey($"Plugins\\{pluginId}")!;
    }
}
