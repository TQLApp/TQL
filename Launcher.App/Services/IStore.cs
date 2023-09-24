using Microsoft.Win32;

namespace Launcher.App.Services;

internal interface IStore
{
    string UserSettingsFolder { get; }

    RegistryKey CreateBaseKey();
}
