using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class NewMatch(NewMatchDto dto) : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    public string Text =>
        dto.Type switch
        {
            NewMatchType.Issue
                => MatchText.Path($"{dto.Owner}/{dto.Repository}", Labels.NewMatch_NewIssue),
            NewMatchType.PullRequest
                => MatchText.Path($"{dto.Owner}/{dto.Repository}", Labels.NewMatch_NewPullRequest),
            NewMatchType.Repository => Labels.NewMatch_NewRepository,
            NewMatchType.Gist => Labels.NewMatch_NewGist,
            NewMatchType.Organization => Labels.NewMatch_NewOrganization,
            NewMatchType.ImportRepository => Labels.NewMatch_ImportRepository,
            NewMatchType.Codespace => Labels.NewMatch_NewCodespace,
            _ => throw new ArgumentOutOfRangeException()
        };

    public ImageSource Icon =>
        dto.Type switch
        {
            NewMatchType.Issue => Images.Issue,
            NewMatchType.PullRequest => Images.PullRequest,
            NewMatchType.Repository => Images.Repository,
            NewMatchType.Gist => Images.Gist,
            NewMatchType.Organization => Images.Organization,
            NewMatchType.ImportRepository => Images.ImportRepository,
            NewMatchType.Codespace => Images.Codespace,
            _ => throw new ArgumentOutOfRangeException()
        };

    public MatchTypeId TypeId => TypeIds.New;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, GetUrl());

        return Task.CompletedTask;
    }

    private string GetUrl() =>
        dto.Type switch
        {
            NewMatchType.Issue
                => $"https://github.com/{dto.Owner}/{dto.Repository}/issues/new/choose",
            NewMatchType.PullRequest => $"https://github.com/{dto.Owner}/{dto.Repository}/compare",
            NewMatchType.Repository => "https://github.com/new",
            NewMatchType.Gist => "https://gist.github.com/",
            NewMatchType.Organization => "https://github.com/account/organizations/new",
            NewMatchType.ImportRepository => "https://github.com/new/import",
            NewMatchType.Codespace => "https://github.com/codespaces/new",
            _ => throw new ArgumentOutOfRangeException()
        };
}

internal record NewMatchDto(Guid? Id, string? Owner, string? Repository, NewMatchType Type);

internal enum NewMatchType
{
    Issue,
    PullRequest,
    Repository,
    Gist,
    Organization,
    ImportRepository,
    Codespace
}
