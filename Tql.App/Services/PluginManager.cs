using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.App.Services.Packages.PackageStore;
using Tql.Utilities;

namespace Tql.App.Services;

internal class PluginManager : IPluginManager, IDisposable
{
    private readonly IPluginLoader _loader;
    private readonly ImmutableArray<AvailablePlugin> _availablePlugins;

    public ImmutableArray<IAvailablePlugin> AvailablePlugins { get; }
    public IEnumerable<ITqlPlugin> Plugins =>
        AvailablePlugins.Where(p => p.LoadException == null).Select(p => p.Plugin);

    public PluginManager(IPluginLoader loader)
    {
        // We need to keep a reference to the loader because it's also responsible
        // for resolving assemblies.
        _loader = loader;

        _availablePlugins = loader
            .GetPlugins()
            .Select(p => new AvailablePlugin(p))
            .ToImmutableArray();

        AvailablePlugins = _availablePlugins.CastArray<IAvailablePlugin>();
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

        var ui = (UI)serviceProvider.GetRequiredService<IUI>();

        foreach (var plugin in _availablePlugins.Where(p => p.LoadException != null))
        {
            ui.ShowNotificationBar(
                $"{GetType().FullName}|LoadError|{plugin.Plugin.Id}",
                string.Format(Labels.PluginManager_PluginFailedToLoad, plugin.Plugin.Title),
                Activate
            );

            void Activate()
            {
                var result = ui.ShowConfirmation(
                    ui.MainWindow!,
                    string.Format(Labels.PluginManager_PluginFailedToLoad, plugin.Plugin.Title),
                    string.Format(
                        Labels.PluginManager_PluginFailedToLoadSubtitle,
                        plugin.Plugin.Title,
                        plugin.LoadException!.Message
                    )
                );

                if (result == System.Windows.Forms.DialogResult.Yes)
                    ui.Shutdown(RestartMode.Restart);
            }
        }
    }

    public void Dispose()
    {
        _loader.Dispose();
    }

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

public interface IAvailablePlugin
{
    ITqlPlugin Plugin { get; }
    Exception? LoadException { get; }
}
