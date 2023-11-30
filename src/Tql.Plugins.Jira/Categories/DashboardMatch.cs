using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.Jira.Categories;

internal class DashboardMatch(DashboardMatchDto dto)
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch
{
    public string Text => string.Format(Labels.DashboardMatch_Label, dto.Name);
    public ImageSource Icon => Images.Dashboards;
    public MatchTypeId TypeId => TypeIds.Dashboard;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.View);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.View);

        return Task.CompletedTask;
    }
}

internal record DashboardMatchDto(string Url, string Name, string View);
