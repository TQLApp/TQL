using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class DashboardMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly DashboardMatchDto _dto;

    public string Text =>
        MatchText.Path(
            _dto.ProjectName,
            string.Format(Labels.DashboardMatch_Label, _dto.DashboardName)
        );

    public ImageSource Icon => Images.Dashboards;
    public MatchTypeId TypeId => TypeIds.Dashboard;

    public DashboardMatch(DashboardMatchDto dto)
    {
        _dto = dto;
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
