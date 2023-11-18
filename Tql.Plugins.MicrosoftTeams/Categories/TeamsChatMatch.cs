using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsChatMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly PersonDto _dto;

    public string Text => _dto.DisplayName;
    public ImageSource Icon => Images.Teams;
    public MatchTypeId TypeId => TypeIds.TeamsChat;

    public TeamsChatMatch(PersonDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(GetUrl());

        return Task.CompletedTask;
    }

    private string GetUrl()
    {
        return $"msteams:/l/chat/0/0?users={Uri.EscapeDataString(_dto.EmailAddress)}";
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, GetUrl());

        return Task.CompletedTask;
    }
}
