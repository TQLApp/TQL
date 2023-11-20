using System.Reflection;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages;

internal class PackageEntryResolver
{
    public static List<PackageEntry> Resolve(string path, ILogger logger)
    {
        var appDomain = AppDomain.CreateDomain(
            nameof(PackageEntryResolver),
            AppDomain.CurrentDomain.Evidence,
            new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                ApplicationName = nameof(PackageEntryResolver)
            }
        );

        try
        {
            var loader = (Loader)
                appDomain.CreateInstanceAndUnwrap(
                    typeof(Loader).Assembly.FullName,
                    typeof(Loader).FullName!
                );

            return loader
                .Load(
                    path,
                    Path.GetFileName(typeof(ITqlPlugin).Assembly.Location),
                    typeof(ITqlPlugin).FullName!,
                    typeof(TqlPluginAttribute).FullName!,
                    new Logger(logger)
                )
                .Select(p => new PackageEntry(p.FileName, p.TypeName))
                .ToList();
        }
        finally
        {
            AppDomain.Unload(appDomain);
        }
    }

    private class Logger : MarshalByRefObject
    {
        private readonly ILogger _logger;

        public Logger(ILogger logger)
        {
            _logger = logger;
        }

        public void Log(LogLevel level, string message, params object?[] args)
        {
            _logger.Log(level, message, args);
        }
    }

    private class Loader : MarshalByRefObject
    {
        public List<(string FileName, string TypeName)> Load(
            string path,
            string abstractionsAssemblyFileName,
            string tqlPluginTypeFullName,
            string tqlPluginAttributeTypeFullName,
            Logger logger
        )
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var assemblies = new List<(string FileName, Assembly Assembly)>();
            var abstractionsAssembly = default(Assembly);

            foreach (var fileName in PackageStoreUtils.GetAssemblyFileNames(path))
            {
                if (!fileName.StartsWith(path))
                    throw new InvalidOperationException();

                var relativeFileName = fileName
                    .Substring(path.Length)
                    .TrimStart(Path.DirectorySeparatorChar);

                logger.Log(LogLevel.Debug, "Loading {FileName}", relativeFileName);

                var assembly = Assembly.LoadFrom(fileName);
                assemblies.Add((relativeFileName, assembly));

                if (
                    string.Equals(
                        Path.GetFileName(fileName),
                        abstractionsAssemblyFileName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                    abstractionsAssembly = assembly;
            }

            if (abstractionsAssembly == null)
            {
                throw new InvalidOperationException(
                    "Package does not contain an abstractions assembly"
                );
            }

            var tqlPluginType = abstractionsAssembly.GetType(tqlPluginTypeFullName);
            var tqlPluginAttributeType = abstractionsAssembly.GetType(
                tqlPluginAttributeTypeFullName
            );

            var entries = new List<(string FileName, string TypeName)>();

            foreach (var assembly in assemblies)
            {
                logger.Log(LogLevel.Debug, "Inspecting {FileName}", assembly.FileName);

                try
                {
                    foreach (
                        var type in PackageStoreUtils.GetPackageTypes(
                            assembly.Assembly,
                            tqlPluginType,
                            tqlPluginAttributeType
                        )
                    )
                    {
                        logger.Log(LogLevel.Information, "Discovered {TypeName}", type.FullName);

                        entries.Add((assembly.FileName, type.FullName!));
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(
                        LogLevel.Warning,
                        "Failed to inspect {FileName}: {Message} ({Type})",
                        assembly.FileName,
                        ex.Message,
                        ex.GetType().FullName
                    );
                }
            }

            return entries;
        }

        private Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyKey = AssemblyKey.FromName(new AssemblyName(args.Name));

            return PackageStoreUtils.GetLoadedAssembly(assemblyKey);
        }
    }
}

internal record PackageEntry(string FileName, string TypeName);
