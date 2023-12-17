using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages.AssemblyResolution;

internal class AssemblyResolver : IDisposable
{
    public static AssemblyResolver Create(
        IEnumerable<string> applicationBases,
        AssemblyLoadContext assemblyLoadContext,
        ILogger logger
    )
    {
        var assemblies = new Dictionary<AssemblyName, string>();

        foreach (var applicationBase in applicationBases)
        {
            List<string> assemblyFileNames;

            try
            {
                assemblyFileNames = AssemblyUtils.GetAssemblyFileNames(applicationBase).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Skipping application base '{ApplicationBase}' because of exception",
                    applicationBase
                );
                continue;
            }

            foreach (var fileName in assemblyFileNames)
            {
                try
                {
                    assemblies.Add(
                        AssemblyName.GetAssemblyName(fileName),
                        Path.GetFullPath(fileName)
                    );
                }
                catch (Exception ex)
                {
                    logger.LogDebug(
                        ex,
                        "Skipping file '{FileName}' because of exception",
                        fileName
                    );
                }
            }
        }

        var resolvedAssemblies = assemblies
            .GroupBy(p => AssemblyKey.FromName(p.Key))
            .ToDictionary(p => p.Key, p => p.MaxBy(p1 => p1.Key.Version).Value);

        return new AssemblyResolver(resolvedAssemblies, assemblyLoadContext, logger);
    }

    private readonly ILogger _logger;
    private readonly Dictionary<AssemblyKey, string> _assemblies;
    private readonly AssemblyLoadContext _assemblyLoadContext;

    private AssemblyResolver(
        Dictionary<AssemblyKey, string> assemblies,
        AssemblyLoadContext assemblyLoadContext,
        ILogger logger
    )
    {
        _assemblies = assemblies;
        _assemblyLoadContext = assemblyLoadContext;
        _logger = logger;

        _assemblyLoadContext.Resolving += _assemblyLoadContext_Resolving;
    }

    private Assembly? _assemblyLoadContext_Resolving(
        AssemblyLoadContext assemblyLoadContext,
        AssemblyName name
    )
    {
        // We hard code resolving the abstractions assembly. This ensures that everyone
        // is using the same one always.
        if (
            string.Equals(
                name.Name,
                typeof(ITqlPlugin).Assembly.GetName().Name,
                StringComparison.OrdinalIgnoreCase
            )
        )
            return typeof(ITqlPlugin).Assembly;

        var key = AssemblyKey.FromName(name);

        var loadedAssembly = GetLoadedAssembly(key);

        if (loadedAssembly != null)
        {
            _logger.LogDebug(
                "Using already loaded assembly '{AssemblyName}'",
                loadedAssembly.GetName()
            );

            return loadedAssembly;
        }

        if (_assemblies.TryGetValue(key, out var fileName))
        {
            _logger.LogDebug("Resolved assembly to '{FileName}'", fileName);

            return assemblyLoadContext.LoadFromAssemblyPath(fileName);
        }

        return null;
    }

    private Assembly? GetLoadedAssembly(AssemblyKey assemblyKey)
    {
        return _assemblyLoadContext
            .Assemblies.Select(p => (AssemblyName: p.GetName(), Assembly: p))
            .Where(p => AssemblyKey.FromName(p.AssemblyName).Equals(assemblyKey))
            .OrderByDescending(p => p.AssemblyName.Version)
            .FirstOrDefault()
            .Assembly;
    }

    public void Dispose()
    {
        _assemblyLoadContext.Resolving -= _assemblyLoadContext_Resolving;
    }
}
