using System.Net.Http;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.ConfigurationUI;
using Tql.App.Search;
using Tql.App.Services;
using Tql.App.Services.Database;
using Tql.App.Services.Telemetry;
using Tql.App.Services.Updates;
using Tql.App.Support;
using Tql.Plugins.Azure;
using Tql.Plugins.AzureDevOps;
using Tql.Plugins.Confluence;
using Tql.Plugins.GitHub;
using Tql.Plugins.Jira;
using Tql.Plugins.MicrosoftTeams;
using ConfigurationManager = Tql.App.Services.ConfigurationManager;

namespace Tql.App;

public partial class App
{
    private IHost? _host;
    private MainWindow? _mainWindow;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        System.Windows.Forms.Application.EnableVisualStyles();

        var plugins = GetPlugins().ToImmutableArray();

        var builder = Host.CreateApplicationBuilder(e.Args);

        ConfigureServices(builder.Services, plugins);

        foreach (var plugin in plugins)
        {
            plugin.ConfigureServices(builder.Services);
        }

        _host = builder.Build();

        SetTheme(_host.Services.GetRequiredService<Settings>());

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

    private void SetTheme(Settings settings)
    {
        DoSetTheme();

        settings.AttachPropertyChanged(nameof(settings.Theme), (_, _) => DoSetTheme());

        void DoSetTheme()
        {
            ThemeManager.SetTheme(ThemeManager.ParseTheme(settings.Theme));
        }
    }

    [UsedImplicitly]
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

    private IEnumerable<ITqlPlugin> GetPlugins()
    {
        // TODO: Make extensible.
        var assemblies = new[]
        {
            typeof(AzureDevOpsPlugin).Assembly,
            typeof(AzurePlugin).Assembly,
            typeof(GitHubPlugin).Assembly,
            typeof(JiraPlugin).Assembly,
            typeof(ConfluencePlugin).Assembly,
            typeof(MicrosoftTeamsPlugin).Assembly
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

    private static void ConfigureServices(
        IServiceCollection builder,
        ImmutableArray<ITqlPlugin> plugins
    )
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
        builder.AddSingleton<TelemetryService>();
        builder.AddSingleton<IPeopleDirectoryManager, PeopleDirectoryManager>();
        builder.AddSingleton<HotKeyService>();
        builder.AddSingleton<IPluginManager>(new PluginManager(plugins));

        builder.Add(ServiceDescriptor.Singleton(typeof(ICache<>), typeof(Cache<>)));

        builder.AddTransient<MainWindow>();
        builder.AddTransient<ConfigurationWindow>();
        builder.AddTransient<FeedbackWindow>();
        builder.AddTransient<SearchManager>();
        builder.AddTransient<GeneralConfigurationControl>();
        builder.AddTransient<PluginsConfigurationControl>();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _host?.Dispose();
    }
}
