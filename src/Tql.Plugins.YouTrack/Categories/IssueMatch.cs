using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.YouTrack.Support;

namespace Tql.Plugins.YouTrack.Categories;

internal class IssueMatch(IssueMatchDto dto) : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    public string Text => $"{dto.IdReadable} {dto.Summary}";
    public ImageSource Icon => Images.Issues;
    public MatchTypeId TypeId => TypeIds.Issue;

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
}

internal record IssueMatchDto(string Url, string IdReadable, string Summary)
{
    public string GetUrl() => YouTrackUrls.GetIssueUrl(Url, IdReadable);
};
