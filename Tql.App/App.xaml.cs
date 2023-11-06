using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows.Markup;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.ConfigurationUI;
using Tql.App.QuickStart;
using Tql.App.Search;
using Tql.App.Services;
using Tql.App.Services.Database;
using Tql.App.Services.Packages;
using Tql.App.Services.Telemetry;
using Tql.App.Services.Updates;
using Tql.App.Support;
using ConfigurationManager = Tql.App.Services.ConfigurationManager;
using Path = System.IO.Path;

namespace Tql.App;

public partial class App
{
    private IHost? _host;
    private MainWindow? _mainWindow;
    private WindowMessageIPC? _ipc;

    public static RestartMode RestartMode { get; set; } = RestartMode.Shutdown;
    public static ImmutableArray<Assembly>? DebugAssemblies { get; set; }
    public static bool IsDebugMode { get; set; }
    internal static Options Options { get; private set; } = new();

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Parser.Default.ParseArguments<Options>(FixupArgs(e.Args)).WithParsed(p => Options = p);

        _ipc = new WindowMessageIPC();

        if (!_ipc.IsFirstRunner)
        {
            Shutdown(0);
            return;
        }

        System.Windows.Forms.Application.EnableVisualStyles();

        var notifyIconManager = new NotifyIconManager();

        var store = new Store(Options.Environment);
        var (loggerFactory, inMemoryLoggerProvider) = SetupLogging(store);

        var packageStoreManager = new PackageStoreManager(
            store,
            loggerFactory.CreateLogger<PackageStoreManager>()
        );

        packageStoreManager.PerformCleanup();

        ImmutableArray<ITqlPlugin> plugins;
        if (DebugAssemblies.HasValue)
            plugins = GetDebugPlugins().ToImmutableArray();
        else
            plugins = packageStoreManager.GetPlugins();

        var builder = Host.CreateApplicationBuilder(e.Args);

        builder.Services.AddSingleton(loggerFactory);
        builder.Services.AddSingleton(inMemoryLoggerProvider);
        builder.Services.AddSingleton(notifyIconManager);

        ConfigureServices(builder.Services, store, packageStoreManager, plugins);

        foreach (var plugin in plugins)
        {
            plugin.ConfigureServices(builder.Services);
        }

        _host = builder.Build();

        var settings = _host.Services.GetRequiredService<Settings>();

        if (settings.Language != null)
            SetCulture(CultureInfo.GetCultureInfo(settings.Language));

        ThemeManager.SetTheme(ThemeManager.ParseTheme(settings.Theme));

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

        _ipc.Received += (_, _) => _mainWindow.DoShow();

        if (!Options.IsSilent)
            _mainWindow.DoShow();
    }

    private void SetCulture(CultureInfo culture)
    {
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        foreach (Window window in Current.Windows)
        {
            window.Language = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
        }
    }

    private (ILoggerFactory, InMemoryLoggerProvider) SetupLogging(Store store)
    {
        var logDirectory = Path.Combine(store.DataFolder, "Log");
        Directory.CreateDirectory(logDirectory);

        var inMemoryLoggerProvider = new InMemoryLoggerProvider();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            if (IsDebugMode)
            {
                builder.AddConsole();
                builder.AddDebug();
            }

            builder.AddFile(
                Path.Combine(logDirectory, "Log.log"),
                options =>
                {
                    options.FileSizeLimitBytes = 1 * 1024 * 1024;
                    options.MaxRollingFiles = 5;
                    options.Append = true;
                }
            );

            builder.AddProvider(inMemoryLoggerProvider);

#if DEBUG
            builder.SetMinimumLevel(LogLevel.Debug);
#endif
        });

        var logger = loggerFactory.CreateLogger<App>();

        logger.LogInformation("Starting application");

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            logger.LogCritical((Exception)e.ExceptionObject, "Unhandled AppDomain exception");
        };

        DispatcherUnhandledException += (_, e) =>
        {
            logger.LogCritical(e.Exception, "Unhandled dispatcher exception");

            if (!IsDebugMode)
                e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            logger.LogCritical(e.Exception, "Unobserved task exception");

            if (!IsDebugMode)
                e.SetObserved();
        };

        return (loggerFactory, inMemoryLoggerProvider);
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
        ImmutableArray<ITqlPlugin> plugins
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
        builder.AddSingleton<QuickStartManager>();

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
        _ipc?.Dispose();

        if (RestartMode is RestartMode.Restart or RestartMode.SilentRestart)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Assembly.GetEntryAssembly()!.Location
            };
            if (RestartMode == RestartMode.SilentRestart)
                startInfo.Arguments += " --silent";
            if (Options.Environment != null)
                startInfo.Arguments += $" --env \"{Options.Environment}\"";

            Process.Start(startInfo);
        }
    }

    private static IEnumerable<string> FixupArgs(IEnumerable<string> args)
    {
        foreach (var arg in args)
        {
            if (string.Equals(arg, "/silent", StringComparison.OrdinalIgnoreCase))
                yield return "--silent";
            else
                yield return arg;
        }
    }
}
