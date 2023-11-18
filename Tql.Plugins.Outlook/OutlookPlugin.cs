using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Outlook.Categories;
using Tql.Plugins.Outlook.Data;
using Tql.Plugins.Outlook.Services;
using Tql.Plugins.Outlook.Support;
using Tql.Utilities;

namespace Tql.Plugins.Outlook;

[TqlPlugin]
public class OutlookPlugin : ITqlPlugin
{
    public static readonly Guid Id = Guid.Parse("90410ab2-0836-49e8-9af3-3df479d43e75");

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private IMatchTypeManager? _matchTypeManager;

    Guid ITqlPlugin.Id => Id;
    public string Title => Labels.OutlookPlugin_Label;

    public OutlookPlugin()
    {
        _matchTypeManagerBuilder = MatchTypeManagerBuilder.ForAssembly(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<OutlookPeopleDirectory>();
        services.AddSingleton<ICacheManager<OutlookData>, OutlookCacheManager>();

        _matchTypeManagerBuilder.ConfigureServices(services);
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        serviceProvider
            .GetRequiredService<IPeopleDirectoryManager>()
            .Add(serviceProvider.GetRequiredService<OutlookPeopleDirectory>());

        _matchTypeManager = _matchTypeManagerBuilder.Build(serviceProvider);
    }

    public IEnumerable<IMatch> GetMatches()
    {
        return from matchType in _matchTypeManager!.MatchTypes
            where matchType.GetType().GetCustomAttribute<RootMatchTypeAttribute>() != null
            select matchType.Deserialize(JsonSerializer.Serialize(new RootItemDto()));
    }

    public IMatch? DeserializeMatch(Guid typeId, string value)
    {
        return _matchTypeManager?.Deserialize(typeId, value);
    }

    public IEnumerable<IConfigurationPage> GetConfigurationPages() =>
        Array.Empty<IConfigurationPage>();
}
