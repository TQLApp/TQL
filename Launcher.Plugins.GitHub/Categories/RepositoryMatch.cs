using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.GitHub.Categories;

internal class RepositoryMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly RepositoryMatchDto _dto;

    public string Text => _dto.Name;
    public ImageSource Icon => Images.GitHub;
    public MatchTypeId TypeId => TypeIds.Repository;

    public RepositoryMatch(RepositoryMatchDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().LaunchUrl(_dto.Url);

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

internal record RepositoryMatchDto(Guid ConnectionId, string Name, string Url);
