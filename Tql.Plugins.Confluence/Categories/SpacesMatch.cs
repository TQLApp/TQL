using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Confluence.Data;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SpacesMatch : CachedMatch<ConfluenceData>, ISerializableMatch
{
    private readonly string _url;
    private readonly IconCacheManager _iconCacheManager;
    private readonly ConfigurationManager _configurationManager;
    private readonly ILogger<SpacesMatch> _logger;

    public override string Text { get; }
    public override ImageSource Icon => Images.Confluence;
    public override MatchTypeId TypeId => TypeIds.Spaces;

    public SpacesMatch(
        string text,
        string url,
        ICache<ConfluenceData> cache,
        IconCacheManager iconCacheManager,
        ConfigurationManager configurationManager,
        ILogger<SpacesMatch> logger
    )
        : base(cache)
    {
        _url = url;
        _iconCacheManager = iconCacheManager;
        _configurationManager = configurationManager;
        _logger = logger;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(ConfluenceData data)
    {
        // Download the project avatars in the background.

        var spaces = data.GetConnection(_url).Spaces;

        return from space in spaces
            select new SpaceMatch(
                new SpaceMatchDto(_url, space.Key, space.Name, space.Url, space.Icon),
                _iconCacheManager,
                _configurationManager
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
