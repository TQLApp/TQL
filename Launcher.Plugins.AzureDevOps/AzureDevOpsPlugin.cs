using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Categories;
using Launcher.Plugins.AzureDevOps.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps;

[LauncherPlugin]
public class AzureDevOpsPlugin : ILauncherPlugin
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<RepositoriesCategory>();
        services.AddSingleton<ICacheManager<AzureData>, AzureCacheManager>();
    }

    public ImmutableArray<ICategory> CreateCategories(IServiceProvider serviceProvider)
    {
        return ImmutableArray.Create<ICategory>(
            serviceProvider.GetRequiredService<RepositoriesCategory>()
        );
    }
}
