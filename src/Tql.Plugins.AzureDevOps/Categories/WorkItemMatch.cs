using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class WorkItemMatch(WorkItemMatchDto dto, AzureWorkItemIconManager iconManager)
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch
{
    public string Text => MatchText.Path(dto.ProjectName, $"{dto.Type} {dto.Id}: {dto.Title}");

    public ImageSource Icon { get; } =
        iconManager.GetWorkItemIconImage(dto.Url, dto.ProjectName, dto.Type) ?? Images.Boards;
    public MatchTypeId TypeId => TypeIds.WorkItem;

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
        var clipboard = serviceProvider.GetRequiredService<IClipboard>();

        var url = dto.GetUrl();

        clipboard.CopyMarkdown(
            $"[{clipboard.EscapeMarkdown(dto.Type)} {dto.Id}]({clipboard.EscapeMarkdown(url)}): {clipboard.EscapeMarkdown(dto.Title)}",
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
