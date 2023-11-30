using System.Reflection;
using Tql.Abstractions;

namespace Tql.App.Services.Packages.PackageStore;

internal static class PluginLoaderUtils
{
    public static IEnumerable<Type> GetPluginTypes(Assembly assembly)
    {
        foreach (var type in assembly.GetExportedTypes())
        {
            var attribute = type.GetCustomAttribute<TqlPluginAttribute>();
            if (attribute == null)
                continue;

            if (!typeof(ITqlPlugin).IsAssignableFrom(type))
            {
                throw new InvalidOperationException(
                    $"'{type}' does not implement '{nameof(ITqlPlugin)}'"
                );
            }

            yield return type;
        }
    }
}
