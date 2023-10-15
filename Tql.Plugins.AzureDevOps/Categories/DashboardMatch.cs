using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class DashboardMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly DashboardMatchDto _dto;

    public string Text => $"{_dto.ProjectName}/{_dto.DashboardName} Dashboard";
    public ImageSource Icon => Images.Dashboards;
    public MatchTypeId TypeId => TypeIds.Dashboard;

    public DashboardMatch(DashboardMatchDto dto)
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

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.GetUrl());

        return Task.CompletedTask;
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
