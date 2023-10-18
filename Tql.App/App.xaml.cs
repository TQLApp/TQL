using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Tql.Abstractions;
using Tql.App.ConfigurationUI;
using Tql.App.Search;
using Tql.App.Services;
using Tql.App.Services.Database;
using Tql.App.Services.Updates;
using Tql.App.Themes;
using Tql.Plugins.Azure;
using Tql.Plugins.AzureDevOps;
using Tql.Plugins.GitHub;

namespace Tql.App;

public partial class App
{
    private IHost? _host;
    private MainWindow? _mainWindow;

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

#if !DEBUG
        logger.LogInformation("Checking for updates");

        if (TryStartUpdate(logger))
            return;
#endif

        logger.LogInformation("Initializing plugins");

        ((UI)_host.Services.GetRequiredService<IUI>()).SetSynchronizationContext(
            SynchronizationContext.Current
        );

        var pluginManager = (PluginManager)_host.Services.GetRequiredService<IPluginManager>();

        pluginManager.Initialize(_host.Services);

        logger.LogInformation("Startup complete");

        _mainWindow = _host.Services.GetRequiredService<MainWindow>();

#if DEBUG
        _mainWindow.DoShow();
#endif
    }

    private bool TryStartUpdate(ILogger<App> logger)
    {
        try
        {
            if (_host!.Services.GetRequiredService<UpdateChecker>().TryStartUpdate())
            {
                logger.LogInformation("Update is running; shutting down");

                Shutdown();
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check for updates");
        }

        return false;
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

    private IEnumerable<ITqlPlugin> GetPlugins()
    {
        // TODO: Make extensible.
        var assemblies = new[]
        {
            typeof(AzureDevOpsPlugin).Assembly,
            typeof(AzurePlugin).Assembly,
            typeof(GitHubPlugin).Assembly
        };

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.ExportedTypes)
            {
                var attribute = type.GetCustomAttribute<TqlPluginAttribute>();
                if (attribute == null)
                    continue;

                if (!typeof(ITqlPlugin).IsAssignableFrom(type))
                    throw new InvalidOperationException(
                        $"'{type}' does not implement '{nameof(ITqlPlugin)}'"
                    );

                yield return (ITqlPlugin)Activator.CreateInstance(type)!;
            }
        }
    }

    private static void ConfigureServices(IServiceCollection builder)
    {
        builder.AddSingleton<IStore, Store>();
        builder.AddSingleton<IDb, Db>();
        builder.AddSingleton<Settings>();
        builder.AddSingleton<IConfigurationManager, ConfigurationManager>();
        builder.AddSingleton<IUI, UI>();
        builder.AddSingleton<CacheManagerManager>();
        builder.AddSingleton<IClipboard, ClipboardImpl>();
        builder.AddSingleton<HttpClient>();
        builder.AddSingleton<UpdateChecker>();

        builder.AddTransient<MainWindow>();
        builder.AddTransient<ConfigurationWindow>();
        builder.AddTransient<SearchManager>();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _host?.Dispose();
    }
}
