using System.IO;
using Microsoft.Win32;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services;

internal class Store : IStore
{
    public string DataFolder { get; }
    public string CacheFolder { get; }

    public Store()
    {
        DataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
#if DEBUG
            "TQL - Debug"
#else
            "TQL"
#endif
        );

        Directory.CreateDirectory(DataFolder);

        CacheFolder = Path.Combine(DataFolder, "Cache");

        Directory.CreateDirectory(CacheFolder);
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

    public RegistryKey OpenKey(Guid pluginId)
    {
        using var key = CreateBaseKey();

        return key.CreateSubKey($"Plugins\\{pluginId}")!;
    }

    public string GetDataFolder(Guid pluginId)
    {
        var path = Path.Combine(DataFolder, "Plugins", pluginId.ToString());

        Directory.CreateDirectory(path);

        return path;
    }

    public string GetCacheFolder(Guid pluginId)
    {
        var path = Path.Combine(CacheFolder, "Plugins", pluginId.ToString());

        Directory.CreateDirectory(path);

        return path;
    }
}
