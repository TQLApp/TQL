using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SpaceMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    private readonly SpaceMatchDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<SearchMatch, SearchMatchDto> _factory;

    public string Text => string.Format(Labels.SpaceMatch_Label, _dto.Name);
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Space;
    public string SearchHint => Labels.SpaceMatch_SearchHint;

    public SpaceMatch(
        SpaceMatchDto dto,
        IconCacheManager iconCacheManager,
        ConfigurationManager configurationManager,
        IMatchFactory<SearchMatch, SearchMatchDto> factory
    )
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;

        Icon = iconCacheManager.GetIcon(new IconKey(dto.Url, dto.Icon)) ?? Images.Confluence;
    }

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(_dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.GetUrl());

        return Task.CompletedTask;
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

        var client = _configurationManager.GetClient(_dto.Url);

        string cql =
            $"space = \"{_dto.Key.Replace("\"", "\\\"")}\" and siteSearch ~ \"{text.Replace("\"", "\\\"")}\" AND type in (\"space\",\"user\",\"attachment\",\"page\",\"blogpost\")";

        return SearchUtils.CreateMatches(
            _dto.Url,
            await client.SiteSearch(cql, 25, cancellationToken),
            _factory
        );
    }
}

internal record SpaceMatchDto(string Url, string Key, string Name, string ViewUrl, string Icon)
{
    public string GetUrl()
    {
        var url = ViewUrl;
        if (!url.Contains("://", StringComparison.Ordinal))
            url = $"{Url.TrimEnd('/')}/{ViewUrl.TrimStart('/')}";
        return url;
    }
};
