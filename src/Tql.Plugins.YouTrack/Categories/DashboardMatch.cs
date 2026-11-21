using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.YouTrack.Support;

namespace Tql.Plugins.YouTrack.Categories;

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
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.GetUrl());

        return Task.CompletedTask;
    }
}

internal record DashboardMatchDto(string Url, string Id, string Name)
{
    public string GetUrl() => YouTrackUrls.GetDashboardUrl(Url, Id);
};
