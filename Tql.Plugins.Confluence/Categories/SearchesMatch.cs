using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SearchesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly string _url;
    private readonly ConfigurationManager _configurationManager;

    public string Text { get; }
    public ImageSource Icon => Images.Confluence;
    public MatchTypeId TypeId => TypeIds.Searches;

    public SearchesMatch(string text, string url, ConfigurationManager configurationManager)
    {
        _url = url;
        _configurationManager = configurationManager;

        Text = text;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = _configurationManager.GetClient(_url);

        string cql =
            $"siteSearch ~ \"{text.Replace("\"", "\\\"")}\" AND type in (\"space\",\"user\",\"attachment\",\"page\",\"blogpost\")";

        return SearchUtils.CreateMatches(_url, await client.SiteSearch(cql, 25, cancellationToken));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
