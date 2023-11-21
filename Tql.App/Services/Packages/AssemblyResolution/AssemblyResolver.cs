using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Tql.App.Services.Packages.AssemblyResolution;

internal class AssemblyResolver : IDisposable
{
    public static AssemblyResolver Create(IEnumerable<string> applicationBases, ILogger logger)
    {
        var assemblies = new Dictionary<AssemblyName, string>();

        foreach (var applicationBase in applicationBases)
        {
            foreach (var fileName in AssemblyUtils.GetAssemblyFileNames(applicationBase))
            {
                try
                {
                    assemblies.Add(AssemblyName.GetAssemblyName(fileName), fileName);
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
            .ToDictionary(p => p.Key, p => p.OrderByDescending(p1 => p1.Key.Version).First().Value);

        return new AssemblyResolver(resolvedAssemblies, logger);
    }

    private readonly ILogger _logger;
    private readonly Dictionary<AssemblyKey, string> _assemblies;

    private AssemblyResolver(Dictionary<AssemblyKey, string> assemblies, ILogger logger)
    {
        _assemblies = assemblies;
        _logger = logger;

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name);
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

            return Assembly.LoadFile(fileName);
        }

        return null;
    }

    private Assembly? GetLoadedAssembly(AssemblyKey assemblyKey)
    {
        return AppDomain
            .CurrentDomain
            .GetAssemblies()
            .Select(p => (AssemblyName: p.GetName(), Assembly: p))
            .Where(p => AssemblyKey.FromName(p.AssemblyName).Equals(assemblyKey))
            .OrderByDescending(p => p.AssemblyName.Version)
            .FirstOrDefault()
            .Assembly;
    }

    public void Dispose()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
    }
}
