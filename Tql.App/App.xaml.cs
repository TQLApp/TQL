using System.Diagnostics;
using System.Globalization;
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
using Tql.App.Services.Packages.PackageStore;
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

    // It's a puzzler why this property is not public but internal.
    public static bool IsShuttingDown =>
        (bool)
            typeof(Application)
                .GetProperty(nameof(IsShuttingDown), BindingFlags.NonPublic | BindingFlags.Static)!
                .GetValue(null);

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        Parser.Default.ParseArguments<Options>(FixupArgs(e.Args)).WithParsed(p => Options = p);

        _ipc = new WindowMessageIPC(Options.Environment);

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

        IPluginLoader loader;
        if (Options.Sideload != null)
            loader = new SideloadedPluginLoader(
                Options.Sideload,
                loggerFactory.CreateLogger<SideloadedPluginLoader>()
            );
        else if (DebugAssemblies.HasValue)
            loader = new AssemblyPluginLoader(DebugAssemblies.Value);
        else
            loader = new PackagesPluginLoader(
                packageStoreManager,
                loggerFactory.CreateLogger<PackagesPluginLoader>()
            );

        var pluginManager = new PluginManager(loader);

        var builder = Host.CreateApplicationBuilder(e.Args);

        builder.Services.AddSingleton(loggerFactory);
        builder.Services.AddSingleton(inMemoryLoggerProvider);
        builder.Services.AddSingleton(notifyIconManager);

        ConfigureServices(builder.Services, store, packageStoreManager, pluginManager);

        pluginManager.ConfigureServices(builder.Services);

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
        else
        {
            notifyIconManager.State = NotifyIconState.Running;
        }

        logger.LogInformation("Initializing plugins");

        ((UI)_host.Services.GetRequiredService<IUI>()).SetSynchronizationContext(
            SynchronizationContext.Current
        );

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
        var inMemoryLoggerProvider = new InMemoryLoggerProvider();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            if (IsDebugMode)
            {
                builder.AddConsole();
                builder.AddDebug();
            }

            builder.AddFile(
                Path.Combine(store.LogFolder, "Log.log"),
                options =>
                {
#if !DEBUG
                    options.FileSizeLimitBytes = 1 * 1024 * 1024;
                    options.MaxRollingFiles = 5;
#endif
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

    private static void ConfigureServices(
        IServiceCollection builder,
        Store store,
        PackageStoreManager packageStoreManager,
        PluginManager pluginManager
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
        builder.AddSingleton<IPluginManager>(pluginManager);
        builder.AddSingleton<PackageManager>();
        builder.AddSingleton(packageStoreManager);
        builder.AddSingleton<QuickStartManager>();
        builder.AddSingleton<QuickStartScript>();
        builder.AddSingleton<IEncryption, Encryption>();

        builder.Add(ServiceDescriptor.Singleton(typeof(ICache<>), typeof(Cache<>)));
        builder.Add(ServiceDescriptor.Singleton(typeof(IMatchFactory<,>), typeof(MatchFactory<,>)));

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
                FileName = Assembly.GetEntryAssembly()!.Location,
                UseShellExecute = false
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
