using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.Jira.Categories;

internal class DashboardMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly DashboardMatchDto _dto;

    public string Text => $"{_dto.Name} Dashboard";
    public ImageSource Icon => Images.Dashboard;
    public MatchTypeId TypeId => TypeIds.Dashboard;

    public DashboardMatch(DashboardMatchDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(_dto.View);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.View);

        return Task.CompletedTask;
    }
}

internal record DashboardMatchDto(string Url, string Id, string Name, string View);
