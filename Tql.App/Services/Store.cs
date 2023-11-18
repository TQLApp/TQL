using System.IO;
using Microsoft.Win32;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services;

internal class Store : IStore
{
    private readonly string _environmentName;

    public string PackagesFolder { get; }
    public string DataFolder { get; }
    public string CacheFolder { get; }
    public string LogFolder { get; }

    public Store(string? environment)
    {
        _environmentName = "TQL";
        if (environment != null)
            _environmentName += $" - {environment}";

        DataFolder = EnsureFolder(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            _environmentName
        );
        var localFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            _environmentName
        );
        CacheFolder = EnsureFolder(localFolder, "Cache");
        LogFolder = EnsureFolder(localFolder, "Log");
        PackagesFolder = EnsureFolder(localFolder, "Packages");
    }

    public RegistryKey CreateBaseKey()
    {
        return Registry.CurrentUser.CreateSubKey($"Software\\{_environmentName}")!;
    }

    public string GetCacheFolder(Guid pluginId) =>
        EnsureFolder(CacheFolder, "Plugins", pluginId.ToString());

    private string EnsureFolder(params string[] paths)
    {
        var folder = Path.Combine(paths);

        Directory.CreateDirectory(folder);
        return folder;
    }
}
