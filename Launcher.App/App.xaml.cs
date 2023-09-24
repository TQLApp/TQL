using Launcher.Abstractions;
using Launcher.App.Services;
using Launcher.Plugins.AzureDevOps;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;
using Launcher.App.Services.Database;

namespace Launcher.App;

public partial class App
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var plugins = GetPlugins().ToImmutableArray();

        var builder = Host.CreateApplicationBuilder(e.Args);

        BuildContainer(builder.Services);

        builder.Services.AddSingleton<IPluginManager>(
            serviceProvider =>
                new PluginManager(
                    plugins
                        .Select(
                            plugin =>
                                new PluginEntry(plugin, plugin.CreateCategories(serviceProvider))
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

        builder.AddScoped<MainWindow>();
    }
}
