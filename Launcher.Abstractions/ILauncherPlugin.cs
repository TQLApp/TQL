using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Abstractions;

public interface ILauncherPlugin
{
    void ConfigureServices(IServiceCollection services);

    ImmutableArray<ICategory> CreateCategories(IServiceProvider serviceProvider);
}
