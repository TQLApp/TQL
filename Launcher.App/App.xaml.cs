using Launcher.Abstractions;
using Launcher.App.Services;
using Launcher.Plugins.AzureDevOps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;
using Launcher.App.Services.Database;
using Launcher.App.ConfigurationUI;
using Microsoft.Win32;
using FramePFX.Themes;

namespace Launcher.App;

public partial class App
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        System.Windows.Forms.Application.EnableVisualStyles();

        SetMode();

        var plugins = GetPlugins().ToImmutableArray();

        var builder = Host.CreateApplicationBuilder(e.Args);

        BuildContainer(builder.Services);

        builder.Services.AddSingleton<IPluginManager>(
            serviceProvider =>
                new PluginManager(
                    plugins
                        .Select(
                            plugin => new PluginEntry(plugin, plugin.Initialize(serviceProvider))
                        )
                        .ToImmutableArray()
                )
        );

        builder.Services.Add(ServiceDescriptor.Singleton(typeof(ICache<>), typeof(Cache<>)));

        foreach (var plugin in plugins)
        {
            plugin.ConfigureServices(builder.Services);
        }

        using var host = builder.Build();

        using var scope = host.Services.CreateScope();

        scope.ServiceProvider.GetRequiredService<MainWindow>().Show();
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

    private static void BuildContainer(IServiceCollection builder)
    {
        builder.AddSingleton<IImageFactory, ImageFactory>();
        builder.AddSingleton<IStore, Store>();
        builder.AddSingleton<IDb, Db>();
        builder.AddSingleton<Settings>();
        builder.AddSingleton<IConfigurationManager, ConfigurationManager>();

        builder.AddTransient<MainWindow>();
        builder.AddTransient<ConfigurationWindow>();
    }
}
