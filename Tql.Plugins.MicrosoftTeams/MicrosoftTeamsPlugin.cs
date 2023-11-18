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
    public static readonly Guid ConfigurationPageId = Guid.Parse(
        "0fc1ad1b-92d5-41e8-96a7-621b13402c0b"
    );

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private IMatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ITqlPlugin.Id => Id;
    public string Title => Labels.MicrosoftTeamsPlugin_Label;

    public MicrosoftTeamsPlugin()
    {
        _matchTypeManagerBuilder = MatchTypeManagerBuilder.ForAssembly(GetType().Assembly);
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

        _matchTypeManager = _matchTypeManagerBuilder.Build(serviceProvider);
    }

    public IEnumerable<IMatch> GetMatches()
    {
        var configurationManager = _serviceProvider!.GetRequiredService<ConfigurationManager>();

        return from directoryId in configurationManager.DirectoryIds
            let json = JsonSerializer.Serialize(new RootItemDto(directoryId))
            from matchType in _matchTypeManager!.MatchTypes
            where matchType.GetType().GetCustomAttribute<RootMatchTypeAttribute>() != null
            select matchType.Deserialize(json);
    }

    public IMatch? DeserializeMatch(Guid typeId, string value)
    {
        return _matchTypeManager?.Deserialize(typeId, value);
    }

    public IEnumerable<IConfigurationPage> GetConfigurationPages()
    {
        yield return _serviceProvider!.GetRequiredService<ConfigurationControl>();
    }
}
