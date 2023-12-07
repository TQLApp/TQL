﻿using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows.Forms;
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
using Tql.Utilities;
using Application = System.Windows.Application;
using ConfigurationManager = Tql.App.Services.ConfigurationManager;
using MessageBox = System.Windows.Forms.MessageBox;
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
                .GetValue(null)!;

    private void Application_Startup(object? sender, StartupEventArgs e)
    {
        Parser.Default.ParseArguments<Options>(FixupArgs(e.Args)).WithParsed(p => Options = p);

        if (Options.Sideload != null)
            IsDebugMode = true;

        System.Windows.Forms.Application.SetHighDpiMode(HighDpiMode.PerMonitor);
        System.Windows.Forms.Application.EnableVisualStyles();

        var store = new Store(Options.Environment, TraceLogger.Instance);

        SetUICulture(store);

        if (Options.RequestReset && ConfirmReset())
            return;

        _ipc = new WindowMessageIPC(
            Options.Environment,
            Options.RequestReset,
            TraceLogger.Instance
        );

        if (Options.RequestReset)
        {
            PerformReset(store);
            return;
        }

        if (!_ipc.IsFirstRunner)
        {
            Shutdown(0);
            return;
        }

        using var splashScreen = new SplashScreen();

        if (!Options.IsSilent)
            splashScreen.Show();

        var notifyIconManager = new NotifyIconManager();

        var (loggerFactory, inMemoryLoggerProvider) = SetupLogging(store);

        var packageStoreManager = new PackageStoreManager(
            store,
            loggerFactory.CreateLogger<PackageStoreManager>()
        );

        splashScreen.Progress.SetProgress(0.1);

        packageStoreManager.PerformCleanup(splashScreen.Progress.GetSubProgress(0.1, 0.2));

        splashScreen.Progress.SetProgress(0.2);

        var loader = CreatePluginLoader(packageStoreManager, loggerFactory);

        var pluginManager = new PluginManager(
            loader,
            splashScreen.Progress.GetSubProgress(0.2, 0.3)
        );

        splashScreen.Progress.SetProgress(0.3);

        var builder = Host.CreateApplicationBuilder(e.Args);

        builder.Services.AddSingleton(loggerFactory);
        builder.Services.AddSingleton(inMemoryLoggerProvider);
        builder.Services.AddSingleton(notifyIconManager);
        builder.Services.AddSingleton<IStore>(store);
        builder.Services.AddSingleton<IPluginManager>(pluginManager);
        builder.Services.AddSingleton(packageStoreManager);

        ConfigureServices(builder.Services);

        pluginManager.ConfigureServices(builder.Services);

        splashScreen.Progress.SetProgress(0.4);

        _host = builder.Build();

        splashScreen.Progress.SetProgress(0.5);

        var settings = _host.Services.GetRequiredService<Settings>();

        ThemeManager.SetTheme(ThemeManager.ParseTheme(settings.Theme));

        splashScreen.Progress.SetProgress(0.6);

        var logger = _host.Services.GetRequiredService<ILogger<App>>();

        if (!IsDebugMode)
        {
            logger.LogInformation("Checking for updates");

            if (TryStartUpdate(splashScreen.Progress.GetSubProgress(0.6, 0.8), logger))
                return;
        }
        else
        {
            notifyIconManager.State = NotifyIconState.Running;
        }

        splashScreen.Progress.SetProgress(0.8);

        logger.LogInformation("Initializing plugins");

        ((UI)_host.Services.GetRequiredService<IUI>()).SetSynchronizationContext(
            SynchronizationContext.Current
        );

        pluginManager.Initialize(_host.Services);

        splashScreen.Progress.SetProgress(0.9);

        logger.LogInformation("Startup complete");

        _mainWindow = _host.Services.GetRequiredService<MainWindow>();

        RegisterHotKey(logger);

        _ipc.Received += (_, _) => _mainWindow.DoShow();

        splashScreen.Hide();

        if (!Options.IsSilent)
            _mainWindow.DoShow();
    }

    private void RegisterHotKey(ILogger<App> logger)
    {
        var settings = _host!.Services.GetRequiredService<Settings>();
        var hotKey = HotKey.FromSettings(settings);

        try
        {
            _host!.Services.GetRequiredService<HotKeyService>().RegisterHotKey(hotKey);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not register hot key");

            var ui = _host!.Services.GetRequiredService<IUI>();

            ui.ShowAlert(
                _mainWindow!,
                Labels.App_CouldNotRegisterHotKey,
                string.Format(Labels.App_CouldNotRegisterHotKeySubtitle, hotKey.ToLabel())
            );
        }
    }

    private bool ConfirmReset()
    {
        if (Options.IsSilent)
        {
            if (Options.Environment == null)
            {
                Shutdown(1);
                return true;
            }
        }
        else
        {
            if (Options.Environment == null)
            {
                MessageBox.Show(
                    Labels.App_CannotResetMainEnvironment,
                    Labels.ApplicationTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                Shutdown(1);
                return true;
            }

            var result = MessageBox.Show(
                string.Format(Labels.App_AreYouSureResetEnvironment, Options.Environment),
                Labels.ApplicationTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result != DialogResult.Yes)
            {
                Shutdown(1);
                return true;
            }
        }

        return false;
    }

    private void PerformReset(Store store)
    {
        try
        {
            store.Reset();

            if (!Options.IsSilent)
            {
                MessageBox.Show(
                    Labels.App_ResetCompletedSuccessfully,
                    Labels.ApplicationTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            Shutdown(0);
        }
        catch (Exception ex)
        {
            if (!Options.IsSilent)
            {
                MessageBox.Show(
                    string.Format(
                        Labels.App_ResetFailed,
                        $"{ex.Message} ({ex.GetType().FullName})"
                    ),
                    Labels.ApplicationTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }

            Shutdown(1);
        }
    }

    private void SetUICulture(Store store)
    {
        using var key = store.CreateBaseKey();

        if (key.GetValue(nameof(Settings.Language)) is string language)
        {
            var culture = CultureInfo.GetCultureInfo(language);

            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            foreach (Window window in Current.Windows)
            {
                window.Language = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
            }
        }
    }

    private static IPackageLoader CreatePluginLoader(
        PackageStoreManager packageStoreManager,
        ILoggerFactory loggerFactory
    )
    {
        if (Options.Sideload != null)
        {
            return new SideloadedPackageLoader(
                AssemblyLoadContext.Default,
                Options.Sideload,
                loggerFactory.CreateLogger<SideloadedPackageLoader>()
            );
        }

        if (DebugAssemblies.HasValue)
            return new AssemblyPackageLoader(DebugAssemblies.Value);

        return new PackageStoreLoader(
            packageStoreManager,
            AssemblyLoadContext.Default,
            loggerFactory.CreateLogger<PackageStoreLoader>()
        );
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

            var fileIndex = 0;

            builder.AddFile(
                Path.Combine(store.LogFolder, "Log.log"),
                options =>
                {
#if !DEBUG
                    options.FileSizeLimitBytes = 1 * 1024 * 1024;
                    options.MaxRollingFiles = 5;
#endif
                    options.HandleFileError = p =>
                    {
                        // ERROR_SHARING_VIOLATION
                        if ((uint)p.ErrorException.HResult == 0x80070020 && fileIndex < 10)
                        {
                            p.UseNewLogFileName(
                                Path.Combine(store.LogFolder, $"Log_{++fileIndex}.log")
                            );
                        }
                    };
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
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            logger.LogCritical(e.Exception, "Unobserved task exception");

            if (!IsDebugMode)
                e.SetObserved();
        };

        return (loggerFactory, inMemoryLoggerProvider);
    }

    private bool TryStartUpdate(IProgress progress, ILogger<App> logger)
    {
        try
        {
            if (_host!.Services.GetRequiredService<UpdateChecker>().TryStartUpdate(progress))
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

    private static void ConfigureServices(IServiceCollection builder)
    {
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
        builder.AddSingleton<PackageManager>();
        builder.AddSingleton<QuickStartManager>();
        builder.AddSingleton<QuickStartScript>();
        builder.AddSingleton<IEncryption, Services.Encryption>();

        builder.Add(ServiceDescriptor.Singleton(typeof(ICache<>), typeof(Cache<>)));
        builder.Add(ServiceDescriptor.Singleton(typeof(IMatchFactory<,>), typeof(MatchFactory<,>)));

        builder.AddTransient<MainWindow>();
        builder.AddTransient<ConfigurationWindow>();
        builder.AddTransient<FeedbackWindow>();
        builder.AddTransient<SearchManager>();
        builder.AddTransient<GeneralConfigurationControl>();
        builder.AddTransient<PluginsConfigurationControl>();
        builder.AddTransient<PackageSourcesConfigurationControl>();
        builder.AddTransient<PackageSourceEditWindow>();
    }

    private void Application_Exit(object? sender, ExitEventArgs e)
    {
        _host?.Dispose();
        _ipc?.Dispose();

        if (RestartMode is RestartMode.Restart or RestartMode.SilentRestart)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, ".exe"),
                UseShellExecute = false
            };

            if (RestartMode == RestartMode.SilentRestart)
                startInfo.ArgumentList.Add("--silent");

            if (Options.Environment != null)
            {
                startInfo.ArgumentList.Add("--env");
                startInfo.ArgumentList.Add(Options.Environment);
            }

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
