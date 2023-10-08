using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mail;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class TeamsVideoMatch : IRunnableMatch, ISerializableMatch
{
    private readonly GraphUserDto _dto;
    private readonly Images _images;

    public string Text => _dto.DisplayName;
    public IImage Icon => _images.Teams;
    public MatchTypeId TypeId => TypeIds.TeamsVideo;

    public TeamsVideoMatch(GraphUserDto dto, Images images)
    {
        _dto = dto;
        _images = images;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider
            .GetRequiredService<IUI>()
            .LaunchUrl(
                $"msteams:/l/call/0/0?users={Uri.EscapeDataString(_dto.EmailAddress)}&withVideo=true"
            );

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
