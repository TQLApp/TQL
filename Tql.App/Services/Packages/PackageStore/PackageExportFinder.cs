using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace Tql.App.Services.Packages.PackageStore;

internal class PackageExportFinder
{
    public static List<PackageExport> Resolve(string path, ILogger logger)
    {
        var context = new AssemblyLoadContext(nameof(PackageExportFinder), true);

        try
        {
            using var loader = new SideloadedPluginLoader(context, path, logger);

            var entries = new List<(string FileName, string TypeName)>();

            foreach (var type in loader.GetPluginTypes())
            {
                logger.Log(LogLevel.Information, "Discovered {TypeName}", type.FullName);

                entries.Add((type.Assembly.Location, type.FullName!));
            }

            return entries.Select(p => new PackageExport(p.FileName, p.TypeName)).ToList();
        }
        finally
        {
            context.Unload();
        }
    }
}
