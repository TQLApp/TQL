using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Abstractions;

public interface ILauncherPlugin
{
    Guid Id { get; }

    void ConfigureServices(IServiceCollection services);

    ImmutableArray<ICategory> Initialize(IServiceProvider serviceProvider);
}
