using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Categories;

// This class is not serializable because instances of this class
// are transient by definition. The moment the user creates the pull
// request, the instance in the history becomes invalid. Because of
// this, there also isn't a MatchType class for this match. We do
// still need the TypeId though!
internal class NewPullRequestMatch(NewPullRequestMatchDto dto) : IRunnableMatch, ICopyableMatch
{
    public string Text => dto.SourceBranch;
    public ImageSource Icon => Images.PullRequest;
    public MatchTypeId TypeId => TypeIds.NewPullRequest;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.GetUrl());

        return Task.CompletedTask;
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.GetUrl());

        return Task.CompletedTask;
    }
}

internal record NewPullRequestMatchDto(
    string Url,
    string ProjectName,
    string RepositoryName,
    string SourceBranch
)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_git/{Uri.EscapeDataString(RepositoryName)}/pullrequestcreate?sourceRef={Uri.EscapeDataString(SourceBranch)}";
}
