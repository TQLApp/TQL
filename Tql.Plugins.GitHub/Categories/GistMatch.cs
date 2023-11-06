using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.GitHub.Categories;

internal class GistMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly GistMatchDto _dto;

    public string Text => _dto.Name;
    public ImageSource Icon => Images.Gist;
    public MatchTypeId TypeId => TypeIds.Gist;

    public GistMatch(GistMatchDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(_dto.Url);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.Url);

        return Task.CompletedTask;
    }
}

internal record GistMatchDto(Guid ConnectionId, string Name, string Url);
