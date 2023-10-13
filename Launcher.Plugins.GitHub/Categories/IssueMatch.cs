using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

namespace Launcher.Plugins.GitHub.Categories;

internal class IssueMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly IssueMatchDto _dto;

    public string Text => $"{_dto.RepositoryName}: #{_dto.Number} {_dto.Title}";
    public ImageSource Icon => _dto.State == ItemState.Open ? Images.OpenIssue : Images.ClosedIssue;
    public MatchTypeId TypeId => TypeIds.Issue;

    public IssueMatch(IssueMatchDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().LaunchUrl(_dto.Url);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        var clipboard = serviceProvider.GetRequiredService<IClipboard>();

        clipboard.CopyMarkdown(
            $"[#{_dto.Number}]({clipboard.EscapeMarkdown(_dto.Url)}): {clipboard.EscapeMarkdown(_dto.Title)}",
            _dto.Url
        );

        return Task.CompletedTask;
    }
}

internal record IssueMatchDto(
    Guid ConnectionId,
    string RepositoryName,
    int Number,
    string Title,
    string Url,
    ItemState State
);
