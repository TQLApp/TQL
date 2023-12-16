using Microsoft.Extensions.Logging;

namespace Tql.App.Services.Packages.PackageStore;

internal partial class PackageStoreManager
{
    private void MigrateConfiguration()
    {
        if (_configurationManager.GetConfiguration(Constants.PackageStoreConfigurationId) != null)
            return;

        _logger.LogInformation("Migrating package store configuration");

        var packages = ImmutableArray.CreateBuilder<PackageRef>();

        using var baseKey = _store.CreateBaseKey();

        using (var key = baseKey.OpenSubKey("Packages"))
        {
            if (key != null)
            {
                foreach (var id in key.GetValueNames())
                {
                    if (key.GetValue(id) is string version)
                        packages.Add(new PackageRef(id, version));
                }
            }
        }

        Configuration = new ConfigurationDto(packages.ToImmutable());

        baseKey.DeleteSubKeyTree("Packages", false);
    }
}
