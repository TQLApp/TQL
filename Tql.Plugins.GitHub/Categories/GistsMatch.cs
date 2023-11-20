using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Tql.Plugins.GitHub.Categories;

internal class GistsMatch : ISearchableMatch, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly GitHubApi _api;
    private readonly ICache<GitHubData> _cache;
    private readonly HttpClient _httpClient;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<GistMatch, GistMatchDto> _factory;

    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.GistsMatch_Label,
            Labels.GistsMatch_MyLabel,
            _configurationManager.Configuration,
            _dto
        );

    public ImageSource Icon => Images.Gist;
    public MatchTypeId TypeId => TypeIds.Gists;
    public string SearchHint => Labels.GistsMatch_SearchHint;

    public GistsMatch(
        RootItemDto dto,
        GitHubApi api,
        ICache<GitHubData> cache,
        HttpClient httpClient,
        ConfigurationManager configurationManager,
        IMatchFactory<GistMatch, GistMatchDto> factory
    )
    {
        _dto = dto;
        _api = api;
        _cache = cache;
        _httpClient = httpClient;
        _configurationManager = configurationManager;
        _factory = factory;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (_dto.Scope == RootItemScope.Global && text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var search = await GitHubUtils.GetSearchPrefix(_dto, _cache, false) + text;

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"https://gist.github.com/search?q={Uri.EscapeDataString(search)}"
        );

        var client = await _api.GetClient(_dto.Id);
        var connection = (Octokit.Connection)client.Connection;

        request
            .Headers
            .UserAgent
            .Add(
                new ProductInfoHeaderValue("TQL", GetType().Assembly.GetName().Version.ToString())
            );
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            connection.Credentials.Password
        );

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        var doc = new HtmlDocument();

        doc.LoadHtml(await response.Content.ReadAsStringAsync());

        var matches = new List<IMatch>();

        foreach (
            var element in doc.DocumentNode
                .Descendants("div")
                .Where(p => p.HasClass("gist-snippet-meta"))
        )
        {
            var fileElement = element
                .Descendants("strong")
                .Single(p => p.HasClass("css-truncate-target"));
            var linkElement = FindParent(fileElement, "A");
            var containerElement = FindParent(linkElement, "SPAN");

            var name = Regex.Replace(containerElement.InnerText, @"\s+", "");

            matches.Add(
                _factory.Create(
                    new GistMatchDto(
                        _dto.Id,
                        name,
                        $"https://gist.github.com/{linkElement.GetAttributeValue("href", null)}"
                    )
                )
            );
        }

        return matches;
    }

    private HtmlNode FindParent(HtmlNode node, string name)
    {
        for (var parent = node.ParentNode; parent != null; parent = parent.ParentNode)
        {
            if (string.Equals(parent.Name, name, StringComparison.OrdinalIgnoreCase))
                return parent;
        }

        throw new InvalidOperationException();
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
