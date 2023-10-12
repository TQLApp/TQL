using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class TeamsChatMatch : IRunnableMatch, ISerializableMatch
{
    private readonly GraphUserDto _dto;

    public string Text => _dto.DisplayName;
    public ImageSource Icon => Images.Teams;
    public MatchTypeId TypeId => TypeIds.TeamsChat;

    public TeamsChatMatch(GraphUserDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider
            .GetRequiredService<IUI>()
            .LaunchUrl($"msteams:/l/chat/0/0?users={Uri.EscapeDataString(_dto.EmailAddress)}");

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
