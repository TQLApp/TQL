using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.ConfigurationUI;
using Tql.App.Search;
using Tql.App.Services;
using Tql.App.Services.Database;
using Tql.App.Services.Packages;
using Tql.App.Services.Telemetry;
using Tql.App.Services.Updates;
using Tql.App.Support;
using ConfigurationManager = Tql.App.Services.ConfigurationManager;

namespace Tql.App;

public partial class App
{
    private IHost? _host;
    private MainWindow? _mainWindow;

    public static bool RestartRequested { get; set; }
    public static ImmutableArray<Assembly>? DebugAssemblies { get; set; }
    public static bool IsDebugMode { get; set; }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        System.Windows.Forms.Application.EnableVisualStyles();

        var store = new Store();
        var packageStoreManager = new PackageStoreManager(store);

        packageStoreManager.PerformCleanup();

        ImmutableArray<ITqlPlugin> plugins;
        if (DebugAssemblies.HasValue)
            plugins = GetDebugPlugins().ToImmutableArray();
        else
            plugins = packageStoreManager.GetPlugins();

        var builder = Host.CreateApplicationBuilder(e.Args);

        var inMemoryLoggerProvider = new InMemoryLoggerProvider();

        builder.Logging.AddProvider(inMemoryLoggerProvider);

        ConfigureServices(
            builder.Services,
            store,
            packageStoreManager,
            plugins,
            inMemoryLoggerProvider
        );

        foreach (var plugin in plugins)
        {
            plugin.ConfigureServices(builder.Services);
        }

        _host = builder.Build();

        SetTheme(_host.Services.GetRequiredService<Settings>());

        var logger = _host.Services.GetRequiredService<ILogger<App>>();

        if (!IsDebugMode)
        {
            logger.LogInformation("Checking for updates");

            if (TryStartUpdate(logger))
                return;
        }

        logger.LogInformation("Initializing plugins");

        ((UI)_host.Services.GetRequiredService<IUI>()).SetSynchronizationContext(
            SynchronizationContext.Current
        );

        var pluginManager = (PluginManager)_host.Services.GetRequiredService<IPluginManager>();

        pluginManager.Initialize(_host.Services);

        logger.LogInformation("Startup complete");

        _mainWindow = _host.Services.GetRequiredService<MainWindow>();

        if (IsDebugMode)
            _mainWindow.DoShow();
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

    private IEnumerable<ITqlPlugin> GetDebugPlugins()
    {
        foreach (var assembly in DebugAssemblies!.Value)
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
        Store store,
        PackageStoreManager packageStoreManager,
        ImmutableArray<ITqlPlugin> plugins,
        InMemoryLoggerProvider inMemoryLoggerProvider
    )
    {
        builder.AddSingleton<IStore>(store);
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
        builder.AddSingleton<PackageManager>();
        builder.AddSingleton(packageStoreManager);
        builder.AddSingleton(inMemoryLoggerProvider);

        builder.Add(ServiceDescriptor.Singleton(typeof(ICache<>), typeof(Cache<>)));

        builder.AddTransient<MainWindow>();
        builder.AddTransient<ConfigurationWindow>();
        builder.AddTransient<FeedbackWindow>();
        builder.AddTransient<SearchManager>();
        builder.AddTransient<GeneralConfigurationControl>();
        builder.AddTransient<PluginsConfigurationControl>();
        builder.AddTransient<PackageSourcesConfigurationControl>();
    }

    private void Application_Exit(object sender, ExitEventArgs e)
    {
        _host?.Dispose();

        if (RestartRequested)
            Process.Start(Assembly.GetEntryAssembly()!.Location);
    }
}
