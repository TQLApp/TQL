using System.IO;
using Microsoft.Win32;

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

    public RegistryKey CreateBaseKey() => Registry.CurrentUser.CreateSubKey("Software\\Launcher");
}
