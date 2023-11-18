using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class WorkItemMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly WorkItemMatchDto _dto;

    public string Text => MatchText.Path(_dto.ProjectName, $"{_dto.Type} {_dto.Id}: {_dto.Title}");

    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.WorkItem;

    public WorkItemMatch(WorkItemMatchDto dto, AzureWorkItemIconManager iconManager)
    {
        _dto = dto;

        Icon =
            iconManager.GetWorkItemIconImage(dto.Url, dto.ProjectName, dto.Type) ?? Images.Boards;
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
        var clipboard = serviceProvider.GetRequiredService<IClipboard>();

        var url = _dto.GetUrl();

        clipboard.CopyMarkdown(
            $"[{clipboard.EscapeMarkdown(_dto.Type)} {_dto.Id}]({clipboard.EscapeMarkdown(url)}): {clipboard.EscapeMarkdown(_dto.Title)}",
            url
        );

        return Task.CompletedTask;
    }
}

internal record WorkItemMatchDto(string Url, string ProjectName, int Id, string Type, string Title)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_workitems/edit/{Id}";
};
