using System.IO;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages.AssemblyResolution;

internal static class AssemblyUtils
{
    private static readonly string[] AssemblyExtensions = { ".dll", ".exe" };

    public static IEnumerable<string> GetAssemblyFileNames(string applicationBase)
    {
        var assemblyExtensions = new HashSet<string>(
            AssemblyExtensions,
            StringComparer.OrdinalIgnoreCase
        );

        return Directory
            .GetFiles(applicationBase, "*", SearchOption.AllDirectories)
            .Where(p => assemblyExtensions.Contains(Path.GetExtension(p)));
    }
}
