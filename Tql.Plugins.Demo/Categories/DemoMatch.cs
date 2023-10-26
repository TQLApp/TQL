using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.Demo.Categories;

internal class DemoMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    public string Text => "TQL Website";
    public ImageSource Icon => Images.SpaceShuttle;
    public MatchTypeId TypeId => TypeIds.Demo;

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl("https://github.com/pvginkel/TQL");

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return "{}";
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider
            .GetRequiredService<IClipboard>()
            .CopyUri(Text, "https://github.com/pvginkel/TQL");

        return Task.CompletedTask;
    }
}
