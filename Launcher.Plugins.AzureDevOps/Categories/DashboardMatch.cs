using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class DashboardMatch : IRunnableMatch, ISerializableMatch
{
    private readonly DashboardMatchDto _dto;
    private readonly Images _images;

    public string Text => $"{_dto.ProjectName}/{_dto.DashboardName} Dashboard";
    public IImage Icon => _images.Dashboards;
    public MatchTypeId TypeId => TypeIds.Dashboard;

    public DashboardMatch(DashboardMatchDto dto, Images images)
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

internal record DashboardMatchDto(
    string Url,
    string ProjectName,
    Guid DashboardId,
    string DashboardName
)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_dashboards/dashboard/{Uri.EscapeDataString(DashboardId.ToString())}";
};
