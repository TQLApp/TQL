using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.App.Services.Packages;
using Tql.App.Services.Packages.PackageStore;
using Tql.App.Services.UIService;
using Tql.App.Support;
using Tql.Utilities;

namespace Tql.App.Services;

internal class PluginManager : IPluginManager, IDisposable
{
    private readonly IPackageLoader _loader;
    private readonly ImmutableArray<AvailablePlugin> _availablePlugins;
    private ImmutableArray<ITqlPlugin>? _plugins;
    private readonly object _syncRoot = new();

    public ImmutableArray<IAvailablePackage> Packages { get; }

    public ImmutableArray<ITqlPlugin> Plugins
    {
        get
        {
            lock (_syncRoot)
            {
                if (!_plugins.HasValue)
                    throw new InvalidOperationException("Plugins are not yet available");

                return _plugins.Value;
            }
        }
    }

    public PluginManager(IPackageLoader loader, IProgress progress)
    {
        // We need to keep a reference to the loader because it's also responsible
        // for resolving assemblies.
        _loader = loader;

        var packages = ImmutableArray.CreateBuilder<IAvailablePackage>();
        var plugins = ImmutableArray.CreateBuilder<AvailablePlugin>();

        foreach (var package in loader.GetPackages(progress))
        {
            if (package.Plugins.HasValue)
            {
                var packagePlugins = package
                    .Plugins.Value.Select(p => new AvailablePlugin(p))
                    .ToList();

                plugins.AddRange(packagePlugins);
                packages.Add(
                    new AvailablePackage(
                        package.Id,
                        packagePlugins.ToImmutableArray<IAvailablePlugin>(),
                        package.LoadException
                    )
                );
            }
            else
            {
                packages.Add(new AvailablePackage(package.Id, null, package.LoadException));
            }
        }

        Packages = packages.ToImmutable();

        _availablePlugins = plugins.ToImmutable();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        foreach (var plugin in _availablePlugins.Where(p => p.LoadException == null))
        {
            try
            {
                plugin.Plugin.ConfigureServices(services);
            }
            catch (Exception ex)
            {
                plugin.LoadException = ex;
            }
        }
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        foreach (var plugin in _availablePlugins.Where(p => p.LoadException == null))
        {
            try
            {
                plugin.Plugin.Initialize(serviceProvider);
            }
            catch (Exception ex)
            {
                plugin.LoadException = ex;
            }
        }

        lock (_syncRoot)
        {
            _plugins = _availablePlugins
                .Where(p => p.LoadException == null)
                .Select(p => p.Plugin)
                .ToImmutableArray();
        }

        ShowLoadErrors(serviceProvider);
    }

    private void ShowLoadErrors(IServiceProvider serviceProvider)
    {
        var ui = (UI)serviceProvider.GetRequiredService<IUI>();

        foreach (var package in Packages)
        {
            if (package.LoadException != null)
            {
                ui.ShowNotificationBar(
                    $"{GetType().FullName}|PackageLoadError|{package.Id}",
                    string.Format(Labels.PluginManager_PackageFailedToLoad, package.Id.Id),
                    Activate
                );

                void Activate(IWin32Window owner)
                {
                    var result = ui.ShowConfirmation(
                        owner,
                        string.Format(Labels.PluginManager_PackageFailedToLoad, package.Id.Id),
                        string.Format(
                            Labels.PluginManager_PackageFailedToLoadSubtitle,
                            package.Id.Id,
                            package.LoadException!.Message
                        )
                    );

                    if (result == DialogResult.Yes)
                        ui.Shutdown(RestartMode.Restart);
                }
            }

            if (package.Plugins.HasValue)
            {
                foreach (var plugin in package.Plugins.Value.Where(p => p.LoadException != null))
                {
                    ui.ShowNotificationBar(
                        $"{GetType().FullName}|PluginLoadError|{plugin.Plugin.Id}",
                        string.Format(Labels.PluginManager_PluginFailedToLoad, plugin.Plugin.Title),
                        Activate
                    );

                    void Activate(IWin32Window owner)
                    {
                        var result = ui.ShowConfirmation(
                            owner,
                            string.Format(
                                Labels.PluginManager_PluginFailedToLoad,
                                plugin.Plugin.Title
                            ),
                            string.Format(
                                Labels.PluginManager_PluginFailedToLoadSubtitle,
                                plugin.Plugin.Title,
                                plugin.LoadException!.Message
                            )
                        );

                        if (result == DialogResult.Yes)
                            ui.Shutdown(RestartMode.Restart);
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        _loader.Dispose();
    }

    private record AvailablePackage(
        PackageRef Id,
        ImmutableArray<IAvailablePlugin>? Plugins,
        Exception? LoadException
    ) : IAvailablePackage;

    private record AvailablePlugin(ITqlPlugin Plugin) : IAvailablePlugin
    {
        private volatile Exception? _loadException;

        public Exception? LoadException
        {
            get => _loadException;
            set => _loadException = value;
        }
    }
}

internal interface IAvailablePackage
{
    PackageRef Id { get; }
    ImmutableArray<IAvailablePlugin>? Plugins { get; }
    Exception? LoadException { get; }
}

internal interface IAvailablePlugin
{
    ITqlPlugin Plugin { get; }
    Exception? LoadException { get; }
}
