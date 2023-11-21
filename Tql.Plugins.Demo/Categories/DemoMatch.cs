using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.Demo.Categories;

internal class DemoMatch(DemoMatchDto dto) : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    public string Text => Labels.DemoMatch_Label;
    public ImageSource Icon => Images.SpaceShuttle;
    public MatchTypeId TypeId => TypeIds.Demo;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl("https://github.com/TQLApp/TQL");

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
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
