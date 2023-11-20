using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Categories;

internal class IssueMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly IssueMatchDto _dto;

    public string Text => $"{_dto.Key} {_dto.Summary}";
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Issue;

    public IssueMatch(IssueMatchDto dto, IconCacheManager iconCacheManager)
    {
        _dto = dto;
        Icon =
            iconCacheManager.GetIcon(new IconKey(dto.Url, dto.IssueTypeIconUrl)) ?? Images.Issues;
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
}

internal record IssueMatchDto(string Url, string Key, string Summary, string IssueTypeIconUrl)
{
    public string GetUrl() => $"{Url.TrimEnd('/')}/browse/{Uri.EscapeDataString(Key)}";
};
