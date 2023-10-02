using Launcher.Abstractions;
using Launcher.App.Services;
using Launcher.Plugins.AzureDevOps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Launcher.App.Services.Database;
using Launcher.App.ConfigurationUI;
using Microsoft.Win32;
using FramePFX.Themes;
using Launcher.App.Search;
using Microsoft.Extensions.Logging;

namespace Launcher.App;

public partial class App
{
    private IHost? _host;
    private IServiceScope? _scope;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        System.Windows.Forms.Application.EnableVisualStyles();

        SetMode();

        var plugins = GetPlugins().ToImmutableArray();

        var builder = Host.CreateApplicationBuilder(e.Args);

        ConfigureServices(builder.Services);

        builder.Services.AddSingleton<IPluginManager>(new PluginManager(plugins));

        builder.Services.Add(ServiceDescriptor.Singleton(typeof(ICache<>), typeof(Cache<>)));

        foreach (var plugin in plugins)
        {
            plugin.ConfigureServices(builder.Services);
        }

        _host = builder.Build();

        var logger = _host.Services.GetRequiredService<ILogger<App>>();

        logger.LogInformation("Initializing plugins");

        var pluginManager = (PluginManager)_host.Services.GetRequiredService<IPluginManager>();

        pluginManager.Initialize(_host.Services);

        logger.LogInformation("Startup complete");

        _scope = _host.Services.CreateScope();

        _scope.ServiceProvider.GetRequiredService<MainWindow>().Show();
    }

    private void SetMode()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"
        );

        var isLight = (key?.GetValue("AppsUseLightTheme") as int?) == 1;

        if (isLight)
            ThemesController.SetTheme(ThemeType.LightTheme);
        else
            ThemesController.SetTheme(ThemeType.SoftDark);
    }

    private IEnumerable<ILauncherPlugin> GetPlugins()
    {
        // TODO: Make extensible.
        var assemblies = new[] { typeof(AzureDevOpsPlugin).Assembly };

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.ExportedTypes)
            {
                var attribute = type.GetCustomAttribute<LauncherPluginAttribute>();
                if (attribute == null)
                    continue;

                if (!typeof(ILauncherPlugin).IsAssignableFrom(type))
                    throw new InvalidOperationException(
                        $"'{type}' does not implement '{nameof(ILauncherPlugin)}'"
                    );

                yield return (ILauncherPlugin)Activator.CreateInstance(type)!;
            }
        }
    }

    private static void ConfigureServices(IServiceCollection builder)
    {
        builder.AddSingleton<IImageFactory, ImageFactory>();
        builder.AddSingleton<IStore, Store>();
        builder.AddSingleton<IDb, Db>();
        builder.AddSingleton<Settings>();
        builder.AddSingleton<IConfigurationManager, ConfigurationManager>();
        builder.AddSingleton<IUI, UI>();

        builder.AddTransient<MainWindow>();
        builder.AddTransient<ConfigurationWindow>();
        builder.AddTransient<SearchManager>();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _scope?.Dispose();
        _host?.Dispose();
    }
}
