using System.IO;
using Microsoft.Win32;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.PluginTestSupport.Services;

internal class TestStore : IStore
{
    public string DataFolder { get; }
    public string CacheFolder { get; }

    public TestStore()
    {
        DataFolder = "Data";

        Directory.CreateDirectory(DataFolder);

        CacheFolder = Path.Combine(DataFolder, "Cache");

        Directory.CreateDirectory(CacheFolder);
    }

    public RegistryKey CreateBaseKey()
    {
        return Registry.CurrentUser.CreateSubKey("Software\\TQL - Unit Test")!;
    }

    public string GetCacheFolder(Guid pluginId)
    {
        var path = Path.Combine(CacheFolder, "Plugins", pluginId.ToString());

        Directory.CreateDirectory(path);

        return path;
    }
}
