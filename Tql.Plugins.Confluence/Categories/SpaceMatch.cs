using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SpaceMatch(
    SpaceMatchDto dto,
    IconCacheManager iconCacheManager,
    ConfigurationManager configurationManager,
    IMatchFactory<SearchMatch, SearchMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => string.Format(Labels.SpaceMatch_Label, dto.Name);
    public ImageSource Icon { get; } =
        iconCacheManager.GetIcon(new IconKey(dto.Url, dto.Icon)) ?? Images.Confluence;
    public MatchTypeId TypeId => TypeIds.Space;
    public string SearchHint => Labels.SpaceMatch_SearchHint;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.GetUrl());

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

        var client = configurationManager.GetClient(dto.Url);

        string cql =
            $"space = \"{dto.Key.Replace("\"", "\\\"")}\" and siteSearch ~ \"{text.Replace("\"", "\\\"")}\" AND type in (\"space\",\"user\",\"attachment\",\"page\",\"blogpost\")";

        return SearchUtils.CreateMatches(
            dto.Url,
            await client.SiteSearch(cql, 25, cancellationToken),
            factory
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
