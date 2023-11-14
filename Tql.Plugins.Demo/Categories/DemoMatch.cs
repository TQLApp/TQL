using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Tql.Abstractions;

namespace Tql.Plugins.Demo.Categories;

internal class DemoMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly DemoMatchDto _dto;
    public string Text => Labels.DemoMatch_Label;
    public ImageSource Icon => Images.SpaceShuttle;
    public MatchTypeId TypeId => TypeIds.Demo;

    public DemoMatch(DemoMatchDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl("https://github.com/pvginkel/TQL");

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider
            .GetRequiredService<IClipboard>()
            .CopyUri(Text, "https://github.com/pvginkel/TQL");

        return Task.CompletedTask;
    }
}

internal record DemoMatchDto();
