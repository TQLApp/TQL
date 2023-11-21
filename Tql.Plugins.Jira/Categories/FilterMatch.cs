using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal class FilterMatch(
    FilterMatchDto dto,
    ConfigurationManager configurationManager,
    IMatchFactory<IssueMatch, IssueMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => dto.Name;
    public ImageSource Icon => Images.Filters;
    public MatchTypeId TypeId => TypeIds.Filter;
    public string SearchHint => Labels.FilterMatch_SearchHint;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.ViewUrl);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.ViewUrl);

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        await context.DebounceDelay(cancellationToken);

        var client = configurationManager.GetClient(dto.Url);

        var issues = await client.SearchIssues(dto.Jql, 100, cancellationToken);

        return IssueUtils.CreateMatches(dto.Url, issues, factory);
    }
}

internal record FilterMatchDto(string Url, string Name, string ViewUrl, string Jql);
