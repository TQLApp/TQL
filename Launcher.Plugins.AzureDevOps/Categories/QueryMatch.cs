using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class QueryMatch : IRunnableMatch, ISerializableMatch
{
    private readonly QueryMatchDto _dto;
    private readonly Images _images;

    public string Text =>
        System.IO.Path.Combine(_dto.ProjectName, _dto.Path.Trim('\\')).Replace('\\', '/');
    public IImage Icon => _images.Boards;
    public MatchTypeId TypeId => TypeIds.Query;

    public QueryMatch(QueryMatchDto dto, Images images)
    {
        _dto = dto;
        _images = images;
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

internal record QueryMatchDto(string Url, string ProjectName, Guid Id, string Path, string Name)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_queries/query-edit/{Uri.EscapeDataString(Id.ToString())}";
};
