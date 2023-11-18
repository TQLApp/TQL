using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Tql.Abstractions;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssueMatchBase : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    protected IssueMatchDto Dto { get; }

    public string Text => $"{Dto.RepositoryName}: #{Dto.Number} {Dto.Title}";
    public abstract ImageSource Icon { get; }
    public abstract MatchTypeId TypeId { get; }

    protected IssueMatchBase(IssueMatchDto dto)
    {
        Dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(Dto.Url);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(Dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        var clipboard = serviceProvider.GetRequiredService<IClipboard>();

        clipboard.CopyMarkdown(
            $"[#{Dto.Number}]({clipboard.EscapeMarkdown(Dto.Url)}): {clipboard.EscapeMarkdown(Dto.Title)}",
            Dto.Url
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
