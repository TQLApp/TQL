using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class NewMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly NewMatchDto _dto;

    public string Text =>
        _dto.Type switch
        {
            NewMatchType.WorkItem => $"{_dto.ProjectName} › New {_dto.Name}",
            NewMatchType.Query => $"{_dto.ProjectName} › New Query",
            _ => throw new ArgumentOutOfRangeException()
        };

    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.New;

    public NewMatch(NewMatchDto dto, AzureWorkItemIconManager iconManager)
    {
        _dto = dto;

        if (dto.Type == NewMatchType.WorkItem)
        {
            Icon =
                iconManager.GetWorkItemIconImage(dto.Url, dto.ProjectName, dto.Name!)
                ?? Images.Boards;
        }
        else
        {
            Icon = Images.Boards;
        }
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
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

internal enum NewMatchType
{
    WorkItem,
    Query
}

internal record NewMatchDto(string Url, string ProjectName, NewMatchType Type, string? Name)
{
    public string GetUrl() =>
        Type switch
        {
            NewMatchType.WorkItem
                => $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_workitems/create/{Uri.EscapeDataString(Name!.ToLower())}",
            NewMatchType.Query
                => $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_queries/query-edit/?newQuery=true",
            _ => throw new ArgumentOutOfRangeException()
        };
}
