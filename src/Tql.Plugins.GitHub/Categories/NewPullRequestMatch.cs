using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

// This class is not serializable because instances of this class
// are transient by definition. The moment the user creates the pull
// request, the instance in the history becomes invalid. Because of
// this, there also isn't a MatchType class for this match. We do
// still need the TypeId though!
internal class NewPullRequestMatch(NewPullRequestDto dto) : IRunnableMatch, ICopyableMatch
{
    public string Text => dto.CompareBranch;
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

internal record NewPullRequestDto(Guid Id, string Owner, string Repository, string CompareBranch)
{
    public string GetUrl() =>
        $"https://github.com/{Uri.EscapeDataString(Owner)}/{Uri.EscapeDataString(Repository)}/compare/{Uri.EscapeDataString(CompareBranch)}?expand=1";
}
