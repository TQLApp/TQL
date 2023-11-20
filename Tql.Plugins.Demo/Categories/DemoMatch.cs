using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
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

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl("https://github.com/TQLApp/TQL");

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
            .CopyUri(Text, "https://github.com/TQLApp/TQL");

        return Task.CompletedTask;
    }
}

internal record DemoMatchDto();
