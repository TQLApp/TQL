using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Categories;
using Tql.Plugins.MicrosoftTeams.ConfigurationUI;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams;

[TqlPlugin]
public class MicrosoftTeamsPlugin : ITqlPlugin
{
    public static readonly Guid Id = Guid.Parse("8f6c5e2c-455e-4eb0-badd-ed577fe03a3a");

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private MatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ITqlPlugin.Id => Id;

    public MicrosoftTeamsPlugin()
    {
        _matchTypeManagerBuilder = new MatchTypeManagerBuilder(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ConfigurationManager>();
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
        var configuration = _serviceProvider!
            .GetRequiredService<ConfigurationManager>()
            .Configuration;

        IEnumerable<string> directoryIds;

        if (configuration.Mode == ConfigurationMode.Selected)
        {
            directoryIds = configuration.DirectoryIds;
        }
        else
        {
            var peopleDirectoryManager =
                _serviceProvider!.GetRequiredService<IPeopleDirectoryManager>();

            directoryIds = peopleDirectoryManager.Directories.Select(p => p.Id);
        }

        return from directoryId in directoryIds
            let json = JsonSerializer.Serialize(new RootItemDto(directoryId))
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
        public string Title => "Microsoft Teams";

        public IConfigurationUI CreateControl(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ConfigurationControl>();
        }
    }
}
