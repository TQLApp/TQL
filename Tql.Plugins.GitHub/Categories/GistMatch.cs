using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.GitHub.Categories;

internal class GistMatch(GistMatchDto dto) : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    public string Text => dto.Name;
    public ImageSource Icon => Images.Gist;
    public MatchTypeId TypeId => TypeIds.Gist;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.Url);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.Url);

        return Task.CompletedTask;
    }
}

internal record GistMatchDto(Guid ConnectionId, string Name, string Url);
