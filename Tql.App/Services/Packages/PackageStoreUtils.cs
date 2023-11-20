using System.IO;
using System.Reflection;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages;

internal static class PackageStoreUtils
{
    private static readonly string[] AssemblyExtensions = { ".dll", ".exe" };

    public static IEnumerable<string> GetAssemblyFileNames(string packageFolder)
    {
        var assemblyExtensions = new HashSet<string>(
            AssemblyExtensions,
            StringComparer.OrdinalIgnoreCase
        );

        return Directory
            .GetFiles(packageFolder, "*", SearchOption.AllDirectories)
            .Where(fileName => assemblyExtensions.Contains(Path.GetExtension(fileName)));
    }

    public static Assembly? GetLoadedAssembly(AssemblyKey assemblyKey)
    {
        var loadedAssembly = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Select(p => (AssemblyName: p.GetName(), Assembly: p))
            .Where(p => AssemblyKey.FromName(p.AssemblyName).Equals(assemblyKey))
            .OrderByDescending(p => p.AssemblyName.Version)
            .FirstOrDefault();

        return loadedAssembly.Assembly;
    }

    public static IEnumerable<Type> GetPackageTypes(Assembly assembly) =>
        GetPackageTypes(assembly, typeof(ITqlPlugin), typeof(TqlPluginAttribute));

    public static IEnumerable<Type> GetPackageTypes(
        Assembly assembly,
        Type tqlPluginType,
        Type tqlPluginAttributeType
    )
    {
        foreach (var type in assembly.GetExportedTypes())
        {
            var attribute = type.GetCustomAttribute(tqlPluginAttributeType);
            if (attribute == null)
                continue;

            if (!tqlPluginType.IsAssignableFrom(type))
            {
                throw new InvalidOperationException(
                    $"'{type}' does not implement '{nameof(ITqlPlugin)}'"
                );
            }

            yield return type;
        }
    }
}
