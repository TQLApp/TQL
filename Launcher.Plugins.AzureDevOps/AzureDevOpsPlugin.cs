using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Categories;
using Launcher.Plugins.AzureDevOps.ConfigurationUI;
using Launcher.Plugins.AzureDevOps.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Launcher.Plugins.AzureDevOps;

[LauncherPlugin]
public class AzureDevOpsPlugin : ILauncherPlugin
{
    public static readonly Guid Id = Guid.Parse("36828080-c1f0-4759-8dff-2b764a44b62e");

    Guid ILauncherPlugin.Id => Id;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<RepositoriesCategory>();
        services.AddSingleton<ICacheManager<AzureData>, AzureCacheManager>();
        services.AddTransient<ConfigurationControl>();
    }

    public ImmutableArray<ICategory> Initialize(IServiceProvider serviceProvider)
    {
        serviceProvider
            .GetRequiredService<IConfigurationManager>()
            .RegisterConfigurationUIFactory(new ConfigurationUIFactory());

        return ImmutableArray.Create<ICategory>(
            serviceProvider.GetRequiredService<RepositoriesCategory>()
        );
    }

    private class ConfigurationUIFactory : IConfigurationUIFactory
    {
        public string Title => "Azure DevOps";

        public IConfigurationUI CreateControl(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ConfigurationControl>();
        }
    }
}
