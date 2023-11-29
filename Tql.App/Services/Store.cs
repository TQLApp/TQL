using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services;

internal class Store : IStore
{
    private const string RegistryRoot = "Software";

    private readonly ILogger _logger;
    private readonly string _environmentName;
    private readonly string _localDataFolder;

    public string DataFolder { get; }
    public string CacheFolder { get; }
    public string LogFolder { get; }
    public string PackagesFolder { get; }

    public Store(string? environment, ILogger logger)
    {
        _logger = logger;
        _environmentName = "TQL";
        if (environment != null)
            _environmentName += $" - {environment}";

        DataFolder = EnsureFolder(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            _environmentName
        );
        _localDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            _environmentName
        );
        CacheFolder = EnsureFolder(_localDataFolder, "Cache");
        LogFolder = EnsureFolder(_localDataFolder, "Log");
        PackagesFolder = EnsureFolder(_localDataFolder, "Packages");
    }

    public RegistryKey CreateBaseKey()
    {
        return Registry.CurrentUser.CreateSubKey($"{RegistryRoot}\\{_environmentName}")!;
    }

    public string GetCacheFolder(Guid pluginId) =>
        EnsureFolder(CacheFolder, "Plugins", pluginId.ToString());

    private string EnsureFolder(params string[] paths)
    {
        var folder = Path.Combine(paths);

        Directory.CreateDirectory(folder);
        return folder;
    }

    public void Reset()
    {
        _logger.LogInformation("Resetting store");

        Retry(() =>
        {
            _logger.LogInformation(
                "Deleting registry key '{RegistryKey}'",
                $"{RegistryRoot}\\{_environmentName}"
            );

            using var key = Registry.CurrentUser.CreateSubKey(RegistryRoot);

            bool exists;

            using (var subKey = key.OpenSubKey(_environmentName))
            {
                exists = subKey != null;
            }

            if (exists)
                key.DeleteSubKeyTree(_environmentName);
        });

        Retry(() =>
        {
            _logger.LogInformation("Deleting local app data");

            DeleteDirectory(_localDataFolder);
        });

        Retry(() =>
        {
            _logger.LogInformation("Deleting roaming app data");

            DeleteDirectory(DataFolder);
        });
    }

    private void DeleteDirectory(string folder)
    {
        foreach (var fileName in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
        {
            _logger.LogDebug("Deleting {FileName}", fileName);

            File.SetAttributes(fileName, FileAttributes.Normal);
            File.Delete(fileName);
        }

        _logger.LogDebug("Deleting {Folder}", folder);

        Directory.Delete(folder, true);
    }

    private void Retry(Action action)
    {
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                action();
                return;
            }
            catch (Exception ex) when (attempt < 5)
            {
                _logger.LogWarning(ex, "Operation failed, retrying");
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
