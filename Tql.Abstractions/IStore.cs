using Microsoft.Win32;

namespace Tql.Abstractions;

public interface IStore
{
    RegistryKey OpenKey(Guid pluginId);

    string GetDataFolder(Guid pluginId);

    string GetCacheFolder(Guid pluginId);
}
