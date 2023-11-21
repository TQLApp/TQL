using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal class IssueMatch(IssueMatchDto dto, IconCacheManager iconCacheManager)
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch
{
    public string Text => $"{dto.Key} {dto.Summary}";
    public ImageSource Icon { get; } =
        iconCacheManager.GetIcon(new IconKey(dto.Url, dto.IssueTypeIconUrl)) ?? Images.Issues;
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

internal record IssueMatchDto(string Url, string Key, string Summary, string IssueTypeIconUrl)
{
    public string GetUrl() => $"{Url.TrimEnd('/')}/browse/{Uri.EscapeDataString(Key)}";
};
