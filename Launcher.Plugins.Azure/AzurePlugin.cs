using Launcher.Abstractions;
using Launcher.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Launcher.Plugins.Azure.Categories;
using Launcher.Plugins.Azure.ConfigurationUI;
using Launcher.Plugins.Azure.Services;
using Launcher.Plugins.Azure.Support;

namespace Launcher.Plugins.Azure;

[LauncherPlugin]
public class AzurePlugin : ILauncherPlugin
{
    public static readonly Guid Id = Guid.Parse("51ebb93a-4d72-4231-adf0-4985f377a1b7");

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private MatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ILauncherPlugin.Id => Id;

    public AzurePlugin()
    {
        _matchTypeManagerBuilder = new MatchTypeManagerBuilder(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<AzureApi>();
        services.AddSingleton<Images>();
        services.AddSingleton<ConnectionManager>();

        services.AddTransient<ConfigurationControl>();

        _matchTypeManagerBuilder.ConfigureServices(services);
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var configurationManager = serviceProvider.GetRequiredService<IConfigurationManager>();

        configurationManager.RegisterConfigurationUIFactory(new ConfigurationUIFactory());

        _matchTypeManager = _matchTypeManagerBuilder.Build(serviceProvider);
    }

    public IEnumerable<IMatch> GetMatches()
    {
        var connectionManager = _serviceProvider!.GetRequiredService<ConnectionManager>();

        return from connection in connectionManager.Connections
            let json = JsonSerializer.Serialize(new RootItemDto(connection.Id))
            from matchType in _matchTypeManager!.MatchTypes
            where matchType.GetType().GetCustomAttribute<RootMatchTypeAttribute>() != null
            select matchType.Deserialize(json);
    }

    public IMatch? DeserializeMatch(Guid typeId, string json)
    {
        return _matchTypeManager?.Deserialize(typeId, json);
    }

    private class ConfigurationUIFactory : IConfigurationUIFactory
    {
        public string Title => "Azure Portal";

        public IConfigurationUI CreateControl(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ConfigurationControl>();
        }
    }
}
