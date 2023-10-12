using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class WorkItemMatch : IRunnableMatch, ISerializableMatch
{
    private readonly WorkItemMatchDto _dto;

    public string Text => $"{_dto.ProjectName}/{_dto.Type} {_dto.Id}: {_dto.Title}";
    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.WorkItem;

    public WorkItemMatch(WorkItemMatchDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().LaunchUrl(_dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}

internal record WorkItemMatchDto(string Url, string ProjectName, int Id, string Type, string Title)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_workitems/edit/{Id}";
};
